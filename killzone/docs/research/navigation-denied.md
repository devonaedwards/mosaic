# Navigation when there is no link and no satellite fix

Research note for KILL ZONE, September 2026. Written for a designer who has to
pick a number and defend it. KILL ZONE is a video game; nothing here is
operational guidance.

Covers sections 1, 2 and 3 of `BRIEF-navigation.md` (scene matching, celestial
navigation, inertial drift), plus a short pass on what is fielded in Ukraine now
and the 2027–28 trajectory. **Section 4, weather, is a separate document** and is
not duplicated here; where weather gates navigation — principally cloud against
star trackers — it is flagged and handed off.

## How this was researched, and how much to trust it

Retrieved 14 September 2026 via web search. The egress proxy blocked direct page
fetches on most defence, journal and vendor domains — arxiv.org, mdpi.com,
jhuapl.edu, advancednavigation.com, sodern.com, thedefensepost.com, vantor.com
and pravda.com.ua all refused. **Most figures below therefore come from search
snippets rather than from a page I read end to end.** These are marked
*(snippet)*.

Markers: **[M]** measured or vendor-specified, **[R]** reported, **[I]** my
inference, **[E]** my estimate for the designer.

A standing warning: **GNSS-denied navigation accuracy is a marketing number.**
Almost every impressive figure below is a vendor demonstrating its own product on
a route it chose. There is no independent test regime. Treat any vendor claim of
sub-1% drift over tens of kilometres as a best case on a closed loop.

---

## Summary: the five things that matter

1. **Terrain shape is far more stable than appearance.** Elevation matching
   (radar altimeter against a DEM) is indifferent to snow, ploughing, harvest,
   smoke and most battle damage — none move the ground by enough to matter
   against a DEM's own error. Optical matching is sensitive to all of them. The
   catch: shape matching needs terrain that *has* shape, and East European plain
   is where it degenerates. The two fail in complementary places.

2. **Cross-season optical matching does work, at roughly 12–20 m.** This is the
   number that kills the naive version of the "imagery expires" mechanic. [M]

3. **Celestial navigation is an attitude instrument.** It measures where the
   vehicle is *pointing*, not where it *is*, and yields position only through the
   INS's own vertical reference — a conversion in which one arc-second of
   vertical-deflection error is thirty metres of position error. Section 2.2.

4. **Celestial is not reaching cheap airframes by 2027–28.** The one credible
   priced unit is ~€250,000 and just under 3 kg. The cheap research demonstrator
   achieved 4 km accuracy. Neither is a loitering-munition part. [R]

5. **Inertial error grows as the cube of time, not linearly.** There is no honest
   "metres per minute". Doubling the denied duration multiplies the error by
   about eight.

---

# 1. Scene matching and terrain-referenced navigation

## 1.1 There are three different things here, and the game should not merge them

The brief treats "scene matching" as one mechanic. It is three, with genuinely
different failure modes.

**(a) Terrain elevation matching (TERCOM / TRN).** A radar altimeter samples
height-above-ground along the flight path; the profile is correlated against a
stored digital elevation model. The reference is *geometry*.

**(b) Optical scene matching (DSMAC-lineage, and modern learned equivalents).**
A downward camera image is matched against a georeferenced orthophoto. The
reference is *appearance*.

**(c) Visual odometry / VIO.** No external reference at all — it measures motion
relative to whatever the ground looked like a moment ago. It only slows drift.
Covered in section 3.3.

(a) and (b) give **absolute** fixes and therefore *bound* error. (c) does not.
That distinction is the whole game mechanic: a drone with (a) or (b) has bounded
error forever; a drone with only (c) and an IMU has error that grows without
limit.

## 1.2 What the numbers actually are

| System | Reference type | Reported accuracy | Marker |
|---|---|---|---|
| TERCOM (Tomahawk lineage, 1980s) | DEM | early systems "hundreds of metres"; improved to **75 m CEP** | [M, snippet] |
| Interferometric radar altimeter TRN, 5 Hz | DEM | **3.20 m RMS over rough terrain, 10.2 m over smooth** | [M, snippet] |
| Season-invariant CNN matching to orthophotos (Kinnari/Verdoja/Kyrki) | orthophoto | **12.6–18.7 m lateral error**, converging in **23–44 updates** from an uninformed start, winter-vs-summer | [M, snippet] |
| Hierarchical visual geolocalisation (2025-era academic) | satellite imagery | mean absolute error improved **34.2 m → 20.1 m** | [M, snippet] |
| Maxar/Vantor **Raptor** | 3D terrain (Precision3D, 3 m accurate) + live camera | **<10 m RMSE** position in all dimensions; **<3 m** for ground coordinate extraction | [M, vendor, snippet] |
| Twist Robotics **OSCAR** (Ukraine, fielded) | landmark map | **"accurate to 20 m without cumulative error during flight"**; day, night, and claimed fog | [M, vendor, snippet] |
| UAV Navigation **VNS01** (Sept 2026 release) | satellite map + DEM | error **bounded at ~30 m** over prolonged missions on previously unflown routes | [M, vendor, snippet] |

The convergence on **10–30 m** is the strongest single result here. Four
unrelated organisations — an academic group, an American imagery company, a
Ukrainian startup and a Spanish avionics firm — report absolute fixes in the same
band by different methods. [I] That is a real number, not a marketing artefact.

**In game units:** at 12:1 compression a 20 m real fix is **1.7 map metres** —
for gameplay purposes, exact. A drone holding a scene-matching lock is **not
degraded at all**. The interesting mechanic is therefore not "scene matching is
less accurate", it is "scene matching either works or it doesn't".

