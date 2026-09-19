# Findings from building the simulation

Things the design documents got wrong, left ambiguous, or did not anticipate,
discovered by implementing them. Recorded here rather than quietly patched, so
whoever owns the balance can decide what they actually want.

Entries are amended in place and never removed. An entry that has been measured
again carries its amendment under its own heading, with what was believed, what it
was measured against, and what the number is now.

**Amendment pass, after WIRING-SPEC Phase 0 and Phase 1.** Every balance
experiment was re-run on the corrected tree. Four faults had been corrupting them:
a harness that overwrote the gun mount's range back to the figure item 2 had
already corrected, a harness world that was flat, clear, ownerless and uncovered,
a track hold that was documented and dead, and a gun mount with neither a traverse
rate nor a magazine. Items 2, 12, 13, 15, 16, 17, 18, 20, 21, 22, 25 and 30 carry
amendments as a result. Items 10, 16 and the headline of 25 came through it
intact, and are marked as having survived rather than left silent. Items 31, 32
and 33 are new, and two of the three are about the instrument rather than the
game. Every number below was taken again after the engagement-channel and
autocannon work landed mid-pass; the handful that moved are the ones quoted.

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

### Amended after the Phase 0 and Phase 1 re-run - this entry is now wrong in the other direction

Everything above was measured against a gun mount with **550 metres of reach,
infinite ammunition and instant traverse**, on a flat, clear, ownerless, uncovered
map. Not one of those was a design decision: the 550 was three balance experiments
overwriting the catalogue behind the reader's back (AUDIT-UNWIRED F33), the
infinite ammunition and instant traverse were two fields nobody had set on the
unit (F4), and the empty map was every experiment world in the project (F34). All
four are now fixed, and the same experiment re-run on the mount that is actually
in the game says the opposite of the entry above:

| drones sent together | reached the gun | gun destroyed | Materiel spent |
|---|---|---|---|
| 1 | 0.8 | never | 200 |
| 2 | 1.8 | never | 400 |
| 3 | 2.8 | **80%** | 600 |
| 5 | 4.8 | every time | 1,000 |
| 8 | 7.8 | every time | 1,600 |

Three drones - six hundred Materiel - take a four-hundred-and-fifty Materiel
structure four times in five, for an expected **750 Materiel per kill**. The
exchange rate went from more than ten to one down to about five to three. It is
still nominally in the defender's favour and that no longer means anything: a
structure that dies to three of the cheapest airframe in the game is not a wall,
whatever it costs. Every word of this entry's problem statement has evaporated.
There is nothing to solve, nothing needs a suppression weapon to outrange, and the
three "possible answers for whoever owns balance" answer a problem that no longer
exists. The open question has reversed: a gun mount now has to be made worth
building.

The mechanism is finding 31, and it is not the range: **the mount gets one shot.**

Two smaller notes on the rest of the current output. Its 16- and 24-drone rows
both read 14.8 arrived, because the experiment stops counting the moment the gun
dies rather than because anything capped the flight; above about twelve drones
those rows measure the harness and not the game. And **attacking at night is now
worth almost nothing** - three drones by day take the gun 80% of the time, three
at night 100%, and from five drones upward day and night are both 100%. Item 12
predicted that collapse for the wrong reason (microphones) and it has arrived for
a different one: by day the gun is already losing anyway.

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

**Survived the honest re-run, unchanged to the last digit.** Damage, survivals
and the number of mines each vehicle eats are identical on the corrected tree and
on the corrected world. This is the only table in this document that came through
the re-run untouched, and it is worth saying why: mines do not involve a sensor,
a hit roll, a magazine or a traverse rate, so not one of the four faults could
reach it.

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

---

## 11. There was no signature model at all

Every target was equally detectable by every sensor. A sensor had one reach and
three booleans, and a tank and a quadcopter were found from the same distance.
The design's own rule that a jammer gives itself away by transmitting was
declared in the data as `SignatureWhileEmitting` and never read by anything.

**Rebuilt as five channels on each side.** A unit emits on radio, thermal,
acoustic, visual and radar cross-section. A sensor is fitted with some
combination of optics, thermal imaging, microphones, active radar and passive
radio listening. Detection succeeds if any one pairing reaches, and reach scales
with the square root of the target's strength on that channel.

The consequences were not designed, they fell out:

- A fiber drone emits nothing on radio, so passive listening will never find one
  however close it gets. It is still a quadcopter, so a microphone hears it
  exactly as well as it hears a radio-controlled one. Fiber buys silence on one
  channel and nothing on the others, which is the correct shape.
- A jammer is the loudest object on the map while it is switched on, and much
  harder to find once it is not. Switching it off is now a real choice between
  denying the enemy their radios and not being located.
- A small drone is genuinely hard to see. A gun mount's camera picks a tank out
  at 569 m and a quadcopter at 232 m, from the same mount, in the same light.

## 12. Turret sensors, and why darkness stopped being a free pass

The question that prompted this: does a turret use optics, or electronic
support, or both? The answer the model now gives, in metres of reach against a
550 m gun:

| fitted with | vs a quad, day | vs a quad, night | vs a tank, day |
|---|---|---|---|
| optics only | 232 | 81 | 569 |
| acoustic only | 335 | 335 | 369 |
| optics + acoustic | 335 | 335 | 569 |
| optics + thermal | 232 | 156 | 569 |
| all three | 335 | 335 | 569 |

Three things worth noticing. **The gun's envelope is set by its sensors, not its
barrel** - it reaches 550 m and can only find a drone at 335. **Microphones are
the best anti-drone sensor at any hour**, which is why real counter-drone mounts
have them and why the acoustic channel had to exist. And **thermal buys back the
night against vehicles and very little against drones**, because a small drone is
not very hot - so the expensive sensor is not the answer to the cheap threat.

This also corrects item 2. Attacking at night was halving the drone requirement
only because the turret had no sensor that worked in the dark. Against a turret
with microphones, darkness is worth very little, which is the more honest answer.

### Amended: new table, and two of the three conclusions fall

Every number above predates the `thermal-optical.md` §11 signature table (twenty
numbers, never applied) and the night-optical constant going from 0.35 to 0.20.
Re-measured:

| fitted with | vs a quad, day | vs a quad, night | vs a tank, day |
|---|---|---|---|
| optics only | 73 | 15 | 300 |
| acoustic only | 48 | 96 | 320 |
| optics + acoustic | 73 | 96 | 320 |
| optics + thermal | 73 | **73** | 300 |
| all three | 73 | 96 | 320 |

**The first conclusion survives and hardens.** The gun's envelope is set by its
sensors and not its barrel - only now the barrel is 85 m and the sensors find a
quadcopter at 73, so the two have converged from opposite ends and the honest
figure is smaller than either.

**The second is now false by day.** "Microphones are the best anti-drone sensor
at any hour" was written when acoustic reach was 335 m. It is 48 m by day against
73 m of camera, so by daylight the camera is better. After dark it is 96 against
15, so the claim is true at night and false in the sun. Item 20 began this walk-
back by cutting the nominal acoustic reach; the signature table finished it.

**The third is overturned.** "Thermal buys back the night against vehicles and
very little against drones" was measured when thermal took a quadcopter from 41 m
to 80 m after dark - about double. It now takes it from 15 m to 73 m, close to
five times, and restores the full daylight figure. A thermal imager is now the
difference between a mount that is blind after dark and one that is not, against
exactly the target this entry said it would not help against. The reason is the
one the entry itself gave and got the sign of: a small drone is not very hot, but
the corrected table made *everything else* about optical detection worse at night,
and thermal does not care.

## 13. Stacked turrets are a structural dead end, not a balance number

| turrets | defence cost | drones needed | attacker cost |
|---|---|---|---|
| 1 | 450 | 16 | 3,200 |
| 2 | 900 | 32 | 6,400 |
| 3 | 1,350 | more than 64 | more than 12,800 |
| 4 | 1,800 | more than 64 | more than 12,800 |

**Three gun mounts, costing 1,350 Materiel, cannot be destroyed by drones at any
budget.** Not because the numbers are badly tuned, but because the attacker's
maximum possible simultaneous force is capped by a different system than the
defender's maximum density: crews cap a flight at thirty, and three turrets need
more than sixty-four.

No amount of retuning gun range fixes that. It needs one of:

1. **A standoff weapon** that outranges the turret's sensors. The design cut
   artillery, and every anti-structure weapon in the roster is a one-way airframe
   that has to fly into the envelope to work. This is the gap.
2. **A density cap** - turrets interfering with each other, or costing more to
   place near one another.
3. **Accepting it**, and making prepared positions something you go around rather
   than through. That is a legitimate answer and arguably the realistic one, but
   it needs the map design to always offer a way around, and it needs saying out
   loud rather than discovering it in a playtest.

### Amended: the table is wrong, and the experiment behind it cannot see anything

| turrets | defence cost | drones needed | attacker cost |
|---|---|---|---|
| 1 | 450 | 8 | 1,600 |
| 2 | 900 | 8 | 1,600 |
| 3 | 1,350 | 8 | 1,600 |
| 4 | 1,800 | 8 | 1,600 |

Three gun mounts costing 1,350 Materiel fall to 1,600 Materiel of drones. The
claim in bold above - that they cannot be destroyed at any budget - is false by
a factor that no longer has a meaningful denominator.

Two things are worth separating. The first is that this table dates from when the
gun's catalogue range really was 550 m and it had no magazine, and the honest
mount is much weaker, so the direction of the correction is unsurprising. The
second is not:
**the stacking experiment printed 8 / 8 / 8 / 8 on every build in this pass, and
on the build before it.** It reported the same four numbers with the gun at 550 m
and at 85 m, with and without a magazine, with and without the track hold, with
and without the corrected signature table, on a flat map and on a mixed one. It
has no discriminating power at all, so it cannot be the evidence for anything -
including for the corrected table directly above. See finding 32.

The structural argument underneath - that crews cap the attacker's simultaneous
force while nothing caps the defender's density - is not refuted by this. It was
never tested. The experiment never reaches the regime where it would bite.

## 14. The model is now more truthful than the interface can draw

Worth stating plainly, because it is a problem I introduced.

The gameplay spec commits to four display rules and nothing else, and says that
any mechanic needing a submenu gets cut. There are now five sensor channels and
five signature channels: twenty-five interactions, none of which the current
visual language can express. There is no way to show a player that one turret has
microphones and another does not, and that difference now decides whether a night
attack works.

The simulation should keep the five channels, because they produce behaviour that
is correct and that players will feel even if they cannot name it. The interface
should never show five numbers. Two suggestions:

- Draw one detection ring per turret **against the currently selected unit**, so
  it answers the only question a player actually has: how close can I get with
  *this*.
- On a unit card, at most two icons for what finds it.

If that still reads as a spreadsheet in a playtest, the fallback is to merge
thermal into optics as one "sight" channel and radar into passive listening as
one "electronic" channel, leaving three. Three legible channels beat five
accurate ones.

---

## 15. Guns always hit, and that was the whole problem

**This overturns item 2 and both of my earlier answers about turrets.**

Direct fire was deterministic. A shot that reached did exactly the table damage,
every time, at any range, against anything. For ground fire that is a deliberate
design choice and stays - a strategy player has to be able to count how many
drones a tank takes. For a gun firing at a two-kilogram object crossing its front
at eighty kilometres an hour it is simply wrong, and it was the reason a single
turret looked unbeatable.

Air defence now rolls. The chance falls as the square of fractional range, scales
with how big the target looks, falls with how fast it crosses, and drops another
thirty percent when shooting upward. A mount's stated reach is how far its rounds
carry, not how far it can reliably hit.

The same experiment, before and after:

| drones sent together | destroyed the gun, guns always hit | destroyed the gun, guns roll |
|---|---|---|
| 3 | never | 10% |
| 5 | never | **72%** |
| 8 | never | 98% |
| 24 | every time | every time |

Five drones now take a gun position about three times in four, at 1,000 Materiel
against a 450-Materiel structure. The exchange rate went from more than ten to
one to about two to one, and nothing was nerfed to achieve it - the gun still
reaches 550 m, it simply cannot hit a small fast thing out there.

This also answers the jet question without any special case. A turbojet strike
drone crossing at three times the speed of a quadcopter is not hard to shoot at,
it is hard to hit, and the speed term does that on its own.

### Amended twice, and the first amendment is the more uncomfortable one

**The recorded table had already gone stale before any of this pass's work.**
Re-running the same experiment on the commit immediately preceding these four
changes - the tree this entry was supposed to describe - gives 37% at three
drones, 92% at five and 100% at eight. Not 10 / 72 / 98. Nobody changed the entry
because nobody re-ran the experiment; the numbers drifted out from under it at
some earlier point and the document went on quoting them. A measurement in a
document is evidence about the day it was taken, and this file has no mechanism
that notices when it stops being true. That is the same shape as finding 30, one
level up: there, code nothing called; here, a number nothing re-checks.

**And the honest re-run is different again.** On the deployed 85 m mount, with a
magazine, a traverse rate, the corrected signature table and a real world:

| drones sent together | always hit | guns roll, as recorded | pre-pass tree, measured | now |
|---|---|---|---|---|
| 3 | never | 10% | 37% | 80% |
| 5 | never | 72% | 92% | **100%** |
| 8 | never | 98% | 100% | 100% |
| 24 | every time | every time | every time | every time |

The entry's *conclusion* - that making direct fire roll was the single change that
unstuck the turret problem - survives all of this and is arguably understated. Its
numbers do not. Five drones no longer take a gun position three times in four;
three drones take it four times in five.

What does not survive is the sentence "the gun still reaches 550 m, it simply
cannot hit a small fast thing out there". The gun never reached 550 m. That was
the harness, and this entry is one of the five that measured it.

### Amended again: two of those rungs are no longer measured, and the jet sentence has no experiment behind it

**The 8-drone and 24-drone rows are gone from the harness** (FINDINGS 40). A Gun
Mount goes from 0% at two drones to 100% at six, so every rung above six was the
same answer repeated and five of the eight cells in that column read 0% or 100%.
The ladder is 2, 3, 4, 5, 6 now. Nothing that was being measured moved — three
drones is 37% and five is 98%, identical to the digit — so the table above is
still a fair record of what those rungs said; there is simply no current
measurement of the two at the bottom of it, and there does not need to be.

**And this entry's answer to the jet question is unsupported.** “A turbojet strike
drone crossing at three times the speed of a quadcopter is not hard to shoot at,
it is hard to hit, and the speed term does that on its own” rests on
`AirHitChance`'s speed term, which **no balance experiment can measure**.
`FiringSolutionReferenceSpeed` was moved from 45 m/s to 90 and then to 200 and all
ten experiments printed identical bytes: the term is `reference / target speed`
clamped at 1.15, every airframe the harness flies against a gun is at 28 or
33 m/s, and the only faster ones appear in `decoys`, whose defender is an
interceptor and does not use that function. The claim may be correct. It is
untested, and nothing flying faster than 50 m/s has ever been in a balance table.

## 16. Stacked turrets stopped mattering, which is now suspicious in the other direction

| turrets | drones needed |
|---|---|
| 1 | 8 |
| 2 | 8 |
| 3 | 8 |
| 4 | 12 |

Adding the second and third turret buys almost nothing. The reason is that each
turret's *effective* envelope is now much smaller than its nominal one, so
turrets spread along a line are firing at long range with poor odds against
anything not directly in front of them.

That is arguably correct - mutual support at those ranges is thin - but it is a
big swing from "three turrets are unbeatable" to "three turrets are worth one,"
and the truth is probably between. The falloff curve is the thing to check, and
it is currently a guess. Research is out on real hit-probability figures.

### Amended: this one survived, and got slightly stronger

| turrets | drones needed, recorded | drones needed, now |
|---|---|---|
| 1 | 8 | 8 |
| 2 | 8 | 8 |
| 3 | 8 | 8 |
| 4 | 12 | **8** |

The fourth turret stopped buying anything too - and it had already stopped on the
build before this pass, so the recorded 12 was stale in the same way item 15's
saturation table was. Of every conclusion in items 13 through 18 this is the one
the corrected measurement agrees with, and the explanation the entry gave is now
measurable rather than asserted: a mount's effective envelope really is far
smaller than its nominal one, and the number is **about 70 m** - the range at
which it first holds a track on a quadcopter, against an 85 m barrel and a 127 m
analytic sensor reach (finding 31).

One honest caveat, which cuts against the entry rather than for it: the stacking
experiment cannot currently distinguish anything (finding 32), so "this survived"
means "the re-run did not contradict it", not "the re-run confirmed it". Those
are different claims and this document should not blur them.

## 17. Decoy escorts work, and nothing had to special-case them

| package (about 2,600 Materiel) | real drones through | turret destroyed |
|---|---|---|
| 3 real + 1 decoy | 2.23 of 3 (74%) | 67% |
| 2 real + 6 decoys | 1.70 of 2 (85%) | **80%** |
| 1 real + 13 decoys | 1.00 of 1 (100%) | 0% |

Trading warheads for decoys raises the survival rate of the real drones and, up
to a point, raises the chance of killing the target. Past that point there are
not enough warheads left to finish the job, which is the correct shape.

What is worth noting is that none of this is special-cased. A defence picks
targets by how much of one it can remove per shot, and a 90 hit-point decoy dies
to one shot exactly as a real strike drone does - so it is an equally attractive
thing to shoot at. The mechanic fell out of the targeting rule from item 8.

### Amended: the result is not refuted, it is no longer measurable

| package (about 2,600 Materiel) | real drones through, recorded | now |
|---|---|---|
| 3 real + 1 decoy | 2.23 of 3 (74%) | 3.00 of 3 (100%) |
| 2 real + 6 decoys | 1.70 of 2 (85%) | 2.00 of 2 (100%) |
| 1 real + 13 decoys | 1.00 of 1 (100%) | 1.00 of 1 (100%) |

The gradient was the finding. Trading warheads for decoys raised survival from
74% to 85%, and that rise is what "decoy escorts work" meant. Every mix now puts
every real drone through, so there is no gradient left to see - not because decoys
stopped working but because the turret stopped killing anything (finding 31). An
escort cannot be shown to protect a package that was never in danger.

The "turret destroyed" column is also gone: `RunDecoyStrike` still computes it and
`AverageThrough` discards it, so the second half of the table above is no longer
produced by the harness at all. Printing it again is the cheapest way to give this
experiment back some resolution, and it is a harness change rather than a finding.

Item 25 already superseded this entry's claim in its strong form. What the re-run
adds is that the weak form is not currently supported either.

