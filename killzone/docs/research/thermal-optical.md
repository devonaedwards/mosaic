# Thermal and electro-optical detection

Research for the KILL ZONE sensor and signature model. Read `BRIEF.md` first for
what the game currently assumes and why these numbers matter.

Confidence markers used throughout:

- **[M]** measured — a published experiment or field trial with numbers.
- **[S]** specification — a manufacturer or programme figure. Vendor optimism applies.
- **[J]** journalism — reporting on Ukraine or Russia, usually second-hand, often
  repeating a claim rather than verifying it.
- **[D]** derived — my arithmetic on top of a cited figure, stated as such.
- **[G]** guess — reasoning from physics with no source. Treat as a placeholder.

Two caveats up front. First, most defence-sector sites block direct fetching from
this environment, so several figures below come from search-engine summaries of
those pages rather than the pages themselves; where that is the case the figure is
marked and the URL given so a human can check it. Second, almost nothing in the
open literature gives you what a game designer actually wants — the *same sensor,
the same target, measured at noon and again at 0300*. That experiment is either
classified or nobody bothered to publish it. Everything in the day/night section
is assembled from adjacent evidence, and I say so where it matters.

---

## 1. Summary of findings

**The game's biggest error is not a multiplier, it is the assumption that day/night
thermal degradation is a property of the weather.** It is overwhelmingly a property
of *the target's temperature* and *what is behind it*. A turbojet drone at 600 °C
exhaust barely notices whether the sun is up. A small electric quadcopter whose
hottest part is a 60 °C battery is a completely different sensor problem at noon
than at 0300, and the difference is mostly about whether it is silhouetted against
cold sky or lost in sun-baked ground clutter. One global multiplier cannot express
this, and the current one splits the difference in a way that is wrong for both
ends of the roster. **[D]**

**Thermal crossover happens at dawn and dusk, so the game's twilight figure of 0.90
is backwards.** The literature is explicit: crossover — the moment target and
background radiate identically and contrast goes to zero — "typically" occurs at
dawn and dusk, and a conventional single-band thermal sensor can lose the target
outright. **[M]** The game currently treats twilight as better than daytime. It
should be the worst window of the day, not the second-best.

**Small electric drones are genuinely nearly cold, and the operational reporting
agrees.** Electric motors run 40–80 °C and packs reach about 60 °C under load, with
the battery, not the motors, the dominant radiator. **[M/S]** Against a background
at 20 °C that is a delta-T of 20–40 K over a radiating area of a few tens of square
centimetres. Reporting on the Lancet — electric, pusher prop — says it "emits very
little heat and is almost undetectable" to IR-guided interceptors and is "nearly
undetectable until close range." **[J]** The game's thermal signature of 8 for a
small electric quad is defensible and rare among game numbers in being about right.

**The electric-versus-combustion gap in the game is directionally correct but the
combustion end is compressed.** Two-stroke exhaust runs 325–345 °C at nominal load
and four-stroke 360–380 °C, with four-stroke peaks of 600–900 °C during the exhaust
stroke. **[M]** The Shahed-136's Mado MD-550 is a ~50 hp four-cylinder two-stroke,
a copy of the Limbach L550E. **[S]** That is an order-of-magnitude-plus jump in
delta-T over an electric quad, not the roughly 7.5× the game's 8-versus-60 implies
once you account for the square-root reach law flattening everything.

**Cooled versus uncooled is worth about 2× in range, not 10×.** NETD figures:
cooled 10–30 mK, uncooled 30–120 mK; typical microbolometer ~45 mK against ~18 mK
for a cryogenically cooled photon detector. **[S]** Uncooled sensitivity is
"2 to 6 times worse." **[S]** The one clean paired range figure I found is 5.2 km
uncooled against 11.0 km cooled — a factor of 2.1. **[S]** Counter-UAS commentary
claiming cooled/uncooled is "the difference between seeing a target at 1 km versus
10 km" is marketing; treat it as an upper bound on a favourable target, not a rule.
**[J]**

**Manufacturer-declared thermal ranges are not achievable.** Testing against the
standard NATO 2.3 m × 2.3 m target at a 2 K temperature difference, "none of the
tested cameras reached the detection range declared by the manufacturer," with
large scatter between units. **[M]** Every vendor number in the table below should
be discounted; my working assumption is 0.5–0.7× of claim in field conditions.

**Detection, recognition and identification ranges differ by roughly 6× end to
end.** NATO practice is about 1 cycle across the critical dimension for detection,
3 for recognition and 6 for identification at 50 % probability; Johnson's original
figures were 1 / 4 / 6.4. **[M]** So recognition happens at roughly a third of
detection range and identification at a sixth. The game models only detection,
which is probably right for a real-time strategy game, but it means the game
cannot express the thing Ukraine actually struggles with: knowing *which* drone
you have found in time to decide whether to spend an interceptor on it.

