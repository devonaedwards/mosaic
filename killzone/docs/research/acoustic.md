# Acoustic detection

Research for the KILL ZONE sensor and signature model. This is a video game
document: everything below exists to justify a number in a balance table.

Scope note: this ran past the brief's 2,500–4,000 words because the
coordinator added two requirements mid-task — a full source-level spread with
named endpoints (§3a) and a computable propagation model rather than a table of
range guesses (§4, §5, §5a). Sections 1, 2, 3, 6–11 are the original brief.

Confidence markers: **[High]** = multiple independent published sources agree, or
it is textbook physics. **[Medium]** = one good source, or vendor specification
not independently verified, or a reasonable extrapolation. **[Low]** = single
claim, promotional source, or my own inference.

---

## 1. Summary of findings

1. **Acoustic detection of small electric quadcopters is a short-range affair —
   tens to a few hundred metres, not kilometres.** Single microphones and small
   arrays measure out at 35–200 m. A serious 128-microphone array gets 300–500 m
   typical, 1 km claimed at best. Industry consensus for the channel as a whole
   is a 300–500 m ceiling against small drones. **[High]**

2. **Acoustic detection of combustion-engine drones is a completely different
   problem, and works at kilometres.** Ukraine's Zvook reports 3–5 km on
   Shahed-type drones and 5–7 km on cruise missiles from a sensor on a 10–12 m
   mast. That is a factor of ten or more over the same sensor against a quad.
   **[Medium — vendor and press claims, not independently measured]**

3. **The reason is spectrum, not just loudness.** A quad's detectable energy
   sits in the kilohertz band, where air absorbs 10–120 dB per kilometre. A
   two-stroke or piston aero engine puts its energy at 80–250 Hz, where
   absorption is well under 1 dB/km. Low frequency travels; high frequency does
   not. Any model that collapses "acoustic signature" to a single loudness
   number will get the ordering between airframes wrong unless the number is
   explicitly defined as *detection range*, not *volume*. **[High]**

4. **The game's acoustic channel is roughly 4x too generous against small
   quadcopters and roughly 2–3x too mean against Shahed-class drones.** Details
   and replacement numbers in section 7. **[High, given the sources]**

5. **Acoustic gives a bearing, not a fire solution.** Published angular accuracy
   for UAV sources is 3–5° mean error and 7–11° RMSE; one study reports ~10° in
   azimuth and elevation at 150 m. On top of that, sound arrives late — 2.9
   seconds from 1 km — so an acoustic track points at where the target *was*.
   It cues a camera or a sector; it does not lay a gun. **[High]**

6. **Wind is the dominant environmental killer.** Performance degrades severely
   above about 5 m/s. A good windshield alone extends range 31–131% over a bare
   microphone in strong wind and drops the low-frequency noise floor 2–3 dB.
   Upwind targets fall into an acoustic shadow; downwind targets are heard much
   further. **[High on the direction, Medium on the magnitudes]**

7. **Urban and battlefield noise cost both range and trust.** Detection accuracy
   falls from ~99.9% in clean conditions to ~78% at 6 dB SNR, with classification
   accuracy collapsing much harder. Reported false-positive rates run 5–8% in
   mixed environments and 8–15% in realistic urban ones. **[Medium]**

8. **The game's 52% edge reliability for acoustic is the least-wrong number in
   the model.** It is defensible as-is. **[Medium]**

9. **Altitude is where acoustic divides cleanly.** A small quad at 2 km is
   inaudible to anything. A Shahed at 2–3 km slant range is still comfortably
   inside Zvook's claimed envelope. The game's flat "35% at altitude" rule is
   wrong in both directions at once. **[High]**

