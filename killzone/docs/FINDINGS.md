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
structure four times in five. The exchange rate is not more than ten to one
against the attacker; it is about four to three **in the attacker's favour**. Every
word of this entry's problem statement has evaporated: there is no wall, nothing
needs a suppression weapon to solve it, and the three "possible answers for
whoever owns balance" are answers to a problem that no longer exists. The open
question has reversed. A gun mount now has to be made worth its 450 Materiel.

The mechanism is finding 31, and it is not the range: **the mount gets one shot.**

Two smaller corrections to the tables above. The 16- and 24-drone rows read 14.8
arrived because the experiment stops counting when the gun dies, not because
anything capped the flight; above about twelve drones those rows measure the
harness, not the game. And **attacking at night is now worth almost nothing** -
three drones by day take the gun 80% of the time, three at night 100%, and from
five drones upward day and night are both 100%. Item 12 predicted that collapse
for the wrong reason (microphones) and it has arrived for a different one: by day
the gun is already losing.

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

Two things are worth separating. The first is that this table was measured
against the 550 m mount with infinite ammunition, and the honest mount is much
weaker, so the direction of the correction is unsurprising. The second is not:
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

The fourth turret stopped buying anything too. Of every conclusion in items 13
through 18, this is the one the corrected measurement agrees with, and the
explanation the entry gave is now measurable rather than asserted: a mount's
effective envelope really is far smaller than its nominal one, and the number is
**about 70 m** - the range at which it first holds a track on a quadcopter, as
against an 85 m barrel and a 127 m analytic sensor reach (finding 31).

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
without, and has read exactly that on every build in this pass and the one before
it. The experiment cannot distinguish an attack split across two altitudes from
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
to **7.4** of 8, and the five-drone saturation row goes from the gun always losing
to losing **88%** of the time. A gun that no longer drops a fringe track kills
about a third of a drone more per engagement.

That is a real effect and a small one, and it is swamped by the other three
changes in this pass, which all run the other way. The title stands - nothing is
a switch - and it did not stand when it was written.

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
85 m barrel or on the 550 m test mount, by day or at night — gets **one shot**.
Not five, not "however many fit into the crossing time". One. Measured by counting
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
  taken. Wiring it changed not one digit in any of the ten experiments — the whole
  harness output is byte-identical across the commit that added it.
- **The traverse rate binds hard**, because it is spent inside the only engagement
  there is. Turning it off hands 34 points of survival back to the gun (item 18).
- **Barrel length barely matters.** The reach sweep reads 7.4 of 8 arriving at
  550 m, at 450 m and at 350 m — identical — and 7.8 at 85 m. Twenty-five seconds
  of nominal exposure and 3.9 seconds of it produce almost the same result,
  because above about 70 m the barrel is not what stops the mount shooting.

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

## 32. Three of the ten balance experiments cannot detect a change to the simulation

Stacking, Vertical and Decoy Escort produced **byte-identical output on five
different builds** spanning the whole of this pass: with the gun's range
overwritten to 550 m and with the real 85 m; with a magazine and without; with the
track hold and without; with the twenty-number signature table applied and not; on
a flat, ownerless, uncovered map and on a mixed one with a border and imagery.
Every row of all three tables sits on a ceiling — 8 drones take any number of
turrets, 6 drones take any altitude split, every real drone gets through every
decoy mix — and a measurement pinned to a ceiling is a constant, not a result.

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
