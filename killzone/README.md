# KILL ZONE

**This is a video game.** Specifically, it is a real-time strategy game in the
Command & Conquer tradition — base building, a resource economy, a build
sidebar, fog of war, and two asymmetric factions fighting over a map — built
for iPad and iPhone first, with PC as a second platform.

The setting is a **fictional** near-future Eastern European conflict. Factions,
place names, and units are invented. Where the design borrows from real-world
military technology it does so the way *Command & Conquer: Generals* or
*Wargame: Red Dragon* do: as genre furniture and unit archetypes, abstracted
into game mechanics with invented names, stat blocks, build costs and
counter-relationships.

Nothing in this repository is operational guidance, hardware design, targeting
information, or usable technical instruction for anything real. It is game
simulation code: integers, grids, cooldown timers and damage multiplier tables.
The "drones" here are sprites with hit points that cost 200 Materiel and take
eight seconds to build.

## The design idea

Most strategy games make you build a tank and then drive the tank. This one
separates the airframe from the crew from the radio link:

> **The unit you build is not the thing that fights.** You build airframes by
> the dozen and they sit in a hangar. What fights is a *sortie*: an airframe,
> plus a crew, plus a working control link back to that crew, plus somebody who
> can see the target. Take away any one of the four and your most expensive
> drone is inventory.

That gives the game its signature system — the **control-link ladder**. Five
link types (radio, mesh, fiber, satellite, autonomy), each buying resistance to
being jammed and paying for it in speed, range, price or judgement. Each is
countered by the rung behind it. Players climb the ladder over a match, and the
ladder *is* the tech tree.

Full design documents live in `docs/`.

## Repository layout

```
killzone/
  docs/                 design specifications (gameplay, technical, UI, art, futures)
  src/
    KZ.Sim/             the deterministic simulation — pure C#, no engine types
      Math/             Q31.32 fixed-point math, deterministic RNG
      Core/             entity table, components, world, commands, events
      Content/          unit and structure definitions
      Link/             the control-link layer: jamming, mesh, tethers, autonomy
      Sortie/           crew pool and sortie lifecycle
      Systems/          movement, combat, economy
    KZ.Headless/        runs a match with no renderer and prints a state hash
    KZ.Balance/         balance experiments, run in bulk
    KZ.Play/            the playable interface: a local host and a canvas
      web/              the page, served from disk rather than compiled
    KZ.Tests/           simulation unit tests and determinism checks
  build.sh
```

`KZ.Sim` deliberately has no dependency on any game engine. It is a pure C#
library that advances a world state by one fixed tick at a time. A Unity
project will later sit on top of it as a pure presentation layer, but the
simulation must stay engine-free so it can be tested headlessly, replayed, and
run identically on an iPhone and a PC.

## Why fixed point

The simulation uses Q31.32 fixed-point arithmetic (`Fix`), not floats, and runs
at exactly 32 Hz. Both choices exist for one reason: an iPad and a Windows PC
must be able to play the same multiplayer match and agree on the outcome for
48,000 consecutive ticks. Floating point does not survive that across ARM and
x86 compilers. A 64-bit integer multiply does.

32 Hz rather than 30 because 1/32 is exactly representable in binary, so
`dt` introduces no rounding drift.

## Build and test

Requires either the .NET SDK or Mono.

```sh
./build.sh          # compile and run the test suite
./build.sh headless # compile and run a scripted match, print the state hash
./build.sh play     # compile and serve the playable interface on :8080
```

## What works today

The simulation core is implemented and tested: 64 tests, all passing, including
cross-checks that two worlds given the same orders stay bit-identical for
thousands of ticks.

`./build.sh headless` plays a scripted three-minute engagement. An attacker
pushes drones east at a tank sitting under a jamming bubble, alternating cheap
radio airframes with expensive fiber ones. A typical run:

```
sorties launched      38
  refused, no crew    16      <- crews are the cap, not money
links gone amber      34
links gone black      34      <- the radio half of the force, inside the bubble
drones lost to jamming 33
tethers cut           16      <- what fiber pays instead
kills, verified        1
the tank was destroyed
```

That is the whole design thesis in one run. The radio drones are annihilated by
a single jammer. The fiber drones ignore it completely and pay in parted
threads. Four of them eventually get through, and the kill is worth full tasking
points because a radar mast was watching when it happened.

It runs about 360 times faster than real time, which is what makes overnight
balance testing practical.

## Playing it

```sh
./build.sh play        # then open http://localhost:8080/
```

`src/KZ.Play` is the first thing in this repository that draws. It serves a
canvas to a browser and pushes one team's view of the world as JSON: the map,
your own units, and **enemy contacts only where your sensors reach them**. You
select, order, and launch sorties; you pause, single-step, and run at 1x, 2x or
4x. Crude on purpose - coloured shapes and one-pixel lines - because the point
is to find out whether the game is worth anything before spending on art.

The renderer is a pure view transform: real metres in, pixels out, with real
zoom from the first frame. `docs/SCALE.md`'s correction is that compression is a
view parameter rather than a content one, so the same build serves a phone, a
tablet and a desktop at different zoom defaults rather than at different map
sizes.

`src/KZ.Play/Host.cs` is a swappable shim - the only file that knows the game is
being played over HTTP, and the only one that touches Mono. On a machine with
the .NET SDK it is replaced by a thirty-line ASP.NET host and nothing else
changes; the page, the canvas and the input handling do not know what is on the
other end of `/api/state`.

## Not built yet

No networking, no pathfinding beyond direct steering, no AI opponent worth the
name (the scenario's defender executes fixed standing orders), no economy
buildings or build queues, and no way to build anything during a match.
`docs/spec-technical.md` has the full plan; this is the first milestone of six.
