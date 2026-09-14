# Ground robots, logistics and the infantry

Research note for KILL ZONE. Compiled 14 September 2026.

**Scope.** What carries supply through a drone-patrolled band, what it costs,
what proportion of it is now robotic, what happens to a position that loses its
supply, and what a strategy game has to model for any of this to feel real.

## How to read this document

Every load-bearing figure carries a marker:

- **[R] Reported** — a specific number appearing in press or institutional
  reporting, attributed to a named source.
- **[C] Claimed** — a number asserted by a party with an interest in it: a
  defence ministry, a vendor, a unit talking about its own performance. Often
  true, never independently checked.
- **[I] Inferred** — my arithmetic or reasoning on top of [R]/[C] figures. The
  assumptions are stated so you can reject them.
- **[U] Unestablished** — I looked and did not find it, or could not find it
  within this session's research budget. Do not fill these in from memory.

**A methodological caveat that matters.** This session's outbound network policy
blocked direct page fetches for every domain tried (War on the Rocks, MWI,
CEPA, Kyiv Post, mod.gov.ua, Euromaidan Press, Defense Express and others), and
the session's web-search budget was exhausted partway through. Everything below
comes from search-engine result summaries of those pages rather than from the
pages themselves. That is good enough to establish orders of magnitude and the
shape of the system; it is not good enough to quote anyone verbatim. Several
areas the brief asked about — motorcycle and quad loss rates, literal tunnels,
decoy movement, Russian UGV procurement volumes — came back thin or empty and
are marked [U] rather than guessed at. A follow-up pass with working page
fetches should close those.

One internal consistency check does give confidence in the Ukrainian mission
numbers: the monthly figures, the "66,300 since the start of the year" total
published on 31 July, and the ~112,000 total published in early September all
reconcile to within 1%. Three separately-published numbers that add up are
unlikely to be invented.

---

## Summary

1. **The band is much deeper than "the last kilometres."** The contested strip
   is 20–25 km on each side of the line on the eastern front, with a corps
   commander expecting 30 km by end of 2026 [R]. Units now need their logistics
   delivered 15–20 km forward, and infantry walk more than 15 km to reach
   positions on active sectors [R].

2. **Trucks do not die at the line; they die well behind it.** Russian Ural and
   KamAZ trucks have been struck at depths of 30–50 km, which has produced
   outright bans on convoy movement in some sectors [R]. The truck-death line is
   *outside* the drone band, not inside it.

3. **Robotic logistics has gone from marginal to routine in nine months.**
   Ukrainian UGV missions recorded in the DELTA system: 7,511 in January 2026
   rising to over 25,000 in August, ~112,000 cumulative by early September, a
   3.3-fold monthly increase across the year [C, Ukrainian MoD].

4. **Procurement matches.** Ukraine's MoD planned 25,000 UGVs for the first half
   of 2026, roughly double all of 2025; over 22,000 had been contracted by July
   [C/R]. Unit costs cluster at $8,000–$20,000 [R].

5. **The proportion of forward logistics carried by robots is contested.**
   Credible reporting says "up to 80% in some units" [C]; a market-research blog
   asserts ~90% of all Ukrainian military logistics activity [C, weak source];
   the MoD's stated *goal* is 100% of frontline logistics [C]. My reading: a
   high share of the *most dangerous last leg* in units that have the robots,
   and a much lower share of total tonnage. Treat 80–90% as aspiration reported
   as achievement.

6. **Robots are consumed, not operated.** A UGV survives roughly 7–10 trips
   before being hit [R, two sources giving 7–8 and 9–10]. One brigade reported
   losing 2–5 robots per day [R]. That is an 11–13% loss per mission [I] — a
   running cost, not a catastrophe, because the vehicle costs about as much as
   two quadcopters.

7. **Fibre-optic control is explicitly *not* for logistics.** Ukraine tested
   seven fibre-controlled UGV models through Brave1, and the intended use is
   kamikaze and high-risk combat runs, not resupply [R]. Spools run 5–20 km,
   with 10 km the 2025 workhorse [R]. The cable is a one-way consumable; a
   supply robot has to come back.

8. **Protected corridors are now real infrastructure.** Ukraine built 822 km of
   anti-drone netting over frontline roads between January and June 2026 [C,
   Defence Minister], at a pace that rose from 5 km/day to a 20 km/day target,
   with 4,000 km more planned [C]. Russia builds the same thing, including a
   2 km tunnel on the Bakhmut–Chasiv Yar road [R]. Drone operators are
   sceptical of how well nets actually work [R].

9. **Casualty evacuation is the binding constraint on what infantry will risk.**
   Evacuation from a forward position can take days, weeks, occasionally months
   [R]; one documented case ran 12 days and ended in amputation for gangrene
   [R]; tourniquets are reported left on for seven hours to days against a
   two-hour standard [R]. Evacuation waits for the sky to clear, not for the
   casualty's condition.

