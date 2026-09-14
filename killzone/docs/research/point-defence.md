# Point defence against drones

Research for the KILL ZONE balance model. This is a video game document: every
figure below exists to justify a cooldown timer, a range ring or a build cost.
Nothing here is operational guidance.

**Confidence markers used throughout:**
`[H]` high — multiple independent reports, or a vendor spec plus corroborating
field reporting. `[M]` medium — single credible source, or vendor claim not yet
independently confirmed. `[L]` low — contested, press-release-grade, or
extrapolated. `[CALC]` — my own arithmetic from sourced inputs, shown so you can
check it.

**Scale note.** The game compresses distance roughly twelve to one. Throughout,
real metres are converted at **1 map metre ≈ 12 real metres**, so 1,000 real
metres ≈ 83 map metres. That conversion is what produces the single most
important structural finding in this document: *sensor reach and weapon reach
are not the same order of magnitude*, and the game currently treats them as
though they were.

---

## Summary

1. **The game's single "gun mount" is the wrong abstraction.** In the real
   2025–26 counter-drone fight there are at least six distinct point-defence
   families with non-overlapping failure modes. A machine-gun turret and a laser
   and an interceptor drone are not tiers of the same thing; they fail at
   different targets, different altitudes and different tempos. `[H]`

2. **Machine-gun-class point defence is an extremely short-range weapon with
   an extremely long-range sensor.** An AI turret can *see* a Shahed at several
   kilometres — Ukraine's acoustic networks pick them up at ~5 km, and a
   thermal camera sees a hot turbojet further — but it can only reliably *hit*
   inside roughly 500–1,000 m. `[H]` In map metres that is a detection ring of
   400+ and a kill ring of 40–80. The game's Gun Mount with 600 m optical reach
   is not wrong as a *sensor*; it is wrong if that number is also its weapon
   range.

3. **Altitude, not speed, is what killed the gun truck.** Russian one-way attack
   drones moved their ingress to 2,000–2,500 m and dive terminally, which put
   them above heavy machine guns and MANPADS both. Ukrainian mobile fire groups
   went from the most cost-effective drone killer in the war to crews watching
   targets they cannot engage. `[H]`

4. **Interceptor drones took over.** By February 2026 Ukraine's Commander-in-
   Chief credited drones with **more than 70% of Shahed downings**; January 2026
   set a record of 1,704 Shahed-type drones destroyed. `[H]` Unit costs cluster
   at $1,000–5,000, against a $20,000–50,000 target. `[H]`

5. **Turbojet drones are breaking the interceptor too.** Propeller interceptors
   top out around 300–315 km/h; jet Geran variants are designed for 500–600 km/h
   (observed cruise more like 300–350, sprinting at the end). Ukraine's answer is
   jet-powered interceptors exceeding 600 km/h, in combat test from mid-2026. `[M]`

6. **Directed energy is real but narrow.** Israel's 100 kW Iron Beam went
   operational and saw first confirmed combat use in March 2026 at ~$2–5 a shot,
   effective to roughly 7–10 km. `[M]` The UK's 50 kW DragonFire killed drones at
   403 mph (650 km/h) in trials and is committed to Type 45s by 2027. `[H]` The
   US Army's 50 kW Stryker laser, by contrast, disappointed its own soldiers and
   had its budget cut. `[H]` Lasers are weather-gated and need seconds of dwell
   per target; microwaves are all-weather but reach under ~1 km.

7. **Slew rate is almost never the binding constraint; re-acquisition is.**
   A gun mount needs only 3–19°/s to track an incoming drone at 500–1,000 m
   `[CALC]`, far inside any powered mount's capability. What costs time is
   settling, re-ranging and re-classification after the slew — and a passive
   EO turret with no radar pays that in full every time.

8. **Nothing in this class engages two targets at once except high-power
   microwave.** Guns, lasers and missile launchers are strictly serial; radar
   lets them *track* hundreds while *engaging* one.

9. **No published hit-probability-versus-range curve exists for any
   counter-drone gun system.** `[H]` on the absence. The derived model below
   puts single-burst kill probability against a Shahed at roughly **99% at
   200 m, 51% at 500 m, 16% at 1,000 m** — and shows that the speed penalty for
   a turbojet target is **range-dependent**, negligible at point-blank and a
   factor of three at a kilometre. The game's flat speed multiplier is the wrong
   shape.

10. **Electronic warfare is the cheapest layer and the one the threat has already
   routed around.** Shahed-class drones carry 16-element (soon 20-element)
   controlled-reception-pattern antennas and fall back to inertial navigation
   plus barometer when jammed. `[M]` Fiber-optic FPVs are immune outright. `[H]`
   The interesting 2026 development is that **high-power microwave defeats
   fiber-optic drones where RF jamming cannot** — Epirus demonstrated this in
   January 2026. `[M]`

---

## The target set these systems have to beat

You cannot size point defence without fixing the threat. Four archetypes matter:

| Target | Speed | Typical ingress altitude | Why it is hard |
|---|---|---|---|
| Small FPV / multirotor | 60–150 km/h | 5–200 m | Tiny, low, often fiber-guided |
| Shahed-136 / Geran-2 | 160–220 km/h cruise `[H]` | 1,000 m typical, 2,000–2,500 m now common, 4,000 m ceiling `[H]` | Cheap, arrives in hundreds |
| Gerbera decoy | as above | as above | Forces you to spend an effector on nothing |
| Geran-3 / Shahed-238 turbojet | designed 550–600 km/h; observed 300–350 cruise with terminal sprint `[M]` | ~3,000 m `[M]` | Compresses every engagement window |

Volume is the other axis. Russia launched **5,181 Shahed-type UAVs in May 2026**,
and a single 24-hour cycle in March 2026 carried **948 drones plus 35 missiles**.
`[H]` Roughly 1,400 jet-powered attack drones were launched in the first half of
2026 against about 180 in all of 2025. `[M]`

---

## Type by type

### 1. AI-directed machine-gun turrets

**Sky Sentinel (Ukraine).** An autonomous turret built around an M2 Browning
.50-calibre machine gun with a commercial sensor stack. It scans, classifies,
computes a firing solution and engages without a human in the loop; 360°
traverse; reported unit cost around **$150,000**. `[M]` Sensor fit is
**two thermal cameras** — one wide search, one narrow sighting — chosen because
a warm airframe against cold sky is the easiest discrimination problem
available. `[M]` Vendor and press claims put it against targets up to
**800 km/h**, which is a statement about the *tracking loop*, not about hit
probability. Early combat testing credited it with six Shahed-136 kills. `[M]`
**Its effective engagement range has not been published.** `[L]` Anyone quoting
a specific number for it is guessing.

**Bullfrog (Allen Control Systems, US).** A ~300–400 lb robotic weapon station
that turns an existing **7.62 mm M240** into an autonomous counter-drone gun,
using an electro-optical sensor plus computer vision — **fully passive
detection**, with a claimed sub-2% false-negative rate. `[M]` It fires at the
M240's standard **850 rpm** cyclic rate. `[H]` It is scoped against Class 1–3
UAS (up to 600 kg). `[M]` Selected by the US Marine Corps for integration into
L-MADIS under the Ground-Based Air Defense programme, and tested by the Army on
Abrams and Bradley. `[H]` The Marines' own framing is the cost story: **"ten
dollars per engagement instead of ten thousand."** `[M]`

**What the calibre buys you.** The M2's quoted effective range is ~1,830 m with
a maximum effective range around 2,000 m, and historically it was considered
useful against low aircraft out to roughly 1.8–2.3 km. `[H]` Those are
*area-fire* numbers against aircraft-sized targets. Against a 2–3 m drone the
binding constraint is dispersion, not ballistics.

**[CALC] Why the real kill ring is 500–800 m.** Take a mount-and-ammunition
dispersion of about 6 milliradians (a conventional figure for a vehicle-mounted
heavy machine gun; treat as an assumption, not a source). At 1,000 m that
scatters rounds over a circle of roughly 6 m radius — about 113 m² — against a
Shahed presenting perhaps 0.5 m² head-on. Single-round hit probability is then
on the order of 0.4%, so a 30-round burst yields around a 12% chance of a hit.
At 500 m the dispersion circle is a quarter the area, hit probability per round
rises to roughly 1.8%, and the same burst gives about 42%. That gradient — a
factor of three to four across a 500 m step — is why AI turrets are a
close-in weapon no matter what their tracker can follow.