10. **Ukraine's distributed networks are real and large: ~9,500 to 14,000 Sky
    Fortress sensors at $400–500 each, feeding triangulated tracks to mobile
    fire groups.** Effectiveness claims (80 of 84 UAVs in one raid; "95%
    interception") come from advocates and should be treated as marketing.
    **[High on existence and scale, Low on the performance claims]**

11. **Acoustic reach can be computed rather than guessed.** Section 4 gives a
    band-by-band equation — spherical spreading, ISO 9613 absorption, ground
    effect, barrier diffraction, refraction, array gain, ambient floor — with
    every coefficient either sourced or explicitly labelled as my estimate, plus
    a reference implementation in §5a. The game should use it to *generate* the
    signature table rather than hand-tune one. **[High on the structure, mixed on
    individual coefficients]**

12. **Nobody publishes source levels for military airframes.** Consumer
    multirotors are measured (78–83 dBA at 1 m); Shahed, Orlan, Baba Yaga and
    small turbojets are not. Every military figure in §3a is my estimate and is
    marked as one. **[High that the gap exists]**

13. **The night/day swing is the largest environmental effect in the whole
    channel and the game models none of it.** Ambient falls 10–15 dB from midday
    to a calm night, and a dawn inversion bends rays downward on top of that.
    Acoustic should get a ×2–×3 reach multiplier at night and dawn against
    low-flying targets. **[Medium]**

14. **A diving FPV is only ~30% easier to hear than a cruising one.** Absorption
    compresses level differences within an airframe class: +6 dB buys +33% range
    at 4 kHz against +100% in free field. The game does not need a throttle
    state for acoustics. **[High — arithmetic]**

---

## 2. Measured and claimed detection ranges

| Sensor | Target | Range | Kind | Source |
|---|---|---|---|---|
| Single practical acoustic sensor | small drone | 35–45 m | measured | Drone Warfare C-UAS 101 |
| Single MEMS array, standard features, real time | drone | ~60 m | measured | Acta Acustica 2026 |
| Acoustic delivery-detection rig | drone | up to 100 m | measured | arXiv 2602.09991 |
| Deep-learning array + beamforming | drone | up to 135 m | measured | Multimedia Tools & Apps, 2023 |
| Fraunhofer IDMT sensor, 360° | drone | 50–200 m, noise-dependent, 1 s update | institute spec | Fraunhofer IDMT |
| Zvook NW0 tactical sensor, 360° | FPV | 150–450 m declared | vendor | Unmanned Airspace / Militarnyi |
| Phased-array acoustic, field test | FPV | 200–300 m "targeting range" | claimed | Drone Warfare C-UAS 101 |
| Industry consensus ceiling | small quad | 300–500 m | commentary | Robin Radar |
| Squarehead Discovair G2, **128 mics** + camera | drone | 300–500 m typical, up to 1 km | vendor | Squarehead / cuashub datasheet |
| Research literature, various arrays | aircraft & UAVs | in excess of 2 km | measured | JASA 151(2) 2022 |
| Zvook node on 10–12 m mast | Shahed-type | 3 km (one report), 5 km (another) | vendor/press | United24, Unmanned Airspace |
| Zvook node on 10–12 m mast | cruise missile | 5 km (one report), 7 km (another) | vendor/press | as above |
| Sky Fortress network, ~9,500–14,000 nodes | Shahed, cruise missiles | per-node range not published; network-level cueing | programme reporting | army.mil |

Two things to read off this table. First, the **array-size effect is real and
large**: 35–60 m for one microphone, 50–200 m for a small array, 300–500 m for
128 elements. That is close to what theory predicts — coherent array gain goes as
10·log₁₀(N), so 128 microphones buy about 21 dB over one, and 21 dB is roughly a
6–8x range gain before atmospheric absorption claws some of it back. **[High]**

Second, the **target class matters more than the sensor**. The same class of
hardware that struggles to reach 300 m against a quad is credited with 3–5 km
against a Shahed. No amount of array engineering closes a 10x gap; the gap is in
the source.

---

## 3. Why rotors are distinctive, and which airframes are loud

The dominant sound of a propeller is tonal: the **blade passage frequency**
(BPF = rotational speed in revolutions per second × number of blades) and its
harmonics, sitting on a broadband bed generated by tip vortices, trailing-edge
turbulence and blade–vortex interaction. **[High]** Measured example: a
quadcopter's primary BPF was ~234 Hz at 4,600 rpm and ~252 Hz at 5,050 rpm.
**[Medium — one study]**

That harmonic comb is what makes drones classifiable. Birds, wind and traffic do
not produce a stack of evenly spaced tones that drift smoothly with throttle.
Published work on the problem explicitly contrasts "propeller harmonics" with
"wing beats". **[High]**

But — and this is the point the game model misses — the *fundamental* BPF of a
small quad is only a couple of hundred hertz and carries relatively little
energy. The energy that actually makes a quad audible sits in the harmonics and
broadband above 1 kHz, exactly where the air eats it. The standard ISO 9613
octave-band atmospheric absorption coefficients (15 °C, 70% RH) run roughly:
0.1 dB/km at 63 Hz, 1.0 at 250 Hz, 3.7 at 1 kHz, 9.7 at 2 kHz, 33 at 4 kHz,
117 at 8 kHz. **[Medium on the exact digits — I could not re-open the standard,
the proxy blocks it; High on the shape of the curve, which is textbook]**

So, by class:

- **Small electric quadcopter (FPV, 5–10 inch).** Roughly 80–83 dBA at 1 m for
  consumer-class airframes; military FPV is in the same band. Energy mostly
  above 1 kHz. Quietest meaningful target. **[High]**
- **Fiber-optic quadcopter.** Acoustically identical to the radio one. The spool
  adds mass and drag, so if anything marginally louder. This is the correct and
  important asymmetry: fiber buys silence on RF and buys *nothing* acoustically.
  **[High]**
- **Large electric multirotor (Baba Yaga / Vampire class).** Six large, slow,
  high-thrust rotors: more sources, lower BPF, far more radiated power. Ukrainian
  and Russian reporting describes these as having a high acoustic signature and
  relying on darkness rather than quietness for survival. Note the operational
  counter already in use: releasing munitions from higher altitude specifically
  to make acoustic detection harder. **[Medium]**
- **Fixed-wing electric recon.** One propeller, often throttled back or gliding
  on approach. Quieter than a multirotor of equal mass, and the game's low
  acoustic score here is directionally right. **[Medium]**
- **Combustion loitering munition / small piston recon (Orlan class).** A
  two-stroke engine is an order of magnitude louder than an electric motor and
  radiates at 80–250 Hz. Routinely described as sounding like a lawnmower, and
  audible from the ground while flying at 1–2 km. **[Medium]**
- **Combustion heavy strike drone (Shahed-136 / Geran-2).** The reference loud
  target. Described everywhere as a moped or lawnmower; its MD550-family engine
  is characterised in open reporting as excessively noisy. Newer variants have
  moved to a two-cylinder engine chosen partly for a lower acoustic signature
  than the earlier four-cylinder — i.e. the adversary treats acoustic detection
  as a real threat worth engineering against. **[Medium]**
- **Turbojet strike drone (Shahed-238 / Geran-3).** Very high source level, but
  a *different* signature — reporting describes a distinctive high-pitched
  whistle rather than a propeller hum, and notes it complicates acoustic
  detection systems including Sky Fortress. Note the contradiction in the
  sources: some reporting claims the jet is quieter, which I do not believe for
  a 600 km/h turbojet and would treat as repeated vendor spin. The real reason
  it is hard is that it flies at 5–9 km and moves fast, not that it is quiet.
  **[Low on any quantitative figure; Medium on "loud but geometrically hard"]**

### 3a. Source levels, named endpoints, and the throttle question

What is actually published: **consumer multirotor A-weighted SPL at the 1 m
bystander position**. DJI Air 3S 81 dB, Mavic 4 Pro 83 dB, Phantom 4 ~82 dB,
Phantom 4 Pro 81 dB; the Mavic/Air/Mini family generally sits in a 70–90 dBA
band at a few metres. Useful anchors from the same scale: petrol lawnmower
~90 dB, vacuum cleaner ~75, conversation ~60. **[Medium — hobbyist and vendor
measurement, not standardised flyover testing]**

What is **not** published, anywhere I could find: source levels at a stated
reference distance for military airframes — Shahed/Geran, Orlan, Lancet, Baba
Yaga/Vampire, or any small turbojet drone. Nobody puts a sound level meter next
to a Shahed and writes it up. Everything in the table below for those rows is my
estimate, built from the consumer anchors plus rotor and jet noise scaling, and
labelled as such. Do not cite it as a measurement.

| Airframe | Overall SPL @ 1 m | Dominant band | Basis |
|---|---|---|---|
| Micro quad (3", loiter) | ~68 dBA | 3–8 kHz | **estimate** |
| **Quietest worth modelling:** small electric quad, 7–10", cruise | **78–83 dBA** | 1.5–6 kHz, BPF ~200–250 Hz | measured analogues (DJI) |
| Same quad, full throttle / terminal dive | **88–93 dBA** | shifts up ~30%, 2–8 kHz | **estimate**, see below |
| Fixed-wing electric recon, cruise | 75–80 dBA | 0.5–4 kHz | **estimate** |
| Large electric multirotor (hexa, 15 kg+) | 93–98 dBA | 0.3–3 kHz, BPF ~80–120 Hz | **estimate** |
| Two-stroke fixed-wing (Orlan class) | 95–100 dBA | 100–400 Hz + firing harmonics | **estimate**, anchored on lawnmower |
| Piston heavy strike (Shahed-136 / Geran-2, ~50 hp) | 105–110 dBA | 80–250 Hz | **estimate**, anchored on ultralight aero engines |
| Main battle tank | ~108–112 dBA | 30–200 Hz | **estimate** |
| **Loudest worth modelling:** small turbojet (TJ100 class) | **125–130 dBA** | 200 Hz–4 kHz, broadband roar + shaft tones | **estimate**; jet mixing noise scales as the eighth power of exhaust velocity, which is why a 1 kN turbojet dwarfs everything propeller-driven |

**The A-weighting trap.** Every published figure above is A-weighted, and
A-weighting deliberately discounts low frequency — about −16 dB at 125 Hz and
−26 dB at 63 Hz. That is the right weighting for annoyance and exactly the wrong
weighting for detection at range, because low frequency is the part that
survives kilometres of air. A-weighted numbers therefore **understate**
combustion drones relative to quads by 10–20 dB for our purposes. For the game's
model, use unweighted band levels: 100–500 Hz for combustion and heavy
multirotors, 1–5 kHz for small electric. **[High — this is what A-weighting is]**

**Loiter versus acceleration.** Three different answers by airframe:

- **Multirotor.** A hovering multirotor is already near its power limit; it
  cannot get much quieter without descending. Loiter-to-aggressive-manoeuvre
  spans maybe **3–5 dB**. Rotor noise scales roughly as 50–60·log₁₀(RPM), so a
  30% RPM increase is 6–7 dB — that is the dive case, not the loiter case.
  **[Medium — the RPM scaling law is standard aeroacoustics; the range is my
  estimate]**
- **Electric fixed-wing.** Cruise sits well below full power and many designs
  throttle back or glide on the run-in. **8–12 dB** spread, and the quiet end is
  genuinely quiet. **[Estimate]**
- **Combustion fixed-wing.** Cruise throttle is typically 50–70% of maximum;
  spread **6–10 dB**, and even the quiet end is louder than any electric drone's
  loud end. **[Estimate]**

**Does it matter at detection range? Less than you would think, and this is
important.** In free field, +6 dB doubles range. With atmospheric absorption it
does not. Worked example at 4 kHz, where a quad lives: detection at 300 m needs
the source to overcome 49.5 dB of spreading plus 9.9 dB of absorption. Give the
same airframe +6 dB and solve again — the new range is about **400 m, a 33%
gain, not 100%**. At 125 Hz, where a Shahed lives, absorption is negligible and
+6 dB really does buy close to double the range. **[High — arithmetic on the
absorption table in §4]**

Two design consequences. First, **a diving FPV is only moderately easier to hear
than a cruising one** — roughly a 30% range gain, not a transformation, so the
game does not need a throttle state for acoustics. Second, and more useful:
**absorption compresses level differences within a class but not between
classes**. The quad-versus-Shahed gap is a *frequency* gap, not a loudness gap,
and no amount of throttle closes it.

---

## 4. Propagation: the equation and its coefficients

This section is written so that the game can compute audibility from conditions
rather than look up a table. Everything is per frequency band; evaluate it in
octave bands and take the best-scoring band, and the whole quad-versus-Shahed
story falls out of the arithmetic instead of being hand-tuned in.

### 4.1 The equation

For a source radiating `L_src(f)` decibels (unweighted band SPL at 1 m) and a
receiver at slant range `r` metres:

```
L_rec(f) = L_src(f)
         - 20·log10(r)              # spherical spreading
         - α(f, T, RH) · r / 1000   # atmospheric absorption, α in dB/km
         - A_ground(f, θ, surface)  # ground effect
         - A_barrier(f, terrain)    # terrain/building diffraction
         + A_refract(gradient, wind_bearing)   # signed: −20 … +6 dB

SNR(f)  = L_rec(f) − L_ambient(f) + 10·log10(N_mics)

audible = max over f of SNR(f) ≥ D
```

with

- `r = sqrt(d_ground² + h²)` — slant range; `θ = atan(h / d_ground)` is the
  elevation angle.
- `N_mics` = number of coherently combined microphones. Array gain against
  *uncorrelated* noise (wind pseudo-noise, sensor self-noise, diffuse ambient)
  is `10·log10(N)`: 8 mics = +9 dB, 32 = +15 dB, 128 = +21 dB. This is the term
  that explains the entire range table in §2. Against a *correlated* interferer
  (one nearby generator) the gain is much smaller unless the array can null it.
  **[High]**
- `D` = required SNR. Recommended values, and see §4.8 for why they matter:
  **D = 10 dB for solid continuous detection, D = 0 dB for intermittent, below
  0 dB nothing.** A harmonic-comb tracker integrating over several seconds can
  work a few dB below 0; a plain energy detector needs 6–10 dB. The one measured
  anchor available: **at 6 dB SNR detection accuracy was 77.8% and
  classification accuracy 25.3%**. **[Medium]**

### 4.2 Spreading

**20·log10(r), spherical, i.e. 6 dB per doubling of distance.** Valid from about
one rotor diameter out (nearer than that you are in the aeroacoustic near field
and the point-source assumption fails) all the way out, for an **airborne source
and a ground receiver**. There is no cylindrical-spreading regime for a free
airborne source; 10·log10(r) only applies inside a duct (see §4.5) and in the
very special case of strong inversion ducting near the ground. **[High]**

Do not add a separate "ground wave" term. Most of the excess attenuation people
attribute to spreading in real measurements is actually ground effect and
refraction, which are listed separately below, and double-counting is the
commonest way to build a model that is 20 dB too pessimistic.

### 4.3 Atmospheric absorption

Standard ISO 9613 octave-band coefficients at **15 °C, 70% RH, 101.325 kPa**,
converted to the per-100 m units the coordinator asked for:

| Band | dB/100 m | dB/km | Loss over 3 km |
|---|---|---|---|
| 63 Hz | 0.01 | 0.1 | 0.3 dB |
| 125 Hz | 0.04 | 0.4 | 1.2 dB |
| 250 Hz | 0.10 | 1.0 | 3 dB |
| 500 Hz | 0.19 | 1.9 | 5.7 dB |
| 1 kHz | 0.37 | 3.7 | 11 dB |
| 2 kHz | 0.97 | 9.7 | 29 dB |
| 4 kHz | 3.28 | 32.8 | 98 dB |
| 8 kHz | 11.7 | 117 | 351 dB |

**[Medium on the exact digits — the proxy in this environment blocks ISO,
Acta Acustica and MDPI, so I could not re-open the table; High on the shape,
which is textbook and which one fetched source corroborates at "about 5 dB/km at
1 kHz and 160 dB/km at 10 kHz"]**

**Which spectrum survives.** Absorption is the reason the whole subject divides
into two problems. A usable rule: **beyond range `r`, only frequencies where
α(f)·r < ~10 dB contribute.** That gives a ceiling frequency of roughly

| Range | Highest useful frequency |
|---|---|
| 300 m | ~6 kHz |
| 1 km | ~2 kHz |
| 3 km | ~1 kHz |
| 5 km | ~700 Hz |
| 10 km | ~400 Hz |

A small quad puts most of its energy above 1.5 kHz; its only sub-500 Hz content
is the BPF fundamental around 200–250 Hz and its first harmonic, which sit
10–20 dB below the overall level. So past a few hundred metres the quad has
almost nothing left to detect. A piston aero engine puts most of its energy at
80–250 Hz, which is essentially free to propagate. **That single fact is the
whole quad-versus-Shahed range gap, and it is worth more than any coefficient in
this document.** **[High]**

**Temperature and humidity.** Absorption at mid and high frequency is governed by
the vibrational relaxation of oxygen and nitrogen, both catalysed by water
vapour, so humidity is the dominant variable above ~1 kHz — and it is
non-monotonic, peaking at low-to-moderate humidity. ISO 9613-1 gives a closed
form (classical viscothermal term + O₂ relaxation + N₂ relaxation); implement
that if you want exactness. If you want a multiplier on the 2–4 kHz rows of the
table, my estimates:

| Condition | Multiplier on α at 2–4 kHz |
|---|---|
| 15 °C, 70% RH (baseline) | ×1.0 |
| 25 °C, 90% RH (warm, humid) | ×0.8 |
| 20 °C, 20% RH (hot, dry) | ×1.5–2.0 |
| 0 °C, 80% RH (cold, damp) | ×1.3–1.6 |
| −10 °C, 60% RH (cold winter) | ×1.5–2.5 |

**[Low–Medium — these are my estimates from the shape of the ISO curves, not
read off the standard. Below 500 Hz the variation is small enough to ignore, and
below 500 Hz is where the long-range detections happen, so getting these wrong
mostly does not matter.]** The practical takeaway: **cold dry air is the worst
case for hearing a small quad, and barely matters for hearing a Shahed.**

### 4.4 Ground effect

Interference between the direct ray and the ground-reflected ray. Two regimes:

- **Steep elevation angle (θ > ~15°), which is the normal case for an overhead
  drone.** The reflected ray arrives close in phase and you get up to **+3 dB**
  (pressure doubling over hard ground, a mic on a concrete pad or on the ground
  plane) down to about **0 dB** over grass. Positive, not negative. **[Medium]**
- **Grazing (θ < ~5°), which is the case for a low FPV at 1 km or a
  ground-to-ground path.** Destructive interference over soft ground produces
  the classic "ground dip", **−5 to −15 dB** centred somewhere in 200–600 Hz for
  grass and ploughed earth, less over asphalt or water. **[Medium — ISO 9613-2
  models this as an A_gr term; the numbers here are typical values, not from a
  single cited measurement]**

Game-relevant consequence: **raise the microphone.** Zvook's nodes sit on
10–12 m masts, and that is not an accident — it lifts the grazing geometry away
from the ground dip and away from ground-level noise. Worth a few decibels, i.e.
tens of percent of range.

### 4.5 Terrain shadowing and refraction

**Barriers.** A ridge, treeline or building between source and receiver
diffracts rather than blocks. ISO 9613-2 uses a Fresnel-number form; the
practical envelope is an insertion loss of roughly **5 dB for a marginal
obstruction rising to 20–25 dB for a deep one**, and it is **strongly frequency
dependent — high frequencies are shadowed far harder than low ones.** **[Medium]**

This produces a genuinely useful game asymmetry: **a ridge line substantially
hides a quadcopter and barely hides a Shahed.** It is also why Fraunhofer can
advertise detecting drones "around the corner" and outside line of sight — the
one thing acoustic does that optics categorically cannot. **[High on the
principle]**

**Refraction.** Sound speed rises with temperature and with downwind component,
so the vertical gradient of (temperature + wind) bends rays:

- **Daytime lapse** (ground warmer than air above, the normal sunny afternoon):
  rays bend **upward**, creating a ground-level shadow zone. For a ground-level
  source this can cost 10–20 dB beyond a few hundred metres. For an **airborne**
  source the geometry is much kinder — rays reaching the ground are already
  descending — so the penalty is more like **3–8 dB** at long horizontal range
  and near zero for a target overhead. **[Medium]**
- **Dawn / clear calm night inversion** (air above warmer than ground): rays
  bend **downward** and can be ducted, approaching cylindrical spreading within
  the duct. **Yes, a dawn inversion genuinely extends detection, and materially.**

  How much: the inversion itself is worth perhaps **+5 to +10 dB** at
  low frequency over long near-ground paths, and the ambient floor at dawn is
  **10–15 dB lower** than the same site at midday (§4.7). Combined, call it
  **+15 to +25 dB of SNR**. At low frequency, where absorption is negligible,
  +15 dB is a 5.6x range gain in free field. I would **not** model that much — in
  practice other terms clip it — but a **×2 to ×3 acoustic reach multiplier at
  dawn and on calm clear nights** is well supported by the mechanism and is
  consistent with the universal operational observation that Shaheds are heard
  much further at night. **[Medium on the mechanism and direction, Low on the
  exact multiplier — this is my estimate]**

  Important limit: **refraction only matters for shallow ray paths.** A target
  at 3 km altitude is on a steep path, barely bent at all. So the dawn bonus
  applies to low-flying drones and not to the high ones. This is the opposite of
  a convenient simplification and should be modelled as such if the game already
  distinguishes altitude bands.

### 4.6 Wind

Two separate mechanisms, routinely conflated.

**(a) Refractive asymmetry.** A downwind receiver gets downward-bending rays; an
upwind receiver sits in a shadow zone. This is why the measured windshield
benefit in the outdoor range tests was **azimuth-dependent (31–131%)** — the
azimuth dependence is the wind direction. Practical figures, relative to calm:

| Geometry | Range multiplier |
|---|---|
| Receiver downwind of target | **×1.3 – ×1.5** |
| Crosswind | ×0.9 – ×1.0 |
| Receiver upwind of target | **×0.4 – ×0.6** |

i.e. a **2.5–3x downwind/upwind asymmetry**, growing with wind speed and
strongest for grazing paths. **[Low–Medium — the effect and its direction are
textbook and corroborated by the azimuth dependence in the measurements; the
multipliers are my estimates]**

**(b) Microphone self-noise.** Turbulence on the diaphragm produces
low-frequency pseudo-noise that rises steeply with wind speed — roughly as the
cube of wind speed or worse in the band below 200 Hz, which is exactly the band
the long-range detections live in. Measured mitigation: a **WS6 windshield gave
2–3 dB of noise-floor reduction, 1.8–4.4 dB of wideband SNR, and 31–131% more
range in strong wind.** Wind noise is **uncorrelated between spaced microphones**,
so an array gets its full 10·log10(N) against it — the reason a 128-element
array survives weather that kills a single microphone. **[High]**

**The speed at which it stops working.** The recurring figure in the literature
is **severe degradation above ~5 m/s**. Layering the mitigations:

| Wind speed | State |
|---|---|
| < 3 m/s | No meaningful penalty |
| 3–5 m/s | Bare mic degrading; windshielded array fine |
| 5–8 m/s | Bare mic effectively dead; good windshielded array losing 30–50% of range |
| 8–12 m/s | Only large windshielded arrays still working, and only on loud low-frequency targets |
| **> 12 m/s** | **Acoustic detection of small drones is over regardless of array quality** — wind noise on the mics plus vegetation noise plus refraction, and no amount of `10·log10(N)` recovers it |

**[Medium — the 5 m/s figure is from the literature; the rest of the ladder is
my construction from it]**

### 4.7 Ambient noise floors

Typical broadband A-weighted levels. For the model, remember §3a: what matters
is the level *in the band you are detecting in*, and low-frequency ambient is
higher than the A-weighted figure suggests — add roughly 10–15 dB to these
numbers for an unweighted 100–500 Hz band level.

| Environment | Typical L_ambient (dBA) |
|---|---|
| Rural, calm, night | **25–30** |
| Rural, day, light wind | 35–45 |
| Open country, 5 m/s wind (vegetation noise) | 45–55 |
| Suburban, day | 45–55 |
| Urban street | 65–75 |
| 10 m from a busy road | 70–80 |
| Generator or vehicle at 10 m | 70–80 |
| **On a moving vehicle** | **70–85** |
| Artillery position between rounds | 60–70 |
| Artillery muzzle blast, at 1 km | 100+ peak, impulsive |

**[Medium — these are standard environmental-acoustics ranges rather than
figures from one cited source; the urban and road numbers are corroborated in
direction by the reported 8–15% urban false-alarm rates]**

Two modelling notes. **Artillery blinds intermittently, not continuously** — the
floor returns between rounds, so a firing position degrades acoustic detection
in bursts rather than killing it. And **the moving-vehicle row is why Zvook
demonstrating a sensor on a moving vehicle in August 2026 was news**: it is a
20–40 dB harder problem than a mast.

The **night-versus-day difference is 10–15 dB of ambient**, before any
refraction bonus. That alone is a 3–5x free-field range gain at low frequency
and is the single largest environmental swing in the whole model. The game
currently gives acoustic no day/night variation at all.

### 4.8 Why this vindicates the game's structure

Set `D = 0 dB` as the detection limit and `D = 10 dB` as the solid-detection
limit. At 0.6 of maximum range, spherical spreading alone gives back
20·log10(1/0.6) = **4.4 dB**, plus whatever absorption is saved on the shorter
path — for a quad at 4 kHz over 300 m that is another 4 dB, so **8–9 dB of extra
SNR at 60% of reach**. Against the measured curve (77.8% accuracy at 6 dB SNR,
99.9% clean) that lands in the high 80s to mid 90s percent. Meanwhile the fringe
of reach sits at 0–3 dB SNR, where measured performance is well below 78%.

**So the game's "solid inside 60% of reach, and 52% reliable beyond it" is not
an arbitrary shape — it is close to what the physics and the one measured
SNR-versus-accuracy curve predict.** That is the strongest defence of any number
in the acoustic model, and it is worth writing in a comment next to the
constant.

---

## 5. Altitude and slant range

**Altitude sets a floor on range that nothing can beat.** Slant range is
`sqrt(d² + h²)`, so a target at 3 km altitude is never closer than 3 km however
directly overhead it passes. Everything else follows from putting that number
into §4.1.

Take a small quad: about 80 dBA at 1 m, energy at 2–6 kHz. At 2,000 m, spherical
spreading costs 66 dB and absorption at 4 kHz costs a further 66 dB. The signal
arrives more than 50 dB below any plausible noise floor. **A small electric drone
at 2 km is not acoustically detectable, by anything, ever.** **[High — this is
arithmetic]**

Take a Shahed at 2 km: 105–110 dBA at 1 m but with its energy at 80–250 Hz.
Spreading costs the same 66 dB; absorption costs **2 dB**. Arriving level is
somewhere around 40 dB, which is above a rural night floor of 25–30 dB and
marginal against a daytime 40 dB. That is exactly consistent with Zvook's
claimed 3–5 km and with reporting that Shaheds now cruise at 2–2.8 km and dive
from above 2 km to get above small arms. **Acoustic remains a usable early
warning channel against piston drones at their operating altitude, and that is
what Sky Fortress is for.** **[Medium]**

Jet Shaheds at 7 km are the edge case. With an estimated 125–130 dB source level
and broadband content, spreading costs 77 dB and absorption maybe 7–15 dB
depending on how much of the energy sits below 1 kHz — arriving in the 35–45 dB
region. Detectable on a quiet rural night, marginal otherwise, and with a very
short engagement window at 600 km/h. Ukrainian reporting notes these fly above
what most air defence can reach. **[Low — my calculation, not a measurement]**

**Does looking up change absorption or refraction?** Yes, both, and both
modestly:

- **Absorption.** Temperature drops about 6.5 °C per km and absolute humidity
  drops faster. Colder, drier air raises the mid- and high-frequency absorption
  coefficient, so a 3 km slant path through the real atmosphere loses a few more
  decibels at kilohertz than the sea-level table predicts. Below 500 Hz the
  difference is negligible. Net: **it makes high targets slightly harder to hear
  and changes nothing about which ones are findable.** **[Low — my estimate]**
- **Refraction.** Steep paths are barely refracted. All the wind and inversion
  effects in §4.5–4.6 are grazing-path phenomena. **High-altitude acoustic
  detection is therefore more predictable and less weather-dependent than
  low-altitude detection, and also weaker.** A useful, counterintuitive rule for
  the game: dawn, wind direction and terrain should modulate the detection of
  low-flying drones strongly and of high-flying ones hardly at all. **[Medium]**

And the lag, again: sound from 3 km arrives **8.8 seconds late**. A Shahed at
180 km/h has moved 440 m; a jet variant at 600 km/h has moved 1.5 km. An
acoustic track on a high-altitude target is a historical record, not a position.

---

## 5a. Reference implementation

```python
# Octave-band centres and ISO 9613 alpha at 15 C / 70% RH, dB/km
BANDS = [63, 125, 250, 500, 1000, 2000, 4000, 8000]
ALPHA = [0.1, 0.4, 1.0, 1.9, 3.7, 9.7, 32.8, 117.0]

def audible(src_band_spl,      # list, unweighted dB @ 1 m per band
            d_ground, h,       # metres
            ambient_band,      # list, dB per band
            n_mics=8,
            rh_alpha_mult=1.0, # from the 4.3 table
            wind_ms=0.0, upwind=False,
            inversion=False,
            terrain_blocked=False,
            surface="grass"):
    import math
    r = math.hypot(d_ground, h)
    theta = math.degrees(math.atan2(h, max(d_ground, 1e-6)))
    best = -999
    for i, f in enumerate(BANDS):
        a = ALPHA[i] * (rh_alpha_mult if f >= 2000 else 1.0)
        L  = src_band_spl[i] - 20 * math.log10(max(r, 1.0)) - a * r / 1000.0

        # ground effect
        if theta > 15:   L += 3.0 if surface == "hard" else 0.0
        elif theta < 5:  L -= 10.0 if 200 <= f <= 600 else 4.0

        # terrain diffraction, frequency dependent
        if terrain_blocked:
            L -= 8.0 if f <= 250 else (15.0 if f <= 1000 else 22.0)

        # refraction, grazing paths only
        if theta < 20:
            if inversion:      L += 7.0
            elif wind_ms > 2:  L += (-8.0 if upwind else 3.0)

        # array gain against uncorrelated noise; wind pseudo-noise
        gain = 10 * math.log10(n_mics)
        floor = ambient_band[i]
        if wind_ms > 3 and f <= 250:
            floor += 6.0 * (wind_ms - 3)      # windshielded estimate
        best = max(best, L - floor + gain)

    if best >= 10: return "solid"
    if best >= 0:  return "intermittent"
    return "nothing"
```

Coefficients that are **published**: the ALPHA row, the `10·log10(N)` array gain,
the 20·log10(r) spreading, the 6 dB SNR accuracy anchor. Coefficients that are
**my estimates and are labelled as such above**: every ground, terrain,
refraction and wind constant in that function, and the wind-noise slope. They
are the right sign and roughly the right size; none of them is a measurement.

---

## 6. Localisation versus detection

Published angular accuracy for UAV acoustic sources: **3–5° mean error, 7–11°
RMSE**; a separate study gives roughly **10° in azimuth and elevation at 150 m**
ground distance. Array design sets the ceiling — eight microphones on a 15 cm
square grid are spatially unambiguous only up to ~1.1 kHz. **[Medium]**

Ten degrees is about 175 milliradians. Cannon fire against a manoeuvring drone
needs single-digit milliradians. **Acoustic is short by a factor of twenty to
a hundred.** It can slew a camera turret into the right 20° sector — which is
precisely how Discovair is built, a microphone array bolted to an optical camera
— and it cannot do more than that on its own.

Worse, and underappreciated: **sound is late.** At 340 m/s, a target at 1,000 m
is heard 2.9 s after the fact; a Shahed doing 180 km/h has moved 145 m in that
window. At 300 m against an FPV doing 120 km/h the lag is 0.9 s and 30 m — still
larger than the target. Distributed triangulation fixes the bearing geometry but
cannot fix the lag; it is in the physics. **[High]**

Design consequence for the game: **an acoustic-only track should not be
targetable.** It should mark a sector, wake the unit, and hand off to optics.
If that is too much machinery, give acoustic-only tracks a large to-hit penalty
and a positional error that grows with range.

---

## 7. Verdict on the game's numbers

### The 130 map-metre acoustic reach

At the stated 12:1 compression, 130 map metres is **1,560 real metres** against
a notional signature-100 target, and the current rule `reach × √(sig/100)` gives:

| Target | Sig | Current reach (map m) | Current reach (real m) | Published reality | Verdict |
|---|---|---|---|---|---|
| Small electric quad (FPV) | 70 | 108.8 | ~1,310 | 200–500 m | **~4x too generous** |
| Fiber-optic quad | 70 | 108.8 | ~1,310 | 200–500 m | ~4x too generous |
| Electric heavy multirotor | 95 | 126.7 | ~1,520 | ~600–1,000 m (est.) | ~1.7x too generous |
| Fixed-wing recon (electric) | 25 | 65.0 | ~780 | ~300–600 m | ~1.6x too generous |
| Combustion loitering munition | 35 | 76.9 | ~920 | ~1,000–2,000 m | **too mean** |
| Combustion heavy strike (Shahed) | 55 | 96.4 | ~1,160 | 3,000–5,000 m | **~3x too mean** |
| Turbojet strike | 70 | 108.8 | ~1,310 | ≥2,500 m | ~2x too mean |
| Decoy drone (piston) | 35 | 76.9 | ~920 | ~1,500–2,500 m | too mean |
| Main battle tank | 85 | 119.8 | ~1,440 | 2,000–3,500 m | somewhat mean |

**The base number is not the main problem. The dynamic range is.** The game
currently says a small FPV quad is the second-loudest thing in the sky and
exactly as detectable as a turbojet, and that a Shahed is quieter than a quad.
Every one of those is wrong. The real ordering of *acoustic detection range* is
roughly: turbojet ≈ Shahed > tank > piston decoy > small piston recon > heavy
multirotor > electric fixed-wing > small quad — spanning about **10–15x**, not
the 1.9x the current table spans.

**The √ rule cannot express that.** Over a 0–100 scale, square-root scaling caps
the achievable range spread at 10x only if you are willing to use signature
values near 1, which is absurd. So:

> **Recommendation 1. Acoustic should scale linearly with signature — exponent
> 1.0, not 0.5 — and the signature number should be read as "fraction of
> maximum acoustic detection range", not "loudness".** Everything else in the
> model can keep √. **[This is a modelling choice, not a finding; the finding it
> serves is the 10–15x spread, which is well-supported.]**

> **Recommendation 2. Gun Mount acoustic base: 130 → 200 map metres** (2,400
> real m at signature 100). A gun mount carries a small array on a noisy
> vehicle; it should not get Zvook's tower numbers. A dedicated listening post,
> if one is ever added, should sit at 400–450.

Net effect: against an FPV quad the gun mount's acoustic reach drops from 109 to
**24 map metres** (~290 real m — right on the published figures). Against a
Shahed it rises from 96 to **180 map metres** (~2,160 real m — conservative
against Zvook's 3–5 km, appropriate for a vehicle-mounted array).

**This has a large and, I think, welcome gameplay consequence.** FINDINGS §12
concluded that "microphones are the best anti-drone sensor at any hour" and that
darkness therefore stopped being an attacker advantage. That conclusion is an
artefact of the wrong number. With realistic values, a gun mount at night sees
an FPV at 81 map m on optics and hears it at 24 — optics still wins, and
**darkness becomes a real attacker advantage again**, which matches how both
sides actually fight: FPV and heavy-multirotor night operations are a thing
precisely because nobody can hear a quad in time.

### The 52% edge reliability

**About right. Leave it.** Acoustic is the flakiest channel and deserves the
lowest reliability of the five. If anything the evidence supports being slightly
meaner: 78% detection accuracy at 6 dB SNR and 8–15% urban false-alarm rates are
not the numbers of a dependable sensor, and 52% at the *fringe* of reach is a
fair rendering of that.

Two refinements worth more than moving the number:

> **Recommendation 3. Make acoustic reliability environment-dependent.** 52 as
> the baseline; **35** in rain/high wind or when the listener is near a road,
> generator or firing position; **60** for a quiet rural night. This is the one
> channel where weather should visibly matter, and wind is the single best-
> attested degradation in the literature. **[High]**

> **Recommendation 4. Give acoustic a longer track hold — 4 s rather than 2 s.**
> A drone is a continuous emitter; what breaks an acoustic track is a momentary
> masking event (a passing truck, a gust), not the target disappearing. Acoustic
> should flicker *more* at the edge and drop out *less* abruptly. **[Medium — my
> inference from the mechanism]**

### The "35% at altitude" rule

**Wrong in both directions simultaneously.** It is far too generous for small
electric aircraft (which are simply gone at 2 km) and too harsh for combustion
and jet aircraft (which are exactly what the real high-altitude acoustic nets
are built to hear). With the corrected signature table it mostly fixes itself,
because the small-quad values are now tiny:

### Day, night and weather

The game applies day/night scaling to optical and thermal and **nothing to
acoustic**. That is a real omission: §4.7 puts the ambient swing between midday
and a calm rural night at 10–15 dB, and §4.5 adds a dawn inversion on top. At
low frequency, where absorption is negligible, 12 dB is a 4x free-field range
gain.

> **Recommendation 7. Give acoustic a time-of-day multiplier: ×1.0 by day,
> ×2.0 at night, ×2.5 at dawn — applied only to low-altitude targets.** High
> targets sit on steep ray paths that refraction barely touches, so they should
> get the ambient part (×1.5) and not the refraction part. This makes night a
> double-edged choice for the attacker: better against optics, worse against
> microphones, which is a more interesting decision than the current model
> offers. **[Medium]**

> **Recommendation 8. Wind should modulate acoustic and nothing else.** ×1.0
> below 3 m/s, ×0.7 at 5–8 m/s, ×0.4 at 8–12 m/s, and **acoustic off entirely
> above 12 m/s**. If the game ever models wind direction, a downwind listener
> gets ×1.4 and an upwind one ×0.5. **[Medium on the ladder, Low on the
> direction multipliers — both are my estimates from §4.6]**

> **Recommendation 5. High-altitude acoustic multiplier 0.35 → 0.50**, applied
> on top of the new signature values. A Shahed at altitude then gives
> 0.50 × 0.85 × 200 = 85 map m (~1,020 real m) from a gun mount and ~190 map m
> (~2,280 m) from a dedicated post — credible against Zvook's claims. A quad at
> altitude gives 0.50 × 0.12 × 200 = 12 map m, i.e. effectively nothing, which
> is correct.

### Recommended acoustic signature values

Using linear scaling and a 200 map-metre gun-mount base — so real detection
range ≈ 24 × signature metres.

| Unit | Current | **Recommended** | Implied real range | Reasoning |
|---|---|---|---|---|
| Small electric quad (FPV) | 70 | **12** | ~290 m | Fraunhofer 50–200 m; Zvook NW0 150–450 m; 300–500 m industry ceiling. High-frequency content, dies in the air. |
| Fiber-optic quad | 70 | **12** | ~290 m | Acoustically identical. The whole point of the fiber unit is that RF silence buys nothing here — keep it equal. |
| Electric heavy multirotor | 95 | **35** | ~840 m | Six large low-BPF rotors, high radiated power, but still electric and still mid-frequency. Loudest of the electrics by a wide margin, well short of a piston engine. **[Medium — no published range figure for this class; scaled from disc loading and rotor count]** |
| Fixed-wing recon (electric) | 25 | **18** | ~430 m | Single prop, often throttled. Slightly louder than a quad, much quieter than anything combustion. **[Low–Medium]** |
| Combustion loitering munition | 35 | **55** | ~1,320 m | Two-stroke, 80–250 Hz, "lawnmower", routinely heard from the ground at operating altitude. |
| Combustion heavy strike (Shahed/Geran) | 55 | **90** | ~2,160 m | The reference loud target; Zvook 3–5 km from a mast, discounted for a vehicle array. The single biggest correction in the table. |
| Turbojet strike | 70 | **95** | ~2,280 m | Very high source level; hard to engage because of speed and altitude, not quietness. Let the altitude rule and its speed do that work, not the signature. **[Low]** |
| Decoy drone (Gerbera class) | 35 | **65** | ~1,560 m | Piston-engined decoys are *meant* to look and sound like the real thing. If a decoy is quiet it is not doing its job. Note: if the design intends glider/electric decoys, use 15 instead. |
| Main battle tank | 85 | **80** | ~1,920 m | Tracks and a diesel are very loud and very low-frequency, but ground-level propagation is worse than air-to-ground: terrain screens it. Slight reduction only. **[Medium]** |
| Jammer, transmitting | 20 | **20** | ~480 m | It is a generator on a truck. Unchanged. |

Sanity check on the ordering this produces: turbojet > Shahed > tank > decoy >
loitering munition > multirotor > fixed-wing electric > quad. That matches the
physical argument in §3 and the measured/claimed ranges in §2.

---

## 8. False alarms

What else sounds like a drone, in rough order of nuisance value:

- **Lawnmowers, generators, chainsaws, brush cutters.** Small piston engines
  with harmonic combs. These are not *like* a Shahed's signature, they are
  essentially the same signature at zero altitude. **[High]**
- **Mopeds and motorcycles.** The comparison runs both ways: Ukrainian observers
  describe the Shahed as sounding like a moped. **[High]**
- **Passing vehicles and birds.** Directly measured as the source of a **5–8%
  false-positive rate** in one study's affected recordings. **[Medium]**
- **Wind on the microphone.** Not a classification failure but a noise-floor
  failure; it produces dropouts rather than ghosts.
- **Distant helicopters and light aircraft**, and in an urban setting, HVAC
  plant and rooftop fans.

Deployed systems handle this with confidence scoring over signal stability,
harmonic structure and **multi-node correlation** — a real drone appears at
several sensors with consistent geometry; a lawnmower does not. That is the
strongest argument for distributed networks over single sensors and it is worth
representing. **[Medium]**

> **Recommendation 6.** Acoustic should be the only channel that can generate a
> *false* contact — a short-lived ghost near roads, generators or friendly
> vehicles, at something like a 10% rate per genuine alert in cluttered terrain.
> It gives the channel a personality (early, unreliable, cheap) that the other
> four do not have.

---

## 9. Real deployments

**Sky Fortress (Ukraine).** Began as a microphone and a mobile phone on a pole.
Roughly **9,500 sensors** by the US Army's account, with other reporting citing
**14,000 in place** and **15,000 third-generation units** planned. Unit cost
**$400–500** (some sources say up to $1,000). First-generation units used mobile
phones as the processor; third-generation units use purpose-built compute.
Sensors triangulate over the cellular network; tracks are fused with radar in
central command systems and pushed to **mobile fire teams on tablets** — the
acoustic net is a *cueing* layer for guns, which is exactly the role §6 says it
can support. Claimed effect includes 80 of 84 UAVs intercepted in one raid and
"95% interception rates". **[High on scale and architecture; Low on the
effectiveness figures — these come from programme advocates]**

**Zvook (Ukraine, Lviv).** Sensors on radio towers and fixed infrastructure at
**10–12 m**, claiming **3–5 km on Shahed-type drones** and **5–7 km on cruise
missiles** (sources disagree on which pair). Separately sells the **NW0**
tactical sensor for FPV detection: **150–450 m declared, 360°**. In **August
2026**, at NATO's Baltic exercise in Latvia, Zvook demonstrated its passive
sensors on a **moving vehicle** for the first time. **[Medium]**

**Acoustic-cued turrets.** Ukraine fields several automated counter-drone
turrets — Khyzhak (7.62 mm, via Brave1), the Dron ZP 12.7 mm neural-network
turret, Sky Sentinel, net-capture turrets. Reporting on these describes acoustic
and passive RF as *cueing* sensors used when dust or smoke defeats the optics,
with optical tracking doing the actual gun-laying. That is the correct
division of labour and the one the game should model. **[Medium]**

**NATO / Europe.** **Latvia is the first NATO member to deploy acoustic drone
detection along its entire eastern border.** NATO's Baltic Trust 26 exercise
(Riga, 3–14 August 2026) tested the BlackTalon ecosystem fusing radar, EO/IR,
RF, acoustic and jamming. The US Army C5ISR Center issued an RFI on 14 January
2026 for acoustic detection of Group 1 and Group 2 UAS for dismounted soldiers;
the US Air Force has sought acoustic sensors as well. **[High]**

---

## 10. What changes by 2027–28

1. **Classification improves faster than range does.** The bottleneck on
   acoustic is SNR, which is physics, and classification, which is software.
   Expect the software half to improve substantially — better noise robustness,
   domain adaptation to battlefield audio, fewer nuisance alarms. Expect range
   to improve only through array size and better siting. **Model change: raise
   acoustic edge reliability from 52 toward 60–65 by 2028 for networked sensors;
   do not raise reach.** **[Medium]**
2. **Distributed nets become the default, and coverage replaces range as the
   relevant quantity.** At $400–500 a node, the sensor is the network. Ukraine
   proved it; Latvia is copying it along a national border. **Model change: if
   the game ever adds a listening-post structure, its value should be area
   coverage and multi-node confirmation — better reliability and a bearing
   crossing — rather than a bigger radius.** **[High on the trend]**
3. **Quieter small drones — real but overstated.** MIT Lincoln Laboratory's
   toroidal propellers are credited with up to **20 dB** of reduction versus
   conventional props at comparable thrust, concentrated in the **1–5 kHz**
   band, which is precisely the detection band. Twenty decibels is a 10x range
   reduction in free field. I would not model the full 20 dB — that is a
   best-case laboratory comparison — but 6–10 dB fielded is plausible.
   **Model change: by 2028, small electric quad acoustic 12 → 7, heavy
   multirotor 35 → 25.** **[Medium on the claim, Low on the fielded figure]**
4. **The loud targets stay loud, and go higher.** Piston Shaheds are moving to
   two-cylinder engines partly for acoustic reasons, which is a few decibels,
   not a transformation. Jet variants go faster and higher instead. **Model
   change: none to the signature; the altitude rule already handles it.**
   **[Medium]**
5. **Fiber-optic drones keep acoustic alive despite its weakness.** Against an
   airframe that emits nothing on RF and is too small for radar, a microphone is
   one of very few passive options, however short its range. Expect acoustic to
   be bought in quantity *because the alternatives fail*, not because it is
   good. That is a satisfying thing for a game to model: a cheap, unreliable,
   short-ranged sensor that you buy anyway.

---

## 11. What is genuinely uncertain

- **Per-node range of Sky Fortress is not published.** Every network-level
  effectiveness claim about it comes from people selling or promoting it.
- **The Zvook figures disagree with themselves** across sources (3 vs 5 km on
  Shaheds, 5 vs 7 km on cruise missiles). I used the lower end.
- **There is no published measured acoustic detection range for a heavy
  multirotor of the Baba Yaga class.** My value of 35 is scaled reasoning from
  rotor count and disc loading, not a measurement. Flag it if it becomes
  load-bearing.
- **Turbojet drone acoustics are genuinely murky.** Sources contradict each
  other on whether the jet is louder or quieter than the piston version. My
  value of 95 assumes louder; if anyone finds a measurement, use it.
- **My tank figure (80) assumes open terrain.** Ground-to-ground acoustic
  propagation is far more terrain-sensitive than air-to-ground.
- **Every propagation coefficient outside the ISO absorption table is my
  estimate.** Specifically: the ground-effect values (+3 / −10 dB), the barrier
  losses (8/15/22 dB by band), the refraction terms (+7 dB inversion, ±8 dB
  wind), the wind-noise slope (6 dB per m/s above 3 m/s in the low band), the
  humidity multipliers on α, the wind-speed ladder, and the day/night and
  dawn multipliers. They are the right sign and roughly the right magnitude,
  and any one of them could be off by 5 dB. The three things I would defend
  without qualification are spherical spreading, the absorption curve's shape,
  and `10·log10(N)` array gain.
- **No published source level exists for any military airframe in §3a.** The
  Shahed, Orlan, Baba Yaga, tank and turbojet rows are estimates anchored on
  consumer multirotor measurements and on standard rotor and jet noise scaling
  laws. If a real measurement surfaces for any of them, it should overwrite the
  estimate immediately — the Shahed row in particular is load-bearing for the
  whole signature table.
- **The choice of linear rather than square-root scaling for acoustic is a
  modelling decision, not a finding.** The finding is the 10–15x range spread
  between airframe classes. If the design prefers one rule for all five
  channels, keep √ and accept that no signature table can reproduce the real
  spread — in which case the honest fallback is to widen as far as √ allows
  (quad 10, Shahed 95, a 3.1x spread) and note in the code that acoustic is
  compressed on purpose.

---

## Sources

- US Army, "Listening to the Sky: Acoustic Drone Detection Systems – Ukraine & Emerging Technologies" — https://www.army.mil/article/292099/listening_to_the_sky_acoustic_drone_detection_systems_ukraine_emerging_technologies
- United24 Media, "Sky Fortress—Ukraine's Acoustic Detection System That Tracks Drones Cheap and Fast" — https://united24media.com/war-in-ukraine/sky-fortress-ukraines-acoustic-detection-system-that-tracks-drones-cheap-and-fast-9451
- Odessa Journal, Ukrainian acoustic sensor network — https://odessa-journal.com/ukrainian-engineers-have-created-a-network-of-nearly-10000-acoustic-sensors-to-track-russian-drones
- United24 Media, "Ukraine's Zvook Acoustic System Tests Mobile Detection of Low-Flying Missiles and Drones" — https://united24media.com/defense-tech/ukraines-zvook-acoustic-system-tests-mobile-detection-of-low-flying-missiles-and-drones-21587
- Unmanned Airspace, "Ukrainian start-up ZVOOK launches new low-cost C-UAS FPV drone acoustic sensor" — https://www.unmannedairspace.info/counter-uas-systems-and-policies/ukrainian-start-up-zvook-launches-new-low-cost-c-uas-acoustic-sensor/
- Militarnyi, "Ukraine Develops Acoustic Detector for FPV Drones" — https://militarnyi.com/en/news/ukraine-develops-acoustic-detector-for-fpv-drones/
- Drone Warfare, "Counter-UAS 101 – Acoustic Drone Detection" — https://drone-warfare.com/counter-uas/acoustic-detection/
- Robin Radar, "The Pros and Cons of Using an Acoustic Detection System Against Drones" — https://www.robinradar.com/blog/acoustic-sensors-drone-detection
- Squarehead Technology, Drone Detection (Discovair G2) — https://www.sqhead.com/drone-detection
- Discovair G2 product sheet (cuashub mirror) — https://cuashub.com/wp-content/uploads/2022/12/Discovair-G2-Product-Sheet_v1.3.pdf
- Fraunhofer IDMT, "Reliable acoustic drone detection" — https://www.idmt.fraunhofer.de/en/Press_and_Media/press_releases/2025/acoustic-drone-detection.html
- Fraunhofer IDMT, "Detecting drones early — with intelligent acoustics from Oldenburg" — https://www.idmt.fraunhofer.de/en/Press_and_Media/press_releases/2026/Detecting_drones_early_EN.html
- MDPI Sensors, "Outdoor Microphone Range Tests and Spectral Analysis of UAV Acoustic Signatures for Array Development" — https://doi.org/10.3390/s25227057
- Acta Acustica, "Passive acoustic detection and localization of drones using MEMS microphones and machine learning" — https://acta-acustica.edpsciences.org/articles/aacus/full_html/2026/01/aacus250134/aacus250134.html
- Multimedia Tools and Applications, "Deep Learning-based drone acoustic event detection system for microphone arrays" — https://link.springer.com/article/10.1007/s11042-023-17477-1
- JASA 151(2) 2022, "Acoustic detection of unmanned aerial vehicles using biologically inspired vision processing" — https://pubs.aip.org/asa/jasa/article/151/2/968/2838373/Acoustic-detection-of-unmanned-aerial-vehicles
- JASA 152(5), "Drone noise directivity and psychoacoustic evaluation using a hemispherical microphone array" — https://pubs.aip.org/asa/jasa/article/152/5/2735/2839503/Drone-noise-directivity-and-psychoacoustic
- AIP Advances, "From classical approaches to recent advancements: A holistic review of acoustic detection for UAVs" — https://pubs.aip.org/aip/adv/article/15/12/120701/3373725/From-classical-approaches-to-recent-advancements-A
- arXiv, "Acoustic UAV Detection in Battlefield Scenarios: Handling Noise, Domain Shift, and Weak Labels" — https://arxiv.org/html/2608.14287v1
- arXiv, "Acoustic Analysis of Uneven Blade Spacing and Toroidal Geometry for Reducing Propeller Annoyance" — https://arxiv.org/pdf/2504.12554
- Springer Aerospace Systems, "Enabling quiet urban flight: MIT's toroidal propeller as a game-changer in reducing drone acoustic signatures" — https://link.springer.com/article/10.1007/s42401-025-00375-9
- ISO 9613-1:1993, Attenuation of sound during propagation outdoors — absorption by the atmosphere — https://www.iso.org/standard/17426.html
- Wikipedia, HESA Shahed 136 — https://en.wikipedia.org/wiki/HESA_Shahed_136
- Wikipedia, Shahed 238 — https://en.wikipedia.org/wiki/Shahed_238
- Army Recognition, "Russia's jet-powered Shahed-238 drones introduce new challenges to Ukraine's air defenses" — https://www.armyrecognition.com/focus-analysis-conflicts/army/conflicts-in-the-world/russia-ukraine-war-2022/russias-jet-powered-shahed-238-drones-introduce-new-challenges-to-ukraines-air-defenses
- Defense Express, "Why russia's Jet-Powered Drones Fly at 7 km, Which Air Defense Systems Cannot Reach Them" — https://en.defence-ua.com/weapon_and_tech/why_russias_jet_powered_drones_fly_at_7_km_which_air_defense_systems_cannot_reach_them-19700.html
- United24 Media, "Overwhelming Air Defense: The Impact of Swarming Shahed Drones on Ukraine" — https://united24media.com/war-in-ukraine/why-russias-drone-swarms-are-getting-deadlier-by-flying-higher-9305
- Euromaidan Press, "Shahed drones now dive like missiles—and Ukraine can't shoot fast enough" — https://euromaidanpress.com/2025/06/29/why-cant-ukraine-stop-russias-shahed-drones-anymore/
- Militaer Aktuell, "Baba Yaga – Ukraine's dreaded attack drone" — https://militaeraktuell.at/en/baba-yaga-ukraines-dreaded-attack-drone/
- Wikipedia, Baba Yaga (aircraft) — https://en.wikipedia.org/wiki/Baba_Yaga_(aircraft)
- DefenseScoop, "Army seeks acoustic detection systems to counter small drones" — https://defensescoop.com/2026/01/21/army-counter-drone-small-uas-acoustic-detection-systems/
- Unmanned Airspace, "US Army publishes RFI for small drone acoustic detection systems" — https://www.unmannedairspace.info/counter-uas-systems-and-policies/us-army-publishes-rfi-for-small-drone-acoustic-detection-systems/
- Defence Blog, "NATO tests British counter-drone system in Baltic exercise" — https://defence-blog.com/nato-tests-british-counter-drone-system-in-baltic-exercise/
- Army Recognition, "Ukrainian Innovation by Dron ZP Targets FPV Drones with Semi-Autonomous Turret" — https://www.armyrecognition.com/focus-analysis-conflicts/army/conflicts-in-the-world/russia-ukraine-war-2022/ukrainian-innovation-by-dron-zp-targets-fpv-drones-with-semi-autonomous-turret
- The Defense Post, "Meet Ukraine's Newest Counter-Drone Tech: An Autonomous Net Turret for FPV Systems" — https://thedefensepost.com/2026/05/08/ukraine-autonomous-net-turret/
- D-Fend Solutions, "Counter-Drone Detection Technologies Compared" — https://d-fendsolutions.com/anti-drone-detection/
- Dronesgator, "How Loud Are Drones? Noise Levels by Model (2026)" — https://dronesgator.com/how-loud-are-drones

**Note on method.** The network proxy in this environment blocks direct page
fetches for most of the domains above (MDPI, arXiv, Fraunhofer, Robin Radar,
Acta Acustica and others all returned egress errors). Figures were taken from
search-result summaries of those pages. Anything marked [High] I would stand
behind; anything marked [Medium] or [Low] should be re-verified against the
source page before it becomes load-bearing.