10. **Infantry has reorganised around all of this.** Movement in twos and
    threes, positions dug far apart and rarely left, rotations extended
    indefinitely — the extreme documented case is two soldiers holding a
    position near Orikhiv for 165 days across thirty failed relief attempts [R].

---

## 1. The geometry: where the band starts and where trucks stop

| Boundary | Distance from line of contact | Confidence |
|---|---|---|
| FPV / fibre FPV saturation (the "kill zone" proper) | 15 km on the most active sectors; 20–25 km typical on the eastern front; 30 km projected by end-2026 | [R] multiple, incl. Ukrainian drone-forces commander and 7th Airborne Assault Corps commander |
| Depth at which logistics becomes "difficult" | up to 20 km | [R] |
| Distance at which units now need delivery | 15–20 km forward | [R] |
| Distance infantry walk to position on active sectors | >15 km | [R] |
| Depth at which Russian Ural/KamAZ trucks have been hit | 30–50 km | [R] |

The important structural point for the game: there are **three** zones, not two.

- **Beyond ~50 km**: conventional road logistics, degraded by deep-strike drones
  and missiles but still recognisably trucks on roads.
- **~15–50 km**: the interdiction band. Trucks *can* move but are hunted. This is
  where the netting, the timing discipline, the convoy bans and the dispersal
  live. Losses here are episodic rather than certain.
- **Inside ~15 km**: the kill zone. Wheeled vehicles with drivers are a losing
  proposition on any repeatable schedule. This is robot and aerial-drone
  country, with men on foot as the fallback.

## 2. Ukrainian UGV volume and economics

**Missions recorded in DELTA, 2026** [C, Ukrainian MoD, published monthly]:

| Month | Missions | Change |
|---|---|---|
| January | 7,511 | baseline |
| February | 7,960 | +6% |
| March | 9,072 | +14% |
| April | 11,028 | +22% |
| May | 14,059 | +27% |
| June | 16,664 | +19% |
| July | 19,969 | +20% |
| August | >25,000 | +25% |
| **Cumulative to early Sept** | **~112,000** | 3.3× monthly rate since January |

Derived figures, all [I]:

- August rate ≈ 830 missions/day, roughly one every 1.8 minutes across the whole
  front. Over the year to 20 August the average was about one every 3.5 minutes,
  which is the figure that appears in commentary.
- Across a ~1,200 km front that is about **0.7 robot missions per kilometre of
  front per day** — but it will be wildly concentrated on a handful of active
  sectors, so 5–10 per km/day on a hot sector is the more useful design number.
- If a typical load is 150–300 kg, August's missions moved on the order of
  **3,750–7,500 tonnes** [I, assumption-heavy — the mission count includes
  evacuation and empty return legs, so treat as an upper bound].

**Procurement** [C/R]: 25,000 UGVs planned for H1 2026, over 22,000 contracted
by July, against a 2025 total the MoD describes as roughly half that — so
~12,000 or fewer in 2025 [I].

**Attrition economics** [R]: at ₴600,000–700,000 ($15,800–18,400) per unit and
7–10 surviving trips, the amortised cost is ₴75,000–87,000 ($2,000–2,300) per
mission — described in reporting as about the price of one quadcopter. A brigade
losing 2–5 robots per day is therefore running something like **14–50 UGV
missions per day** [I, from the 7–10 trip survival figure].

One institutional claim worth flagging: a study by the Central Research
Institute of the Armed Forces of Ukraine reports UGVs have cut Ukrainian
casualties by 30% [C]. Plausible mechanism, unverifiable magnitude.

## 3. UGV types

| Platform | Side | Payload | Range / endurance | Cost | Primary role | Conf. |
|---|---|---|---|---|---|---|
| TerMIT (Tencore), tracked | UA | 300 kg | 20–40 km, up to 12 h (v2.0) | ~$12,000 | Logistics, casevac, combat; modular | [R] |
| Bizon-L | UA | 300 kg | 50 km | [U] | Logistics; NATO-codified, MoD-authorised | [R]/[C] |
| Murakha / Muraha | UA | 500 kg | 50 km (electric) | [U] | Heavy sustained logistics | [C] |
| Zmiy | UA | 500 kg | [U] | [U] | Silent electric logistics; codified late 2024 | [C] |
| NUMO, tracked | UA | [U] | [U] | [U] | Multi-purpose | [C] |
| Spider, tracked | UA | n/a | [U] | [U] | Fire support; LTE / radio / Starlink / fibre control | [R] |
| Generic UA logistics UGV | UA | 150–300 kg typical | 10–50 km | $8,000–$20,000 | Resupply, casevac | [R] |
| Kuryer / Courier | RU | 200 kg (more with trailer) | [U] | [U] | Logistics; also thermobaric Shmel module, Bagulnik-82 82 mm mortar, 107 mm Type 75 rockets | [R, Janes/ArmyRecognition] |
| Depesha (Rostec) | RU | 150 kg | [U] | [U] | Fuel, ammunition, provisions, casevac | [C, vendor] |
| Impuls | RU | [U] | [U] | [U] | Logistics/combat | [C] |