## 18. Attacking from two altitudes at once does not work, and I think the model is wrong

| six drones, split | arrived | turret destroyed |
|---|---|---|
| all low | 5.2 | 100% |
| three low, three high | 5.2 | 100% |
| all high | 5.8 | 100% |

Height helps a little. Splitting never beats committing everything high, at
either of the two scales tested.

The mechanism that should make splitting pay is that a mount has one barrel and
must physically re-lay between a low target close by and a high one further off.
That is now modelled - traverse at ninety degrees a second, plus a two-thirds of
a second penalty for changing height band - and it is too cheap to matter against
a forty-tick engagement cycle.

So either the tactic is not as good as it sounds, or the re-laying penalty is
badly understated. My guess is the second. Real figures for traverse rates and
re-acquisition time have been requested; this entry should be revisited when they
arrive rather than tuned until it gives the answer I expected.

### Amended: the penalty was not understated. It was zero, and unreachable.

This needs saying plainly, because it is a different kind of error from the rest
of this document and the difference is the lesson.

**What was believed:** that a mount with one barrel must physically re-lay between
a low target close by and a high one further off; that this was modelled, at
ninety degrees a second plus two thirds of a second for changing band; that the
modelled penalty was "too cheap to matter against a forty-tick engagement cycle";
and therefore that the penalty was **badly understated** and the tactic probably
sound.

**What it was measured against:** a Gun Mount with no traverse rate. The field was
never set on the unit, and `SlewTicks` returns early on a zero traverse rate. For
the one unit every turret finding in this document was measured on, the penalty
this entry called too cheap was **exactly zero**, and no value of it could have
been reached by any experiment, because the code path returned before reading it.
There was no number to understate.

That is not a mis-estimate. A mis-estimate says the world is 3 and the model says
1, and it is fixed with a better number. This entry compared a measurement against
a mechanism, found the mechanism too weak, and concluded its constant wanted
raising - when the constant was not being read. The fix was not a number. It was
one `grep` for call sites, which is the same tool that would have caught items 29
and 30 and is now a build step.

**What the number is now.** The mount carries 150 degrees a second, from
`point-defence.md`'s light electro-optical turret figures. Holding the whole
scenario fixed and switching only the traverse rate between that and the old
instant slew, sixty trials a cell:

| | 3 drones | 5 drones | 8 drones |
|---|---|---|---|
| traverse wired, 150°/s | gun lost 80% | 100% | 100% |
| traverse not wired, instant | gun lost **46%** | 100% | 100% |

Thirty-four points at the only drone count that still discriminates. Far from
being too cheap to matter, re-laying is the **largest single term** in whether a
gun mount survives an attack: of the 3.2 seconds between the mount first holding
a track and the drone striking it, about 1.7 are spent slewing (finding 31). The
guess recorded above was wrong in both halves - the penalty was not understated,
it was absent; and once present it is if anything generous to the attacker.

**The tactic itself is now unmeasured rather than disproved.** Every row of the
vertical experiment reads 6.0 arrived and 100% turret killed, with a radar and
without, and has read exactly that on every build in this pass, on the one before
it, and after the engagement-channel change that landed while this was being
written. The experiment cannot distinguish an attack split across two altitudes from
one that is not, so it cannot support this entry's conclusion in either
direction. See finding 32.

## 19. Thermal was modelled backwards

It was treated as unaffected by darkness. It is substantially *better* in
darkness and substantially worse in daylight, because it works on temperature
contrast and sunlight heats the background until there is little contrast left.
The same imager that finds a warm engine against a cold sky before dawn can
struggle past a few hundred metres on a hot afternoon.

Corrected to 0.55 by day, 1.25 at night, 0.90 at twilight.

The related error was treating all drones as similarly warm. An electric
quadcopter runs its motors at forty to eighty degrees and its battery at sixty; a
combustion engine runs at four to eight hundred, and a turbojet hotter again.
Thermal signatures were widened accordingly - electric airframes down to 8-10,
combustion strike drones up to 60, turbojet to 85. The consequence is that the
cheap drones are the hard ones to find on heat and the expensive ones are easy,
which is the right way round and was not true before.

## 20. Acoustic reach was fantasy

The model gave a gun mount 400 map metres of acoustic detection. Published
experiments put practical acoustic detection of drones at tens of metres for a
bare microphone and around 160 metres for a large array, degrading badly in wind.
Cut to 130.

This matters because it reverses a conclusion. With 400 metres of hearing,
microphones were the best anti-drone sensor at any hour and darkness was worth
little. At a realistic 130 they are a last-ditch sensor - closer to a trip-wire
than a search sensor - and night matters again. Deeper research is running and
this number should be checked against it.

**Amended: survived, and the trip-wire description is if anything too generous.**
Measured against a quadcopter, the acoustic channel reaches **48 m by day and
96 m at night** - the night figure is the doubling that already exists in the
model for still air, not a new effect. Forty-eight metres is inside the drone's
own strike range plus a second of flight. The one thing the re-run adds is that
after dark acoustic is now the *best* channel a mount has, at 96 m against 15 m
of camera, which is the opposite of the daylight picture and is why item 12's
"best sensor at any hour" had to go.

## 21. Nothing is a switch

Detection was binary: in range or not. It now has a solid band inside about two
thirds of a sensor's reach and an intermittent one beyond, with per-channel
reliability - passive radio listening 95, optical 88, thermal 84, radar 78,
acoustic 52 - and a two-second track hold so a marginal contact does not strobe.

Radar was also rescaled to the fourth root of cross-section rather than the
square root, which is how the radar equation actually behaves and is why a drone
the size of a dinner plate is so much harder than an aircraft.

### Amended: this entry described behaviour that did not exist

**What was believed:** the sentence above, "a two-second track hold so a marginal
contact does not strobe", and the title drawn on top of it.

**What the model actually did:** `SimConstants.TrackHoldTicks` had **zero call
sites**. `Reaches()` keyed its edge roll on target, tick and channel and was
deliberately memoryless, so a contact in the intermittent band was re-rolled
independently **thirty-two times a second**, with no memory of the tick before.
The solid band was real. The per-channel reliabilities were real. The hold was
prose. A marginal contact did not fail to strobe: strobing at 32 Hz is precisely
what it did, and a gun that lost its track re-paid the acquisition cost each time
it came back.

So the entry was not merely optimistic about a number. It named a mechanism, drew
a conclusion from that mechanism, and the mechanism was not there - the same
failure as item 18, found by the same one-line check, in the same audit.

**What it does now, measured in isolation.** The hold is wired at 64 ticks, and it
is the one change in this pass that made the defence better rather than worse.
Isolating it - the commit that wires it touches nothing else in the simulation -
the number of drones reaching the gun across the whole reach sweep falls from 7.7
to **7.4** of 8 at that commit pair, and the five-drone saturation row goes from
the gun always losing to losing **88%** of the time. A gun that no longer drops a
fringe track kills about a third of a drone more per engagement.

That is a real effect and a small one, and it is swamped by the two changes in
this pass that run the other way - the honest 85 m range and the corrected
signature table. The title stands - nothing is a switch - and it did not stand
when it was written.

---

## 22. Aperture: seeing far and seeing wide are the same budget

A camera or a thermal imager has a fixed number of pixels, and it spends them on
either a narrow slice of the world seen a long way off, or a wide slice seen
close in. It cannot have both. The model had no concept of this: every sensor saw
its full reach in every direction at once, which is not how any of them work.

Pointed sensors now have an arc, a facing, and optionally a sweep rate, and reach
scales as the square root of how narrow the arc is. The same 600 m camera:

| arc | reach against a quadcopter | covered at once | heads for 360° | cost |
|---|---|---|---|---|
| 30° | 402 m | 8% | 12 | 5,400 |
| 45° | 329 m | 12% | 8 | 3,600 |
| 90° | 232 m | 25% | 4 | 1,800 |
| 120° | 201 m | 33% | 3 | 1,350 |
| 180° | 164 m | 50% | 2 | 900 |
| 360° | 116 m | 100% | 1 | 450 |

So a mount has four options and all of them cost something. A narrow staring head
sees furthest and is blind over eleven twelfths of the sky. A sweeping head covers
everything and is looking somewhere else two thirds of the time. A panoramic head
has no blind side and less than a third of the reach. Several heads have neither
problem and cost several times as much.

**Microphones and passive radio listening are exempt**, because they are
omnidirectional by nature. That is not a special case, it is the reason those two
channels exist: they are the cheap way to know that something is out there and
the useless way to know where it is. Which in turn is the argument for a line of
cheap short-range acoustic posts rather than one expensive mount - the coverage
comes from the count, not the quality, and detection is already the union of
everything a side owns, so a sensor line works without any code for it.

This is the most legible thing added so far. It is one slider, it is drawable as
a wedge on the map, and every position on it is a defensible purchase.

**Amended: the shape survived exactly, every number fell by about a third.** The
square-root-of-arc law, the coverage fractions, the head counts and the costs are
all unchanged; the reaches are not, because the corrected visual signature table
and the night-optical constant moved the base figure this table is a function of.

| arc | reach vs a quadcopter, recorded | now |
|---|---|---|
| 30° | 402 m | 255 m |
| 45° | 329 m | 208 m |
| 90° | 232 m | 147 m |
| 120° | 201 m | 127 m |
| 180° | 164 m | 104 m |
| 360° | 116 m | 73 m |

The argument is untouched and the purchase decision is the same one. What changes
is that a 360-degree head now finds a quadcopter at 73 m, which is inside the
range at which a drone is already committed - so the cheap panoramic option has
crossed from "less reach" into "barely a sensor", and the case for a line of
short-range acoustic posts that this entry ends on is stronger than when it was
written.

## 23. Autonomy is modelled as one thing and is two

From the autonomy research, and not yet implemented.

What is actually fielded in 2026 is **last-mile terminal guidance**: a human picks
the target, and the machine flies the final few hundred metres. That removes the
radio link, not the operator, and it makes the drone markedly *more* accurate -
reported improvements from around twenty percent to around eighty - because it
beats a dead link rather than out-recognising a human.

**Full autonomous target selection** - a machine choosing what to attack from a
class filter and an area - has never been independently verified anywhere, six
years after the first contested report. The 2026 claims rest on official
statements about a test plus thinly sourced operational reporting, and contradict
the stated policy of the side making them.

The game treats these as one thing: autonomy costs no crew and pays an error
rate. That is wrong for the thing that exists and speculative for the thing that
does not. It should be two tiers:

| | crew | link | accuracy | decoys |
|---|---|---|---|---|
| Last-mile guidance | still needed | immune once committed | **better**, not worse | no effect |
| Autonomous selection | none | immune | error rate | large effect |

No error rates are published by anyone, because nobody has ground truth. Any
number here is an estimate and should be labelled as one in the data.

## 24. Economics: there are two valves, not one

From the economics research.

"Airframes are cheap, crews are the cap" is most of the picture and misses the
first valve. The best-evidenced fact in 2026 reporting is that drone factories
sit partly idle for want of orders - installed capacity of eight to ten million
small airframes a year against about four and a half million planned purchases.

So the honest model has two constraints in series: **money gates production,
crews gate employment.** The game currently only has the second, which is why a
player with a large bank and no crews feels correct and a player with crews and
no bank does not exist. Worth adding before the economy is tuned.

## 25. The decoy's reflector is physically right and tactically inert

This supersedes item 17, and it is a correction to something I reported as a
success.

Item 17 said decoy escorts worked and nothing had to special-case them. Half of
that was true for the wrong reason. Two research passes then found that the radar
signature scale could not express what a decoy is: the 0-100 index was linear and
fed to a fourth root, so a decoy at 80 escorting a strike drone at 60 was detected
seven percent further out. Three orders of magnitude of real cross-section
difference had been flattened into nothing. That is fixed - the scale is decibels
now, and the probe confirms the intended behaviour exactly:

```
radar reach vs decoy   2373 m
radar reach vs strike   750 m       3.16x, as designed
```

Then I put a control into the decoy experiment - the same package run again with
the reflector stripped - and the two columns came out **identical to two decimal
places at every mix**. The reflector buys nothing.

The reason is not the reflector. It is that the game detects at 1400-2400 metres
and kills at 85-320:

```
radar mast detection    1400 m (2373 against a reflector)
gun mount kill ring        85 m
interceptor battery       320 m
```

Early warning cannot matter when nothing can shoot at the range the warning
arrives. By the time anything enters a weapon's envelope it has been found by
optics and microphones anyway, so the radar track is redundant at the only moment
it could pay.

**What is actually missing is a cued launch.** A reflector decoy does not fool
gunnery and was never meant to; what it does is make an air defence commit an
interceptor, and a crew, and several minutes, against an inflatable. The game has
interceptors but scrambles them by hand, so there is no decision for the decoy to
corrupt. Until a radar cue can launch something, this whole category of unit is
buying a property the game has nowhere to spend.

So: the signature model is right and should stay. Item 17's conclusion holds only
in its weaker form - a decoy is one more thing worth shooting at, and shot-value
targeting handles that without special cases. The interesting half is not
implemented yet.

The general lesson is the same one as item 15 and item 18. A number can be
corrected to something demonstrably more truthful and change no outcome, because
what was wrong was never the number. Verifying the fix against its own mechanism
(the probe) said it worked. Verifying it against the game (the control column)
said it did not.

**Amended: still literally zero, and that is now weaker evidence, not stronger.**
The control column was re-run on the corrected tree and the corrected world. It is
still identical to two decimal places at every mix - 3.00 against 3.00, 2.00
against 2.00, 1.00 against 1.00 - so the headline claim of this entry holds
exactly as recorded, and the range arithmetic it rests on is unchanged: the probe
still reports 2,373 m against a reflector and 750 m against a strike drone, and
the weapons still kill at 85 and 320.

But the column no longer *demonstrates* it. Both sides of it now read every real
drone through, on five different builds spanning every change in this pass, and
they read the same on the build before any of them. A control that cannot move
under any intervention is not evidence that the thing it controls for is inert;
it is evidence that the experiment is (finding 32). This entry's conclusion now
stands on its arithmetic, which is checkable, and no longer on its control, which
is not.

That is a third variety of the same trap and worth naming beside the other two.
Item 21 believed a mechanism that was not running. Item 18 tuned a constant that
was never read. This entry drew a sound conclusion from an instrument that
happened to be stuck, and was right anyway - which is the most dangerous of the
three, because nothing about the result looked wrong.

**One prediction against this entry has since been tested and failed.** A mount
was given an explicit engagement channel - it now commits to a target and holds it
rather than re-scoring every candidate each time it comes off cooldown - and the
change was made partly on the reasoning that fire-splitting was a direct cause of
the reflector measuring as inert, because nothing ever made a mount spend a whole
engagement on a decoy. It is a good argument and the change is right on its own
research. It moved the control column by **nothing**: 3.00 against 3.00, 2.00
against 2.00, 1.00 against 1.00, unchanged to the hundredth.

Which is the fourth time in this document that a mechanism was corrected, verified
against itself, and changed no outcome in the game. The reason is the one already
written above and it has not altered: a mount that fires once (finding 31) cannot
divide its fire, so there was no division to remove.

## 26. The top rung of the link ladder was unconditional, and untested

The control-link ladder is the spine of the game: radio dies to jamming, mesh
dies with its parent, fiber is unjammable but leashed, satellite is unlimited
range and scarce capacity, autonomy has nothing to jam and pays in target
selection. Each rung is supposed to buy something and pay for it somewhere.

Satellite was not paying. `LinkResolver` read:

```csharp
case LinkKind.Satellite:
    // Coverage is everywhere; the scarce thing is a channel.
    return true;
```

A literal unconditional true. Uplink capacity was the only cost, which makes the
rung a strictly better radio for anyone who can afford the channel.

The correction came from outside the research corpus - a satellite constellation
is licensed **by country**, so the coverage boundary is a national border. And a
border is not a front line. It was drawn before the shooting started and it does
not move when the fighting does.

Modelling those as two separate things produces a mechanic nobody designed:

> **Push an offensive past your own border and your drones are unsupported over
> ground you have taken and hold.** The constellation is not watching the war, it
> is reading a map. The top rung of the ladder is the one that punishes success.

That is now a test. So is the fact that crossing a geofence is **instant** rather
than a fade - every other way of losing a link in this game degrades through
amber first, and this one cannot, because the drone did not fly out of range of
anything.

**The part worth recording is the test coverage.** After making the change, the
full suite passed - 78 of 78. The default territory is unowned, so that change had
just made every satellite drone in the game permanently black, and nothing
failed. A grep explains why: across tests, headless missions and balance
experiments, the string `Satellite` appeared **zero** times, and exactly one unit
in the catalog carries the link. An entire rung of the game's central mechanic
had no coverage at all, and a green suite said nothing was wrong.

This is the third time in this project that a green signal has been worthless
(see 15 and 25). The pattern is consistent enough to state as a rule: **a passing
suite is evidence about the paths the suite walks and nothing else**, and the
most dangerous code is whatever is both important and never constructed in a
test. Worth an audit of which other catalog entries and enum branches are never
instantiated anywhere.

What is still missing, and is the interesting half: what a drone *does* past the
line. No comms and no satellite navigation means onboard sensors, and matching
what a camera sees against stored imagery requires imagery **of ground you do not
control** - a resource the defender inherently has more of. That makes deep
strike a reconnaissance-supply problem as much as a flying one, and it is the
natural place for the two-tier autonomy split in item 23 to land: crossing the
geofence is a forced demotion from last-mile guidance to autonomous selection,
mid-sortie.

## 27. Robots are not safer. They are cheaper to lose.

From the re-run of the logistics research, which went from nine searches to
sixty-six and overturned the thing the whole logistics design was resting on.

The assumption - mine, never stated plainly enough to be checked - was that
ground robots took over the last kilometres because they survive better than a
vehicle with a person in it. The numbers say the opposite:

| | loss per run |
|---|---|
| Pickup or SUV on a logistics run | ~2-3% (derived) |
| Ground robot | 10-33% |

A robot is **three to ten times more likely to die on a given trip**. The reason
to send one is not that it lives; it is that when it dies, nobody was aboard, and
the thing that was lost cost eight to twenty thousand rather than a crew.

That is a completely different mechanic from the one I would have built. Robots
are not a survivability upgrade to be unlocked, they are a currency conversion:
**materiel spent instead of people**, at a poor exchange rate that is worth
taking anyway. It also explains a number that looked wrong - one Ukrainian
brigade losing two to five robots a day is not a system failing, it is the system
working as intended.

### Three corrections underneath it

