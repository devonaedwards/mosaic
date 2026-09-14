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

## 21. Nothing is a switch

Detection was binary: in range or not. It now has a solid band inside about two
thirds of a sensor's reach and an intermittent one beyond, with per-channel
reliability - passive radio listening 95, optical 88, thermal 84, radar 78,
acoustic 52 - and a two-second track hold so a marginal contact does not strobe.

Radar was also rescaled to the fourth root of cross-section rather than the
square root, which is how the radar equation actually behaves and is why a drone
the size of a dinner plate is so much harder than an aircraft.

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
