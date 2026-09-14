# Ground robots, logistics and the infantry

Research note for KILL ZONE. Compiled 14 September 2026.

**Scope.** What carries supply through a drone-patrolled band, what it costs,
what proportion of it is now robotic, what happens to a position that loses its
supply, and what a strategy game has to model for any of this to feel real.

## How to read this document

- **[R] Reported** — a specific figure in press or institutional reporting,
  attributed to a named source.
- **[C] Claimed** — asserted by an interested party: a defence ministry, a
  vendor, a unit describing itself. Often true, never independently checked.
- **[I] Inferred** — my arithmetic on top of [R]/[C] figures; assumptions stated.
- **[U] Unestablished** — looked for, not found. Do not fill in from memory.

**Caveat.** This session's egress policy blocked direct page fetches for every
domain tried (War on the Rocks, MWI, CEPA, Kyiv Post, mod.gov.ua, Euromaidan
Press, Defense Express), and the search budget ran out partway. Everything below
comes from search-result summaries, not from the pages. Good enough for orders
of magnitude; not good enough to quote. Motorcycle and quad loss rates, tunnels,
decoy movement and Russian procurement volumes came back empty and are marked
[U] rather than guessed at.

One consistency check supports the Ukrainian mission numbers: the monthly
figures, the "66,300 year-to-date" total published on 31 July and the ~112,000
published in early September reconcile to within 1%.

---

## Summary

1. **The band is deeper than "the last kilometres."** 20–25 km each side of the
   line on the eastern front, with a corps commander expecting 30 km by end-2026
   [R]. Units need delivery 15–20 km forward; infantry walk over 15 km to reach
   positions on active sectors [R].

2. **Trucks die outside the band, not inside it.** Russian Ural and KamAZ trucks
   have been struck at 30–50 km depth, producing outright bans on convoy
   movement in some sectors [R].

3. **Robotic logistics went from marginal to routine in nine months.** Ukrainian
   UGV missions in DELTA rose from 7,511 in January 2026 to over 25,000 in
   August, ~112,000 cumulative by early September [C, Ukrainian MoD].

4. **Procurement matches.** 25,000 UGVs planned for H1 2026, roughly double all
   of 2025; over 22,000 contracted by July [C/R]. Unit costs cluster at
   $8,000–$20,000 [R].

5. **The robotic share of forward logistics is contested.** "Up to 80% in some
   units" [C]; ~90% of all logistics per a market-research blog [C, weak]; the
   MoD's *goal* is 100% [C]. My reading: a high share of the most dangerous last
   leg where robots exist, a much lower share of total tonnage.

6. **Robots are consumed, not operated.** A UGV survives 7–10 trips before being
   hit [R, two sources]; one brigade reported losing 2–5 per day [R]. That is
   10–14% loss per mission [I] — a running cost, not a catastrophe, at about the
   price of two quadcopters.

7. **Fibre-optic control is explicitly *not* for logistics.** Seven fibre UGV
   models were tested through Brave1 and pointed at kamikaze and high-risk
   combat runs [R]. Spools run 5–20 km, 10 km the 2025 workhorse [R]. The cable
   is a one-way consumable; a supply robot has to come back.

8. **Protected corridors are real infrastructure.** Ukraine built 822 km of
   anti-drone netting over frontline roads January–June 2026 [C], at a rate
   rising from 5 to a targeted 20 km/day, with 4,000 km more planned [C]. Russia
   does the same [R]. Drone operators are sceptical that nets work [R].

9. **Casualty evacuation binds what infantry will risk.** Evacuation can take
   days, weeks, occasionally months [R]; one case ran 12 days and ended in
   amputation for gangrene [R]; tourniquets are left on seven hours to days
   against a two-hour standard [R]. Evacuation waits for the sky to clear.

10. **Infantry has reorganised around it.** Movement in twos and threes,
    positions dug far apart and rarely left, rotations extended indefinitely —
    the documented extreme is two soldiers holding a position near Orikhiv for
    165 days across thirty failed relief attempts [R].

---

## 1. The geometry: where the band starts and where trucks stop