**Signature drives attrition, and it is now sourced.** Robot survival is
collapsing - a named commander reports the same units going from twelve to
fifteen missions each, to eight or nine, to **three or four now**. Against that,
one purpose-built machine engineered to be near-silent and low-heat averages
**fifty-seven**. That is a fifteen-to-one survival difference attributed
directly to signature, in the same war, in the same year. The game's five-channel
signature model has been carrying air units only; this says it belongs on
anything that moves.

**The position-holding estimate was wrong in the same direction twice.** The old
figures - three to seven days before degradation, two to three weeks before
untenable - were labelled designer estimates and were far too generous. One
brigade went **up to seventeen days without food**, with soldiers stating they
fainted and could not defend their positions, and two commanders were sacked over
it. Positions become combat-ineffective at seven to seventeen days and untenable
in *months*, not weeks. Being cut off is much slower and much grimmer than the
game was going to model.

**Theatre demand and last-mile delivery differ by a hundred to one.** The
deep-strike research gives about 200 kg per soldier per day; a forward position
actually receives about **nine kilograms a day** - chocolate bars, oatmeal, a
bottle of water. Those are not the same quantity measured twice, they are two
different flows, and a game with one supply number would be modelling neither.

### The lesson, again

This is the same shape as items 15, 25 and 26. The logistics document was the
thinnest in the corpus - nine searches - and nothing about reading it revealed
that. It was fluent, well-structured, and wrong in the specific way that a
document reasoned rather than researched is wrong: confident exactly where the
evidence was absent. The tell was not in the text, it was in the search count.

Worth carrying forward: **ask what a document cost to produce before trusting
what it says.**

## 28. The corpus had missed an entire war, and autonomy is settled

From the second re-run. Two things, one embarrassing and one useful.

### The embarrassing one

`beyond-ukraine.md` — the document whose entire job is to stop this game being a
simulation of exactly one conflict — was written on a nearly-empty search budget
and **missed a state-on-state drone and missile campaign that had been running
since February 2026**. Air defences in the Gulf report hundreds of drones and
scores of ballistic missiles engaged; a Shahed reached a US tactical operations
centre on 1 March 2026 and killed six soldiers, with the intercept failing; the
campaign is estimated to have consumed most of a national interceptor stockpile.

That is the closest published analogue to an alternative campaign for this game,
and the corpus did not know it existed. It is a sharper version of item 27's
lesson: the thin document was not vaguer than the others, it was *absent* on
something central, and nothing in its prose signalled that.

### The useful one: autonomy is two tiers and the world has one of them

Item 23 has been open a long time. It is now settled by evidence rather than
argument:

- **Terminal guidance became routine during 2026.** Production airframes ship
  with it; the add-on costs about a hundred dollars.
- **Autonomous target selection did not.** Still in initial combat testing at the
  same date, officially not fielded, one contested single-source kill claim.

So the near-future this game is set in is a **terminal-guidance world**, and the
consequence is the one that inverts the design:

> **Terminal guidance keeps the crew.** It is not a step towards replacing
> people. It is a way of making the last two seconds survivable when the link
> dies, and it makes the shot *better*, because the hard part of an attack is the
> final approach.

The game had autonomy as one thing that saved a crew and paid an error rate.
That description fits neither tier: the common one saves no crew and pays no
error, and the rare one is speculative. Both tiers are now in the simulation, the
classifier no longer runs at all for terminal guidance — a decoy screen cannot
deceive a decision a human already made while they could see — and only one unit
in the catalogue carries target selection, which is the correct rarity.

### Three more corrections worth carrying

**A fiber drone is not invulnerable, and saying so was my error.** The claim
above originally read that a fiber drone's zero radio signature made it immune
and that directed energy was the missing counter. That is wrong, and the
catalogue says so:

```
Fiber FPV Team   SigRadio 0   SigThermal 8   SigAcoustic 12   SigVisual 15   SigRadar 22
```

It is visible on **four of the five channels**. A gun mount shoots it. An
interceptor rams it. Cameras, microphones, heat sensors and radar all find it.
What the fiber actually buys is exactly two things: it cannot be **jammed**, and
it cannot be found by **passive radio listening**. That is a narrow and specific
immunity, and it is already correctly modelled - which is what "the counter to
any rung sits one rung back" was always supposed to mean.

The error was reading "immune to the electronic-warfare channel" as "immune",
and it is worth keeping on the record because it is the failure mode this
project keeps hitting from the other direction: I generalised from one
interesting property to a claim about the whole unit without opening the file
that would have refuted it in one line.

**The real gap is smaller and sits on the interceptor.** Rotary interceptors are
a working counter-drone weapon when they carry a seeker, and the game's carries
optics only:

```
Interceptor FPV   SensorOptical 180   (no thermal)
```

At night that 180 collapses to about a quarter of its reach, against a target
with a visual signature of 15 - so an interceptor after dark can barely find
anything on its own and has to be cued by ground radar. That is realistic, and it
is also the shape of a real upgrade rather than a flaw: a thermal seeker is what
turns an interceptor from a daylight weapon into an all-hours one, and it should
cost accordingly. Directed energy remains a genuine absence from the point-defence
model, but as one more counter among several, not as the answer to an
invulnerability that does not exist.

**Crew skill is the variable, and there is now a clean measurement of it.**
Fiber FPVs in one theatre land about 19% of attacks; comparable hardware in
Ukraine lands 30–60% in clean conditions. Same technology, different crews. That
is the value of training isolated from equipment, and it is a strong argument for
the game's crew veterancy being a bigger lever than its tech tree.

**Falling vehicle losses are evidence of the drone threat, not against it.**
Russian vehicle losses fell to their lowest in 46 months — because the vehicles
stopped being used. Anyone tuning balance against a loss-rate chart would draw
exactly the wrong conclusion.

## 29. Nothing blocks anything. There is no line of sight at all.

The largest structural gap in the project, and it went unnoticed through five
separate sensor research passes, an offline propagation tool, and twenty-eight
findings — because every one of those was about how *far* a sensor reaches and
none was about whether anything is in the way.

Verified in code rather than assumed:

- `Terrain.BlocksGroundSight()` is **defined and never called**. Zero callers.
- `World.cs`, which contains every line of detection code, makes **zero** calls
  to `Terrain`. Detection is range, times sensor arc, times target signature.
  Nothing occludes anything, ever.
- There is **no elevation model**. `HeightTiles` and `HeightMetres` are the map's
  y-dimension, not altitude. The terrain is a flat grid of seven tile classes:
  Open, Road, Forest, PowerLine, Rubble, Water, Impassable. No trench, no tunnel,
  no hill, no building.
- Forest exists but does not block sight. It affects fiber-tether snag rate and
  whether a vehicle can drive there. Nothing else.

So every number produced by the sensor work — five channels, the aperture model,
the decibel radar scale, the reworked acoustic table, the propagation tool with
its four validated field anchors — **is computed as though the ground were a
billiard table**. A gun mount sees a drone through a hill.

The research knew and the game could not listen. `acoustic.md` §5a specifies a
terrain diffraction term of 8 dB at low frequency rising to 22 dB above a
kilohertz, and the game has no way to supply the flag it keys off.

### Why it hid

Because it is an absence rather than an error. Every previous finding here was a
wrong number, a wrong law, or a wrong assumption — all of which are visible in
something. This one is a system that was never written, in a codebase where a
function with the right name exists and is never called, which reads as
completed work to anyone skimming.

The tell, in hindsight, was available cheaply: `grep` for callers. A defined
function with zero callers is the single strongest signal of unfinished work in a
codebase, and it costs one command to check. That belongs in the same category as
item 27's lesson about search counts — **cheap mechanical checks catch things
that careful reading does not.**

### What it does not mean

Not that the sensor work was wasted. Reach and occlusion are independent terms
and the reach work stands. But it does mean every detection figure is an upper
bound, and the flat-ground assumption has been silently underwriting every
balance conclusion drawn so far — including, probably, some of the turret
findings in items 13 through 16.

Research is commissioned. The open question it has to answer is not "what does
terrain do" but **whether a tactical game on a phone can afford to know** —
detection is already the most expensive thing per tick, and a per-pair,
per-channel visibility test may simply not fit. A cheaper abstraction that
captures most of the behaviour would be a legitimate answer.

## 30. An audit of what was researched and never wired, and it is bad

FINDINGS 29 found that terrain occlusion was specified in the research and absent
from the code. The obvious next question was how many more there were. The answer
is **thirty-four dead, thirty-two missing, fourteen contradicted**, and it
invalidates several conclusions in this very document.

### The three that matter most

**The two-second track hold does not exist.** `SimConstants.TrackHoldTicks` has
zero call sites. Item 21 above — "nothing is a switch" — states it as implemented
and builds its conclusion on it. `Reaches()` keys its edge roll on target, tick
and channel and is deliberately memoryless, so every fringe contact flickers
thirty-two times a second. **Item 21 is wrong about its own mechanism.**

**The Gun Mount has neither a traverse rate nor a magazine.** Neither field is
set on it, so the slew code returns early and reload accounting is skipped
entirely. Which means **items 13 through 18 measured a turret with infinite
ammunition and instant slew** — including item 18's conclusion that the re-laying
penalty was "badly understated", when for that unit it was exactly zero. Every
turret conclusion in this document was drawn against a weapon that does not
behave the way the document says it does.

**There is no income.** Salvage is never collected, `SalvageCollected` is never
pushed, and `Materiel` only ever decreases. Tasking Points are credited and never
spent. The economy has one direction.

### The part that is my own work, this session

Worse, because it was written after the lesson was available:

- `NavState.ErrorMetres` is computed by the entire navigation system and read by
  nothing but tests.
- `ReferenceImagery.GrantAround` and `Invalidate` have zero callers.
- **`Territory` is never populated outside tests** — so in any actual match every
  satellite link is permanently black and scene matching can never lock.

I built three systems this session, wrote passing tests for each, and shipped all
three inert. The tests pass because the tests construct the state; nothing else
ever does.

And one outright regression: adding the autonomy tier early-return killed target
selection on three strike drones that had tuned `AutonomyQuality` and no tier.
Fixed by giving them terminal guidance — a Shahed-class airframe flies to
coordinates a human chose, so it never selects anything and its quality figure
was meaningless — which leaves the Autonomous Munition as the only
target-selecting unit in the game. That is the correct rarity and it should have
been the design from the start.

### The harness was lying too

Three balance experiments overwrite the Gun Mount's range **back to 550** — the
figure item 2 corrected to 85. And every experiment world is built with
`Fill(Open)`, clear weather, firm ground and no territory owner. So the
experiments ran on a flat, empty, weatherless map with a turret whose corrected
range had been undone.

### What to actually take from this

Not "the project is broken". The research is good, the systems are written, the
tests pass. What is missing is the wire between them, and **a passing test proves
the wire exists only where the test itself built it.**

The mechanical checks that would have caught nearly all of this are cheap and I
was not running them: a public symbol with zero call sites, a constant never
referenced, an enum value never compared, a def field never read outside the file
that declares it. That is the same lesson as items 27 and 29, arriving for the
third time, which means it is not a lesson yet — it needs to be a build step.

### Amended: all four are fixed, and the re-run is in items 2, 12, 13, 15–18, 20–22, 25 and 31–33

It is now a build step — `tools/check_dead_symbols.py`, run by `./build.sh`, with
the 86 it found recorded as a ratchet and anything new failing immediately.

Each of the three headline items above has been re-measured rather than argued
about, and the audit's own predictions did not all survive contact:

- **The track hold** is wired, and item 21 is amended. Its isolated effect is
  real, small, and the only thing in this pass that helped the defence.
- **The gun mount's traverse** is wired and is worth thirty-four points of win
  rate. The audit was right that it invalidated item 18 and wrong about which way
  the correction runs: the penalty was not understated, it was absent.
- **The gun mount's magazine** is wired and is worth **nothing at all**, because
  the mount never fires often enough to empty it. This entry says items 13–18
  measured a turret with infinite ammunition, which is true; the harness's own
  header goes further and says the five-round belt was already a binding
  constraint at higher drone counts, which is not. Measured, it binds nowhere:
  zero reload events across every experiment at every drone count from one to
  twenty-four, on the 85 m mount and on the 550 m one. See finding 31. That
  claim was produced the same way item 18's was — by reasoning about a mechanism
  instead of counting it.
- **The harness** no longer overwrites the gun's range and no longer fights on an
  empty map. The first correction moved almost every turret number in this
  document. The second moved **nothing**, and finding 33 is why that is the
  expected result rather than a disappointment.

## 31. The gun mount fires once

The single most consequential thing the honest re-run turned up, and none of the
four fixes predicted it.

A Gun Mount defending against an attack — one drone or twenty-four, on its own
85 m barrel or on the 550 m test mount, by day or at night — gets **one shot**,
and in one of the twelve configurations measured, two. Not five, and not "however
many fit into the crossing time". Measured by counting
reload events and reading `EngagementsRemaining` every tick across twenty runs at
each of six drone counts: **zero reloads, ever**, and the counter never fell below
four of five except once, on the 550 m mount against twenty-four drones, where it
reached three.

Traced tick by tick, a single FPV against the shipped mount:

```
tick 552   70 m   first detected             (the barrel reaches 85 m; the
                                              analytic sensor figure is 127 m)
tick 552   70 m   acquisition + slew begins  71 ticks = 2.2 s
tick 622   22 m   FIRES.  belt 5 -> 4
tick 647          the next shot would be allowed
tick ~650   0 m   the drone arrives
```

Three numbers do all the work. The mount **first holds a track at about 70 m**,
not at the 127 m its sensors compute, because a 120-degree head sweeping at
seventy degrees a second is usually pointed somewhere else and a contact in the
intermittent band has to latch. The drone covers that 70 m in **3.2 seconds** at
22 m/s. And acquisition plus slew costs **2.2 of those 3.2 seconds** — about 1.7 s
of it traverse, because the mount begins facing wherever it happened to be facing.

So the engagement is: see it, turn to it, fire once at twenty-two metres, die. The
measured hit rate on that one shot is about one in five.

Everything else about the weapon is downstream of that, and mostly unreachable:

- **The five-round belt cannot bind.** It is a limit on a fifth shot that is never
  taken. Run the same assault with the belt in place and with it removed, sixty
  trials a cell, and the two agree in every cell at every drone count — 2.8 / 80%
  at three drones either way, 4.8 / 100% at five, 7.8 / 100% at eight.
- **The traverse rate binds hard**, because it is spent inside the only engagement
  there is. Turning it off hands 34 points of survival back to the gun (item 18).
- **Barrel length barely matters.** The reach sweep reads 7.6 of 8 arriving at
  550 m, at 450 m and at 350 m — identical to one another — and 7.8 at 85 m.
  Twenty-five seconds of nominal exposure and 3.9 seconds of it produce almost the
  same result, because above about 70 m the barrel is not what stops the mount
  shooting.

This is item 9 arriving with a number attached. "You cannot shoot what you cannot
see" was implemented as a rule; what nobody had measured is that for the game's
only dedicated air-defence structure the seeing is so much shorter than the
shooting that the weapon is effectively a one-shot device.

Whether that is *wrong* is a design question rather than an implementation one. A
heavy machine gun getting one burst at a drone already inside a hundred metres is
not obviously unrealistic. It does mean the gun mount cannot be the answer to
anything, and that item 2's problem — a wall that could not be solved — has been
replaced by its mirror image.

The cheap lever, if one is wanted, is not the barrel. It is the 2.2 seconds. A
mount that starts an engagement already pointed the right way, or that has a
second head, or that is cued by another sensor, gets a second shot and roughly
doubles its output. That is also the shape of an upgrade a player can be sold.

Two changes landed in the simulation while this was being written — an engagement
channel, so a mount commits to one target rather than re-scoring every cooldown,
and an Autocannon Mount tier carrying a radar. Every figure above was re-measured
after both and none of them moved: same single shot, same zero reloads, same
trace. The autocannon itself is not measured here at all; no balance experiment
spawns it yet, and until one does it is a catalogue entry rather than a result.

### Amended: half of this was correct and the other half was a defect

A follow-up traced the same engagement with a probe that printed the mount's
facing, detection state and acquisition timer every tick, instead of reasoning
from the outputs. It found the finding above is two separate claims that happen
to look like one, and they have different answers.

**The first seventy metres are correct, and for the reason stated.** It is pure
arc phase. The optical channel reaches 127 m against an FPV and is solid inside
76 m; the head sweeps seventy degrees a second, so its 120-degree arc covers any
given bearing for a 55-tick window once per revolution. The window before the one
that caught this drone closed while the drone was still beyond 127 m. So it
crossed the entire intermittent band with the head pointed elsewhere and latched
at 72.5 m, already inside the guaranteed band. No double-charge, no arc reset —
all three were checked. The omnidirectional channel does not rescue it either:
acoustic scales linearly with signature, and an FPV at signature 12 gives a
200 m array twenty-four metres.

**The last nine ticks were a defect.** `Acquiring` was cleared after every shot,
so the next cycle re-entered the acquisition branch and paid for acquisition and
slew again — for a target the barrel had never left. Consecutive bursts came forty
ticks apart instead of twenty-four. Three separate doc comments in the codebase
say that is not supposed to happen, including one on the commitment mechanic
explicitly promising not to charge twice. Fixed: the mount holds its lay between
bursts and clears it only when the engagement genuinely ends.

Gun survival at three drones moved **80% to 62%** by day; shots per attempt went
1.00 to 1.80. Night did not move, because at 48 m of reach there is no time for a
second shot at all.

### And the belt claim above is wrong in a way worth keeping

> **The five-round belt cannot bind.**

That is true of the experiments and says nothing about the simulation, which is a
different statement and a much weaker one.

The belt was never unreachable in the code. It was unreachable in the
**experiment design**, before the fix as well as after it: every balance
experiment launches its drones simultaneously, so the mount gets exactly one
engagement window and the belt could not bind in any cell. Vary only the launch
stagger and it binds immediately:

| eight drones | reloads per sixty trials, before | after |
|---|---|---|
| simultaneous | 0 | 0 |
| three seconds apart | 33 | **60** |
| eight seconds apart | 60 | 64 |

So the belt-in versus belt-out A/B agreeing in every cell was a correct
measurement that licensed no conclusion at all. This is item 32's failure mode —
an experiment that cannot move the variable it is being read for — turning up in
a place item 32 did not look, one finding later, in a claim written by the same
pass that discovered item 32.