## 1.3 Shape versus appearance: how much more stable is shape?

The brief's instinct is right, and the margin is large.

A DEM's own vertical accuracy is metres — Maxar's Precision3D is quoted at 3 m
[M, vendor, snippet], and open global DEMs are worse. Now consider what the
battlefield does to ground height:

- **Snow.** Typical accumulation in that theatre is centimetres to tens of
  centimetres. Kyiv has snow cover roughly **80 days a year** [M, snippet]. Even
  half a metre of snow is well inside a 3 m DEM error budget. Snow is **invisible
  to elevation matching and catastrophic for optical matching** — it is the
  cleanest example of the shape/appearance divergence.
- **Ploughing and harvest.** Surface height changes by tens of centimetres;
  reflectance changes completely. Same story.
- **Craters.** An artillery crater is order 1–3 m deep and a few metres across.
  A radar altimeter footprint at low altitude is tens of metres wide and returns
  an *average* height over that footprint [M, snippet], so individual craters are
  averaged away. Cratering bites elevation matching only when dense enough to
  shift the mean height of a whole footprint. [I]
- **Destroyed buildings.** The one case where shape genuinely changes: a
  collapsed ten-storey block removes thirty metres of height. Elevation matching
  over a destroyed built-up area *is* degraded — but built-up areas are a small
  fraction of route length. [I]
- **Flooding.** Changes both. Water is a specular radar-altimeter target and a
  featureless optical one; a breached dam is a genuine double failure. [I]
- **Smoke and fire.** Invisible to radar altimetry. Devastating to optical: dust
  and smoke "scatter ambient light and suppress image contrast" so that "reliably
  detected feature counts collapse — triggering abrupt tracking failures and
  unrecoverable localization loss" [M, snippet]. The same source notes an
  adversary can deploy smoke that "reduces local contrast but preserves global
  structure".
- **Vegetation, the honest exception.** Shape is not perfectly stable. A radar
  altimeter over forest reads somewhere between canopy top and ground, and the
  DEM literature confirms canopy adds metres of apparent height and leaf-on/
  leaf-off changes it [M, snippet]. Over forest, elevation matching has a
  seasonal term too — just a much smaller one than optical.

**Quantified answer for the designer [E]:** across the worst seasonal transition,
treat elevation matching as losing about **10% of its fix quality**, and optical
matching as losing **50–80%** of its *match rate* with a naive matcher or
**20–30%** with a season-invariant one. Estimates built on the measured anchors
above, not measured themselves.

## 1.4 Is failure gradual or a cliff? Both, in a specific pattern

**Accuracy degrades gradually; lock is lost as a cliff.** All modern
implementations are recursive estimators — particle filters, Monte Carlo
localisation, Kalman-family filters — fusing a stream of weak matches. Degrading
conditions reduce the information per match, so the estimate gets noisier and
convergence gets slower (the 23–44 updates figure above is exactly this:
convergence *time* degrades first). Past a threshold the filter's hypotheses no
longer contain the truth, it latches onto a wrong mode, and there is no graceful
path back — "unrecoverable localization loss" [M, snippet].

The older literature has the same shape: TERCOM over flat ground suffers
"significant correlation ambiguity, often leading to navigation failures and
necessitating fallback to INS-only mode" [R, snippet, low-quality source — see
section 8]. Ambiguity, not error growth. The correlation surface stops having a
single peak.

**Recommended game model.** Two variables, not one:

- a continuous **fix quality** (metres of error) that degrades smoothly with
  conditions — use it for targeting accuracy;
- a discrete **lock** state with a probability-of-loss per unit time driven by
  the same conditions. Once lost, re-acquisition requires either a distinctive
  region or a long re-convergence (the 23–44 updates figure; at, say, 1 Hz and
  30 m/s that is **700–1,300 m of flight to re-converge**) [I from M].

That gives the designer the drama of a cliff without an on/off switch, and it is
faithful.

## 1.5 How current must reference imagery be?

**The honest answer: the literature does not publish a decay-versus-age curve,
and I could not find one.** What it publishes is performance across *season
pairs*, which is a proxy for age but not the same thing. Anyone who gives you
"reference imagery degrades X% per week" is making it up. What can be said:

- **Seasonal change dominates calendar age.** The Kinnari dataset deliberately
  spans **2002 to 2021** — nineteen years of reference imagery for the same areas
  — and the hard axis in that work is winter-vs-summer, not old-vs-new [M,
  snippet]. Imagery from the same season nineteen years earlier is easier than
  imagery from six months earlier across a season boundary. [I]
- **Military practice is months, not days.** For Tomahawk DSMAC, "reference
  processing may be completed months before the map is flown", and reference
  imagery is "generally acquired weeks or months before the DSMAC flight unit
  images the scenes" [M, snippet]. Block II mission planning took **24 to 80
  hours** when TERCOM and DSMAC data was already in hand, and could be **delayed
  indefinitely** if the imagery was not [M, snippet]. That is a supply-chain
  constraint, not a freshness constraint — and it is a good one for the game.
- **Feature selection is the mitigation.** DSMAC-era practice used "an improved
  feature extraction algorithm designed to eliminate most of the features that
  vary with climate, time, and season" [M, snippet]. Modern practice does the
  same by choosing semantically stable references: VNS01 correlates against
  "roads, rivers and coastlines" [M, vendor, snippet] — geometry wearing an
  optical disguise. **This is the real answer to the brief's question:
  production systems dodge the currency problem by refusing to match on anything
  that changes.**

**Estimate for the designer [E]:** for a system built with season-invariant
feature selection, calendar age of reference imagery is a **second-order** effect
over one to two years in rural terrain. Where it becomes first-order is where the
*structure* changed: a destroyed settlement, a new bridge or a new bank of
fortifications. That is a change-in-kind, not decay, and it is localised.