**The game's "high-altitude" penalties are wrong in sign for both optical and
thermal.** A target silhouetted against clear sky is the easiest background either
sensor will ever get: uniform, high contrast, no clutter. IR cameras "can often
detect drone motion represented by as small as a 2×2 pixel cluster, given the sky's
cold background." **[S]** The hard case is the opposite one — a drone flying
nap-of-the-earth against warm, textured, sun-loaded ground. The game penalises
altitude and implicitly rewards terrain-hugging, which is exactly inverted from why
FPV crews fly low.

---

## 2. Table of measured and claimed ranges

All ranges are real-world metres. Divide by 12 for map metres.

| Figure | Value | Type | Source |
|---|---|---|---|
| Cooled IR detector NETD | 10–30 mK | [S] | Teledyne FLIR OEM |
| Uncooled IR detector NETD | 30–120 mK | [S] | Teledyne FLIR OEM |
| Typical microbolometer NETD | ~45 mK | [S] | moviTHERM |
| Cooled photon detector NETD | ~18 mK | [S] | moviTHERM |
| Uncooled vs cooled sensitivity penalty | 2–6× worse | [S] | moviTHERM / trade press |
| Paired detection range, uncooled vs cooled | 5.2 km vs 11.0 km | [S] | LightPath / science.gov aggregate |
| Current cooled MWIR state of art | 8 µm pixels, 1280×1024, 30 mK | [S] | LightPath |
| Common IR formats in C-UAS | 320×256, 640×512 | [S] | Teledyne FLIR OEM |
| Minimum IR detection cue vs sky | 2×2 pixel cluster | [S] | Teledyne FLIR OEM |
| NATO test target / delta-T used | 2.3 m × 2.3 m at 2 K | [M] | *Sensors* 19(15):3313, 2019 |
| Cameras reaching declared detection range | none of those tested | [M] | *Sensors* 19(15):3313, 2019 |
| DRI cycle criteria (NATO) | 1 / 3 / 6 cycles at 50 % | [M] | Johnson criteria literature |
| DRI cycle criteria (Johnson original) | 1 / 4 / 6.4 cycles | [M] | Johnson criteria literature |
| Thermal+LiDAR detection accuracy, day | 55.9 % | [M] | *Sensors Int.* S2405959521001818 |
| Thermal+LiDAR detection accuracy, night | 57.5 % | [M] | same |
| Visible+LiDAR detection accuracy, night | 49.9 % | [M] | same |
| Daytime terrain contrast vs night | 2–3× greater | [M] | thermal-translation literature |
| Vegetation/soil IR separability 2000–0600 | effectively none | [M] | same |
| Electric drone motor temperature | 40–80 °C | [S] | C-UAS technical commentary |
| Electric drone battery under load | ~60 °C, dominant radiator | [M/S] | same |
| Two-stroke exhaust gas temp, nominal load | 325–345 °C | [M] | *Case Stud. Therm. Eng.* S2214157X14000021 |
| Four-stroke exhaust gas temp, nominal load | 360–380 °C | [M] | same comparison set |
| Four-stroke peak exhaust-stroke temp | 600–900 °C | [M] | four-stroke SI engine study |
| Shahed-136 powerplant | Mado MD-550, ~50 hp 4-cyl two-stroke | [S] | GlobalSecurity |
| Shahed-136 MWIR detection claim | >3–5 km | [J] | drone-warfare.com analysis |
| Shahed-238 / Geran-3 (turbojet) | "increased thermal signature", tracked by it | [J] | CSIS Missile Threat |
| Lancet thermal signature | "almost undetectable", electric | [J] | Grey Dynamics / Army Recognition |
| Russian Yolka interceptor EO module | detects 1 m wingspan at 700–1000 m | [J] | Russian-language reporting, aggregated |
| Wide-angle static camera drone detection | ~1 km, target a few dozen pixels | [M] | IPSJ CVA 10.1186/s41074-019-0059-x |
| Baba Yaga onboard thermal, target ID | ~120 m | [J] | Army Recognition |
| Baba Yaga onboard thermal, person / vehicle | 2 km / 10 km | [J] | TheDefenseWatch — **implausible, see §6** |
| FPV-carried thermal effective working range | 100–500 m | [J] | star-navi commentary |
| Multi-sensor C-UAS vs DJI Phantom | 5 km detect, 3 km visual track | [S] | ADDS vendor claim |
| DJI Mavic 3T thermal | 640×512, ≤50 mK, 8–14 µm, 1.4 mrad IFOV | [S] | DJI Enterprise specs |
| Fused multi-modal C-UAS pipeline | 96.1 % detection, 3.2 % false alarm | [M] | DroneShield-AI, arXiv 2606.11687 |
| Uncooled sensor cost fall, past decade | 65–70 % | [S] | microbolometer market analysis |
| Entry-level uncooled module price | below USD 250 | [S] | same |
| Uncooled share of global IR shipments | >85 % | [S] | same |