**One systematic bias, found and deliberately not fixed.** Fifty-eight of those
seventy-one ticks are a 180-degree swing plus a ground-to-low band change, because
a mount spawns facing due east and tracking the ground while every attack in every
experiment arrives from the west. That is not "wherever it happened to be facing"
as written above — it is reliably the worst case available. It was left alone
because no research figure covers a turret's resting facing, and inventing one
would move every recorded number for a reason nothing supports. It is the largest
remaining term in the 2.2 seconds and it is a design decision, not a defect.

## 32. Three of the ten balance experiments cannot detect a change to the simulation

Stacking, Vertical and Decoy Escort produced **byte-identical output on six
different builds** spanning the whole of this pass: with the gun's range
overwritten to 550 m and with the real 85 m; with a magazine and without; with the
track hold and without; with the twenty-number signature table applied and not;
with the mount free to re-score targets every cooldown and with it committed to
one engagement channel; on a flat, ownerless, uncovered map and on a mixed one
with a border and imagery. Every row of all three tables sits on a ceiling — 8
drones take any number of turrets, 6 drones take any altitude split, every real
drone gets through every decoy mix — and a measurement pinned to a ceiling is a
constant, not a result.

Those three experiments are the sole evidence for items 13, 16, 17, 18 and the
control column of item 25.

It is worse than it looks, because what resolution the suite has left is
concentrated in one cell. Across the whole of Saturation and Darkness the only row
that is neither 0% nor 100% is **three drones**: 80% by day, 100% at night. One
number, in one row, of one experiment, is carrying the entire turret balance of
this document.

The fix is not a finding, it is arithmetic: put the experiments back on their
knees. Fewer drones, more turrets, a defence that starts with an advantage. An
experiment whose answer is 100% before the run begins is a unit test asserting
`true`, and this project has made that mistake often enough (items 15, 25, 26, 30)
that it ought to be caught by the shape of the output rather than by someone
eventually noticing.

The general form, and it is the sharpest version of this project's recurring
lesson: **a green suite proves nothing about the paths it does not walk, and a
saturated experiment proves nothing about the variables it cannot move.** Item 30
learned the first half. This is the second.

### Amended twice, and the second amendment is the one that matters

**Amended once, in item 34: discharged, not fixed.** The catalogue rescale moved
all ten experiments and the guard reported zero inertia warnings. That was true
of generation 13 and it was a discharge by luck rather than by repair: nothing in
this entry's prescription had been done, and the three tables were still pinned.

**Amended again, by FINDINGS 40, and this entry's reasoning survives while its
list and its instrument do not.**

*The list is stale, as this entry half-predicted.* Stacking, Vertical and Decoy
Escort are no longer the three. Vertical and Decoys started moving; Stacking
resolves from 11% to 100% and was being flagged by a heuristic rather than by its
own numbers. The experiments actually sitting on a ceiling in generation 15 were
**saturation**, **night** and **vertical**, only one of which this entry names.

*The instrument that found them was wrong and is replaced.* The
sibling-correlation signal that grew out of this entry reached seven warnings out
of ten, including `sensors`, which carries no radar and was *supposed* to be
unmoved by a Doppler notch. FINDINGS 40 has the measurement and the replacement:
this entry's own method, reading the table for a ceiling, made mechanical.

*The prescription is right and was applied, with one correction.* “Fewer drones,
more turrets, a defence that starts with an advantage” fixed saturation and
darkness. But **more turrets is the one form of it that does not work here** and
this document already contained the proof: the stacking table measures four
mounts as worth two points over one against a tap. The defence that starts with
an advantage has to be a different mount, not more of the same one.

*And one ceiling is not a defect.* Vertical's Gun Mount column is 100% in all four
rows because a Gun Mount cannot reach the High band, and no flight size moves it
— two, three and four quads across every split give 99–100% in every cell. A
ceiling can be the comparison an experiment exists to draw. That case is now
excused in writing, by value, in `tools/experiment-ceiling-allow.txt`, so the day
those four numbers move the warning comes back.

*What this entry could not have known.* Its subject is experiments that cannot
detect a change. FINDINGS 40 found the larger version: experiments that never
instantiate the system at all. Four of the systems wired since this entry was
written — interception vectoring, emission control, ground radar, fiber — can be
broken outright without any of the ten printing a different byte.

## 33. The realistic world changed the description and not one number

AUDIT F34 found that every experiment ran on `Fill(Open)` with clear weather, firm
ground, no territory owner and no imagery, and concluded that five whole systems
were provably inert in every number the project had recorded. That is fixed: the
experiments now paint a road, a treeline and a stretch of rubble, set a weather
and a ground state, draw a border with an owner on each side, and grant each side
imagery over its own ground — and print all of it above their results.

Run the same assault on both worlds, holding everything else including the seed
and the start tick fixed:

| drones | flat, ownerless, uncovered | mixed terrain, border, imagery |
|---|---|---|
| 1 | 1.0 arrived, gun never lost | 1.0, never |
| 2 | 2.0, never | 2.0, never |
| 3 | 3.0, every time | 3.0, every time |
| 5 | 5.0, every time | 5.0, every time |
| 8 | 8.0, every time | 8.0, every time |

Identical. Every cell.

This is the correct result and worth recording as one rather than filing as a
disappointment, because it says exactly what F1, F5 and F6 say: **nothing consumes
any of it yet.** Terrain occludes nothing (item 29), navigation error displaces no
aimpoint, and reference imagery is granted and never read. Painting a forest on
the map cannot change a detection that never asks what is in the way.

So the harness fix bought honesty rather than accuracy. Before it, an experiment
silently asserted a flat empty world and reported the result as though it were the
game; now it states its world and the result happens to be the same. The
distinction is worth the work: the next time one of those five systems is wired
the experiments will already be exercising it, and the day the two columns diverge
will be evidence instead of a surprise.

The one thing to guard against is reading that table as permission. It is tempting
to conclude terrain does not matter. What it shows is that terrain is not yet
*connected*, and item 29 is unambiguous that when it is, every detection figure in
this document becomes an upper bound.

## 34. The rescale killed "the gun mount fires once", and inverted the decoy answer

The catalogue went to real metres and one global 4x time multiplier; the harness
followed; generation 13 is the first trustworthy measurement since. It costs this
document more than any previous pass, so what follows is a ledger rather than a
narrative.

### Item 31's headline is dead

> **The gun mount fires once.**

It does not. Against a single tap it now fires **4.95 rounds**, against a stream
**7.90**, and it empties its five-round belt in nearly every trial. Reloads
happen routinely.

"Effectively a one-shot device" was a property of compressed units, not of the
weapon. At 12:1 the mount reached 85 map metres and a drone crossed its envelope
in 3.2 seconds; at real scale it reaches 1,000 m and the crossing takes about
six seconds of play. The engagement was always long enough for several shots and
the units hid it.

**Item 31's amended half stands.** The `Acquiring` lay-and-hold defect — the
mount re-paying acquisition and slew for a target the barrel had never left — was
a genuine code fault and the fix survives the rescale. That is worth separating:
the *diagnosis* was right and the *conclusion drawn around it* was an artefact.

**And item 31's belt amendment is superseded in the opposite direction.** It said
the belt "could not bind in any cell". It now binds in every cell of every arm.
The reasoning behind that amendment was correct — a saturated experiment cannot
move the variable it is read for — and the specific claim was still wrong, for a
reason that had nothing to do with saturation.

### Item 17 and 25's control: the answer inverts

Heavy escort was the best package and is now the worst but one.

| package | before | after |
|---|---|---|
| 3 real + 0 decoy | 0.38 through, 22% killed | 0.43, 20% |
| 3 real + 1 decoy | 0.70, 37% | **0.75, 35%** |
| 2 real + 6 decoy | **0.98, 45%** | 0.43, **7%** |
| 1 real + 13 decoy | 0.48, 0% | 0.08, 0% |

Light escort is now clearly best and clearly beats the no-decoy control; mass
escort is close to useless. The gradient item 17 described survives in shape and
reverses in ordering. Item 25's separate arithmetic — that radar cross-section is
read at one place and gates only things already inside a weapon envelope — still
holds, now with a 3,840 m envelope rather than 320.

### The rest, briefly

- **Item 2.** "A single gun mount hard-counters every frontal attack" — three
  drones now take it 37% of the time.
- **Item 12.** Its recorded numbers were **already stale before the rescale** and
  do not match generation 12. It must be *replaced* from the current table, never
  multiplied. A stale number multiplied by twelve is a confident wrong number.
- **Items 13 and 16.** Answerable at last, and the answer is conditional: against
  one tap, four mounts read within two points of one mount in every column;
  against a stream they take six points off at three drones and twelve at six,
  and every supporting mount fires four and a half rounds instead of zero. The
  structural claim survives only for the simultaneous case.
- **Item 18.** Shape survives, every magnitude moved by three or more.
- **Item 22.** Aiming the narrow head was worth +26 points and is now worth
  **−5**; the sweep cost 18 and now costs 1. The "missing aim-a-sensor mechanic
  is worth 26 points" claim is gone. The conclusion that the panoramic head is
  the only one worth buying survives, for a different reason.
- **Item 32 is discharged**, not amended. All ten experiments moved and the guard
  reports zero inertia warnings. **Amended by FINDINGS 40 on both halves.** All ten
  did move, and that discharged nothing: the rescale moved every number in the
  harness without unpinning a single table, and saturation, night and vertical
  were still sitting on ceilings two generations later. The zero-warnings half is
  now a statement about an instrument that no longer exists — the inertia signal
  was measuring sibling behaviour and has been replaced.
- **Item 10 is the only finding the rescale leaves standing verbatim** — mines
  are identical in every cell, which is exactly what a weapon with no sensor, no
  hit roll, no magazine and no arrival order should do.

### A trap worth naming

**Item 31's times do not multiply.** Its distances go up twelvefold and its
durations go up about twofold, in the opposite direction from the intuition, so
the engagement that was 3.2 seconds is about 6.4 seconds of play. Anyone amending
that entry by scaling every number in it by twelve will produce something
internally consistent and wrong.

### And a defect the rescale exposed, still open

The aimpoint-displacement gate does not ask whether a munition is **terminally
guided**. An FPV Team is dead-reckoning, one-way, *and* terminal-guidance on a
live radio link with a camera — and at real geometry it accrues 66 m of error
against a 40 m miss radius, so **every FPV strike in every gun-mount experiment
missed and the mount was immortal at every drone count.**

Nothing in `navigation-denied.md` says a munition being flown onto a target
through a camera should miss for navigational reasons; §5's separability argument
is about position versus operator. Nothing in the catalogue lets an attacker buy
out of it either. The harness worked around it by construction — moving the
border behind the objective, which is one of `SCALE.md`'s own three border
relations — rather than engineering a thin margin, and the interaction is still
measured on the flat control world. But the defect is in the simulation and it is
not fixed.

### Fixed, and the fix produced a better rule than the brief asked for

I framed this as "terminal guidance should defeat navigational miss". That was
the wrong question. The gate now asks **"is a person flying this on a picture that
is actually arriving?"** — the eye, not the tier.

Conditioning on `AutonomyTier.TerminalGuidance` would have double-counted the
seeker: `MunitionMissRadiusMetres` is *already* derived as a terminal seeker's
in-frame tolerance. And a plain radio FPV with no terminal guidance is flown the
same way and is equally indifferent to its own coordinates.

Two ordering details carry the fix, and neither was obvious:

**Crew is checked before the link pip, and must be.** `LinkResolver` pins
`LinkKind.Autonomy` to Green, because there is nothing on that rung to jam. Every
deep-strike one-way airframe in the catalogue sits on that rung with no crew, so a
pip-only test would have handed that entire family a blanket exemption from
navigation error — the exact opposite of the intent.

**Amber does not count.** A stuttering picture is the state a pilot is already
half blind in, and the decoy rule had already drawn its line at Green. Two
different definitions of "the pilot can see" would be worse than one imperfect
one.

### The black link compounds, which is a real mechanic

The open question was whether a drone that lost its link mid-flight — carrying on
to its designated point under `BlackPolicy.LastMile` — should still be exempt. No,
and the reason is in the code rather than in taste: `LastMile` navigates to a
remembered **world coordinate**, in exactly the drifted frame `ErrorMetres`
describes. Exempting it would mean the drone somehow knows the true coordinate it
was handed, which is the assumption the whole navigation system exists to deny.

So the rule reads in both directions now:

> Crossing the border costs you the operator. **Losing the operator is what makes
> losing your position cost you the shot.** Navigation error is latent until the
> link dies.

Jamming and navigation denial compound, for a stated reason rather than as a
tuning choice. This does not break `navigation-denied.md` §5's separability, which
is about a drone that *brought a map* keeping its position while still losing its
operator — it says nothing about charging a navigation penalty to a drone that
never lost one.

**One experiment moved, and it moved back into agreement.** The reach sweep's flat
control world went 100% to 89% mount survival, and now reads identically to the
same mount on the realistic world — which is what item 33 recorded before this
defect bit. The drones had always been arriving; only their warheads were landing
on grass.

**Neither number I suspected was the problem.** The 3%/metre drift rate is sourced
and the 40 m miss radius has an argued derivation. The gate was asking the wrong
question and no constant needed to move. Worth recording, because the reflex when
a model misbehaves is to reach for the nearest tunable.

What remains undefended is recorded in the constant rather than tuned away: a
one-way airframe with **neither** an operator nor a seeker is held to a seeker's
tolerance, making it slightly too accurate. The honest fix is a second number, and
a second unmeasured estimate is worse than a known conservatism.

## 35. The first play-test: interesting, not yet fun, and don't buy art

The simulation drew its first pixel. Seven research documents, thirty-four
findings, two build guards and about eight thousand lines of deterministic
fixed-point later, somebody looked at it.

The verdict, from an agent that drove it with a browser for ten minutes and read
fifteen screenshots rather than asserting it worked: **interesting, not yet fun.**
Both halves are worth having in writing.

### The fog of war is the best thing on the screen

This is the surprise, and it inverts the worry in `SCALE.md` that a phone-sized
viewport would make the game incomprehensible.

It is comprehensible precisely *because* the interface shows you **why** you can
see something — the sensor arc as a drawn wedge, a line back to the sensor holding
the contact, a letter for the channel that found it. So losing a contact reads as
"my eyes moved" rather than as a bug. Pushing a scout east, watching four
kilometres of enemy rear appear, and watching it vanish when the scout dies is
reportedly the most game-like moment in it.

And the three-step chain the scenario produces was not designed — it falls out of
systems that already existed. Fiber kills the jammer because fiber is unjammable
and the lane is clean; radio only then reaches the tank; the deep target needs
something flying on what it brought. That is the link ladder working as a
sequence of decisions rather than as a table of stats.

The log narrating `a link went black` and then `a warhead went off on empty
ground — it did not know where it was` is the whole design thesis explaining
itself to a player, unprompted, out of `SimEventKind`.

### What is dull is a design problem, not a rendering one

**The dead time.** A sortie is fifty to ninety play-seconds from pad to target.
You launch four and then there is nothing to do for a minute and a half — no build
queue, no defence, no mid-flight choice. `SCALE.md` argued the phone form factor
wanted "a queue of decisions"; what exists is four decisions and then three
minutes of watching. The tester ended up playing at 4x, which is the finding.

**You cannot lose.** The defender's drones accumulate into a dozen contacts and
never threaten a five-thousand-point command post. No pressure, therefore no
urgency.

**The map is nearly empty.** Twenty-nine kilometres holding about sixteen
entities. Semantic zoom works; there is not enough content to zoom into.

**And there is no hit event.** The simulation has a kill event and nothing for a
strike that lands and does not kill. That is a real gap in `SimEventKind`, worked
around in the interface with a damage bar — it is the difference between "four
drones into a tank" and "four drones into empty ground", and the interface should
not have to infer it.

### The recommendation, which I am recording because it will be tempting to ignore

**Do not spend money on art yet.** Not because it looks bad — coloured shapes and
one-pixel lines suit a game about sensor returns better than sprites would. Because
**the minute and a half of nothing between waves will not be fixed by art**, and
decorating it is paying for the wrong thing.

The order that would fix it, cheapest first, and none of it is renderer work:
make incoming drones interceptable — `Interceptor FPV` and the whole interception
path already exist and the player simply has no access to them; then shorten the
approach or widen the crew cap so more sorties are airborne at once; then a losing
condition that can actually fire.

The renderer is now adequate to see whether any of that helped, which was the
entire point of building it crude.

## 36. Interception: it was a missing system, not a missing interface — and the crew cap is not the constraint

KILL ZONE is a video game. Everything below is measured inside a fictional
simulation against fictional factions; the numbers are the game's, not anyone's.

Item 35 ended with an order of work, cheapest first: make incoming drones
interceptable, then widen the crew cap, then a losing condition. The first two are
done and the second one turned out to be the wrong thing to do.

### The answer to "interface or simulation" was neither

`IsInterceptor`, `InterceptBaseChance`, `ResolveInterception`, `CueMultiplier` and
`SpeedRatio` all existed and all described the **terminal** moment — the roll at
the merge. Nothing ever got the interceptor to the merge. It flew the order every
other airframe flies: at the target's position *this* tick, recomputed next tick. A
pure curve of pursuit, which against anything faster than you is a tail chase you
lose.

So the vectoring mechanic had never been built, and every stat describing
interception was a stat about a moment that could not be reached. This is the
FINDINGS 30 shape again — a complete-looking subsystem with no path into it — and
it survived an explicit audit of exactly that kind because the audit looked for
symbols with no callers, and all of these had callers.

### Three bugs came out first, and two of them are why nothing was ever inbound

Found by probing before building, which is now the house style and keeps earning
it:

1. **`Scenario.cs` passed the tick as the launch index.** `SortieSystem.Launch`
   spaces a flight off the pad with `EgressUntilTick = tick + 8 + index * 4`, and
   the defender's standing order handed it `tick`. Forty play-seconds in, that is a
   five-thousand-tick hold, and it grows for the rest of the match. **Every airframe
   the defence ever launched sat on its pad until the game ended.** One character.
   That is the entire content of "you cannot intercept what the defender sends" —
   there was nothing to intercept.
2. **With that fixed, the defence could still see nothing to launch at.** Its
   order is gated on `IsDetectedBy` and its forward-most eye was a tank with 4.3 km
   of optics in a 90° arc, 5.8 km from the nearest thing the player owned. Three
   hundred play-seconds of a passive match: zero detections, zero launches.