**Sensor fit.** Passive EO/IR only, in both named systems. That matters twice:
it means no emissions for the enemy to home on, and it means **no direct
velocity measurement**. Range and closing rate have to be inferred from image
scale, which is exactly the measurement that a firing solution is most sensitive
to (see Q1).

**Cycle.** Detect-to-burst is advertised in "seconds"; a burst is 20–50 rounds,
under a second at 850 rpm; then re-lay and re-acquire. Call it **3–8 s per
engagement** `[CALC]`, with a 100-round belt giving **3–6 engagements** before a
reload measured in tens of seconds.

### 2. Gun trucks and mobile fire groups

Ukraine's mobile fire groups — pickups with heavy machine guns, sometimes ZU-23-2
autocannon, cued by the national air picture — were for two years the most
cost-effective Shahed killer in the war. Their decline is the clearest natural
experiment in this whole document.

Russia raised Shahed ingress to **2,000–2,500 m** and adopted a near-vertical
terminal dive. `[H]` Ukrainian reporting is blunt: crews "are increasingly
reaching a technological ceiling, often forced to observe targets they cannot
engage," and the drones now "fly at altitudes out of range of machine guns and
MANPADS." `[H]` The Prytula Foundation publicly explained the effectiveness
decline in the same terms. `[H]`

Two secondary failures compound it. First, **sensors**: mobile groups largely
lack organic radar, so they depend on a cued picture and eyeballs, and Ukrainian
analysts have pushed to equip them with compact radars — with one widely
discussed figure being a **$4,000-class** small radar. `[M]` Second, **weather**:
the cheap detection layer these crews rely on is acoustic and optical, both of
which degrade in rain and wind.

The response has been to hand mobile groups **interceptor drones** rather than
better guns, specifically because the threat is now above gun altitude — with
requirements stated around **5,000 m** of interceptor ceiling. `[M]`

### 3. Autocannon with programmable airburst

This family sits between machine guns and missiles and the game currently has no
representation of it at all.

- **30 mm (Skyranger 30, KCE revolver gun): effective range up to 3,000 m.** `[H]`
- **35 mm (Skyranger 35 / Skynex / Gepard lineage): up to 4,000 m**, with C-RAM
  capability. `[H]`
- **AHEAD airburst** rounds release ~152 tungsten sub-projectiles near the
  target, producing a cloud rather than requiring a direct hit — the decisive
  change for small drones. `[H]`

The Gepard's limitation in Ukraine was precisely the absence of this ammunition
class: twin radar-aimed 35 mm cannon, but not optimised for the modern target
set. `[M]` Skyranger turrets pair guns with short-range missiles (Stinger,
Mistral, or dedicated C-UAS interceptors), which is layering inside one turret.
`[H]`

**Sensor fit:** an organic AESA search-and-track radar plus EO/IR. This is the
first family in the list where the system *measures* target velocity rather than
inferring it, which is what makes the airburst fuze setting meaningful — fuzes
are programmed from actual measured muzzle velocity, not a nominal figure. `[M]`

Cost per engagement is the honest gap in this section: **I could not source a
per-round price for AHEAD-class ammunition** and will not invent one. `[L]`
Qualitatively it is orders of magnitude above a .50 burst and well below a
guided missile.

### 4. Interceptor drones

The dominant effector of 2026.

