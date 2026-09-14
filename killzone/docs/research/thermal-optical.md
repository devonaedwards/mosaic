# Thermal and electro-optical detection

Research for the KILL ZONE sensor and signature model. Read `BRIEF.md` first for
what the game currently assumes and why these numbers matter.

Confidence markers used throughout:

- **[M]** measured — a published experiment or field trial with numbers.
- **[S]** specification — a manufacturer or programme figure. Vendor optimism applies.
- **[J]** journalism — reporting on Ukraine or Russia, usually second-hand.
- **[T]** textbook — standard IR/atmospheric-optics engineering values, recalled from
  the handbook literature (Hudson, Holst, MODTRAN-derived tables) rather than fetched
  from a source in this session. **Directionally reliable, verify before shipping.**
- **[D]** derived — my arithmetic on top of a cited figure, shown so you can check it.
- **[G]** guess — reasoning from physics with no source. A placeholder, not a finding.

Two caveats up front. Most defence-sector sites block direct fetching from this
environment, so several figures come from search-engine summaries of those pages
rather than the pages themselves; the URLs are all in §13 so a human can check them.
And almost nothing in the open literature gives you what a designer actually wants —
*the same sensor, the same target, measured at noon and again at 0300*. That
experiment is either classified or nobody published it. §3 and §9 are assembled from
adjacent evidence, and §6 exists so you don't have to take my word for any of it: it
gives you the physics to compute the answer yourself.

---

## 1. Summary of findings

**The day/night multiplier is not the game's real problem — the missing
ground-clutter modifier is.** Working the radiometry (§6, validated against the
reported field anchors in §2), a small electric quad seen against cold night sky is
detectable at about 2.4 km by a decent uncooled imager. The *same drone, the same
sensor*, seen against sun-loaded ground at midday: **110 m**. That is a factor of
**21×**. The game expresses a factor of 2.27× and applies it to everything. **[D]**

**But the day/night pair itself is roughly right, for the case it was probably
written for.** Comparing like with like — sky-backed aerial target, day versus night
— the model gives **1.8–2.0×**, against the game's 2.27×. My first instinct was that
hot targets shouldn't care about time of day at all; the arithmetic says otherwise,
because against a cold sky the *airframe at ambient temperature* is already a strong
emitter and the sky's apparent temperature swings with the diurnal cycle too. I was
wrong and the calculation corrected me. Day 0.55 / night 1.25 should become roughly
**day 0.62 / night 1.15**, a small change. The large change is elsewhere. **[D]**

**Thermal crossover happens at dawn and dusk, so twilight 0.90 is wrong in sign.**
The literature is explicit that crossover — target and background radiating
identically, contrast to zero — "typically" occurs at dawn and dusk. **[M]** The game
currently treats twilight as better than daytime. It is the worst window of the day.

**Crossover is catastrophic for parked things and merely inconvenient for running
ones.** A powered target has an internal heat source that never crosses. A parked
tank whose only contrast is solar loading genuinely disappears: the model puts a
running tank at 7.35 km on a clear night and a cold parked one at dusk at 1.37 km, a
5.4× loss. **[D]** A flying drone's motors are always on, so it loses far less.

**Small electric drones are genuinely nearly cold, and the reporting agrees.**
Electric motors run 40–80 °C, packs reach ~60 °C under load, and the battery, not
the motors, is the dominant radiator. **[M/S]** Reporting on the Lancet — electric,
pusher prop — says it "emits very little heat and is almost undetectable" to
IR-guided interceptors, "nearly undetectable until close range." **[J]** The game's
thermal signature of 8 for a small electric quad is defensible and is one of the few
numbers in the table that is about right.

**Band choice is a real decision and the game could model it.** Blackbody arithmetic
**[D]**: against a 300 K background, a 600 K exhaust is **293× brighter** in mid-wave
(3–5 µm) but only **12.9×** brighter in long-wave (8–12 µm). Conversely, for
near-ambient targets long-wave delivers **11× more absolute signal per kelvin**
(1.98 vs 0.21 W/m²/K at 300 K). So: **cheap uncooled long-wave is the right sensor
for finding cold electric drones; expensive cooled mid-wave is the right sensor for
finding combustion and jet drones far away.** That is a genuine, defensible tech-tree
fork.

**Cooled versus uncooled is worth about 2–3× in range, not 10×.** NETD: cooled
10–30 mK, uncooled 30–120 mK **[S]**; uncooled sensitivity "2 to 6 times worse"
**[S]**. The one clean paired range figure found is 5.2 km uncooled against 11.0 km
cooled — 2.1×. **[S]** The model in §6 gives 2.4 km (good uncooled) versus 7.4 km
(cooled MWIR) against an FPV quad on a clear night, about 3×, most of which comes
from the longer focal length a cooled system can afford rather than from NETD.

**Manufacturer-declared thermal ranges are not achievable.** Against the standard
NATO 2.3 m × 2.3 m target at 2 K delta-T, "none of the tested cameras reached the
detection range declared by the manufacturer." **[M]** Every vendor number below is
discounted accordingly; the model's system efficiency term η (§6.7) is where that
lives.

**Detection, recognition and identification differ by ~6× end to end.** NATO practice
is ~1 cycle across the critical dimension for detection, 3 for recognition, 6 for
identification at 50 % probability; Johnson's original was 1 / 4 / 6.4. **[M]** In
pixels: **2 / 6 / 12**. Recognition at a third of detection range, identification at
a sixth.

**The game's "high-altitude" optical penalty is wrong in sign.** A target
silhouetted against clear sky is the best background either sensor will ever get.
IR cameras "can often detect drone motion represented by as small as a 2×2 pixel
cluster, given the sky's cold background." **[S]** The hard case is the opposite —
nap-of-the-earth flight against warm textured ground. The game penalises altitude
and implicitly rewards terrain-hugging, which is inverted from why FPV crews fly low.

