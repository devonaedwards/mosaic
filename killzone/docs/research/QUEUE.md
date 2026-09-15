# Research queue — briefs to run somewhere with a working network

KILL ZONE is a **video game**: a real-time strategy game in the Command & Conquer
tradition, set in a fictional near-future Eastern Europe, built for iPhone first
and for the desktop browser and iPad as well. It has hit points, build costs,
cooldown timers, a materiel economy and a crew roster. Nothing in this repository
is operational guidance, and nothing requested below is either. Every question
here exists because a number in a game balance table is currently a guess, and a
guess that nobody can check is a number nobody can tune.

This document exists because **this environment's network policy denies outbound
HTTPS to everything except a package-registry allowlist** (see `BRIEF.md` for the
verification). WebSearch works; page retrieval does not. These briefs are written
to be carried to an environment where a research agent can actually read sources,
run one at a time, and come back as a markdown document dropped into
`docs/research/`.

---

## The preamble every agent must be given, verbatim

Paste this above whichever brief you are running. It is not boilerplate — a prior
session was flagged by a classifier for asking these questions without it, and the
framing is also what keeps the answers useful, because a game wants
*recognisable and tunable*, not *complete*.

> You are researching for **KILL ZONE**, a real-time strategy **video game** about
> drone warfare, set in a fictional near-future Eastern Europe. It is a game in the
> Command & Conquer tradition: units have hit points, build costs and cooldowns, and
> the player builds a base and sends drones at an opposing base.
>
> Your output is a markdown design document that will be read by a game designer
> deciding what number to put in a balance table. It is **not** operational
> guidance, and you should not write it as though it were: no recommendations, no
> tactics, no procedures. What the game needs is the shape of a relationship — what
> gets bigger when what else gets bigger, and roughly by how much — so that a
> player who knows the subject recognises the game's behaviour as plausible.
>
> Use only published, open-source reporting: trade press, academic papers,
> manufacturer specifications, think-tank analysis, defence journalism. Where
> figures vary between sources, give the range and say which sources disagree.
>
> **Say when you do not know.** A marked gap is worth more than a confident
> invention, because the game's convention is that every content number cites the
> research document and section it came from, and a number citing a fabrication is
> worse than a number citing nothing. If your answer rests on a search-result
> snippet rather than an article you actually read, mark it as snippet-derived.
>
> Finish with a section titled **"What a game should do with this"** giving
> concrete, abstracted mechanics — not a simulation, a mechanic — and a section
> titled **"What I could not find"**.

---

## A. Terrain occlusion, per channel — the largest structural absence

**Why the game needs it.** There is currently no line of sight anywhere in the
simulation (`AUDIT-UNWIRED.md` F1). Every sensor sees through every hill. This is
the single biggest thing the detection model is missing, and it is also the thing
that would most change how the map plays, because it turns terrain from decoration
into cover.

**What the game does now.** Five detection channels — radar, thermal, optical,
acoustic, passive RF — each a range check with modifiers, none of them asking
whether anything is in the way. `docs/research/terrain.md` exists but stops short
of per-channel occlusion figures.

**Questions.**
1. For each of radar, thermal/optical, acoustic and passive RF: how much does
   terrain between emitter and sensor actually cost? Which of these are hard
   binary blocks and which are graded losses?
2. Acoustic specifically: diffraction over a ridge is frequency-dependent, so a
   multirotor's buzz and a fixed-wing's drone should be blocked differently. Is
   there a usable rule of thumb?
3. Passive RF: how much does terrain attenuate a control-link emission that a
   direction-finder is trying to hear? A prior session overstated that terrain
   barely affects passive RF and was corrected; the correction was not sourced.
4. Control link: is terrain masking a binary drop, and how much elevation does a
   relay need to restore it? (The game already has a relay mast and this is what
   it would be buying.)
5. Vegetation, urban clutter and built structures as distinct from landform.
6. **The design question, and the most useful thing you could answer:** is there a
   cheap abstraction that captures most of this? The game runs at 32 Hz on a
   phone and a per-pair per-channel ray test may not fit the tick budget at all. A
   coarser model that gets the *behaviour* right is an acceptable and possibly
   correct answer.

**Where it lands.** `docs/research/terrain.md` §occlusion, then AUDIT F1.

---

## B. Evasion, and what a drone does when it knows it is being hunted

**Why the game needs it.** The game has no evasion at all: a target flies its
order and nothing it does depends on being shot at. That absence is why a second
interceptor is currently worth exactly `1−(1−p)²` and nothing more, and it is why
"bracketing" — covering an adversary's *choices* rather than measurement error —
could not be built honestly (`FINDINGS 36`). Evasion is the missing branch.