Notes:

- **Industrial base.** Tencore announced a joint venture with Germany's Quantum
  Systems (Quantum Tencore Industries) to build 2,000 TerMITs in Germany for
  Ukraine [R]. Ukrainian UGV production is no longer purely domestic or
  improvised.
- **The Russian fleet is smaller.** Reporting assesses Russian UGV numbers at
  the front as lower than Ukraine's, with communications limitations the named
  constraint [C, Ukrainian-sourced analysis — treat with appropriate suspicion,
  but it is consistent with the Kuryer appearing mainly in single-vehicle
  propaganda clips rather than in mission-count statistics].
- **The asymmetry in evidence is itself a finding.** Ukraine publishes monthly
  mission counts from a combat-management system. Russia publishes videos. Any
  comparison of the two sides' robot logistics is comparing an accounting system
  against a press office.

## 4. What UGVs are actually used for, in order of volume

The Ukrainian MoD reports its UGV missions as a single combined category —
"logistics and evacuation" — so the internal split is **[U]**. Ordering below is
my reading of which roles dominate, with the evidence for each.

1. **Resupply.** Ammunition, food, water, batteries. The dominant use by a wide
   margin; it is the reason the mission count is a logistics statistic [R].
2. **Casualty evacuation.** Bundled with logistics in the official count, so it
   is clearly a large minority. Often the same vehicle: supply forward, casualty
   back [R].
3. **Mine laying and clearing, obstacle emplacement.** Explicitly listed as a
   growing role [R].
4. **Carrying electronic warfare and surveillance kit into position.** A robot
   that emplaces a jammer or a camera on a route and leaves it there [R]. This
   is a logistics mission in form and a combat mission in effect.
5. **One-way attack.** UGVs loaded with anti-tank mines or bulk charges driven
   into trench lines and treeline bunkers to blow gaps [R]. This is what
   fibre-optic control is actually for.
6. **Direct fire.** Real but rarer: the Russian Kuryer with a thermobaric
   launcher or an 82 mm automatic mortar module [R]; Ukrainian armed UGVs
   including a documented case where a single strike UGV of the NC13 Strike UGV
   Company, 3rd Army Corps, held a position against repeated Russian assaults
   for 45 days with no Ukrainian human losses [C — a unit talking about itself,
   but specific enough to be checkable].
7. **Demining as a deliberate engineering task** (as opposed to breaching) is
   the smallest documented category in what I found [U on volume].

## 5. Failure modes and how often robots are lost

| Failure mode | Evidence | Conf. |
|---|---|---|
| Killed by FPV / drone-dropped munition | Implicit in the 7–10 trip survival figure; the whole point of the kill zone | [I] from [R] |
| Loss of control link (jamming, terrain masking, range) | Named as the reason fibre is being tested; named as the constraint on Russian UGV use; radio links degrade badly in rough terrain because they need line of sight | [R] |
| Terrain — mud, shell craters, ditches, treelines | Strongly implied by the choice of tracked platforms (TerMIT, NUMO, Spider, Kuryer) over wheeled ones for the forward leg | [I] |
| Mines and UXO on the route | The route forward is by definition a route through a mined belt | [I], specific rates [U] |
| Mechanical failure / battery exhaustion | Endurance quoted as up to 12 h; "simple enough to fix in the field" is a stated design goal | [R] |

**Loss rate.** Two independent figures: a UGV survives 7–8 trips before being
hit, and separately, 9–10 missions on average [R]. That brackets a per-mission
loss probability of **10–14%**, call it 1 in 8 [I]. A brigade losing 2–5 robots
a day [R] is consistent.

For design purposes this is the single most useful number in the document: a
supply robot is not a unit that survives and accumulates value. It is a
consumable with an expected life of about eight round trips.

## 6. Control links for ground robots

| Link | Range | Jam-resistant | Used for | Conf. |
|---|---|---|---|---|
| Analogue/digital radio | Line-of-sight limited; degrades badly in rough terrain | No | The bulk of logistics runs | [R] |
| LTE (commercial cell) | Where coverage survives | Partially | Listed as an option on Spider | [R] |
| Starlink | Long, needs sky view and power | Partially | Listed as an option on Spider; Starlink terminals are themselves a delivered cargo item | [R] |
| Fibre-optic tether | Spool 5–20 km; 10 km the 2025 workhorse, 15 km less common, 20 km in testing | Yes — unjammable | **Kamikaze and high-risk combat, explicitly not logistics** | [R] |
| Airborne repeater drone | — | No | Widely discussed for extending UGV control; I could not confirm scale of use | [U] |

