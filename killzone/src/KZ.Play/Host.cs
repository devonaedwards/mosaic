// KILL ZONE - a real-time strategy video game.
//
// THIS FILE IS THE SHIM. It is the only file in this project that knows the
// simulation is being played over HTTP, and the only one that touches Mono.
//
// Its whole job is three sentences long:
//
//   1. serve the static assets in web/ to a browser,
//   2. accept player commands as JSON and hand them to MatchLoop,
//   3. hand back MatchLoop's JSON snapshot when asked for one.
//
// It exists because this machine has Mono and no .NET SDK, so Blazor and
// ASP.NET are both out. When the project moves to a machine with the SDK, this
// file is replaced by an ASP.NET minimal-API host - about thirty lines - and
// nothing else in src/KZ.Play changes. What a replacement must implement:
//
//   GET  /                     -> web/index.html
//   GET  /<path>               -> the file at web/<path>
//   GET  /api/static           -> MatchLoop.StaticJson()          (once)
//   GET  /api/state?since=<n>  -> MatchLoop.ViewJson(n)           (polled)
//   POST /api/command          -> parse, build a KZ.Sim Command, Enqueue
//   POST /api/control          -> pause / resume / step / speed / reset
//   and a timer calling MatchLoop.Service() every few milliseconds.
//
// Nothing in web/ knows what is on the other end of those six routes, which is
// the property that makes the swap cheap.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using KZ.Sim;

namespace KZ.Play
{
    public static class Program
    {
        static MatchLoop loop;

        public static int Main(string[] args)
        {
            int port = 8080;
            ulong seed = 20260915UL;
            int startTick = 0;
            bool open = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--port" && i + 1 < args.Length) port = int.Parse(args[++i]);
                else if (args[i] == "--seed" && i + 1 < args.Length) seed = ulong.Parse(args[++i]);
                else if (args[i] == "--night") startTick = SimConstants.PlaySeconds(3 * 3600);
                else if (args[i] == "--open") open = true;
            }

            string webRoot = FindWebRoot();
            if (webRoot == null)
            {
                Console.Error.WriteLine("cannot find src/KZ.Play/web - run from the repository root");
                return 1;
            }

            loop = new MatchLoop(seed, startTick);

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();

            Thread ticker = new Thread(ServiceLoop);
            ticker.IsBackground = true;
            ticker.Start();

            Console.WriteLine("KILL ZONE - open http://localhost:" + port + "/");
            Console.WriteLine("serving " + webRoot);
            if (open) Console.WriteLine("(paused at tick 0 - press space in the page to start)");

            while (true)
            {
                HttpListenerContext ctx;
                try { ctx = listener.GetContext(); }
                catch (Exception) { break; }
                try { Handle(ctx, webRoot); }
                catch (Exception ex) { Console.Error.WriteLine("request failed: " + ex.Message); }
            }
            return 0;
        }

        /// <summary>
        /// The one place real time touches the match. 4 ms is an eighth of a tick
        /// at 32 Hz, so the loop is never the thing that makes a tick late.
        /// </summary>
        static void ServiceLoop()
        {
            while (true)
            {
                loop.Service();
                Thread.Sleep(4);
            }
        }

        static void Handle(HttpListenerContext ctx, string webRoot)
        {
            string path = ctx.Request.Url.AbsolutePath;

            if (path == "/api/static") { SendJson(ctx, loop.StaticJson()); return; }

            if (path == "/api/state")
            {
                string sinceText = ctx.Request.QueryString["since"];
                int since;
                if (!int.TryParse(sinceText, out since)) since = 0;
                SendJson(ctx, loop.ViewJson(since));
                return;
            }

            if (path == "/api/command" || path == "/api/control")
            {
                string body;
                using (StreamReader r = new StreamReader(ctx.Request.InputStream, Encoding.UTF8))
                    body = r.ReadToEnd();
                Dictionary<string, string> m = JsonReader.ReadFlatObject(body);
                if (path == "/api/command") ApplyCommand(m); else ApplyControl(m);
                SendJson(ctx, "{\"ok\":true}");
                return;
            }

            SendFile(ctx, webRoot, path);
        }