---

## 3. Thermal crossover and solar loading

The mechanism is simple and the game has half of it. Sunlight heats everything in
the scene. Terrain, being mostly low-thermal-mass surface material, heats fast and
a lot; a metal target heats fast too. Twice a day the two curves cross, and at that
moment the thermal image goes flat. The 2017 Beihang study imaged several targets
outdoors over a full 24-hour cycle specifically to characterise this, and found
that a conventional single-band MWIR sensor loses targets during the diurnal cycle,
which multispectral imaging in the 3–5 µm band recovers. **[M]**

What that means for the game:

- **Daytime is worse, but not uniformly.** Daytime *scene* contrast is 2–3× higher
  than night because solar loading differentiates vegetation from soil. **[M]** More
  scene contrast is worse for finding a small target in it — the clutter gets
  brighter along with everything else — but it does not touch a target seen against
  sky, because sky is not sun-loaded in the thermal band.
- **Night is the flat case.** Vegetation and soil are "nearly identical" in thermal
  intensity from 2000 to 0600. **[M]** A featureless cold background is exactly what
  you want: anything warm stands out.
- **Dawn and dusk are the disaster.** This is where the game is wrong. Crossover
  "typically" happens at dawn and dusk. **[M]**

The only paired day/night measurement I could find is indirect but useful: a
thermal-plus-LiDAR detection stack scored 55.9 % by day and 57.5 % at night, while
the visible-plus-LiDAR stack fell to 49.9 % at night. **[M]** That is a road-vehicle
benchmark, not drones, and it measures classifier accuracy rather than range — but
it says thermal is *slightly better* at night and visible is clearly worse, which
is the shape the game needs, at a much gentler magnitude than the game applies.

**Verdict: the day/night spread is roughly right in direction and too wide in
magnitude, and the twilight value is wrong in sign.**

---

## 4. Thermal signatures by drone type

| Class | Hottest component | Temp | Radiating area | Delta-T vs 20 °C ground | Delta-T vs cold sky |
|---|---|---|---|---|---|
| Small electric quad | LiPo pack under load | ~60 °C **[M/S]** | ~0.01 m² | ~40 K | ~80–100 K |
| Heavy electric multirotor | 6–8 motors + packs | 60–90 °C **[D]** | ~0.1 m² | 40–70 K | 100–130 K |
| Two-stroke loitering munition | exhaust pipe / plume | 325–345 °C **[M]** | ~0.02 m² + plume | ~310 K | ~350 K |
| Four-stroke strike drone | exhaust, engine block | 360–380 °C, peaks to 900 °C **[M]** | ~0.05 m² + plume | ~350 K | ~400 K |
| Turbojet strike drone | exhaust nozzle + plume | 500–700 °C **[G]** | large plume | ~600 K | ~650 K |
| Main battle tank | engine deck, exhaust | 80–120 °C deck **[G]** | ~2–4 m² | 60–100 K, **~0 at crossover** | n/a — ground target |

Three things fall out of this that the game does not currently express.

**Why small electric drones are nearly cold.** It is not that the battery is cool —
60 °C is warm. It is that the radiating area is minuscule and it is mostly hidden
inside a plastic shell. Radiant flux at the sensor scales with area × delta-T (to
first order in the small-delta-T limit), and a 0.01 m² source at 40 K over ambient
puts out a few watts of excess in-band. At 1 km, with a 640×512 sensor over a
sensible field of view, that source is well under one pixel. You are not resolving
it; you are hoping it raises one pixel above noise. That is why the FLIR guidance
about a 2×2 pixel cluster against the sky's cold background is the operative
criterion **[S]** and why FPV-carried thermal is described as working best at
100–500 m. **[J]**

**Why combustion drones are a different problem entirely.** A 345 °C exhaust at a
tenth the area radiates roughly two orders of magnitude more excess power than a
60 °C battery, before you count the plume. This is why the Shahed-136 is credibly
called detectable beyond 3–5 km by modern MWIR **[J]** while the Lancet, same rough
size class but electric, is "almost undetectable." **[J]** The propulsion choice, not
the airframe, decides the thermal problem.

**Why the tank is the worst daytime case in the roster, not the best.** A tank is a
large flat-sided object sitting in the same sunlight as the ground around it. Its
engine deck is hot, but its glacis, turret roof and skirts solar-load to whatever
the surrounding terrain does. The classic thermal-crossover failure is a parked
armoured vehicle at dusk. The game gives the tank thermal 90 and applies the same
0.55 daytime multiplier as it applies to a turbojet — which is exactly backwards.
The turbojet should barely care; the tank should care enormously.

---

## 5. Optical and visible detection

Optical detection of a small drone is an angular-size problem with a contrast
gate on top. Two anchors:

- A static wide-angle camera detects drones "up to approximately 1 km", with the
  target "as small as a few dozens of pixels." **[M]**