## 1.6 Where the whole family fails structurally

**Flat terrain kills elevation matching.** TERCOM "experiences difficulty in flat
or featureless areas like deserts or oceans"; performance "degrades over flat,
featureless terrains such as vast plains ... due to ambiguous profiles,
necessitating route planning to favour distinctive areas — hills, valleys, or
river gorges" [R, snippet]. **A game set on the Pontic steppe should treat
elevation matching as the *weaker* technique over most of its map**, which
inverts the usual intuition. River valleys, spoil heaps, reservoir banks and
urban edges are where it works.

**Featureless or repetitive ground kills optical matching.** Snow-covered fields
produce "featureless white regions" through sensor saturation [M, snippet]; large
monoculture fields and forest canopy produce repetitive texture. Same correlation
ambiguity, different route.

**The two failures are anti-correlated** [I]: rugged ground has shape but often
forest; flat farmland has no shape but has field boundaries, roads and tracks
that optical matching loves. A drone carrying both is markedly more robust than
one carrying either — which is exactly what VNS01 did by adding TRN *and*
satellite map matching in the same September 2026 release [R].

---

# 2. Celestial navigation

## 2.1 What a star tracker actually measures

A star tracker images stars and matches them to a catalogue. Its output is
**attitude**: the orientation of its own boresight in an inertial frame. Quoted
accuracies are angular — a typical unit around 15 arc-seconds, high-end
spacecraft units 0.2″, with ground-test results better than 5″ (3σ) pointing and
11″ (3σ) roll [M, snippet]. Sodern's Astradia is quoted at "a few arc-seconds,
equivalent to 1 metre at a distance of 70 km" — an *angular* figure expressed as
a lever arm, not a position accuracy [M, vendor, snippet].

## 2.2 **Position fix or heading reference? The answer**

**It is a heading and attitude reference first. It produces a position fix only
indirectly, through the vehicle's own vertical reference, and the conversion
destroys most of the accuracy.**

The reasoning is worth stating because it is the part that gets blurred:

A star's direction in inertial space is known exactly from the catalogue and the
time, so measuring it tells you your orientation relative to the stars. To get
*position* you need the star's angle relative to **local vertical** — what a
marine sextant does with the visible horizon — and that yields a **celestial line
of position**, not a point. Two or more stars give a fix. Airborne practice is
the same: the astro compass "provided heading as its primary output and could
also help determine the location of the aircraft using a technique called the
celestial line of position" [M, snippet]. The current literature still treats
heading-independent celestial *localisation* as an open research problem.

An aircraft has no usable horizon reference, so local vertical comes from the
INS. And the INS's idea of vertical is wrong by the local **deflection of the
vertical** — the angle between the gravity vector and the ellipsoid normal, which
varies with local geology and is not in a cheap system's model. The published
conversion is devastating and exact:

> **Vertical deflection of 1 arc-second produces a coordinate error of 30 m;
> 1 arc-minute produces 1 nautical mile.** [M, snippet]

So a 2-arc-second star tracker on a platform whose vertical is uncertain by 10
arc-seconds gives a 300 m position fix. The star tracker is not the limiting
element and never was.

The vendor language tracks this once you read it carefully. Sodern describe
Astradia as providing "daytime and nighttime **attitude measurement**, for
precise, robust and reliable onboard geopositioning data", and say it "helps
counter the natural drift in inertial navigation systems" [M, vendor, snippet] —
attitude in, drift suppression out. Northrop's LN-120G is the same shape: in
GPS-denied environments "the stellar-inertial mode provides heading performance
while position error is **bounded** by stellar fixes" [R, snippet].

**Game mechanic implication.** Celestial is **a drift-rate modifier, not a
periodic position reset** — the wrong answer to the brief's "does each fix reset
the error" question if you were hoping for a reset. It **bounds** azimuth/heading
error rather than letting it walk, which removes the dominant term in long-run
inertial position growth (see 3.2: kill the heading drift and you kill the
cubic). "Error grows without limit" becomes "error grows slowly and boundedly".

The SR-71's system is the canonical demonstration: **less than half a nautical
mile terminal error after more than half way around the world** [R, snippet] —
900 m after ~20,000 km, achieved with a gimballed platform, a 52-star catalogue,
six simultaneously tracked stars and an INS no expendable drone will carry. A
widely repeated claim that it gave "course guidance with an accuracy of at least
90 metres (300 feet)" [R, snippet] is inconsistent with that same source's
terminal figure and looks like a conflation; **do not use it.**

## 2.3 Who carries it, at what cost and mass

| Class | Example | Mass | Cost | Marker |
|---|---|---|---|---|
| Strategic / ISR aircraft | Northrop LN-120G (RC-135) | not found | not found | [R] |
| High-end manned & large UAS | Sodern **Astradia** | **<3 kg**, ~176×185×207 mm ("roughly the size of a shoebox") | **~€250,000** per unit; commercial deliveries from June 2025 | [M/R, snippet] |
| Research demonstrator on a small UAV | UniSA (Teague & Chahl) | Raspberry Pi 5 + Alvium 1800 U-240 mono camera + 6 mm f/1.4 lens; a few hundred grams | low hundreds of dollars in parts | [M, snippet] |

**Verdict on the brief's question: it is not reaching cheap expendable airframes
by 2027–28.** The €250k figure is a snippet price, indicative rather than a
quote, but it is three to four orders of magnitude above the airframes the game
calls expendable. The European military star-tracker market is quoted at roughly
**USD 0.13 bn in 2025 → 0.23 bn by 2034, ~7% CAGR** [R, snippet]: a niche
instrument market, not a technology about to go mass. [I]