        // ---- the two write routes -------------------------------------------
        //
        // Deliberately narrow. Everything the browser can ask for is one of the
        // five orders below, and each one is turned into the same KZ.Sim.Command
        // struct a replay or a campaign script would produce. There is no route
        // that can reach into the world and set something.

        static void ApplyCommand(Dictionary<string, string> m)
        {
            string kind = JsonReader.Str(m, "kind");
            byte team = Scenario.PlayerTeam;
            int x = JsonReader.Int(m, "x", 0);
            int y = JsonReader.Int(m, "y", 0);
            Fix2 point = new Fix2(Fix.FromInt(x), Fix.FromInt(y));
            EntityHandle subject = loop.Resolve(JsonReader.Int(m, "subject", 0));
            EntityHandle target = loop.Resolve(JsonReader.Int(m, "target", 0));

            switch (kind)
            {
                case "move":
                    loop.Enqueue(Command.MoveTo(team, subject, point));
                    break;
                case "attack":
                    loop.Enqueue(Command.Attack(team, subject, target));
                    break;
                case "stop":
                    loop.Enqueue(Command.Stop(team, subject));
                    break;
                case "launch":
                    loop.Enqueue(Command.LaunchSortie(team, JsonReader.Int(m, "defId", -1),
                                                      point, target, JsonReader.Int(m, "n", 0)));
                    break;
                case "altitude":
                    loop.Enqueue(Command.SetAltitude(team, subject,
                                                     (Layer)JsonReader.Int(m, "layer", 1)));
                    break;
            }
        }

        static void ApplyControl(Dictionary<string, string> m)
        {
            switch (JsonReader.Str(m, "action"))
            {
                case "pause": loop.SetPaused(true); break;
                case "resume": loop.SetPaused(false); break;
                case "step": loop.StepOnce(); break;
                case "speed": loop.SetSpeed(JsonReader.Int(m, "speed", 1)); break;
                case "reset": loop.Reset(); break;
            }
        }

        // ---- static assets ---------------------------------------------------

        static void SendFile(HttpListenerContext ctx, string webRoot, string path)
        {
            if (path == "/") path = "/index.html";
            // No traversal: a shim that serves the repository because someone
            // typed ../.. is a shim with a security hole in it.
            if (path.Contains("..")) { ctx.Response.StatusCode = 400; ctx.Response.Close(); return; }

            string full = Path.Combine(webRoot, path.TrimStart('/'));
            if (!File.Exists(full)) { ctx.Response.StatusCode = 404; ctx.Response.Close(); return; }

            byte[] bytes = File.ReadAllBytes(full);
            ctx.Response.ContentType = ContentTypeOf(full);
            // The page is edited while the server runs. Caching it is how you
            // spend twenty minutes debugging a fix that was already applied.
            ctx.Response.Headers["Cache-Control"] = "no-store";
            ctx.Response.ContentLength64 = bytes.Length;
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            ctx.Response.Close();
        }

        static string ContentTypeOf(string file)
        {
            if (file.EndsWith(".html")) return "text/html; charset=utf-8";
            if (file.EndsWith(".js")) return "application/javascript; charset=utf-8";
            if (file.EndsWith(".css")) return "text/css; charset=utf-8";
            return "application/octet-stream";
        }

        static void SendJson(HttpListenerContext ctx, string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            ctx.Response.ContentType = "application/json; charset=utf-8";
            ctx.Response.Headers["Cache-Control"] = "no-store";
            ctx.Response.ContentLength64 = bytes.Length;
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            ctx.Response.Close();
        }

        /// <summary>
        /// The assets sit beside their source, not beside the build output, so
        /// they can be edited without a rebuild. Looked up rather than configured
        /// because a path in a config file is one more thing to get wrong.
        /// </summary>
        static string FindWebRoot()
        {
            string[] candidates =
            {
                Path.Combine(Directory.GetCurrentDirectory(), "src/KZ.Play/web"),
                Path.Combine(Directory.GetCurrentDirectory(), "../src/KZ.Play/web"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../src/KZ.Play/web"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web")
            };
            for (int i = 0; i < candidates.Length; i++)
                if (File.Exists(Path.Combine(candidates[i], "index.html")))
                    return Path.GetFullPath(candidates[i]);
            return null;
        }
    }
}
