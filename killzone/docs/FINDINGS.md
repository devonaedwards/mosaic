# Findings from building the simulation

Things the design documents got wrong, left ambiguous, or did not anticipate,
discovered by implementing them. Recorded here rather than quietly patched, so
whoever owns the balance can decide what they actually want.

---

## 1. Four drones did not kill a tank

**Spec:** *"260 x 2.20 = 572 per hit, so four connected drones kill it - three
will not, at any veterancy, which is exactly the margin we want."*

**Reality:** four hits come to 2288 against 2300 hit points. The fourth drone
left the tank alive on twelve hit points.

**Resolved:** tank lowered to 2250. The stated intent - four kill it, three do
not - is the thing worth keeping; the arithmetic slip is not. There is now a
test named after the promise, so anyone who retunes either number will be told
which design commitment they just broke.

---

## 2. A single gun mount hard-counters every drone attack

**Not resolved. Needs a design decision.**

The gun mount as specified is 550 metres of reach, fragmentation damage that
one-shots any rotary drone, and a shot every 0.75 seconds.

A drone attacking it has to physically arrive, so it spends the whole 550 metres
under fire: about 25 seconds for a radio airframe, 37 for a fiber one. In that
window the gun kills roughly twenty drones. An attacker launching one drone a
second, or committing a flight of six at once, is simply feeding it. In a
ninety-second scripted attack, forty-two sorties produced zero hits on anything.

This is not a simulation bug. It is what those numbers mean. But "the last-line
answer to FPVs" should be a wall you have to solve, not a wall that cannot be
solved, and at present there is no tool in the roster that solves it: every
anti-structure weapon in the game is a one-way airframe that has to fly into the
gun's engagement envelope to do its job.

Three possible answers, for whoever owns balance:

1. **Shorten its reach.** 550 metres is suspicious on its own terms. The design
   compresses real distances roughly twelve to one, and 550 map metres is about
   six and a half kilometres of real ground - far beyond any gun of this kind.
   Something in the 150-250 range would let drones approach from outside it and
   make positioning the gun a real decision.
2. **Give it an arc.** A gun that covers 120 degrees can be flanked, which turns
   it from a wall into a direction you must not attack from.
3. **Give the attacker a suppression weapon** that outranges it. Nothing in the
   current roster does.

Option 1 is the smallest change and the most defensible. It is not made here
because it is a balance call, not an implementation detail.

---

## 3. Tether snag probability compounded with thread length

**A real bug, found by watching a match.**

The snag rule reads "nine percent per second under power lines". It was
implemented as a roll per thread segment per evaluation, which multiplies the
stated hazard by the length of the thread. A drone trailing a kilometre of line
over a quiet road has eighty-odd segments, so a 1.5 percent hazard became a
near-certainty, and fiber drones were losing their line constantly for no reason
a player could have understood.

**Resolved:** one roll per tick against the per-second rate, taking the worst
ground under either the newest segment or one rotating older one. The newest
segment is what makes flying into a treeline bite immediately; the rotating one
is what makes the trailing line matter. The stated number now means what it says,
which matters because it is a number the player is expected to learn.

---

## 4. Crew quarters granted no crews

Straightforward omission. The building existed, cost money, and did nothing. Now
adds four crews, and destroying it takes them away - permanently for the faction
that flies from forward dugouts, temporarily for the one that flies from remote
bays.

---

## 5. Orders issued outside the tick lost their events

The event ring is cleared at the start of each tick. Anything that pushed an
event from outside `Step()` had it wiped before any observer saw it, so launches
were invisible to the interface, the sound system and the replay.

**Resolved:** commands now queue and execute inside the tick, immediately after
the ring is cleared. This is the architecture the networked game needs anyway -
a match is a seed plus a list of commands - so it was worth doing properly rather
than moving the clear.

---

## 6. The jamming threshold sits at an awkward number

A basic radio drone has robustness 40. An electronic warfare post reaches 450
metres at strength 70. Working through the falloff, the drone survives down to
252 metres from the emitter and loses its pilot inside that.

Nothing is wrong here, but 252 is not a number anyone chose, and the interface
will have to draw that ring. Either the ring is drawn from the real arithmetic,
or the constants get nudged so the boundary lands somewhere a designer picked.
Worth deciding before the visual language is built on top of it.

---

## 7. One-way drones dive on whatever they are parked next to

Not a bug, but it surprised the tests. A one-way airframe sitting within weapon
reach of an enemy structure attacks it and is expended, because that is what a
one-way airframe does. It made three tests fail for reasons that had nothing to
do with what they were testing.

Worth remembering when building missions: a drone told to hold position near an
enemy building will not hold position.