**Optical detection of small drones is resolution-limited against sky and
contrast-limited to zero against ground.** Running Koschmieder (§8) with realistic
contrast: a 0.3 m quad against sky is detectable to the limit of the optic's
resolution (300 m on a wide search FOV, ~3 km on a narrow tracker); against ground
clutter its inherent contrast (~0.18) sits *below* the clutter-raised threshold
(~0.20) and the contrast-limited range is **zero at any distance**. **[D]** Motion
detection is the only thing that finds a small drone over ground optically. That is
worth a game mechanic.

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
| Paired detection range, uncooled vs cooled | 5.2 km vs 11.0 km | [S] | LightPath / aggregate |
| Current cooled MWIR state of art | 8 µm pixels, 1280×1024, 30 mK | [S] | LightPath |
| Common IR formats in C-UAS | 320×256, 640×512 | [S] | Teledyne FLIR OEM |
| Minimum IR detection cue vs sky | 2×2 pixel cluster | [S] | Teledyne FLIR OEM |
| NATO test target / delta-T | 2.3 m × 2.3 m at 2 K | [M] | *Sensors* 19(15):3313, 2019 |
| Cameras meeting declared range | none of those tested | [M] | *Sensors* 19(15):3313, 2019 |
| DRI criteria (NATO) | 1 / 3 / 6 cycles = 2 / 6 / 12 px | [M] | Johnson criteria literature |
| DRI criteria (Johnson original) | 1 / 4 / 6.4 cycles | [M] | Johnson criteria literature |
| Thermal+LiDAR detection accuracy, day | 55.9 % | [M] | *Sensors Int.* S2405959521001818 |
| Thermal+LiDAR detection accuracy, night | 57.5 % | [M] | same |
| Visible+LiDAR detection accuracy, night | 49.9 % | [M] | same |
| Daytime terrain contrast vs night | 2–3× greater | [M] | thermal-translation literature |
| Vegetation/soil IR separability 2000–0600 | effectively none | [M] | same |
| Electric drone motor temperature | 40–80 °C | [S] | C-UAS technical commentary |
| Electric drone battery under load | ~60 °C, dominant radiator | [M/S] | same |
| Two-stroke EGT, nominal load | 325–345 °C | [M] | *Case Stud. Therm. Eng.* S2214157X14000021 |
| Four-stroke EGT, nominal load | 360–380 °C | [M] | same comparison set |
| Four-stroke peak exhaust-stroke temp | 600–900 °C | [M] | four-stroke SI engine study |
| Shahed-136 powerplant | Mado MD-550, ~50 hp 4-cyl two-stroke | [S] | GlobalSecurity |
| Shahed-136 MWIR detection claim | >3–5 km | [J] | drone-warfare.com analysis |
| Shahed-238 / Geran-3 (turbojet) | "increased thermal signature", tracked by it | [J] | CSIS Missile Threat |
| Lancet thermal signature | "almost undetectable", electric | [J] | Grey Dynamics / Army Recognition |
| Russian Yolka interceptor EO module | 1 m wingspan at 700–1000 m | [J] | Russian-language reporting |
| Wide-angle static camera, drone detection | ~1 km, target a few dozen px | [M] | IPSJ CVA 10.1186/s41074-019-0059-x |
| Baba Yaga onboard thermal, target ID | ~120 m | [J] | Army Recognition |
| Baba Yaga onboard thermal, person / vehicle | 2 km / 10 km | [J] | TheDefenseWatch — **see §9** |
| FPV-carried thermal, effective working range | 100–500 m | [J] | star-navi commentary |
| Multi-sensor C-UAS vs DJI Phantom | 5 km detect, 3 km visual track | [S] | ADDS vendor claim |
| DJI Mavic 3T thermal | 640×512, ≤50 mK, 8–14 µm, 1.4 mrad IFOV | [S] | DJI Enterprise specs |
| Fused multi-modal C-UAS pipeline | 96.1 % detection, 3.2 % false alarm | [M] | DroneShield-AI, arXiv 2606.11687 |
| Uncooled sensor cost fall, past decade | 65–70 % | [S] | microbolometer market analysis |
| Entry-level uncooled module price | below USD 250 | [S] | same |
| Uncooled share of global IR shipments | >85 % | [S] | same |

### Model outputs, for comparison

Computed by §6 with η = 0.5, k = 3, and the emitter catalogue in §5. These are *my
model's* numbers, not measurements — but they land on top of the field anchors above,
which is the argument for trusting the model in conditions nobody has published.

| Case | Cheap uncooled | Good uncooled | Cooled MWIR |
|---|---|---|---|
| FPV quad, night, sky-backed | 0.91 km | 2.38 km | 7.36 km |
| FPV quad, day, sky-backed | 0.45 km | 1.17 km | 3.66 km |
| FPV quad, night, ground-backed | 0.19 km | 0.53 km | 1.91 km |
| **FPV quad, day, ground-backed** | **0.04 km** | **0.11 km** | **0.42 km** |
| Shahed-class, night, sky-backed | 3.96 km | 8.33 km | >20 km |
| Shahed-class, day, sky-backed | 2.13 km | 4.62 km | 10.40 km |
| Tank running, night, ground | 3.53 km | 7.35 km | 18.81 km |
| Tank running, day, ground | 1.43 km | 3.30 km | 8.13 km |
| Tank parked, dusk crossover | 0.53 km | 1.37 km | 4.23 km |

Sensor definitions: cheap = 256×192, 12 µm, f = 9 mm, NETD 60 mK. Good = 640×512,
12 µm, f = 25 mm, NETD 50 mK. Cooled = 1280×1024, 8 µm, f = 67 mm, NETD 25 mK.
Note the FPV-quad-day-ground row against the reported 100–500 m working range of
FPV-carried thermal **[J]** — the model reproduces it from first principles.

---

## 3. Thermal crossover and solar loading

Sunlight heats the scene. Low-thermal-mass surfaces heat fast; a thin metal or
plastic target heats faster still. Twice a day the target and background curves
cross and the image goes flat. The 2017 Beihang study imaged targets outdoors over a
full 24-hour cycle for exactly this reason and found a conventional single-band MWIR
sensor loses targets over the diurnal cycle, which multispectral 3–5 µm imaging
recovers. **[M]**

- **Daytime *scene* contrast is 2–3× higher than night**, because solar loading
  differentiates vegetation from soil. **[M]** More scene contrast is *worse* for
  finding a small target, because the clutter brightens with everything else. This
  is the σ_clutter term in §6.
- **Night is the flat case.** Vegetation and soil are "nearly identical" in thermal
  intensity from 2000 to 0600. **[M]** Featureless cold background is exactly what
  you want.
- **Dawn and dusk are the disaster**, and this is where the game is wrong.

The only paired day/night measurement available is indirect: a thermal+LiDAR stack
scored 55.9 % by day and 57.5 % at night, while visible+LiDAR fell to 49.9 % at
night. **[M]** Road vehicles, not drones, and classifier accuracy, not range — but it
says thermal is slightly *better* at night and visible clearly worse, which is the
right shape.

**How much of the day/night difference is contrast rather than absorption?**
Essentially all of it. Atmospheric transmission does worsen by day — warmer air holds
more water vapour, which is what absorbs in LWIR — but the effect is roughly 10–25 %
more attenuation per kilometre on a hot humid afternoon than a cold clear night
**[T]**. The contrast-and-clutter term changes by a factor of **5 to 60** over the
same interval (σ_clutter 0.05 K over clear sky, 3 K over sunlit mixed terrain). **[G]**
Absorption is a second-order correction; contrast is the whole story.

**One daytime effect that is absorption-adjacent and band-specific:** solar radiation
has meaningful energy at 3–5 µm and essentially none at 8–12 µm. Sunlight glinting
off an airframe is a real MWIR daytime noise source and a non-issue in LWIR. **[T]**
So the cheap uncooled long-wave imager is *relatively* better by day than its NETD
alone suggests. Model it as roughly doubling σ_clutter for MWIR sensors in daylight.

---

## 4. Thermal signatures by drone type — the headline gap

| Class | Hottest component | Temp | Emitting area | ΔT vs 20 °C ground | ΔT vs cold sky |
|---|---|---|---|---|---|
| Small electric quad | LiPo pack under load | ~60 °C **[M/S]** | ~0.01 m² | ~40 K | ~80–100 K |
| Heavy electric multirotor | 6–8 motors + packs | 60–90 °C **[D]** | ~0.1 m² | 40–70 K | 100–130 K |
| Two-stroke loitering munition | exhaust pipe / plume | 325–345 °C **[M]** | ~0.02 m² + plume | ~310 K | ~350 K |
| Four-stroke strike drone | exhaust, block | 360–380 °C, peaks 900 °C **[M]** | ~0.05 m² + plume | ~350 K | ~400 K |
| Turbojet strike drone | nozzle + plume | 450–650 °C **[G]** | large plume | ~500 K | ~550 K |
| Main battle tank | engine deck, exhaust | 70–120 °C deck **[G]** | 2–4 m² | 60–100 K, **~0 at crossover** | n/a |