3. **A sortie whose target died froze forever and kept its crew.** `MovementSystem`
   fell back to `OrderPoint`, flew there, and the arrival test refused to clear an
   order still naming a dead handle. The defence chases the player's *one-way*
   drones, which kill themselves on impact, so every interceptor it launched
   stalled over a corpse. Six crews gone inside a minute.

Bug 3 is the one that matters beyond this scenario: it is the crew leak that made
the cap look tight. See below.

### What got built, and what the measurement says

`World.TrackQualityOf` returns `None` / `Optical` / `Radar`, lifted out of
`CueMultiplier` so that the vector an interceptor flies and the terminal roll it
makes read the same answer about the same contact. `MovementSystem.InterceptPoint`
solves the meeting point by three fixed iterations — the closed form squares
range × speed and overflows Q31.32 at these distances, which is `SCALE.md`'s
46,340 m ceiling turning up somewhere nobody expected it — and flies a fraction of
the computed lead set by what the track is made of: **1.00 radar** (a measured
velocity), **0.55 optical** (passive EO/IR gives bearing and no velocity; the 0.55
is a marked designer estimate, not a sourced number), **0.00 with no track**.

Twenty trials per row, same seeds, pure pursuit against vectored:

| | before | after |
|---|---|---|
| FPV (33 m/s) head-on | 11/20 | 11/20 |
| Jet (140 m/s) head-on | 7/20 | 7/20 |
| **Jet crossing, radar alive** | **0/20** | **7/20** |
| **Jet crossing, radar dead** | **0/20** | **3/20** |
| Jet crossing, two interceptors | 0/20 | 10/20 |

The lead matters exactly where it should and nowhere else. Head-on needs no lead
and gets no benefit; crossing against a target four times your speed is otherwise
impossible. And the radar mast finally buys something: `ResolveInterception`'s doc
comment has claimed for months that "killing the radar is how you open the sky",
and until now that sentence was about nothing.

### Two models declined, and the reasons are worth more than the models

**Bracketing.** Built, then deleted. The argument for it was that two interceptors
should cover an adversary's *choices* rather than measurement noise — hunting, not
error. That is right, and it is why the spread version was wrong. But **this game
has no evasion, so its targets have no branches, so a second interceptor is
honestly one more terminal roll**: 7/20 → 10/20, which is what 1−(1−p)² predicts.
Bracketing without evasion is a coefficient with a story attached. The real
version is recorded in the code instead of faked: `IsPilotedOnLiveFeed` already
separates airframes that *could* break from autonomous ones that cannot, and the
catalogue already prices branch count — a jet turns at 22°/s and an FPV at 180°/s,
which is few large options against many small ones.

**Miss distance.** The better model, declined on cost. Removing the
`RandomStream.Interception` draw reorders every recorded result exactly as adding
one would; `InterceptBaseChance` becomes a dead `UnitDef` field and fails the
guard; the static Interceptor Battery does not fly and would have to keep the roll
anyway; and at 10.6 m of travel per tick a two-metre lethal radius needs a swept-
segment closest approach, not a point test. Shipped instead was the half that pays
now at no risk: **track quality is visible on every contact** — filled ring radar,
open ring optical, dot for memory — so a player can see what their 300 materiel is
buying before they spend it.

### The crew cap: do not move it. Money binds, not hands.

Item 35 proposed widening the crew cap. Measured, not argued:

- **The scenario does not run at 6 crews. It runs at 14** (`StartingCrews` 6, plus
  two Crew Quarters at `CrewsPerQuarters` 4). The premise was wrong.
- In the *before* play-test the player sat at **14/14 at every sample from t=53 s**.
  Crews were never binding; there was nothing to spend them on.
- Recovery is 8 play-seconds against a 50–90 second sortie. The roster refills far
  faster than flight time drains it. **The cap that bites is transit.**
- Playing the defence hard for ten play-minutes: 29 interceptors, 11 kills, crews
  finish **14/14**, materiel finishes **2,500 of 12,000**.
- The only time crews ever ran out was bug 3 above — a leak, now fixed.

So `FINDINGS §24`'s two valves are both working and the one biting is money, which
is correct for a defensive spend: 8,700 materiel for 11 kills against 200-materiel
airframes is a real decision, and it is a decision about economy rather than about
hands. A wider cap would have loosened the valve that was not closed.

### The dead time is measurably gone

Item 35's complaint was four decisions and then three minutes of watching. Before:
`air=[]` at all twelve samples across 123 play-seconds — **zero** things to react
to, and the log four identical lines of *a warhead went off on empty ground*. After,
headless over ten play-minutes: **97% of five-second samples have an enemy airframe
in sight, longest empty stretch five seconds.** First contact moved from T+100 to
T+0.

That is the specific complaint answered with the specific measurement. Whether it
is *fun* still wants a human at a keyboard.

### Three things that are still wrong, undressed

1. **The quiet returns at the end.** A player who intercepts aggressively runs to
   2,500 materiel and then gets a 195-second empty stretch, because they have
   bankrupted themselves. Arguably correct — you spent your way out of the game —
   but it is item 35's problem wearing a new hat and a human should say which.
2. **With the jammer alive, the defence hits nothing.** Thirty-four sorties, zero
   hit points taken off the player. Its own EW Post throws a 5.4 km bubble across
   its only launch corridor, so every raid it flies goes black on departure and
   finishes on a remembered coordinate. Kill the jammer — objective one — and the
   same defence takes the player's relay mast off the map. **This was not tuned
   away.** It is `FINDINGS §21`, "nothing is a switch", arriving as a decision: the
   first thing on the objective list is also the thing protecting you. It is either
   the best thing in the build or it reads as the opposition being broken, and that
   is a judgement a play-test makes, not a measurement.

   **Amended — answered in finding 37, and not the way this entry expected.** The
   judgement it was waiting for turned out not to be needed: the defence was never
   supposed to be standing in its own bubble with no way out of it. It has an off
   switch now, jamming stays team-blind, and the same thirty-four sorties for zero
   hit points became seventeen for seven hundred and seventeen. The sentence above
   about the objective list still holds and is now a schedule rather than a trap:
   the jammer protects the defence for forty play-seconds in every eighty, and the
   window it opens for itself is the same window the player's own radio flies in.

   It is worth keeping this entry's framing because it was right about the shape
   and wrong about the remedy. "Either the best thing in the build or the
   opposition being broken" assumed the choice was to tune the jammer or leave it.
   There was a third option and it was an order that did not exist yet.
3. **Autonomous deep strikes always miss.** Team 2 holds reference imagery only
   east of the border, so anything autonomous over the player's ground has no
   scene-matching lock and `NavMissedAimpoint` fires every time. The entire
   autonomous threat axis is deleted by navigation denial working exactly as
   designed. Correct mechanically, empty as content: the fix is imagery coverage in
   the scenario, not a change to the navigation model.

### A drift was attributed to the wrong change, and checking was cheap

The delivered report read the `range` experiment's flat-control row moving 100% →
89% as a consequence of the new one-way expend-on-arrival rule. Item 34 had already
recorded that same move, attributing it to the aimpoint-displacement fix, and
predicting it would land at exactly the realistic world's 89%.

Both cannot be the cause. Building the commit that carries item 34 and none of the
interception work reads **89%** — so item 34 was right, needed no amendment, and
the only outstanding action was acknowledging the baseline it had already explained
in prose. Generation 14 is recorded.

The cost of settling it was one worktree and four minutes. The cost of not settling
it would have been an amendment to a correct finding, which is the failure mode
item 30 is named after: **the error looks like a fix.**

## 37. Emission control: the off switch, and the price of not having one

KILL ZONE is a video game. Everything below is measured inside a fictional
simulation against fictional factions; the numbers are the game's, not anyone's.

FINDINGS 36 closed with three things still wrong and undressed. The second was
this: *"with the jammer alive, the defence hits nothing. Thirty-four sorties, zero
hit points taken off the player."* Its own EW Post at x=18,000 throws a 5,400 m
bubble and its forward pad at x=19,800 sits 1,800 m inside it, so every raid it
flew went black off the pad and finished on a remembered coordinate.

The underlying gap is one line wide. `JamEmitter` carries a `Team` field and
`SignalGrid.SampleFor` takes no team, so a bubble denies everybody standing in it.

### The obvious fix was declined, and the reason is the whole entry

Reading the `Team` field and exempting your own side is one line. So is a reduced
own-side penalty. Both were declined.

Electronic fratricide is a real phenomenon and a reduced-penalty coefficient is a
number nothing in this repository measures — research queue brief M, question 4,
is the brief that would give us one, and it has not run. So jamming stays fully
team-blind and **the mitigation is emission control**: the cost of a jammer is
paid in a decision about when to radiate rather than in a coefficient with a story
attached. `EmitterState.Active`'s own doc comment has claimed since it was written
that *"switching off is a real option"*, and until now the only thing in the entire
repository that could take it was one line of `SimTests.cs`.

That is the same declination as FINDINGS 36's bracketing model, for the same
stated reason, and it is worth having the pair on the record: **the second
coefficient is the one to refuse.**

### What it is worth, measured

Same scenario, same seed, ten play-minutes, nobody at the keyboard.

| | before | after |
|---|---|---|
| defender sorties | 34 | 17 |
| of which finished on a drifted coordinate | **34** | 11 |
| hit points taken off the player | **0** | **717** |
| things of the player's destroyed | 0 | 1 |

Six of seventeen connect where none of thirty-four did. The defence flies half as
many sorties and stops being a fireworks display.

**Read each of those as one sample rather than a mean, and know why it can be.**
Three seeds — 20260917, 11111, 22222 — give *identical* counts on both sides of
the change, to the hit point, while their state hashes differ. The randomness is
live and simply does not reach these outcomes: in a passive match nobody rolls
for anything that decides them. So the comparison is clean and it is also narrow,
and a player doing things would move it.

### The half that is the player's, and it is the larger number

The same bubble is across the player's approach too, and the window is theirs to
use. The player's raid, flown by the probe rather than by a person — one FPV Team
at the defence's forward tank every ten play-seconds, from the relay mast:

| | jammer always on | with emission control |
|---|---|---|
| sorties flown | 59 | 22 |
| hit points taken off the defence | **0** | **2,251** |
| its own links going black | 55 | 14 |
| finishing on a drifted coordinate | 11 | 2 |

Fifty-nine sorties for nothing, against a tank killed in twenty-two — the raid
stops at twenty-two because there is nothing left to aim at. **The thing that was
defeating the player's shallow strike was the defence's jammer being switched on,
and the thing that beats it is the defence switching it off.** That is the design
thesis arriving as a schedule the player can read off the log and plan against,
which is what `SimEventKind.EmissionsChanged` is for.

### A radar that is switched off cannot see

The half that needed no new numbers. An emitter with `Active` false is dark on the
**radar** channel only — `World.IsRadiating`, read by `DetectionRangeFor` and by
`ComputeDetection` — and keeps every passive channel it owns.

Twenty trials, a 140 m/s jet crossing, an Interceptor FPV launched off a pad
1,500 m off the target's track, the player's Radar Mast the only thing holding it:

| | intercepted |
|---|---|
| radar radiating | **8/20** |
| radar switched off by order | **0/20** |

`TrackQualityOf` reads `Radar` → `None` → `Radar` across the order and back, while
`IsDetectedBy` stays true throughout: the mast still hears the contact on its ESM,
it just cannot measure its velocity any more, so the interceptor flies pure
pursuit against a 140 m/s target at 85 m/s and never closes.

That is the same shape as FINDINGS 36's 7/20-with-radar against 3/20-without, and
the gap is wider here for a stated reason rather than a mysterious one: 36 killed
the mast, which left the player an optical track worth 0.55 of the lead. Switching
it off leaves **no** track — the Radar Mast carries no optics — so the lead
fraction is 0.00. Killing the radar and silencing it are not the same event, and
the difference is legible in the number.

### Pressure did not go away, but the pressure *metric* moved, and it is worth saying which

FINDINGS 36 recorded 97% of five-second samples with an enemy airframe in sight
and a longest empty stretch of five seconds. The same measurement now:

| | before | after |
|---|---|---|
| samples with an enemy airframe **in sight** | 117/120 (97%) | 112/120 (93%) |
| samples with an enemy airframe **airborne at all** | 120/120 | 120/120 |
| longest stretch with nothing in sight | 5 s | 15 s |

So this is a small regression on 36's stated number and not a regression in the
thing 36's number was measuring. The sky is never empty in either run; the raid
now arrives in bursts of five rather than as a continuous drip, and the gaps
between bursts are where the five samples went. Against FINDINGS 35's original
complaint — ninety seconds of nothing — fifteen is not the same problem wearing a
hat. It is recorded rather than tuned away because tuning the cycle to recover
four percentage points would be fitting the doctrine to the instrument.

### Two things that came out of probing and would not have come out of reading

**The brief for this work said `JamEmitter.Team` is never read. It is.**
`KZ.Play/Snapshot.cs:185` reads it, to decide whether a jamming dome is drawn at
all and whether it is labelled as the viewer's own. The substantive claim — that
nothing in the *simulation* reads it — survives, and the interface was quietly
depending on a field the audit had written off. Cost of checking: one grep.

**The defence's reconnaissance flies off the same pad, inside the same bubble.**
Six cycle lengths were measured before one was chosen, and the two built on a
60-second cycle — one with a 40-second quiet period and one with 30 — produced
*zero* damage and only seven or eight sorties, worse than doing nothing. The cause
is not the raid. It is that the Recon Wing launches on its own 45-second clock,
and when that clock lands while the post is radiating the scout goes black off the
pad, the defence sees nothing, and there is nothing for the raid to be launched
at. Everything the defence does downstream of its own eyes is gated by its own
jammer, which is the same finding as the headline arriving one level up.

Gating the scout's launch on the quiet window as well was tried and **reverted**:
at the chosen 80/40/20 cycle it produces a bit-identical state hash, so it is a
branch that never fires, which is the failure item 30 is named after. The
coupling is recorded here instead, where anyone changing those three numbers will
find it.

### What was deliberately not done

The radio signature column is **not** converted to log power. AUDIT F8 asks for it
and it is correct and it is blocked on brief M, because it means re-deriving every
radio value in the catalogue. So one consequence of the decoupling should be read
with that in mind: the Radar Mast now takes the existing `SignatureWhileEmitting`
default of 85 in place of its static 25, which under the √ law is 1.84× ESM reach
against it — and measured against the scenario, with everything else held, it
changed **nothing**: 34 sorties, 0 hit points, 117/120 samples, identical to the
digit. Only the state hash moved. The boost is arithmetically real and tactically
nothing, which is brief M's own argument, now with a measurement under it.

### The asymmetry is closed, and it was the right thing to argue about

The build of this entry shipped the order in the simulation with only the
opposition able to give it: `Host.cs` had no case and the interface had no
affordance, on the reasoning that a Host branch nothing produces is the dead-code
pattern this repository polices. The reasoning is sound and the conclusion was
backwards. A mechanic only the AI can use is worse than a dead branch, because it
is invisible rather than merely unreachable — and the fix that removes the dead
branch is to wire the other half, not to leave both out.

So the player has it now: the snapshot reports an own unit's emitting state when
it has an emitter, `Host` takes `kind: "emitting"`, and a button appears only for
a selection that can actually radiate. Verified against the running server rather
than by reading — the Radar Mast flips false and back, radar-held contacts go to
zero while it is quiet, and the log narrates both transitions.

One thing that cost ten minutes and is worth writing down: **the match starts
paused at tick 0**, so a command sent before the first resume is enqueued and
silently never applied. It is not new and it is not a defect, and it looks exactly
like a broken command. The first attempt at the round-trip test read as a failed
wire when it was a paused clock.

### FINDINGS 36 is amended, not replaced

Item 36's third open problem — autonomous deep strikes always missing, because
team 2 holds imagery only east of the border — is untouched and still open. Its
second is answered: the answer was not to tune the jammer down, and it was not to
move the pad. It was to give the defence the decision it was always supposed to
have and let the bubble keep hurting whoever is standing in it.

## 38. The Doppler notch, and a bearing that stopped being a firing solution

KILL ZONE is a video game. Everything below is measured inside a fictional
simulation against fictional factions; the numbers are the game's, not anyone's.

Two audit items, AUDIT F7 and F9, done together because they are the same
sentence read twice: **detection is not one thing**. What a sensor reaches, what
a sensor measures, and what a weapon may be pointed at were all the same boolean,
and the research says they are three different questions.

### Both audit entries survived contact with the code, and one line of each did not

Probed before building, which is the house style and earned its keep again:

- **F7's claim that a radar sees nothing on the ground is exactly right.** A
  Radar Mast against a Main Tank at any range on the map: `SigRadar` 0, radar
  reach 0 m, best reach across all five channels 0 m, `IsDetectedBy` false. The
  entry's line numbers have moved — `World.cs:857` is now a mine's trigger radius
  — but both gates were where it said, in `ComputeDetection` and in
  `DetectionRangeFor`.
- **F7's claim that `Entities.Velocity` is "read by nothing" is out of date**, as
  the brief already said: `MovementSystem.InterceptPoint` has read it since
  FINDINGS 36.
- **F9 understated its own case.** The entry says an ESM detection is treated as
  equivalent to a radar track. It was treated as equivalent to an *optical* one,
  which is worse than it sounds: `TrackQualityOf`'s second pass asked
  `BestDetectionRange`, which includes the ESM channel, so a drone held on
  nothing but its own video transmitter read as `TrackQuality.Optical` — and an
  interceptor sent at it flew **0.55 of a lead computed from a velocity nobody
  had measured, off a contact whose range was never known at all**, and then
  rolled its terminal chance at the 0.60 optical cue multiplier rather than the
  0.30 for firing blind. Measured, before the change: a Command Post at 6,000 m
  of ESM against an FPV Team 4,000 m away — optical reach 587 m, thermal 0,
  acoustic 0, ESM 5,019 m — reads `detected=True, track=Optical`.

### The scale the research is on is not quite the scale the catalogue is on

Worth recording because the next person to add a signature will hit it.

`radar-rf.md` §5's table is built on `radar_sig = 2 × dBsm + 60`. The code's
`RadarReachTable` is documented as `10^((S−80)/80)` with "signature 80 is one
square metre", which is `S = 2 × dBsm + 80`. The two are twenty points apart —
ten decibels, 1.78× of reach.

The catalogue follows **§5's** scale, not its own doc comment: §5 gives a small
quad 18 and the game has 22; §5 gives a Shahed 48 and the game gives the Heavy
Strike Drone 52; §5 gives a reflector decoy 90 and the game gives 92. So the tank
is written at **94, straight off §5**, because consistency with the fifteen
airframes already in the catalogue is what decides whether the decoy still reads
three times louder than the thing it escorts. Translating 94 through the code's
own documented anchor instead would have put an MBT at 5 m² rather than §5's 50,
and translating it the other way would have run off the top of the table.

