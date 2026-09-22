# How to play KILL ZONE

KILL ZONE is a video game — a real-time strategy game about drone warfare in a
fictional near-future Eastern Europe. This file is how to get it running.

**You need a computer.** The simulation is a C# program; the browser is only a
view onto it. A phone can be the *screen* (see "From your phone" below), but
something has to run the server, and cloning the repository onto a handset is
not enough.

---

## 1. Install a C# compiler, once

The game has no engine and no package dependencies — it builds anywhere a C#
compiler exists. Pick either:

- **.NET SDK** (recommended, one installer): https://dot.net
  - macOS: `brew install --cask dotnet-sdk`
  - Ubuntu/Debian: `sudo apt install dotnet-sdk-8.0`
  - Windows: the installer from the link above
- **Mono**, if you would rather: `brew install mono` / `sudo apt install mono-devel`

Check it worked: `dotnet --version` (or `mono --version`) prints a number.

## 2. Get the code and run it

```sh
git clone https://github.com/devonaedwards/mosaic.git
cd mosaic/killzone
git checkout claude/drone-warfare-rts-concept-4qhdk0
./build.sh play
```

First build takes a minute or so. Then open **http://localhost:8080**.

On Windows without a bash shell, run the two steps by hand:

```
dotnet build   (or: see build.sh for the exact compile line)
build\KZ.Play.exe
```

## 3. From your phone

With the computer and the phone on the same wifi:

```sh
./build.sh play --lan
```

It prints the address to type into the phone:

```
KILL ZONE - open http://localhost:8080/
       or on this network: http://192.168.1.42:8080/
(--lan: no password, no encryption - anyone who can reach this port can play)
```

That warning is real. There is no authentication of any kind — anyone who can
route to that port can drive the match. Fine on a home network. Not fine on a
café or office one.

If the phone cannot reach it, the usual cause is the computer's firewall
blocking inbound 8080, not the game.

---

## Playing

The match **starts paused**. Press space or the play button.

| | |
|---|---|
| drag | pan the map |
| wheel / pinch | zoom |
| click | select |
| right-click | order the selected unit (move, or attack a contact) |
| `1`–`7` | arm an airframe from the hangar |
| armed card, then a target | launch a sortie at it |
| `e` | radiate / go quiet, on a selected mast that can |
| space | pause |
| `.` | single step |
| 1× 2× 4× | speed |

**You are the western side (team 1).** Diamonds are contacts your sensors are
holding; the letter on one is the channel that found it (**O**ptical,
**T**hermal, **A**coustic, **R**adar, **E**SM). A filled ring means a radar
track, an open ring optical, a bare dot a memory of something no longer seen —
and you cannot shoot at what you only have a bearing on.

You can lose. If you do nothing at all, the sector falls at about eight minutes
of play: the defence's armoured column drives at your command post under an
electronic-warfare escort. The column only moves while that escort is alive, so
the escort is the thing to kill — and the log will tell you the column is
coming long before it arrives.

## Other commands

```sh
./build.sh              compile, test, and run both fast guards
./build.sh headless     a scripted match with no interface
./build.sh balance      the balance experiments and their tables
./build.sh mutations    the slow coverage guard (rebuilds per mutation)
./build.sh play --night start after dark
./build.sh play --seed 12345
```

## If something goes wrong

- **"No C# compiler found"** — step 1 did not take. Open a new shell so the
  installer's `PATH` change applies.
- **"cannot find src/KZ.Play/web"** — run `./build.sh` from `mosaic/killzone`,
  not from the repository root.
- **Port 8080 busy** — `./build.sh play --port 8099`.
- **The page loads but nothing moves** — it starts paused. Press space.