- The Russian Yolka interceptor's electro-optical module is reported to detect a
  100 cm wingspan drone at 700–1000 m. **[J]** This is the most useful single figure
  I found, because it is a cheap fielded system against a stated target size.

Scale that to a 30 cm FPV quad by angular size and you get roughly 210–300 m for
the same optic. **[D]** Scale it the other way to a 7 m tank and you get 5–7 km,
which is about where crewed day optics actually acquire vehicles. The ratio between
the smallest and largest targets in the roster is therefore **around 20–25× in
optical detection range**, in identical conditions.

Modifiers, in rough order of importance:

- **Background.** Sky is the best case: uniform, no clutter, the drone is a dark
  silhouette. Ground clutter is the worst: the target is a few pixels against a
  textured, moving, similarly-coloured scene, and "distant micro-drones occupy only
  a few pixels and are often confused with birds or background clutter." **[M]**
- **Night.** A small unlit drone against a dark sky is effectively invisible to a
  passive daylight camera. This is the whole reason Ukrainian night bombers work.
  Low-light CMOS under moonlight recovers some of it against large targets; against
  a 30 cm matte quad it recovers very little.
- **Haze and cloud.** Visible-band attenuation follows meteorological visibility
  closely; a 5 km visibility day cuts useful optical detection far more than it cuts
  LWIR, which is one of the standing arguments for thermal as the all-weather layer.
- **Sun angle.** Looking into the sun kills a sky-silhouette track outright. Looking
  with the sun behind you against sky is close to best case.

### Detection vs recognition vs identification

Using NATO's 1 / 3 / 6 cycles at 50 % probability **[M]**, and the Yolka anchor:

| Task | 1 m drone | 0.3 m quad | 7 m tank |
|---|---|---|---|
| Detection ("something is there") | 700–1000 m **[J]** | 210–300 m **[D]** | 5–7 km **[D]** |
| Recognition ("it is a multirotor") | 230–330 m **[D]** | 70–100 m **[D]** | 1.7–2.3 km **[D]** |
| Identification ("it is a Baba Yaga, carrying") | 115–165 m **[D]** | 35–50 m **[D]** | 0.8–1.2 km **[D]** |
| Track maintenance (already acquired) | ~1.3× detection **[G]** | ~1.3× detection **[G]** | ~1.3× detection **[G]** |

The track-maintenance multiplier is my extrapolation and nothing more: once you
know where to look you can integrate frames, gate the search, and hold a target
below the cold-search threshold. It is the justification for the game's two-second
track hold, which is reasonable in kind if arbitrary in magnitude.

---

## 6. Ukraine reporting, and what to believe in it

The genuinely load-bearing reporting:

- **Baba Yaga / Vampire night bombers exist because of thermal.** The defining
  feature of the class is reliable night flying, enabled by combined thermal and
  optical cameras. **[J]** The Vampire's reach grew from ~20 km to ~60 km. **[J]**
- **Winter improves it materially.** Cold weather raises target-to-background
  contrast: soldiers, electronics and heaters are difficult to hide and their
  signatures accumulate. **[J]** This is the operational statement of the flat-cold-
  background effect the measurement literature describes.
- **The Russians counter it with IR, and Ukraine now counters *that*.** Russian
  operators have mass-produced interception of heavy bombers **[J]**, using thermal
  sighting systems, and some Baba Yagas now carry small infrared projectors
  specifically to dazzle the IR cameras used to intercept them at night. **[J]** An
  IR counter-countermeasure arms race is already live in 2026.
- **The stated limit against electric drones is real.** Commentary from the FPV side
  is unusually candid: thermal is *not* the preferred night sensor for FPV
  operations, because of low resolution (a 640×512 FLIR Vue Pro is called out as
  making detail identification hard), cost, and a 2.35–3.1 W power draw that shortens
  endurance. **[J]** And on the detection side, electric propulsion "produces minimal
  thermal trail," leaving drones like the Lancet "nearly undetectable until close
  range." **[J]**

What not to believe: the claim that a Baba Yaga's onboard thermal detects a person
at 2 km and ground equipment at 10 km **[J]** cannot be reconciled with the other
figure in circulation — target identification at ~120 m **[J]** — or with the physics
of a 640×512 uncooled core behind a short lens. Both appear in secondary sources
with no methodology. The 120 m figure is plainly an *identification* range and the
2 km/10 km figures are plainly *detection* claims for a far better sensor, and
somebody has collapsed the DRI distinction. Take the DRI-corrected middle: a good
uncooled drone thermal detects a person at a few hundred metres and a running
vehicle at 1–3 km, and identifies either at a fifth of that. **[D]**

---

## 7. Verdict on the game's numbers

### Thermal day 0.55 / night 1.25 / twilight 0.90 — **wrong, in three ways**