The anchor mismatch is real and is not this item's to fix — it is absorbed by the
Radar Mast's reference reach of 16,800 m, which AUDIT F25 already says is too
long by about the same factor. Two errors covering for each other is a thing to
write down, not a thing to be pleased about.

### What the radar sees now

`SimConstants` carries §2B.3's table as a radial-velocity gate on the radar
channel only — under 1.5 m/s no contact at all, 1.5–4 m/s reach ×0.40 and
reliability ×0.30, 4–10 m/s ×0.80 and ×0.70, above that full — and a third
altitude band for the ground at ×0.40, which is §4's nap-of-the-earth figure
applied to the layer that is definitionally in the clutter.

A Radar Mast's reach, everything closing at its own speed:

| | signature | speed | radar reach |
|---|---|---|---|
| FPV Team / Scout Quad | 22 | 33 / 17 m/s | 2,532 m |
| Recon Wing | 40 | 25 m/s | 6,375 m |
| Jet Strike Drone | 50 | 140 m/s | 8,501 m |
| Heavy Strike Drone | 52 | 50 m/s | 9,005 m |
| Decoy Drone | 92 | 50 m/s | 28,477 m |
| **Main Tank** | **94** | 8 m/s | **8,044 m** |
| **IFV** | 90 | 9 m/s | 7,169 m |
| **Supply Truck** | 88 | 14 m/s | 8,460 m |
| **Motorcycle Squad** | 64 | 17 m/s | 4,240 m |
| **Logistics UGV** | 76 | 5 m/s | 2,396 m |
| Designator Team | 0 | 1.5 m/s | 0 m |

Every bold row was zero before. The one worth reading twice is the Supply Truck:
a soft-skinned lorry is **seen further than a main battle tank**, because it is
six decibels quieter and twice as fast, and speed is what buys a clean return.
That is the mechanic doing its job rather than a bug, and it is a better sentence
than anything a signature column alone could have produced.

The two foot teams keep a zero deliberately. §2.7 says radars classify "human"
alongside "vehicle", so a number would be defensible — but both walk at 1.5 m/s,
the floor of the notch, so their radial component is under it in almost every
geometry. A signature nothing can ever read is the disease WIRING-SPEC exists to
cure.

### A tank that stops disappears, and that is the whole of it

Through the production path — spawned, ordered to drive, ordered to stop, no
field written by hand:

| | radar reach | detected |
|---|---|---|
| parked | 0 m | no |
| driving at the mast | 8,044 m | yes, `TrackQuality.Radar` |
| driving across its face | 0 m | no |
| stopped again, past the track hold | 0 m | no |

### Hovering and tangential flight are now tactics, and against a jet they are a poor one

Finding 12 says the notch "makes hovering and tangential flight into real
tactics". It does, and the size of the effect is the interesting part, because it
is not the same for everything.

Two identical Heavy Strike Drones 7,000 m from the mast, one running in and one
running across: **9,005 m of reach against 3,602 m**, and the crossing one is out
of contact entirely at a range where the inbound one is a radar track.

But across a whole pass the notch costs an air-defence radar very little against
anything fast, because at 140 m/s a target has to be within about four degrees of
tangential to fall inside 10 m/s of radial. Ticks of a crossing pass held as a
radar track, mast offset from the target's track:

| offset | jet, 140 m/s | heavy strike, 50 m/s |
|---|---|---|
| 0 m (head-on) | 972 → 972 | 2,881 → 2,880 |
| 1,500 m | 956 → 954 | 2,841 → 2,826 |
| 4,000 m | 858 → 845 | 2,581 → 2,478 |
| 7,000 m | 552 → **494** | 1,813 → **1,634** |

Ten percent at the worst offset measured, nothing head-on. **The notch is brutal
against things that move slowly and barely touches things that do not**, which is
§2B.3's own stated reason for existing — "why radar handles Shaheds far better
than FPVs" — arriving as a measurement rather than as a claim.

### What can no longer shoot, quantified

`CanEngage` used to ask `IsDetectedBy`. It now asks `HasFiringSolution`, which is
true of every channel except a lone ESM bearing. `TrackQuality` gains a rung
below `Optical`:

```
None = 0  ·  Bearing = 1  ·  Optical = 2  ·  Radar = 3
```

`Bearing` falls through to the `default` arm of both existing switches, so an
interceptor vectored at a bearing-only contact flies **no** lead (it used to fly
0.55) and rolls at 0.30 rather than 0.60. Those two switches were checked before
the member was added, which is what the brief asked for and is the only reason
adding a middle rung was safe: both list `Radar` and `Optical` explicitly and
default the rest.

**The audit's claim that this inflates every ESM-carrying unit is true, and here
is the size of it.** An Interceptor Battery against a transmitting FPV quad: it
hears it at 6,024 m, sees it on thermal at 547 m, holds it on radar at 1,447 m
and only while it is closing, and its missiles reach 3,840 m.

| quad's range | crossing, before | crossing, after | closing, before | closing, after |
|---|---|---|---|---|
| 1,200–3,600 m | shoots at every range | **never shoots** | shoots | shoots |

And the audit's exact sentence — a Command Post's ESM makes a transmitting drone
shootable by anything on the team — measured at night, where a Gun Mount's own
camera reaches 305 m and its microphone 576 m against a quad, and its barrel
reaches 1,000 m. Rounds it spends on a crossing quad in forty play-seconds:

| quad's range | gun alone | gun + a Command Post 2.4 km behind it |
|---|---|---|
| 300 m | 1 | 1 → 1 |
| 500 m | 1 | 1 → 1 |
| 700 m | 0 | **5 → 0** |
| 900 m | 0 | **4 → 0** |

The outer half of every gun's envelope on the team was being filled in by a
building two kilometres behind it hearing a video transmitter. It is not any
more.

### The cross-fix was built, because it is the better game

`radar-rf.md` §3A.3's two-baseline rule: a bearing becomes a fix when two
listeners hold the same emitter and the bearings cross by more than about 20°,
because the fix error goes as 1/sin(crossing angle) — 131 m at 90°, 1,500 m at 5°.

Two Command Posts, one quad, same range, twice; the only thing that changes is
where the second listener stands:

| | track | may a weapon engage it |
|---|---|---|
| listeners 2 km apart, both on the target's bearing | `Bearing` | no |
| listeners 6 km apart, across it | `Optical` | yes |

A cross-fixed contact reports as `Optical` rather than getting a rung of its own,
and that is deliberate: the rung means "a position, and no measured velocity",
which is exactly what two crossed bearings deliver. A parallel concept would have
been a second thing to keep in step with `InterceptPoint`'s lead table.

**It does not fire in the shipped scenario, and that is stated rather than
hidden.** Team 1 owns two listeners (Command Post at x=3,600 and Radar Mast at
x=5,400, both on y=9,360) and team 2 owns one. Two listeners 1,800 m apart on the
same line of latitude cross at under a degree against anything east of them, so
the player's pair is geometrically one listener. The branch is live, reachable
and tested through `Spawn`/`Step` — a player who builds a second Command Post
somewhere other than next to the first gets a firing solution out of it — but
nothing in the scenario as laid out uses it. Whether that is content or a
placement puzzle nobody has been given a reason to solve is a scenario question,
and it is the next thing to try.

### Nothing regressed, and here is what was checked rather than assumed

- **FINDINGS 37's interception pair reproduces to the trial**: a 140 m/s jet
  crossing, an Interceptor FPV off a pad 1,500 m off its track, twenty trials —
  **8/20 radar radiating, 0/20 switched off**, before and after, identical.
- **Head-on interception, twenty trials each, unchanged**: FPV 10/20 → 10/20, jet
  8/20 → 8/20. (These are FINDINGS 36's rows at a different pad offset; the point
  is the pair, not the absolute.)
- **The play scenario is bit-for-bit unmoved in everything but its hash**: 17
  defender sorties, 13 link-blacks, 11 drifted aimpoints, **717 hit points off the
  player**, 1 unit lost, 112/120 five-second samples with an enemy airframe in
  sight, longest empty stretch 15 s. Identical on seeds 20260917, 11111 and
  22222. Only `StateHash()` moved, because the firing solution's track hold is
  persistent state and is now in it.
- **The player's raid is unmoved too**: 23 sorties, 2,250 hit points off the
  defence, 23 things destroyed. Of the 3,631 contact-seconds team 1 holds across
  ten play-minutes, **340 are now bearing-only** — 9% of everything the player
  sees is no longer shootable — and it changes nothing, because the player owns
  nothing that shoots. Team 2 holds **zero** bearing-only contact-seconds.
- **Cost: about 5%.** 6,090 → 5,765 ticks per second on the headless match, three
  runs each. The radial component needs one square root per radar-carrying sensor
  per target per tick; the square-free form does not exist, because the dot
  product of a 45 km baseline with a 140 m/s closure is seven figures and its
  square leaves Q31.32 an order of magnitude behind — `SCALE.md`'s 46,340 m
  ceiling arriving from the other side. An ESM-only contact also stops
  short-circuiting, because "is there anything better than a bearing" cannot be
  answered by the first bearing.

### The thing that did not happen, said plainly

**In the shipped scenario, neither change does anything at all.**

F7 gives the player's Radar Mast nothing, because of where it is standing: it
sits at x=5,400 and the defence's nearest vehicle is a tank at x=16,800, which is
11,402 m away against 8,044 m of reach. Contact-seconds across ten play-minutes
are identical to the digit before and after. The mechanic exists, the tests
exercise it, and the scenario's geometry does not reach it — a Radar Mast is a
rear-area building and ground search is a forward job, so the interesting version
of this is a forward radar the player has to push up and defend, which is a
scenario change and not a simulation one.

F9 gives the defence nothing either, because team 2's only listener is a Command
Post at x=25,200, eight kilometres behind the tank everything is shot at.

So the honest summary is: **two mechanics, both measured, both correct, both
inert where the game is actually played.** That is not an argument for reverting
them — the audit items were real and the old behaviour was wrong in ways the
Interceptor Battery table above makes obvious — but it is the difference between
"this changes the game" and "this stops the game being wrong when somebody
eventually builds the unit that cares", and the record should say which.

### Two experiments drifted, and both are the notch landing where it should

Reported, not acknowledged: `--update-baseline` was not run.

- **`decoys`.** The warhead and mast-killed columns, which are what FINDINGS 34's
  decoy conclusion rests on, **did not move in any row**. What moved is decoys
  shot in the 1-real-13-decoy package, 13.0 → 12.3, and rounds per kill, 21.2 →
  26.6 and 41.5 → 56.3. The defender is an Interceptor Battery, which carries the
  only radar in that experiment, and the package flies past it at a target 1,200 m
  behind it — so the radial component swings through the notch on the way past.
  FINDINGS 34's conclusion is unaffected and is not amended.
- **`vertical`.** 4.21 → 4.20 quads lost and 31% → 32% mount killed, in the
  **Autocannon Mount** columns only. The Gun Mount columns beside them are
  identical to the digit, which is the attribution: the autocannon is the one with
  `SensorRadar`. FINDINGS 18's conclusion is unaffected.

### Four tests were passing by construction, and the notch caught them

The first run after the notch landed broke four tests, and every one of them was
asserting something about radar against a target that was **hovering**: a fiber
quad placed and never ordered anywhere, two Multirole Quads compared across
altitude bands with neither of them moving, a decoy and a strike drone side by
side, and FINDINGS 37's own emission-control test, which launched a jet with no
target and left it parked in mid-air for the whole test.

None of them was wrong about the thing it was testing. All four were reading a
radar reach that, under the research the game is built on, should never have
existed. They are repaired by giving each target an order and a heading rather
than by relaxing the rule, and each repair says in a comment why the order is
there — because the next person to read "w.Step()" and a stationary drone will
otherwise put it back.

That is the WIRING-SPEC opening rule turning up from the other direction. The
tests did not build the state they asserted on; they inherited a *default* they
asserted on, and the default was zero velocity.

## 39. The filament is a line on the ground, and now it points at you

KILL ZONE is a video game. Everything below is measured inside a fictional
simulation against fictional factions; the numbers are the game's, not anyone's.

AUDIT F14. Fiber's design has always been three liabilities paying for one
privilege: a leash, a snag risk, and a thread lying across the map that an enemy
can find and follow home. Two were built. The third was three unconnected pieces
— a geometry function with one caller in the test file, an event kind nothing
pushed, and a launch point recorded on every thread and read by nothing — so the
unjammable rung has been shipping at two thirds of its stated price.

### The entry held, and two of the three symbols were wrong to wire at all

Probed before building. `AnySegmentNear` had exactly one call site and it was
`SimTests.cs:569`; `TetherFound` had zero pushes; `TetherLingerTicks` was already
keeping a dead man's thread on the map for thirty seconds, as the entry said.

Then the part that did not survive contact. **`Tether.Anchor` cannot hold the
bearing home, because production code can only ever set it to nothing.**
`SortieSystem.Launch` takes a pad as a `Fix2` — a point, not an entity — and
passes `EntityHandle.None` at every launch. The launch point the mechanic needed
is `AnchorPosition`, a different field, already read by `ConstrainVelocity`. So
`Anchor` is deleted rather than wired: a field that only a test can fill is the
same failure as a function only a test can call.

**`ComponentMask.Tethered` is separate from F14 and is also deleted.** Whether an
airframe is on a thread is `TetherId >= 0`, which `UpdateTethers`,
`MovementSystem` and `LinkResolver` all already read. A mask flag beside it would
be a second answer to one question, free to disagree with the first. That is one
audit item (F20) closed by subtraction, and the dead-symbol ledger is four lines
shorter: 60 known findings down to 56, 0 new.

### What finding one buys, and why it is a bearing

The option list ran from a log line to surfacing the launch site as a contact.
It is the strong end, with the game's existing vocabulary doing the limiting:

- `World.FindTethers` runs once a play-second over each side's **ground** units
  against the other side's live and lingering threads. Ground only, per
  `spec-technical.md` §4.4 — a filament is found by driving over it — which is
  also what keeps the scan cheap.
- Finding one gives the finder a **`TrackQuality.Bearing`** contact on everything
  inside a 1,080 m disc around the launch point, for twenty play-seconds. Seen,
  drawn, narrated, and **not shootable**: `CanEngage` asks `HasFiringSolution`,
  and FINDINGS 38's rung means "a direction and not a position".
- No roll. Discovery is geometry — `AnySegmentNear` against the polyline — so no
  stream gains a draw and no recorded result reorders.

The rung is doing real work here rather than being reused for tidiness.
`CanEngage`'s own comment says what a bearing is for: *it hands them a direction
to point something else in.* The defence's answer is to send the one sensor it
owns that moves — an FPV with a camera — which converts the bearing into a
position by arriving. The drone that was unjammable on the way in is what told
the other side where its operator sits, and it still cannot be shot at off the
thread alone.

**Numbers and their provenance.** 288 m to find a thread and 1,080 m of reveal
are `spec-technical.md` §4.4's 24 m and 90 m on the map-metre scale that document
was written at, scaled by twelve exactly as `TetherNodeSpacingMetres` was
(12 → 144). No research document in the corpus gives a figure for either, so both
say **designer estimate** in the comment and name what they are anchored on. The
twenty-second window is §4.4's, in play-seconds for the reason `TetherLingerTicks`
is: it is how long the player has to react, which is pacing.

### It fires in the shipped scenario, and here is how often

The scenario's own geometry does the work, which is the opposite of FINDINGS 38's
problem. The player's fiber targets sit on the defence's forward belt, and the
defence's Main Tank patrols across the lane the threads are dragged down — so the
vehicle being struck is the vehicle standing on the cable.

Ten play-minutes, the interface's own default pad, a fiber sortie at the forward
tank every ten play-seconds, nobody at the keyboard:

| | seed 20260917 | 11111 | 22222 |
|---|---|---|---|
| threads found | **7** | **9** | **8** |
| hit points off the player, before | 234 | 234 | 234 |
| hit points off the player, after | **600** | **600** | **600** |
| things of the player's destroyed, before → after | 0 → **1** | 0 → **1** | 0 → **1** |
| defender sorties, before → after | 20 → 26 | 20 → 31 | 20 → 28 |

The one destroyed is the forward **Relay Mast**, every time. In the run before
the change it ends the match at 366/600 and alive; after it, the defence stops
chasing drones for a few sorties and finishes it.

### Does fiber's advantage narrow? Partly, and it is now a choice about where you launch

FINDINGS 37's baseline for what fiber is worth is 59 radio sorties for nothing
against 22 through an emission-control window. In the scenario as it stands
today — the window exists, so radio works in it — the same ten minutes:

| | radio FPV raid | fiber raid |
|---|---|---|
| hit points off the defence | 4,202 | 2,250–3,500 |
| things of the defence's destroyed | 2 | 1–2 |
| hit points off the player | **0** | **600** |
| things of the player's destroyed | 0 | **1** |

Fiber was already the worse buy in this scenario on offence — 420 materiel
against the FPV Team's 200, so a 12,000 balance buys 28 sorties instead of the
clock's 59 — and it now also costs the player their forward relay. That is the price
arriving; it is not a claim that fiber is beaten, because the run where the
jammer never switches off is the run fiber exists for and this scenario no longer
contains one.

**And the price is escapable, deliberately.** The same raid flown from the
player's rear pad at (4,560, 8,400) instead of the forward one:

| | forward pad | rear pad |
|---|---|---|
| threads found | 7 | 7 |
| sorties launched at the revealed site | 6 | 6 |
| hit points off the player | **600** | **0** |

The thread is found just as often and the defence raids the launch site just as
promptly, and not one of those six sorties takes a hit point off anything: the
rear site is 15.2 km from the defence's pad while its radio reaches 12 km from a
relay at x=19,200, so the raid goes black short of the target and finishes on a
remembered coordinate. Nineteen drifted aimpoints in that run say so. So fiber's third liability is real
and it is **a decision about where you launch**: forward, and 16.8 km of spool
buys you a short flight and a found cable that gets your relay killed; from the
rear, you spend most of the spool to put the launch site outside the enemy's
reach and the found cable is a log line. That is the mechanic being interesting
rather than either inert or unavoidable, and it is the thing to check first if
anyone moves the defence's pad west.

### The scenario change, and what it cost

One branch, in the defender's standing orders: while the defence holds a thread
bearing, its four-second raid goes at the **structure** it can see at the launch
site instead of the nearest contact. Two things about it are worth recording.

