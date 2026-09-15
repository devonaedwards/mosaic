// KILL ZONE - a real-time strategy video game.
// The wire format: plain JSON, written and read by hand.
//
// By hand rather than by a serializer because the host this file ships with is
// Mono's HttpListener and the host it will be replaced by is ASP.NET. Anything
// that came out of a Mono serializer would have to be re-derived on the other
// side, and a JSON shape that a browser and a C# host both agree on is the one
// part of this project that must survive the swap untouched.
//
// It is also deliberately small. Nothing here needs a general object model: the
// simulation writes a snapshot and reads a command, and both are flat.

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace KZ.Play
{
    /// <summary>
    /// Appends JSON to a StringBuilder. Tracks whether a comma is due so callers
    /// never have to, because a stray comma is the one JSON bug that is invisible
    /// in C# and fatal in the browser.
    /// </summary>
    public sealed class JsonWriter
    {
        readonly StringBuilder sb = new StringBuilder(1 << 16);
        bool needComma;

        public void BeginObject() { Sep(); sb.Append('{'); needComma = false; }
        public void EndObject() { sb.Append('}'); needComma = true; }
        public void BeginArray() { Sep(); sb.Append('['); needComma = false; }
        public void EndArray() { sb.Append(']'); needComma = true; }

        public void Key(string name)
        {
            Sep();
            Quote(name);
            sb.Append(':');
            needComma = false;
        }

        public void Object(string name) { Key(name); BeginObject(); }
        public void Array(string name) { Key(name); BeginArray(); }

        public void Value(string v)
        {
            Sep();
            if (v == null) sb.Append("null"); else Quote(v);
            needComma = true;
        }

        public void Value(int v)
        {
            Sep();
            sb.Append(v.ToString(CultureInfo.InvariantCulture));
            needComma = true;
        }

        public void Value(long v)
        {
            Sep();
            sb.Append(v.ToString(CultureInfo.InvariantCulture));
            needComma = true;
        }

        public void Value(bool v) { Sep(); sb.Append(v ? "true" : "false"); needComma = true; }

        public void Field(string name, string v) { Key(name); Value(v); }
        public void Field(string name, int v) { Key(name); Value(v); }
        public void Field(string name, long v) { Key(name); Value(v); }
        public void Field(string name, bool v) { Key(name); Value(v); }

        void Sep()
        {
            if (needComma) sb.Append(',');
            needComma = false;
        }

        void Quote(string s)
        {
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        public override string ToString() { return sb.ToString(); }
    }

    /// <summary>
    /// Reads one flat JSON object into a string dictionary. Commands are the only
    /// thing that travels this direction and they are all flat - a kind, a couple
    /// of integers, a pair of coordinates - so nesting is refused rather than
    /// supported. A command that needs nesting is a command that has grown past
    /// what this protocol should carry.
    /// </summary>
    public static class JsonReader
    {
        public static Dictionary<string, string> ReadFlatObject(string text)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            if (text == null) return map;
            int i = 0;
            SkipSpace(text, ref i);
            if (i >= text.Length || text[i] != '{') return map;
            i++;
            while (true)
            {
                SkipSpace(text, ref i);
                if (i >= text.Length) break;
                if (text[i] == '}') break;
                if (text[i] == ',') { i++; continue; }
                if (text[i] != '"') break;
                string key = ReadString(text, ref i);
                SkipSpace(text, ref i);
                if (i >= text.Length || text[i] != ':') break;
                i++;
                SkipSpace(text, ref i);
                if (i >= text.Length) break;
                string value;
                if (text[i] == '"') value = ReadString(text, ref i);
                else value = ReadBare(text, ref i);
                map[key] = value;
            }
            return map;
        }

        public static int Int(Dictionary<string, string> m, string key, int fallback)
        {
            string v;
            if (!m.TryGetValue(key, out v)) return fallback;
            int parsed;
            if (!int.TryParse(v, NumberStyles.Integer | NumberStyles.AllowDecimalPoint
                              | NumberStyles.AllowLeadingSign,
                              CultureInfo.InvariantCulture, out parsed))
            {
                double d;
                if (!double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                    return fallback;
                parsed = (int)d;
            }
            return parsed;
        }

        public static string Str(Dictionary<string, string> m, string key)
        {
            string v;
            return m.TryGetValue(key, out v) ? v : null;
        }

        static void SkipSpace(string s, ref int i)
        {
            while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r')) i++;
        }

        static string ReadString(string s, ref int i)
        {
            StringBuilder sb = new StringBuilder();
            i++; // opening quote
            while (i < s.Length && s[i] != '"')
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    i++;
                    switch (s[i])
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        default: sb.Append(s[i]); break;
                    }
                }
                else sb.Append(s[i]);
                i++;
            }
            i++; // closing quote
            return sb.ToString();
        }

        static string ReadBare(string s, ref int i)
        {
            int start = i;
            while (i < s.Length && s[i] != ',' && s[i] != '}' && s[i] != ' ' && s[i] != '\n') i++;
            return s.Substring(start, i - start);
        }
    }
}