The fibre finding is the one most likely to be got wrong by a game designer,
because it seems obvious that an unjammable link should be the premium option
for every robot. It is not. Ukraine tested seven fibre UGV models through Brave1
and directed them at one-way missions [R]. The reasons are structural: the cable
is consumed, it does not survive the return leg, it snags on the same terrain
that a supply robot has to cross repeatedly, and the spool mass competes with
payload. By February 2026 over 80 Ukrainian fibre-optic systems (drones and
UGVs together) had been approved for use [R]. Defence Express published a piece
titled "Valuable Pros and Unexpected Cons of Ukraine's First Fiber-Optic UGVs"
which I could not read — the cons are worth chasing in a follow-up.

## 7. The last kilometres when robots are not available

Supply methods, with what I can and cannot substantiate:

| Method | Typical leg | Load per trip | Practical throughput | Loss rate per trip | Conf. |
|---|---|---|---|---|---|
| Truck / KamAZ / Ural convoy | 30–50+ km rear | tonnes | High | Losses documented at 30–50 km depth; convoy movement banned in some Russian sectors | [R] on the depth, [U] on rate |
| Truck or pickup on a netted corridor | Through the 15–25 km band | 0.5–2 t | High while the net holds | [U] — nets exist at scale; effectiveness disputed by drone operators | [R]/[U] |
| Pickup / buggy dash, unprotected road | 5–15 km | ~0.5 t | Low; one-time or nightly | [U] | [U] |
| Motorcycle / quad | 3–15 km | 50–150 kg | Low per trip, fast turnaround | [U] — named repeatedly as a method, never with a loss figure I could source | [R] on use, [U] on rate |
| **UGV** | **10–20 km each way** | **150–500 kg** | **~1 mission per vehicle per night** | **10–14% per mission** | **[R]** |
| Heavy multirotor (Vampire/"Baba Yaga") | tens of km, up to 60 km reach; flies at night | ~15 kg | Many sorties, small each | [U] | [R] on payload/reach |
| Men carrying | >15 km on foot on active sectors | 20–40 kg | Very low | [U] | [R] on distance |
| Tunnels / underground routes | — | — | — | — | [U] — asked for, nothing found this session |

Three things this table says that a designer should take seriously:

- **There is no clean substitute for the robot.** The alternatives are either
  much smaller (a 15 kg drone sortie against a 300 kg robot run — twenty sorties
  to match one robot), much slower (men on foot), or much more expensive in
  lives (a driver in a pickup).
- **The heavy aerial drone is a *supplement*, not a replacement.** Reporting is
  blunt that a 15 kg Vampire load is "little more than a middling stream of
  small arms ammunition, sausages, energy drinks and cigarettes" [R]. It keeps a
  position alive; it does not let it fight a battle. Against that, at some
  zero-line positions heavy drones now deliver "almost all" food, ammunition and
  water [C] — because nothing else can get there at all.
- **Timing and darkness are the free counter-measure.** Vampires fly at night
  [R]. Robots run at night. The whole system's tempo is nocturnal, and that is
  the cheapest protective measure anyone has.

## 8. Counter-measures protecting logistics

**Netted road corridors** are the headline measure and are now at
infrastructure scale:

- Ukraine: 822 km built January–June 2026 [C, Defence Minister Fedorov], with a
  further 4,000 km planned by end of 2026 [C]. Build rate rose from ~5 km/day in
  January to 12 km/day in February against a 20 km/day March target [C].
- A single 40 km net tunnel on the Izium–Sloviansk road [R].
- Russia: a 2 km mesh tunnel on the Bakhmut–Chasiv Yar road [R]; makeshift mesh
  structures along the E50 supporting the Pokrovsk axis [R].
- Effectiveness is genuinely disputed: Ukrainian FPV operators have publicly
  dismissed Russia's anti-FPV tunnels [R, 2025]. Nets defeat a casual dive
  attack; they do not defeat a determined one, and they mark the route.

**Other measures**, with weaker evidence:

- **Dispersal and convoy bans.** Russian commanders have banned convoy movement
  in some sectors and require mobile fire-support groups to escort logistics
  columns [R].
- **Hiding the transfer point.** The 93rd Brigade around Druzhkivka is described
  running ammunition, food and water to robot-resupply teams hidden under trees
  [R]. The robot marshalling area is itself a target and has to be concealed and
  moved.
- **Night movement and timing** — universal, treated as assumed rather than
  reported.
- **Camouflage, decoy movement, false routes** — [U]. Asked for, not found in
  this session's results. Do not invent numbers for these.

## 9. What a cut-off position actually experiences

This is the part the sources describe most vividly and quantify least. What can
be established:

**It is degradation, not collapse.** The mechanism is stated directly:
trench sections that cannot receive water, batteries, ammunition or anti-tank
weapons "lose combat power before being physically overrun" [R]. A position
does not die when it is cut off. It stops being able to shoot back, stops being
able to see, and is then taken.

**The consumption list has changed.** Before drones, a cut-off position needed
water, food and ammunition. Now it also needs **batteries**, **electronic
warfare kit**, and **construction materials** for continuous fortification [R].
Batteries are the new water: without them there is no EW, no comms, no night
vision, no drone of one's own. A position with ammunition and no batteries is
blind.