| Boundary | Distance from line of contact | Confidence |
|---|---|---|
| FPV / fibre FPV saturation (the "kill zone" proper) | 15 km on the most active sectors; 20–25 km typical on the eastern front; 30 km projected by end-2026 | [R] multiple, incl. Ukrainian drone-forces commander and 7th Airborne Assault Corps commander |
| Depth at which logistics becomes "difficult" | up to 20 km | [R] |
| Distance at which units now need delivery | 15–20 km forward | [R] |
| Distance infantry walk to position on active sectors | >15 km | [R] |
| Depth at which Russian Ural/KamAZ trucks have been hit | 30–50 km | [R] |

The structural point for the game: there are **three** zones, not two.

- **Beyond ~50 km** — conventional road logistics, degraded by deep strike but
  still trucks on roads.
- **~15–50 km** — the interdiction band. Trucks move but are hunted; losses are
  episodic rather than certain. This is where netting, timing, convoy bans and
  dispersal live.
- **Inside ~15 km** — the kill zone. Driven vehicles lose on any repeatable
  schedule. Robot and aerial-drone country, with men on foot as the fallback.

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

Derived figures, all [I]: August's rate is ~830 missions/day, one every 1.8
minutes across the whole front. Spread over ~1,200 km that is **0.7 missions per
kilometre of front per day** — but it concentrates heavily, so 5–10 per km/day on
a hot sector is the more useful design number. At 150–300 kg a load, August moved
on the order of **3,750–7,500 tonnes** (upper bound: the count includes
evacuation and return legs).

**Procurement** [C/R]: 25,000 planned for H1 2026, over 22,000 contracted by
July, against a 2025 total the MoD calls roughly half that — ~12,000 or fewer
[I].

**Attrition economics** [R]: at ₴600,000–700,000 ($15,800–18,400) per unit and
7–10 surviving trips, the amortised cost is ₴75,000–87,000 ($2,000–2,300) per
mission, about the price of one quadcopter. A brigade losing 2–5 robots a day is
therefore running roughly **14–50 UGV missions a day** [I].

A study by the Central Research Institute of the Armed Forces of Ukraine reports
UGVs have cut Ukrainian casualties by 30% [C] — plausible mechanism,
unverifiable magnitude.

## 3. UGV types

| Platform | Side | Payload | Range / endurance | Cost | Primary role | Conf. |
|---|---|---|---|---|---|---|
| TerMIT (Tencore), tracked | UA | 300 kg | 20–40 km, to 12 h (v2.0) | ~$12,000 | Logistics, casevac, combat | [R] |
| Bizon-L | UA | 300 kg | 50 km | [U] | Logistics; NATO-codified | [R]/[C] |
| Murakha / Muraha | UA | 500 kg | 50 km (electric) | [U] | Heavy sustained logistics | [C] |
| Zmiy | UA | 500 kg | [U] | [U] | Silent electric logistics, codified 2024 | [C] |
| NUMO, tracked | UA | [U] | [U] | [U] | Multi-purpose | [C] |
| Spider, tracked | UA | n/a | [U] | [U] | Fire support; LTE/radio/Starlink/fibre | [R] |
| Generic UA logistics UGV | UA | 150–300 kg typical | 10–50 km | $8,000–$20,000 | Resupply, casevac | [R] |
| Kuryer / Courier | RU | 200 kg, more with trailer | [U] | [U] | Logistics; also Shmel thermobaric, Bagulnik-82 82 mm mortar, 107 mm rockets | [R, Janes] |
| Depesha (Rostec) | RU | 150 kg | [U] | [U] | Fuel, ammunition, provisions, casevac | [C, vendor] |
| Impuls | RU | [U] | [U] | [U] | Logistics/combat | [C] |

Notes. Tencore has a joint venture with Germany's Quantum Systems to build
2,000 TerMITs in Germany for Ukraine [R] — production is no longer purely
domestic or improvised. Russian fleet numbers are assessed as lower than
Ukraine's, with communications limits the named constraint [C, Ukrainian-sourced].
The asymmetry in *evidence* is itself a finding: Ukraine publishes monthly
mission counts out of a combat-management system, Russia publishes videos.
Comparing the two sides' robot logistics compares an accounting system against a
press office.