**What the game does now.** `IsPilotedOnLiveFeed` already separates airframes a
person is flying from autonomous ones that cannot react. The catalogue already
prices turn rate: a jet strike drone turns at 22°/s, an FPV at 180°/s. Nothing
reads either for evasion.

**Questions.**
1. How does an aircraft or drone learn it is being engaged? Warning receivers,
   the operator seeing the interceptor on the feed, ground control calling it,
   or simply not learning at all. Which classes of drone have which.
2. What does the reaction look like — a break turn, a dive, a change of route, a
   descent to terrain? What does each cost in time, fuel, endurance and arrival
   accuracy?
3. **How much does evading actually help?** This is the number the game wants: the
   change in probability of being intercepted, however roughly bracketed.
4. Does an autonomous airframe evade at all, and if so on what cue?
5. Speed against agility: a fast aircraft that cannot turn tightly versus a slow
   one that can pivot. Which survives interception better, and does the answer
   flip with the interceptor's own speed?
6. How does an interceptor's guidance cope with a target that breaks — does it
   re-solve, or does the break defeat the solution outright?

**Where it lands.** A new `docs/research/evasion.md`; then a model gated on
`IsPilotedOnLiveFeed`, and the honest version of bracketing.

---

## C. Terminal interception: proximity fuzing and miss distance

**Why the game needs it.** Interception currently resolves as a die roll at the
merge (`InterceptBaseChance` × cue and speed multipliers). The better model is
geometric: the interceptor flies at a computed meeting point and whether it kills
depends on how close it actually gets — horseshoes, with a lethal radius. That was
declined on cost (`FINDINGS 36`) but it is the right model and it wants grounding
before it is built rather than after.

**Questions.**
1. Lethal radius against a small airframe, for the plausible interceptor families:
   a small fragmentation warhead, a proximity-fuzed round, a kinetic hit-to-kill
   drone, and a net or entangling interceptor. The working assumption in
   conversation has been a couple of metres — is that the right order of magnitude,
   and how does it vary by target size?
2. Proximity fuzing on small interceptors: what triggers it, at what radius, and
   how reliably against a target the size of a quadcopter?
3. What miss distances are actually achieved, and what drives them — seeker
   quality, closing speed, target manoeuvre, or the quality of the track the
   interceptor was vectored on?
4. Fragment versus blast against a light airframe: does a near miss that does not
   destroy typically still end the sortie?
5. Cost per interception attempt across the layers — a drone interceptor, a gun,
   a missile, electronic means — and the exchange ratio each achieves against a
   cheap attacking airframe. The game's current play-test spends 8,700 materiel
   for 11 kills against 200-materiel airframes, and nothing anchors whether that
   ratio is plausible or absurd.

**Where it lands.** `docs/research/point-defence.md` §terminal, then the
miss-distance model replacing `RandomStream.Interception`.

---

## D. What a passive optical track actually gives you

**Why the game needs it.** `MovementSystem.InterceptPoint` flies a fraction of the
computed lead depending on what the track is made of: **1.00** on radar,
**0.55** on optical, **0.00** with no track. The 0.55 is a marked designer
estimate and it is load-bearing — it is the difference between the radar mast
being worth building and being decoration.

**Questions.**
1. A passive EO/IR track gives bearing. Under what conditions does it give range,
   and how? Stereo baselines, known target size, a laser rangefinder paired to the
   optic, motion parallax from a moving sensor.
2. Can range *rate* be derived from a passive optical track well enough to lead a
   fast crossing target, and how much worse is the resulting solution than a radar
   track?
3. How much does a laser rangefinder on the optic close that gap, and is that a
   distinct thing the game should let a player buy?
4. Track fusion: two optical sensors with a known baseline versus one radar — is
   that a real substitution?
5. How long does a passive track stay good after the sensor loses the target, and
   what makes it decay? (The game has a track-hold concept and would like it
   grounded.)

**Where it lands.** `docs/research/thermal-optical.md` §tracking, then
`SimConstants.InterceptLeadOpticalTrack`.

---

## E. Thermal ground clutter

**Why the game needs it.** `thermal-optical.md` itself says ground clutter is the
largest factor in the thermal model, and the simulation has no clutter term at all
(AUDIT F11). A thermal sensor that ignores what it is looking *at* will make
terrain choice meaningless on the channel where it should matter most.

**Questions.**
1. How does background thermal clutter change detection range against a small
   airframe? Give the shape — what multiplies what.
2. Which backgrounds are worst and best: open field, forest canopy, urban
   rooftops, water, snow, bare rock.
3. Time of day and season: the diurnal crossover where background and target reach
   the same apparent temperature, and how long it lasts.
4. Looking down at a ground target against terrain versus looking up at an
   airframe against sky — how different are these, and by how much?