**Why small electric drones are nearly cold.** Not because 60 °C is cool — it isn't.
Because the radiating area is minuscule and mostly hidden inside a plastic shell. At
1 km with a sensible field of view the source is well under one pixel: you are not
resolving it, you are hoping it lifts one pixel above noise. Hence the FLIR guidance
about a 2×2 cluster against the sky's cold background **[S]** and the reported
100–500 m working range for FPV-carried thermal **[J]**.

**Why combustion drones are a different problem.** A 345 °C exhaust at a tenth the
area radiates roughly two orders of magnitude more excess in-band power than a 60 °C
battery, before the plume. Hence the Shahed-136 credibly detectable beyond 3–5 km by
MWIR **[J]** while the Lancet — same size class, electric — is "almost undetectable."
**[J]** The propulsion choice, not the airframe, decides the thermal problem.

**Why the tank is the worst daytime case in the roster.** A tank is a large flat-sided
object in the same sunlight as the ground. Its deck is hot but its glacis, turret roof
and skirts solar-load to whatever the terrain does. The game gives it thermal 90 and
applies the same 0.55 daytime multiplier as a turbojet, which is backwards.

---

## 5. The full thermal spread, with temperatures and areas

The distinction that decides everything for a low-resolution imager is **small hot
point** versus **large warm area**. A small hot point delivers a lot of radiance from
a tiny solid angle: it survives to long range as a sub-pixel point source but it is
easy to lose in one bad pixel or one frame of jitter. A large warm area delivers
little per unit area but fills many pixels: it is robust, resolvable, recognisable —
and it disappears completely at crossover, because large areas are exactly what solar
loading equalises.

Coldest to hottest. ΔT is against a 15 °C (288 K) ambient. Emissivities are LWIR.

| Source | Temperature | ΔT | Projected area | ε | Class |
|---|---|---|---|---|---|
| Shaded unpowered airframe | 15 °C | 0 K | 0.03–0.8 m² | 0.90 | large, zero contrast |
| Building wall, occupied | 17–23 °C | 2–8 K | 20–200 m² | 0.92 | very large, very low ΔT |
| LiPo pack at rest | 18–25 °C | 3–10 K | 0.003–0.02 m² | 0.90 | small, negligible |
| Human, clothed | 20–28 °C clothing, 32 °C skin | 5–17 K | 0.5–0.7 m² frontal | 0.98 | **large warm area** |
| Sun-loaded plastic airframe, midday | 45–55 °C | 30–40 K | 0.03–0.8 m² | 0.90 | large — *but so is the ground* |
| Electric motor casing, cruise | 40–55 °C | 25–40 K | 0.0008 m² each | 0.85 | small warm point |
| LiPo pack, hard discharge | 50–65 °C | 35–50 K | 0.003–0.02 m² | 0.90 | small warm point |
| Vehicle road wheels / tracks, 30 min run | 40–70 °C | 25–55 K | 0.3–0.8 m² | 0.90 | medium warm area |
| ESC / power board, heavy lift | 60–80 °C | 45–65 K | 0.002 m² | 0.85 | small warm point |
| Electric motor casing, heavy lift at max | 70–90 °C | 55–75 K | 0.002 m² each | 0.85 | small hot point |
| MBT engine deck / grille, running | 70–120 °C | 55–105 K | 1.5–3 m² | 0.90 | **large hot area — the beacon** |
| Recently fired tank barrel | 100–200 °C outer | 85–185 K | 0.15 m² | 0.85 | medium hot |
| Piston exhaust plume (gas) | 150–300 °C | — | 0.1–1 m² | **0.1–0.3, band-selective** | large, dim, MWIR-only |
| MBT exhaust outlet | 300–500 °C | 285–485 K | 0.05–0.15 m² | 0.90 | small very hot point + plume |
| Two-stroke UAV exhaust pipe | 325–345 °C **[M]** | ~330 K | 0.01–0.03 m² | 0.85 | small very hot point |
| Four-stroke UAV exhaust | 360–380 °C **[M]** | ~350 K | 0.02–0.05 m² | 0.85 | small very hot point |
| Turbojet nozzle + tailpipe skin | 450–650 °C **[G]** | 435–635 K | 0.03–0.08 m² | 0.80 | small extremely hot point |
| Turbojet exhaust plume | 400–600 °C core **[G]** | — | 1–5 m³ | **band-selective** | large, MWIR-dominant |
| Muzzle flash, burning vehicle | 800–1500 °C | — | — | — | transient, saturates the sensor |

Three consequences worth building into the model:

**Emissivity is not free.** Painted and oxidised surfaces sit at ε ≈ 0.85–0.95 in
LWIR, but **bare polished metal is ε ≈ 0.05–0.2** **[T]**. A bare aluminium exhaust
stack can read close to ambient in LWIR while being blazing in MWIR, because the
MWIR signal is dominated by the hot gas rather than the pipe. If the game ever wants
a "polished / unpainted" trait, this is where it lives.

**Exhaust plumes are an MWIR target, hot skin is an LWIR target.** Combustion gases
are selective emitters — CO₂ at 4.3 µm, H₂O at 2.7 and 6.3 µm — so a plume radiates
strongly in mid-wave and weakly in long-wave. **[T]** Combined with the blackbody
result (a 600 K source is 293× its background in MWIR and 12.9× in LWIR **[D]**),
this is why every long-range engagement sensor against jets and missiles is cooled
MWIR, and why a cheap LWIR bolometer is the right choice for cold drones.

**Against a cold sky, everything is a target.** This is the single most
counter-intuitive result in §6, and it took the arithmetic to see it. With air at
5 °C and a clear zenith sky at −33 °C, a drone airframe merely at ambient temperature
contributes **1.2 of the 1.6 m²·K** total signature of an FPV quad — more than the
motors and battery combined. The reason small drones are detectable against sky at
all with cheap thermal is not their motors. It is that the sky is 40 K colder than the
air, so anything in front of it is warm by definition.

### Optical spread — presented area and inherent contrast

Note the frontal-versus-planform distinction: an approaching drone presents frontal
area, a crossing one presents planform, often 4–6× more. Aspect matters far more for
fixed-wing than for multirotors, which is a free gameplay lever if you want one.

| Target | Critical dim | Frontal area | Planform area | C₀ vs sky | C₀ vs ground |
|---|---|---|---|---|---|
| 5″ racing / FPV quad | 0.22–0.30 m | 0.010–0.02 m² | 0.05 m² | 0.85 | 0.10–0.20 |
| 7–10″ FPV / fiber quad | 0.35–0.50 m | 0.03–0.06 m² | 0.12 m² | 0.85 | 0.15 |
| Mavic-class recon quad | 0.35 m | 0.02 m² | 0.09 m² | 0.80 | 0.15 |
| Heavy multirotor (Baba Yaga) | 1.3–1.8 m | 0.3–0.7 m² | 1.8 m² | 0.80 | 0.25 |
| Fixed-wing recon, 3 m span | 3.0 m | 0.15 m² | 0.5–1.0 m² | 0.75 | 0.25 |
| Shahed-136 class | 2.5 m span | 0.20 m² | 1.2 m² | 0.70 | 0.30 |
| Turbojet strike drone | 2.2 m span | 0.18 m² | 0.9 m² | 0.70 | 0.30 |
| Main battle tank | 7.0 × 3.6 m | 7 m² | 12–25 m² side | n/a | 0.20–0.45 |
| Jammer vehicle with mast | 7 m + 6 m mast | 10 m² | 20 m² | n/a | 0.35–0.55 |