## 4. What UGVs are actually used for, in order of volume

The MoD reports missions as one combined category, "logistics and evacuation",
so the internal split is **[U]**. The ordering below is my reading, with the
evidence for each.

1. **Resupply** — ammunition, food, water, batteries. Dominant by a wide margin;
   it is why the mission count is published as a logistics statistic [R].
2. **Casualty evacuation** — bundled into the same official category, so clearly
   a large minority. Often the same vehicle: supply forward, casualty back [R].
3. **Mine laying and clearing, obstacle emplacement** — explicitly listed as a
   growing role [R].
4. **Emplacing electronic warfare and surveillance kit** — a logistics mission in
   form, a combat mission in effect [R].
5. **One-way attack** — UGVs loaded with anti-tank mines or bulk charges driven
   into trench lines and treeline bunkers to blow gaps [R]. This is what fibre
   control is for.
6. **Direct fire** — real but rarer: the Russian Kuryer with a thermobaric
   launcher or an 82 mm automatic mortar module [R]; on the Ukrainian side a
   single strike UGV of the NC13 Strike UGV Company, 3rd Army Corps, reportedly
   held a position through repeated assaults for 45 days with no human losses [C].
7. **Deliberate demining** (as opposed to breaching) is the smallest documented
   category [U on volume].

## 5. Failure modes and how often robots are lost

| Failure mode | Evidence | Conf. |
|---|---|---|
| Killed by FPV or drone-dropped munition | Implicit in the 7–10 trip survival figure | [I] from [R] |
| Control link lost (jamming, terrain masking, range) | The stated reason fibre is being tested, and the stated constraint on Russian UGV use; radio needs line of sight and degrades in rough ground | [R] |
| Terrain — mud, craters, ditches, treelines | Implied by the preference for tracked platforms (TerMIT, NUMO, Spider, Kuryer) on the forward leg | [I] |
| Mines and UXO on the route | The route forward crosses a mined belt by definition | [I]; rates [U] |
| Mechanical failure, battery exhaustion | Endurance up to 12 h; "simple enough to fix in the field" is a stated design goal | [R] |

**Loss rate.** Two independent figures — 7–8 trips before being hit, and 9–10
missions on average [R] — bracket a per-mission loss probability of **10–14%**,
call it 1 in 8 [I]. A brigade losing 2–5 robots a day [R] is consistent.

This is the most useful number in the document. A supply robot is not a unit
that survives and accumulates value; it is a consumable with an expected life of
about eight round trips.

## 6. Control links for ground robots

| Link | Range | Jam-resistant | Used for | Conf. |
|---|---|---|---|---|
| Radio | Line-of-sight limited; degrades in rough terrain | No | The bulk of logistics runs | [R] |
| LTE | Where coverage survives | Partly | An option on Spider | [R] |
| Starlink | Long; needs sky view and power | Partly | An option on Spider; terminals are themselves delivered cargo | [R] |
| Fibre tether | Spool 5–20 km; 10 km the 2025 workhorse, 20 km in testing | Yes, unjammable | **Kamikaze and high-risk combat, explicitly not logistics** | [R] |
| Airborne repeater | — | No | Discussed for extending UGV control; scale unconfirmed | [U] |

The fibre finding is the one a designer is most likely to get wrong, because an
unjammable link looks like a strict upgrade. It is not. Ukraine tested seven
fibre UGV models through Brave1 and pointed them at one-way missions [R]. The
reasons are structural: the cable is consumed, does not survive a return leg,
snags on the terrain a supply robot must cross repeatedly, and the spool mass
competes with payload. Over 80 Ukrainian fibre systems (drones and UGVs
together) were approved by February 2026 [R]. Defence Express published
"Valuable Pros and Unexpected Cons of Ukraine's First Fiber-Optic UGVs", which I
could not read — worth chasing.

## 7. The last kilometres when robots are not available

Supply methods, with what can and cannot be substantiated:

| Method | Typical leg | Load per trip | Practical throughput | Loss rate per trip | Conf. |
|---|---|---|---|---|---|
| Truck / KamAZ / Ural convoy | 30–50+ km rear | tonnes | High | Hit at 30–50 km depth; convoys banned in some Russian sectors | [R] depth, [U] rate |
| Truck or pickup on a netted corridor | Through the 15–25 km band | 0.5–2 t | High while the net holds | [U]; nets exist at scale, effectiveness disputed | [R]/[U] |
| Pickup / buggy dash, unprotected road | 5–15 km | ~0.5 t | Low; one-time or nightly | [U] | [U] |
| Motorcycle / quad | 3–15 km | 50–150 kg | Low per trip, fast turnaround | [U] — named repeatedly as a method, never with a loss figure I could source | [R] on use, [U] on rate |
| **UGV** | **10–20 km each way** | **150–500 kg** | **~1 mission per vehicle per night** | **10–14% per mission** | **[R]** |
| Heavy multirotor (Vampire/"Baba Yaga") | tens of km, up to 60 km reach; flies at night | ~15 kg | Many sorties, small each | [U] | [R] on payload/reach |
| Men carrying | >15 km on foot on active sectors | 20–40 kg | Very low | [U] | [R] on distance |
| Tunnels / underground routes | — | — | — | — | [U] — asked for, nothing found this session |

Three points a designer should take from this table:

- **There is no clean substitute for the robot.** Every alternative is either
  much smaller (twenty 15 kg drone sorties to match one 300 kg robot run), much
  slower (men on foot), or paid for in lives (a driver in a pickup).
- **The heavy aerial drone supplements, it does not replace.** A 15 kg Vampire
  load is described as "little more than a middling stream of small arms
  ammunition, sausages, energy drinks and cigarettes" [R]. It keeps a position
  alive; it does not let it fight. Yet at some zero-line positions heavy drones
  now deliver almost all food, ammunition and water [C] — nothing else reaches.
- **Darkness is the free counter-measure.** Vampires fly at night [R]; robots run
  at night. The system's tempo is nocturnal.

## 8. Counter-measures protecting logistics

**Netted road corridors** are the headline measure and are now at
infrastructure scale:

- Ukraine: 822 km built January–June 2026 [C, Defence Minister Fedorov], 4,000 km
  more planned by end-2026 [C]. Build rate rose from ~5 km/day in January to
  12 km/day in February against a 20 km/day March target [C].
- A single 40 km net tunnel on the Izium–Sloviansk road [R].
- Russia: a 2 km mesh tunnel on the Bakhmut–Chasiv Yar road [R]; makeshift mesh
  along the E50 supporting the Pokrovsk axis [R].
- Effectiveness is disputed — Ukrainian FPV operators have publicly dismissed
  Russia's anti-FPV tunnels [R, 2025]. Nets defeat a casual dive attack, not a
  determined one, and they mark the route.

**Other measures**, weaker evidence: dispersal and outright convoy bans, with
mobile fire-support groups escorting Russian logistics columns [R]; concealment
of the transfer point, as when the 93rd Brigade around Druzhkivka runs supplies
to robot-resupply teams hidden under trees [R] — the marshalling area is itself a
target and has to keep moving; night movement, universal and assumed rather than
reported. Camouflage, decoy movement and false routes: [U], asked for and not
found. Do not invent numbers for these.

## 9. What a cut-off position actually experiences

The sources describe this most vividly and quantify it least.

**It is degradation, not collapse.** Trench sections that cannot receive water,
batteries, ammunition or anti-tank weapons "lose combat power before being
physically overrun" [R]. A cut-off position stops being able to see and shoot,
and is then taken.

**The consumption list has changed.** It is no longer water, food and
ammunition. It is now also **batteries**, **EW kit** and **construction
material** for continuous fortification [R]. Batteries are the new water:
without them there is no EW, no comms, no night vision, no drone of one's own.

**Cut off also means isolated.** Fighters bury themselves in the ground and
rarely leave [R], and positions sit much farther apart than before [R], so
neighbours often cannot reinforce them or even see them.