5. Does clutter affect *detection* and *tracking* differently?

**Where it lands.** `docs/research/thermal-optical.md` §clutter, then AUDIT F11.

---

## F. The economy: costs, salvage, and what a sortie is worth

**Why the game needs it.** The game's economy is the constraint that actually
binds (`FINDINGS 36`: money, not crews), and most of it is a guess.
`World.cs` marks two economic constants as designer estimates with no figure in
`economics.md` behind them; salvage is never collected and there is no income at
all (AUDIT F3); AUDIT F32 is the cost ratios.

**Questions.**
1. Relative cost, not absolute: what does a cheap attack drone cost against a
   reconnaissance airframe, a jet-type strike drone, an interceptor, a radar, an
   electronic-warfare post, a relay mast? Ratios survive inflation and currency;
   absolute figures do not.
2. What fraction of a system's cost is the airframe versus the seeker, the datalink
   and the warhead? This decides whether the game's upgrades should be modular.
3. Production tempo: what does it take to build these at rate, and what actually
   limits it — components, optics, motors, people?
4. Is anything recovered from a wreck, by either side, and is that a real economy
   or a curiosity? (The game has a salvage system wired to nothing.)
5. What does an operator crew cost relative to an airframe, and how long does one
   take to train? The game rations crews as a second currency and has no anchor
   for their scarcity.
6. Reusable airframes: how many sorties does one actually fly before it is lost,
   and what is the maintenance burden between them?

**Where it lands.** `docs/research/economics.md`, then AUDIT F3 and F32.

---

## G. The decoy signature scale — a documented contradiction to settle

**Why the game needs it.** Two of the game's own research documents use different
offsets for identical physics: `radar-rf.md` works in a scale where a signature is
`2·RCS_dBsm + 80` and `decoys-masking.md` uses `+60`. A prior session read one
document's scale into the other's table and changed a unit's number by twenty
decibels — the change *looked* like a fix, which is why it nearly stood. The
scales need reconciling from a source rather than by picking one.

**Questions.**
1. Radar cross-section of an inflatable or corner-reflector decoy against the real
   vehicle it imitates, in dBsm. How close do they actually get?
2. Which channels does a cheap decoy fail on? The game's position is that a decoy
   convincing on radar is obvious on thermal and acoustic because it has no engine
   and makes no noise — is that right, and what closes the gap (heaters, emitters)?
3. Corner reflectors and radar-amplifying decoys: how much can a small, cheap
   object inflate its apparent radar return?
4. RCS in dBsm for the classes the game carries: a small quadcopter, a fixed-wing
   strike drone, a jet-type drone, a ground vehicle, a truck. Enough to sanity-check
   the whole ordering rather than one row.
5. Thermal and acoustic decoys as distinct things, and whether they are used.
6. Camouflage nets, thermal blankets and cage armour: what each does to which
   channel, as a multiplier. (The game has blankets and cages as data that cannot
   be fitted to anything — AUDIT F16 — and the blanket is currently a flat 40% on
   everything, which is certainly wrong.)

**Where it lands.** Reconciles `radar-rf.md` and `decoys-masking.md`, then AUDIT
F16, F29 and the signature table.

---

## H. Autonomy: the operator ratio, the error rates, and reference imagery

**Why the game needs it.** The game's central trade is that autonomy buys you
freedom from a jammable link and costs you accuracy and crew. Both halves of that
price are estimates. And a measured play-test found the **entire autonomous threat
axis is deleted** by navigation denial working correctly — an autonomous drone over
ground the attacker holds no reference imagery for never gets a scene-matching lock
and always misses (`FINDINGS 36`). That is mechanically correct and empty as
content, and the fix is imagery coverage, which nothing grounds.

**Questions.**
1. Operator-to-airframe ratio: how many airframes can one crew realistically run
   at each autonomy level, from hand-flown through waypoint to terminal-guidance to
   target-selecting?
2. Error rates for autonomous target selection: misidentification, engaging the
   wrong thing, engaging nothing. Even an order of magnitude helps.
3. Scene-matching and terrain-referenced navigation: what reference imagery does it
   need, how current must it be, how is it obtained, and how much ground can a side
   plausibly hold imagery for? **This is the one that unblocks the deep-strike axis.**
4. How accurate is a scene-matched arrival compared with satellite navigation, and
   compared with dead reckoning?
5. What actually degrades an inertial or dead-reckoned solution over distance? The
   game uses 3% of distance travelled as its drift and it is sourced but thin.
6. Where does a human stay in the loop and where do they not, and what is the stated
   reason each way?

**Where it lands.** `docs/research/autonomy.md` and `navigation-denied.md`, then
AUDIT F17, F30 and the imagery coverage in the scenario.