1. **The spread is too wide for hot targets.** Night/day is currently 2.27×. For a
   two-stroke or turbojet drone, the delta-T changes by maybe 10–15 % between noon
   and 0300, because a 345 °C exhaust does not care what the sun did to the terrain.
   Range scaling under the game's own square-root law gives a night/day ratio of
   about **1.07** for such a target. **[D]**
2. **It is too narrow for cold targets over ground.** A 60 °C battery pack against
   sun-baked ground at 50 °C is at delta-T ~10 K; at night against ground at 5 °C it
   is at ~55 K. That is a 5.5× swing in delta-T and roughly **2.3×** in range —
   coincidentally, almost exactly what the game currently applies to everything.
   The game has taken the cold-target-over-ground case and applied it universally.
3. **Twilight is inverted.** Crossover is at dawn and dusk. **[M]**

**What to use instead.** The right fix is to move the modifier from the sensor to
the signature and make it target-dependent. Because the game's reach already scales
as √(signature/100), a multiplier *m* on effective signature produces a range
multiplier of √*m*, which drops out of the physics for free.

| Time | Global reach multiplier | Additional delta-T multiplier on signature |
|---|---|---|
| Night | **1.15** (was 1.25) | 1.00 |
| Day | **0.85** (was 0.55) | see per-class table below |
| Twilight | **0.60** (was 0.90) | ×0.55 for all classes |

| Class | Daytime delta-T multiplier | Effective day reach multiplier |
|---|---|---|
| Electric (all) | 0.35 | 0.85 × √0.35 = **0.50** |
| Piston combustion | 0.80 | **0.76** |
| Turbojet | 0.90 | **0.81** |
| Ground vehicle | 0.40 | **0.54** |

Net effect: a small electric quad's daytime thermal reach lands at 0.50 — slightly
harsher than the current 0.55. A turbojet lands at 0.81 instead of 0.55, which is
the correction that matters. And a tank stays punished by day, correctly, for the
opposite reason to the quad.

If you will only accept one number, use **day 0.70, night 1.15, twilight 0.60.** It
is wrong for both ends of the roster but wrong by less than the current values, and
it fixes the twilight inversion, which is free.

### Optical night 0.35 — **too generous, should be 0.22**

Losing 65 % of reach at night implies a low-light-capable sensor and a target with
some inherent brightness. For the units that matter — small, matte, unlit drones —
a passive visible camera at night against a dark sky is close to useless, and the
existence of an entire Ukrainian night-bombing doctrine is the evidence. The one
measured paired figure, visible detection accuracy falling from ~56 % to 49.9 % at
night **[M]**, is for illuminated road vehicles and is the *best* case.

Use **0.22** as the global optical night multiplier. Consider an exception the
other way for units that emit light — a firing gun, a burning vehicle, a jammer
with running lights — if the engine supports per-unit overrides.

### "High-altitude" modifiers — **optical sign is wrong**

Current: acoustic ×0.35, radar ×1.20, optical ×0.70. The acoustic and radar figures
are outside this document's scope and look sane. The optical −30 % is wrong: a
sky-silhouetted target is the best optical background available. Recommend **optical
×1.00 at altitude, thermal ×1.30 at altitude**, and introduce the penalty where it
belongs — a **×0.55 optical and ×0.50 thermal modifier for targets flying
nap-of-the-earth against ground clutter**. That single change makes low-flying
approach a real tactic instead of a free one, which is what the war actually looks
like.

### Cooled/uncooled implied by the sensor table — **too tight**

Interceptor Battery thermal 500 against Main Tank thermal 300 is a 1.67× ratio. If
the battery is a cooled MWIR fire-control sensor and the tank is a second-generation
uncooled sight, the literature's paired figure is 2.1× (5.2 vs 11.0 km) **[S]**.
Recommend **battery 520, tank 250**, or leave the battery and drop the tank.

### Channel reliability — thermal 84 vs optical 88

Reversed for aerial targets. Against a sky background, thermal is the more reliable
discriminator (uniform cold background, motion cue on a 2×2 cluster), and optical is
the one that falls over in haze, glare and darkness. Recommend **thermal 88, optical
82**, and accept that optical's real weakness is already modelled by the night
multiplier.

---

## 8. Recommended signature values