**How long can a position hold unsupplied?** No figure found — the most important
[U] in the document. What exists is bounding anecdote: two soldiers near Orikhiv
held a position **165 days without rotation** across thirty attempted reliefs
[R] — the sources do not say they were unsupplied throughout, and supply is the
easier problem because supply can be robotic and relief cannot, but it shows
"indefinitely, in misery" is a real outcome; a single armed UGV held a
position through repeated assaults for **45 days** [C]; evacuation delays of days
to weeks [R] set the timescale on which an isolated position's problems compound.

My inference, clearly labelled: the binding constraints are **ammunition in a
firefight in hours, water and batteries in days, casualties and morale in
weeks** [I]. A comprehensively cut-off position — no robot, no drone drop, no
night runner — is combat-ineffective in roughly 3–7 days and untenable in 2–3
weeks. A designer's number, not a researched one; flag it as such in the code.

## 10. Casualty evacuation under observation

The most consequential change to infantry behaviour, and the best-evidenced
human cost in the sources:

- Getting a casualty out of a forward position can take **days, weeks, sometimes
  months**, especially if they cannot walk [R].
- One documented case: **12 days** to the forward field unit; by arrival the man
  had gangrene and lost the leg [R].
- Tourniquets are left on **seven hours, sometimes days**, against a two-hour
  standard [R] — the mechanism by which delay turns survivable wounds into
  amputations.
- Russian forces hit **all routes leading to casualty collection points** while
  observing them from a distance [R]. The collection point is a known, fixed,
  targetable node.
- The operative rule, from a Ukrainian medic: nobody moves toward a position
  under drone observation, because nobody will risk more lives. Evacuation
  happens "when the sky has cleared" [R].
- With a ground robot available, the wait is **hours** rather than days [R].
  That is the argument for UGV casevac in one comparison.
- A **400 kg** payload quadcopter for casualty evacuation is contracted for
  Ukraine, production from October 2026 [C — not yet an operational capability].

**What this does to what infantry can risk.** If a casualty cannot be evacuated
for days, every action is priced against the chance of producing one. The
behaviours reporting describes follow directly: do not counter-attack for ground
you can hold by staying still, do not move by day, do not concentrate, do not
leave the hole. The evacuation system, not enemy fire, sets the infantry's
aggression level.

## 11. Infantry organisation now

| Dimension | Before | Now | Conf. |
|---|---|---|---|
| Movement group size | Section/platoon | **Twos and threes** | [R] |
| Position spacing | Mutually supporting | Far apart, isolated, deeply dug | [R] |
| Rotation length | Days to weeks | Indefinite; documented extreme **165 days**, 30 failed reliefs | [R] |
| Relief mechanism | Vehicle-borne at night | On foot, >15 km, or not at all | [R] |
| Armour | Manoeuvre and fire support | Stays hidden | [R] |

Rotation is the thing games get most wrong. **Relief is harder than resupply.** A
robot can carry 300 kg forward at a 1-in-8 risk of losing the robot; it cannot
carry four fresh men forward and four exhausted men back at a 1-in-8 risk of
losing the men. Supply scales with robots and relief does not — which is exactly
why two men spent 165 days in a hole that was being resupplied.

The counterweight, stated bluntly in commentary: drones and robots cannot
replace infantry [C, Atlantic Council]. Ground is still occupied by people, and
those people are now fewer, more static, more isolated, and held in place longer
than any previous system would have tolerated.

---

## 12. What a strategy game has to model

### Is "trucks die inside the band, robots and bikes carry the last kilometres" right?

**Broadly yes, with four corrections that change the gameplay.**

**Correction 1 — the robot leg is long.** Delivery is needed 15–20 km forward and
the kill zone is 20–25 km deep and growing. The robot leg is not a 1 km dash
from a treeline; it is a 10–20 km one-way run taking hours, mostly at night. At
the game's twelve-to-one compression that is 800–1,700 map metres — long enough
that it must be a *journey with exposure along its whole length*, not a transfer
between adjacent tiles. Treat robot delivery as a short final hop and the
interesting decisions disappear.