Contrast values are **[G]**, reasoned from typical albedo differences: a dark matte
airframe against bright sky is close to silhouette (C₀ → 0.85), the same airframe
against mixed vegetation is close to invisible (C₀ → 0.15). They are the most
tunable numbers in this document and the ones I would change first if playtest
disagrees.

---

## 6. Thermal propagation model

This section is the deliverable: enough to write `detect_thermal(target, range,
environment, sensor) -> bool` without looking anything else up.

### 6.1 Band radiometry

Computed from the Planck function, integrated over each band. **[D]**

| Source T | 3–5 µm exitance | 8–12 µm exitance |
|---|---|---|
| 250 K | 0.68 W/m² | 45.7 W/m² |
| 273 K | 2.01 | 74.6 |
| 300 K | 5.86 | 121.0 |
| 330 K | 15.9 | 189.4 |
| 400 K | 93.8 | 420.2 |
| 600 K | 1719 | 1555 |
| 900 K | 13354 | 4075 |

| Quantity at 300 K | 3–5 µm | 8–12 µm |
|---|---|---|
| dM/dT | 0.213 W/m²/K | 1.978 W/m²/K |
| Fractional dM/dT | 3.63 %/K | 1.64 %/K |

| Contrast ratio vs 300 K background | 3–5 µm | 8–12 µm |
|---|---|---|
| 400 K source | 16.0× | 3.5× |
| 600 K source | 293× | 12.9× |
| 900 K source | 2278× | 33.7× |

**Rule for the game:** long-wave for near-ambient targets (11× more absolute signal
per kelvin), mid-wave for hot targets (23× better contrast ratio against ambient).

### 6.2 Target signature as an area–kelvin sum

Rather than a scalar "signature", sum the emitters. Define the target's **thermal
area-kelvin**:

```
AK = Σᵢ  Aᵢ · εᵢ · (Tᵢ − T_bg)          [units: m²·K]
```

over the emitter list in §5, using projected area for the current aspect. AK is a
signed quantity — a sun-loaded airframe against hotter ground goes negative, which
is physically correct (it appears as a cold spot) and detectable either way, so take
|AK| at the decision step.

For a mid-wave sensor, weight each emitter by its band ratio from §6.1 rather than
treating contrast as linear, because hot sources are wildly superlinear in MWIR:

```
AK_mwir = Σᵢ Aᵢ · εᵢ · (M_mwir(Tᵢ) − M_mwir(T_bg)) / (dM_mwir/dT at T_bg)
```

This is the same quantity expressed as "the ΔT of an equal-area ambient object that
would produce this much mid-wave signal." Using it lets one code path serve both
bands.

### 6.3 Background temperature

**Clear sky, as a function of elevation angle θ:**

```
T_sky(θ) = T_air − ΔT_zen · sqrt(sin θ)
```

| Sky condition | ΔT_zen | Type |
|---|---|---|
| Clear, cold and dry | 45–60 K | [T] |
| Clear, temperate | 35–45 K | [T] |
| Clear, warm and humid | 20–30 K | [T] |
| Broken cloud | 10–20 K | [G] |
| Overcast, low cloud | 2–5 K | [T] |

The `sqrt(sin θ)` form is **[G]** — a fitted shape that gives the right endpoints
(maximum depression at zenith, zero at the horizon). It matters a lot: a drone at
10° elevation sits against a background only ~0.4 of the way to full zenith
depression, so low-elevation targets are much harder even when sky-backed.

**Overcast destroys the cold-sky advantage**, and the game has no modifier for it.
A drone under a low overcast is seen against a background at air temperature, which
removes the dominant airframe term from §5 entirely. Expect a 2–3× range loss. **[D]**

**Ground:**

```
T_ground = T_air + S · I_solar/1000 · (1 − 0.75·cloud)      (day)
T_ground = T_air − 4 · (1 − cloud)                          (night)
```

with I_solar = 1000 · max(0, sin(solar elevation)) W/m², and S from:

| Surface | S (K per kW/m²) | Type |
|---|---|---|
| Asphalt, bare dry soil, rubble | 25 | [T] |
| Dry grass, sand | 15 | [T] |
| Green vegetation | 10 | [T] |
| Wet ground, water | 4 | [T] |
| Snow | 2 | [T] |

Night ground runs 3–8 K *below* air temperature under clear skies (radiative
cooling), which the −4 K term approximates. **[T]**

### 6.4 Clutter — the term that does the real work

Replace NETD alone with an effective noise floor:

```
σ_eff = sqrt( NETD² + σ_clutter² )
```

| Background | σ_clutter | Type |
|---|---|---|
| Clear sky | 0.05 K | [G] |
| Hazy or lightly clouded sky | 0.3 K | [G] |
| Broken cloud (cloud edges are strong false alarms) | 0.8 K | [G] |
| Uniform ground, night, 0200–0500 | 0.5 K | [G] |
| Ground at dusk/dawn, differential cooling | 1.5 K | [G] |
| Mixed terrain, sunlit midday | 3.0 K | [G] |
| Urban / rubble, sunlit | 4–6 K | [G] |
| Any of the above, MWIR sensor, daylight (solar glint) | ×2 | [T] |

These σ_clutter values are the least-sourced numbers in the document and the most
important. They are anchored by the measured finding that daytime terrain contrast is
2–3× night **[M]** and by back-fitting the model to the reported field ranges in §2,
which it reproduces. Treat them as calibrated, not measured, and tune them first.

### 6.5 Atmospheric transmission

```
τ(R) = exp(−α · R_km)
```

Extinction coefficient α, per kilometre, by band and condition. All **[T]**.

| Condition | 3–5 µm | 8–12 µm | Visible |
|---|---|---|---|
| Cold dry clear (0 °C, 50 % RH) | 0.08 | 0.11 | 0.13 |
| Temperate clear (15 °C, 50 % RH, 23 km vis) | 0.14 | 0.19 | 0.17 |
| Warm humid (30 °C, 80 % RH) | 0.30 | 0.50 | 0.25 |
| Light haze (V = 10 km), add | +0.08 | +0.05 | +0.39 total |
| Moderate haze (V = 5 km), add | +0.18 | +0.11 | +0.78 total |
| Rain, 4 mm/h, add | +0.45 | +0.45 | +0.50 |
| Rain, 16 mm/h, add | +1.40 | +1.40 | +1.60 |
| Fog, V = 200 m | +3.0 | +2.5 | +19.6 total |
| Carbon / HC battlefield smoke | +2.0 | +1.5 | +20 |
| Multispectral obscurant (metal flake) | +15 | +20 | +30 |

**Which band suffers more in which conditions:**

- **Humidity punishes long-wave harder.** The 8–12 µm window is bounded by the water
  vapour continuum; warm humid air roughly triples LWIR attenuation while MWIR
  roughly doubles. In a humid summer the expensive cooled mid-wave system pulls
  further ahead than its NETD suggests.
- **Haze and smoke punish short wavelengths harder,** because aerosol scattering goes
  roughly as λ^−q. Haze that cuts visible range fivefold barely touches LWIR. This is
  the origin of "thermal sees through smoke" — true for carbon smoke, false for
  modern multispectral obscurants, which were designed specifically to defeat it.