| Unit | Thermal now | Thermal rec. | Visual now | Visual rec. | Reasoning |
|---|---|---|---|---|---|
| Small electric quad (FPV) | 8 | **8** | 15 | **6** | Thermal is right — one 60 °C pack, sub-pixel at range. Visual 15 gives the gun mount a 232 m acquisition on a 0.3 m airframe; scaled from the Yolka anchor it should be ~90 m map. 6 is still generous. |
| Fiber-optic quad | 8 | **9** | 15 | **6** | Same airframe; the spool and its drag mean slightly more motor load, so marginally hotter. Not a meaningful difference. |
| Electric heavy multirotor | 22 | **32** | 55 | **30** | 6–8 motors at 60–90 °C plus large packs is roughly 10× the radiating area of an FPV. Russians intercept these at night by IR **[J]**, so it must be a real thermal target. Visual 55 was too high for a 1.5 m airframe relative to a tank. |
| Fixed-wing recon | 25 | **14** or **38** | 30 | **40** | **Ambiguous — pick one.** If electric, 14. If it carries a small two-stroke, 38. The game cannot have it both ways and 25 is the average of two different aircraft. Visual raised: 3 m wingspan is a much bigger optical target than a quad. |
| Combustion loitering munition | 45 | **52** | 25 | **20** | Two-stroke at 325–345 °C **[M]**. Visual lowered: these are small (1–2 m) and often approach in a dive. |
| Combustion heavy strike drone | 60 | **72** | 45 | **38** | Shahed-136 class: ~50 hp two-stroke, exposed exhaust, credibly detectable beyond 3–5 km by MWIR **[J]**. 60 understates it against a cold-sky background. |
| Turbojet strike drone | 85 | **92** | 40 | **34** | Geran-3 class. "Increased thermal signature", tracked by that signature **[J]**. Should be the hottest thing in the sky. Visually it is smaller and faster than the piston version, not bigger. |
| Decoy drone (reflectors) | 25 | **40** | 30 | **22** | **Ambiguous.** Gerbera-type decoys are piston-powered, which makes them thermally *convincing* — that is the point of a decoy. If the intent is a cheap electric decoy, use 12 and accept that thermal cleanly unmasks it. |
| Main battle tank | 90 | **90** | 90 | **100** | Keep thermal 90 as the night value; the new per-class daytime multiplier handles crossover. Visual 100 anchors the top of the optical scale — it is the largest, most contrasty thing in the roster. |
| Jammer, transmitting | 40 | **48** | 70 | **75** | A high-power transmitter dumps kilowatts into an amplifier and a cooling loop. It is a genuine hot spot on a vehicle-sized visual target. |

### The structural problem you should know about

The square-root reach law caps the expressible ratio between the weakest and
strongest signature at **10×** (signature 1 → 0.10 reach, signature 100 → 1.00). The
real optical detection-range ratio between a 0.3 m quad and a 7 m tank is **20–25×**
**[D]**. **The game physically cannot represent how much harder small drones are to
see optically than vehicles.** My recommended visual values (6 vs 100) use 4.1× of
the available 10× and are already a compromise. If small-drone survivability against
optics ever feels wrong in playtest, the fix is not the signature table — it is
either an exponent below 0.5 for the optical channel, or shorter optical baselines
on the sensor side. `FINDINGS.md` §2 independently reached the same conclusion about
the gun mount's 600 m optical reach from gameplay evidence, which is a useful
convergence.

The thermal channel has the opposite problem and is fine: the real electric-to-
turbojet delta-T ratio is about 15× **[D]**, which under the square-root law needs a
signature ratio of ~11.5×. My recommended 8 → 92 gives exactly that.

---

## 9. What changes by 2028

**Uncooled sensors get cheap enough to be standard issue, not an asset.** Uncooled
module costs have fallen 65–70 % in a decade, entry-level modules are already under
USD 250, and uncooled parts are >85 % of global IR shipments with 12 µm pitch
detectors in million-unit annual production. **[S]** The market is forecast to grow
from USD 557 M in 2026 to USD 898 M by 2033. **[S]** By 2028 the reasonable
assumption is that *every* front-line vehicle and a meaningful fraction of
expendable drones carry a thermal sight. **Model change: raise thermal *reach* on
cheap platforms** — the Gun Mount should probably acquire a thermal channel it does
not currently have — rather than raising signatures.

**Classifiers close the gap faster than sensors do.** A fused multi-modal pipeline
already reports 96.1 % detection with a 3.2 % false-alarm rate. **[M]** The binding
constraint on small-drone detection has not been photons for several years; it has
been discriminating two warm pixels from a bird, a hot rock and sensor noise, and
that is a software problem on a software improvement curve. **Model change: raise
per-channel reliability for thermal and optical by roughly 5 points by 2028**
(thermal 88 → 93, optical 82 → 87) rather than raising reach.

**Multi-spectral fusion specifically defeats crossover.** This is the clearest
near-future finding in the whole document: the 2017 Beihang work exists precisely to
show that multispectral MWIR recovers targets a single-band sensor loses over the
diurnal cycle **[M]**, and SWIR is now being integrated "as a complementary imaging
layer" in C-UAS stacks alongside radar, visible and thermal. **[S]** **Model change:
a 2028-era sensor should have its twilight multiplier raised from 0.60 to ~0.95 and
its daytime multiplier improved, while a 2026-era sensor keeps the penalty.** If the
game has a tech-tier concept, "crossover immunity" is a clean, historically honest
upgrade to sell.