**Correction 2 — trucks die outside the band.** Trucks are hit at 30–50 km depth.
The rule is not "trucks cannot enter the drone band" but "trucks cannot use
predictable routes anywhere in deep-strike reach, and cannot enter the band at
all." That gives the player a *third* zone — the interdiction belt where trucks
work but are hunted, and where netting, timing and dispersal decisions live. A
two-zone model (safe rear / lethal band) loses the most interesting layer.

**Correction 3 — the band is not uniformly lethal, because corridors are
buildable.** 822 km of netting in six months, 4,000 km planned, 5–20 km/day, with
a disputed but non-zero protective effect. This is the strongest game mechanic in
the research set: **protected routes as attritable, constructible
infrastructure**. Corridors are built at a real rate, raise survival on a
specific line, are visible and so concentrate enemy attention, and can be broken
faster than they are built.

**Correction 4 — "bikes" is under-specified.** The sources name motorcycles and
quads as a method but gave no loss rate for them. What they do support is a
four-way portfolio for the last leg:

| Leg | Mass per trip | Risk | Character in play |
|---|---|---|---|
| UGV | 150–500 kg | ~1 in 8 per trip, a cheap machine | The workhorse; attrition is a budget line |
| Heavy multirotor | ~15 kg | A machine, no person | Keeps a position breathing; 20 sorties per robot run |
| Motorcycle / quad / pickup | 50–500 kg | A person | Fast, high-variance, for urgent loads |
| Men on foot | 20–40 kg | A person, very slowly | The floor; never zero, never enough |

So: **yes, robots and bikes carry the last kilometres — but robots carry the
mass, drones carry the difference between a position living and dying, and men
on foot are the floor nothing can take away.** Model only robots and bikes and
you lose the characteristic situation of this war: a position kept barely alive
by 15 kg drone drops while everyone waits for the weather or the jamming to
change.

### The specific mechanics worth building

1. **Consumable robots, not units.** Expected life eight round trips (10–14%
   loss per mission), cost about two attack drones. The player should be
   comfortable losing them and uncomfortable running out. No veterancy.

2. **Supply as a rate with variance, not a binary.** A position is not "supplied"
   or "cut off": it has an inflow and four separate stocks — ammunition,
   water/food, **batteries**, fortification material — drawing down at different
   rates. Batteries are the interesting one: exhaust them and the position loses
   its EW, sensors and own drones, which raises the loss rate on everything
   trying to reach it. A death spiral the player can see coming and fight.

3. **Casevac as reverse logistics competing for the same vehicles.** Every
   evacuation run is a supply run not made. Unevacuated casualties should degrade
   combat power *and* willingness to act — units stop counter-attacking because
   they cannot get the wounded out, not because they cannot win.

4. **Relief is harder than resupply.** Rotation should be a separate, riskier
   action with its own failure mode (the relief is turned back). Unrelieved
   positions should accumulate fatigue without dying, so the 165-day hole is a
   reachable, memorable game state.

5. **Fibre is for one-way jobs.** Offer it as the unjammable *assault and mining*
   option with a spool limit (5–20 km, ~400–1,700 map metres) and no return trip
   — not a strictly-better logistics link. That makes fibre a choice rather than
   an upgrade, and it matches the reporting.

6. **Night as the master clock.** Nearly all supply movement is nocturnal. With
   optical detection already halved at night, the counter-play becomes thermal
   and acoustic detection of the night runs — the sensor model the brief already
   describes.

7. **Degradation timescales.** Placeholders until better sources exist, to be
   flagged in code as *designer estimates, not research*: combat-ineffective
   after 3–7 days fully cut off; untenable after 2–3 weeks; evacuation delay of
   hours with a robot, days without, weeks under sustained observation.

### What is genuinely uncertain

- The **robotic share of forward logistics** (80–90% claimed; measurement
  unclear, claims from interested parties).
- The **split between resupply and casevac** — reported as one combined category.
- **Loss rates for every non-robot method.** The 10–14% UGV figure has no
  counterpart for motorcycles, quads, pickups or foot parties, so any comparison
  the game draws between them is a designer's judgement.
- **How well nets work.** Built at enormous scale, dismissed by some operators.
- **Russian volumes.** Platforms are documented; fleet size and tempo are not.
- **How long a position holds unsupplied.** No figure found; the most important
  gap here.

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