- **Fog and rain kill everything.** Droplets are comparable to or larger than IR
  wavelengths, so scattering is near-geometric and roughly band-independent. There is
  no clever sensor answer to fog.

**Design consequence for the game's weather:** if KILL ZONE's weather is mostly
temperate haze and smoke, cheap uncooled LWIR is the right buy and the cooled MWIR
premium is small. If it is warm and humid, or the targets are hot, cooled MWIR wins
decisively. That is a real procurement trade and it would make a good tech choice.

### 6.6 Two regimes — resolved and unresolved

**This distinction is the thing most game sensor models get wrong.** Johnson's
criteria only apply to a *resolved* target. A small drone at any useful range is
*unresolved* — sub-pixel — and its detection is governed by signal-to-noise, not
resolution.

```
n_px = D_target / (R · IFOV)              where IFOV = pixel_pitch / focal_length
```

- **n_px ≥ 2** → resolved. Johnson/TTP applies. Range limited by resolution.
- **n_px < 2** → unresolved point source. Range limited by SNR. Detection is still
  possible — often at far greater range than the resolution limit — but recognition
  and identification are flatly impossible.

Pixel thresholds, NATO convention, 50 % probability **[M]**:

| Task | Cycles | Pixels across critical dimension |
|---|---|---|
| Detection ("something is there") | 1 | 2 |
| Recognition ("it is a multirotor") | 3 | 6 |
| Identification ("it is a Baba Yaga, carrying") | 6 | 12 |
| Track maintenance, already acquired | — | ~1.3× detection range **[G]** |

Johnson's original 1 / 4 / 6.4 cycles gives 2 / 8 / 12.8 px; use NATO's if you want
recognition to feel achievable. Track maintenance being easier than cold acquisition
is standard practice (you gate the search, integrate frames, and accept a lower SNR
threshold) but the 1.3× multiplier is my estimate. It is the justification for the
game's existing two-second track hold, which is reasonable in kind if arbitrary in
magnitude.

### 6.7 The decision

```
fill  = AK / (R² · IFOV²)                 # apparent ΔT at the pixel, K
ΔT_ap = fill · τ(R)
SNR   = ΔT_ap / σ_eff
detected = SNR ≥ k
```

with **k = 3.0** for single-frame detection at an acceptable false-alarm rate, and
**k = 1.8** where the system does multi-frame temporal integration or moving-target
indication **[G]** — which every serious C-UAS system does, and which is precisely
the capability that separates a 2028 sensor from a 2026 one (§11).

Closed form for maximum range, ignoring atmosphere:

```
R₀ = (η / IFOV) · sqrt( AK / (k · σ_eff) )
```

Then solve `R · exp(α·R/2) = R₀` for R. **Use bisection, not fixed-point iteration** —
the fixed-point form diverges at long range, as I found when validating this. Three
bisection steps on [0, R₀] are plenty.

**η is the system efficiency factor, and it is where honesty lives.** It covers
optics MTF, point-spread energy loss on a sub-pixel source, atmospheric turbulence,
boresight jitter, and false-alarm margin. Clean radiometry overpredicts fielded
performance by roughly 2×, which is consistent with the measured finding that no
tested camera reached its declared range **[M]**. **η = 0.5** **[G]**, calibrated so
the model reproduces the field anchors in §2. It is a fudge factor. It is labelled
as one. If you replace it with something better, everything downstream improves.

### 6.8 Worked example

FPV quad, clear night, sky-backed at 45° elevation. T_air = 5 °C = 278 K,
ΔT_zen = 45 K, so T_bg = 278 − 45·√(sin 45°) = **240 K**.

| Emitter | A (m²) | ε | T (K) | Contribution (m²·K) |
|---|---|---|---|---|
| 4 × motor bells | 3.2e-3 | 0.90 | 333 | 0.268 |
| Battery pack, 40 % visible | 1.2e-3 | 0.90 | 330 | 0.097 |
| Airframe and arms at ambient | 0.030 | 0.90 | 285 | **1.215** |
| **AK** | | | | **1.58** |

Sensor: 640×512, 12 µm, f = 25 mm → IFOV 480 µrad; NETD 0.05 K. Sky clutter 0.05 K →
σ_eff = 0.071 K. k = 3 → k·σ_eff = 0.212 K.

R₀ = (0.5 / 4.8e-4) · √(1.58 / 0.212) = 1042 · 2.73 = **2.84 km** in vacuum.
With α = 0.15/km: solve R·e^{0.075R} = 2.84 → **R = 2.38 km**.

Same drone, same sensor, midday, over sunlit mixed terrain. T_air = 25 °C = 298 K,
S = 25 → T_bg = 323 K. The airframe is sun-loaded to 318 K — **colder than the
ground** — so its term goes negative and cancels most of the motor signal:
AK = 0.029 + 0.008 − 0.135 = **−0.10 m²·K**. σ_clutter = 3.0 K → k·σ_eff = 9.0 K.

R₀ = 1042 · √(0.10/9.0) = **0.11 km**.

**2.38 km versus 0.11 km. A factor of 21.6, from one equation, in conditions the
published literature does not cover.** That is what the model is for.

---

## 7. Optical and visible detection

Two field anchors:

- A static wide-angle camera detects drones "up to approximately 1 km", target "as
  small as a few dozens of pixels." **[M]**
- The Russian Yolka interceptor's EO module detects a 100 cm wingspan drone at
  700–1000 m. **[J]** The most useful single figure found: a cheap fielded system
  against a stated target size.

Working the Yolka number backwards: 1 m at 1000 m subtends 1 mrad; two pixels across
that needs IFOV ≤ 500 µrad, which is a ~1024-pixel sensor over a ~29° field. That is
exactly what a cheap wide-*search* camera looks like. **So the Yolka figure is a
search-field-of-view limit, not a limit of physics** — and a narrow-field tracker on
the same mount reaches an order of magnitude further. **[D]**

**The game should therefore separate search reach from track reach on optical
sensors.** They are different numbers for the same hardware and the gap is roughly
10×. Search is wide-field and short; once cued, a gimballed tracker holds the target
much further out. This is also how real C-UAS works — EO/IR "typically requires
cueing from other sensors to search the correct patch of sky" **[J]** — and it is a
better model of radar-cues-optics than anything the game has now.

Modifiers, in order of importance: background (sky is the best case, ground clutter
the worst), night illumination, haze and cloud, sun angle (looking into the sun kills
a sky-silhouette track outright).

---

## 8. Optical propagation model

### 8.1 Extinction and the visibility relationship

Koschmieder's law, the standard basis of every visibility definition:

```
σ_ext = 3.912 / V                          V = meteorological visibility, km
C(R)  = C₀ · exp(−σ_ext · R)
```

The 3.912 is ln(1/0.02) — visibility is defined as the range at which a large black
object falls to 2 % contrast. **[T]**

| Condition | V (km) | σ_ext (/km) |
|---|---|---|
| Exceptionally clear | 50 | 0.078 |
| Clear | 30 | 0.130 |
| Typical good day | 20 | 0.196 |
| Light haze | 10 | 0.391 |
| Moderate haze | 5 | 0.782 |
| Mist | 2 | 1.956 |
| Light fog | 1 | 3.912 |
| Fog | 0.2 | 19.56 |

### 8.2 Contrast threshold, and the clutter gate

Detection requires `C(R) ≥ C_th`. The threshold is not a constant — it is where
background clutter enters:

| Situation | C_th | Type |
|---|---|---|
| Human observer, large object, sky background | 0.02 | [T] |
| Algorithmic detector, few-pixel target, clear sky | 0.10 | [G] |
| Same, broken cloud | 0.15 | [G] |
| Same, uniform ground (field, water) | 0.15 | [G] |
| Same, mixed vegetation / terrain | 0.20 | [G] |
| Same, urban or rubble | 0.30 | [G] |
| Motion-detection (MTI) assist, static sensor | ×0.4 on the above | [G] |

Contrast-limited range:

```
R_contrast = ln(C₀ / C_th) / σ_ext          (zero if C₀ ≤ C_th)
```

Resolution-limited range, same as §6.6:

```
R_resolution = D_target / (n_px_required · IFOV)
```

And the achievable range is the smaller:

```
R_optical = η · min(R_contrast, R_resolution)
```

### 8.3 Which gate binds, and the result that matters

Computed with the §5 contrast values. **[D]**

**Resolution-limited detection range (2 px), km:**

| Target | Wide search FOV (500 µrad) | Narrow tracker (50 µrad) |
|---|---|---|
| FPV quad, 0.3 m | 0.30 | 3.00 |
| Heavy multirotor, 1.5 m | 1.50 | 15.0 |
| Fixed-wing, 3 m | 3.00 | 30.0 |
| Shahed, 2.5 m | 2.50 | 25.0 |
| Tank, 7 m | 7.00 | 70.0 |

**Contrast-limited range, km:**

| Visibility | Drone vs sky (C₀ 0.85, C_th 0.10) | Drone vs ground (C₀ 0.18, C_th 0.20) | Vehicle on terrain (C₀ 0.35, C_th 0.12) |
|---|---|---|---|
| 30 km | 16.4 | **0 — never detected** | 8.2 |
| 20 km | 10.9 | **0** | 5.5 |
| 10 km | 5.5 | **0** | 2.7 |
| 5 km | 2.7 | **0** | 1.4 |
| 2 km | 1.1 | **0** | 0.55 |

**The headline: against sky, optical detection of a small drone is limited by the
optic, not the air — the contrast gate is never the binding constraint in anything
better than mist. Against ground clutter, a small drone's inherent contrast sits
below the clutter-raised threshold and the contrast-limited range is zero at any
distance.** A small drone over terrain is not "hard to see optically"; it is
*invisible* optically, and the only thing that finds it is motion. **[D]**

That is why the MTI multiplier is in the table, and why every real system runs frame
differencing before it runs a classifier: "Moving Target Indication methods —
including frame differencing and background subtraction — often serving as the first
stage." **[M]** With the MTI assist (C_th × 0.4 → 0.08), a drone over vegetation
becomes detectable to 10 km at V = 30 km — *if it is moving against a static
background*, which fails the moment the sensor itself is moving or the drone is
hovering.

**Game consequence:** a hovering drone over ground clutter should be effectively
invisible to optics. A moving one should be detectable. That is a mechanic, free.

### 8.4 Night

The right model for optical at night is **not a range multiplier**. It is a collapse
of C₀. Ambient illumination **[T]**:

| Condition | Illuminance |
|---|---|
| Overcast day | 1 000–10 000 lux |
| Deep twilight | 1–10 lux |
| Full moon, clear | 0.1–0.25 lux |
| Quarter moon | 0.01 lux |
| Clear starlight, no moon | 0.001–0.002 lux |
| Overcast starlight | 0.0001 lux |

Modern low-light CMOS is usable to roughly 0.001 lux with SNR cost; below that you
need intensification or active illumination. But illuminance is the *second* problem.
The first is that **an unlit dark drone against a dark sky has no contrast at all** —
both are near-black, C₀ collapses from 0.85 to below 0.02, and no exposure time
recovers what is not there.

| Night case | C₀ vs sky | Effect |
|---|---|---|
| Unlit drone, clear moonless sky | < 0.02 | **undetectable at any range** |
| Unlit drone, full moon, clear sky | 0.05–0.10 | marginal, hundreds of metres |
| Unlit drone, moonlit or urban-lit overcast (bright cloud deck behind) | 0.30–0.60 | **recovers substantially** |
| Drone with nav lights / visible motor glow | effectively 1.0 | detectable well beyond daytime range |
| Ground vehicle, lights off, moonlight | 0.05–0.15 | short range only |
| Ground vehicle, firing / burning / lights on | > 1.0 | detectable at extreme range |

All **[G]**, reasoned from the physics of silhouette contrast.

The counter-intuitive and correct result: **a drone under a lit overcast is more
visible at night than one under clear starlight**, because the cloud deck backlights
it. If the game has weather, this is a nice inversion to have in it.

---

## 9. Ukraine reporting, and what to believe in it

- **Baba Yaga / Vampire night bombers exist because of thermal.** The defining
  feature of the class is reliable night flying via combined thermal and optical
  cameras. **[J]** The Vampire's reach grew from ~20 km to ~60 km. **[J]**
- **Winter improves it materially.** Cold weather raises target-to-background
  contrast; soldiers, electronics and heaters are hard to hide and their signatures
  accumulate. **[J]** This is the operational statement of the flat-cold-background
  effect the measurement literature describes, and §6.3 gives the mechanism: cold dry
  air means ΔT_zen of 45–60 K instead of 20–30 K, and night ground running below air
  temperature.
- **The Russians counter it with IR, and Ukraine now counters that.** Russian
  operators have mass-produced interception of heavy bombers **[J]** using thermal
  sighting systems, and some Baba Yagas now carry small infrared projectors
  specifically to dazzle the IR cameras used to intercept them at night. **[J]** An
  IR counter-countermeasure arms race is live as of 2026.
- **The stated limit against electric drones is real.** FPV-side commentary is
  unusually candid: thermal is *not* the preferred night sensor for FPV operations,
  because of low resolution (a 640×512 FLIR Vue Pro is called out as making detail
  identification hard), cost, and a 2.35–3.1 W draw that shortens endurance. **[J]**
  And on the detection side, electric propulsion "produces minimal thermal trail,"
  leaving drones like the Lancet "nearly undetectable until close range." **[J]**

**What not to believe.** The claim that a Baba Yaga's onboard thermal detects a person
at 2 km and ground equipment at 10 km **[J]** cannot be reconciled with the other
figure in circulation — target identification at ~120 m **[J]** — or with a 640×512
uncooled core behind a short lens. Both appear in secondary sources with no
methodology. §6.6 explains the discrepancy exactly: **120 m is an identification
range and 2 km/10 km are detection claims, and somebody collapsed the DRI
distinction.** The NATO 6× ratio reconciles them almost perfectly — 120 m × 6 ≈ 720 m
detection for a person, which is the right order for that hardware. Take the
DRI-corrected middle: a good uncooled drone thermal detects a person at a few hundred
metres and a running vehicle at 1–3 km, and identifies either at a sixth of that.
**[D]**

---

## 10. Verdict on the game's numbers

### Thermal day 0.55 / night 1.25 — **approximately right, for sky-backed targets only**

I expected to find this badly wrong and it is not. Comparing like with like, the
model gives:

| Target, sky-backed | Day | Night | Ratio |
|---|---|---|---|
| FPV quad | 1.17 km | 2.38 km | **2.03×** |
| Shahed-class | 4.62 km | 8.33 km | **1.80×** |
| Tank, ground-backed, running | 3.30 km | 7.35 km | **2.23×** |