**It still reads the world through `IsDetectedBy`**, like the line below it, so
the opposition is not being handed the entity table — it is being handed a
contact the simulation gave it.

**The first version picked the nearest *contact* to the anchor and was worth
nothing**: 7 threads found, 234 hit points off the player, unchanged to the
digit. The nearest contact to a launch pad is almost always one of that pad's own
drones two seconds after launch, which is what the doctrine would have picked
anyway. Measured, noticed, fixed by asking for a structure. A mechanic whose
consumer looks at it and chooses the same thing it always chose is indistinguish-
able from FINDINGS 38's inert pair, and this one was that for an hour.

### Nothing regressed, and here is what was checked rather than assumed

- **The passive scenario is identical on every counter, on three seeds:** 17
  defender sorties, 13 link-blacks, 11 drifted aimpoints, **717** hit points off
  the player, 1 unit lost, 112/120 five-second samples with an enemy airframe in
  sight. Only `StateHash()` moved, because the per-team bearing and the per-thread
  found-flag are persistent state and are now in it.
- **The radio raid is identical too**, before and after, on three seeds: 59
  sorties, 4,202 hit points off the defence, 2 things destroyed, 0 off the player,
  116/120 samples. With no thread on the map there is nothing to find, and the
  measurement says so rather than the argument.
- **FINDINGS 37's interception pair reproduces to the trial**: a 140 m/s jet
  crossing, an Interceptor FPV off a pad 1,500 m off its track, twenty trials —
  **8/20 radar radiating, 0/20 switched off**, before and after, identical. Pad on
  the track: 8/20 and 6/20, before and after.
- **The ten balance experiments: 0 drift.** `--update-baseline` was not run and
  did not need to be. The seven "suspected inert" warnings are the same seven that
  were there before and are FINDINGS 32's, not this change's.
- **137 tests pass**, four of them new.

### Cost: about five percent, and the polyline walk is not where it went

Twelve interleaved headless runs, six each: **5,492 → 5,212 ticks per second**,
5.1% down. The scan was the suspect and is not the answer. Isolated — the scan
present and the two detection-path lines removed — it is **about 1%**, which is
what the early-outs are for: a thread is found once and then skipped forever, and
a thread whose anchor is further away than its own spooled length plus the
discovery range cannot have a segment in reach, because every node is within
`Spooled` of the anchor by path length and therefore within it in a straight line.

The other four percent is the two lines added to the detection path — the reveal
branch inside `RebuildDetection`'s team loop and the `InFoundThreadReveal` tail on
`TrackQualityOf` — and I could not make that attribution convincing, because each
of them is a comparison against a `-1`. Turning the scan off does not recover it;
removing the whole diff does. It may well be code layout in a 2,300-line file
where two functions dominate the tick. It is recorded as a measurement I cannot
explain rather than explained badly.

### Two things that came out of probing and would not have come out of reading

**The brief's description of the third dead piece was wrong, and so was the
ledger's.** Both name `Tether.Anchor` as "the launch point recorded on every
thread and read by nothing". The launch point is `AnchorPosition` and it is read;
`Anchor` is a handle that is structurally always `None`. One grep, and it changed
the work from "wire three things together" to "wire two and delete two".

**The mechanic's first honest run produced no change at all in the outcome
column**, with the event firing seven times. That is the FINDINGS 38 shape
exactly — correct, reachable, and not touching the game — and the only reason it
is not this entry's conclusion is that the measurement was run before the write-up
rather than after it.

## 40. What the ten experiments actually touch, and it is less than anyone thought

KILL ZONE is a video game. Everything below is measured inside a fictional
simulation against fictional factions; the numbers are the game's, not anyone's.

The drift guard was reporting seven of ten experiments as suspected inert after
three sessions of wiring. Three explanations were on the table — the guard is
crying wolf, the experiments are saturated, or the harness has no coverage of
what was wired — and all three turned out to be true of different rows. This
entry is the evidence for each, in the order the evidence was taken, because the
third one is the expensive one and it would not have been believed without the
first two out of the way.

### The method: change one constant in the simulation and see who notices

Everything below rests on one procedure, which is worth naming because it is
cheap and this project has not used it before. Take a constant the last three
sessions installed, change it, rebuild, run all ten experiments, and compare
each one's output byte for byte against the same run on the unmodified tree.
An experiment that does not move has no path to that constant — not an opinion
about coverage, a measurement of it.

Eight mutations, each on a scratch copy of the tree, each a real rebuild and a
real run of all ten:

| mutation | what it breaks | experiments that noticed |
|---|---|---|
| `TrackHoldTicks` 2 s → 8 s | detection, everywhere | aperture, approach, night, range, saturation, sensors, stacking, vertical |
| `RadarNotch*` gates removed | FINDINGS 38's Doppler notch | decoys, vertical |
| `IsRadiating` → always false | every radar switched off (37) | decoys, vertical |
| `RadarGroundScale` 0.40 → 0.01 | FINDINGS 38's ground-clutter band | **nothing** |
| `InterceptLead*` 1.00/0.55 → 0.05 | FINDINGS 36's interception vectoring | **nothing** |
| `TetherDiscoveryRangeMetres` 288 → 2,880 | FINDINGS 39's filament discovery | **nothing** |
| `FiringSolutionReferenceSpeed` 45 → 90 | the speed term of every air hit roll | **nothing** |
| the same, 45 → 200 | as above, four times harder | **nothing** |

The track-hold row is the control and it did its job: a change to detection is
felt by eight of the ten, which is what a live harness looks like. Every other
row is the finding.

### Hypothesis 1 holds: the guard was measuring the wrong thing

`INERTIA_MIN_OPPORTUNITIES` flagged an experiment that sat unchanged while at
least three *other* experiments moved. That is a heuristic about a population
used to accuse an individual, and both of its failure modes were live in the
warning list it was printing:

- **`sensors` was a false positive by construction.** It compares optics,
  acoustic and thermal against a quadcopter and a tank. It carries no radar at
  all. A Doppler notch landing in the simulation *should* leave it byte-identical,
  and the mutation table above confirms it: the notch and the radar off-switch
  both leave it untouched, while the track hold moves it. It was insulated
  because it was supposed to be.
- **`stacking` was a false positive on its own numbers.** Its table resolves from
  11% to 100% across eight columns and it moved at generation 13. It was flagged
  because generations 14 and 15 happened to move three siblings.

Six of the seven warnings rested on that same two-generation coincidence: three
experiments moved at gens 14–15, the gate is three, and everything that did not
move was accused. That is not evidence of anything about the seven.

**The replacement is FINDINGS 32's own diagnosis made mechanical.** Item 32 named
three dead experiments by reading their tables — *"every row of all three tables
sits on a ceiling"* — and needed no ledger, no sibling and no history to do it.
The guard now does the same thing: every percentage cell inside a table, grouped
into columns, then *pinned* (at least half the column at 0% or 100%, the bounds
of a share-of-trials) or *flat* (every cell the same number).

One rule in it was not obvious and is the part worth keeping. **A sweep is
entitled to one rung at each end of its range.** Bracketing a transition means
having a rung the defence always survives and a rung it never does; without the
allowance, `approach` — 81, 86, 90, 86, 85, 94, 100, six rows doing work and one
running off the end — reads the same as `saturation` — 0, 0, 37, 98, 100, 100,
100, 100, four rungs saying one thing. With it they separate cleanly.

Seven warnings became three findings, and all three are real:

| | what the detector said |
|---|---|
| `saturation` | 6 of 8 cells at a bound |
| `night` | 3 of 4 at a bound in the night column |
| `vertical` | a column that does not vary at all: 100%, 100%, 100%, 100% |

`aperture`, `approach`, `mines`, `sensors` and `stacking` were cleared, which is
the point: four of them were never anything but correctly unaffected, and saying
so out loud is worth more than a warning that covers them along with everything
else.

### Hypothesis 2 holds for two of the three, and the third cannot be fixed by a ladder

**`saturation` and `night` were sampling past the transition rather than
measuring it.** A Gun Mount against a tap goes from 0% killed at two drones to
100% at six. Saturation swept 1, 2, 3, 5, 8, 12, 16, 24 — five rungs past the top
of the band — and darkness swept 3, 5, 8, 12, of which three are at or past it.
Both are rebuilt, and the rebuilding cost nothing that was being measured: three
drones is still 37%, five still 98%, three drones by day still 36% against 41% at
night, identical to the digit. The dead rungs were replaced with live ones.

Saturation now runs two arms, because item 32's "more turrets" is the one form of
its own prescription that does not work here — `stacking` already measures four
mounts as worth two points over one against a tap, since a supporting mount
commits to a target that is already being engaged. The defence that starts with
an advantage has to be a different *mount*, so the second arm is the Autocannon
Mount the catalogue already ships, swept 6 to 16 across its own band.

That arm pays for itself beyond un-pinning the column, and the answer is not the
one the ladder alone would suggest:

| mount | absorbs | cheapest cost to remove it | its own price | ratio |
|---|---|---|---|---|
| Gun Mount | 0% at 2 drones, 100% at 6 | 1,016 MAT | 450 | **2.3x** |
| Autocannon Mount | 2% at 6, 100% at 16 | 2,482 MAT | 1,600 | **1.6x** |

**The cheap mount is the better buy per Materiel and the expensive one is the
better buy per position**, and which a player wants is a map question rather than
a balance one. That sentence did not exist before, because the table it comes
from had four rows saying 100%.

**`vertical` is a ceiling that no ladder can move, and it is measured rather than
argued.** Its Gun Mount column reads 100% in all four rows because a Gun Mount
cannot engage the High band at all, so a drone sent there is outside the defence
rather than evading it. The obvious fix is fewer quads; it does not work. Flights
of two, three and four across every split:

| flight | splits tried | mount killed |
|---|---|---|
| 2 quads | 2/0, 1/1, 0/2 | 99%, 100%, 100% |
| 3 quads | 3/0, 2/1, 1/2, 0/3 | 100% in every cell |
| 4 quads | 4/0, 3/1, 2/2, 1/3, 0/4 | 100% in every cell |

Two Multirole Quads on a three-second spacing take a Gun Mount ninety-nine times
in a hundred. The column is a property of the units in the comparison, not of the
rungs the experiment chose, and it is the contrast the Autocannon arm beside it is
read against. So it goes in `tools/experiment-ceiling-allow.txt` with a written
reason, the dead-symbol allowlist's shape and rule — and the entry names the
column by its four values, so the day they move the entry stops matching and the
warning comes back.

### Hypothesis 3 holds, and it is worse than the question assumed

The ten experiments have **no coverage of any of the four systems wired in the
last three sessions.** Not thin coverage. None, in the strict sense that the
constants those systems are built on can be changed to values that visibly break
them and every one of the ten prints the same bytes.

| experiment | what it measures | can its output move | what it exercises |
|---|---|---|---|
| `saturation` | how much of a tap one point-defence mount absorbs, and what removing it costs | yes — changed at gens 2, 3, 4, 6, 11, 13, 16, and under the track hold | optics and acoustic detection, air hit rolls, the belt and the cooldown, pad egress, territory and navigation error |
| `range` | whether the barrel or the seeing is the binding number | yes — gens 2, 3, 4, 6, 9, 12, 13, 14, and under the track hold | the same, plus the flat control world as a territory comparison |
| `approach` | what a forward launch pad is worth | yes — gens 2, 3, 4, 6, 9, 11, 13, and under the track hold | the same, plus the track-latch geometry |
| `night` | what darkness costs a mount that sees with cameras | yes — gens 2, 3, 4, 6, 9, 11, 13, 16, and under the track hold | the day/night cycle against optics and acoustic |
| `mines` | what a mine does to each vehicle class, and whether it cares whose it is | no trial variance — no sensor, no hit roll, no magazine, no arrival order, so one run is the answer. It changed at gens 6, 12 and 13 when the world and the catalogue scale moved under it, and it is the one experiment the track hold correctly leaves alone | mine arming and trigger radius, damage by armour class, ground movement orders |
| `sensors` | what each sensor fit finds, and what finding it is worth | yes — gens 3, 6, 12, 13, and under the track hold | optics, acoustic, thermal, the day/night cycle. **No radar, by design** |
| `stacking` | whether more mounts covering one approach pay, against a tap and against a stream | yes — gens 6, 12, 13, and under the track hold; resolves 11%–100% across its own table | engagement commitment, the belt, arrival spacing |
| `vertical` | whether splitting an attack across altitude bands works | yes — gens 2, 6, 12, 13, 15, and under the track hold | altitude bands, re-lay cost, the upward-shot penalty, and **radar** — the Autocannon's |
| `decoys` | whether decoys buy warheads onto a target | yes — gens 2, 3, 6, 12, 13, 15 — but **not** under the track hold, which is unexplained and recorded as such | **radar**, ESM, interception rolls, radar cross-section, the Doppler notch |
| `aperture` | what an arc costs and what it buys | yes — gens 3, 6, 12, 13, and under the track hold | the arc/reach law, sweep versus stare, optics and acoustic |

Two rows in that table are not like the others and both are worth stating
plainly. `mines` is flat because a mine is flat, which the harness already
argued and this measurement supports rather than discovers. `decoys` sitting
out the track-hold mutation is a loose end: its defender is the only
interceptor in the harness and plausibly never loses a contact long enough for
a longer coast to matter, but that is a guess and it is written here as one.

And what none of them exercise:

- **Interception vectoring and the lead table (FINDINGS 36, 38).** Nothing in the
  harness launches an airframe that flies an intercept. `decoys` spawns an
  Interceptor Battery, which is a structure: it rolls `ResolveInterception` at the
  merge and never flies to one. Setting the radar lead to 0.05 and the optical
  lead to 0.05 — which is the mechanic switched off — changes nothing anywhere.
- **Emission control (FINDINGS 37).** No experiment issues the order; the string
  "Emission" does not appear in `KZ.Balance`. Forcing every emitter dark does move
  `decoys` and `vertical`, so the *radar channel* is exercised, but nothing in the
  harness can tell you what the decision is worth, which is the whole of item 37.
- **Ground radar and the Doppler notch against the ground (FINDINGS 38).** The
  clutter band can be taken from 0.40 to 0.01 with no effect on anything. `mines`
  is the only experiment with a ground vehicle under orders to drive, and there is
  no radar within reach of it; `decoys` and `vertical` are the only experiments
  with a radar, and everything they look at is airborne. The two halves of the
  mechanic are in different experiments.
- **Fiber (FINDINGS 39).** No experiment spawns a Fiber FPV Team. The whole tether
  subsystem — the leash, the snag, the thread on the ground and now its discovery
  — has never been in a balance table. Multiplying the discovery range by ten
  changes nothing.

**The pattern behind all four is the same and it is not laziness.** Items 36 to 39
were measured, carefully, through `KZ.Play`'s scenario and headless runs — twenty
trials of a jet crossing a mast, ten play-minutes on three seeds, contact-seconds
counted. Those are good measurements. They are also one scenario's geometry, and
item 38 says so in its own words: *"two mechanics, both measured, both correct,
both inert where the game is actually played."* The balance harness is where a
mechanic gets swept rather than sampled, and nothing from the last three sessions
has been swept.

### A fifth thing the sweep found that nobody was looking for

**The speed term of `AirHitChance` cannot be measured by this harness at all.**
`FiringSolutionReferenceSpeed` was moved from 45 m/s to 90 and then to 200 — a
four-fold change in the reference every air hit roll is scored against — and all
ten experiments printed identical bytes both times.

The cause is a clamp. The term is `reference / target speed`, clamped to
[0.20, 1.15]. Every airframe the harness flies against a gun is an FPV Team at
33 m/s or a Multirole Quad at 28, and 45/33 and 200/33 both land on the 1.15
ceiling. The only airframes above 45 m/s in any experiment are the Heavy Strike
Drone and the Decoy Drone at 50, and they appear solely in `decoys`, where the
defender is an interceptor and interceptors do not use `AirHitChance` —
`CombatSystem` branches to `ResolveInterception` before reaching it.

So FINDINGS 15's answer to the jet question — *"a turbojet strike drone crossing
at three times the speed of a quadcopter is not hard to shoot at, it is hard to
hit, and the speed term does that on its own"* — has no experiment behind it. The
claim may well be right; the harness cannot say, and could not have noticed the
term being deleted. Nothing flying faster than 50 m/s has ever been in a balance
table, and the one airframe the sentence is about, the Jet Strike Drone at
140 m/s, has never been spawned by `KZ.Balance`.

### What is not fixed, and what each missing experiment would be for

Three experiments are specified and not built. They are named here rather than
guessed at later, and each one says what it is *for* before anybody writes it:

1. **A radar experiment.** To answer what a radar is worth against a ground
   target and what the notch costs, by sweeping a vehicle's *radial* velocity
   rather than its range: a Radar Mast against a Main Tank driving in, driving
   across, and parked, at several offsets. It is the only shape that puts a
   moving ground vehicle in front of a radar, which is the half of FINDINGS 38
   nothing currently reaches. It also gives the emission-control decision its
   first sweep: the same table with the mast radiating and dark is what the
   order is worth.
2. **An interception experiment.** To answer what the lead table buys, by
   sweeping target speed and crossing angle against a flying interceptor. It
   needs the Interceptor FPV, which the catalogue ships and no experiment has
   ever spawned, and it is the natural home for the Jet Strike Drone — which
   would also, as a side effect, be the first experiment in which
   `AirHitChance`'s speed term is off its clamp.
3. **A fiber experiment.** To answer what the thread costs, by sweeping the
   distance between the launch pad and the enemy's ground traffic: the same
   sortie flown from a forward pad and a rear one, against a lane a vehicle
   patrols. Item 39 already found by hand that this is a decision about where
   you launch; a sweep is what turns that into a number a player can be
   taught.

None of the three is written here. The diagnosis was the expensive part and it is
worth having on its own, and an experiment written in the last hour of a session
is how the harness got into this state.

### The general form

Item 30 learned that a green suite proves nothing about the paths it does not
walk. Item 32 learned that a saturated experiment proves nothing about the
variables it cannot move. This is the third one and it is the one that needed a
tool rather than a reading:

**An experiment proves nothing about a system it never instantiates, and there is
no way to tell which those are by looking at it.** The ten read as a broad suite.
They are ten variations on one engagement — a mount, a pad, and FPV Teams — and
the way to find that out was to break the simulation on purpose and see who
complained.


### Amended by FINDINGS 41: the method is a tool now, and two of the three specifications needed correcting

