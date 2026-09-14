# Ground robots, logistics and the infantry

Research note for KILL ZONE. First compiled 14 September 2026; **substantially
revised and deepened the same day** (second research pass).

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

## Method note (revised)

**Everything here is snippet-sourced.** Direct page retrieval — WebFetch, curl,
any form of fetch — cannot work in this environment. The first pass on this
document recorded that as though particular publishers (War on the Rocks, MWI,
CEPA, Kyiv Post, mod.gov.ua) were blocking it. **They were not.** The
environment's network policy denies outbound HTTPS to everything except a
package-registry allowlist; the gateway answers `403 to CONNECT (policy denial)`
before the site is ever contacted, so the identity of the site is irrelevant.
WebSearch runs server-side and works normally. That is the only channel.

**Search budget.** The first pass got **nine** WebSearch calls before the session
budget ran out, which is why it was largely reasoned rather than researched. This
pass used **66**. Roughly forty figures below are new, and six of the first
pass's estimates have been replaced with sourced numbers (each one flagged
**[CORRECTED]** where it appears).

**What snippet-sourcing means for weight.** Every URL in the Sources section
appeared in search results; none was opened. Where a figure appears in two or
more independent snippets I say so, because that is the only corroboration
available. Where a snippet gives a number without its units, date, or
denominator, I say that too rather than inferring the missing part.

**Consistency check on the headline series.** The Ukrainian monthly UGV mission
figures reconcile exactly: 7,511 + 7,960 + 9,072 + 11,028 + 14,059 + 16,664 =
**66,294**, matching the "over 66,000 year-to-date" published 31 July (so that
total covers January–June, not January–July); plus July's 19,969 and August's
25,143 gives **111,406**, matching the "~112,000" published in early September.
That is a real internal consistency, not a coincidence, and it is the strongest
reason to treat this series as the document's spine.

---

## Summary

1. **The band is deeper than "the last kilometres."** 20–25 km each side of the
   line, "25 km or more depending on the sector" per the commander of Ukraine's
   Unmanned Systems Forces, with a corps commander expecting 30 km by end-2026
   [R]. IEEE Spectrum describes it as a ~35 km-wide swath straddling the line
   [R]. Infantry walk over 15 km to positions; Russian infantry on the southern
   front now walk **30 km**, six to eight hours, because the trucks are gone [R].

2. **Trucks die outside the band, not inside it.** Russian Ural and KamAZ trucks
   have been struck at 30–50 km depth, and Ukraine claims strikes on **~446 motor
   vehicles and fuel tankers per day** in August 2026 [C, Ukrainian General
   Staff]. Unarmored and civilian vehicles are now **90% of Russian vehicle
   losses**, up from 25% a year earlier [R, David Axe].

3. **Robotic logistics went from marginal to routine in nine months.** Ukrainian
   UGV missions in DELTA rose from 7,511 in January 2026 to **25,143** in August,
   **111,406** cumulative by early September [C, Ukrainian MoD].

4. **Procurement matches and then some.** 25,000 UGVs contracted in H1 2026;
   Zelensky ordered **50,000 produced in 2026** (announced 27 April) [C]. Unit
   costs cluster at $8,000–$20,000; 19 contracts worth ₴11 bn (~$250 m) [R].

5. **The robotic share of forward logistics is high in specific units and
   seasonal.** 28th Mechanised Brigade 70%; 3rd Assault Brigade 80%; 21st
   Unmanned Systems Regiment up to 90%; Pokrovsk/Myrnohrad 90% [R/C]. The NC13
   commander gives the honest version: **80% in July–August, falling to 50% as
   weather and ground deteriorated** [R]. Seasonality is a first-class variable,
   not noise.

6. **Robots are consumed, and the consumption is accelerating.** **[CORRECTED]**
   The first pass gave a flat 7–10 trips and 10–14% loss per mission. The real
   picture is a distribution that is getting worse: ~7 missions is the usual
   baseline; 5–10 near the line of contact; **1–2** for robots that go right up
   to infantry positions; and a ground-robot unit commander in Kharkiv Oblast
   says it is now **3–4 missions where it was 8–9 before and 12–15 earlier
   still** [R]. Per-mission loss is therefore **10–14% at best, 25–33% on a hot
   sector, and up to 50–100% on the last leg** [I]. The outlier that proves the
   mechanism: the **Zmiy**, engineered to be near-silent and low-heat, averages
   **57 missions** [R].

7. **Fibre-optic control is explicitly *not* for logistics.** Seven fibre UGV
   models were tested through Brave1 and pointed at kamikaze and high-risk
   combat runs [R]. Spools run 5–20 km. The cable is a one-way consumable; a
   supply robot has to come back.

8. **Protected corridors are real infrastructure and the numbers have moved.**
   822 km January–June 2026, **1,066 km by July**, **1,382.6 km by end-August**,
   with **213.1 km built in August alone** (~6.9 km/day) and 4,000 km the
   year-end target [C, Fedorov / State Special Transport Service]. Effectiveness
   is genuinely contested: nets catch drones, and nets burn, melt, ice up and
   collapse, and drones wait underneath them [R].