**Countermeasures arrive at the same time.** IR-suppressed designs are the obvious
response — exhaust mixing, plume shielding, low-emissivity coatings — and the
direction is already visible in the field: Baba Yagas carrying IR projectors to
dazzle Russian interceptor optics. **[J]** Expect combustion-drone thermal signatures
to be *designed down* by 2028 where the airframe can afford the weight. **Model
change: reduce combustion heavy strike drone thermal from 72 to ~58 and turbojet from
92 to ~80 in a 2028 roster**, and consider an active IR-dazzle ability that suppresses
enemy thermal reach for a few seconds. That last one is a good mechanic and it is
real.

**What does not change: small electric drones stay cold.** There is no technology
path that makes a 60 °C battery in a plastic shell into a bright IR target. The
electric-versus-combustion gap is the most durable finding here and should stay the
spine of the thermal model through any tech tier the game adds.

---

## 10. Genuinely uncertain or contested

- **No published same-system day/night range pair exists in open sources.** Every
  day/night number in §7 is derived from delta-T reasoning plus the game's own
  square-root law. Confidence: moderate on direction, low on magnitude.
- **Turbojet exhaust temperature for Shahed-238-class drones is my estimate**
  (500–700 °C) and is marked [G]. I found qualitative statements that the signature
  is larger, not a number.
- **The Yolka 700–1000 m figure** comes from Russian-language reporting reaching me
  through search summaries. It is the load-bearing anchor for the whole optical
  section and nobody has verified the primary source from this environment.
- **Baba Yaga onboard thermal ranges are contested by a factor of ~16** between
  sources, as discussed in §6. My reconciliation is reasoning, not evidence.
- **Two-stroke and four-stroke exhaust temperatures** are from general engine
  studies, not UAV powerplants specifically. Small air-cooled two-strokes at high
  specific output likely run hotter than the cited 325–345 °C.
- **Whether a modern low-light CMOS should count as "optical" or as its own channel**
  is a design question the research cannot answer. I assumed it is folded into
  optical, which is why I recommend 0.22 rather than something closer to zero.

---

## Sources