**Sting (Wild Hornets, Ukraine).** FPV-form interceptor, **315 km/h** recorded,
around **$2,000–2,500** a unit, claimed average hit rate **80–90%**. `[M]`
Cumulative claims: 200+ Shaheds and Gerberas by one October 2025 report, 400+
shortly after, and **7,000 kills claimed by mid-2026**. `[L] on the cumulative
figure — these are manufacturer claims.*

**Octopus (Ukraine/UK).** High-speed quadcopter interceptor: **300 km/h**,
combat radius ~**30 km**, ceiling **4.5 km**, 15 minutes endurance, 1.2 kg
payload. `[M]` Joint UK–Ukraine production was set at **1,000 units per month
from February 2026**. `[M]`

**Merops / Surveyor (Perennial Autonomy, US).** Truck-launched interceptor,
propeller-driven, **280+ km/h**, prosecuting targets autonomously from RF,
radar or thermal cues — explicitly designed for GPS/comms-degraded conditions.
`[M]` Deployed to Poland and Romania with US, Polish and Romanian forces, and
forward to the Middle East against Shahed attacks. `[H]` Lithuania bought 48 at
a reported **$15,000 per shot**. `[M]`

**Jet-capable interceptors.** Ukraine began combat testing jet interceptors
exceeding **600 km/h** in August 2026. `[M]` SkyFall's **JetKiller** claimed more
than a dozen jet-Shahed kills in combat testing with mass production planned from
August 2026; the **JEDI Shahed Hunter** was approved separately. `[M]`
A Ukrainian interceptor downed a jet-powered Shahed for the first time in 2026.
`[M]`

**Economics.** Interceptors cost $1,000–5,000 (a near-fully autonomous variant
at **$3,500**) against Shaheds at **$20,000–50,000**. `[H]` This is the only
effector family that wins the exchange ratio outright and scales to hundreds of
targets a night. NATO buyers are now shopping a low-cost interceptor market
where "low price is everything." `[H]`

**Sensor fit.** Ground radar or acoustic cue for launch; thermal seeker plus
radar track for the endgame; AI-assisted terminal guidance, with a human often
taking the last seconds. `[M]` The launcher is useless without an external cue —
this is a *networked* weapon, not a self-contained one.

### 5. Short-range missiles and adapted air-to-air missiles

**APKWS II (70 mm laser-guided rocket).** The cost-curve success story. Unit
cost reported between **$22,000 and $35,000** depending on what is counted; the
US placed a **$1.7 bn, ~55,000-round** order running to 2031 explicitly for
counter-drone use. `[H]` Ground-launched as **VAMPIRE** (four-round LAND-LGR4
launcher plus sensor turret and fire control), 14 systems delivered to Ukraine.
`[H]` Sensor fit: EO/IR turret with a laser designator — **which means cloud,
fog and smoke between launcher and target break the guidance**.

**Coyote (Raytheon).** Block 2 is a jet-turbine interceptor that kills by impact
and blast; Block 3 carries a non-kinetic electromagnetic payload for swarms.
`[M]` Thousands on order. `[M]`

**MANPADS-class.** Stinger unit cost is genuinely contested in open sources:
figures from **$120,000–150,000** up to **$400,000–480,000** appear, reflecting
different accounting and different decades. `[L]` Either way, it is the wrong
weapon to spend on a $20,000 drone, and it cannot reach the high divers reliably
either.

**Adapted air-to-air missiles.** **Gravehawk** is a containerised launcher firing
Soviet **R-73** heat-seekers, developed by the UK and Denmark, at roughly
**£1 m per system**, two delivered with 15 more planned. `[H]` Ukraine fielded
its own **FrankenSAM** variant also firing R-73s. `[M]` The logic is stockpile
arbitrage: the seeker already exists, so only the launcher and cueing are new.
Against drones the R-73's IR seeker likes a hot turbojet and dislikes a cold
piston engine — worth reflecting in game as a target-type restriction.

### 6. Directed energy

**Lasers — what is actually demonstrated.**

- **Iron Beam (Rafael, 100 kW).** Delivered to the IDF at end-2025; described as
  fully operational in 2026; **first confirmed combat use 2–3 March 2026**
  against rockets, mortars and drones. `[M]` Effective to roughly **7–10 km**
  against rockets, mortars, UAVs and short-range projectiles. `[M]` Burn-through
  **2–5 seconds** per target. `[M]` Cost per shot **$2–5**, versus $50,000+ for
  a Tamir interceptor. `[M]` Truck-mounted Iron Beam-M and helicopter variants
  were scheduled for later in 2026. `[L]`
- **DragonFire (MBDA/UK, 50 kW class).** Killed drones flying at **403 mph
  (650 km/h)** in Hebrides trials; can hit a coin-sized target at 1 km;
  **~£10 per shot**; committed to Royal Navy Type 45 destroyers **by 2027**,
  five years early. `[H]`
- **DE M-SHORAD (US Army, 50 kW on Stryker).** The counter-example. Four
  prototypes went to CENTCOM; **soldier feedback was not positive**; heat
  dissipation, electronics volume and vehicle wear in a tactical environment were
  all cited; lab and live-fire results did not transfer. `[H]` GAO judged it not
  mature; the Army cut FY25 funding by $38 m and planned ~$186 m less across
  FY26–28, and considered abandoning the Stryker as the host platform. `[H]`

The honest reading: **a laser on a fixed site with grid or generator power and a
big thermal plant works; a laser on a manoeuvre vehicle does not yet.** That
distinction is directly playable.

Weather is the standing limitation — atmospheric attenuation, turbulence, fog,
dust and smoke all degrade beam quality and effective range. `[H]`

**High-power microwave.**

- **Leonidas (Epirus).** Four IFPC-HPM systems delivered to the Army by May 2024
  under a $66 m contract; a further **$43 m** for Generation II aimed at
  **doubling effective range and increasing power**. `[H]` A Leonidas autonomous
  ground vehicle was unveiled in March 2026; the Marines moved to the
  vehicle-mounted **HAVOC** variant. `[M]`
- **The 2026 headline:** in January 2026 Epirus demonstrated Leonidas defeating a
  **fiber-optic-guided UAS** — the first known case of electromagnetic attack
  killing a drone that carries no radio link at all. `[M]` HPM does not jam the
  link; it couples energy into the airframe's electronics.
- **Range is the catch.** Effective range is on the order of **hundreds of metres
  to low thousands**, typically cited as under ~1 km, because energy density
  falls below the kill threshold fast. `[M]`
- **Weather is not the catch.** Microwaves are substantially less affected by
  fog, dust, smoke and moderate rain than lasers. `[H]`
- Constraints: prime power confines HPM to vehicles or fixed sites; hardened
  autopilots and Faraday-shielded payloads survive exposure; and the beam has to
  be deconflicted from your own radios and sensors. `[M]`

**One burst covers a cone, not a point.** That is the entire reason HPM exists in
this list: it is the only effector whose cost per target *falls* as the swarm
gets bigger.

### 7. Net-based and close-in kinetic

- **SkyWall 100 / SkyWall Patrol (OpenWorks):** shoulder-launched compressed-air
  net capture, effective to roughly **100 m**. `[M]`
- **Stationary autonomous net turret (e.g. ParaZero DefendAir):** coverage to
  **100 m**, net sizes 16–100 m², operating autonomously with radar connectivity
  plus optical tracking. `[M]`
- **Airborne net interceptors (Fortem DroneHunter F700):** fires a tethered
  Kevlar net from an interceptor aircraft, entangles rotors and lowers the target
  under parachute; engagement ranges **100–200 m**; **4,500+ captures**
  claimed. `[M]` Air-launched net configurations reach out to ~2 km. `[M]`

This family only works against slow rotorcraft. Nothing in it touches a
one-way attack drone at 200 km/h, let alone 600. Its real value is **capture
rather than destruction** — recovering an intact airframe, and not raining
fragments over your own position. That is a distinct game verb.

### 8. Electronic warfare as point defence

EW remains the cheapest, widest-coverage layer, and the one with the most
clearly mapped blind spots.

**What it cannot touch:**

1. **Fiber-optic FPVs.** A hair-thin optical cable carries the control link;
   RF jamming has literally nothing to attack. By early 2026, 35+ Ukrainian
   manufacturers produced fiber drones and Russian adoption reached 30–50% in
   some front-line units. `[M]`
2. **CRPA-equipped satnav users.** The Shahed-136 pairs civilian GPS/GLONASS
   with **16-element controlled-reception-pattern antennas**, with 20-element
   arrays expected to follow; field data suggests the 16-element array can null
   up to 15 spoofers at once. `[M]`
3. **Inertial fallback.** With satnav fully denied, the drone switches to an
   internal INS, holding altitude on a barometer and heading on a compass. `[M]`
   Accuracy degrades, but against a city-sized target it does not need to be good.
4. **Autonomous terminal guidance.** Anything doing its own final targeting from
   onboard imagery is unaffected by link denial.
5. **Ballistic and unguided threats.** Out of scope entirely.

And EW is not free: a transmitting jammer is a beacon. The game already encodes
this correctly — RF signature 100 for a transmitting jammer is the right
instinct and should stay.

---

## Comparison table

Ranges are real metres, with map metres at 12:1 in brackets.

| Type | Effective engagement range | Can hit | Cannot hit | Cycle / rate | Cost per engagement | Sensor fit |
|---|---|---|---|---|---|---|
| **AI MG turret** (Sky Sentinel, Bullfrog) | ~500–1,000 m [40–85] `[CALC]` | Multirotors, fixed-wing recon, Shahed-class **below ~1 km altitude** | Anything above ~1,200 m; hardened; very fast crossers at range | Burst <1 s at 850 rpm; 3–8 s per engagement; 3–6 engagements per belt | **~$10** `[M]` | Passive EO/IR only (twin thermal on Sky Sentinel); no velocity measurement |
| **Gun truck / mobile fire group** | ~500–1,000 m [40–85] | Low-flying Shaheds, decoys | Anything at 2,000–2,500 m ingress `[H]` | Human-limited, 5–15 s | ~$10–50 | Eyeball + cued national picture; radar usually absent |
| **Autocannon w/ airburst** (Skyranger 30/35, Skynex) | 3,000 m (30 mm) / 4,000 m (35 mm) [250 / 330] `[H]` | Everything from FPV to cruise missile within envelope | Targets above ~4 km; saturation beyond magazine | ~2 s per target, revolver cannon | Unsourced; between a burst and a missile `[L]` | Organic AESA search/track + EO/IR; measured velocity |
| **Interceptor drone** (Sting, Octopus, Merops) | 30 km combat radius, 4.5 km ceiling [2,500 / 375] `[M]` | Shahed/Gerbera class up to ~300 km/h | Jet drones at 500–600 km/h (until jet interceptors) | Launch-to-intercept in minutes; one-shot | **$1,000–5,000**; Merops $15,000 `[H]` | External radar/acoustic cue; thermal + radar terminal; AI guidance |
| **Jet interceptor** (JetKiller, JEDI) | Comparable radius, >600 km/h `[M]` | Jet Gerans | Unproven at scale | One-shot | Higher; not published `[L]` | As above |
| **Guided rocket** (APKWS / VAMPIRE) | ~5,000 m class [415] `[M]` | Shahed-class, helicopters, some cruise missiles | Targets obscured by cloud/smoke — **laser designation required** | ~4 rounds ready, seconds apart | **$22,000–35,000** `[H]` | EO/IR turret + laser designator |
| **Adapted AAM** (Gravehawk R-73, FrankenSAM) | Several km `[L]` | Hot targets — turbojets, aircraft, cruise missiles | Cold piston-engine drones (weak IR contrast) `[L]` | Seconds | System ~£1 m; missile from stock `[H]` | IR seeker + external cue |
| **Laser, fixed site** (Iron Beam 100 kW) | 7–10 km [580–830] `[M]` | Drones, rockets, mortars, short-range missiles | Anything in fog, rain, dust, smoke; multiple simultaneous targets | **2–5 s dwell** per kill + slew `[M]` | **$2–5** `[M]` | Fire-control radar + EO track |
| **Laser, mobile** (DE M-SHORAD 50 kW) | Similar in theory; **immature in practice** `[H]` | — | Heat, power and wear defeat it on a manoeuvre vehicle `[H]` | — | — | — |
| **HPM** (Leonidas, HAVOC) | **<1 km** [<85] `[M]` | **Whole cone at once**, including **fiber-optic drones** `[M]` | Hardened/Faraday-shielded airframes; anything outside the cone; non-electronic threats | Sub-second pulse, effectively unlimited shots | Electricity | Cued by radar/EO; needs EM deconfliction with own forces |
| **Net capture** (SkyWall, DefendAir turret) | **100 m** static [8]; 100–200 m airborne | Slow multirotors | Anything fast or high | Seconds, single-shot | ~$1,000s `[L]` | Radar + optical, autonomous in turret form |
| **EW / jamming** | Hundreds of m to km | Radio-linked and satnav-dependent drones | **Fiber-optic, CRPA-equipped, INS fallback, autonomous terminal** `[H]` | Continuous | Power only | Passive RF detection; **emits, so it is detectable** |

---

## The four questions

### Q1. Can a gun turret realistically engage a turbojet drone at 370–600 km/h?

**Yes, but only inside a few hundred metres, and only with four things true.**

Vendors already claim the tracking: Sky Sentinel is described as engaging targets
to 800 km/h `[M]`, and DragonFire — a different effector, but a comparable
tracking problem — actually killed 650 km/h drones in trials `[H]`. Tracking is
not the constraint. Fire control is.

**[CALC] The arithmetic.** A .50 BMG round leaves at roughly 890 m/s and averages
perhaps 700 m/s over the first kilometre, so time of flight is about **1.4 s at
1,000 m** and **0.65 s at 500 m**. A 600 km/h target moves at 167 m/s. Required
lead is therefore ~233 m at 1,000 m and ~108 m at 500 m.

Now the error budget. A **5% error in estimated target velocity** puts the burst
8.4 m/s × 1.4 s ≈ **11.7 m off** at 1,000 m — five airframe-widths of miss. To
land inside a 2.5 m target you need velocity known to roughly **1%**. Published
fire-control analysis makes the same point from the other direction: a 0.03 s
error in projectile time of flight — half a percent — misses a Mach 1 target by
20 m. `[M]`

So the four conditions:

1. **Short range.** Halve the range and you quarter the dispersion area and halve
   the sensitivity to velocity error. The engagement is viable at 300–600 m and
   marginal beyond 800 m.
2. **A velocity-measuring sensor.** Passive EO alone infers closing rate from
   image scale change, which is not good to 1%. This is the strongest argument
   for pairing AI gun turrets with a **small Doppler radar** — and it is exactly
   what Ukrainian analysts have pushed for with the ~$4,000 compact radar. `[M]`
3. **Volume of fire.** 850 rpm exists to buy statistics. Airburst ammunition
   solves the same problem by removing the need for a direct hit. `[H]`
4. **Enough window.** `[CALC]` A head-on 600 km/h target covers 167 m/s. From a
   1,500 m detection to a 700 m weapon edge is 4.8 s of tracking, then about
   **2.4 s of firing** before it is inside minimum range or past. The same
   geometry against a 200 km/h Shahed gives about 14 s of firing. **A turbojet
   drone cuts the gun's firing window by roughly a factor of three.** That single
   ratio is the most game-relevant number in this document.

### Q2. What altitude puts a target out of reach?

- **Above ~1,000–1,200 m: machine-gun-class point defence is finished.** The M2's
  quoted effective range is 1,830 m `[H]`, but that is slant range against an
  aircraft-sized target, and hit probability against a drone collapses long
  before it. Ukrainian mobile fire groups lost the fight when ingress moved to
  **2,000–2,500 m**. `[H]`
- **Above ~3,000 m: 30 mm is finished.** `[H]`
- **Above ~4,000 m: 35 mm is finished** — and 4,000 m is also the Shahed-136's
  stated service ceiling, which is not a coincidence. `[H]`
- **Above ~4,500 m: current quadcopter interceptors are finished** (Octopus
  ceiling). `[M]` Ukrainian units have asked for **5,000 m**. `[M]`
- **Above that:** SAMs, crewed aircraft, or nothing.

The game's existing rule — ground weapons lose 40% of reach shooting upward — is
directionally right but too gentle and too smooth. The real behaviour is a
**hard ceiling per weapon class**, not a taper.

### Q3. How many simultaneous targets, and what does saturation look like?

**Detection is not the bottleneck.** Modern C-UAS radars handle hundreds of
tracks; Teledyne's Cerberus XL is reported to detect **up to 500 targets**. `[H]`
Ukraine's acoustic networks — Sky Fortress at ~14,000 sensors, Zvook alongside,
**24,000+ listening posts for under $5 m combined** — detect Shaheds at about
**5 km** and cruise missiles further, passively. `[M]` At **$400–1,000 per
sensor** `[M]`, wide-area detection is the cheapest thing in this document.

**Effectors are the bottleneck, and most of them are strictly serial:**

| Effector | Simultaneous engagements | Magazine before reload |
|---|---|---|
| MG turret | **1** | 3–6 engagements (100-round belt) `[CALC]` |
| Autocannon | **1** | tens of engagements |
| Laser | **1**, for 2–5 s each `[M]` | unlimited while powered and cool |
| Guided rocket | 1 at a time | ~4 ready rounds (VAMPIRE) `[H]` |
| Interceptor drone | limited by launchers and crews | one airframe per intercept |
| **HPM** | **whole cone at once** `[M]` | unlimited |

**Saturation therefore looks like this:** the picture stays perfect while the
shooting stops. Operators watch tracks they cannot service. A 100 kW laser
needing 3 s per kill can theoretically service ~20 targets a minute in ideal
weather with zero slew time; in a 948-drone night that is not the binding
problem — geography is, because each battery only covers its own 7–10 km bubble.

The empirical shape of it: Russia scaled from ~200 launches a week to 1,000+ a
week during 2025, and to **5,181 in May 2026** `[H]`, deliberately mixing
ballistic missiles, cruise missiles, strike UAVs and **decoys** to fragment the
defensive response. `[H]` Gerbera decoys are a magazine-depletion weapon: every
interceptor spent on a decoy is one not available for a warhead.

**Design consequence:** saturation should be modelled as **effector-channel
exhaustion and per-engagement time**, not as a detection failure. A point-defence
unit in the game should have an explicit *engagement channel count* (almost
always 1) and a *reload*, and decoys should be a real unit that consumes them.

### Q4. What does layering buy that more of one type does not?

Each family has a *different* failure mode, and stacking two of the same never
covers a gap:

| Layer | Fails against |
|---|---|
| MG turret | altitude above ~1 km; speed above ~400 km/h at range |
| Autocannon | above 4 km; cost at volume |
| Interceptor drone | jet targets; needs an external cue; one-shot |
| Guided rocket | cloud, smoke, fog (laser designation); cost per shot |
| Laser | weather; serial engagement; power and cooling |
| HPM | range under 1 km; shielded airframes |
| Net | anything fast or high |
| EW | fiber-optic, CRPA, INS, autonomous terminal |

Three concrete pairings that cover each other:

1. **HPM + gun.** HPM thins a swarm inside 1 km at zero marginal cost; the gun
   finishes the hardened survivors. Adding a second gun instead just gives you two
   turrets that each engage one target at a time.
2. **Laser + autocannon.** The laser is free per shot but fails in fog; the
   autocannon is weather-indifferent but has a finite magazine. Two lasers both
   go blind in the same weather.
3. **Interceptor drone + EW.** EW is free and wide but blind to fiber and INS;
   interceptors are cued, costly per shot, and hit what EW misses. Two jammers
   are still two jammers that fiber drones ignore.

This is not theory. Skyranger turrets ship guns *and* missiles in one mount
`[H]`; Iron Beam is integrated alongside Iron Dome rather than replacing it
`[M]`; Bullfrog is being folded into L-MADIS as one element of a larger GBAD
architecture `[H]`. Everyone building this is building layers.

---

## Traverse, slew and re-acquisition

**Sourcing warning.** This section is the weakest-sourced in the document. Slew
rates for counter-drone mounts are either unpublished (Sky Sentinel, Bullfrog —
neither vendor states a traverse rate) or buried in datasheets I could not reach
in this session. Figures below marked `[GEN]` are general engineering knowledge
of the class of machine, not citations, and should be treated as design
defaults to be replaced if a real datasheet turns up. What *is* well founded is
the reasoning about which constraint actually binds — and it is not the slew
rate.

### Azimuth and elevation rates by class

| Class | Azimuth | Elevation | Confidence |
|---|---|---|---|
| Light AI EO turret (Sky Sentinel, Bullfrog) | ~100–200°/s | ~60–120°/s | `[GEN]` — unpublished; inferred from a 300–400 lb station with "precision robotics" and a machine-gun-weight payload |
| General remote weapon station (CROWS/Protector class, ground-target optimised) | ~60°/s | ~30–45°/s | `[GEN]` |
| Towed AA gun, manual (ZU-23-2 class) | ~30–50°/s, crew-limited, poor settling | ~20–30°/s | `[GEN]` |
| Self-propelled AAA turret (Gepard, Skyranger) | ~90–120°/s | ~45–60°/s | `[GEN]` |
| Naval CIWS (Phalanx class) | ~100°/s+ | ~80°/s+ | `[GEN]` |
| Light SHORAD missile launcher | ~45–60°/s | ~30–45°/s | `[GEN]` |

Two robust structural rules fall out, and these are worth more to the game than
any single number:

1. **Elevation is roughly half azimuth rate, in every class.** Elevation drives
   are lifting mass against gravity through a shorter lever. Whatever azimuth
   figure you pick, halve it for pitch.
2. **Mass sets the rate.** A machine gun on a light robotic mount is the
   fastest thing on the list; a 35 mm turret with an ammunition feed and a radar
   is slower; a missile rail heavier still.

### Why slew rate is usually the wrong constraint

**[CALC] Angular rate demand rises as range falls.** A target crossing at speed
`v` and slant range `R` demands a tracking rate of `v/R`:

| Target | 1,000 m | 500 m | 300 m | 150 m |
|---|---|---|---|---|
| Shahed at 180 km/h (50 m/s) | 2.9°/s | 5.7°/s | 9.5°/s | 19°/s |
| Jet Geran at 600 km/h (167 m/s) | 9.5°/s | 19°/s | 32°/s | 64°/s |

Every one of those is inside every mount's slew capability. **A gun turret is
never slew-limited against an incoming drone at useful range.** What it is
limited by is:

- **Settling.** A fast slew excites the mount; the tracker has to re-converge
  before the firing solution is trustworthy. `[GEN]` Budget **0.3–1.0 s** after
  a large re-lay, and note that it is longer on a light mount than a heavy one —
  the exact opposite of the slew-rate ordering.
- **Re-acquisition and re-ranging.** After a slew the system must reclassify the
  new target and, critically, re-establish range and closing rate. For a passive
  EO turret with no radar that is the slow step, because range from image scale
  needs several frames of growth to converge. Sky Sentinel and Bullfrog are both
  fully passive. `[M]`
- **Handover from search to track.** Systems with a separate search sensor
  (Skyranger's AESA, Leonardo DRS multi-mission radars handling hundreds of
  tracks `[H]`) pay almost nothing here, because the next target is already
  tracked before the gun moves. Systems without one pay the full search cost
  again.

That last point is the real design lever. **The sensor architecture, not the
motor, decides how fast a mount can switch targets.**

### Does changing elevation band cost extra?

Mechanically, barely. `[CALC]` Climbing 30° of elevation at 45°/s is 0.67 s;
at 90°/s it is 0.33 s. So the game's flat **two-thirds of a second is a
reasonable mechanical figure for a heavy mount and roughly double the truth for
a light one.**

But three real costs are missing from that model:

1. **Slant range grows with elevation.** A target at 800 m ground range and
   2,000 m altitude is at 2,154 m slant range — beyond every machine gun on the
   list. Elevation change is not a time cost, it is a *range* cost.
2. **Angular rate at high elevation is punishing.** A target passing near
   overhead sweeps through large angles quickly regardless of its speed, and near
   the zenith azimuth rate demand goes to infinity — the classic gimbal problem.
   This is the only place a gun mount genuinely does hit its slew limit.
3. **Re-ranging dominates.** Changing band usually means changing target, which
   means paying the re-acquisition cost above, not the motor cost.

**Recommendation:** replace the flat 0.67 s with **0.3 s mechanical + 0.5 s
re-acquire** for light AI turrets and **0.6 s + 0.5 s** for autocannon and
missile mounts, make it asymmetric (upward costs more than downward, because the
target overhead is rate-demanding), and add a **hard ceiling** per weapon class
(Q2) which matters far more than any of this.

### Can any of them engage two targets at once?

**Guns: no. Strictly one at a time.** One barrel, one line of sight, one firing
solution. This is true of every gun system in this document.

**But tracking and engaging are different verbs**, and the game should separate
them:

| System | Tracks simultaneously | Engages simultaneously |
|---|---|---|
| Passive AI turret | a handful (single EO field of view) | 1 |
| Autocannon with AESA | hundreds `[H]` | 1 |
| Laser | many (radar) | **1**, for 2–5 s each `[M]` |
| Missile launcher | many | 1 per guidance channel; command-guided and laser-designated systems have **1–2 channels**, fire-and-forget IR seekers can have several missiles in flight at once `[GEN]` |
| Interceptor drone pad | many | as many as you have airframes airborne and operators |
| **HPM** | — | **everything in the cone, in one pulse** `[M]` |

Bullfrog's demonstrations against "multi-drone engagements in rapid succession"
`[M]` are exactly that — *succession*, not simultaneity. The phrase is worth
reading carefully; it is the honest description of a serial weapon.

**Design consequence:** give every unit an explicit `engagement_channels` field.
It is 1 for almost everything, several for interceptor pads, and "all in arc"
for HPM. That single field is the cleanest way to encode Q3 and Q4 in the model.

---

## Hit probability against range

### What is actually published

**Plainly: there are no published Pk-versus-range curves for any counter-drone
gun system in open sources I could reach.** `[H]` on the absence — vendors
publish engagement *envelopes* and anecdotes, not probability curves, because Pk
is the number that sells or sinks a programme. Three data points are all the
open record gives:

1. **Bullfrog: "ten dollars per engagement."** `[M]` **[CALC]** At roughly
   $0.60–0.90 per 7.62 mm NATO round, $10 is **11–17 rounds**. If that describes
   a *successful* engagement, it implies a per-round hit probability around 6–9%,
   which corresponds to a very short range against a small slow target. It is a
   marketing round number and should not be over-read, but it is the only
   ammunition-per-kill figure in the public record.
2. **Sky Sentinel destroyed six Shahed-136 drones in early combat testing.** `[M]`
   No rounds-expended figure was published.
3. **Bullfrog's "less than two percent false-negative rate."** `[M]` This is
   widely misquoted as an accuracy figure. **It is a detection metric** — how
   often the system fails to notice a drone — **not a hit metric.** Do not tune
   the game against it.

The one genuinely useful published constraint is the fire-control sensitivity
figure: **a 0.03 s error in projectile time of flight, about half a percent,
misses a Mach 1 target by 20 metres.** `[M]` That is the physics the whole
section rests on.

### A derived Pk model

Everything below is `[CALC]`. Inputs: Gaussian angular error `σ`, combining
dispersion (**5 mrad** assumed for a rigid-mounted M2 — an assumption, not a
source) with aiming bias from velocity-estimate error (**5%**, appropriate to a
passive EO tracker with no Doppler). Per-round hit probability
`p = 1 − exp(−r²/2σ²R²)`; burst probability `1 − (1−p)^n` for a 20-round burst.
Effective vulnerable radius: **0.3 m** small quadcopter, **0.8 m** Shahed-class.

**Single-burst kill probability (20 rounds, .50-calibre class):**

| Range | Small quad @ 60 km/h | Shahed @ 180 km/h | Shahed @ 500–600 km/h |
|---|---|---|---|
| 200 m | **~60%** | **~99%** | **~78%** |
| 500 m | **~13%** | **~51%** | **~22%** |
| 1,000 m | **~3.5%** | **~16%** | **~5%** |

**Expected rounds per kill (same model):**

| Range | Small quad | Shahed @ 180 | Shahed @ 500–600 |
|---|---|---|---|
| 200 m | ~32 | ~5 | ~14 |
| 500 m | ~200 | ~28 | ~82 |
| 1,000 m | ~550 | ~116 | ~370 |

A 100-round belt therefore buys **three Shahed kills at 500 m and less than one
at 1,000 m.** That is the ammunition economy the game should feel.

### The speed penalty is range-dependent, and this is where the game's shape is wrong

Compare the 180 km/h and 500–600 km/h columns:

| Range | Pk ratio, slow ÷ fast |
|---|---|
| 200 m | **1.3×** |
| 500 m | **2.3×** |
| 1,000 m | **3.1×** |

At point-blank, speed barely matters — time of flight is so short that even a
sloppy velocity estimate produces a small lead error. At a kilometre it is the
dominant term. **The game currently scales hit chance inversely with target
speed as a flat multiplier across all ranges. That is the wrong shape.** Speed
should enter as `velocity_error × time_of_flight`, i.e. **proportional to speed
*and* to range**, combined in quadrature with the range-independent dispersion
term. Practically: make the speed penalty near-zero at 20% of max range and
roughly a factor of three at full range.

### The range falloff is also the wrong shape

Per-round hit probability does fall roughly as `1/R²` — the game's instinct is
right in the tail. But **burst** probability is `1 − (1−p)^n`, which saturates.
The resulting curve is:

- **Flat and near-certain** inside about 25% of effective range,
- **collapsing steeply** through the middle band,
- with a **long thin non-zero tail** past nominal maximum range — you do
  occasionally hit at 1,500 m, just rarely.

A parabola of the form `1 − (R/Rmax)²` gets the flat head roughly right, but it
falls too gently through the middle and then **hits exactly zero at Rmax**, which
is wrong in a way players notice: there is no lucky long-range kill, ever.

**Recommended replacement:**

```
Pk = 1 - exp( -k * (R_ref / R)^2 * size_factor / speed_factor(R) )
```

with `k` tuned so that `Pk = 0.5` at the class's design range (500 m for a
.50-calibre mount, 2,000 m for a 30 mm airburst mount), and
`speed_factor(R) = 1 + c * v * R`. This gives saturation near the gun, an
inverse-square middle band, and a non-zero tail, in one expression.

### Airburst changes the model, not just the number

The entire reason 30/35 mm AHEAD exists is that it **removes the requirement for
a direct hit** — 152 tungsten sub-projectiles released near the target `[H]`.
In model terms the effective target radius jumps from ~0.8 m to something like
the burst cloud radius, several metres. That is why an autocannon holds useful
Pk out to 3,000–4,000 m `[H]` while a machine gun does not hold it past 1,000.
**Airburst should be a target-radius multiplier in the game, not a range
multiplier.** Getting that right is the single cleanest way to make the
autocannon unit feel different from the machine gun rather than simply better.

### Proportion of misses, and what it costs

No open source publishes a miss rate. `[H]` on the absence. From the derived
model, at a gun's design range of ~500 m against a Shahed, **roughly half of
bursts miss**; against a small quadcopter at the same range, **seven out of
eight miss**; against a turbojet target, **four out of five**.

Because the game now makes a miss cost the mount its firing cycle, chain that
against Q1's window arithmetic: a head-on 600 km/h target gives about **2.4 s of
firing**, which at a 3–4 s engagement cycle is **one burst, perhaps two.**
Multiply through and a single AI gun turret has something like a **5–20% chance
of stopping a single turbojet drone that flies directly at it**, and perhaps
**50–70% against a subsonic Shahed at 500 m**. Those numbers are `[CALC]` and
should be treated as the shape of the answer rather than the answer, but they
match the observed reality that Ukrainian mobile fire groups and AI turrets are
credited with kills in ones and sixes while interceptor drones are credited with
70% of a national total. `[H]`

### The 30% upward-fire cut

The flat 30% penalty is a defensible proxy but it is modelling the wrong thing.
Shooting upward, the real effects are:

1. **Slant range exceeds map range.** A target 800 m away on the ground plane at
   2,000 m altitude is 2,154 m away in reality. This alone accounts for most of
   the observed penalty and it is already in the Pk curve if you use slant range.
2. **Angular rate demand rises near the zenith** — the only case where the mount
   is genuinely rate-limited.
3. **A hard ceiling**, which no multiplier can express (Q2).

**Recommendation:** drop the flat 30%, compute Pk on **slant** range, add the
per-class **ceiling**, and keep a small residual penalty (10–15%) for the
rate-demand effect at high elevation. That reproduces the real behaviour —
cheap point defence that is fine against low targets and simply cannot touch
high ones — instead of smearing it into a uniform tax.

---

## Where the current model is wrong, and what to build instead

### Where the existing numbers break

1. **Gun Mount's 600 m optical reach is a detection figure being used as a
   weapon.** Keep it as a sensor; add a separate, much shorter **engagement
   range** of ~70 map metres.
2. **There is no altitude ceiling.** The 40% upward-fire penalty is too soft.
   Real point defence has hard per-class ceilings (Q2).
3. **Acoustic detection is undervalued.** The game gives Gun Mount 130 m of
   acoustic reach. Sky Fortress detects Shaheds at ~5 km real — over 400 map
   metres — passively and for a few hundred dollars a node. `[M]` Acoustic
   reliability of 52 is defensible for a single sensor; a *network* of them is
   far better, which argues for a cheap networked acoustic unit whose value is
   coverage rather than accuracy.
4. **Nothing in the game distinguishes an effector that engages one target at a
   time from one that engages a cone.** That distinction is the whole of Q3/Q4.
5. **90°/s azimuth for a gun turret is a sound default**, but it should not be
   one number: ~150°/s for a light AI EO turret, ~90°/s for an autocannon
   turret, ~45°/s for a crewed gun truck. **45°/s for a missile battery is
   right** — and largely irrelevant, since a missile does its own turning after
   launch and the launcher only needs coarse pointing.
6. **Elevation should be half the azimuth rate** in every class.
7. **The flat 0.67 s band-change penalty is about right mechanically for a heavy
   mount and roughly double the truth for a light one** — but it is modelling
   the wrong cost. Replace with mechanical time plus a **0.5 s re-acquisition**
   charge, and make elevation matter through **slant range and a hard ceiling**
   rather than a timer.
8. **The hit-probability curve is the wrong shape twice over**: the range
   falloff should saturate near the gun and keep a thin non-zero tail past
   nominal maximum, and the speed penalty should scale with range rather than
   applying flat. The 30% upward-fire cut should be replaced by slant-range
   geometry plus a ceiling.

### Suggested replacement units

Range in map metres (12:1). Costs are relative, for the game's economy to scale.

| Unit | Engage | Ceiling | Channels | Cycle | Magazine / reload | Cost/shot | Notes |
|---|---|---|---|---|---|---|---|
| **AI Gun Turret** (Sky Sentinel class) | 70 | low band only | 1 | 4 s | 5 shots / 20 s | trivial | Passive thermal, no emissions; hit chance falls sharply with range; **firing window ×⅓ vs turbojets** |
| **Gun Truck** | 60 | low band only | 1 | 8 s | 5 / 25 s | trivial | Mobile, cheap, no organic radar; needs a cue to be useful |
| **Airburst Autocannon** (Skyranger class) | 280 | mid band | 1 | 2 s | 20 / 40 s | moderate | Organic radar; the workhorse; expensive to build |
| **Interceptor Pad** (Sting class) | launches to 1,500 | high band, 375 ceiling | 2 in flight | 15 s launch | 6 airframes, slow rebuild | low | Consumable; needs external cue; **cannot catch turbojet targets** |
| **Jet Interceptor Pad** | launches to 1,500 | high band | 1 in flight | 25 s | 3 airframes | moderate | The only cheap answer to turbojets; 2027+ availability |
| **Rocket Battery** (VAMPIRE class) | 400 | all bands | 1 | 5 s | 4 / 30 s | high | **Disabled by fog/smoke** — designation required |
| **Laser Post** (Iron Beam class) | 600 | all bands | 1 | 3 s dwell + 2 s slew | unlimited | ~zero | **Needs a power link; disabled by weather/smoke**; fixed site only |
| **Microwave Emitter** (Leonidas class) | 80, in a 90° cone | low/mid | **all in cone** | 1 s pulse | unlimited | ~zero | **Kills fiber-optic drones**; no effect on shielded targets; degrades friendly sensors in arc |
| **Net Turret** | 10 | low only | 1 | 6 s | 2 / 30 s | low | **Captures** rather than destroys; slow rotorcraft only |
| **Jammer** (existing) | wide | all | area | continuous | — | zero | No effect on fiber, INS or autonomous targets; RF signature 100 |

Two systemic additions make the above work:

- **A cue network.** Interceptor pads and gun trucks should be materially worse
  without a Radar Mast or an acoustic net in range. This is true in reality and it
  creates the layering decision the game wants.
- **An acoustic sensor net unit** — cheap, passive, wide, unreliable per node,
  strong in aggregate. It is the single most cost-effective thing in the real
  fight and it does not exist in the model.

---

## What changes by 2028

1. **Guns stop being the answer to strategic drones and become the answer to
   tactical ones.** The economics of Bullfrog and Sky Sentinel are unbeatable at
   $10 a burst — but only below a kilometre. Expect them everywhere on vehicles
   and perimeters, and nowhere in the strategic Shahed fight. `[M]`
2. **Autocannon arrives in bulk in Europe.** Rheinmetall plans up to **400
   Skyranger/Skynex systems annually by 2027** `[M]`; German Skyranger 30 serial
   deliveries run **2027–28** and Romania's Skyranger 35 order runs **2028–2030**.
   `[M]` By 2028 the 30/35 mm airburst turret is the default European point
   defence.
3. **Interceptors industrialise and get faster.** 1,000 Octopus a month from
   early 2026 `[M]`; jet interceptors past 600 km/h entering service `[M]`; the
   US Army buying Ukraine-proven designs `[H]`; NATO shopping on price alone `[H]`.
   Expect per-shot costs to fall and autonomy to increase, with the human moving
   out of the terminal loop.
4. **Lasers split into two stories.** Fixed and ship-mounted lasers mature —
   DragonFire on Type 45s by 2027 `[H]`, Iron Beam-M truck variants `[L]`, the
   Army's Enduring High Energy Laser effort `[M]`, and 20–50 kW lasers planned as
   a Skyranger add-on `[M]`. Mobile 50 kW lasers on manoeuvre vehicles stay
   troubled. `[H]`
5. **HPM range doubles, at least on paper.** The Gen II Leonidas contract is
   explicitly about doubling effective range and raising power. `[H]` If that
   lands, HPM moves from last-ditch to a genuine 1–2 km layer, and its
   fiber-optic kill becomes strategically significant.
6. **EW keeps losing ground.** 20-element CRPA, better INS, more fiber, more
   autonomous terminal guidance. `[M]` By 2028 EW should be modelled as a
   *softening* layer, not a defeat mechanism.
7. **The threat gets faster and higher.** Jet drone launches went up eightfold
   between 2025 and the first half of 2026. `[M]` If that trend holds, gun-class
   point defence loses relevance against strategic attack faster than anything
   else in the list.

---

## Genuinely uncertain or contested

- **Sky Sentinel's effective engagement range is unpublished.** The 800 km/h
  figure describes tracking, not killing. My 500–1,000 m estimate is `[CALC]`,
  not sourced.
- **Interceptor success rates are manufacturer claims.** Sting's 80–90% hit rate
  and 7,000 cumulative kills come from Wild Hornets. The 70%-of-downings figure
  is from Ukraine's Commander-in-Chief and is better supported but still a
  belligerent's claim.
- **Stinger unit cost ranges from $120,000 to $480,000** across open sources.
  Pick one and note it.
- **Airburst ammunition cost per round is not sourced here.** Do not put a number
  on it without finding one.
- **Iron Beam's 7–10 km is press-reported, not a vendor datasheet**, and almost
  certainly varies enormously by target hardness and weather.
- **Geran-3 performance is contested**: designed for 550–600 km/h, observed
  cruising at 300–350 with a terminal sprint. Both numbers are real; they
  describe different phases.
- **HPM "under 1 km"** is a general characterisation, not a measured figure for
  any specific system. Actual effective range is classified and target-dependent.
- **My dispersion assumption of 5–6 mrad** in the Q1 and Pk calculations is an
  engineering rule of thumb I applied, not a sourced value. The *shape* of the
  result — that hit probability falls roughly as the square of range, and that
  the speed penalty grows with range — is robust; the absolute percentages are
  not.
- **Every slew-rate figure in this document is unsourced.** Neither Sky Sentinel
  nor Bullfrog publishes a traverse rate, and this session's web-search budget
  was exhausted before I could reach remote-weapon-station or Skyranger
  datasheets. The class-ordering (light EO turret > autocannon turret > missile
  rail > crewed gun) and the elevation-is-half-azimuth rule are solid; the
  numbers are defaults to be replaced.
- **No Pk-versus-range curve is published for any counter-drone gun.** The whole
  hit-probability section is derived, not reported. Treat the derived tables as a
  defensible model with stated assumptions, not as findings.
- **Bullfrog's "$10 per engagement"** is a marketing figure. My inference that it
  implies 11–17 rounds rests on an assumed ammunition price, and on reading
  "engagement" as "successful engagement", which the source does not state.
- **Rounds-expended-per-kill is not published by anyone** for Sky Sentinel,
  Bullfrog, or Ukrainian mobile fire groups. If a real figure surfaces it should
  override the derived table immediately.

---

## Sources

**AI gun turrets**
- https://www.kyivpost.com/analysis/53546
- https://www.army.mil/article/291738/sky_sentinel
- https://united24media.com/war-in-ukraine/ai-powered-turret-that-hunts-russian-drones-meet-sky-sentinel-ukraines-new-air-defense-8589
- https://militarnyi.com/en/news/sky-sentinel-ukraine-to-produce-anti-aircraft-turrets-for-intercepting-drones-and-cruise-missiles/
- https://www.uasvision.com/2025/05/29/sky-sentinel-ukraines-ai-powered-anti-drone-turret/
- https://nationalinterest.org/blog/buzz/ukraines-ai-driven-sky-sentinel-turret-rewriting-air-defense-as-we-know-it-bw-120325
- https://thedefensepost.com/2024/09/04/us-anti-drone-gun-demo/
- https://www.military.com/marines-just-added-ai-that-that-turns-m240-machine-guns-into-drone-killers
- https://www.theregister.com/offbeat/2026/07/21/us-marines-latest-anti-drone-toy-is-an-ai-turret-that-uses-regular-machine-guns/5275691
- https://www.armyrecognition.com/archives/archives-land-defense/land-defense-2024/us-turns-to-ai-powered-bullfrog-turret-to-address-growing-threat-of-low-cost-drones
- https://www.businesswire.com/news/home/20250306711870/en/Allen-Control-Systems-Selected-to-Demonstrate-Counter-Drone-Robotic-Gun-System-at-the-Joint-Counter-Small-Unmanned-Aircraft-Systems-Offices-Sixth-C-sUAS-Demonstration
- https://united24media.com/latest-news/us-marines-roll-out-10-ai-gun-turrets-to-stop-shaheds-with-eyes-on-ukraine-deployment-12072
- https://www.thefirearmblog.com/blog/marine-corps-pairs-autonomous-m240-turret-w-expeditionary-air-defense-44829649
- https://www.globalsecurity.org/military/systems/ground/m2-50cal-specs.htm

**Gun trucks and mobile fire groups**
- https://euromaidanpress.com/2025/06/29/why-cant-ukraine-stop-russias-shahed-drones-anymore/
- https://gwaramedia.com/en/ukrainian-mobile-firing-groups-hunt-for-russian-shaheds-with-guns-unfit-to-efficiently-down-them/
- https://frontliner.ua/en/the-fight-against-shaheds-why-mobile-fire-groups-need-to-be-modernized/
- https://news.liga.net/en/politics/news/the-effectiveness-of-mobile-firing-units-is-declining-the-prytula-foundation-explained-why
- https://www.aol.com/articles/machine-gunners-going-head-head-130324242.html
- https://www.wesodonnell.com/p/ukraines-cheap-drone-killers-have

**Autocannon and airburst**
- https://www.rheinmetall.com/en/products/air-defence-systems/mobile-air-defence-skyranger
- https://en.wikipedia.org/wiki/Skyranger_35
- https://www.twz.com/land/germany-to-bet-big-on-skyranger-gun-system-to-address-growing-drone-threat
- https://www.armyrecognition.com/news/army-news/2026/romanias-lynx-based-skyranger-order-builds-a-mobile-counter-drone-shield-for-natos-black-sea-flank
- https://en.defence-ua.com/news/rheinmetall_to_produce_up_to_400_skyranger_and_skynex_systems_annually_to_counter_shahed_drones_in_ukraine_and_europe-18008.html
- https://drone-warfare.com/counter-uas/drone-defeat/

**Interceptor drones**
- https://www.unmannedairspace.info/counter-uas-systems-and-policies/wild-hornets-sting-interceptor-reaches-315-km-h-shoots-down-over-200-shaheds-and-gerberas/
- https://wildhornets.com/en/sting-interceptor
- https://en.wikipedia.org/wiki/Sting_(drone)
- https://en.defence-ua.com/weapon_and_tech/ukrainian_wild_hornets_workshop_revealed_impressive_effectiveness_of_their_sting_interceptors_against_shahedgerbera_type_uavs-15885.html
- https://www.hisutton.com/Ukrainian-Interceptor-Drones.html
- https://united24media.com/defense-tech/what-is-ukraines-interceptor-one-of-the-worlds-most-in-demand-drones-17055
- https://www.defensenews.com/global/europe/2026/03/05/novel-interceptor-drones-bend-air-defense-economics-in-ukraines-favor/
- https://www.militarytimes.com/news/pentagon-congress/2026/03/11/these-are-ukraines-1000-interceptor-drones-the-pentagon-wants-to-buy/
- https://www.forbes.com/sites/davidkirichenko/2026/03/08/ukraine-turns-to-autonomous-drone-interceptors-as-shahed-attacks-surge/
- https://defence-blog.com/ukraines-autonomous-shahed-killer-costs-3500-per-unit/
- https://www.nationaldefensemagazine.org/articles/2026/4/15/ukraine-flips-cost-imbalance-script-with-lowcost-interceptors
- https://www.twz.com/land/cheap-interceptor-drones-proven-in-ukraine-protected-u-s-troops-against-iranian-shaheds
- https://www.armyrecognition.com/news/army-news/2026/u-s-army-deploys-merops-interceptor-drones-to-counter-iranian-shahed-136-swarm-threat
- https://www.defensenews.com/unmanned/2026/04/20/us-army-turns-to-ukraine-tested-drones-to-counter-iranian-uav-threat/
- https://www.defensenews.com/global/europe/2026/05/05/nato-nations-size-up-an-interceptor-drone-bazaar-where-low-price-is-everything/
- https://www.army.mil/article/291379/g_tead_delivers_rapid_counter_drone_capability_to_natos_eastern_flank_demonstrating_the_power_of_accelerated_acquisition
- https://www.pravda.com.ua/eng/news/2026/08/14/8048658/
- https://www.armyrecognition.com/news/aerospace-news/2026/ukraine-begins-mass-production-of-skyfall-600-km-strike-drone-as-jetkiller-shahed-interceptor-debuts
- https://www.airforce-technology.com/news/ukraine-jedi-shahed-hunter-interceptor/
- https://militarnyi.com/en/news/ukrainian-interceptor-drone-downs-jet-powered-shahed-for-the-first-time/
- https://www.fpri.org/article/2026/03/better-late-than-never-us-and-allies-race-toward-ukrainian-counter-shahed-tech/

**Missiles and guided rockets**
- https://www.flightglobal.com/military-uavs/us-military-commits-17bn-for-baes-apkws-ii-rockets-to-counter-drone-threats/164317.article
- https://www.airandspaceforces.com/apkws-base-laser-guided-rockets-pentagon-contract/
- https://en.wikipedia.org/wiki/Advanced_Precision_Kill_Weapon_System
- https://www.battlepolicy.com/apkws/
- https://www.twz.com/our-best-look-at-a-vampire-counter-drone-system-for-ukraine
- https://en.defence-ua.com/news/ukraine_will_receive_all_14_vampire_counter_drone_systems-8803.html
- https://newatlas.com/military/coyote-interceptor-test-takes-out-drone-swarms/
- https://en.defence-ua.com/analysis/how_effective_coyote_counter_uav_drones_would_be_against_russian_shahed_attacks-6668.html
- https://www.twz.com/land/containerized-sam-system-that-fires-soviet-air-to-air-missiles-for-ukraine-breaks-cover
- https://theaviationist.com/2025/02/13/gravehawk-for-ukraine/
- https://euromaidanpress.com/2025/02/17/uk-engineers-turn-soviet-r-73-missiles-into-ukraines-new-gravehawk-air-defense/
- https://www.zona-militar.com/en/2025/03/22/ukrainian-armed-forces-unveil-new-frankensam-air-defense-system-that-also-employs-r-73-air-to-air-missiles/
- https://en.wikipedia.org/wiki/FIM-92_Stinger
- https://www.missiledefenseadvocacy.org/missile-defense-systems/missile-interceptors-by-cost/

**Directed energy**
- https://www.tomshardware.com/tech-industry/100kw-iron-beam-laser-becomes-worlds-first-drone-defense-zapper-to-be-operationally-deployed-it-can-also-shoot-down-rockets-mortars-and-other-aerial-threats
- https://interestingengineering.com/military/worlds-first-combat-ready-100kw-laser
- https://nationalsecurityjournal.org/israel-now-has-fully-operational-100-kilowatt-iron-beam-lasers/
- https://www.globaldefensecorp.com/2026/03/08/israeli-100kw-iron-beam-air-defense-system-is-fully-operational/
- https://gulfnews.com/world/mena/israel-says-laser-missile-shield-to-cost-just-2-per-interception-1.88286207
- https://www.tomshardware.com/tech-industry/uk-confirms-dragonfire-laser-weapon-for-royal-navy-destroyers-by-2027
- https://www.tomshardware.com/tech-industry/uk-dragonfire-laser-downs-high-speed-drones
- https://www.royalnavy.mod.uk/news/2024/april/12/240412-powerful-laser-to-be-installed-on-royal-navy-warship-by-2027
- https://www.defensenews.com/global/europe/2025/11/20/uk-royal-navy-to-equip-mbdas-drone-frying-lasers-by-2027/
- https://breakingdefense.com/2024/05/army-soldiers-not-impressed-with-strykers-outfitted-with-50-kilowatt-lasers-service-official-says/
- https://www.defensedaily.com/army-details-challenges-with-stryker-mounted-50kw-laser-prototypes-deployed-to-centcom/army/
- https://www.laserwars.net/p/army-directed-energy-maneuver-short-range-air-defense-de-m-shorad-problems-gao
- https://www.congress.gov/crs_external_products/IF/PDF/IF12397/IF12397.17.pdf
- https://defensescoop.com/2025/11/03/army-enduring-high-energy-laser-ehel-rfi-counter-uas/
- https://www.nationaldefensemagazine.org/articles/2026/1/20/counterdrone-mission-seen-as-killer-app-for-directed-energy
- https://dronelife.com/2025/10/15/microwave-counter-drone-system/
- https://www.twz.com/land/army-puts-50m-bet-on-next-gen-leonidas-high-power-microwave-counter-drone-tech
- https://www.epirusinc.com/press-releases/epirus-leonidas-demonstrates-successful-use-of-high-power-microwave-to-defeat-fiber-optic-controlled-uas
- https://www.armyrecognition.com/news/army-news/2026/u-s-marines-move-to-vehicle-mounted-epirus-havoc-microwave-weapon-to-defeat-drone-swarms
- https://uasdefense.io/countermeasures/microwave-hpm
- https://www.droneshield.com/blog/a-counter-to-drone-swarms-high-power-microwave-weapons

**Nets and close-in capture**
- https://openworksengineering.com/skywall-patrol/
- https://fortemtech.com/products/dronehunter-f700/
- https://interestingengineering.com/military/net-based-counter-drone-system
- https://www.airsight.com/knowledge-hub/counter-drone-technology/air-to-air

**Electronic warfare and its limits**
- https://drone-warfare.com/counter-uas/electronic-warfare/
- https://militarnyi.com/en/blogs/how-shahed-locks-onto-targets/
- https://militarymachine.com/fiber-optic-drones-unjammable-weapons
- https://www.gisreportsonline.com/r/ukraine-diy-drones/

**Threat, sensors and saturation**
- https://www.csis.org/analysis/drone-saturation-russias-shahed-campaign
- https://www.csis.org/analysis/shahed-geran-how-russia-continues-reinvent-one-way-attack-drone
- https://isis-online.org/isis-reports/monthly-analysis-of-russian-shahed-136-deployment-against-ukraine
- https://www.forbes.com/sites/vikrammittal/2026/07/14/russia-is-adapting-their-strategies-for-their-geran-shahed-drones/
- https://en.wikipedia.org/wiki/HESA_Shahed_136
- https://www.globalsecurity.org/military/world/iran/shahed-136.htm
- https://drone-warfare.com/research/shahed-136/
- https://www.pravda.com.ua/eng/articles/2026/05/19/8035370/
- https://en.defence-ua.com/news/russian_jet_powered_drone_hits_kyiv_avionics_and_engine_point_to_geran_3_based_on_shahed_238-14821.html
- https://www.forbes.com/sites/davidhambling/2025/09/18/russias-new-jet-powered-shahed-revealed-what-it-means-for-ukraine/
- https://united24media.com/war-in-ukraine/sky-fortress-ukraines-acoustic-detection-system-that-tracks-drones-cheap-and-fast-9451
- https://militarnyi.com/en/news/nato-shows-interest-in-ukrainian-acoustic-detection-networks-for-air-defense/
- https://interestingengineering.com/military/us-military-counter-uas-system-teledyne-cerberus
- https://drone-warfare.com/counter-uas/radar-detection/