*The sweep is `tools/check_experiment_mutations.py`.* The procedure this entry
describes — break a constant, rebuild, run all ten, compare byte for byte — is a
standing guard with its own subcommand and its own self-test, and the eight
experiments this entry measured under the track hold reproduce exactly. What the
tool adds is that an experiment now **declares** the constants it claims to
measure, in its own source, and a claim that stops being true fails rather than
going stale the way this table would have.

*Specification 2 was wrong where it said an interception experiment would take
`AirHitChance`'s speed term off its clamp.* It would not: `CombatSystem` branches
to `ResolveInterception` before `AirHitChance` is reached, which this entry says
itself two sections earlier. The experiment carries a third arm — the same
airframes against an Autocannon Mount, which shoots rather than intercepts — and
that is what reaches the term. Measured there for the first time: 100% against a
50 m/s airframe, 46% against the 140 m/s jet.

*Specification 1 was wrong where it said "the same table with the mast radiating
and dark" would give emission control its first sweep.* Run dark, that table is
eighteen cells of zero — a Radar Mast's only other sensor is passive RF and a
Main Tank's radio signature is 0 — and there is no continuously varying quantity
behind the order to sweep. It is recorded as a two-row comparison instead.

*Specification 3 held as written.* The fiber sweep is the one that changed a
conclusion, and not the one it was aimed at: what decides whether a filament is
ever found is the ground it is dragged over, not the distance it is dragged.

## 41. The three experiments, and the tool that says whether they work

KILL ZONE is a video game. Everything below is measured inside a fictional
simulation against fictional factions; the numbers are the game's, not anyone's.

Item 40 diagnosed the harness and named three experiments it did not write. This
is those three, plus the instrument its diagnosis was made with, turned into
something that stays.

### The tool first, because the experiments are only worth what it says they are

`tools/check_experiment_mutations.py`. Item 40's method, unchanged: take a
constant, change it to something visibly wrong, rebuild, run the experiments,
compare byte for byte against the same run on the unmodified tree. **An
experiment that does not move has no path to that constant.** What is new is that
it is a standing guard with its own subcommand (`./build.sh mutations`) and its
own self-test, and that the claim it checks is written where an experiment's
author has to look at it.

Two files, and which fact lives in which was the only real design decision. The
**claim** goes in `src/KZ.Balance/Program.cs`, on a `// MEASURES <experiment>:
<mutation>, ...` line beside the experiment, because a manifest is a second place
to remember and the person rewriting an experiment does not open it. The
**mutation** goes in `tools/experiment-mutations.txt` — file, the exact text to
replace, what to replace it with, and one line of prose saying what the
simulation stops doing — described once, so the hundredth mutation costs four
lines. `docs/EXPERIMENT-DRIFT.md` has the rest, including why it is deliberately
not in the default `./build.sh`.

The rule with a failure behind it: **a mutation's `from` text must match its file
exactly once**, and zero matches is a hard error rather than a skip. A mutation
that has stopped applying has been testing nothing since whatever renamed the
line, and it reads as coverage while being none — which is this repository's own
argument about checkers, applied to a checker.

**Fourteen mutations, twenty-six claims, twenty-six verified.** The ten existing
experiments were given claims only after the sweep proved them, so the guard
starts green with no baseline file: anything unverified appears as an uncovered
mutation, which is a warning and a visible gap rather than suppressed debt.

It found the first thing nobody was looking for within a minute of first running:
breaking a mine's fuze made `mines` divide by an observed zero and crash. A
harness that crashes rather than reporting "the mechanic did nothing" cannot be
asked whether the mechanic matters. Fixed, and the fix changes no output on the
unmodified tree.

It also took a line off the dead-symbol ledger without being asked. `SimEvent.B`
— the killer on a `UnitDied` event — had been write-only since it was written.
Two of the three experiments below read it, because "the raid is no longer alive"
and "the defence killed the raid" are different questions.

### Radar: the notch is not a modifier on the radar, it is the radar

`radar`. A Radar Mast at 14,400 m, a Main Tank driving a 12,000 m leg past it,
swept by the vehicle's closest approach and its heading. No trial loop: nothing
here draws from a random stream — the edge-of-envelope roll in `World.Reaches` is
keyed to the tick and the pair — so one run is the answer, the same argument
`mines` makes.

Each cell is the share of the drive's ticks the mast holds the tank at all.

| closest approach | driving in | driving across | parked |
|---|---|---|---|
| 300 m | 46% | 29% | 0% |
| 1,200 m | 47% | 31% | 0% |
| 2,400 m | 53% | 39% | 0% |
| 3,600 m | 50% | **79%** | 0% |
| 4,800 m | 28% | 49% | 0% |
| 6,000 m | 13% | 26% | 0% |
| 7,200 m | 0% | 0% | 0% |

The headline is in the experiment's header rather than its table: **a mast holds
a tank closing head-on out to 8,044 m and a tank crossing its face at 0 m, at any
range whatever.** That is item 38's mechanic swept for the first time, and it
survives the sweep intact.

Three things came out of it that were not in item 38.

**Armour can never be a full-strength radar return in this game, and the loudest
thing on the ground is a supply truck.** The Doppler bands are: under 1.5 m/s not
detected, 1.5–4 at ×0.40 reach and 30% reliability, 4–10 at ×0.80 and 70%, over
10 full. Everything armoured is below 10 m/s — Main Tank 7.5, IFV 9 — so it sits
in the middle band whatever it does, and no manoeuvre available to it gets the
missing fifth of the mast's reach back. Only the two soft-skinned vehicles can
reach the top band, and they are also the two with the smallest cross-sections,
so the trade could go either way. A third table asks which way, measured off a
driving vehicle rather than computed:

| vehicle | speed | cross-section | band | held out to | share of the pass |
|---|---|---|---|---|---|
| Motorcycle Squad | 17 m/s | 64 | full | 4,240 m | 33% |
| Supply Truck | 14 m/s | 88 | full | **8,460 m** | 69% |
| IFV | 9 m/s | 90 | medium | 7,169 m | 55% |
| Main Tank | 7.5 m/s | 94 | medium | 8,044 m | 47% |

For the Supply Truck the band wins: 88 of cross-section at full Doppler out-reaches
the tank's 94 at 0.80 of it. For the Motorcycle Squad it loses badly — 64 of
cross-section is about half the tank's reach even at full Doppler. **A radar mast
is at its best against exactly the traffic it was not bought for.** (Four rows and
not the whole roster: the two ground robots need a link to move and a parked robot
is the notch rather than a measurement, and the EW Truck and Logistics UGV carry
radio signatures the mast hears on its ESM, which would make the last column a
measurement of something other than radar. The four above are crewed and
radio-silent.)

**The parked column is the notch in its pure form** — same tank, same mast, same
position, nothing different but whether the engine is running — and it is the one
cell-for-cell comparison in the table. It is excused in
`tools/experiment-ceiling-allow.txt` as a `control`, with the two moving columns
at the same seven offsets recorded in the reason as the measurement that it is
the motion and not the rungs.

**The crossing column peaking at 3,600 m is left standing rather than smoothed.**
Nearer than that, a crossing tank spends most of its leg at an angle where the
radial falls into the 1.5–4 m/s band — short reach *and* 30% reliability, so the
contact is both closer and intermittent. Further out the whole leg is past what
the mast reaches. 3,600 m is where the geometry puts the entire crossing inside
the medium band and inside the envelope at once. The two moving columns are not a
controlled comparison of heading and the experiment says so in its own output:
both legs are 12,000 m, but one spans every range from 13 km down and the other
never leaves the stated range by more than half its length.

### Emission control has no sweep in it, and that is the measurement

Item 40 specified "the same table with the mast radiating and dark". The dark arm
is eighteen cells of zero. A Radar Mast's only other sensor is passive RF and a
Main Tank's radio signature is **0**, so a dark mast is blind to a tank by
construction rather than by degree — and eighteen cells of zero is item 32's unit
test asserting `true`, inverted. It is printed as two rows instead:

| the mast is | heard at | a listener at 3,000 m holds it | its own 1,200 m pass |
|---|---|---|---|
| radiating | 4,425 m | 100% of ticks | 47% |
| dark | 2,400 m | 0% of ticks | 0% |

**Switching off does not hide the mast.** It moves the range at which the other
side's passive listening reaches it from 4,425 m to 2,400 m — a factor of 1.8,
because a quiet mast is still a structure with a radio signature of its own — and
it pays for that with the entire ground picture. Item 37's order is not a posture
a player holds while still working; it is a decision to stop seeing in exchange
for halving the radius inside which somebody can find you.

A ladder over the listener's range was tried first and rejected: passive
listening is the most reliable channel in the model, so the share is 100% out to
the reach and 0% past it with almost nothing in between, and a seven-rung ladder
of 100s and 0s is a ceiling at both ends pretending to be a sweep. Two honest
rows beat it.

### Interception: flat until the target outruns the interceptor, and then it is everything

`intercept`. One Interceptor FPV — an airframe the catalogue has always shipped
and no experiment had ever spawned — scrambled from a pad 19,800 m out against
one incoming raid, swept by the raid's speed and by how far off its track the pad
sits. Share of 60 trials in which the interceptor removes the raid, asked of
`UnitDied`'s killer field and not of whether the airframe disappeared: every
target here is one-way and kills itself the instant it strikes, and the first
version of this counted "no longer alive" and reported 100% in every cell.

With a Radar Mast radiating behind the defence:

| target | on track | 900 m off | 1,800 m off | 3,000 m off |
|---|---|---|---|---|
| Multirole Quad, 28 m/s | 40% | 40% | 40% | 40% |
| Loitering Munition, 36 m/s | 53% | 53% | 53% | 53% |
| Mid-Range Striker, 45 m/s | 53% | 53% | 53% | 53% |
| Heavy Strike Drone, 50 m/s | 53% | 53% | 53% | 53% |
| Jet Strike Drone, 140 m/s | **25%** | **25%** | **25%** | **0%** |

With no radar at all, every one of the first four rows reads 40% in every cell,
and the jet reads 16%, 16%, 0%, 0%.

**Interception in this game is not a spectrum.** Against anything under about
50 m/s an 85 m/s interceptor closes by pursuit whatever it was told and from
wherever it started, so the lead table is worth nothing and the pad's position is
worth nothing: those rows move between the arms and not across them, and the 13
points between 40% and 53% is the *cue* multiplier at the merge rather than the
vectoring. Item 36 built the vectoring and called it the missing system. It is
built, it is correct, and **it pays against exactly one airframe in the
catalogue.**

**The Multirole Quad is the exception and it is a signature, not a speed.** Its
radar cross-section is 26 against the Heavy Strike Drone's 52, and the mast does
not hold it at the merge at all, so both of its arms read 40%. A radar mast is
not air defence for small drones. It is air defence for big ones.

**The jet row is the experiment.** Across, the offset columns fall to nothing: a
pad off the raid's track has to make the lateral distance up out of a closing
budget it does not have. Down, the two arms differ by about a third at every
offset the interceptor reaches at all — a radar track flies the whole computed
lead and scores the merge at 1.00, an optical one flies 0.55 and scores at 0.60,
and against 140 m/s the missing 45% of the lead is most of a kilometre of
aimpoint.

And a design fact the sweep surfaced rather than measured: **the roster has
nothing between 50 m/s and 140 m/s.** The mechanic that makes detection quality
pay has no gradient to work over, because there is no airframe in the middle of
it.

### The speed term, off its clamp at last

Item 40 says this experiment would be "the first experiment in which
`AirHitChance`'s speed term is off its clamp". **It would not have been**, and
the reason is two sections earlier in item 40 itself: `CombatSystem` branches to
`ResolveInterception` before `AirHitChance` is reached, so an interceptor never
scores an air hit roll at all. The repair is a third arm — the same five
airframes flown at an **Autocannon Mount**, which is not an interceptor and
therefore does score that roll.

| target | mount kills it | speed term |
|---|---|---|
| Multirole Quad, 28 m/s | 98% | on the clamp |
| Loitering Munition, 36 m/s | 96% | on the clamp |
| Mid-Range Striker, 45 m/s | 98% | live |
| Heavy Strike Drone, 50 m/s | 100% | live |
| Jet Strike Drone, 140 m/s | **46%** | live |

`firing-solution-speed` is now a claimed mutation that moves an experiment, so
item 40's fifth finding is discharged. Two things it says:

**Item 15's sentence now has an experiment behind it, and the effect is large.**
*"A turbojet strike drone crossing at three times the speed of a quadcopter is
not hard to shoot at, it is hard to hit, and the speed term does that on its
own."* Measured: 100% against the 50 m/s airframe and 46% against the 140 m/s
one — same mount, same geometry, same approach.

**The clamp boundary itself is invisible.** The term is clamped under 39 m/s, and
the two rungs chosen to sit either side of that line — 36 and 45 m/s — read 96%
and 98%, which is noise at 60 trials. Only the jet is far enough past the clamp
for the term to do anything, so the boundary is not a place where the game
changes and nobody should tune against it.

### Fiber: the decision is not only where you launch, it is what you drag the cable over

`fiber`. One Fiber FPV Team — the first this harness has ever spawned — flown at
a structure 21,600 m out from a pad the sweep moves back in 3,000 m steps, with a
Relay Mast 600 m behind the pad as the launch site, against a Main Tank
patrolling a lane across the approach. Each trial starts the tank at a different
point on its lane: discovery is pure geometry with no roll in it (item 39 kept it
that way on purpose), so the honest trial variable is where the traffic was when
you launched.

The experiment was written to sweep standoff. The variable that turned out to
matter is the one it was not written to sweep.

**(a) flown down the road — the route every other experiment in this file stages on**

| pad at | out from target | parts after | thread found | site held, fiber | site held, radio |
|---|---|---|---|---|---|
| 18,600 m | 3,000 m | 1,373 m | 35% | 100% | 100% |
| 15,600 m | 6,000 m | 1,550 m | 5% | 100% | 100% |
| 12,600 m | 9,000 m | 1,568 m | 0% | 0% | 0% |
| 9,600 m | 12,000 m | 1,568 m | 0% | 0% | 0% |
| 6,600 m | 15,000 m | 1,568 m | 0% | 0% | 0% |
| 3,600 m | 18,000 m | 1,568 m | 0% | 0% | 0% |

**(b) the same sortie over open ground, 1,700 m north of the road**

| pad at | out from target | parts after | thread found | site held, fiber | site held, radio |
|---|---|---|---|---|---|
| 18,600 m | 3,000 m | 2,787 m | 95% | 100% | 100% |
| 15,600 m | 6,000 m | 5,786 m | 100% | 100% | 100% |
| 12,600 m | 9,000 m | 8,636 m | 100% | **100%** | **0%** |
| 9,600 m | 12,000 m | 11,786 m | 100% | **100%** | **0%** |
| 6,600 m | 15,000 m | 14,785 m | 100% | **100%** | **0%** |
| 3,600 m | 18,000 m | 16,803 m | 20% | 20% | 0% |

**A filament dragged down a road parts after about 1,500 m whatever the airframe
is carrying.** `Terrain.SnagRatePerSecond` is 1.5% per real second on a road and
zero on open ground; at 22 m/s that is a thread with an expected life of a
kilometre and a half. So the **16,800 m spool in the catalogue is not the leash**
on the route this harness flies — the thread is gone at a tenth of it, and the
number on the unit's card is one the player will never see spent. It is spent
exactly once in this table: the bottom row of the open arm, where 18,000 m of
flight on 16,800 m of thread parts the line 1,200 m short and four fifths of the
trials never get a cable across the lane at all.

**That changes what fiber's third liability is.** A thread that parts 1,500 m
from the pad can only be found by traffic within 1,500 m of the pad, so on the
road arm the found column collapses as soon as the pad is further back, and the
cable is a liability only from a pad already standing in the enemy's lap. On the
open arm the thread survives and the found column reads 95–100% across a
twelve-kilometre sweep of standoff.

**Half of item 39 holds and the sweep says so.** Its hand measurement — seven
threads found from the forward pad, seven from the rear — is reproduced across
six standoffs rather than two runs: how often a cable is found is not a function
of where you launched.

**The other half is refined rather than contradicted.** Item 39 concluded that
from a rear pad the found cable "is a log line", because the revealed site was
out of the defence's reach. The control column is what isolates that: the same
sortie flown by a radio FPV Team drags no thread, so where the two "site held"
columns agree the filament disclosed nothing the defence's own sensors had not.
They agree out to about 6,600 m and part past about 9,600 m — the edge of the
Radar Mast's passive listening. **Those three rows are the price of the thread
with everything else divided out: from a pad the enemy could not otherwise find
at all, one vehicle driving over a cable hands him the whole launch site.** So a
rear pad is not automatically safe. It is safe on a road, where the cable never
reaches him; over open ground it is the *worst* place to launch from, because the
thread is found just as often and there was nothing else to give the site away.

### What was excused, what is recorded as debt, and what is still uncovered

Two entries added to `tools/experiment-ceiling-allow.txt`, both matched by value
so they stop applying the day the numbers move: the radar experiment's parked
column (`control`), and the shape `100,100,0,0,0,0`, which covers three of
fiber's columns at once (`structural` — a launch site is held when the defence's
passive listening reaches it and not when it does not, and no rung choice moves a
sensor reach).

**Three of fiber's columns warn and are left warning rather than excused.** They
are pinned because the underlying mechanics are hard gates — a thread is found or
it is not, a site is inside a sensor's reach or it is not — but "the mechanic is a
gate" is an argument, and this project's rule is that an allowlist entry is for a
ceiling *measured* not to move with the rungs. That measurement has not been made
for these three, so they stay visible.

Every one of the fourteen mutations in the catalogue is now claimed by at least
one experiment. That is not a clean bill of health and the tool says so in its own
header: the catalogue is written by hand, so it can only ask about systems
somebody thought to name, and an unmutated constant is invisible to it.

### The general form

Item 30: a green suite proves nothing about the paths it does not walk. Item 32:
a saturated experiment proves nothing about the variables it cannot move. Item
40: an experiment proves nothing about a system it never instantiates, and there
is no way to tell which those are by looking at it. This is the fourth, and it is
about what to do with the first three:

**A claim about what an experiment measures is worth exactly as much as the
mechanism that can falsify it.** Item 40's table was right and it was a
paragraph, and a paragraph goes stale the first time somebody renames a constant.
The same table as a guard costs four lines per mutation and fails on the day the
claim stops being true. Every conclusion in this entry is one `./build.sh
mutations` away from being checked, which is the only reason any of it should be
believed.