The game's 1.25/0.55 = 2.27× is at the top of that band but inside it. Recommend
**day 0.62, night 1.15** (ratio 1.85) purely to centre it. This is a minor tuning
change and not the interesting one.

My initial instinct — that a hot target should barely care about time of day — was
wrong, and I am recording that because the reasoning is instructive. Working from
ΔT of the *exhaust* alone gives a day/night ratio of about 1.07. But against a cold
sky the airframe-at-ambient term dominates the total signature (§5, §6.8), and the
sky's apparent temperature itself swings 15–25 K between a hot humid afternoon and a
cold clear night. The background, not the target, drives most of the diurnal effect.

### Thermal twilight 0.90 — **wrong in sign, should be 0.60, and target-dependent**

Crossover is at dawn and dusk. **[M]** The model puts a *parked* tank at dusk at
1.37 km against 7.35 km running at night — a 5.4× loss. A *powered* target loses far
less, because an internal heat source never crosses.

Recommend **twilight 0.60 globally**, and if the engine supports it, **twilight 0.35
for stationary ground units with cold engines** and **0.75 for anything with a
running powerplant**. The asymmetry is the interesting part: it makes "move before
dusk or be invisible until you start your engine" a real decision.

### The missing modifier — **ground clutter, and it should be ×0.30**

This is the big one. The game has no term for what the target is seen against, and it
is the single largest factor in the entire thermal model:

| Case | Good uncooled sensor | Ratio to sky-backed night |
|---|---|---|
| FPV quad, night, sky | 2.38 km | 1.00 |
| FPV quad, day, sky | 1.17 km | 0.49 |
| FPV quad, night, ground | 0.53 km | 0.22 |
| FPV quad, day, ground | 0.11 km | **0.046** |

Recommend a **thermal ×0.30 modifier for targets seen against ground clutter**,
stacking with the day/night multiplier, which reproduces the 0.046 figure almost
exactly (0.62 × 0.30 = 0.19 — somewhat generous, but the game's square-root signature
law absorbs some of the rest). Apply it to any aerial unit flying nap-of-the-earth
and to all ground units by default.

### "High-altitude" modifiers — **optical sign is wrong**

Current: acoustic ×0.35, radar ×1.20, optical ×0.70. Acoustic and radar are out of
scope here and look sane. The optical −30 % is wrong: a sky-silhouetted target is the
best optical background available and §8.3 shows the contrast gate never binds
against sky. Recommend **optical ×1.00 and thermal ×1.30 at altitude**, and move the
penalty to where it belongs — the ground-clutter modifier above.

### Optical night 0.35 — **too generous, should be 0.20**

Losing 65 % implies a low-light sensor and a target with inherent brightness. For
small matte unlit drones, §8.4 says C₀ collapses below threshold and the honest
answer is close to zero. The existence of an entire Ukrainian night-bombing doctrine
is the evidence. The one measured paired figure — visible detection accuracy falling
from ~56 % to 49.9 % at night **[M]** — is for *illuminated road vehicles* and is the
best case, not the typical one.

Use **0.20**. Consider exceptions the other way for units that emit light (a firing
gun, a burning vehicle, a lit jammer mast) if per-unit overrides exist, and if weather
is modelled, **×1.8 under lit overcast** for the backlighting inversion in §8.4.

### Search versus track reach — **the model is missing a distinction it needs**

§7: the same optical hardware has a wide-field search reach and a narrow-field track
reach differing by roughly 10×. The game's single optical number conflates them.
Cheapest fix that captures it: keep the listed reach as **track** reach, and give
**search** reach at 0.35× of it, requiring a cue from radar or RF to engage beyond
that. This makes the Radar Mast genuinely valuable as a cueing asset rather than just
another detector, which matches how C-UAS actually works. **[J]**

### Cooled/uncooled implied by the sensor table — **too tight**

Interceptor Battery thermal 500 versus Main Tank thermal 300 is 1.67×. The literature's
paired figure is 2.1× **[S]** and the model gives ~3× (2.38 vs 7.36 km on an FPV
quad). Recommend **battery 520, tank 250**.

### Channel reliability — thermal 84 vs optical 88 is reversed for aerial targets

Against sky, thermal is the more reliable discriminator; optical is the one that dies
in haze, glare and darkness. Recommend **thermal 88, optical 82**.

---

## 11. Recommended signature values

| Unit | Thermal now | Thermal rec. | Visual now | Visual rec. | Reasoning |
|---|---|---|---|---|---|
| Small electric quad (FPV) | 8 | **8** | 15 | **6** | Thermal is right — one 60 °C pack, sub-pixel at range. Visual 15 gives the gun mount 232 m against a 0.3 m airframe; §8.3 puts the wide-search limit at 300 m real ≈ 25 map m. 6 is still generous. |
| Fiber-optic quad | 8 | **9** | 15 | **6** | Same airframe, spool drag means marginally more motor load. Not a meaningful difference. |
| Electric heavy multirotor | 22 | **32** | 55 | **30** | 6–8 motors at 60–90 °C plus large packs is ~10× the FPV's radiating area. Russians intercept these at night by IR **[J]**, so it must be a real thermal target. Visual 55 was too high for a 1.5 m airframe relative to a tank. |
| Fixed-wing recon | 25 | **14** or **38** | 30 | **40** | **Ambiguous — pick one.** Electric: 14. Small two-stroke: 38. 25 is the average of two different aircraft. Visual raised: 3 m span is a much bigger optical target than a quad. |
| Combustion loitering munition | 45 | **52** | 25 | **20** | Two-stroke at 325–345 °C **[M]**. Visual lowered: small (1–2 m), often approaching in a dive, so frontal not planform area. |
| Combustion heavy strike drone | 60 | **72** | 45 | **38** | Shahed-136 class: ~50 hp two-stroke, exposed exhaust, credibly detectable beyond 3–5 km by MWIR **[J]**; the model gives 8.3 km against night sky. 60 understates it. |
| Turbojet strike drone | 85 | **92** | 40 | **34** | Geran-3 class. 293× contrast ratio against ambient in MWIR **[D]**; "increased thermal signature", tracked by it **[J]**. Should be the hottest thing in the sky. Visually smaller and faster than the piston version, not bigger. |
| Decoy drone (reflectors) | 25 | **40** | 30 | **22** | **Ambiguous.** Gerbera-type decoys are piston-powered, which makes them thermally *convincing* — that is the point. If the intent is a cheap electric decoy, use 12 and accept that thermal cleanly unmasks it. |
| Main battle tank | 90 | **90** | 90 | **100** | Keep 90 as the night value; the twilight and ground-clutter modifiers handle crossover. Visual 100 anchors the top of the optical scale. |
| Jammer, transmitting | 40 | **48** | 70 | **75** | A high-power transmitter dumps kilowatts into an amplifier and a cooling loop — a genuine hot spot on a vehicle-sized visual target. |

### The electric-versus-combustion gap, quantified

This is the question the brief asked to pay particular attention to, so here is the
arithmetic rather than an assertion. Area-kelvin totals against a 240 K night sky,
from §5 and §6.2:

| Class | AK (m²·K) | Model range, good uncooled | Ratio to FPV |
|---|---|---|---|
| Small electric quad | 1.58 | 2.38 km | 1.0 |
| Heavy electric multirotor | ~6 **[D]** | ~4.4 km | 1.8 |
| Two-stroke loitering munition | ~18 **[D]** | ~6.2 km | 2.6 |
| Shahed-class four-stroke | 47.4 | 8.33 km | 3.5 |
| Turbojet | ~140 **[D]** | ~12 km | 5.0 |