9. **Casualty evacuation is a minority of robot missions but consumes robots out
   of all proportion.** **[CORRECTED — this was the document's biggest [U].]**
   Third Army Corps: 18,000+ UGV missions, **4,500 t of cargo and 600+ wounded
   evacuated** in a year [R]. National Guard, January–June 2026: **1,477 t and
   113 wounded** [R]. Evacuations are ~3% of missions by count and ~1% of
   tonnage — but Euromaidan Press reports **four ground robots expended per
   evacuation** under Russian counter-targeting, and one case of six consecutive
   failed rescue attempts in which every robot used was destroyed or disabled
   [R].

10. **A cut-off position degrades over one to three weeks, it does not collapse
    in days.** **[CORRECTED — the first pass's 3–7 days / 2–3 weeks was an
    explicit designer guess. It now has evidence, and it was too fast.]** The
    14th Brigade near Kupyansk went **up to 17 days without food deliveries**
    and reported seven months of shortage, with soldiers saying "we often faint
    and are physically unable to defend our positions" — the plainest statement
    of combat-ineffectiveness in the whole source set [R]. The 121st Territorial
    Defence Brigade: **3–7 days routinely, 12 days in one instance**, with
    **240 kg of food in 30 days** (27 nine-kilogram packages) delivered to a
    platoon position [R/C].

11. **Infantry organisation: a real rule now exists.** **[CORRECTED]** The first
    pass said rotations were "indefinite." In April 2026 Syrskyi ordered a
    **mandatory two-month limit** on the forward edge after public outcry [R].
    The documented extremes remain — 165 days for two soldiers near Orikhiv, a
    year in a foxhole, eight months for the unit the 121st relieved — so the rule
    is a rule, not a description.

---

## 1. The geometry: where the band starts and where trucks stop

| Boundary | Distance from line of contact | Confidence |
|---|---|---|
| FPV / fibre FPV saturation (the "kill zone" proper) | 20–25 km typical on the eastern front; "25 km or more depending on the sector" (Robert "Magyar" Brovdi, Unmanned Systems Forces); 30 km projected by end-2026 (7th Airborne Assault Corps) | [R] multiple |
| Same, expressed as total width | ~35 km straddling the line | [R] IEEE Spectrum |
| Depth at which logistics becomes "difficult" | up to 20 km | [R] |
| Distance at which units now need delivery | 15–20 km forward | [R] |
| Distance Ukrainian infantry walk to position | >15 km on active sectors | [R] |
| Distance Russian infantry walk on the southern front | **30 km / 19 miles, 6–8 hours in kit** | [R] Fedorov, July 2026, via ISW strike counts |
| Depth at which Russian Ural/KamAZ trucks have been hit | 30–50 km | [R] |
| Depth of the medium-range interdiction campaign | 30–300 km; strikes recorded 205 km into occupied Luhansk | [R] |

Note on the kill-zone figure: the sources are explicit that it **"refers not to
the maximum flight range of individual drones, but to the regularity and density
of strikes"** [R]. That is exactly the right framing for a game — it is a *rate*
boundary, not a *range* boundary, and it should move with the density of enemy
drone units on that sector rather than being a fixed radius.

The structural point: there are **three** zones, not two.

- **Beyond ~50 km** — conventional road logistics, degraded by deep strike but
  still trucks on roads.
- **~15–50 km** — the interdiction band. Trucks move but are hunted; losses are
  episodic rather than certain. This is where netting, timing, convoy bans,
  dispersal and camouflage live.
- **Inside ~15 km** — the kill zone. Driven vehicles lose on any repeatable
  schedule. Robot and aerial-drone country, with men on foot as the fallback.
  A 59th Brigade drone operator states it flatly: **"heavy bomber drones are
  currently the only viable way to resupply forces within 10 km of the front
  line"** [R], and a 107th Territorial Defence Brigade commander says pickups no
  longer reach forward positions at all [R].

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
| August | **25,143** | +26% |
| **Cumulative to early Sept** | **111,406** | 3.3× the January rate |

(August is now a precise figure, not ">25,000"; a separate MoD statement gives
15,729 missions in 1–20 August, consistent with a month-end 25,143.)

**Tonnage per mission is now sourced, not guessed.** **[CORRECTED]** The first
pass assumed 150–300 kg a load and produced a wide 3,750–7,500 t estimate for
August. Two independent unit-level datasets now pin the average:

| Source | Period | Missions | Cargo | Wounded evacuated | kg per mission |
|---|---|---|---|---|---|
| 3rd Army Corps (Lyman–Borova) | 12 months to June 2026 | >18,000 | 4,500 t | >600 | **250 kg** [I] |
| National Guard of Ukraine | Jan–June 2026 | [U] | **1,477 t** | **113** | — |
| Khartiia (13th) UGV battalion | ~20 days, April 2026 | >80 | >25 t | — | **~310 kg** [I] |

Applying 250 kg/mission to the national series: **August moved on the order of
6,300 tonnes**, and the cumulative 111,406 missions represent roughly **28,000
tonnes** [I]. That is a much tighter number than the first pass's range and it
sits in the upper half of it.

Derived rates, all [I]: August's 25,143 missions is **811 missions/day**, one
every 1.8 minutes across the whole front. Spread over ~1,200 km that is 0.7
missions per kilometre of front per day — but it concentrates heavily, so
**5–10 per km/day on a hot sector** remains the useful design number.

**Procurement** [C/R]: 25,000 contracted in H1 2026 (more than double all of
2025); Zelensky ordered **50,000 produced during 2026** on 27 April; 19 contracts
worth ₴11 bn (~$250 m) signed; contracts for **2027** already being signed to
stabilise manufacturer pipelines [R]. Ukraine has moved to ministry-set
specifications with a rule that **no maker can take more than half of any
class** [R] — the industry is being consolidated, not just scaled.

**Attrition economics.** At ₴600,000–700,000 ($15,800–18,400) per unit, the
amortised cost per mission depends entirely on which survival figure you use:

| Survival | Amortised cost per mission | Where this applies |
|---|---|---|
| 57 missions (Zmiy) | ~$300 | A stealth-optimised platform on a quiet route |
| 12–15 missions | $1,100–1,500 | Kharkiv Oblast, "earlier still" |
| 8–9 missions | $1,800–2,300 | Kharkiv Oblast, "before" |
| ~7 missions (typical) | ~$2,300 | The usual quoted baseline |
| 3–4 missions | $4,000–6,100 | **Kharkiv Oblast now** [R] |
| 1–2 missions | $8,000–18,400 | Right up to an infantry position [R] |

A brigade losing 2–5 robots a day [R] is therefore running anywhere from **6 to
50 UGV missions a day** depending on sector [I] — the first pass's 14–50 was
computed from the optimistic end of the survival range only.

A study by the Central Research Institute of the Armed Forces of Ukraine reports
UGVs have cut Ukrainian casualties by 30% [C]; Ukraine's General Staff has been
credited with the same "up to 30%" figure [C]. Plausible mechanism, unverifiable
magnitude, and note that both versions come from the same institution.

**Scale of the operator force.** Around **150 different ground robot models** are
in use [R] — which is a supply-chain problem, not a strength. The robot-unit
count went from 67 to 167 in four months [R]. As many as 12 brigades recruit
robot operators; pilots take **four to six months** to become experienced, and
operators spend **up to 12 hours** piloting a slow vehicle while watching for
threats [R]. One described mission crew: **a commander, three operators and a
navigator** — five people for one robot [R]. Ihor Shmyryov of Brave1: *"Right
now, the UGV and operator are working together"* — these are remotely operated
avatars, not autonomous vehicles [R].

## 3. UGV types

| Platform | Side | Payload | Range / endurance | Cost | Primary role | Conf. |
|---|---|---|---|---|---|---|
| TerMIT (Tencore), tracked | UA | 300 kg | 20–40 km, to 12 h (v2.0) | ~$12,000 | Logistics, casevac, combat | [R] |
| Bizon-L | UA | 300 kg | 50 km | [U] | Logistics; NATO-codified | [R]/[C] |
| Murakha / Muraha | UA | 500 kg | 50 km (electric) | [U] | Heavy sustained logistics | [C] |
| **Zmiy** | UA | 500 kg | [U] | [U] | Silent electric logistics; **near-noiseless, low thermal signature, averages 57 missions, "can make it back even if two wheels are destroyed"** | [R] |
| **Protector** | UA | **up to 1 t per mission**; to 60 km/h | [U] | [U] | Heavy logistics; footage shows one surviving an FPV hit and completing delivery | [R] |
| **Vepr** | UA | [U] | [U] | [U] | **Dedicated medevac, MoD-codified 2026** | [R] |
| **MAUL** | UA | [U] | [U] | [U] | Armoured medevac capsule; retrieved a casualty 64 km inside Russian-held ground, survived repeated drone strikes and a mine | [R] |
| NUMO, tracked | UA | [U] | [U] | [U] | Multi-purpose | [C] |
| Spider, tracked | UA | n/a | [U] | [U] | Fire support; LTE/radio/Starlink/fibre | [R] |
| Ratel H | UA | [U] | [U] | [U] | **Survived three FPV hits and ~50 missions in a year** | [R] |
| Generic UA logistics UGV | UA | 150–300 kg typical (250 kg observed average) | 10–50 km | $8,000–$20,000 | Resupply, casevac | [R] |
| **Kuryer / Courier** | RU | 200 kg, more with trailer; **35 km/h; control range 3–10 km** | [U] | [U] | **Most widely deployed Russian UGV; primary role logistics, not fire support.** Also 10× TM-62 mines, one casualty internally, Shmel thermobaric, Bagulnik-82 82 mm mortar | [R, Janes] |
| **Varan** | RU | [U] | [U] | [U] | One of three RU types in serial production at scale | [R, StateWatch] |
| **Impuls / Impulse-M** | RU | [U] | [U] | [U] | Serial production; **hundreds delivered by early 2026** | [R, StateWatch] |
| Omich / Omich-2 | RU | [U] | [U] | [U] | Among the most widespread; "dozens to hundreds" at the front | [R] |
| Depesha (Rostec) | RU | 150 kg | [U] | [U] | Fuel, ammunition, provisions, casevac | [C, vendor] |

Notes. Tencore has a joint venture with Germany's Quantum Systems to build
2,000 TerMITs in Germany for Ukraine [R]. Ukraine is also converting Soviet-era
BRDM-2 hulls into remotely controlled platforms [R], and the K-2 Brigade has
stood up what is described as the world's first UGV battalion [R].

**Design note on the Zmiy.** This is the most useful platform fact in the
document for a game with a signature model. The Zmiy survives **eight times**
longer than the fleet average, and the stated reason is that it is *"engineered
to be nearly noiseless and emit as little heat as possible, helping it to elude
Russia's ISR drones."* That is a direct, sourced link between the acoustic and
thermal signature channels the brief already models and the attrition rate of a
logistics unit. Signature should not be cosmetic on ground vehicles; it should be
the single biggest multiplier on how many trips a robot makes.

## 4. What UGVs are actually used for — the resupply/casevac split, now measured

The MoD publishes missions as one combined category, "logistics and evacuation,"
so the first pass marked the internal split **[U]** and said only that resupply
was "dominant by a wide margin." **That judgement was right and it can now be
quantified.**

**The two hard datasets:**

| Formation | Period | Cargo | Wounded evacuated | Cargo per casualty |
|---|---|---|---|---|
| 3rd Army Corps | 12 months to June 2026 | 4,500 t over >18,000 missions | >600 | **7.5 t** |
| National Guard of Ukraine | Jan–June 2026 | 1,477 t | 113 | **13.1 t** |

**Derived split** [I]:

- **By mission count: evacuations are ~3.3% of UGV missions** (600 of 18,000) if
  each evacuation is one mission. That is the lower bound.
- **Upper bound: ~13%.** Euromaidan Press reports **four ground robots per
  evacuation** under Russian counter-targeting; if the average successful
  evacuation consumes four robot sorties, the evacuation share of *missions*
  rises to roughly 13% even though the casualty count is unchanged.
- **By tonnage: ~1% or less.** A casualty and a stretcher are ~100 kg against
  4,500 t of cargo.
- **Third Army Corps' claim that "every second ground robot in Ukraine's Armed
  Forces operates within the Third Army Corps"** [C] does **not** reconcile with
  the mission counts — the corps ran ~1,500 missions/month against a national
  ~11,000/month average in H1 2026. Treat the "half of all robots" line as unit
  publicity, not an order of battle.

**So, for the game: roughly 30 supply runs per casualty evacuation by count, and
100:1 by tonnage — but the evacuation run is four times more likely to cost you
the vehicle.** That is a much better mechanic than a simple 50/50 split, because
it makes casevac rare, expensive, and emotionally weighted rather than routine.

Revised ordering of UGV work, by volume:

1. **Resupply** — ammunition, food, water, batteries, engineering materials,
   communications kit. Dominant: ~97% of missions and ~99% of tonnage [I].
2. **Casualty evacuation** — small by count, disproportionate in robots consumed
   and in what it does to infantry behaviour [R].
3. **Mine laying and clearing, obstacle emplacement** — a growing role; the
   Russian Kuryer carries 10 TM-62 anti-tank mines [R].
4. **Emplacing EW and surveillance kit** — a logistics mission in form, a combat
   mission in effect [R].
5. **Recovery** — towing disabled robots and damaged vehicles out. The 93rd
   Brigade adapted ground drones specifically for this [R]. New to this pass.
6. **One-way attack** — UGVs loaded with anti-tank mines or bulk charges driven
   into trench lines and treeline bunkers [R]. This is what fibre control is for.
7. **Direct fire** — real but rarer: the Russian Kuryer with a thermobaric
   launcher or an 82 mm automatic mortar module [R]; on the Ukrainian side a
   single armed UGV of the NC13 company, 3rd Army Corps, **held a position
   through repeated assaults for six weeks** [R, Defense One — the first pass's
   "45 days" and this "six weeks" are the same event].

## 5. Failure modes and how often robots are lost

| Failure mode | Evidence | Conf. |
|---|---|---|
| Killed by FPV or drone-dropped munition | The dominant mode; explicitly the reason survival is falling in Kharkiv Oblast ("denser Russian FPV drone and mining fire") | [R] |
| Mines and UXO on the route | Named alongside FPV as the cause of collapsing UGV lifespan | [R] |
| **Terrain — Ukraine's heavy black soil** | Explicitly the main challenge: soil clogs running gear after rain and leaves vehicles stuck or disabled. Platforms that pass test-range trials "frequently encounter their hardest terrain for the first time in combat" | [R] |
| **Overheating electronics and weak components** | Named: many platforms perform in trials and fail under sustained real use | [R] |
| Control link lost (jamming, terrain masking, range) | The stated reason fibre is being tested, and the stated constraint on Russian UGV use | [R] |
| Mechanical failure, battery exhaustion | Endurance to 12 h; soldiers modify systems in the field to keep them running | [R] |

**Loss rate — the corrected picture.** **[CORRECTED]** The first pass gave a flat
7–10 trips / 10–14% per mission and called it "the most useful number in the
document." It is still the most useful number, but it is **a distribution that is
moving, not a constant**:

| Population | Missions survived | Implied per-mission loss | Source character |
|---|---|---|---|
| Zmiy (stealth-optimised) | **57** | ~1.8% | [R], explicitly a record |
| Ratel H (one documented vehicle) | ~50 in a year, 3 FPV hits survived | ~2% | [R], an anecdote |
| Fleet baseline, "usually" | **~7** | ~14% | [R], Ukrainian roboticists |
| Near the line of contact | 5–10 | 10–20% | [R] |
| Kharkiv Oblast, 2026 **now** | **3–4** | **25–33%** | [R], named unit commander |
| Kharkiv Oblast, "before" | 8–9 | 11–13% | [R], same source |
| Kharkiv Oblast, "earlier still" | 12–15 | 7–8% | [R], same source |
| Approaching an infantry position | **1–2** | **50–100%** | [R] |
| Contested medevac | ~4 robots consumed per casualty out | — | [R] |

The named source for the declining trend is **Andrii Kopach, call sign
"Matematyk," commanding a ground-robot unit in the "Lava" unmanned regiment of
the Khartia corps**, who attributes the collapse to denser Russian FPV and mining
fire [R].

**Design consequence.** A supply robot is not a unit that survives and
accumulates value; it is a consumable with an expected life that depends on
*where* it goes, *what it sounds and looks like*, and *what month it is*. Model
it as a per-leg survival roll whose odds are set by the sector's drone density
and the vehicle's signature, not as a flat 1-in-8.

## 6. Control links for ground robots

| Link | Range | Jam-resistant | Used for | Conf. |
|---|---|---|---|---|
| Radio | Line-of-sight limited; degrades in rough terrain | No | The bulk of logistics runs | [R] |
| LTE | Where coverage survives | Partly | An option on Spider | [R] |
| Starlink | Long; needs sky view and power | Partly | An option on Spider; terminals are themselves delivered cargo — and Russia has strapped Starlink antennas to **horses** to extend coverage | [R] |
| Fibre tether | Spool 5–20 km; 10 km the 2025 workhorse, 20 km in testing | Yes, unjammable | **Kamikaze and high-risk combat, explicitly not logistics** | [R] |
| Airborne repeater / balloon | — | No | Ukraine uses balloons plus Starlink to extend FPV range; UGV application unconfirmed | [R] on balloons, [U] on UGVs |

The fibre finding is the one a designer is most likely to get wrong, because an
unjammable link looks like a strict upgrade. It is not. Ukraine tested seven
fibre UGV models through Brave1 and pointed them at one-way missions [R]. The
reasons are structural: the cable is consumed, does not survive a return leg,
snags on the terrain a supply robot must cross repeatedly, and the spool mass
competes with payload. Over 80 Ukrainian fibre systems (drones and UGVs together)
were approved by February 2026 [R].

Note the counter-counter-measure, new this pass: **lasers are being used against
the fibre itself** — heating the cable until it fails and severing the link [R].
Unjammable is not uncuttable.

## 7. The last kilometres — every method, with the loss rates now available

**This was the document's single biggest hole. It is now substantially filled for
non-robot transport, though not with a clean per-trip probability.** What exists
is fleet-level attrition, lifespan figures and unit anecdote — which is enough to
set relative risk defensibly.

| Method | Typical leg | Load per trip | Practical throughput | Loss evidence | Conf. |
|---|---|---|---|---|---|
| Truck / KamAZ / Ural convoy | 30–50+ km rear | tonnes | High | **~446 Russian motor vehicles and fuel tankers claimed hit per day, August 2026** (13,835 for the month); ~500/day at peak in late May–June; 3,600 Russian logistics vehicles destroyed or damaged in July; ACLED puts Russian trucks burnt on southern occupied roads since mid-June at **3,000–5,000** | [C] Ukrainian claims, [R] ACLED |
| Truck or pickup on a netted corridor | Through the 15–25 km band | 0.5–2 t | High while the net holds | See §8; nets catch drones and are also burned, iced and waited-under | [R]/[U] |
| **Pickup / SUV, unprotected road** | 5–15 km | ~0.5 t | Low; one-time or nightly | **Average front-line lifespan ~90 days / "two to three months maximum"** (Max Titov, volunteer procurer) [R]. A brigade commander: **"up to ten vehicles per week lost on logistics runs"** before adopting UGVs [R/C]. Freedom Convoy MTÜ: **1,000 Ukrainian cars lost in a month** [C]. Russian drones "can destroy a thousand vehicles in a month" [R]. 47th Mech drone operator: *"We are losing them every day, in big numbers."* | **[R]** — new this pass |
| **Motorcycle / quad / ATV** | 3–15 km | 50–150 kg | Low per trip, fast turnaround | **Unarmored vehicles (ATVs, bikes, cars, vans, scooters) are 90% of Russian vehicle losses, up from 25% a year earlier — nine written off for every tank or BMP** [R, David Axe]. For assault use: *"only one in several groups survives to complete the mission"*; *"most riders are killed before reaching their target"* [R, Frontelligence]. One Donetsk attack: 15 motorcycles and 40 men lost [R, RUSI]. Petrol bikes mask the sound of approaching drones [R] | **[R]** on the assault case; **[U]** still, for a pure *logistics* motorcycle run |
| **UGV** | **10–20 km each way** | **250 kg average (150–1,000 kg range)** | 1 to several missions per vehicle per night | **10–14% baseline, 25–33% on a degraded sector, 50–100% on the last leg** | **[R]** |
| Heavy multirotor (Vampire/"Baba Yaga") | 20 km delivery, 35–45 km reach; flies at night | **~15 kg** | Many sorties, small each; one brigade moved **4 t in a month** by Vampire | **[U]** — manufacturer and MoD do not publish loss rates; explicitly noted as undisclosed | [R] on payload/reach, [U] on rate |
| **Fixed-wing cargo drone (Windracers ULTRA)** | 1,000 km+, >10 h endurance, >4,000 m ceiling | **150 kg** (upgrade to 200 kg / 2,000 km planned) | Dozens in Ukrainian service; 10/month production | [U] | [R] — **new this pass** |
| Men carrying | >15 km on foot (UA); **30 km, 6–8 h** (RU southern front) | 20–40 kg | Very low | [U] on rate; the journey itself carries drone, artillery and remote-mine risk [R] | [R] on distance |
| **Pack animals (horses, donkeys, mules)** | [U] | [U] | Very low | Systematic Russian use near Pokrovsk, Zaporizhzhia and Siversk; quiet, need no fuel; one FPV strike on two horsemen filmed | [R] — **new this pass** |
| Tunnels / covered underground routes | [U] | — | — | **2 km of linked underground fortifications joining 12 positions** with internal communication routes, built by the Ukrainian Association of Developers on the eastern front, January 2026; both sides build tunnel networks connecting fighting positions to rest areas, command posts and resupply points | [R] — **previously [U], now partially filled** |

Four points a designer should take from this table:

- **The pickup has a measurable half-life and it is about ninety days.** That is
  the number to price a logistics vehicle against. At, say, three runs a week
  over ninety days, a pickup makes roughly **forty runs before it dies** — a
  per-run loss probability on the order of **2–3%** [I]. Compare the UGV's
  10–33%. **The robot is not safer per trip; it is more expendable per trip.**
  Ukraine sends robots not because they survive better but because losing one
  costs $12,000 and no funeral. This inverts the intuitive design assumption and
  is the most important correction in this section.
- **The assault motorcycle and the logistics motorcycle are different animals.**
  All the quantified motorcycle loss reporting is about *assaults into contact*
  ("one in several groups survives"). For a bike carrying ammunition to a
  position ten kilometres back, no loss figure exists in open sources. Say so in
  the code.
- **The heavy aerial drone supplements, it does not replace.** A 15 kg Vampire
  load is described as "little more than a middling stream of small arms
  ammunition, sausages, energy drinks and cigarettes" [R]. Yet within 10 km of
  the line it is described as *the only viable method* [R]. The fixed-wing ULTRA
  at 150 kg changes the ceiling but not the last mile — it is a theatre-level
  delivery aircraft, not a trench-level one.
- **Darkness is the free counter-measure.** Vampires fly at night; robots run at
  night; a single robot can make several missions in one night [R]. The system's
  tempo is nocturnal.

## 8. Counter-measures protecting logistics

**Netted road corridors** are the headline measure and are now at infrastructure
scale. **[CORRECTED — the first pass had only the 822 km and 5–20 km/day
figures.]**

| Milestone | Figure | Source character |
|---|---|---|
| Jan–June 2026 | **822 km** | [C, Fedorov] |
| By mid-May 2026 | "more than 1,170 km" of anti-drone road protection in frontline oblasts | [C, Fedorov] — note this appears to include pre-2026 work; the two series are not consistently scoped |
| By July 2026 | **1,066 km** built by the State Special Transport Service | [C, SSTS] |
| By end-August 2026 | **1,382.6 km**, of which **213.1 km in August alone** (~6.9 km/day) | [C, SSTS via Ukrinform] |
| Year-end target | **4,000 km** (≈2,500 miles) | [C] |
| Build rate | 5 km/day January → 12 km/day February → **9.2 km/day by June**; 2026 pace "doubled" over 2025 (4 → 8.5 km/day) | [C] — the February 12 and June 9.2 figures do not sit comfortably together; treat 5–12 km/day as the honest band |
| Funding | ₴1.6 bn (~$37 m) additional from the state budget | [C] |
| Notable single works | 6.5 km tunnel on the Orikhiv highway, Zaporizhzhia (first of its kind, July 2025); 40 km on Izium–Sloviansk; the **last 80 km of the Kyiv–front road** | [R] |
| Russian equivalent | 2 km mesh tunnel on Bakhmut–Chasiv Yar; makeshift mesh along the E50; "anti-drone corridors" in Kursk; **Darwin-D**, a purpose-built modular protective net intended for rapid coverage and rapid repair | [R] |

**Does netting work? The contested answer, in both directions.**

*For:*
- Ukrainian military personnel say netting "makes it possible to move troops more
  safely and evacuate the wounded"; the Orikhiv nets "caught the first drones"
  [R].
- Fedorov's framing: *"each protected kilometer means safer logistics, faster
  supply, casualty evacuation, and safer military movement even under constant
  threat of drone attacks"* [C].
- Nets work against **fibre-optic** drones, which EW cannot touch — explicitly
  the reason the State Special Transport Service adopted them [R]. That is the
  strongest functional argument for netting: it is the only counter-measure that
  is indifferent to the control link.
- The mechanism is mechanical and cheap: propellers tangle in nylon cord and the
  drone loses control or falls [R]. No maintenance after installation [R].

*Against:*
- Ukrainian FPV operators publicly dismissed Russia's anti-FPV tunnels [R, 2025].
- **Nets burn and melt.** Incendiary munitions destroy sections or open holes
  large enough for other FPVs to fly through — and then the attacking drone has
  quarry confined in a narrow tunnel, which is *worse* than open road [R].
- **Nets ice up and collapse.** During winter testing the nets froze and came
  down; moisture resistance is now a design priority [R].
- **Drones wait underneath.** Operators park FPVs under the netting and ambush
  what comes along [R].
- **Gaps are ambush points.** Reporting specifically frames the current state as
  "catching Russian drones, but gaps leave vehicles exposed to ambush" [R].
- A net marks the route. It is a permanent, visible declaration of where traffic
  will be.

**What netting does to the 71% number.** The deep-strike research records
military traffic on one road falling by up to 71% under drone interdiction. That
road is the **R-280 / M-14**, Rostov → Mariupol → Berdyansk → Melitopol → Crimea,
Russia's main overland artery to the peninsula since the Kerch bridge was
degraded; the baseline was ~3,500 cargo trucks a day, and occupation authorities
suspended traffic on 22 May 2026 after repeated strikes [R]. **No source found in
this pass gives a before/after traffic figure for a netted road.** What the
sources do show is that Russia responded to interdiction on that corridor with a
*package*: netting on the most exposed sections, mobile gun teams escorting
convoys, dazzle/"zebra" paint schemes aimed at drone computer vision, military
trucks repainted as civilian dump trucks, and route changes [R]. Netting is one
element of a bundle whose combined effect is what partially reopened the road —
so **do not attribute a traffic-recovery number to netting alone.** Marking this
**[U]** is the honest answer, and the bundle is the better game mechanic anyway.

**Other measures**, now better evidenced than in the first pass:

- **Camouflage and deception** — previously [U], now [R]. Russian logistics
  vehicles painted with black-and-white dazzle stripes and swirling geometric
  patterns, which experts assess are **aimed at the computer-vision systems in
  drone targeting workflows, not at human eyes** [R]. Military Ural-4320 trucks
  repainted in civilian colours and modified to look like commercial dump trucks
  [R]. Decoy aircraft outlines painted at installations [R].
- **Mobile fire-support groups** escorting Russian logistics columns — described
  as largely ineffective, with drones outpacing the gunners [R].
- **Dispersal and outright convoy bans** in some Russian sectors [R].
- **Concealment of the transfer point**, as when the 93rd Brigade around
  Druzhkivka runs supplies to robot-resupply teams hidden under trees [R] — the
  marshalling area is itself a target and has to keep moving.
- **Night movement** — universal and assumed rather than reported.

## 9. What a cut-off position actually experiences

**[CORRECTED — this section was the first pass's most important [U] and its
numbers were explicitly designer estimates. They are now replaced.]**

**It is degradation, not collapse.** Trench sections that cannot receive water,
batteries, ammunition or anti-tank weapons "lose combat power before being
physically overrun" [R]. A cut-off position stops being able to see and shoot,
and is then taken.

**How long does it take? Here is the actual reporting.**

| Case | Interruption | Outcome | Conf. |
|---|---|---|---|
| **14th Brigade, Kupyansk, April 2026** | **Up to 17 days without food deliveries**; seven months of "constant delays in food, water and gasoline"; Ukrainian positions on the far bank of the Oskil cut off from ground supply entirely, drone-only, and drones limited by range and payload | Soldiers: **"we often faint and are physically unable to defend our positions."** Photographs of emaciated soldiers went viral; the General Staff confirmed the problem and **dismissed the commanders of the 14th Brigade and 10th Corps** on 24 April | [R] |
| **121st Territorial Defence Brigade, Zaporizhzhia, August 2026** | Food and water undelivered **3–7 days routinely, 12 days in one case**; four months without rotation (told it would be one) | Soldiers drank their own urine; unit appealed publicly to the command. Brigade's reply: **27 packages of 9 kg = ~240 kg of food in 30 days** to the platoon; the soldiers' family said the real figure was lower | [R] / [C] for the brigade's reply |
| **Two soldiers near Orikhiv (Zaporizhzhia)** | Deployed May for "no more than a month", held **165 days**; **water and food ran out in the first two weeks** | Rationed to **one sip of water at 6 a.m.**; squeezed moisture from wet wipes. Drone-delivered packages were shot down in the air, and even when one landed they often could not retrieve it without giving away their position | [R] |
| **Vladyslav, 23, Klishchiivka (Donetsk)** | Ordered to hold two weeks; held **two months**; supplies by drone, none in wet or stormy weather | Drank his own urine after days without water | [R] |
| **A Ukrainian soldier, Donetsk, to July 2026** | **A year in a foxhole** | "Many guys have gone out of their mind" | [R] |
| **Unit relieved by the 121st** | **Eight months** on the same position without rotation | — | [R] |

**The synthesis a designer can use — replacing the first pass's guess.** The
first pass said, explicitly as a designer's number: *combat-ineffective in 3–7
days fully cut off, untenable in 2–3 weeks.* The reporting says both halves are
wrong in the same direction:

- **Ammunition in a firefight: hours.** Unchanged, still [I].
- **Water: 2–4 days before improvisation** (urine, wet wipes) and hard rationing
  begin — this is now [R] rather than [I], from two independent cases.
- **Combat-ineffectiveness ("physically unable to defend our positions"):
  roughly 7–17 days** of interrupted supply, not 3–7 [R, 14th Brigade].
- **Untenable: months, not 2–3 weeks.** Positions have been held for 165 days, 8
  months, and a year on intermittent drone supply. **"Indefinitely, in misery" is
  the normal outcome, not the extreme one.**
- The binding constraint on abandoning a position is not the supply state; it is
  whether anyone can physically *reach* it to relieve or evacuate. Positions are
  not lost because they run out; they are lost because they lose the ability to
  observe and shoot, and are then taken.

**What actually arrives.** A drone-sustained position receives on the order of
**9–10 kg a day** — a single heavy-bomber package of water, food, batteries and
ammunition [R], or ~8 kg/day averaged over the 121st Brigade's 240 kg month.
Al Jazeera's description of drone-fed positions: *"weeks of chocolate bars,
oatmeal and a bottle of water a day"* [R]. Supplies arrive "once a day or once
every other day according to schedule" where the schedule holds [R].

**The consumption list has changed.** It is no longer water, food and ammunition.
It is now also **batteries**, **EW kit**, **communications equipment** and
**engineering material** for continuous fortification [R] — the National Guard's
own cargo breakdown names "ammunition, food, communication equipment,
engineering materials." Batteries are the new water: without them there is no EW,
no comms, no night vision, no drone of one's own. One sourced figure for the
scale: a **four-soldier drone team on an eight-hour mission consumes roughly
2,000–3,500 Wh a day** (drone batteries 1,500–2,500 Wh; controllers and goggles
200–400 Wh; ground station 200 Wh; Starlink and radio 100–300 Wh) [R].

**Cut off also means isolated.** Fighters bury themselves in the ground and
rarely leave [R], and positions sit much farther apart than before [R], so
neighbours often cannot reinforce them or even see them.

## 9a. Consumption per soldier per day — reconciling with `deep-strike.md`

`deep-strike.md` carries **~200 kg per soldier per day**, with a combined-arms
army of ~20,500 needing **~4,100 t/day** [R, RAND]. **That figure is correct and
it measures a completely different thing from anything in this document.** It is
a theatre-level planning factor for an entire combined-arms force: every litre of
fuel for every vehicle, every artillery round, every spare part and every tonne
of construction material moving through the whole logistic chain. It is not what
a man in a hole eats.

The two ends of the scale, so nobody confuses them:

| Level | Figure | Source |
|---|---|---|
| Combined-arms army, all classes of supply, whole chain | **~200 kg/soldier/day**; ~4,100 t/day for 20,500 | [R, RAND, via deep-strike.md] |
| Russian MoD claim, all ammunition and fuel, theatre-wide, 2023 | 10,000–15,000 t/day | [C] |
| NATO water planning factor, temperate, field conditions (drinking, cooking, hygiene) | **15–20 L/soldier/day**; 25–30 L in hot/arid | [R] |
| US Army combined planning factor including hygiene, meal rehydration and emergency medical | 4.1 gal ≈ **15.5 L/soldier/day** | [R] |
| Ukrainian infantry caloric requirement, Donbas/Zaporizhzhia | **3,500–5,000 kcal/day** | [R] |
| **What a forward position actually receives by drone** | **~9–10 kg/day for the position**, "a bottle of water a day" per man | **[R]** |
| 121st Brigade platoon position, contested month | **240 kg / 30 days ≈ 8 kg/day** for the whole position | [C, brigade's own figure] |
| **3rd Army Corps UGV tonnage per head** | 4,500 t / 365 days ÷ ~25,000 personnel ≈ **0.5 kg/soldier/day** by UGV alone | [I] |

**The reconciliation, and the design point.** The army needs 200 kg per soldier
per day; the forward position gets **one to five kilograms per soldier per day**,
and the sources are unanimous that this is starvation-adjacent. The ratio between
theatre demand and what crosses the last fifteen kilometres is on the order of
**100:1**. Almost all of the 200 kg is consumed by things that are not in the
hole — vehicles, guns, engineering, the rear itself. A game that models one
supply number for the whole army will get both ends wrong. Model **two separate
flows**: a rear tonnage economy where the 200 kg figure lives, and a forward
trickle measured in single-digit kilograms per man per day where the interesting
decisions are.

**Still [U] after searching:** fuel consumption for a *forward position's*
generator, in litres per day. Generic industrial generator figures (200–500 L/day)
exist but are irrelevant at this scale, and nothing unit-level was found. Do not
invent one.

## 10. Casualty evacuation under observation

The most consequential change to infantry behaviour, and the best-evidenced human
cost in the sources. **The timings are now much better established than in the
first pass.**

**Without a robot:**
- Getting a casualty out of a forward position can take **days, weeks, sometimes
  months**, especially if they cannot walk [R]. One source states the **average
  wait for an evacuation is a week, with some taking as long as a month** [R].
- One documented case: **12 days** to the forward field unit; by arrival the man
  had gangrene and lost the leg [R]. Three soldiers were retrieved after **more
  than a month** stranded near the front [R].
- Tourniquets are left on **seven hours, sometimes days**, against a two-hour
  standard [R] — the mechanism by which delay turns survivable wounds into
  amputations.
- Russian forces hit **all routes leading to casualty collection points** while
  observing them from a distance [R]. The collection point is a known, fixed,
  targetable node.
- The operative rule, from a Ukrainian medic: nobody moves toward a position
  under drone observation, because nobody will risk more lives. Evacuation
  happens "when the sky has cleared" [R].

**With a robot:**
- A documented UGV medevac ran **36.5 km in 2 hours 13 minutes** under effective
  enemy fire [R].
- Full chain from loading into the robot to arrival at hospital, all stages of
  care included: **about 12 hours** [R].
- One robot near Pokrovsk ran an evacuation **over six hours under FPV fire**
  after taking a direct hit [R].
- One UGV completed **nine successful evacuations in fifteen days with no
  rescuer losses** [R]. The 141st Brigade evacuated 13 wounded in a few weeks
  [R]. One medical unit completed **six UGV evacuations in a single day** [R].
- Not always fast: one wounded man was evacuated **on the third attempt after two
  days** [R].

**And the robot is targeted precisely because it is the evacuation.**
- **Four ground robots per evacuation** is the headline of dedicated reporting on
  Russian counter-targeting of robotic medevac [R].
- One case: **six failed rescue attempts, every UGV used destroyed or rendered
  inoperable** [R].
- One case where the robot failed and a Ukrainian commander drove a pickup into
  the kill zone himself to bring out two wounded [R] — the human fallback never
  goes away.
- Medevac UGVs now carry **armoured capsules** (the MAUL's is polypropylene,
  rated against drone-dropped munitions and artillery fragments) and robotic arms
  to lift casualties who cannot move themselves [R]. Ukraine has codified a
  dedicated evacuation robot, the **Vepr** [R].

**The institutional picture, which the first pass missed entirely.** Tetiana
Chornovol, commanding an ATGM platoon, argues that in the fifth year of the war
evacuation remains in most units *"a force majeure that no one is ready for"*:
most Ukrainian units have **no dedicated evacuation groups and no specialised
armoured transport**, and evacuations happen in chaos with improvised means, with
exceptions only in individual brigades. Her point about morale is the one a game
should take: *"You can't imagine how the existence of professional evac groups
would improve the psychological atmosphere at the front"* — **the certainty of
rescue matters as much as the rescue** [R].

A **400 kg** payload quadcopter for casualty evacuation is contracted for
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
| Position spacing | Mutually supporting | Far apart, isolated, deeply dug; increasingly linked underground where engineering effort allows | [R] |
| **Rotation rule** | Days to weeks | **Mandatory two-month limit on the forward edge, ordered by Syrskyi, April 2026, after public outcry** | **[R] — new this pass** |
| **Rotation reality** | — | Documented extremes of **165 days** (30 failed reliefs), **eight months**, **four months**, **a year in a foxhole** | [R] |
| Relief mechanism | Vehicle-borne at night | On foot, >15 km (UA) or 30 km (RU southern front), or not at all | [R] |
| Armour | Manoeuvre and fire support | "Armor can barely reach the front now" — stays hidden; robots drive and soldiers walk | [R] |

**[CORRECTED]** The first pass characterised rotation as "indefinite." There is
now a formal rule — two months maximum on the forward edge — and the rule exists
*because* the practice was indefinite and it became a public scandal. Both facts
matter, and a game can use both: a stated rotation policy the player is supposed
to honour, and a physical reality that repeatedly makes honouring it impossible.

Rotation is the thing games get most wrong. **Relief is harder than resupply.** A
robot can carry 250 kg forward at a 1-in-8-to-1-in-3 risk of losing the robot; it
cannot carry four fresh men forward and four exhausted men back at that risk. The
supply side scales with robots and the relief side does not — which is exactly
why two men spent 165 days in a hole that was being resupplied.

The counterweight, stated bluntly in commentary: drones and robots cannot replace
infantry [C, Atlantic Council]. Ground is still occupied by people, and those
people are now fewer, more static, more isolated, and held in place longer than
any previous system would have tolerated.

## 11a. The Russian side: fleet size and employment

**[CORRECTED — the first pass listed this as unknown.]**

| Question | Answer | Conf. |
|---|---|---|
| How many types? | **32 models from at least 20 manufacturers**; at least **20 types confirmed in combat** | [R, StateWatch report, April 2026] |
| Which are in real serial production? | **Three**: Kuryer (LLC NRTK Caps, near Moscow), Varan (LLC Agency of Digital Development), Impulse-M (LLC Gumich-RTK) | [R, StateWatch] |
| How many units? | "Hundreds of units annually across more than 20 manufacturers"; the most widespread types (Kuryer, Varan, Impuls, Omich/Omich-2) are supplied "in quantities ranging from **dozens to hundreds**"; at least 50 Kuryer in the combat zone by late 2024, total production "exceeding hundreds"; **hundreds of Impulse-M delivered by early 2026** | [R]/[C] |
| Confirmed losses as a proxy for use | **At least 71 Russian ground robots knocked out by 1 April 2026, against 207 Ukrainian** — i.e. Ukraine is losing ~3× as many because it is using far more | [R] |
| **Share of Russian logistics carried by robots** | **~0.2% of total logistics volume** — "but their tactical impact on specific front sections is considerably higher than that aggregate number suggests" | **[R, Ukrainska Pravda / StateWatch]** |
| Industrial base | 300 bn rouble national robotics programme to 2030; shifted from state defence plants to private companies and PPPs; reached mass production 2024–26; **critically dependent on Chinese components** (motors, batteries, controllers, cameras, comms modules); only 10 of 20 large manufacturers are US-sanctioned as of March 2026 | [R, StateWatch] |
| What they actually do with them | Kuryer's *primary role has remained logistics, not fire support* — ammunition, equipment, supplies; 10× TM-62 mines; one casualty internally with more riding outside. MoD posted Telegram video of Kuryer resupplying the Western Group of Forces, 26 March 2026. **Training for logistics tasks takes only a few days.** Frontline trials "do not yet allow any conclusions about broad or standardized deployment concepts" | [R, Janes] |
| What they use instead | **Horses, donkeys and mules**, systematically, near Pokrovsk and on the Zaporizhzhia and Siversk axes; use became more frequent through late 2025. Russian troops complain the drone-created "grey zone" is several kilometres deep and no transport can safely pass. Starlink antennas strapped to horses to extend coverage | [R] |

**The asymmetry, stated numerically at last: Ukraine's best brigades run 70–90%
of forward logistics on robots; Russia runs 0.2% of its logistics volume on them
and fills the gap with pack animals and unarmored civilian vehicles that now
account for 90% of its vehicle losses.** That is a clean, sourced, playable
asymmetry — not two armies with the same toolkit at different tech levels, but
two different solutions to the same problem, each with its own failure mode.

The asymmetry in *evidence* is still itself a finding: Ukraine publishes monthly
mission counts out of a combat-management system, Russia publishes videos. The
0.2% figure is a Ukrainian think-tank assessment of Russian practice, not a
Russian disclosure.

---

## 12. What a strategy game has to model

### Is "trucks die inside the band, robots and bikes carry the last kilometres" right?

**Broadly yes, with six corrections that change the gameplay — two of them new
to this pass.**

**Correction 1 — the robot leg is long.** Delivery is needed 15–20 km forward and
the kill zone is 20–25 km deep and growing. The robot leg is not a 1 km dash from
a treeline; it is a 10–20 km one-way run taking hours, mostly at night. At the
game's twelve-to-one compression that is 800–1,700 map metres — long enough that
it must be a *journey with exposure along its whole length*. Concrete timing
anchor: a documented medevac covered 36.5 km in 2 h 13 m, i.e. about 16 km/h
average including the pickup; logistics UGVs run 20–60 km/h flat out but far
slower in practice, and operators sit at the controls for up to 12 hours.

**Correction 2 — trucks die outside the band.** Trucks are hit at 30–50 km depth.
The rule is not "trucks cannot enter the drone band" but "trucks cannot use
predictable routes anywhere in deep-strike reach, and cannot enter the band at
all." That gives the player a *third* zone — the interdiction belt where trucks
work but are hunted, and where netting, timing, camouflage and dispersal
decisions live.

**Correction 3 — the band is not uniformly lethal, because corridors are
buildable.** 1,383 km of netting by end-August 2026, 4,000 km targeted, 5–12
km/day, with a real but contested protective effect. **Protected routes as
attritable, constructible infrastructure** remains the strongest game mechanic in
the research set. Corridors are built at a real rate, raise survival on a
specific line, are visible and so concentrate enemy attention, and can be broken
faster than they are built — by fire (incendiaries burn holes), by weather (ice
collapses them), and by patience (drones wait underneath).

**Correction 4 — "bikes" is now better specified, and the surprise is that the
pickup is the *safer* vehicle per trip.** **[NEW]** The pickup's ~90-day
front-line lifespan implies something like a 2–3% loss per run; the UGV's
observed survival implies 10–33%. Robots are not used because they survive
better. **They are used because losing one costs $12,000 and nobody has to write
a letter.** Build the choice that way: the player trades an expensive, scarce,
relatively survivable vehicle carrying a *person* against a cheap, plentiful,
frequently destroyed machine carrying nobody. That is a genuinely interesting
decision and it is the opposite of the obvious one.

| Leg | Mass per trip | Loss per trip | Character in play |
|---|---|---|---|
| UGV | 250 kg typical (to 1 t) | 10–33%, worse on the last leg | The workhorse; attrition is a budget line |
| Heavy multirotor | ~15 kg | [U] | Keeps a position breathing; the *only* method inside 10 km |
| Fixed-wing cargo drone | 150 kg | [U] | Theatre-scale; not a trench-level answer |
| Pickup / SUV | ~500 kg | ~2–3% [I], ~90-day vehicle life | Fast, high-volume, but it is a person |
| Motorcycle / quad | 50–150 kg | [U] for logistics; catastrophic in assault | Urgent loads; high variance |
| Men on foot | 20–40 kg | [U] | The floor; never zero, never enough |
| Pack animals | [U] | [U] | The Russian answer where nothing else works |

**Correction 5 — seasonality is not flavour, it is a 30-point swing.** **[NEW]**
The NC13 commander: ground drones did ~80% of frontline logistics in July–August
and **fell to about 50%** as weather and ground deteriorated. Black soil after
rain clogs running gear and strands vehicles; the nets themselves ice up and
collapse in winter. A game with a weather system should tie the robotic share of
logistics directly to it, and let the player watch their supply architecture
degrade as the season turns. This is the best-evidenced seasonal mechanic in the
research set.

**Correction 6 — signature sets the attrition rate.** **[NEW]** The Zmiy survives
57 missions against a fleet average of about seven, and the stated reason is that
it is near-silent and low-heat. Ground-vehicle signature should feed the same
model the brief already uses for aircraft, and it should be the dominant term in
a logistics vehicle's survival roll.

### The specific mechanics worth building

1. **Consumable robots, not units, with a sector-dependent survival roll.**
   Expected life 3–10 round trips depending on sector, 1–2 on the last leg, 50+
   for a stealth-optimised platform. Cost about two attack drones. The player
   should be comfortable losing them and uncomfortable running out. No veterancy.
   The survival roll should read: base rate × sector drone density × signature ×
   season.

2. **Supply as a rate with variance, not a binary.** A position is not "supplied"
   or "cut off": it has an inflow and four separate stocks — ammunition,
   water/food, **batteries**, fortification material — drawing down at different
   rates. Batteries are the interesting one: exhaust them and the position loses
   its EW, sensors and own drones, which raises the loss rate on everything
   trying to reach it. A death spiral the player can see coming and fight.
   Calibration: a drone-sustained position receives on the order of **9 kg a
   day**; a healthy one needs several times that.

3. **Casevac as reverse logistics competing for the same vehicles, at four times
   the price.** About **one evacuation per thirty supply runs** by count, but
   **four robots consumed per evacuation** because the enemy counter-targets
   medevac specifically. Unevacuated casualties should degrade combat power *and*
   willingness to act. Chornovol's point is the one to build: *the certainty of
   rescue matters as much as the rescue* — a unit with a functioning evac chain
   should fight differently from one without, even before anyone is hit.

4. **Relief is harder than resupply.** Rotation should be a separate, riskier
   action with its own failure mode (the relief is turned back). Give the player
   a **two-month rotation rule they are expected to honour** and a physical
   reality that makes it impossible, because that is exactly the tension that
   produced Syrskyi's April 2026 order. Unrelieved positions should accumulate
   fatigue without dying, so the 165-day hole is a reachable, memorable game
   state.

5. **Fibre is for one-way jobs.** Offer it as the unjammable *assault and mining*
   option with a spool limit (5–20 km, ~400–1,700 map metres) and no return trip
   — not a strictly-better logistics link. And give the enemy a counter: lasers
   that cut the cable.

6. **Night as the master clock.** Nearly all supply movement is nocturnal; a
   single robot makes several runs a night. With optical detection already halved
   at night, the counter-play becomes thermal and acoustic detection of the night
   runs — the sensor model the brief already describes, and the Zmiy shows it is
   the right model.

7. **Degradation timescales — now research, not placeholders.** **[CORRECTED]**
   Flag these in code as *sourced, with the source*:
   - Ammunition in a firefight: hours [I].
   - Water: hard rationing and improvisation from **day 2–4** [R].
   - Combat-ineffective ("physically unable to defend our positions"):
     **7–17 days** of interrupted supply [R, 14th Brigade].
   - Untenable: **months, not weeks** — 165 days, eight months and a year are all
     documented on intermittent drone supply [R].
   - Evacuation delay: **2–12 hours with a robot** [R]; **a week on average,
     up to a month, without** [R]; days to weeks under sustained observation [R].

8. **An asymmetric opponent, not a mirror.** Give the two sides different last-mile
   solutions: one running 70–90% of forward logistics on robots and paying in
   machines; the other running 0.2% on robots and paying in unarmored vehicles,
   pack animals and men on foot. Their failure modes should differ — the robot
   army fails when the season turns and the ground clogs; the animal-and-pickup
   army fails when the interdiction band deepens past walking range.

### What is genuinely uncertain, after searching

- **Per-trip loss probability for a motorcycle, quad or pickup on a *logistics*
  run.** Fleet-level attrition and vehicle lifespans are now sourced (~90-day
  pickup life, 10 vehicles/week per brigade, 90% of Russian losses unarmored),
  and the derived ~2–3% per run is defensible arithmetic — but no source states a
  per-trip figure directly. The 2–3% is **[I]**, not [R]. All the hard motorcycle
  numbers describe assaults into contact, which is a different activity.
- **Loss rates for heavy resupply drones.** Explicitly noted as not disclosed by
  the manufacturer or the Ukrainian MoD. Still **[U]**.
- **How much netting actually reduces losses, in numbers.** Built at enormous and
  now well-documented scale; qualitatively both praised and dismissed; **no
  before/after figure found for any netted road.** Do not invent one, and do not
  attribute the R-280's 71% traffic drop or its partial recovery to netting alone
  — netting was one element of a bundle that also included gun trucks, dazzle
  paint, civilian disguise and rerouting.
- **Fuel consumption at a forward position**, in litres per day. **[U]**.
- **Russian UGV mission counts.** Platforms, models, manufacturers, confirmed
  losses and a 0.2% logistics share are now sourced; a mission tempo comparable
  to Ukraine's DELTA series does not exist publicly and probably is not
  collected. **[U]**.
- **The exact resupply/casevac split nationally.** Two unit-level datasets give
  ~3% of missions and ~1% of tonnage as evacuation; the MoD does not break out
  the national figure. The 3–13% band depends on whether "four robots per
  evacuation" generalises. **[I] from [R]**.
- **Whether the netting build-rate series is internally consistent.** 822 km
  (Jan–Jun), "over 1,170 km" (mid-May), 1,066 km (July), 1,382.6 km (end-August)
  cannot all be the same measure. At least two different scopes are in play
  (2026-only vs cumulative; Fedorov vs State Special Transport Service). The
  **direction and order of magnitude are solid; the precise figure is not.**
- **Unit sizes behind the per-position tonnage figures.** "240 kg to a fire
  support platoon" does not say how many men were on the position, so the
  per-soldier figure derived from it is soft.

---

## 13. The near-future trajectory: 2027–28

The brief asks what changes by 2027–28 and what that should do to the model.
Here is what the sources actually support, separated from what they do not.

**What is already contracted or ordered, so is near-certain:**

- **Volume keeps climbing.** Ukraine has already begun **signing 2027 contracts**
  to stabilise manufacturer pipelines [R]. Zelensky's 2026 order is 50,000 ground
  robots; 25,000 were contracted in H1 alone [C]. The DELTA mission series has
  grown 3.3× in eight months with no sign of a ceiling.
- **Consolidation, not proliferation.** Ukraine has moved to ministry-set
  specifications with a rule that no manufacturer may take more than half of any
  class [R]. Against ~150 models currently in use [R], expect the field to narrow.
  For a game: fewer, better-defined UGV classes rather than a menagerie.
- **Specialisation into roles.** 2025's UGV was a general-purpose cart. 2026 has
  produced a codified dedicated medevac robot (Vepr), armoured casualty capsules
  (MAUL), a one-tonne heavy hauler (Protector), and a deliberately
  signature-minimised logistics platform (Zmiy). The 2027–28 trajectory is
  clearly toward a family of specialists.
- **Netting reaches saturation on main routes.** 1,383 km built by end-August
  2026 against a 4,000 km target; at ~7–9 km/day the target is roughly a year of
  work. By 2027–28 the main logistics arteries in the frontline oblasts are
  netted by default, and the interesting question becomes maintenance and repair
  rather than construction — which is exactly what Russia's modular Darwin-D net
  is designed for [R].

**What is being worked on and would change the model if it lands:**

- **Autonomy is the binding constraint, and it has not been solved.** Today the
  ratio is roughly one operator per robot — Brave1's UGV lead: *"Right now, the
  UGV and operator are working together"* — with one described mission crew of
  five people for a single vehicle [R]. Convoy-following and waypoint modes
  exist on some platforms [R], and Ukraine has "millions of hours of footage and
  telemetry" as training data for the next generation of navigation and
  recognition [R], but GPS-denied navigation for ground vehicles is explicitly
  unsolved. **If one operator can run four robots, the whole logistics economy
  changes.** That is the single highest-leverage uncertainty in this document.
- **Heavy aerial logistics scales.** Windracers ULTRA (150 kg / 1,000 km, upgrade
  planned to 200 kg / 2,000 km) is already in Ukrainian service in dozens, with
  production of ten a month and a stated intent to double in two years [R]. A
  400 kg casualty-evacuation quadcopter is contracted with production from
  October 2026 [C]. Both would break the current rule that aerial resupply means
  15 kg at a time.
- **Counter-counter-measures on the fibre link.** Lasers that heat and sever
  fibre-optic cables are already described [R]. If that matures, the "unjammable"
  link stops being unjammable and fibre's niche narrows further.

**What the sources suggest will get *worse*, not better:**

- **UGV survival is falling, not rising.** The named trend in Kharkiv Oblast is
  12–15 missions → 8–9 → 3–4, attributed to denser FPV and mining fire [R]. There
  is no reason in the reporting to expect that curve to bend back. Model 2027–28
  as *more* robots doing *shorter* careers, not as a maturing fleet.
- **The kill zone keeps deepening.** 20–25 km now, 30 km expected by end-2026
  [R], and the mechanism (drone range and density) is still improving. If the
  band reaches 40–50 km, the robot leg outruns most current UGV endurance and the
  whole architecture needs another intermediate transfer node.
- **The manpower problem does not improve.** The robot push is explicitly framed
  as a response to a personnel shortage — "we don't have infantry" [R]. Robots
  reduce exposure; they do not hold ground, and the Atlantic Council line that
  drones cannot replace infantry [C] has not been contradicted by anything found
  in this pass.

**What this should do to the game model:**

1. **Make robot survival a declining curve over campaign time**, not a constant.
   A player who builds their logistics around eight-trip robots in month one
   should find them dying in three by month twelve unless they invest in
   signature reduction or corridor protection.
2. **Make autonomy a researchable step change**, not a slider. One operator per
   robot → one operator per section of robots is the difference between a
   logistics arm that scales with manpower and one that scales with production.
3. **Make netting a maintenance burden after it is built.** Construction is the
   2026 story; repair under fire, in winter, is the 2027 story.
4. **Let the band deepen as a campaign-level pressure**, forcing the player to
   re-site transfer points rearward and accept longer, more exposed robot legs.

---

## Sources

All URLs below appeared in search results during this research. **None could be
fetched directly** — the environment's egress policy denies outbound HTTPS to
every content domain, at the gateway, before the site is contacted — so
everything above comes from search-engine summaries of these pages. Verify before
quoting.

**Ukrainian UGV volume, procurement and mission counts**
- https://mod.gov.ua/en/news/robots-at-work-defence-forces-of-ukraine-more-than-triple-their-use-of-ug-vs
- https://mod.gov.ua/en/news/ug-vs-have-performed-over-100-000-frontline-missions-in-place-of-ukrainian-military-personnel-since-the-start-of-the-year
- https://mod.gov.ua/en/news/over-9-000-frontline-missions-in-march-the-defence-forces-continue-to-expand-the-use-of-ground-robotic-systems
- https://www.ukrinform.ua/rubric-ato/4161833-nrk-vid-pocatku-roku-vikonali-112-tisac-logisticnih-ta-evakuacijnih-misij.html
- https://lb.ua/tech/2026/09/07/764296_minoboroni_nrk_serpni_vikonali.html (25,143 missions in August)
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/09/ukraine-260907-ukraine-mod02.htm
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/07/ukraine-260731-ukraine-mod02.htm
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/07/ukraine-260720-ukraine-mod03.htm
- https://www.pravda.com.ua/eng/news/2026/07/31/8046740/
- https://thedefensepost.com/2026/08/25/ukraine-ground-robots-missions/
- https://united24media.com/war-in-ukraine/ukraines-ground-robots-surpass-16000-battlefield-missions-in-a-single-month-20714
- https://united24media.com/war-in-ukraine/ukraines-robot-army-has-already-completed-50000-battlefield-missions-this-year-19688
- https://oboronka.mezha.ua/en/nazemni-roboti-utrichi-zbilshili-kilkist-misiy-314957/
- https://oboronka.mezha.ua/en/viyskovi-ponad-100-tis-misiy-z-vikoristannyam-nrk-314467/
- https://euromaidanpress.com/2026/07/09/ukraine-wants-robots-doing-100-of-frontline-logistics-in-june-they-ran-nearly-17000-supply-and-evacuation-runs/
- https://www.defensenews.com/unmanned/2026/04/24/ukraine-to-field-25000-ground-robots-in-push-to-replace-soldiers-for-frontline-logistics/
- https://www.defensenews.com/unmanned/2026/05/08/ukraine-ramps-up-ground-robot-production-to-spare-soldiers-haul-ammo-and-rescue-grandma/
- https://euromaidanpress.com/2026/04/28/ukraine-plans-50000-ground-robots/ (Zelensky, 50,000 in 2026)
- https://english.nv.ua/russian-war/zelenskyy-orders-50-000-ground-robots-produced-50603575.html
- https://euromaidanpress.com/2026/08/25/for-first-time-ukraine-is-tendering-ground-robots-to-ministry-set-specifications-and-no-maker-can-take-more-than-half-of-any-class/
- https://euromaidanpress.com/2026/04/07/ukraines-robot-army-is-exploding-in-size-from-67-units-to-167-in-four-months/
- https://militarnyi.com/en/news/ministry-of-defense-of-ukraine-25-000-ugvs/
- https://united24media.com/world/ukraines-ground-robots-are-becoming-battlefield-platforms-and-procurement-is-about-to-surge-19106
- https://united24media.com/war-in-ukraine/how-ukraine-builds-its-ground-robots-for-the-kill-zone-18666
- https://interestingengineering.com/military/ukraine-plans-25000-unmanned-ground-vehicles
- https://www.jpost.com/defense-and-tech/article-893608

**The resupply / casevac split (new this pass)**
- https://www.pravda.com.ua/eng/news/2026/06/08/8038331/ (3rd Army Corps: 18,000+ missions, 4,500 t, 600+ wounded)
- https://united24media.com/war-in-ukraine/ukraine-third-army-corps-deploys-ground-robots-to-evacuate-600-wounded-soldiers-19627
- https://oboronka.mezha.ua/en/tretiy-korpus-evakuyuvav-600-biyciv-zavdyaki-nrk-312079/
- https://armyinform.com.ua/2026/06/08/majzhe-1500-tonn-vantazhiv-i-ponad-sotnya-evakujovanyh-ngu-dedali-aktyvnishe-vykorystovuye-nazemnyh-robotiv/ (National Guard: 1,477 t, 113 evacuated)
- https://24tv.ua/auto/nrk_na_sluzhbi_hvardiitsiv_statystyka_komentariv_ne_potrebuie_n72973
- https://www.kyivpost.com/post/74407 (Khartiia: 80+ missions, 25 t, 3–4 robots a night at 200–500 kg)
- https://thedefender.media/en/insights/how-khartiia-uses-ugv/

**UGV economics, attrition and lifespan**
- https://euromaidanpress.com/2026/08/28/ukrainian-robot-commander-calls-ground-robots-persian-elephants-what-army-really-needs-are-arabian-steeds-he-says/ (Kopach/"Matematyk": 3–4 missions now vs 8–9 vs 12–15)
- https://spectrum.ieee.org/ukraine-ground-drones (Zmiy 57 missions; ~35 km kill zone; ~7-mission baseline)
- https://www.pravda.com.ua/eng/news/2026/07/18/8044722/ (Ratel H: 3 FPV hits, ~50 missions)
- https://euromaidanpress.com/2026/05/12/why-combat-robots-fail-at-the-front/ (black soil, overheating, 1–2 missions near infantry, ~150 models)
- https://euromaidanpress.com/2026/08/14/armor-can-barely-reach-ukraines-front-now-so-robots-drive-and-soldiers-walk/
- https://warontherocks.com/ukraines-ground-robots-and-the-economics-of-survival/
- https://mwi.westpoint.edu/networked-for-war-lessons-from-ukraines-ground-robots/
- https://dignitas.fund/blog/building-ukraines-ugvs/ ("up to ten vehicles per week on logistics runs" before UGVs)
- https://epik.eu/publication/robots-take-risk/
- https://foreignpolicy.com/2026/04/13/russia-ukraine-war-drones-ground-robots-ugvs/
- https://dsm.forecastinternational.com/2026/06/09/how-ukraines-ground-robots-are-rewriting-the-rules-of-war/
- https://meta-defense.fr/en/2026/08/05/ukraine-ugv-ground-robots-operational-reality/
- https://english.nv.ua/nation/ukraine-s-unmanned-ground-vehicles-are-reshaping-the-war-50573852.html
- https://www.aspistrategist.org.au/better-to-use-and-lose-robots-than-soldiers-ukraines-ugv-drive/
- https://www.cnn.com/2026/04/20/europe/robots-ukraine-battlefield-drones-intl-cmd
- https://en.defence-ua.com/events/ugvs_in_ukrainian_service_how_they_overcome_kill_zone_and_change_rules_of_war-17932.html
- https://washingtonmonthly.com/2026/08/19/ukraine-war-ground-drones-ugvs/ (crew of five; 4–6 months to train; 12-hour shifts)
- https://frontliner.ua/en/supply-shortages-and-drone-threats-how-the-33rd-mechanized-brigades-unmanned-ground-vehicle-crew-operates/
- https://www.nextmsc.com/blogs/unmanned-ground-vehicles-battlefield-robotics-reshape-2026 (market-research blog; the "90% of logistics" claim originates here — weak)

**Robotic share of forward logistics**
- https://militarywatchmagazine.com/article/ukrainian-frontline-brigades-robots-70pct-logistics (28th Mech Bde, Col. Kulykivskyi, 70%)
- https://dev.ua/en/news/nrk-zabezpechuiut-maizhe-vsiu-pryfrontovu-lohistyku-do-80-pidvezennia-vantazhiv-robliat-nazemni-drony-1765958220 (NC13 "Makar": 80% in summer, 50% in bad weather)
- https://odessa-journal.com/public/up-to-80-of-cargo-deliveries-are-carried-out-by-ground-drones
- https://euromaidanpress.com/2025/12/16/ukraines-war-logistics-go-robotic-in-battle-zones/ (93rd Bde: all-robot supply, 40 t/week for five battalions)
- https://www.rebiogroup.com/insights/ugv-logistics-last-tactical-mile-scale (3rd Assault 80%; Pokrovsk/Myrnohrad 90%; 21st Regt 90%)
- https://en.topwar.ru/286288-trista-tonn-v-mesjac-nazemnye-roboty-kak-rabochaja-loshad-fronta.html ("three hundred tons a month"; Russian-language outlet, treat with care)

**Specific platforms**
- https://www.battlepolicy.com/termit/
- https://mod.gov.ua/en/news/bizon-enters-the-battlefield-defence-forces-receive-a-ugv-capable-of-carrying-up-to-300-kg
- https://militaeraktuell.at/en/ukraine-opts-for-new-ugv-beast-of-burden-bizon-l/
- https://en.defence-ua.com/news/one_ton_per_mission_rare_footage_shows_ukraines_protector_ugv_surviving_fpv_drone_strike_and_delivering_its_cargo-18923.html (Protector: 1 t/mission)
- https://war.obozrevatel.com/ukr/natsgvardiya-pochala-otrimuvati-vazhki-nrk-protector-na-scho-voni-zdatni-i-yak-vikoristovuyutsya-foto.htm
- https://www.zbroya.gov.ua/en/news/minoborony-kodyfikuvalo-modernizovanyi-robot-evakuator-vepr (Vepr medevac codified)
- https://newsukraine.rbc.ua/news/ukraine-deploys-new-vepr-ground-drone-for-1780578177.html
- https://thedefender.media/en/2025/11/maul-evac-saved-serviceman/ (MAUL armoured capsule)
- https://militarnyi.com/en/news/ukraine-spider-ugv-fire-support-system/
- https://dronexl.co/2026/03/08/ukraine-armed-ground-robots/ (K-2: first UGV battalion)
- https://www.defenseone.com/technology/2026/05/ukrainian-ground-robot-defended-position-russian-assault-six-weeks/413642/
- https://www.twz.com/news-features/ukraine-situation-report-kyiv-is-turning-soviet-era-armor-into-robotic-combat-vehicles

**Russian UGVs, fleet size and employment**
- https://www.pravda.com.ua/eng/articles/2026/05/04/8033113/ (Russia's army of robots; the 0.2%-of-logistics figure)
- https://www.pravda.com.ua/eng/news/2026/05/01/8032722/ (StateWatch: 20+ types in combat)
- https://defence-blog.com/new-report-tracks-russias-growing-combat-ground-robot-fleet/ (32 models, 20 manufacturers; Kuryer/Varan/Impulse-M in serial production)
- https://www.thedefensenews.com/news-details/Russia-Develops-Active-Ground-Robot-Industry-with-Limited-Sanctions-Exposure-StateWatch-Report-Finds/
- https://united24media.com/war-in-ukraine/russia-floods-front-lines-in-ukraine-with-20-types-of-ground-robots-18422
- https://www.janes.com/defence-intelligence-insights/defence-news/land/russia-shows-more-applications-of-kuryer-ugv
- https://www.globalsecurity.org/military/world/russia/kuryer.htm
- https://militarnyi.com/en/articles/russian-unmanned-ground-vehicles-deployed-in-the-war-against-ukraine/
- https://oboronka.mezha.ua/en/rosiyski-nrk-310870/ (71 RU vs 207 UA ground robots confirmed lost by 1 April 2026)
- https://armyrecognition.com/news/army-news/2025/russia-fields-courier-ugv-armed-with-shmel-thermobaric-rocket-module-on-the-ukraine-front
- https://oboronka.mezha.ua/en/rosteh-prezentuvav-novi-versiji-nrk-depesha-312594/
- https://defence-blog.com/robot-gunner-joins-russian-assault-units/

**Russian pack animals and improvised transport**
- https://www.pravda.com.ua/eng/articles/2026/01/30/8018731/ (horses meet Starlink)
- https://www.themoscowtimes.com/2025/03/03/russian-armys-use-of-donkeys-in-ukraine-underscores-a-staggering-equipment-shortage-a88174
- https://euromaidanpress.com/2025/02/06/russian-troops-spotted-using-horses-and-donkeys-in-ukraine-but-why/
- https://en.defence-ua.com/analysis/where_do_donkeys_and_horses_come_from_in_russias_army_and_does_this_indicate_a_shortage_of_transport_and_armored_vehicles-13445.html
- https://euromaidanpress.com/2025/12/22/drone-vs-horseman-ukrainian-fpv-strike-ends-bizarre-russian-cavalry-charge-video/

**Non-robot transport loss rates (the first pass's biggest gap)**
- https://daxe.substack.com/p/scooters-bikes-cars-and-golf-carts (unarmored = 90% of Russian vehicle losses, up from 25%)
- https://www.forbes.com/sites/davidaxe/2025/03/24/a-lot-more-russian-troops-are-attacking-in-compact-cars-vans-and-golf-carts/
- https://euromaidanpress.com/2025/11/28/russian-drones-stretch-ukraines-suv-lifeline-to-the-limit/ (~90-day pickup life; 1,000 cars lost in a month; volunteers supply 95%)
- https://frontelligence.substack.com/p/21st-century-dragoons-dissecting ("only one in several groups survives to complete the mission")
- https://www.rusi.org/explore-our-research/publications/commentary/drones-drive-battlefield-motorcycle-tactical-shift (15 motorcycles and 40 men in one Donetsk attack; petrol bikes mask drone noise)
- https://www.adaptinstitute.org/dragoons-of-ukraine-motorcycle-units-in-the-war-in-ukraine/24/04/2026/
- https://www.forbes.com/sites/davidhambling/2025/06/10/race-or-die-russian-survival-guide-for-mad-max-motorbike-assaults/
- https://en.interfax.com.ua/news/general/1183099.html (General Staff daily vehicle loss claims)
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/09/ukraine-260902-ukraine-mod02.htm (13,835 vehicles and fuel tankers claimed in August 2026)
- https://united24media.com/war-in-ukraine/ukraine-is-destroying-russias-trucks-faster-than-moscow-can-replace-them-20654
- https://euromaidanpress.com/2026/06/02/old-russian-trucks/
- https://euromaidanpress.com/2026/06/11/russian-gun-teams/ (convoy escort gun trucks)
- https://fronts.co/article/is-russia-running-out-of-trucks/

**Ukrainian motorcycles and light vehicles as equipment**
- https://euromaidanpress.com/2026/07/23/ukraines-newest-frontline-motorcycle-drives-over-anti-vehicle-mines-and-works-as-power-station/ (MUL.E)
- https://electrek.co/2026/08/11/ukraine-debuts-mine-resistant-all-wheel-drive-combat-electric-motorcycle/
- https://www.zbroya.gov.ua/en/news/shvydki-ta-nepomitni-zsu-posyliat-elektromototsyklamy-wolfstorm (WOLFSTORM)
- https://interestingengineering.com/military/stealthy-electric-motorcycle-with-62-mile-range-approved-for-combat-use
- https://militarnyi.com/en/news/ukrainian-military-forms-the-first-motorcycle-assault-company-under-the-skala-regiment/

**Fibre-optic and other control links**
- https://www.twz.com/land/unmanned-ground-vehicles-controlled-via-fiber-optic-cables-being-tested-by-ukraine
- https://en.defence-ua.com/weapon_and_tech/valuable_pros_and_unexpected_cons_of_ukraines_first_fiber_optic_ugvs-14048.html
- https://militarnyi.com/en/news/ukraine-tests-fiber-optic-fpv-drones-with-a-range-of-20-km/
- https://vgi.com.ua/en/nets-fire-teams-and-lasers-inside-the-fight-against-fiber-optic-drones/ (lasers cutting the fibre)
- https://united24media.com/war-in-ukraine/can-fiber-optic-drones-be-stopped-how-ukraine-faces-the-unjammable-threat-12502
- https://www.sedi-ati.com/disposable-fiber-optic-spools-for-tethered-vehicles/disposable-fiber-optic-spool-for-unmanned-ground-vehicles-ugv/
- https://www.atlanticcouncil.org/blogs/ukrainealert/fiber-optics-drones-have-emerged-as-critical-kit-for-both-russia-and-ukraine/

**Kill zone depth and geometry**
- https://www.pravda.com.ua/eng/news/2026/05/14/8034645/ (Brovdi: 25 km or more)
- https://euromaidanpress.com/2026/07/03/ukraines-eastern-kill-zone-is-25-km-deep-corps-commander-expects-30-by-years-end/
- https://newsukraine.rbc.ua/news/ukraine-s-frontline-kill-zone-expands-to-1786615823.html
- https://militarnyi.com/en/articles/the-kill-zone-of-modern-warfare-size-and-structure-control-and-means-of-destruction-survival-and-shifting-the-lines/
- https://www.fpri.org/article/2026/06/the-kill-zone-drone-warfare-brigade-autonomy/
- https://nationalsecurityjournal.org/the-ukraine-war-has-created-a-drone-kill-zone-that-no-solider-wants-to-get-close-to/
- https://nationalsecurityjournal.org/russian-soldiers-are-now-walking-19-miles-just-to-reach-the-front-ukraine-has-destroyed-the-trucks-the-fuel-and-the-supply-lines/
- https://united24media.com/war-in-ukraine/destroyed-logistics-forcing-russian-infantry-to-march-30-kilometers-to-frontline-positions-20359

**Anti-drone netting and protected corridors**
- https://euromaidanpress.com/2026/06/06/ukraine-has-built-822-kilometers-of-anti-drone-road-tunnels-each-kilometer-means-safer-evacuations-and-faster-supply/
- https://euromaidanpress.com/2026/07/19/ukraine-has-netted-over-1000-kilometers-of-road-thats-how-completely-drones-reshaped-front/
- https://www.ukrinform.net/rubric-ato/4161042-ukraine-equips-over-1300-km-of-frontline-roads-with-antidrone-protection.html (1,382.6 km; 213.1 km in August)
- https://www.pravda.com.ua/eng/news/2026/05/18/8035246/ ("over 1,170 km")
- https://euromaidanpress.com/2026/02/26/from-5-km-to-20-km-a-day-ukraine-triples-anti-drone-tunnel-construction-speed-in-two-months/
- https://www.ukrinform.net/rubric-ato/4155419-netting-vs-molniya-and-fpv-drones-testing-antidrone-protection.html (icing, layering, density)
- https://www.pravda.com.ua/eng/news/2025/07/23/7523078/ (Orikhiv highway, 6.5 km, first of its kind)
- https://armyinform.com.ua/en/2026/02/23/trucks-were-simply-burning-on-the-road-how-modern-approaches-help-the-state-special-transport-service-protect-roads-of-life-from-enemy-drones/
- https://www.aol.com/news/ukraines-front-line-road-nets-124338095.html ("catching drones, but gaps leave vehicles exposed to ambush")
- https://www.npr.org/2026/03/17/nx-s1-5743446/russia-ukraine-war-nets-drones
- https://www.npr.org/2026/03/10/nx-s1-5713313/ukrainian-towns-turn-to-drone-nets-to-try-stop-deaths
- https://united24media.com/latest-news/ukraine-plans-4000-km-anti-drone-net-corridors-to-protect-frontline-logistics-routes-16275
- https://united24media.com/latest-news/russia-builds-anti-drone-tunnel-roads-to-shield-troop-advance-in-eastern-ukraine-8096
- https://www.twz.com/news-features/russians-erect-mesh-net-tunnel-over-a-mile-long-to-counter-ukrainian-fpv-drones
- https://tass.com/defense/2171335 (Russian Darwin-D modular net)
- https://www.forbes.com/sites/davidhambling/2025/02/17/ukrainian-drone-pilots-unimpressed-by-russias-anti-fpv-tunnel/
- https://en.wikipedia.org/wiki/Anti-drone_mesh

**Camouflage, decoys and deception on logistics routes (previously [U])**
- https://euromaidanpress.com/2026/06/04/russia-paints-military-trucks-with-zebra-stripes/
- https://www.france24.com/en/europe/20260611-russian-trucks-covered-zebra-camouflage-ukraine-war-dazzle
- https://united24media.com/war-in-ukraine/russian-military-hides-supply-trucks-as-civilian-vehicles-to-evade-ukrainian-drones-19644

**Casualty evacuation**
- https://euromaidanpress.com/2026/08/28/russia-is-targeting-ukraines-robotic-medevac-missions/ (four robots per evacuation; six failed attempts, all robots lost)
- https://euromaidanpress.com/2026/08/22/evacuation-is-force-majeure-that-no-one-is-ready-for-in-most-ukrainian-units-and-robots-are-too-slow-to-fix-it-commander-says/ (Chornovol)
- https://euromaidanpress.com/2026/08/17/a-russian-drone-knocked-out-an-evacuation-robot-so-a-ukrainian-commander-drove-into-the-kill-zone-in-a-pickup-to-save-two-wounded-soldiers/
- https://euromaidanpress.com/2026/05/19/wounded-warriors-can-wait-months-for-evacuation-ukrainians-are-trying-to-solve-this-heres-how/
- https://euromaidanpress.com/2025/11/05/trapped-for-33-days-in-occupied-zone-wounded-ukrainian-soldier-rescued-by-ground-robot-in-daring-op-video/
- https://kyivindependent.com/under-russian-drone-onslaught-ukraines-critically-wounded-face-slim-survival-chance/
- https://www.pravda.com.ua/eng/news/2026/02/10/8020287/ (72nd Bde Bulava battalion)
- https://www.pravda.com.ua/eng/news/2026/05/22/8035937/ (1st Medical Battalion + Skelia)
- https://newsukraine.rbc.ua/news/direct-hit-didn-t-stop-it-robot-evacuated-1771564499.html (six hours under FPV fire)
- https://oboronka.mezha.ua/en/poranenogo-evakuyuvali-z-tretoji-sprobi-pislya-dvoh-dib-310172/ (third attempt, after two days)
- https://www.ukrinform.net/rubric-ato/4105611-evacuation-under-fire-ground-robots-in-vovchansk.html
- https://www.thedefensenews.com/news-details/Ukrainian-Medical-Unit-Completes-Six-UGV-Evacuations-in-One-Day-Amid-Drone-Attacks/
- https://www.aspistrategist.org.au/ground-robots-are-transforming-battle-casualty-evacuation-in-ukraine/
- https://cepa.org/article/mechanical-medics-transform-ukraines-frontline/
- https://www.npr.org/2026/05/29/nx-s1-5830382/drones-are-changing-the-face-of-warfare-including-battlefield-medicine
- https://aviationweek.com/defense/aircraft-propulsion/casualty-evacuation-drone-headed-ukraine
- https://pubmed.ncbi.nlm.nih.gov/40575542/ (point-of-injury to Role 2+ care in Ukraine; not read)

**Cut-off positions, supply failure and consumption**
- https://kyivindependent.com/121st-brigade-soldiers-report-shortages-of-food-water-command-vows-to-evacuate-them-when-security-situation-allows/
- https://prm.ua/en/240-kilograms-of-food-per-month-and-waiting-for-rotation-the-command-responded-to-the-fighters-of-the-121st-brigade/
- https://english.nv.ua/nation/ukrainian-troops-report-four-months-without-rotation-supply-shortages-50632094.html
- https://www.rferl.org/a/ukraine-russia-war-brigade-food-water-hungry/33743556.html (14th Bde, Kupyansk: up to 17 days, seven months, "physically unable to defend our positions")
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/04/ukraine-260427-rferl01.htm
- https://www.kyivpost.com/post/74634 (command shake-up after starving-troops photos)
- https://english.nv.ua/nation/new-14th-brigade-commander-vows-rotation-for-troops-left-without-food-50602864.html
- https://kyivindependent.com/how-2-soldiers-survived-165-days-trapped-on-ukraines-front-line/ (supplies gone in two weeks; one sip at 6 a.m.)
- https://www.rferl.org/a/ukraine-war-survival-donetsk-stranded/33298103.html (Klishchiivka, 80 days)
- https://www.irishtimes.com/world/europe/2026/07/28/a-year-in-a-foxhole-ukrainian-soldier-survives-beneath-the-front-line/
- https://www.aljazeera.com/news/2026/5/12/with-drone-dropped-food-on-ukraines-frontline-do-soldiers-starve ("chocolate bars, oatmeal and a bottle of water a day")
- https://www.washingtonpost.com/world/2025/09/01/ukraine-drones-resupply-trench/
- https://www.lvivherald.com/post/feeding-the-front-the-nutritional-needs-of-ukrainian-soldiers-at-war (3,500–5,000 kcal/day)
- https://quartermaster.army.mil/pwd/publications/water/Water_Planning_Guide_rev_103008_dtd_Nov_08_(5-09).pdf
- https://www.globalsecurity.org/military/library/policy/army/fm/10-52/Ch3.htm (FM 10-52 water supply planning)
- https://newuseenergy.com/pages/portable-power-for-military-drone-counter-uas-operations (2,000–3,500 Wh/day for a four-soldier drone team)

**Infantry organisation, rotation and underground works**
- https://kyivindependent.com/syrskyi-orders-soldiers-mandatory-rotation-from-front-line-positions-after-2-months/
- https://www.defensenews.com/global/europe/2026/04/30/ukraines-army-chief-shakes-up-troop-rotations-after-outcry/
- https://www.kyivpost.com/post/75270
- https://kyivindependent.com/behind-ukraines-manpower-crisis-lies-a-bleak-new-battlefield-reality-for-infantry/
- https://euroradio.fm/en/kill-zones-extended-rotations-and-robot-support-what-war-ukraine-looks-now
- https://www.atlanticcouncil.org/blogs/ukrainealert/ukraines-robot-army-will-be-crucial-in-2026-but-drones-cant-replace-infantry/
- https://cepa.org/article/ukraines-drone-war-comes-down-to-earth/
- https://thedefensepost.com/2026/01/29/ukraine-underground-fortifications-frontline/ (2 km linking 12 underground positions)
- https://mwi.westpoint.edu/digging-below-the-death-zone-the-underground-war-in-ukraine/
- https://www.rferl.org/a/ukraine-front-lines-trenches/32859005.html

**Aerial resupply**
- https://cepa.org/article/frontline-drones-doing-the-infantrys-heavy-lifting/
- https://ukrainesarmsmonitor.substack.com/p/drone-warfare-in-ukraine-baba-yaga-ae7
- https://offbeatresearch.com/2025/10/supplying-the-frontline-ukraines-drone-logistics/
- https://en.defence-ua.com/weapon_and_tech/27_sorties_in_one_night_how_ukraines_vampire_drone_hunts_russian_assault_troops-14490.html
- https://english.nv.ua/nation/ukrainian-vampire-drone-tops-2025-with-2-5m-missions-billions-in-russian-losses-50579716.html
- https://sansu.pro/defense/skyfall-vampire-how-ukraines-strike-and-logistics-drone-is-rewriting-night-warfare-and-natos-playbook/ (notes that loss rates are not disclosed)
- https://militarnyi.com/en/news/ukrainian-frontline-logistics-strengthened-by-dozens-of-heavy-ultra-drones/ (ULTRA, 150 kg)
- https://united24media.com/latest-news/british-defense-ministry-confirms-windracers-ultra-drones-for-large-scale-deployment-in-ukraine-18086
- https://www.forcesnews.com/technology/one-kind-british-ultra-drone-helping-ukraine-long-range-missions
- https://newsukraine.rbc.ua/news/silent-savior-police-present-vampire-drone-1750876919.html

**Interdiction of the other side's logistics**
- https://acleddata.com/report/ukraines-drone-campaign-dismantling-russias-military-supply-network-occupied-south (3,000–5,000 trucks burnt since mid-June; strike geography)
- https://edition.cnn.com/2026/06/20/europe/ukraine-mid-range-drones-russia-logistics-intl-cmd
- https://jamestown.org/ukrainian-mid-range-drones-target-russian-logistics/
- https://theins.press/en/politics/294084 ("Roads of death")
- https://mickryan.substack.com/p/highway-to-hell-ukraines-logistics
- https://mezha.net/eng/bukvy/797d6205_ukrainian_drones_cripple/ (R-280 corridor)
- https://en.wikipedia.org/wiki/Highway_M14_(Ukraine)
- https://www.forbes.com/sites/vikrammittal/2026/05/14/ukrainian-drones-are-modernizing-siege-warfare/
- https://www.forbes.com/sites/vikrammittal/2026/04/19/ukrainian-drones-are-cutting-off-ammo-resupply-to-russian-artillery/
- https://www.forbes.com/sites/davidkirichenko/2026/05/12/ukraines-ai-drones-are-hunting-russian-supply-lines/