- *Target Detection over the Diurnal Cycle Using a Multispectral Infrared Sensor*, Sensors 17(1):56, 2017 — https://www.mdpi.com/1424-8220/17/1/56 · https://doi.org/10.3390/s17010056
- *Thermal Imager Range: Predictions, Expectations, and Reality*, Sensors 19(15):3313, 2019 — https://www.mdpi.com/1424-8220/19/15/3313
- *Measurement and Analysis of the Parameters of Modern Long-Range Thermal Imaging Cameras*, Sensors 21(17):5700, 2021 — https://doi.org/10.3390/s21175700
- Teledyne FLIR OEM, *Thermal Infrared Sensor Design Considerations for Counter-UAS Defense* — https://oem.flir.com/learn/discover/thermal-infrared-sensor-design-considerations-for-counter-uas-defense/
- Teledyne FLIR OEM, *Comparing Sensitivity of Thermal Imaging Cameras/Modules* — https://oem.flir.com/learn/discover/Comparing-Sensitivity-of-Thermal-Imaging-Cameras-Modules/
- moviTHERM, *What is NETD in a Thermal Camera?* — https://movitherm.com/blog/what-is-netd-in-a-thermal-camera/
- LightPath, *Defense UAV Thermal Imaging Cameras* — https://www.lightpath.com/blog/what-to-look-for-in-a-drone-thermal-imaging-camera
- LightPath, *Drones That See Heat: Thermal Imaging Explained* — https://www.lightpath.com/insights/drones-that-see-heat-thermal-imaging-explained
- Drone Warfare, *Counter-UAS 101 – Electro-Optical and Infrared Detection* — https://drone-warfare.com/counter-uas/eo-ir-detection/
- Drone Warfare, *Countering the Shahed-136: Detection, Intercept, Cost* — https://drone-warfare.com/counter-uas/countering-the-shahed-136/
- Drone Warfare, *Shahed-136 and Geran: Specs, Production, Jet Variants* — https://drone-warfare.com/research/shahed-136/
- Inside Unmanned Systems, *The Edge of Visibility: EO/IR System Design Realities for Modern C-UAS* — https://insideunmannedsystems.com/the-edge-of-visibility-eo-ir-system-design-realities-for-modern-c-uas/
- Inside Unmanned Systems, *MatrixSpace Expands AI Platform for Multi-Sensor Counter-UAS Threat Detection* — https://insideunmannedsystems.com/matrixspace-expands-ai-platform-for-multi-sensor-counter-uas-threat-detection/
- *A sensor fusion system with thermal infrared camera and LiDAR … deep learning based object detection*, Sensors International — https://www.sciencedirect.com/science/article/pii/S2405959521001818
- *Deep learning-based strategies for the detection and tracking of drones using several cameras*, IPSJ Trans. CVA — https://ipsjcva.springeropen.com/articles/10.1186/s41074-019-0059-x
- *A dataset for multi-sensor drone detection* — https://arxiv.org/pdf/2111.01888
- *A review of UAV Visual Detection and Tracking Methods* — https://arxiv.org/pdf/2306.05089
- *DroneShield-AI: A Multi-Modal Sensor Fusion Framework…* — https://arxiv.org/pdf/2606.11687
- *Experimental investigation of exhaust temperature … two-stroke engine*, Case Studies in Thermal Engineering — https://www.sciencedirect.com/science/article/pii/S2214157X14000021
- *Experimental Investigation of the Temperature Values in a Four-Stroke SI Engine* — https://www.researchgate.net/publication/343987871_Experimental_Investigation_of_the_Temperature_Values_in_a_Four-_Stroke_SI_Engine
- *Parametric design and IR signature study of exhaust plume … low flying UAV* — https://www.sciencedirect.com/science/article/pii/S2590123021001213
- GlobalSecurity, *Shahed-136 / Geran-2 Loitering Munition* — https://www.globalsecurity.org/military/world/iran/shahed-136.htm
- CSIS Missile Defense Project, *Shahed-238* — https://missilethreat.csis.org/missile/shahed-238/
- Forbes (Vikram Mittal), *Winter Enhances The Thermal Imaging Systems On Ukraine's Bomber Drones*, 22 Jan 2026 — https://www.forbes.com/sites/vikrammittal/2026/01/22/winter-enhances-the-thermal-imaging-systems-on-ukraines-bomber-drones/
- Forbes (Vikram Mittal), *Ukraine's 'Baba Yaga' Drones Are Becoming Russia's Latest Nightmare*, 19 Dec 2025 — https://www.forbes.com/sites/vikrammittal/2025/12/19/ukraines-baba-yaga-drones-are-becoming-russias-latest-nightmare/
- Army Recognition, *Vampire Baba Yaga Drones Central to Ukrainian Counterattacks* — https://www.armyrecognition.com/focus-analysis-conflicts/army/conflicts-in-the-world/russia-ukraine-war-2022/vampire-baba-yaga-drones-central-to-ukrainian-counterattacks-on-russian-positions-in-kharkiv
- Wikipedia, *Baba Yaga (aircraft)* — https://en.wikipedia.org/wiki/Baba_Yaga_(aircraft)
- www1.ru, *Russian Operators Mass-Producing Interception of Heavy "Baba Yaga" Drones*, 6 Apr 2026 — https://www1.ru/en/news/2026/04/06/rossiiskie-operatory-postavili-na-potok-perexvat-tiazelyx-dronov-baba-iaga.html
- Ukrainska Pravda, *Russians unveil heavy drone described as equivalent of Ukraine's Baba Yaga*, 26 May 2026 — https://www.pravda.com.ua/eng/news/2026/05/26/8036480/
- Grey Dynamics, *Lancet 3: Russia's Spear in the Sky* — https://greydynamics.com/lancet-3-russias-spear-in-the-sky/
- star-navi, *Why Is Thermal Imaging Not Ideal for FPV Drone Surveillance at Night?* — https://star-navi.net/BlogsGernator/blogs/why-is-thermal-imaging-not-ideal-for-fpv-drone-surveillance-at-night.php
- star-navi, *Is Ukraine's Military Prepared for Night Operations with FPV Drones?* — https://star-navi.net/BlogsGernator/blogs/is-ukraines-military-prepared-for-night-operations-with-fpv-drones.php
- New Imaging Technologies, *Enhancing Counter-UAS Capabilities with SWIR Imaging* — https://new-imaging-technologies.com/news/counter-uas-with-swir/
- DJI Enterprise, *Mavic 3 Enterprise specifications* — https://enterprise.dji.com/mavic-3-enterprise/specs
- Unmanned Systems Technology, *Drone Camera Tracking Systems* — https://www.unmannedsystemstechnology.com/expo/drone-camera-tracking-systems/
- Infiniti Electro-Optics, *DRI (Detection, Recognition & Identification)* — https://www.infinitioptics.com/glossary/dri-detection-recognition-identification
- OSTI, *History and Evolution of the Johnson Criteria* — https://www.osti.gov/servlets/purl/1222446
- Grand View Research, *Microbolometer Market Size And Share Report, 2026–2033* — https://www.grandviewresearch.com/industry-analysis/microbolometer-market-report

**Sources I could not open.** WebFetch is blocked by the network egress proxy for
drone-warfare.com, forbes.com, oem.flir.com, insideunmannedsystems.com, mdpi.com and
pmc.ncbi.nlm.nih.gov, and direct curl is refused by the proxy. Figures attributed to
those sites in this document come from search-engine summaries of them, not from the
pages. They are marked with their source so anyone with unrestricted access can
verify them before a number goes into the build. The two I would check first are the
*Sensors* 2019 statement that no tested camera met its declared range, and the Yolka
700–1000 m optical figure, because §7 and §8 lean on both.