The real electric-to-turbojet range ratio is about **5×**. Under the game's
√(signature/100) law, a 5× range ratio needs a 25× signature ratio. My recommended
8 → 92 gives 11.5× in signature and 3.4× in range — **under-representing the gap by
about 30 %**, which is acceptable and errs in the safe direction (drone survivability
slightly understated rather than the reverse).

Note that the range ratio (5×) is much smaller than the raw signature ratio (90×),
because atmospheric attenuation compresses everything at long range. This is why the
square-root law is a reasonable approximation despite being derived from the wrong
physics: it happens to compress in roughly the right way.

### The structural problem you should know about

The square-root reach law caps the expressible ratio between weakest and strongest at
**10×** (signature 1 → 0.10 reach, 100 → 1.00). The real optical detection-range ratio
between a 0.3 m quad and a 7 m tank is **20–25×** **[D]**. **The game physically
cannot represent how much harder small drones are to see optically than vehicles.**
My recommended visual values (6 vs 100) use 4.1× of the available 10× and are already
a compromise. If small-drone survivability against optics feels wrong in playtest, the
fix is not the signature table — it is an exponent below 0.5 for the optical channel,
or shorter optical baselines on the sensor side. `FINDINGS.md` §2 reached the same
conclusion about the gun mount's 600 m optical reach from gameplay evidence alone,
which is a useful convergence.

The thermal channel has the opposite problem and is fine: it needs 25× and the scale
offers 10×, but atmospheric compression means the *effective* requirement is closer to
11×, which 8 → 92 delivers.

---

## 12. What changes by 2028

**Uncooled sensors get cheap enough to be standard issue, not an asset.** Uncooled
module costs have fallen 65–70 % in a decade, entry-level modules are under USD 250,
and uncooled parts are >85 % of global IR shipments with 12 µm detectors in
million-unit annual production. **[S]** The market grows from USD 557 M in 2026 to
USD 898 M by 2033. **[S]** By 2028, assume *every* front-line vehicle and a meaningful
fraction of expendable drones carry thermal. **Model change: give the Gun Mount a
thermal channel it does not currently have**, rather than raising signatures.

**Classifiers close the gap faster than sensors do.** A fused multi-modal pipeline
already reports 96.1 % detection at a 3.2 % false-alarm rate. **[M]** The binding
constraint on small-drone detection has not been photons for years; it is
discriminating two warm pixels from a bird, a hot rock and sensor noise. In §6.7 this
is precisely the **k** term: better classifiers and multi-frame integration take k
from 3.0 to 1.8, which is a **1.29× range gain on every target in every condition**,
for free, in software. **[D]** **Model change: raise per-channel reliability by ~5
points by 2028** (thermal 88 → 93, optical 82 → 87) and give thermal reach a flat
+25 %.

**Multi-spectral fusion specifically defeats crossover.** The clearest near-future
finding here: the 2017 Beihang work exists to show multispectral MWIR recovers targets
a single-band sensor loses over the diurnal cycle **[M]**, and SWIR is now integrated
"as a complementary imaging layer" in C-UAS stacks alongside radar, visible and
thermal. **[S]** SWIR also sits between the bands on the extinction table in §6.5 —
better than visible in haze, better than LWIR in humidity — which is why it is being
added. **Model change: a 2028-era sensor's twilight multiplier goes from 0.60 to
~0.95** while a 2026-era sensor keeps the penalty. If the game has tech tiers,
"crossover immunity" is a clean and historically honest upgrade to sell.

**Counter-measures arrive at the same time.** IR-suppressed designs are the obvious
response — exhaust mixing, plume shielding, low-emissivity coatings. Note from §5 that
emissivity is a *free* 4–10× signature reduction on hot metal if you are willing to
polish it, and that shifting plume energy out of the 4.3 µm CO₂ band by cooling and
diluting the exhaust attacks the MWIR sensor specifically. The direction is already
visible in the field: Baba Yagas carrying IR projectors to dazzle Russian interceptor
optics. **[J]** **Model change: in a 2028 roster reduce combustion heavy strike drone
thermal 72 → ~58 and turbojet 92 → ~80**, and consider an active IR-dazzle ability
that suppresses enemy thermal reach for a few seconds. That last one is a good
mechanic and it is real.

**What does not change: small electric drones stay cold.** There is no technology path
that turns a 60 °C battery in a plastic shell into a bright IR target. Worse for the
defender, §5's finding that the dominant term against cold sky is the *airframe at
ambient* points at the one countermeasure that would work — actively cooling or
radiatively decoupling the airframe skin — and that is hard, heavy and unlikely on a
disposable drone. The electric-versus-combustion gap is the most durable finding in
this document and should stay the spine of the thermal model through any tech tier
the game adds.

---

## 13. Genuinely uncertain or contested

- **No published same-system day/night range pair exists in open sources.** Every
  day/night figure in §10 comes from the §6 model, not from a measurement. Confidence:
  high on direction and on internal consistency, moderate on absolute magnitude.
- **σ_clutter (§6.4) and C_th (§8.2) are the least-sourced and most load-bearing
  numbers in the document.** They are calibrated to reproduce the field anchors in §2,
  not measured. Tune these first; everything else is more solid than they are.
- **η = 0.5 (§6.7) is a fudge factor** covering optics MTF, turbulence and false-alarm
  margin. Labelled as such. It is the difference between clean radiometry and fielded
  performance, and it is consistent with the measured finding that no tested camera
  met its declared range **[M]**, but it is not derived.
- **The atmospheric table (§6.5) is textbook-recalled, not fetched.** Directionally
  reliable — the humidity/LWIR and haze/visible relationships are robust physics — but
  the specific coefficients should be checked against a MODTRAN table before shipping.
- **Turbojet exhaust temperature (450–650 °C) is my estimate.** I found qualitative
  statements that the Geran-3's signature is larger, never a number.
- **The Yolka 700–1000 m figure** reached me through search summaries of
  Russian-language reporting. It is the load-bearing anchor for the whole optical
  section and nobody has verified the primary source from this environment.
- **Baba Yaga onboard thermal ranges are contested by a factor of ~16** between
  sources (§9). My DRI-based reconciliation is reasoning, not evidence, though it
  reconciles almost exactly.
- **Two-stroke and four-stroke exhaust temperatures** come from general engine studies,
  not UAV powerplants. Small air-cooled two-strokes at high specific output likely run
  hotter than the cited 325–345 °C.
- **The `sqrt(sin θ)` sky-temperature form (§6.3)** is a fitted shape with correct
  endpoints, not a radiative-transfer result.
- **Whether low-light CMOS counts as "optical" or as its own channel** is a design
  question the research cannot answer. I assumed it is folded into optical, which is
  why §10 recommends 0.20 rather than something closer to zero.

---

## 14. Sources

The band radiometry in §6.1 and every model output in §2, §6.8, §8.3 and §10 was
computed during this work directly from the Planck function and the equations in §6
and §8. Nothing in those tables is quoted from a source; they are reproducible from
what is written here, which is the point of stating the equations rather than the
conclusions.

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
those sites come from search-engine summaries, not the pages. The three I would check
first are the *Sensors* 2019 statement that no tested camera met its declared range,
the Yolka 700–1000 m optical figure, and the atmospheric coefficients in §6.5, because
§8, §10 and the whole propagation model lean on them.