---

## I. Delegation, reporting and span of control

**Why the game needs it.** The design intent is that a player commands one sector
directly while a larger battle runs around them, and that they can hand a sector to
a subordinate. `SECTOR-COMMAND.md` argues against a *learned* delegate — it breaks
determinism and is illegible — in favour of explicitly authored doctrine. Doctrine
needs a vocabulary, and the game should borrow one rather than invent one.

**Questions.**
1. **How delegation actually works.** Mission command and commander's intent as
   practised: what is specified, what is deliberately left open, and what a
   subordinate is expected to decide alone. This is a large, well-documented open
   literature.
2. **What drone units specifically delegate**, which may be very different from the
   doctrinal answer. The question is what crews are told and what they choose.
3. **Reporting.** What a commander learns about a sector they are not in, how late
   it arrives, and how wrong it is. The gap between what happened and what gets
   reported is a *mechanic*, not a nuisance — it is fog of war for your own side.
4. **Span of control.** How many sectors one commander plausibly holds. This sets
   how many the player can have, which is a concrete interface number.
5. What vocabulary does a delegated order actually use — the verbs. The game wants
   to borrow the words, not paraphrase them.

**Where it lands.** `docs/SECTOR-COMMAND.md`, which currently lists these four as
explicitly unresearched.

---

## J. Campaign tempo, and what losing looks like

**Why the game needs it.** `FINDINGS 35` found you cannot lose, and `FINDINGS 36`
found that a player who over-spends gets a 195-second dead stretch because they
bankrupted themselves. Both are pacing questions and the game has no anchor for
tempo at any scale above a single sortie.

**Questions.**
1. Sortie rate: how many sorties does one drone unit fly in a day, and what limits
   it — airframes, crews, weather, targets, or the enemy?
2. Attrition: what fraction of airframes are lost per sortie by type, and how does
   that change against a defended target?
3. How long does a contested sector stay contested? What does the tempo of a drone
   campaign look like over weeks, and what does a decisive local outcome look like?
4. What ends a local engagement? A position becomes untenable for reasons a game
   would model as a *condition*, not as hit points reaching zero — this is exactly
   the losing condition the game lacks.
5. Reconstitution: how fast does a unit that has been badly hurt come back?

**Where it lands.** `docs/research/front-2026.md` and `economics.md`, then the
losing condition and the wave pacing in `Scenario.cs`.

---

## K. Weather, cold and endurance

**Why the game needs it.** AUDIT F26–F28: acoustic conditioning by weather is
missing, cold and icing are missing, and weather does not touch hit probability at
all while grounding the wrong aircraft.

**Questions.**
1. Acoustic propagation against weather: wind, temperature gradient, humidity,
   precipitation. Which dominates, and by how much?
2. Cold and icing: effect on endurance, on battery airframes versus fuel ones, and
   at what temperature each becomes a real constraint.
3. What weather actually grounds which class of airframe — the game currently
   grounds the wrong ones.
4. Does weather change *hit probability* separately from *detection*, and how?
5. Fog, rain and snow per channel: which of the five channels degrade together and
   which are independent. (The independence is the interesting part for a game —
   weather that kills every channel at once is just a pause.)

**Where it lands.** `docs/research/weather.md` and `acoustic.md`, then AUDIT
F26–F28.

---

## L. Point defence, after the gun mount

**Why the game needs it.** AUDIT F31 is the remainder of point defence once the gun
mount's traverse and magazine were fixed. `FINDINGS 34` also killed an earlier
conclusion here — "the gun mount fires once" survived a rescale as only half true —
so this area has a record of being reasoned about rather than measured.

**Questions.**
1. The layered counter-drone stack: what sits at each layer, at what range, and
   what does each layer cost per engagement?
2. Engagement rates: how many targets can one mount plausibly handle per minute,
   and what is the reload or cooling constraint?
3. Traverse and slew rates for the relevant mounts, and whether slewing is actually
   the binding constraint against a fast crosser.
4. Electronic defeat against kinetic defeat: relative effectiveness, relative cost,
   and what each fails against.
5. Saturation: at what arrival rate does a defence stop coping, and is the failure
   gradual or a cliff? The game has a saturation experiment and nothing anchoring it.

**Where it lands.** `docs/research/point-defence.md`, then AUDIT F31.

---

## How to hand the results back

Each returned document goes in `docs/research/` as markdown with numbered sections,
because the game's convention is that every content number in `Defs.cs` and
`SimConstants.cs` cites its research document **and section**. A document without
stable section numbers cannot be cited, so number the sections.

Then say, per document, which of the game's existing numbers it contradicts. That
is more valuable than what it confirms, and it is the thing a research pass is
most likely to leave implicit.