**Positions are further apart and deeper underground.** Fighters "bury
themselves in the ground and rarely leave unless absolutely necessary" [R], and
positions are established much farther apart than before [R]. The consequence is
that a cut-off position is also an *isolated* position — the neighbours cannot
easily reinforce it, and often cannot see it.

**How long can a position hold unsupplied?** I found no figure for this and it
is the most important [U] in the document. What exists instead are bounding
anecdotes:

- Two soldiers near Orikhiv held a position for **165 days without rotation**,
  across thirty attempted reliefs [R]. They were not unsupplied for 165 days —
  supply is easier than relief, because supply can be robotic and relief cannot
  — but it establishes that "indefinitely, in misery" is a real outcome.
- A single armed UGV held a position against repeated assaults for **45 days**
  [C].
- Casualty evacuation delays of **days to weeks, occasionally months** [R] set
  the timescale on which an isolated position's problems compound.

My inference, clearly labelled: the binding constraint on an isolated position
is **water and batteries in days, ammunition in a firefight in hours, and morale
and casualties on a timescale of weeks** [I]. A position that is
comprehensively cut off — no robot, no drone drop, no night runner — is
combat-ineffective in something like 3–7 days and untenable in 2–3 weeks. That
is a designer's number, not a researched one. Flag it as such in the code.

## 10. Casualty evacuation under observation

The single most consequential change to infantry behaviour, and the best-evidenced
human cost in the sources:

- Getting a casualty out of a forward position can take **days, weeks, and
  sometimes months**, especially if they cannot walk [R].
- One documented case: **12 days** from front line to forward field unit; by
  arrival the wounded man had gangrene and lost the leg [R].
- Tourniquets are reported left in place for **seven hours, sometimes days**,
  against a two-hour clinical standard [R]. That is the mechanism by which delay
  converts survivable wounds into amputations.
- Russian forces are reported to hit **all routes leading to casualty collection
  points** while observing them from a distance [R]. The casualty collection
  point is a known, fixed, targetable node.
- The operative rule, stated by a Ukrainian medic: nobody moves toward a
  position under drone observation, because nobody will risk additional lives.
  Evacuation happens "when the sky has cleared" [R].
- With a ground robot available, a wounded man may wait **hours** rather than
  days [R]. That is the whole argument for UGV casevac in one comparison.
- A quadcopter with a **400 kg** payload intended for casualty evacuation is
  contracted for Ukraine, with production to begin in October 2026 [C, vendor
  and press announcement — not yet an operational capability].

**What this does to what infantry can risk.** If a casualty cannot be evacuated
for days, then every action is priced against the possibility of producing one.
The rational behaviours that follow — and which reporting describes — are:
do not counter-attack to recover ground you can hold by staying still; do not
move by day; do not concentrate; do not leave the hole. The evacuation system,
not the enemy's fire, sets the aggression level of the infantry.

## 11. Infantry organisation now

| Dimension | Before drone saturation | Now | Conf. |
|---|---|---|---|
| Movement group size | Section/platoon | **Twos and threes** | [R] |
| Position spacing | Mutually supporting | Much farther apart, isolated, deeply dug | [R] |
| Rotation length | Days to weeks | Extended indefinitely; documented extreme of **165 days** with 30 failed relief attempts | [R] |
| Relief mechanism | Vehicle-borne, at night | On foot, >15 km, or not at all | [R] |
| Armour's role | Manoeuvre and fire support | Stays hidden | [R] |

Rotation deserves emphasis because it is the thing games get most wrong.
**Relief is harder than resupply.** A robot can carry 300 kg of ammunition
forward through the kill zone at a 1-in-8 risk of losing the robot. It cannot
carry four fresh men forward and four exhausted men back at a 1-in-8 risk of
losing the men. So supply scales with robots and relief does not, and the gap
between the two is exactly why two men spent 165 days in a hole that was being
resupplied.

The counterweight, stated bluntly in commentary: drones and robots cannot
replace infantry [C, Atlantic Council]. Ground still has to be occupied by
people, and the people occupying it are now fewer, more static, more isolated,
and held in place longer than any previous system would have tolerated.

---

## 12. What a strategy game has to model

### Is "trucks die inside the band, robots and bikes carry the last kilometres" right?

**Broadly yes, with four corrections that change the gameplay.**

**Correction 1 — the band is far deeper than "the last kilometres," and the
robot leg is long.** The evidence puts delivery requirements at 15–20 km forward
and the kill zone at 20–25 km and growing. The robot leg is not a 1 km dash from
a treeline; it is a 10–20 km one-way run taking hours, mostly at night. Scaled
at the game's twelve-to-one compression, that is a robot journey of roughly
800–1,700 map metres — long enough that it must be modelled as a *journey with
exposure along its whole length*, not as a transfer between adjacent tiles. If
the game currently treats robot delivery as a short final hop, the interesting
decisions disappear.

