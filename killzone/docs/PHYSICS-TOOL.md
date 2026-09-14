# Should the game contain a physics engine for detection?

Short answer: **build the physics, but not inside the game.** Build it as an
offline tool that produces a calibration table, and have the game read the table.

This is worth writing down because the instinct to put the real model in the
simulation is a good one and the reasons not to are not obvious.

## Why the physics is worth building

The current detection model is a reach figure per sensor, scaled by a square root
of target signature, with a handful of multipliers for night and altitude. Every
one of those numbers was estimated. The model cannot answer questions it should
be able to answer - what a temperature inversion at dawn does to acoustic
detection, why a cheap uncooled thermal imager and an expensive cooled one differ
in rain, why a drone seen nose-on is so much harder for radar than the same drone
seen from the side.

Those questions have real answers with published coefficients. Acoustic
propagation is spherical spreading plus atmospheric absorption plus ground and
wind effects. Thermal detection is apparent temperature difference against
background, atmospheric transmission per band, and pixels on target. Radar is the
range equation with a clutter term. Optical is atmospheric extinction and
contrast reduction. None of this is exotic.

## Why it must not run inside the simulation

Three reasons, in order of how hard they are to argue with.

**It would have to be deterministic, and it cannot be cheaply.** The simulation
runs in fixed-point integers so that an iPad and a PC agree bit for bit over tens
of thousands of ticks. Propagation physics is full of exponentials, logarithms
and power laws. Implementing those in deterministic fixed point means building
tables anyway - so the choice is not "physics versus tables", it is "tables
computed at runtime" versus "tables computed once, offline, in ordinary floating
point, by a tool that can afford to be slow and readable".

**It would not fit in the tick.** Detection is already the most expensive thing
in the simulation - every sensor against every target, once per tick. That is
affordable because the inner test is a handful of multiplies and a compare.
Replacing it with a radiative transfer calculation, per pair, per tick, on a
phone, is not affordable by any margin.

**The player cannot reason about it.** This is the one that actually decides it.
A detection range that varies continuously with humidity is a detection range no
player can plan around. The game's own design rules say a mechanic that needs a
submenu gets cut. A model that is more truthful than the interface can express
produces a game where things happen for reasons nobody can see, which is worse
than a simpler model that is slightly wrong.

## The architecture instead

```
  tools/propagation/          offline, ordinary floating point, slow and readable
      acoustic.py             spreading + absorption + ground + wind
      thermal.py              radiometric contrast + band transmission + pixels
      radar.py                range equation + clutter + aspect
      optical.py              extinction + contrast
      generate_tables.py      sweeps the parameter space
          |
          v
  content/detection.kzb       a compiled table, hashed into the lobby handshake
          |
          v
  KZ.Sim                      one lookup, two multiplies, integer compare
```

The tool sweeps every combination of target class, sensor class, distance band,
altitude band, time of day and weather state, and writes out the reach. The
simulation indexes it. The whole thing is a few tens of kilobytes.

## What this buys

**Fidelity where it is cheap.** The physics runs at design time, where a
calculation taking a second does not matter, and where it can be checked against
published measurements rather than against intuition.

**Affordability where it is scarce.** The runtime cost stays where it is.

**Legibility, if we choose it.** The table is a deliberate quantisation. If the
interface can only usefully draw four weather states and three times of day, the
table has four weather states and three times of day, and the player learns a
system with a manageable number of states rather than a continuum.

**Somewhere to put the argument.** When somebody later asks why a thermal imager
is worse at noon, the answer is a tool with the coefficients in it and a source
next to each one, not a constant in a header file.

## What it does not buy

It does not make the numbers right. It makes them *derived*, which means the
argument moves from "is 130 metres the right acoustic range" to "is this the
right absorption coefficient and is this the right ambient noise floor" - a
better argument to be having, but still an argument.

And it adds a build step and a file format, which is real cost for a small team.
The honest version of this recommendation is: build it when the research lands
and the numbers are known to be wrong in ways a spreadsheet cannot fix. Not
before.
