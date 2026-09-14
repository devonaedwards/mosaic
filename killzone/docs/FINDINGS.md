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

## 2. A single gun mount hard-counters every frontal drone attack

**Partly resolved. The exchange rate still needs a design decision.**

*Revised after measuring it properly. An earlier version of this entry said
saturation worked at eight drones; that was the targeting bug in item 8
flattering the attacker, and the corrected numbers are far worse.*

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

### What the numbers actually are

Run `./build.sh balance`. Eight drones launched together from 1,200 m against a
gun mount at 1,650 m:

| drones sent together | reached the gun | gun destroyed | Materiel spent |
|---|---|---|---|
| 5 | 0 | never | 1,000 |
| 8 | 0 | never | 1,600 |
| 16 | 0 | never | 3,200 |
| 24 | 7 | every time | 4,800 |

It takes twenty-four drones to destroy a four-hundred-and-fifty Materiel
structure: an exchange rate of more than ten to one. Saturation works, but only
at a number no player will reach, because the crew ceiling caps a flight at
roughly fourteen even with six crew quarters built.

### Two things that do work

**Attacking at night.** The gun finds its targets optically, and after dark that
reach collapses to about a third. Nothing about the gun changes; it simply
cannot begin shooting until the drones are much closer, and the seconds it does
not get are the seconds the drones needed.

| drones | destroyed it by day | destroyed it at night |
|---|---|---|
| 8 | never | never |
| 12 | never | every time |

Twelve at night instead of twenty-four by day. That is the counter the subject
matter actually uses, and it costs the attacker nothing but patience.

**Shortening its reach.** At 120 m of reach - about five and a half seconds of
exposure - eight drones take it every time. This is the tuning lever, and 550 m
remains suspicious on its own terms: at the design's roughly twelve-to-one
compression that is six and a half kilometres of real ground.

### Still open

Even at twelve drones on a dark night, killing a 450 Materiel structure costs
2,400 Materiel. Somebody needs to decide whether that is the intended price of
walking into prepared ground, or whether the gun's reach comes down. The
implementation is not the thing standing in the way of either answer.

---

## 8. Weapons shot at whatever was nearest, not at what mattered

**A real bug, and the one that made item 2 look better than it was.**

Target selection picked the closest enemy in range. That is a reasonable-sounding
rule that behaves stupidly: a gun mount with a relay mast parked beside it spent
an entire engagement chipping twenty-one damage at a time off a six-hundred
hit-point structure while the eight drones about to destroy it flew past
unengaged.

**Resolved:** candidates are now scored by the fraction of their remaining health
one shot removes, capped at one, with distance only breaking ties. A weapon
shoots at what it can actually hurt.

Worth noting how this was found. It was not visible in any unit test, because
every unit test put one attacker and one target in an empty world. It showed up
the first time an experiment was run with a realistic amount of scenery on the
map, and the tell was a balance result that made no sense - gun range not
mattering at all, when it obviously should.

---

## 9. Nothing enforced the rule that you cannot shoot what you cannot see

The design's third pillar is that nothing is targetable until somebody has looked
at it. Nothing in the code enforced it: every weapon engaged anything inside its
range, through darkness and terrain alike.

That made the reconnaissance layer decoration and made night cosmetic.

**Resolved:** a weapon now requires its own side to have detection on the target.
Radar sees through darkness but only finds things in the air; everything else is
optical or thermal, and an optical sensor loses about two thirds of its reach
after dark. This is what turned night from a colour change into the mechanic in
item 2.

---

## 10. Mines

Added, because the honest answer to ground you cannot hold by standing on it is
not a cleverer drone.

A heavy drone lays four mines on a night sortie and flies home. One kills a
supply truck, a logistics robot or a fighting vehicle outright; a tank survives
the first and not the second. The field costs nothing to maintain, cannot be
jammed, cannot be shot down, needs no crew and no link, and is still there in the
morning.

It is also armed against whoever drives into it first, including the side that
laid it, and there is a test asserting exactly that. That is not a gameplay
penalty, it is what a mine is, and a game about this subject should not pretend
otherwise.

| vehicle | damage per mine | survives a mine | field stops it |
|---|---|---|---|
| Supply Truck | 780 | no | after one |
| Logistics UGV | 780 | no | after one |
| Fighting vehicle | 1,320 | no | after one |
| Main Tank | 1,320 | yes, one | after two |

The uncomfortable part is that this is the most cost-effective thing in the
roster by a wide margin, and the least interesting to operate. There is no pilot
skill in it, no timing, and no counter-play in the moment. If playtesting shows
players reaching for it first and every time, the answer is probably a clearing
unit and a visible-once-triggered rule, not a damage nerf.

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