**Correction 2 — trucks die outside the band, not inside it.** Trucks are hit at
30–50 km depth. The correct rule is not "trucks cannot enter the drone band" but
"trucks cannot use predictable routes anywhere within deep-strike reach, and
cannot enter the band at all." That gives the player a *third* zone to manage —
the interdiction belt where trucks work but are hunted — and it is where the
netting, timing and dispersal decisions actually live. A two-zone model
(safe rear / lethal band) loses the most interesting layer.

**Correction 3 — the band is not uniformly lethal, because corridors are
buildable.** 822 km of netting built in six months, 4,000 km planned, at 5–20 km
per day, with a disputed but non-zero protective effect. This is the strongest
game mechanic in the whole research set: **protected routes as attritable,
constructible infrastructure**. The player builds netted corridors at a real
construction rate; corridors raise truck survival on a specific line; corridors
are visible, so they concentrate enemy attention; and the enemy can break them
faster than you build them. That is a proper tension, and it is historically
grounded rather than invented.

**Correction 4 — "bikes" is under-specified; the real portfolio has four legs.**
The sources support motorcycles and quads as a method but gave me no loss rate
for them. What the sources *do* support strongly is a four-way portfolio for the
last leg, each with a different shape:

| Leg | Mass per trip | Risk | Character in play |
|---|---|---|---|
| UGV | 150–500 kg | ~1 in 8 per trip, loses a cheap machine | The workhorse. Attrition is a budget line. |
| Heavy multirotor | ~15 kg | Loses a machine, no person | Keeps a position breathing. Twenty sorties to equal one robot. |
| Motorcycle / quad / pickup | 50–500 kg | Loses a person | Fast, high-variance, used when the robot is dead or the load is urgent. |
| Men on foot | 20–40 kg | Loses a person, very slow | The floor. Never zero, never enough. |

So: **yes, robots and bikes carry the last kilometres — but robots carry the
mass, drones carry the difference between a position living and dying, and men
on foot are the floor nothing can take away.** A game that models only robots
and bikes will not produce the characteristic situation of the war, which is a
position kept barely alive by 15 kg drone drops while everyone waits for the
weather or the jamming to change.

### The specific mechanics worth building

1. **Consumable robots, not units.** A supply UGV should have an expected life
   of about eight round trips (10–14% loss per mission) and cost roughly what
   two attack drones cost. The player should feel comfortable losing them and
   uncomfortable running out of them. Do not give them veterancy.

2. **Supply as a *rate* with a *variance*, not a binary.** A position is not
   "supplied" or "cut off." It has an inflow, and its stock of four separate
   things — ammunition, water/food, **batteries**, and fortification material —
   draws down at different rates. Batteries are the interesting one: run them
   out and the position loses its EW, its sensors and its own drones, which
   raises the loss rate on everything trying to reach it. That is a death
   spiral the player can see coming and fight.

3. **Casualty evacuation as reverse logistics that competes for the same
   vehicles.** Every casevac run is a supply run not made. Casualties that are
   not evacuated should degrade the position's combat power and the player's
   willingness to act — the historically correct behaviour is that units stop
   counter-attacking because they cannot get the wounded out, not because they
   cannot win.

4. **Relief is harder than resupply.** Rotation should be a separate, much
   riskier action than supply, with its own failure mode (the relief is turned
   back). Positions that cannot be relieved should accumulate a fatigue penalty
   without dying, so that the 165-day hole is a reachable, memorable game state
   rather than an impossible one.

5. **Fibre is for one-way jobs.** If the game offers fibre-optic control on
   ground robots, it should be the unjammable *assault and mining* option with a
   spool limit (5–20 km, so ~400–1,700 map metres) and no return trip — not a
   strictly-better logistics link. This is a real distinction in the reporting
   and it makes fibre a choice rather than an upgrade.

6. **Night as the master clock.** The tempo of the whole logistics system is
   nocturnal. If the game already halves optical detection at night, the correct
   consequence is that virtually all supply movement is scheduled into darkness,
   and the interesting counter-play is thermal and acoustic detection of the
   night runs — which is exactly the sensor model the brief describes.

7. **Degradation timescales.** Until better sources exist, suggested placeholder
   values, explicitly flagged in code as *designer estimates, not research*:
   combat-ineffective after 3–7 days fully cut off; untenable after 2–3 weeks;
   casualty evacuation delay of hours with a robot, days without one, weeks if
   the sector is under sustained observation.

### What is genuinely uncertain

- The **share of forward logistics carried by robots** (80–90% claimed; the
  underlying measurement is unclear and the claims come from interested
  parties).
- The **split between resupply and casevac** in the mission counts — reported as
  one combined category.
- **Loss rates for every non-robot method.** The 10–14% UGV figure has no
  counterpart for motorcycles, quads, pickups or foot parties. Any comparison
  the game makes between them is currently a designer's judgement.
- **How well nets actually work.** Built at enormous scale, dismissed by some
  operators. Both things are reported; they cannot both be fully true.