The cheap end is real, and its accuracy shows why nobody has fielded it: the
UniSA UAV "consistently estimated its location to within an accuracy of **4 km**"
while flying fixed-altitude, fixed-airspeed orbits [M, snippet]. 4 km is useless
for targeting and adequate for *getting to the right province* — which is in fact
the job. [I]

**Negative finding.** I searched specifically for celestial navigation on
Shahed/Geran-family drones and **found none.** The documented Russian answer is
the Kometa-M CRPA antenna (4 → 8 → 12 → reportedly 16 elements), plus INS,
mobile-network positioning via Ukrainian SIM cards, and reported Starlink
terminals on some 2026 airframes [R, snippet]. **A "cheap drone navigates by
stars" unit is a design invention, not a reflection of 2026.**

## 2.4 Daylight operation, and cloud

**Daylight is real, and it is the whole product.** Astradia is an "endo-
atmospheric star tracker" giving "daytime and nighttime attitude measurement" [M,
vendor, snippet]; the SR-71 system tracked six stars by day as well as night [R].
It works because sky brightness falls with altitude and a narrow field of view
integrating against a known catalogue position can pull a bright star out of a
bright background.

**But only on expensive hardware.** The cheap demonstrator is explicitly a
*night* system — "for UAVs flying at night" [M, snippet]. Daylight is precisely
what the €250k buys. **If a unit carries the cheap celestial option, it is
night-only.** Clean, drawable mechanic.

**Cloud is a hard block, not a degradation**, and the weather document owns the
detail. One datum: **Kyiv records about 31 hours of sunshine in December, roughly
1.0 h/day** [M, snippet], implying overcast for most of the darkest month. **[E]
Placeholder gate: celestial available ~30–40% of hours in winter, ~60–70% in
summer, restored by flying above a low overcast — which the game does not
currently model.**

---

# 3. Inertial drift, with real numbers

## 3.1 The grades, and where an expendable airframe sits

Gyroscope in-run bias stability is the standard axis [M, snippet]:

| Grade | Gyro bias instability | Where it lives |
|---|---|---|
| Consumer MEMS | 10–100 °/hr and worse (one source: "50 °/hr or more") | phone, toy drone, the cheapest FPV flight controller |
| Industrial MEMS | ~10 °/hr | mid-range autopilot |
| Tactical MEMS | 0.5–5 °/hr (Yole's definition); "1 °/hr or better" | guided munitions, serious small UAS |
| Navigation grade (RLG/FOG) | ~0.01 °/hr | LN-100G class, manned aircraft |

**An expendable airframe carries consumer-to-industrial MEMS.** A reusable
reconnaissance drone or a premium loitering munition can justify tactical MEMS.
Nothing expendable carries navigation grade.

## 3.2 The shape of the error, which is a cube law

**There is no honest "metres per minute" figure, and the brief should not get
one.** Free-inertial position error is dominated by two terms:

- accelerometer bias: error = ½·b·t²
- gyro bias, through the tilt it induces: error ≈ g·ε·t³/6

The gyro term overtakes the accelerometer term after roughly **100 seconds** for
typical MEMS parameters [I], and from then on **error grows as the cube of
time.** Beyond about half an hour the Schuler loop turns the cube into a bounded
oscillation plus a ramp, which for a drone sortie is usually moot.

The practical consequence, and the rule I recommend the game implement:

> **Doubling the denied time multiplies the position error by about eight.**

Computed curve, using the formulae above with residual biases representative of
each class [I, computed — the formulae are textbook, the parameters are my
choice]:

| Denied time | Consumer/industrial MEMS (≈1 °/hr, 0.5 mg residual) | Tactical MEMS (≈0.1 °/hr, 0.1 mg) | Navigation grade (0.01 °/hr) |
|---|---|---|---|
| 1 min | ~10 m | ~2 m | <1 m |
| 2 min | ~50 m | ~6 m | ~1 m |
| 5 min | ~430 m | ~65 m | ~8 m |
| 10 min | ~2.6 km | ~350 m | ~40 m |
| 20 min | ~17 km | ~2.1 km | ~310 m |

**Validation.** A published source states a typical MEMS INS "would drift by
**thousands of metres over 20 minutes** of GNSS outage", while the
navigation-grade LN-100G "claims to drift **120 m after a 20-minute GNSS
outage**" [M, snippet]. The table's tactical (2.1 km) and navigation (310 m)
columns bracket those, the navigation one pessimistically by ~2.5×. At the short
end, "unaided MEMS INS accumulates lane-width position errors (~3.5 m) within
30–60 seconds" at highway speed [M, snippet] — the table's ~10 m at 1 minute is
the right order.

**Caveat that changes the game model.** A fixed-wing drone is not doing free
inertial navigation. With a pitot, barometer and magnetometer it is doing
**air-data dead reckoning**, and its error is dominated not by gyro bias but by
**unknown wind** — a 5 m/s wind estimation error for 10 minutes is 3 km [I,
arithmetic], comparable to the worst MEMS column, and *linear* in time rather
than cubic. Advanced Navigation demonstrated a **Certus Evo MEMS INS plus an air
data unit at 8.8 m error over 5 km — 0.17% of distance travelled** [M, vendor,
snippet], with missions "up to around 50 km without GNSS" as the design envelope.

**For the designer:** two error laws; pick one. Cube-law-in-time suits a
quadrotor or a terminal dive. **Percent-of-distance is the better game mechanic**
for cruising fixed-wings because it makes range the currency, which is what the
rest of the game already trades in. [E] Use **1–3% of distance for a cheap
airframe, 0.2–0.5% for a good one**, with wind as the multiplier.

## 3.3 What visual-inertial odometry actually recovers

VIO tracks image features frame to frame and fuses them with the IMU. It
converts the cube law into a **percent-of-distance** law, which is an enormous
win, but it never bounds error because it has no external reference.

Reported drift, all as percent of distance travelled:

- **<1%** for a combined visual/inertial approach on a short trajectory (0.04 m
  over 9 m), versus **4.5%** frame-only and **6.3%** event-only [M, snippet].
- **~1%** quoted by UAV Navigation for its GNSS-Denied Navigation Kit — "error
  rates as low as 1% over covered distances" [M, vendor, snippet].
- **~6%** over 350 m in a parking structure (ground vehicle, degraded visual
  conditions) [M, snippet].
- Bavovna.AI claims **EPPE <0.5% at 30 km**, and 1.4 m endpoint error after a
  7.8 km return-to-launch [M, vendor, snippet]. **Treat with scepticism**: a
  return-to-launch closed loop is the easiest possible test because errors on the
  outbound and inbound legs partly cancel, and "99.98% accuracy" is a marketing
  construction, not a navigation metric. [I]

**[E] Use 1–3% of distance for VIO on a real airframe in real conditions**, with
the vendor 0.5–1% figures as a best case on clear ground in good light.

**Compute and mass cost.** The realistic platform is a Jetson Orin Nano class
module: **7–15 W** (8 GB part), 7–25 W+ (Super), 15–40 W (Orin Nano 2) [M,
snippet]; with carrier board, heatsink and camera, roughly **100–250 g** [E].
MIT's Navion ASIC runs full VIO at **2 mW** [M, snippet], proving the compute
cost is an engineering choice rather than a limit — but no such part is in volume
production.

**Game translation:** 10 W and 200 g is meaningful on a 2 kg quadrotor and
negligible on a 15 kg fixed-wing. **Optical navigation is proportionally cheaper
the bigger the airframe.** [I]

---

# 4. What is fielded now, and 2027–28

## 4.1 Fielded, 2026

- **Twist Robotics OSCAR** (Ukraine). Camera matches terrain against a landmark
  map, feeding the autopilot "as a reliable GPS signal". Claimed **20 m with no
  cumulative error**, day, night "and even in fog"; **24 months of development
  and more than 500,000 km of testing and operational data** [M, vendor via
  press, snippet]. The fog claim is the one to doubt.
- **UKRSPECSYSTEMS Shark-M** (July 2026). GNSS-denied launch algorithms, then
  "switches to optical navigation, matching live video feeds against satellite
  imagery"; automatically disables GNSS on detecting interference. 7 h endurance,
  420 km mission range, 14.5 kg MTOW [R].
- **Maxar/Vantor Raptor** and **UAV Navigation VNS01** — accuracies in the 1.2
  table. Both are built on *3D terrain* and structural features (roads, rivers,
  coastlines) rather than raw imagery; Raptor runs off a "commodity on-board
  camera" against **100 M+ km²** of pre-existing 3D terrain [M, vendor, snippet].
- **Russian practice is antenna-based, not optical.** Kometa-M CRPA with 4→16
  elements, plus INS, SIM-card positioning and reported Starlink terminals [R].

Context figure: Ukraine's Technology Forces state that **~75% of small tactical
UAV losses at the front are caused by EW** [R, snippet]. That is the pressure
driving all of the above.

## 4.2 2027–28 trajectory [I/E]

1. **Optical absolute-fix navigation becomes standard on anything above FPV
   cost.** Four independent products reached market inside eighteen months; the
   compute is a commodity module; the reference data exists globally.
2. **Celestial does not follow.** It stays on €100k+ platforms.
3. **The reference-data supply chain becomes the contested layer** — already
   happening, see section 6.
4. **The counter is not jamming, it is the environment.** Once navigation is
   passive and optical, you defeat it with smoke, weather, darkness and terrain
   change — things the game already wants to model. [I] A good place to land.

---

# 5. Recommendation: what should happen when a drone crosses the geofence

The current design (FINDINGS 26) makes geofence crossing an instant, total loss
of the satellite link. FINDINGS 23 gives a two-tier autonomy split: last-mile
guidance keeps the crew and improves accuracy; autonomous selection drops the
crew and pays an error rate. FINDINGS 26 proposes that crossing the geofence is a
forced demotion from the first to the second.

**That proposal is right, and the research adds one thing: the demotion should be
conditional on what the drone is carrying, not automatic.**

Three tiers of behaviour past the line:

**Tier 0 — dead reckoning only (cheap airframes).** No absolute reference. Error
grows at 1–3% of distance flown, compounded by wind: at 12:1 compression, 3% over
20 km of real ground is **50 map metres** of aimpoint error. Demote these to
autonomous selection *and* apply an accuracy penalty that grows with penetration
depth. **This is the unit that makes the geofence hurt.**

**Tier 1 — scene matching with valid reference data.** Error bounded at 10–30 m
real, **1–3 map metres** — negligible. Do *not* demote these on navigation
grounds: they know where they are. They still lose the *link*, so they still lose
last-mile human guidance, but they arrive at the right place and then have to
decide what to hit. **The navigation penalty and the autonomy penalty should be
separable.**

**Tier 2 — scene matching whose reference data does not cover the ground, or
whose conditions have broken the lock.** Tier 1 falling back to Tier 0
mid-flight, and the most interesting state because it is recoverable: a drone
that loses lock over a snowfield can regain it over a river valley. Use the
two-variable model from 1.4, with a re-acquisition cost of roughly 700–1,300 m of
flight.

**The one-line rule:** *crossing the geofence costs you the operator; it costs
you your position only if you did not bring a map.*

That preserves the mechanic in FINDINGS 26 — pushing an offensive past your own
border unsupports your drones — while making it a **procurement decision** rather
than a flat tax, and it turns reconnaissance of enemy ground into a prerequisite
for deep strike, the theme FINDINGS 26 already identified.

---

# 6. Is "reference imagery as a consumable that expires" defensible?

**Half of it is strongly defensible and the other half is a designer's fantasy.
Take the half that is real.**

**Defensible — imagery as a scarce, suppliable, denialable resource.** Documented,
not speculative:

- In March 2025 the US suspended Ukrainian access to the Global Enhanced GEOINT
  Delivery programme and Maxar confirmed it was suspending sharing of
  US-government-commissioned imagery. Reporting stated that widespread GPS
  jamming *combined with* loss of high-resolution imagery would hamper targeting
  and obstruct the deep-strike campaign, with a named Unmanned Systems Forces
  figure quoted to that effect [R, snippet]. Headlines called Ukraine's drone
  pilots "effectively blinded".
- Ukraine's DELTA consolidates satellite imagery, drone orthophotos and 3D
  terrain, and **"notifies users when updated imagery becomes available, allowing
  units to identify changes"** [R, snippet]. Commercial imagery reaches a
  soldier's device in as little as 15 minutes [R].

Reference data has a source, a pipeline, a latency, an update notification and a
political off-switch — **every property a game resource needs.** A stockpile
produced by reconnaissance assets, consumed by deep-strike sorties, denied by
losing your recon wing, and inherently more plentiful for the defender is a
first-class mechanic and directly supported by reporting.

**Fantasy — imagery that expires on a timer.** No evidence for a decay curve, and
what evidence exists points the other way: imagery spanning nineteen years is
used successfully in cross-season matching [M], military practice acquires
reference imagery weeks-to-months before flight as routine [M], and production
systems deliberately select features that do not change [M]. **A "this imagery is
14 days old, accuracy −20%" mechanic invents a number nobody has measured and
that the physics argues against.**

**What to build instead.** Make the reference a **coverage** resource, not a
**freshness** resource:

- Reference data covers *specific ground*. You have it or you don't. Acquiring it
  costs reconnaissance sorties over ground you do not control.
- Instead of expiry, make it **invalidated by events the player can see**: heavy
  bombardment of a sector, a destroyed settlement, flooding, or the arrival of
  winter. Those are discrete, legible, drawable, and each maps onto a real
  failure mode from section 1.3. Snow is the best of them: it is real, it is
  seasonal, it is visually obvious, and it hits optical matching hard while
  leaving elevation matching alone.
- If the game wants *one* continuous decay term, tie it to fighting intensity in
  that sector, not to the calendar. That is defensible as an inference and should
  be labelled as one.

---

# 7. The options table

Accuracy figures are real-world metres. Divide by ~12 for map metres. Cost is
order-of-magnitude for the navigation subsystem only.

| Option | Real accuracy | Error behaviour | Mass / power | Cost | Beaten by |
|---|---|---|---|---|---|
| GNSS, plain | 3–10 m | bounded | ~10 g, <1 W | $10s | any jammer |
| GNSS + CRPA (4–16 element) | 3–10 m | bounded | 100s of g | $1k–10k+ | enough jammers from enough directions; spoofing |
| MEMS inertial only, cheap | cube law: ~10 m @1 min, ~2.6 km @10 min | **unbounded** | ~10 g, <1 W | $10s | time |
| MEMS inertial, tactical grade | ~350 m @10 min | **unbounded** | 100s of g | $1k–20k | time |
| Air-data dead reckoning (fixed-wing) | 0.2–2% of distance | unbounded, linear | pitot + baro, negligible | $100s | unknown wind |
| Visual-inertial odometry | 1–3% of distance | unbounded, linear | 100–250 g, 7–25 W | $500–5k | darkness, smoke, featureless ground, cloud below the aircraft |
| Terrain-referenced nav (DEM) | 3–10 m rough; 10 m+ smooth; fails flat | **bounded** | radar altimeter, 100s of g; emits | $5k+ | **flat terrain**; being an emitter; forest canopy |
| Optical scene matching to imagery/3D terrain | **10–30 m** | **bounded** | camera + 100–250 g compute | $1k–10k | snow, smoke, darkness (unless IR), cloud below, no reference data for that ground, destroyed/changed structures |
| Celestial (cheap, night-only) | ~4 km | bounded but coarse | ~200 g, few W | $100s | cloud; daylight; only tells you the province |
| Celestial (Astradia class, day+night) | attitude to arc-seconds; position via INS vertical → 10s–100s of m | **bounds heading drift**, does not reset position | <3 kg, shoebox | **~€250k** | cloud; cost |
| Astro-inertial, strategic (SR-71 class) | <0.5 nmi after ~20,000 km | bounded | large, gimballed | very high | cloud; not available at this scale |

---

# 8. What is genuinely uncertain or contested

1. **No decay-versus-age curve exists.** The central premise of the "imagery
   expires" mechanic has no published measurement behind it, in either direction.
   Anyone supplying one is estimating.

2. **Every fielded accuracy figure is a vendor's.** OSCAR's 20 m, Raptor's 10 m,
   VNS01's 30 m, Bavovna's 0.5% — self-reported, untested externally, with no
   stated denominator (does a sortie that lost lock and crashed count?). Their
   *agreement* is the reassuring part; the individual numbers are not.

3. **The "15% inaccuracies over deserts" Gulf War TERCOM claim** surfaced only in
   a low-quality AI-generated encyclopedia entry. Do not build on it.

4. **The SR-71 "90 m / 300 ft" figure is probably a conflation** with the half-
   nautical-mile terminal accuracy from the same article. Do not use it.

5. **Astradia's ~€250k price is a search snippet**, not a Sodern quotation. The
   order of magnitude is solid; the digit is not.

6. **Whether cheap celestial reaches expendable airframes by 2028 is genuinely
   contested.** I found no fielded example and no programme, but the physics does
   not forbid it and the UniSA work shows parts cost is trivial. My inference
   that it stays expensive rests on the daylight requirement and the
   vertical-reference problem, neither of which miniaturisation fixes.

7. **The cube-law inertial table is computed, not measured.** The formulae are
   textbook and anchored at both ends by measured figures, but the residual bias
   per column is my choice. Treat the *shape* as solid, each cell as ±3×.

8. **The OSCAR "works in fog" claim is not credible as stated** [I]. Fog defeats
   visible-band imaging by definition — thin haze, an infrared channel, or
   marketing.

9. **Cloud figures in 2.5 are placeholders** from a sunshine-hours statistic; the
   weather document should replace them.

10. **Altitude is unmodelled and matters a lot.** Cloud, scene scale, radar
    altimeter footprint, star visibility and optical resolution all depend on it.

---

## Sources

Retrieved 14 September 2026. Items marked *(snippet)* were read only as search
result summaries because the egress proxy blocked the page.

- [TERCOM — Wikipedia](https://en.wikipedia.org/wiki/TERCOM) *(snippet)*
- [Enhanced Terrain-Referenced Navigation Through Adaptive Radar Altimeter Error Estimation — Int. J. Aeronautical and Space Sciences](https://link.springer.com/article/10.1007/s42405-024-00881-8) *(snippet)*
- [Accurate Measurement Calculation Method for Interferometric Radar Altimeter-Based Terrain Referenced Navigation — PMC](https://ncbi.nlm.nih.gov/pmc/articles/PMC6480591) *(snippet)*
- [Image Processing for Tomahawk Scene Matching — Irani & Christ, JHU APL Technical Digest V15 N03](https://www.jhuapl.edu/Content/techdigest/pdf/V15-N03/15-03-Irani.pdf) *(snippet; fetch blocked)*
- [Digital Scene Matching Area Correlator (DSMAC) — SPIE 0238](https://www.spiedigitallibrary.org/conference-proceedings-of-spie/0238/1/Digital-Scene-Matching-Area-Correlator-DSMAC/10.1117/12.959130.short) *(snippet)*
- [Terrain Contour Matching (TERCOM): A Cruise Missile Guidance Aid — SPIE 0238](https://www.spiedigitallibrary.org/conference-proceedings-of-spie/0238/1/Terrain-Contour-Matching-TERCOM-A-Cruise-Missile-Guidance-Aid/10.1117/12.959127.short) *(snippet)*
- [Cruise Missile Support Activity (CMSA) — FAS/IRP](https://irp.fas.org/agency/dod/uspacom/cmsa/) *(snippet)*
- [Season-invariant GNSS-denied visual localization for UAVs — Kinnari, Verdoja, Kyrki, arXiv 2110.01967](https://arxiv.org/abs/2110.01967) *(snippet; fetch blocked)*
- [LSVL: Large-scale season-invariant visual localization for UAVs — arXiv 2212.03581](https://arxiv.org/abs/2212.03581) *(snippet)*
- [SIVL code repository — Aalto Intelligent Robotics](https://github.com/aalto-intelligent-robotics/sivl) (fetched)
- [High-precision visual geo-localization of UAV based on hierarchical localization — Expert Systems with Applications](https://www.sciencedirect.com/science/article/abs/pii/S0957417424029312) *(snippet)*
- [Robust Visual SLAM for UAV Navigation in GPS-Denied and Degraded Environments — arXiv 2605.03678](https://arxiv.org/html/2605.03678v1) *(snippet)*
- [What makes visual place recognition easy or hard? — arXiv 2106.12671](https://arxiv.org/pdf/2106.12671) *(snippet)*
- [Visual Localization across Seasons Using Sequence Matching — PMC5713190](https://pmc.ncbi.nlm.nih.gov/articles/PMC5713190/) *(snippet)*
- [Raptor — GPS-Denied UAV Navigation & Coordinate Extraction, Vantor/Maxar](https://www.maxar.com/maxar-intelligence/products/raptor) *(snippet; fetch blocked)*
- [Maxar launches Raptor — press release](https://www.maxar.com/press-releases/maxar-launches-raptor-a-first-of-its-kind-software-that-unlocks-next-gen-gps-resilience-for-autonomous-systems) *(snippet)*
- [Twist Robotics develops OSCAR — Ukrainska Pravda, 28 Jan 2026](https://www.pravda.com.ua/eng/news/2026/01/28/8018266/) *(snippet; fetch blocked)*
- [Ukraine Gives Drones Vision-Based Navigation to Push Past Heavy Jamming — The Defense Post, 29 Jan 2026](https://thedefensepost.com/2026/01/29/ukraine-drones-vision-navigation/) *(snippet; fetch blocked)*
- [Ukraine's Shark-M Drone Adds GNSS-Denied Flight Capability — The Defense Post, 28 Jul 2026](https://thedefensepost.com/2026/07/28/ukraine-shark-m-drone/) *(snippet)*
- [UAV Navigation Updates VNS01 with Terrain Referenced Navigation & Map Matching — UST, Sept 2026](https://www.unmannedsystemstechnology.com/2026/09/uav-navigation-updates-vns01-visual-navigation-system-with-terrain-referenced-navigation-map-matching/) *(snippet)*
- [GNSS-Denied Navigation Kit — UAV Navigation](https://www.uavnavigation.com/products/navigation-systems/gnss-denied-navigation-kit) *(snippet)*
- [Ukraine expands DELTA with satellite imagery, drone orthophotos and 3D terrain data — defence-industry.eu](https://defence-industry.eu/ukraine-expands-delta-with-satellite-imagery-drone-orthophotos-and-3d-terrain-data-to-sharpen-battlefield-planning/) *(snippet)*
- [Maxar confirms US block on satellite images used to help Ukraine drone pilots — Anadolu Agency](https://www.aa.com.tr/en/americas/maxar-confirms-us-block-on-satellite-images-used-to-help-ukraine-drone-pilots-in-war-against-russia/3503600) *(snippet)*
- [Astradia star tracker designed to fill in for GNSS — GPS World](https://www.gpsworld.com/astradia-star-tracker-designed-to-fill-in-for-gnss/) *(snippet)*
- [Sodern announces the commercial launch of Astradia — SatNews, 27 May 2025](https://satnews.com/2025/05/27/sodern-announces-the-commercial-launch-of-their-astradia-star-tracker/) *(snippet)*
- [Sodern unveils Astradia star tracker for aircraft navigation — AeroTime](https://www.aerotime.aero/articles/sodern-astradia-star-tracker-gps-gnss-denied-navigation) *(snippet)*
- [An Algorithm for Affordable Vision-Based GNSS-Denied Strapdown Celestial Navigation — Drones 8(11):652](https://www.mdpi.com/2504-446X/8/11/652) *(snippet; fetch blocked)*
- [GPS alternative for drone navigation using visual data from stars — University of South Australia](https://www.unisa.edu.au/media-centre/Releases/2024/gps-alternative-for-drone-navigation-using-visual-data-from-stars/) *(snippet)*
- [SR-71's "R2-D2" Could Be The Key To Winning Future Fights In GPS Denied Environments — The War Zone](https://www.twz.com/17207/sr-71s-r2-d2-could-be-the-key-to-winning-future-fights-in-gps-denied-environments) *(snippet)*
- [LN-120G Stellar-Inertial-GPS Navigation — Northrop Grumman datasheet mirror](https://pdf4pro.com/view/ln-120g-stellar-inertial-gps-navigation-northrop-554cd6.html) *(snippet; fetch blocked)*
- [Deterministic Heading-Independent Celestial Localization Measurement Model — NAVIGATION 69(3)](https://navi.ion.org/content/69/3/navi.529) *(snippet)*
- [The electromechanical angle computer inside the B-52 bomber's star tracker — righto.com](https://www.righto.com/2026/04/B-52-star-tracker-angle-computer.html) *(snippet)*
- [Problem of the Vertical Deflection in High-Precision Inertial Navigation — Gyroscopy and Navigation](https://link.springer.com/article/10.1134/S2075108720040094) *(snippet)*
- [What does "Tactical Grade" mean for a MEMS IMU? — Analog Devices EngineerZone](https://ez.analog.com/mems/w/documents/4111/what-does-tactical-grade-mean-for-a-mems-imu) *(snippet)*
- [Exploring Tactical Grade MEMS IMUs — Inertial Labs](https://inertiallabs.com/exploring-tactical-grade-mems-imus/) *(snippet)*
- [How Advanced Navigation Strengthens Resilient PNT in GPS Contested Environments — Unmanned Systems Technology](https://www.unmannedsystemstechnology.com/feature/how-advanced-navigation-strengthens-resilient-pnt-in-gps-contested-environments/) *(snippet)*
- [Real Flight Data, Real Results: Demonstrating INS Aiding Tech for GPS-Denied Navigation — Advanced Navigation](https://www.advancednavigation.com/tech-articles/real-flight-data-real-results-demonstrating-ins-aiding-technologies-for-gps-denied-resilient-pnt/) *(snippet; fetch blocked)*
- [Hybrid inertial navigation 99.98% accuracy — Bavovna.AI](https://bavovna.ai/news/hyrbid-inertial-navigation-99-98-accuracy/) *(snippet)*
- [Event-Based Visual/Inertial Odometry for UAV Indoor Navigation — PMC11722967](https://pmc.ncbi.nlm.nih.gov/articles/PMC11722967/) *(snippet)*
- [Navion: A 2mW Fully Integrated Real-Time Visual-Inertial Odometry Accelerator — MIT LEAN](https://lean.mit.edu/papers/navion-journal) *(snippet)*
- [Jetson Orin Nano Power Consumption (2026) — edgeaistack](https://edgeaistack.ai/blog/jetson-orin-nano-power-consumption/) *(snippet)*
- [From Shahed to Geran: How Russia Continues to Reinvent the One-Way Attack Drone — CSIS](https://www.csis.org/analysis/shahed-geran-how-russia-continues-reinvent-one-way-attack-drone) *(snippet)*
- [Europe Celestial Navigation Star Tracker for Military Market 2026 to 2034 — IntelMarketResearch](https://www.intelmarketresearch.com/europe-celestial-navigation-star-tracker-for-military-market-market-46368) *(snippet)*
- [Climate and Average Weather Year Round in Kiev, Ukraine — Weatherspark](https://weatherspark.com/y/96633/Average-Weather-in-Kiev-Ukraine-Year-Round) *(snippet)*
- [Evaluation and improvement of the vertical accuracy of the global open DEM under forest environment — Geocarto International](https://www.tandfonline.com/doi/full/10.1080/10106049.2025.2453024) *(snippet)*