- **Russian volumes.** Individual Russian platforms are well documented; the
  fleet size and mission tempo are not, and the assessment that it is smaller
  comes from Ukrainian-sourced analysis.
- **How long a position holds unsupplied.** No figure found. The most important
  gap in the document.

---

## Sources

All URLs below appeared in search results during this research. **None could be
fetched directly** — the session's egress policy blocked every domain attempted
— so the content summarised above comes from search-engine summaries of these
pages. Verify before quoting.

**Ukrainian UGV volume, procurement and mission counts**
- https://mod.gov.ua/en/news/robots-at-work-defence-forces-of-ukraine-more-than-triple-their-use-of-ug-vs
- https://mod.gov.ua/en/news/ug-vs-have-performed-over-100-000-frontline-missions-in-place-of-ukrainian-military-personnel-since-the-start-of-the-year
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/09/ukraine-260907-ukraine-mod02.htm
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/07/ukraine-260731-ukraine-mod02.htm
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/07/ukraine-260720-ukraine-mod03.htm
- https://thedefensepost.com/2026/08/25/ukraine-ground-robots-missions/
- https://oboronka.mezha.ua/en/nazemni-roboti-utrichi-zbilshili-kilkist-misiy-314957/
- https://oboronka.mezha.ua/en/viyskovi-ponad-100-tis-misiy-z-vikoristannyam-nrk-314467/
- https://www.defensenews.com/unmanned/2026/04/24/ukraine-to-field-25000-ground-robots-in-push-to-replace-soldiers-for-frontline-logistics/
- https://militarnyi.com/en/news/ministry-of-defense-of-ukraine-25-000-ugvs/
- https://united24media.com/world/ukraines-ground-robots-are-becoming-battlefield-platforms-and-procurement-is-about-to-surge-19106
- https://interestingengineering.com/military/ukraine-plans-25000-unmanned-ground-vehicles
- https://www.jpost.com/defense-and-tech/article-893608

**UGV economics, attrition and analysis**
- https://warontherocks.com/ukraines-ground-robots-and-the-economics-of-survival/
- https://mwi.westpoint.edu/networked-for-war-lessons-from-ukraines-ground-robots/
- https://epik.eu/publication/robots-take-risk/
- https://foreignpolicy.com/2026/04/13/russia-ukraine-war-drones-ground-robots-ugvs/
- https://dsm.forecastinternational.com/2026/06/09/how-ukraines-ground-robots-are-rewriting-the-rules-of-war/
- https://meta-defense.fr/en/2026/08/05/ukraine-ugv-ground-robots-operational-reality/
- https://english.nv.ua/nation/ukraine-s-unmanned-ground-vehicles-are-reshaping-the-war-50573852.html
- https://asiatimes.com/2026/07/ukraines-robot-army-advancing-on-the-wars-front-lines/
- https://www.cnn.com/2026/05/30/europe/ukraine-robots-drones-russia-war-intl
- https://en.defence-ua.com/events/ugvs_in_ukrainian_service_how_they_overcome_kill_zone_and_change_rules_of_war-17932.html
- https://www.rebiogroup.com/insights/ugv-logistics-last-tactical-mile-scale
- https://www.nextmsc.com/blogs/unmanned-ground-vehicles-battlefield-robotics-reshape-2026 (market-research blog; the "90% of logistics" claim originates here — weak)

**Specific platforms**
- https://www.battlepolicy.com/termit/
- https://mod.gov.ua/en/news/bizon-enters-the-battlefield-defence-forces-receive-a-ugv-capable-of-carrying-up-to-300-kg
- https://militaeraktuell.at/en/ukraine-opts-for-new-ugv-beast-of-burden-bizon-l/
- https://www.strategybattles.net/2026/04/25/ukraine-25000-ground-robots-2026/
- https://militarnyi.com/en/news/ukraine-spider-ugv-fire-support-system/
- https://www.armyrecognition.com/news/army-news/2026/ukraine-uses-ground-robots-and-drones-to-capture-russian-positions-without-troop-losses

**Russian UGVs**
- https://www.janes.com/defence-intelligence-insights/defence-news/land/russia-shows-more-applications-of-kuryer-ugv
- https://www.armyrecognition.com/news/army-news/2026/russia-expands-courier-ugv-roles-from-thermobaric-strikes-to-bagulnik-82-robotic-mortar-fire-support
- https://armyrecognition.com/news/army-news/2025/russia-fields-courier-ugv-armed-with-shmel-thermobaric-rocket-module-on-the-ukraine-front
- https://oboronka.mezha.ua/en/rosteh-prezentuvav-novi-versiji-nrk-depesha-312594/
- https://oboronka.mezha.ua/en/rosiyski-nrk-310870/
- https://defence-blog.com/robot-gunner-joins-russian-assault-units/

**Fibre-optic and other control links**
- https://www.twz.com/land/unmanned-ground-vehicles-controlled-via-fiber-optic-cables-being-tested-by-ukraine
- https://en.defence-ua.com/weapon_and_tech/valuable_pros_and_unexpected_cons_of_ukraines_first_fiber_optic_ugvs-14048.html
- https://militarnyi.com/en/news/ukraine-tests-fiber-optic-fpv-drones-with-a-range-of-20-km/
- https://www.sedi-ati.com/disposable-fiber-optic-spools-for-tethered-vehicles/disposable-fiber-optic-spool-for-unmanned-ground-vehicles-ugv/

**Kill zone depth and geometry**
- https://www.pravda.com.ua/eng/news/2026/05/14/8034645/
- https://euromaidanpress.com/2026/07/03/ukraines-eastern-kill-zone-is-25-km-deep-corps-commander-expects-30-by-years-end/
- https://newsukraine.rbc.ua/news/ukraine-s-frontline-kill-zone-expands-to-1786615823.html
- https://www.kyivpost.com/post/81206
- https://nationalsecurityjournal.org/the-ukraine-war-has-created-a-drone-kill-zone-that-no-solider-wants-to-get-close-to/
- https://dronexl.co/2026/02/23/kill-zone-ukraine-frontline-drone/

**Anti-drone netting and protected corridors**
- https://euromaidanpress.com/2026/06/06/ukraine-has-built-822-kilometers-of-anti-drone-road-tunnels-each-kilometer-means-safer-evacuations-and-faster-supply/
- https://euromaidanpress.com/2026/02/26/from-5-km-to-20-km-a-day-ukraine-triples-anti-drone-tunnel-construction-speed-in-two-months/
- https://united24media.com/latest-news/ukraine-plans-4000-km-anti-drone-net-corridors-to-protect-frontline-logistics-routes-16275
- https://united24media.com/latest-news/russia-builds-anti-drone-tunnel-roads-to-shield-troop-advance-in-eastern-ukraine-8096
- https://www.twz.com/news-features/russians-erect-mesh-net-tunnel-over-a-mile-long-to-counter-ukrainian-fpv-drones
- https://www.npr.org/2026/03/17/nx-s1-5743446/russia-ukraine-war-nets-drones
- https://www.forbes.com/sites/davidhambling/2025/02/17/ukrainian-drone-pilots-unimpressed-by-russias-anti-fpv-tunnel/
- https://nationalinterest.org/feature/the-next-evolution-ukraines-drone-defense

**Casualty evacuation**
- https://euromaidanpress.com/2026/05/19/wounded-warriors-can-wait-months-for-evacuation-ukrainians-are-trying-to-solve-this-heres-how/
- https://kyivindependent.com/under-russian-drone-onslaught-ukraines-critically-wounded-face-slim-survival-chance/
- https://www.npr.org/2026/05/29/nx-s1-5830382/drones-are-changing-the-face-of-warfare-including-battlefield-medicine
- https://americanhomefront.wunc.org/news/2026-05-12/as-attack-drones-grow-more-common-in-warfare-its-harder-for-medics-to-treat-battlefield-injuries
- https://aviationweek.com/defense/aircraft-propulsion/casualty-evacuation-drone-headed-ukraine
- https://pubmed.ncbi.nlm.nih.gov/40575542/ (qualitative assessment of point-of-injury to Role 2+ care in Ukraine; not read)

**Infantry organisation and rotation**
- https://euroradio.fm/en/kill-zones-extended-rotations-and-robot-support-what-war-ukraine-looks-now
- https://www.atlanticcouncil.org/blogs/ukrainealert/ukraines-robot-army-will-be-crucial-in-2026-but-drones-cant-replace-infantry/
- https://cepa.org/article/ukraines-drone-war-comes-down-to-earth/

**Aerial resupply**
- https://cepa.org/article/frontline-drones-doing-the-infantrys-heavy-lifting/
- https://ukrainesarmsmonitor.substack.com/p/drone-warfare-in-ukraine-baba-yaga-ae7
- https://militaeraktuell.at/en/baba-yaga-ukraines-dreaded-attack-drone/
- https://offbeatresearch.com/2025/10/supplying-the-frontline-ukraines-drone-logistics/
- https://newsukraine.rbc.ua/news/silent-savior-police-present-vampire-drone-1750876919.html

**Interdiction of the other side's logistics**
- https://edition.cnn.com/2026/06/20/europe/ukraine-mid-range-drones-russia-logistics-intl-cmd
- https://acleddata.com/report/ukraines-drone-campaign-dismantling-russias-military-supply-network-occupied-south
- https://www.forbes.com/sites/vikrammittal/2026/05/14/ukrainian-drones-are-modernizing-siege-warfare/
- https://www.forbes.com/sites/vikrammittal/2026/04/19/ukrainian-drones-are-cutting-off-ammo-resupply-to-russian-artillery/
- https://www.forbes.com/sites/davidkirichenko/2026/05/12/ukraines-ai-drones-are-hunting-russian-supply-lines/
