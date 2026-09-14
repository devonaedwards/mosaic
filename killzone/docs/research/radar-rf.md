# Radar and passive RF detection of drones

Research note for the KILL ZONE balance model. Companion to `BRIEF.md`.
Compiled September 2026. All distances given twice: **real** (km/m) and **map
metres** at the game's 12:1 compression (100 map m ≈ 1.2 km).

**Method note.** Many defence and vendor domains (drone-warfare.com,
robinradar.com, echodyne.com, arxiv.org, radartutorial.eu) are blocked by this
environment's network egress proxy, so several figures below come from search
engine extracts of those pages rather than the pages themselves. Where that is
the case the figure is marked **[extract]** and given lower confidence. Nothing
here is invented; where I have reasoned past the sources I have labelled it
**[inference]**.

The propagation sections (§2B, §3A.1) rest on standard radar and RF engineering
relations — the radar range equation, the two-ray multipath model, the 4/3-earth
horizon, Friis, thermal-noise sensitivity. Those are textbook and uncontroversial.
Several *numeric coefficients* inside them (clutter reflectivity σ0, ITU-R rain
and gaseous attenuation, required SNR for Swerling-1 detection, MTI improvement
factors) are quoted from memory of the standard references named in §8, because
the web-search budget for this session was exhausted before I could re-open them.
Those are marked **[Recalled — verify]**. They are textbook values rather than
guesses, but check them before shipping.

Confidence markers used throughout: **[High]** measured or specified in a
primary/peer-reviewed source · **[Med]** vendor spec, trade press, or consistent
across secondary reporting · **[Low]** single source, contested, or marketing ·
**[Inference]** my reasoning from the above.

---

## 1. Summary — what a designer needs to take away

1. **The fourth-root law is correct physics and the game is using it wrongly.**
   Radar range really does go as RCS^(1/4) **[High]**. But the game applies that
   exponent to a *linear* 0–100 signature scale that is already compressed. Real
   radar cross-sections across the game's unit roster span four to five orders of
   magnitude; the game's radar column spans 22 to 80. Applying a fourth root to
   an already-compressed scale compresses it again, and the result is that every
   air target in the game is detected at within 38% of the same range. That is
   the single largest error in the sensor model.

2. **The decoy drone is currently pointless.** A Luneburg lens or trihedral
   corner reflector adds something on the order of **+25 to +40 dB** to a foam
   airframe's return **[Inference from High-confidence geometry]**. In the game
   this becomes 22 → 80 on a linear scale, which after the fourth root is a 1.38×
   range advantage. It should be closer to **4–8×**. Decoys exist precisely
   because the fourth root means you *cannot* process your way out of a 30 dB
   signal difference.

3. **1400 map metres (≈16.8 km) is too far as a *reference* range and about
   right as an *instrumented ceiling*.** Real dedicated C-UAS radars get 2.7–5 km
   on a Group 1 quadcopter and 5–15 km on a Shahed **[Med]**. Recommendation:
   demote 1400 to a hard cap and set the Radar Mast's reference reach to ~900 map
   m against a Shahed-class target.

4. **78% radar edge reliability is generous for small low targets and ignores the
   bigger problem, which is false tracks.** Fielded radars in cluttered
   environments can throw up to ~1,000 candidate targets per hour with false
   positive rates above 35% where birds are active **[Low — single trade
   source]**. The game models misses but not ghosts.

5. **900 map metres (≈10.8 km) passive RF is wrong in both directions at once.**
   Far too long against an FPV control link (real: 1–5 km **[Med]**); far too
   short against a transmitting jammer, a satcom terminal or a radar, which are
   detectable at tens of kilometres **[High]**. Passive RF range is driven almost
   entirely by the *emitter's* power, not by the target's size.

6. **The square-root rule for passive RF is right.** One-way propagation gives
   received power ∝ P/R², so R ∝ √P **[High]**. Keep it. Fix the signature values
   instead.

7. **95% RF reliability is right for the wrong targets.** A jammer or an analogue
   video transmitter is essentially a 100%-reliable detection. A frequency-
   hopping military mesh link in a congested band is not. And fibre-optic and
   emissions-silent autonomous drones are **genuinely, physically zero** — this
   is confirmed and well sourced **[High]**.

8. **Passive RF gives you a bearing, not a firing solution.** A single RF sensor
   produces a line of bearing. Range requires two sensors crossing, or TDOA with
   sub-nanosecond synchronisation that breaks down in multipath **[High]**. The
   game currently treats an RF detection as equivalent to a radar track. It
   should not be.

9. **Radar can see ground targets, and the game says it cannot.** Modern C-UAS
   radars classify "human" and "vehicle" alongside "multirotor" **[Med]**, and
   one 3D radar is specified at 12 km on vehicles versus 5 km on a Phantom 4
   **[Med]**. The tank's `—` in the radar column is wrong. The correct constraint
   is Doppler: a *stationary* vehicle is invisible, a *moving* one is loud.

10. **By 2028 passive RF stops being an anti-drone sensor and becomes an
    anti-emitter sensor.** Fibre-optic and terminal-autonomy drones are removing
    the thing RF listens for, while jammers, radars, satcom terminals and
    operators keep radiating. The game should reflect that shift, not just the
    current snapshot.

11. **Both textbook equations over-predict by 20–30 dB, and that gap is the
    model.** The free-space radar equation with realistic counter-drone
    coefficients puts a Phantom at 14.2 km; vendors deliver 3–5 km — a **~22 dB
    environment penalty**. The free-space RF link budget puts an FPV video
    transmitter at 33 km; vendors deliver 1–5 km — a **~28 dB penalty**. Carry
    those as explicit additive dB loss terms (§2B.1, §3A.1) rather than fudging
    the signature numbers.

12. **The Doppler notch is the missing mechanic.** A target with less than about
    **1.5 m/s of radial velocity** sits inside ground clutter and is not detected
    at all, however large its RCS (§2B.3). This one rule reproduces "low and slow
    is hard" from first principles, makes hovering and tangential flight into
    real tactics, and explains why radar handles Shaheds far better than FPVs.

13. **Terrain masking is binary; the radar horizon is a red herring.** At
    counter-drone ranges the geometric horizon is 20–50 km and never binding. A
    single 15 m treeline 1 km from a 10 m mast blanks everything below 35 m out
    to 5 km (§2B.5). Model the ray, not the curvature.

14. **The signature scales must be logarithmic, and now there is a measured span
    to justify it.** Radar cross-section across the roster runs from a racing
    quad at **0.001 m² (−30 dBsm)** to a reflector decoy at **~32 m² (+15 dBsm)**
    — **45 dB, a 13:1 range spread**. Radiated power runs from a 25 mW video
    downlink (**14 dBm**) to a radar mainbeam (**85 dBm**) — **71 dB, a ~60:1
    range spread** (§2A, §3A).

**§2A and §3A give the full signature spreads with named endpoints; §2B and
§3A.1–3A.3 give the propagation physics; §3B assembles both into two functions
ready to implement.**

---

## 2. RADAR (active)

### 2.1 Radar cross-section — what is actually measured

RCS is not a property of an object; it is a property of an object *at a
frequency, a polarisation and an aspect angle*. Every table below is a
simplification of something that varies by 20 dB or more as the target turns.

| Target | Measured / reported RCS | dBsm | Source & confidence |
|---|---|---|---|
| DJI Phantom-class quad | 0.05 m² @ 24 GHz | −13 | Compact-range & in-flight measurement **[High]** |
| DJI Phantom-class quad | 0.01 m² @ 10 GHz (X-band) | −20 | Same **[High]** |
| DJI Phantom 3 Std | @ 94 GHz, two runs | −17 / −23 | In-flight K/W-band measurement **[High]** |
| Phantom, modelled | Swerling 1, mean 0.01–0.35 m² | −20 to −4.6 | Varies with prop rotation, polarisation, frequency, elevation **[High]** |
| Quadcopter vs airport surveillance radar | as low as −23 dBsm; **not reliably tracked beyond 500 m** | −23 | UK CAA trials, via extract **[Med]** |
| Small fixed-wing UAV | −9.8 to −5.3 dBsm @ 2.75 GHz | −10 to −5 | Björklund et al., IET RSN 2024, typical flights **[High]** |
| Small fixed-wing UAV | −7.8 to −5.0 dBsm @ 4.51 GHz | −8 to −5 | Same **[High]** |
| UAVs generally, X-band | −15 to −5 dBsm | −15 to −5 | RCS measurement survey **[Med]** |
| Drones at DVB-T (UHF) frequencies | −60 to −20 dBsm, **rising with frequency** | −60 to −20 | NATO STO passive-radar feasibility study **[High]** |
| Shahed-136 / Geran-2 | **no credible open figure** | — | See below **[Low]** |
| Trihedral corner reflector, 0.25 m edge, X-band | 18 m² | **+12.6** | Standard geometry, my arithmetic **[Inference]** |
| Trihedral corner reflector, 0.30 m edge, X-band | 38 m² | **+15.8** | Same **[Inference]** |
| Luneburg lens, mm-wave lab samples | up to +15.8 dBsm @ 76.5 GHz | +15.8 | Lens RCS frequency-response study **[High, but wrong band]** |

**On the Shahed.** There is no trustworthy open-source RCS number for the
Shahed-136/Geran-2. What is reported is qualitative and consistent: a delta-wing
planform, an internal honeycomb structure intended to reduce return, and
wing-structure materials believed to scatter incident energy **[Med, CSIS /
RUSI / GlobalSecurity]**. A figure of −38.5 dBsm circulates online but traces to
a commercial wargame's target database, not to measurement — **do not use it**.
My working estimate, clearly flagged as **[Inference]**: a 2.5 m-span composite
airframe with a metal piston engine and a metallic warhead is not a stealth
object; aspect-averaged it should sit around **0.1–0.5 m² (−10 to −3 dBsm)**,
lower nose-on, much higher beam-on. That is one to two orders of magnitude above
an FPV quad and it matches the observed fact that C-UAS radars engage Shaheds at
5–15 km and FPVs at 1–3 km.

**The frequency trap.** Note the DVB-T line in the table. Small drones get
*harder* to see at low frequency, not easier, because at UHF a 30 cm airframe is
in the Rayleigh region. This is the opposite of the intuition people bring from
stealth aircraft. It is why every serious counter-drone radar is X-band or
Ku-band, and it is worth a line of flavour text if the game ever exposes radar
bands.

### 2.2 Detection ranges actually achieved

| System | Target | Range (real) | Map m | Confidence |
|---|---|---|---|---|
| Echodyne EchoShield | Group 1 drone | **2.7–4.8 km**, <0.5° angular accuracy | 225–400 | Vendor spec **[Med]** |
| O.W.L. GA9000 3D radar | DJI Phantom 4 | **5+ km** | 415+ | Vendor spec **[Med]** |
| O.W.L. GA9000 3D radar | **vehicles** | **12 km** | 1000 | Vendor spec **[Med]** |
| O.W.L. GA9000 3D radar | full-size aircraft | **15 km** | 1250 | Vendor spec **[Med]** |
| Robin ELVIRA / IRIS | instrumented range | **5 km**, 360° az, 60° el, 78 km² | 415 | Vendor spec **[Med]** |
| Robin IRIS + LRM module | fixed-wing / Shahed | **5–12 km**, typical **7–8 km** | 580–1000 | Vendor + trade press, 2025–26 **[Med]** |
| Ukrainian CD-T10 (X-band) | air targets to 3 km alt | **15 km** | 1250 | Trade press 2026 **[Med]** |
| Ukrainian CD-T15 | >300 simultaneous tracks | **22 km** | 1830 | Trade press 2026 **[Low]** |
| Rapid Ranger (mobile) | Shahed-class | **15 km** | 1250 | Trade press **[Low]** |
| Ukrainian "Pelikan" | early-warning, large targets | **up to 400 km** | off-map | Trade press **[Low]** |
| Mobile Ku-band, FPV-optimised | FPV quad | "**a handful of kilometres**" | ~150–300 | Trade extract **[Low]** |
| Standard airport surveillance radar | small quadcopter | **<500 m reliable** | <42 | UK CAA via extract **[Med]** |
| Ground surveillance radar (GO 12 class) | ground movers | **up to 27 km** | 2250 | Radar reference **[Med]** |

The shape of this table is the important thing. Against a Phantom-class quad the
best dedicated sensors get **3–5 km**. Against a Shahed they get **7–15 km**.
Against a vehicle, **12 km**. That is a **3–4× spread between an FPV and a
Shahed**, and roughly **4:1 between an FPV and a truck**. Those ratios are what
the game's model has to reproduce, and currently does not.

### 2.3 What clutter, low altitude and terrain actually cost

This is where the honest answer is "everything, and not in a way a multiplier
captures well."

- Drones flying **below ~100 m** deliberately exploit ground clutter and
  multipath fading. Ground clutter energy dominates returns at low grazing
  angles, because the beam footprint covers a large area of terrain, vegetation
  and structures that all reflect **[Med]**.
- Trade and engineering sources are explicit that this creates **detection gaps
  that no amount of signal processing can eliminate** — it is a geometry problem,
  not a DSP problem **[Med]**.
- Shaheds are reported flying "as low as a few metres" on some profiles and at
  under 200 km/h, which makes them both clutter-embedded and slow enough to fall
  near the Doppler notch of radars tuned for fast movers **[Med]**.
- Urban environments are the worst case: multipath from buildings creates false
  targets, and dense traffic clutter masks real returns **[Med]**.
- **Radar horizon is usually *not* the binding constraint at these ranges.** With
  the standard 4/3-earth relation d ≈ 4.12(√h_radar + √h_target) in metres/km, a
  10 m mast sees a 5 m-high target at ~22 km and a 50 m-high target at ~42 km —
  both beyond C-UAS detection range. **[Inference from standard relation]** So
  when a low drone is missed, it is being missed to *clutter and terrain masking*,
  not to earth curvature. Design implication: model masking against terrain
  features, not against a horizon formula.

**Recommended replacement for the game's "+20% radar at high altitude" rule.**
A flat +20% badly understates this. Suggested three-band multiplier:

| Target altitude band | Radar reach × | Radar edge reliability |
|---|---|---|
| High (above clutter, clean Doppler) | **1.25** | **0.85** |
| Medium | **1.00** | **0.70** |
| Nap-of-earth / terrain-following | **0.40** | **0.35** |

The nap-of-earth row is the whole story of the Shahed war and of FPV attack. It
should hurt.

### 2.4 Bird discrimination, micro-Doppler and false alarms

**How it works.** A drone's propellers produce a high-frequency micro-Doppler
modulation — blade rotation on the order of **50–100 Hz** — where a bird's
wingbeat is a slow, irregular **4–10 Hz** flap **[Med, vendor technical blog;
consistent with peer-reviewed K/W-band micro-Doppler work **[High]**]**. That
difference is large and robust, which is why micro-Doppler is the standard
discriminator.

**How well it works, in the lab.** Convolutional-net classification on
micro-Doppler spectrograms reports **~90%+ validation accuracy** generally,
**96–97%** for hovering targets using centroid features, and a bird-as-drone
**false alarm rate of 1.23%** for one network and **0%** for GoogLeNet on a 324
bird-image test set **[High, IET RSN 2020]**.

**How well it works, in the field.** Considerably worse. Fielded C-UAS radars in
complex environments are reported generating **up to 1,000 candidate targets per
hour**, with **false positive rates exceeding 35% in areas with bird activity**
**[Low — single trade-industry source, no methodology given]**. Operational
write-ups consistently say false positives, not missed detections, are the
under-reported problem, because they are what erodes operator trust **[Med]**.

**Design implication.** The gap between 1.23% in a curated dataset and 35% in the
field is the most useful single fact in this section. The game currently models
detection reliability as a per-channel probability of *seeing a real thing*. A
radar that also produces ghosts is more interesting and more accurate. Minimum
viable version: at ranges beyond 60% of reach, a radar contact has a **15–25%
chance of being a false track** that resolves to nothing after two seconds. That
alone makes radar feel like radar.

### 2.5 The fourth-root law — is it the right model for a game?

**The physics.** R_max ∝ (P·G²·λ²·σ / P_min·(4π)³·L)^(1/4), so range ∝ σ^(1/4)
**[High]**. Halving RCS multiplies detection range by 0.84. Reducing RCS by **16×
halves** the range. **[High]**

**The verdict for KILL ZONE: keep the exponent, replace the scale.**

The problem is arithmetic. Take the game's own numbers. Radar Mast reach 1400,
fourth-root scaling:

| Unit | Game radar sig | ×(sig/100)^0.25 | Reach (map m) | Real equivalent |
|---|---|---|---|---|
| Small electric quad (FPV) | 22 | 0.685 | 959 | **11.5 km** |
| Fixed-wing recon | 40 | 0.795 | 1113 | 13.4 km |
| Combustion heavy strike (Shahed) | 60 | 0.880 | 1232 | 14.8 km |
| Decoy with reflectors | 80 | 0.946 | 1324 | 15.9 km |

Every air target in the game is detected between 11.5 and 15.9 km — a **1.38:1
spread**. Reality is a **3–4:1 spread** between FPV and Shahed and more like
**8:1** once you include a reflector decoy. And 11.5 km against a 7-inch FPV quad
is roughly **four to six times** what any real radar achieves.

There are two fixes, and the first is better.

**Fix A (recommended): make the radar signature column a log scale, keep the
fourth root.** Define `radar_sig = 2 × dBsm + 60`, i.e. 60 = 1 m², and every 2
signature points = 1 dB. Then:

```
reach = min(instrumented_cap, reference_reach × 10^((radar_sig − 50) / 80))
```

`reference_reach` is the range against a `radar_sig = 50` target (−5 dBsm,
0.3 m² — a small fixed-wing / light Shahed). This is exact fourth-root physics
expressed on a scale that can actually hold four orders of magnitude.

**Fix B (if the team refuses to change the scale): use a linear exponent, not a
fourth root.** If you keep the current 0–100 linear numbers and want a realistic
4:1 spread between FPV (22) and decoy (80), solve (80/22)^x = 4 → **x ≈ 1.07**.
In other words, *given the game's compressed signature scale*, radar range should
scale **roughly linearly** with the signature number. This looks physically wrong
and is defensible only because the scale is already a fourth-root-ish compression
of RCS. Document it loudly if you go this way, or the next person to read the
code will "fix" it back.

### 2.6 Why reflector decoys work, and what multiplier they deserve

A Gerbera-class decoy is plywood and foam — intrinsically a very poor radar
target, worse than the Shahed it accompanies. It carries a **Luneburg lens**:
reported in Ukraine as a 3D-printed plastic sphere wrapped in metal foil, whose
sole job is to raise the airframe's radar return so it looks like a Shahed or a
cruise missile on an air-defence screen **[Med, multiple consistent OSINT and
Defense Express / CEPA reporting]**. The same principle is used in reverse on
stealth aircraft, which fit Luneburg reflectors so ATC can see them in peacetime
**[High]**, and in the ADM-160 MALD **[Med]**.

**How big is the effect?** I could not find a published X-band RCS figure for the
specific lenses recovered in Ukraine, so this is **[Inference]** from geometry:

- A trihedral corner reflector of edge *a* has RCS_max = 4πa⁴/3λ². At X-band
  (λ = 3 cm), a 25 cm reflector gives **18 m² (+12.6 dBsm)**; a 30 cm reflector
  gives **38 m² (+15.8 dBsm)**.
- A Luneburg lens of similar diameter achieves comparable peak RCS but holds it
  over a **much wider cone of aspect angles**, which is the operational reason
  decoys use lenses rather than corner reflectors — the decoy tumbles and turns
  and must look big from everywhere.
- A foam-and-plywood decoy airframe is perhaps **−25 dBsm** bare.

So the lens is worth roughly **+35 to +40 dB** over the bare airframe and roughly
**+18 to +22 dB over a real Shahed**. Fourth root: **10^(38/40) ≈ 8.7×** the
detection range of the bare airframe, or about **3–4×** a Shahed's.

**Verdict on the game's decoy.** 80 versus 60 for the Shahed on a linear scale
(1.08× range after the fourth root) is not merely conservative, it is a design
bug: it removes the decoy's reason to exist. On the Fix A scale the decoy should
carry `radar_sig ≈ 90` against the Shahed's `≈ 48`, giving it **~3.3× the
detection range** and — more importantly for gameplay — making it the thing the
radar sees *first*, from the furthest away, with the strongest return. That is
exactly the tactical effect decoys have: they arrive at the front of the wave and
soak the engagement budget.

### 2.7 Can radar see ground targets?

**Yes, and the game's `—` for the tank is wrong.**

- Modern C-UAS radars are explicitly dual-domain. Echodyne's EchoGuard is
  described as providing detection, tracking and classification of *airspace and
  ground-based objects*; EchoShield claims 4D awareness across air, ground and
  maritime, with ML classification into **human, vehicle, fixed-wing and
  multi-rotor** classes **[Med, vendor]**.
- The O.W.L. 3D radar's spec sheet gives **12 km on vehicles** alongside 5 km on
  a Phantom 4 **[Med, vendor]**.
- Dedicated ground surveillance radars are a mature, separate category —
  man-portable systems like the PGSR-3i "Beagle" (fielded in 8–10 NATO and
  partner nations) and medium-range pulse-Doppler sets reaching **27 km** on
  ground movers **[Med]**.

**But three real constraints apply**, and they make good rules:

1. **Doppler gate.** Ground surveillance radar detects *moving* targets. A
   stationary vehicle disappears into clutter. In game: a tank should have a
   substantial radar signature **only while moving**, and near-zero while static.
2. **Line of sight.** Ground targets are masked by terrain far more aggressively
   than air targets. Any terrain occlusion should be absolute on this channel.
3. **Mode exclusivity.** A radar running a ground search sector is not running
   the same sector for air. In game: either give the radar an explicit mode
   toggle, or apply a ~0.5 multiplier to air reach when ground detection is
   enabled. The toggle is the more interesting choice.

---


## 2A. Radar cross-section: the full spread

*(§2 above reports what is published. §2A and §2B are the granular and physical
layer underneath it: full spreads with named endpoints, then implementable
propagation. Where §2.3 offers a coarse three-band altitude multiplier and §2B.3–
2B.5 offer the underlying Doppler/multipath/masking model, **implement §2B, not
both** — the coarse rule is there only as a fallback.)*

RCS is a distribution, not a number. Each row below gives the range worth
modelling, at **X-band (8–12 GHz) and Ku/K-band (12–26 GHz)** — the bands every
serious counter-drone radar actually uses. Where a row is measured it says so;
where it is scaled from geometry or from adjacent measurements it is marked
**[Inference]**.

| Target | Min (m² / dBsm) | Typical (m² / dBsm) | Max (m² / dBsm) | Basis |
|---|---|---|---|---|
| **Small bird** (starling, ~75 g) | 0.0001 / −40 | 0.001 / −30 | 0.003 / −25 | Standard avian RCS ballpark **[Med]** |
| **Racing / FPV quad** (5–7", carbon frame) | 0.001 / −30 | 0.008 / −21 | 0.03 / −15 | Scaled below Phantom measurements **[Inference]** |
| **Consumer quad** (Phantom / Mavic) | 0.005 / −23 | 0.02 / −17 | 0.35 / −4.6 | **Measured**: −20 dBsm @ 10 GHz, −13 dBsm @ 24 GHz; Swerling-1 mean range 0.01–0.35 m² **[High]** |
| **Large bird** (goose, swan) | 0.01 / −20 | 0.05 / −13 | 0.2 / −7 | K/W-band in-flight bird measurements **[Med]** |
| **Heavy multirotor** (hex/octo, ~1.5 m) | 0.03 / −15 | 0.15 / −8 | 0.6 / −2 | Scaled by physical area from Phantom **[Inference]** |
| **Small fixed-wing UAV** (~2 m span) | 0.1 / −10 | 0.2 / −7 | 0.5 / −3 | **Measured**: −9.8…−5.3 dBsm @ 2.75 GHz; −7.8…−5.0 @ 4.51 GHz (Björklund 2024) **[High]** |
| **Shahed-class** (2.5 m delta, composite + piston engine) | 0.05 / −13 | 0.25 / −6 | 1.5 / +1.8 | **No published measurement.** Inferred from size, materials reporting, and 5–15 km engagement ranges **[Inference]** |
| **Cruise missile** (Tomahawk ALCM class) | — | **<0.05 / −13** | 0.5 / −3 | GlobalSecurity RCS reference **[Med]** |
| **Shahed + Luneburg lens / corner reflector** | 10 / +10 | **32 / +15** | 100 / +20 | Trihedral geometry 4πa⁴/3λ²: 0.25 m → 18 m²; 0.30 m → 38 m² **[Inference from standard formula]** |
| **Human, walking** | 0.5 / −3 | 1 / 0 | 2 / +3 | Standard ballpark **[Med]** |
| **Helicopter** | 3 / +5 | 20 / +13 | 100 / +20 | Rotor blade flash dominates; widely quoted band **[Low]** |
| **Vehicle / MBT** | 10 / +10 | 50 / +17 | 200 / +23 | Standard ballpark; C-UAS vendor claims 12 km on vehicles vs 5 km on Phantom 4 implies ~+30 dB **[Inference]** |

**The endpoints that matter.** The smallest thing worth modelling is a **racing
quad at 0.001 m² (−30 dBsm)**. The largest air target is a **reflector-equipped
decoy at ~32 m² (+15 dBsm)**. That is **45 dB — a factor of 32,000 in RCS**, and
under the fourth-root law a **13:1 spread in detection range**. Any signature
scale that cannot represent 45 dB is not representing the problem.

**The overlap that defines the whole engineering problem.** A **large bird at
−13 dBsm** sits *above* a consumer quadcopter (−17) and within **7 dB of a
Shahed** (−6). Amplitude alone cannot separate them. This is not a marginal
case — it is the central case, and it is why micro-Doppler exists.

### Aspect-angle variation

Measured RCS of small drones swings by **15–25 dB** across azimuth, with specular
flashes at broadside from the motor arms, battery and body flats **[High — this
is the direct implication of the Phantom Swerling-1 range 0.01–0.35 m², a 15.4 dB
spread, and is consistent across the compact-range and in-flight studies]**.

Recommended implementable aspect model **[Inference]**:

| Aspect | RCS modifier |
|---|---|
| Nose-on / tail-on | **−5 dB** |
| Quartering (45°) | 0 dB |
| Broadside | **+7 dB** |

For a **Shahed-class delta**, nose-on is the aspect the airframe is shaped for;
widen it to **−8 dB nose-on, +9 dB broadside [Inference]**. For a **Luneburg
lens decoy** the whole point is aspect-independence: apply **0 dB modifier at all
aspects**, which is itself a tactical advantage worth modelling.

This creates a genuinely interesting tension with the Doppler rules below: a
**crossing** target is 12 dB brighter than a **closing** one, but has near-zero
radial velocity and so falls into the clutter notch. Bright and invisible at the
same time. That is real, and it is good gameplay.

---

## 2B. Radar propagation: the implementable physics

Everything here is standard radar engineering. Where I quote a coefficient from
memory of a standard reference (Skolnik's *Radar Handbook*, Barton's *Radar
Equations for Modern Radar*, Nathanson's clutter tables, ITU-R P.676 and P.838)
rather than from a page I could open, it is marked **[Recalled — verify]**. My
web-search budget was exhausted before I could re-confirm these; they are
textbook values, not guesses, but they should be checked before shipping.

### 2B.1 The range equation, in implementable form

Use the **energy form**, which handles FMCW and pulse-Doppler alike:

```
R_max^4  =  (P_avg · T_dwell · G_t · G_r · λ² · σ)
            ───────────────────────────────────────────────
            (4π)³ · k · T0 · F · (S/N)_req · L_total
```

- `P_avg` — average transmit power (W)
- `T_dwell` — coherent dwell on target (s)
- `G_t`, `G_r` — antenna gains (linear, not dB)
- `λ` — wavelength (m); X-band 9.4 GHz → 0.032 m, Ku 16 GHz → 0.019 m
- `σ` — target RCS (m²)
- `k·T0` — 1.38×10⁻²³ × 290 = 4.0×10⁻²¹
- `F` — receiver noise figure (linear)
- `(S/N)_req` — required SNR (linear)
- `L_total` — all losses (linear)

**Realistic coefficients for a counter-drone radar [Med, vendor-class values]:**

| Term | Value | dB | Note |
|---|---|---|---|
| `P_avg` | 10 W | +10 dBW | Solid-state FMCW; peak 50–100 W |
| `T_dwell` | 0.1 s | — | Long dwell is what buys micro-Doppler |
| `G_t = G_r` | 3162 (35 dBi) | +35 | ~2° pencil beam |
| `λ` | 0.032 m | — | X-band 9.4 GHz |
| `F` | 2.51 | 4 dB | Good LNA |
| `(S/N)_req` | 20 | 13 dB | Pd 0.8, Pfa 10⁻⁶, Swerling 1 with integration **[Recalled — verify]** |
| `L_total` | 6.3 | 8 dB | Scan, beamshape, radome, processing |

**Worked anchor.** Substituting the above with σ = 0.01 m² (a Phantom):

```
numerator   = 10 × 0.1 × 1.0e7 × 1.024e-3 × 0.01 = 102.4
denominator = 1984 × 4.0e-21 × 2.51 × 20 × 6.3   = 2.51e-15
R^4 = 4.08e16  →  R = 14,200 m
```

**Free space says 14.2 km. Vendors deliver 3–5 km.** That gap is the most useful
number in this document. To land at 4 km you need R to drop by 3.55×, which is
R⁴ down by 159×, which is **≈22 dB of additional loss** not present in the
free-space equation. **[Inference — my arithmetic, but the inputs and the vendor
outputs are both sourced.]**

**Design rule: add an explicit `L_environment` term of 20 dB** and stop
pretending the free-space equation is the answer. That 20 dB is clutter residue,
multipath fading, the raised detection threshold needed to survive clutter false
alarms, and non-ideal aspect. Every real system pays it.

**Fourth-root confirmation.** `R ∝ σ^(1/4)` falls straight out of the equation
and is confirmed explicitly in the sources: halving RCS gives 0.84× range; a 16×
RCS reduction halves the range **[High]**.

### 2B.2 Clutter and sub-clutter visibility

A ground-based radar looking at a low target competes with the ground itself.

**Clutter RCS in the resolution cell:**

```
σ_clutter = σ0 × A_cell
A_cell    = R × θ_az × ΔR          (ΔR = range resolution, m; θ_az in radians)
```

**Surface reflectivity σ0 at X-band, low grazing angle (1–3°) [Recalled from
standard clutter tables — verify against Nathanson]:**

| Terrain | σ0 (dB) |
|---|---|
| Calm water | −50 to −40 |
| Flat farmland / desert | −30 to −25 |
| Rural, scrub, wooded | −20 to −15 |
| Urban / built-up | −10 to −5 |

**Worked example.** R = 3000 m, θ_az = 2° (0.035 rad), ΔR = 5 m, rural σ0 = −20 dB:

```
A_cell = 3000 × 0.035 × 5 = 525 m²
σ_clutter = 0.01 × 525 = 5.25 m²
Target (FPV quad) = 0.008 m²
Signal-to-clutter ratio = −28 dB
```

So before you can even begin, Doppler processing must recover **28 dB** — and
then deliver your 13 dB of detection SNR on top. Required **sub-clutter
visibility ≈ 40 dB**. Typical figures **[Recalled — verify]**: simple MTI gives
**25–35 dB**; coherent pulse-Doppler with a long dwell gives **40–60 dB**. It is
achievable, and it is marginal, which is exactly why performance is so
environment-sensitive.

**Terrain multiplier for the game, derived from the σ0 table [Inference]:**

| Terrain under the target | Clutter σ0 | Radar reach × | Edge reliability × |
|---|---|---|---|
| Over water / open sky | −45 dB | **1.20** | 1.00 |
| Open field, snow, desert | −28 dB | **1.00** | 0.90 |
| Rural, scrub, treelines | −18 dB | **0.75** | 0.70 |
| Forest | −15 dB | **0.65** | 0.60 |
| Urban / built-up | −8 dB | **0.45** | 0.40 |

(The reach multipliers are the fourth root of the clutter power ratio, floored to
account for Doppler processing recovering most but not all of the difference.)

### 2B.3 Why "low and slow" is the hard case — the Doppler notch

This is the single most important mechanic in the radar model, and the game does
not have it.

Ground clutter sits at **zero Doppler**, spread by wind-blown vegetation. Typical
clutter spectral width **σ_v ≈ 0.1–1.0 m/s [Recalled — verify]**.

Doppler frequency: `f_d = 2·v_radial / λ`. At X-band (λ = 0.032 m) that is
**62.5 Hz per m/s** of radial velocity.

- Clutter, ±1 m/s → **±63 Hz**
- Hovering drone, v_radial = 0 → **0 Hz — buried in clutter**
- Drone crossing tangentially at 20 m/s, v_radial ≈ 0 → **buried in clutter**
- Drone closing at 15 m/s → **940 Hz — clean**
- Shahed closing at 55 m/s → **3,440 Hz — very clean**

**Implementable rule.** Define a **radial-velocity gate**:

| |v_radial| | Detection |
|---|---|
| < 1.5 m/s | **Not detected** (in the notch) |
| 1.5–4 m/s | Reach × 0.4, reliability × 0.3 |
| 4–10 m/s | Reach × 0.8, reliability × 0.7 |
| > 10 m/s | Full |

This single rule reproduces, from first principles, everything the sources say
qualitatively about low-and-slow being hard, and it makes **flight behaviour**
matter: a drone hovering behind a treeline is invisible; the same drone running
in on an attack is not. It also explains the Shahed's exposure — it is slow by
aircraft standards but it closes fast enough to sit far clear of the notch, which
is why radar handles Shaheds far better than FPVs.

### 2B.4 Multipath over the ground

Over a reflecting surface, direct and ground-reflected rays interfere. The
pattern-propagation factor multiplies the received power by

```
F⁴ = 16 · sin⁴( 2π · h_radar · h_target / (λ · R) )
```

At long range and low altitude the sine argument goes small and `F⁴ ≈ (4π·h_r·h_t
/ λR)⁴` — meaning **received power falls as R⁻⁸, not R⁻⁴**. Detection range
collapses. **[Standard two-ray model.]**

The transition happens at the first lobe maximum:

```
R_transition = 4 · h_radar · h_target / λ
```

For a **10 m mast at X-band (λ = 0.032 m)**:

| Target altitude | R_transition | Consequence |
|---|---|---|
| 100 m | 125 km | Never binding |
| 30 m | 37.5 km | Never binding at C-UAS ranges |
| 10 m | 12.5 km | Marginal |
| 5 m | 6.3 km | **Binding** |
| 3 m | 3.8 km | **Severe** |

**Implementable rule:** beyond `R_transition`, switch the range exponent from
R⁻⁴ to R⁻⁸ — i.e. detection range beyond that point scales as **σ^(1/8)**, and
effectively stops extending. Below ~10 m target altitude, a ground-based radar
simply does not reach. This is a cleaner and better-founded mechanic than a flat
altitude multiplier, and it is the physical reason nap-of-earth flight works.

### 2B.5 Terrain masking and the radar horizon

**Radar horizon** (4/3-earth, standard):

```
d_horizon(km) = 4.12 · ( √h_radar(m) + √h_target(m) )
```

| Mast | Target alt | Horizon |
|---|---|---|
| 10 m | 3 m | 20.2 km |
| 10 m | 30 m | 35.6 km |
| 10 m | 100 m | 54.2 km |
| 30 m | 3 m | 29.7 km |

**Verdict: the geometric horizon is essentially never the binding constraint at
counter-drone ranges (<20 km).** Anyone modelling low-altitude radar gaps with a
horizon formula is modelling the wrong thing. **Terrain masking is the real
constraint**, and it is local geometry:

```
masked if:  h_target < h_radar + (h_obstacle − h_radar) × (R_target / R_obstacle)
```

**Worked example:** 10 m mast, a 15 m treeline at 1 km. The masking ray rises at
atan(5/1000) = 0.29°. At 5 km that ray is at **35 m**; at 10 km, **60 m**.
Everything below those heights at those ranges is invisible. A single treeline
1 km away costs you the entire sub-35 m volume out to 5 km.

**Implementable rule:** trace the ray from the sensor over the highest
intervening terrain sample; anything below that ray is **not detected at all**
(binary, not a multiplier). Masking should be absolute — this is optics, not
attenuation.

### 2B.6 Atmospheric attenuation and rain

Gaseous absorption, **one-way, sea level, clear air** (ITU-R P.676 shape)
**[Recalled — verify against ITU-R P.676]**:

| Band | Freq | dB/km one-way | Two-way over 10 km |
|---|---|---|---|
| S | 3 GHz | ~0.007 | 0.14 dB — negligible |
| **X** | **10 GHz** | **~0.01** | **0.2 dB — negligible** |
| **Ku** | **16 GHz** | **~0.03** | **0.6 dB** |
| K | 24 GHz | ~0.2 (near the 22.2 GHz water line) | **4 dB** |
| Ka | 35 GHz | ~0.1 | 2 dB |
| W | 94 GHz | ~0.4–0.5 | 9 dB |

Rain, specific attenuation γ = k·R^α, **one-way** (ITU-R P.838 form)
**[Recalled — verify against ITU-R P.838]**:

| Band | 5 mm/h (moderate) | 10 mm/h (heavy) | 25 mm/h (downpour) |
|---|---|---|---|
| X (10 GHz) | ~0.1 dB/km | ~0.2 dB/km | ~0.6 dB/km |
| Ku (16 GHz) | ~0.35 dB/km | ~0.7 dB/km | ~2.0 dB/km |
| K (24 GHz) | ~0.7 dB/km | ~1.5 dB/km | ~4.0 dB/km |

**Worked example, 5 km path, heavy rain (10 mm/h), two-way:**

| Band | Loss | Range multiplier (10^(−dB/40)) |
|---|---|---|
| X | 2.0 dB | **0.89** |
| Ku | 7.0 dB | **0.67** |
| K | 15 dB | **0.42** |

Plus **rain volume clutter**, which can swamp a −20 dBsm target outright; Doppler
and polarisation help but do not eliminate it **[Med]**.

**Implementable weather multipliers [Inference from the above]:**

| Weather | Radar reach × | Reliability × |
|---|---|---|
| Clear | 1.00 | 1.00 |
| Light rain / fog | 0.92 | 0.90 |
| Heavy rain | **0.70** | **0.65** |
| Downpour / hail | **0.50** | **0.40** |

Note this is a much *smaller* penalty than optical or thermal would take in the
same conditions — "radar sees through weather" is broadly true at X-band, and
that difference is worth preserving as radar's distinguishing virtue.

### 2B.7 Micro-Doppler: what it actually buys

**The physics.** A propeller blade tip has its own velocity. A 5-inch FPV prop
(0.127 m diameter) at 20,000 RPM has ω = 2,094 rad/s and a **tip speed of 133
m/s**, producing micro-Doppler sidebands at 2×133/0.032 = **±8.3 kHz** at
X-band. Blade flash rate for a 2-blade prop = 2 × 20000/60 = **667 Hz**
**[Inference — my arithmetic from standard rotor kinematics]**.

A bird's wingtip moves at perhaps 10 m/s and flaps at **4–10 Hz** **[Med, vendor
technical]**, giving sidebands of **±625 Hz** with an irregular, non-periodic
envelope. Drone propeller modulation is typically **50–100 Hz** in the reported
figures for larger multirotors **[Med]**.

**Receiver requirements this imposes, and the cost:**
- PRF must exceed **2 × max micro-Doppler ≈ 17 kHz** at X-band to avoid aliasing
  the blade lines.
- Coherent dwell must span several blade-flash periods: **≥ 20–100 ms**.
- That dwell is time not spent scanning elsewhere. **Micro-Doppler classification
  costs scan rate.** A radar that classifies well updates slowly.

**False alarm rate against birds, with and without:**

| Configuration | Bird-as-drone false alarm rate | Source |
|---|---|---|
| Amplitude + kinematics only (no micro-Doppler) | **>35%** in bird-active areas; up to ~1,000 candidate targets/hour in complex environments | Trade industry **[Low — no methodology]** |
| Micro-Doppler + CNN, curated dataset | **1.23%** (series network) / **0%** (GoogLeNet) on 324 bird images | IET RSN 2020 **[High]** |
| Micro-Doppler + CNN, general validation | ~90%+ accuracy; 96–97% for hovering targets | IET RSN 2020 / Frontiers 2021 **[High]** |
| Micro-Doppler, realistic field estimate | **~5%** | **[Inference — splitting the difference]** |

**Implementable rule.** Give the radar a `has_micro_doppler` flag:

| | False-track rate beyond 60% of reach | Scan/update penalty |
|---|---|---|
| Without | **30%** | none |
| With | **5%** | update interval ×2 |

That is a real trade-off with a real basis, and it gives you a meaningful radar
upgrade path.

---

## 3. PASSIVE RF / ESM

### 3.1 Detection ranges that are actually specified

| System | What it detects | Range (real) | Map m | Confidence |
|---|---|---|---|---|
| DroneShield RfPatrol (body-worn) | drone + controller links | **up to 1 km LoS** | 83 | Vendor spec **[Med]** |
| Dedrone RF-160 | drone links | **1.6 km average, 5 km max** | 133 / 415 | Vendor spec **[Med]** |
| Dedrone RF-360 | drone links + DF | **up to 5 km** | 415 | Vendor spec **[Med]** |
| Generic wideband C-UAS RF (70 MHz–6 GHz) | 433/900 MHz, 2.4/3.5/5.8 GHz | **1.5 km with DF** | 125 | Patent literature **[Med]** |
| DJI AeroScope, mobile | DJI OcuSync telemetry | **~5 km** | 415 | Vendor/reseller **[Med]** |
| DJI AeroScope, stationary | DJI OcuSync telemetry | **7–50 km** by model | 580–4160 | Vendor/reseller **[Med]** |
| DJI AeroScope, operator telemetry | drone **and pilot** position | **up to 20 km** | 1660 | Vendor/reseller **[Med]** |
| Ku-band satcom terminal detector | Starlink user-terminal uplink 14.0–14.5 GHz | **≥30 km**, 1° DF, ≥90% Pd | 2500 | **Vendor marketing — [Low]** |
| Russian claim vs Starlink terminals | terminal geolocation | **10 km, 50–60 m accuracy** | 830 | Claim, reported **[Low]** |
| SeekLevel (UAV-mounted, Ukrainian) | enemy EW emitters, 1500–6000 MHz | not specified | — | Trade press **[Med]** |

The **average-versus-maximum gap** in the Dedrone figures (1.6 km average, 5 km
max) is the most designer-relevant number here. Vendor "maximum" ranges are clean
line-of-sight, clean spectrum, cooperative emitter. Median real performance is
roughly **a third** of the headline. Apply that discount to every vendor figure
in this document.

**Note:** DJI discontinued AeroScope in 2023; it remains the best-documented
data point for what a cooperative-protocol RF sensor can do, and its ranges
should be treated as an upper bound achieved against a *known, unencrypted,
identity-broadcasting* protocol — not as typical.

### 3.2 What is detectable

| Emission type | Detectable? | Notes | Conf. |
|---|---|---|---|
| **Analogue video (5.8 GHz VTX)** | **Trivially.** Continuous, wideband, unencrypted, high duty cycle. The easiest target in the whole spectrum. | Longest RF ranges of any drone link. | **[High]** |
| **Digital proprietary (DJI OcuSync 2.0)** | **Yes.** Dual-band 2.4/5.8 GHz. Critically, DJI identification signals **continue to broadcast out-of-band** even when the user forces the video downlink to one band. | This out-of-band leak is what AeroScope exploited. | **[High]** |
| **Frequency-hopping** | **Yes, but harder.** Wideband SDR front ends (70 MHz–6 GHz) and chirp-symbol detectors work without prior knowledge of centre frequency. Detection is easier than *classification*. | Costs you identification, not detection. | **[Med]** |
| **Military mesh / MANET** | **Partially.** Power-managed, spread-spectrum, low duty cycle. Detectable as energy; often unclassifiable, because RF libraries are signature-matched against known protocols. | Weakest documented area — I found no good open ranges. | **[Low/Inference]** |
| **LTE / cellular control** | **Very hard.** The drone's uplink is indistinguishable from any phone on the same tower; it is legitimate traffic in licensed spectrum. Defeats protocol-library sensors entirely. | Requires network-side cooperation, not an RF sensor. | **[Inference]** |
| **Satellite terminals (Starlink-class)** | **Yes, and at long range.** Ku-band uplink 14.0–14.5 GHz, eight 62.5 MHz channels, high EIRP. Dedicated detectors claim ≥30 km; Russian forces claim 10 km with 50–60 m accuracy followed by artillery. Using Starlink has been reported as painting a target on the user. | Beam is skyward — detection depends on sidelobe geometry. | **[Med]** |
| **Jammers** | **Yes, at very long range. This is the giveaway case.** See 3.6. | | **[High]** |
| **Radars** | **Yes, at several times the radar's own detection range.** See 3.6. | | **[High]** |

### 3.3 What is genuinely undetectable — confirmed

Two categories, and both are real. This is not a hedge.

**Fibre-optic controlled drones.** The drone is physically connected to the
operator by a hair-thin fibre carrying light. There is **no radio emission at
all** — so the drone is simultaneously immune to jamming and invisible to radio
reconnaissance. Reporting is unambiguous on both properties **[High, multiple
independent sources: Lowy Institute, Defense News, Interesting Engineering]**.

Ranges: **5–20 km typical**, with **10 km the Ukrainian standard through 2025**,
Russian fibre drones striking ~19 km behind the lines, and test flights
completing 20 km courses. Performance degrades beyond ~20 km as cables snap under
stress **[Med]**. By early 2025 Ukraine had roughly **15 manufacturers** of
fibre-optic drones producing thousands of units a month **[Med]**.

**One important 2026 caveat the game should model:** Ukrainian fibre drones now
**switch to radio when the cable snaps** **[Med, reported March 2026]**. So
"RF signature = 0" is correct for the nominal flight and wrong for the failure
case. A nice mechanic: a fibre drone that takes damage, or that exceeds its spool
length, flips to an FPV-class RF signature.

**Fully autonomous drones with no emissions.** Terminal autonomy / machine-vision
guidance removes the communications link entirely in the final phase, so there is
nothing to sever, spoof *or detect*. Ukrainian development dates from 2023;
by 2026 AI terminal-guidance is described as operational, using visual-inertial
odometry to fuse camera imagery with IMU readings onboard, with Ukraine's Defense
AI Center A1 (established March 2026) moving AI-assisted targeting into
operational status in GPS-denied environments **[Med, consistent across
GlobalSecurity, IEEE Spectrum, Defense Express, Forbes]**.

**Verdict on the game's `Fiber-optic quad: RF 0`.** Correct, well sourced, keep
it. It is one of the few numbers in the brief that is right for the right reason.
The corresponding gap is that the game has **no fully-autonomous unit class** —
by 2028 that is the more important zero.

### 3.4 Direction-finding accuracy, and whether RF can cue a weapon

**Accuracy.** An SDR-based DF system using the MUSIC algorithm against a DJI
Mavic Air reported average bearing errors of **1.15° static** and **1.86°
dynamic** **[High, peer-reviewed *Measurement* 2024]** — laboratory-grade,
single-target, good SNR. Commercial Ku-band satcom detectors claim **1°** **[Low,
marketing]**. Echodyne's *radar* claims better than 0.5° **[Med]** — worth noting
that radar angular accuracy is better than typical RF DF, not worse.

**Bearing error → position error.** At 1.5° bearing error, cross-range
uncertainty is ~26 m per km. At 5 km that is a **±130 m** ellipse *along the
cross-range axis only* — and **unbounded along the bearing** from a single
sensor.

**Can passive RF alone cue a weapon? No.**

- A single passive sensor produces a **line of bearing, not a position.** Range
  requires either two or more sensors crossing bearings (multilateration) or
  TDOA.
- **TDOA requires sub-nanosecond synchronisation**; small timing offsets produce
  metre-level errors, and where the propagation channel is multipath-dominated
  without a strong line-of-sight component, TDOA "cannot provide accurate
  results" **[High, arXiv 2025 / ResearchGate]**.
- Distributed DF networks fuse bearings from multiple nodes specifically to
  overcome terrain, buildings, reflections and limited LoS **[Med]**.
- C-UAS doctrine sources are explicit that **radar is the only sensor giving
  simultaneous range, speed, altitude and bearing in all weather**, and that
  positive identification is required before kinetic engagement **[Med]**.

**Design recommendation.** Passive RF should produce a **bearing-only contact**:
the target's *direction* is revealed but not its range, and weapons cannot engage
it. Two RF sensors with intersecting bearings, or one RF plus one other channel,
promote it to a full track. This is both accurate and genuinely good
gameplay — it makes sensor placement a spatial puzzle instead of a radius check.

### 3.5 Detecting the operator versus detecting the drone

These are different problems and the game does not distinguish them.

- AeroScope read the **drone's** telemetry, which *contained the pilot's GPS
  position* — an identity-broadcast protocol handing over the operator for free
  **[High]**. This is a property of civilian regulatory protocols and should not
  be assumed for military links.
- Generic RF sensors detect **controller** transmissions independently, and
  research on drone-controller localisation via TDOA treats the controller as the
  primary target rather than the drone **[Med]**.
- The operator is, from an RF standpoint, often the *better* target: ground-based,
  static, sometimes higher power, and — unlike the drone — worth killing
  permanently.
- Real precedent: Russian forces claim to geolocate Starlink terminals **at up to
  10 km with 50–60 m accuracy, then saturate the area with artillery** **[Low —
  a claim, but tactically coherent and consistent with Ukrainian reporting that
  using Starlink paints a target]**.

**Recommendation:** give drone-operating structures/units a separate **operator
RF signature**, detectable on the same channel, and let passive RF cue
counter-battery against the *launch site* rather than the drone. That is the
real, documented use of this sensor.

### 3.6 How jammers and radars give themselves away

**This is the strongest and best-sourced material in the whole document, and the
game currently ignores it.**

**The physics.** An ESM receiver detects the **direct** signal — one-way
propagation, R ∝ √P. A radar detects the **reflected** signal — two-way, R ∝
P^(1/4). The intercept range is therefore *always* much greater than the radar's
own detection range **[High]**. A standard worked example gives an emitter
detected at **120 nautical miles** by an ESM receiver against **20 nmi** of radar
detection range — a **6:1 range advantage factor** **[High, EW handbook /
radar references]**.

**The operational consequence in Ukraine.** "Electronic warfare systems must
radiate to work, and radiating in a contested electromagnetic environment invites
direction finding, geolocation and a strike." Open-source tracking shows roughly
**90 Russian jamming systems visually confirmed destroyed, damaged or captured
across 38 months of fighting** **[Med — OSINT tally, methodology not published,
but the direction of the finding is uncontested]**. Ukrainian DETECT GROUP fields
**SeekLevel**, a UAV-mounted two-antenna DF system operating 1500–6000 MHz that
shows bearings to enemy EW emitters in real time to the drone operator **[Med]**.
AI-assisted jam detection is reported identifying jamming type and source
location "within seconds," enabling kinetic response **[Low]**.

**Design recommendations — three concrete changes:**

1. **A transmitting jammer should be detectable across effectively the whole
   map** by any passive RF sensor, as a bearing-only contact. The game's
   `Jammer, transmitting: RF 100` is directionally right but the reach model
   (900 map m base, √ scaling → 900 m) understates it by an order of magnitude.
2. **A radiating radar should have a very high RF signature, and currently has
   none at all.** The Radar Mast has `RF —`. That is the clearest single omission
   in the signature table. Recommend `RF ≈ 90` while radiating, giving it an
   RF-detection range roughly **3–6× its own radar reach** — the range advantage
   factor, applied directly.
3. **Make emitting a choice.** If jammers and radars can be switched off, the
   player gets the actual dilemma of the modern battlefield: radiate and see, or
   stay silent and live. This is free design value sitting in the physics.

---


## 3A. Passive RF: the full spread in radiated power

*(Same relationship to §3 as §2A/2B have to §2: spreads, then physics, then
§3B assembles both channels into code.)*

Passive RF range is set by the **emitter's EIRP**, not by anything about the
target's size. Below, EIRP = transmit power + antenna gain, in dBm.

| Emitter | Tx power | Ant. gain | **EIRP (dBm)** | Range × vs 30 dBm ref | Conf. |
|---|---|---|---|---|---|
| **Weak digital video downlink** (EU-restricted DJI, 25 mW) | 14 dBm | 0 dBi | **14** | **0.40** | **[Med]** |
| Micro-FPV VTX (200 mW) | 23 | 2 | **25** | 0.75 | **[Inference]** |
| **FPV analogue VTX (600 mW)** — the reference case | 28 | 2 | **30** | **1.00** | **[Med]** |
| FPV analogue VTX (1.6 W) | 32 | 2 | **34** | 1.58 | **[Med]** |
| ELRS-class control link, 900 MHz, 1 W | 30 | 2 | **32** | 1.26 | **[Inference]** |
| DJI OcuSync airborne, 1 W | 30 | 0 | **30** | 1.00 | **[Med]** |
| **Ground controller, 1 W + patch** | 30 | 10 | **40** | **1.78** | **[Inference]** |
| Long-range fixed-wing datalink, 5 W | 37 | 6 | **43** | 2.24 | **[Inference]** |
| Military mesh / MANET node, 10 W | 40 | 6 | **46** | 2.51 (but see LPI penalty) | **[Inference]** |
| **Starlink UT, toward satellite** | 36 | 35 | **71** | 44.7 | **[Med]** |
| **Starlink UT, sidelobe toward ground** | — | — | **~41–46** | 1.9–2.5 | **[Inference]** |
| **Tactical jammer, 50 W omni** | 47 | 6 | **53** | **3.76** | **[Inference]** |
| **Large EW jammer, 1 kW** | 60 | 10 | **70** | **10.0** | **[Med]** |
| **Radar, mainbeam** (100 W peak, 35 dBi) | 50 | 35 | **85** | **23.7** | **[High — this is the ESM range-advantage case]** |
| Radar, sidelobes (35 dB down) | — | — | **~50** | 3.2 | **[Inference]** |
| **Fibre-optic drone** | — | — | **none** | **0** | **[High]** |
| **Autonomous drone, link off** | — | — | **none** | **0** | **[High]** |

**The endpoints.** From a 25 mW restricted video downlink (**14 dBm**) to a radar
mainbeam (**85 dBm**) is **71 dB — a factor of 12.6 million in power**, and under
the square-root law a **~60:1 spread in detection range**. Again: a 0–100 linear
scale cannot hold this. It must be logarithmic.

`range_multiplier = 10^((EIRP_dBm − 30) / 20)` — that is the square-root law
written in dB, and it is exact.

### 3A.1 The detection link budget

```
P_received(dBm) = EIRP(dBm) + G_rx(dBi) − FSPL(dB) − L_env(dB)
FSPL(dB)        = 32.44 + 20·log10(f_MHz) + 20·log10(d_km)
MDS(dBm)        = −174 + 10·log10(B_Hz) + NF(dB) + (S/N)_req(dB)
Detected when   P_received ≥ MDS
```

**Receiver sensitivity by architecture [Standard, Friis / thermal noise]:**

| Receiver type | Bandwidth | NF | SNR req | **MDS** |
|---|---|---|---|---|
| Wideband energy detector (survey) | 20 MHz | 6 dB | 10 dB | **−85 dBm** |
| Channelised receiver | 1 MHz | 6 dB | 10 dB | **−98 dBm** |
| Matched to known waveform | 10 kHz eff. | 6 dB | 3 dB | **−125 dBm** |

**Worked example — FPV analogue VTX at 5.8 GHz:**

```
EIRP 30 dBm, G_rx 10 dBi (directional panel), MDS −98 dBm (channelised)
Allowable FSPL = 30 + 10 + 98 = 138 dB
138 = 32.44 + 75.3 + 20·log10(d_km)
20·log10(d) = 30.3  →  d = 32.7 km  (free space)
```

**Free space says 33 km. Vendors deliver 1–5 km.** The gap is **~25–30 dB**
**[Inference — my arithmetic against sourced vendor ranges]**, and it is the
same lesson as the radar section: the textbook equation over-predicts badly.
The causes are specific and modellable:

| Loss source | Typical | Note |
|---|---|---|
| Drone antenna not pointed at sensor | **6–12 dB** | Dipole nulls are deep |
| Terrain / foliage / building obstruction | **10–25 dB** | Dominant term |
| Elevated noise floor in congested spectrum | **5–15 dB** | Urban 2.4/5.8 GHz is full of WiFi |
| Classification (not just detection) margin | **6–10 dB** | You must decode enough to identify |
| **Total `L_env`** | **25–35 dB** | |

**Implementable:** carry `L_env` as an explicit term, defaulting to **28 dB**,
reduced to 10 dB for clear line of sight over open ground and raised to 40 dB in
forest or urban.

**Resulting realistic ranges at `L_env` = 28 dB, channelised receiver:**

| Emitter | EIRP | Real detection range |
|---|---|---|
| Weak digital downlink | 14 dBm | **~0.5 km** |
| FPV analogue VTX 600 mW | 30 | **~1.3 km** |
| Ground controller + patch | 40 | **~4 km** |
| Fixed-wing datalink | 43 | **~6 km** |
| Tactical jammer 50 W | 53 | **~18 km** |
| Large EW jammer 1 kW | 70 | **~130 km** |
| Radar mainbeam | 85 | **~730 km** (horizon-limited long before this) |

These land squarely on the sourced vendor figures (RfPatrol 1 km, Dedrone 1.6 km
average / 5 km max, AeroScope 7–50 km against a cooperative protocol), which is
the best validation available that the model is calibrated correctly.

### 3A.2 Frequency hopping, LPI and burst transmission

These change *probability of intercept*, not just range, and the game should
model them as two separate effects.

**Frequency hopping.** A narrowband receiver has better sensitivity but must
search; a wideband receiver always sees the hop but pays a noise-bandwidth
penalty of `10·log10(B_wide / B_instantaneous)`.

Worked: a link hopping over **80 MHz** with **1 MHz** instantaneous bandwidth.
- Wideband 80 MHz receiver: **POI = 1.0**, sensitivity penalty **19 dB** →
  detection range **× 10^(−19/20) = 0.11**.
- Scanning 1 MHz receiver: full sensitivity, full range, but **POI ≈ 1/80 per
  dwell** → detection flickers.

**Implementable rule for hopping emitters:** reach × **0.4** *and* reliability
× **0.35**, i.e. detected at shorter range and only intermittently. (The 0.4
represents a practical receiver splitting the difference with a channelised
bank.) **[Inference — the 19 dB figure is exact arithmetic; the 0.4 is my
practical compromise.]**

**LPI / DSSS spread spectrum.** Processing gain `G_p = B_spread / B_data`. A
100× spread link sits **20 dB below the noise floor** to a non-cooperative
receiver. A radiometric detector can recover some of that by integrating, but
not all. Practical cost to an intercept receiver: **10–20 dB of effective
sensitivity → a factor of 3–10 in range** **[Inference — standard EW reasoning;
I found no published C-UAS-specific figure, and the sources confirm only that
mesh/MANET links are "detectable as energy, often unclassifiable"]**.

**Implementable:** LPI/mesh link → reach × **0.25**, and **classification fails**
(the contact shows as "unknown emitter", not "FPV drone"). That matches the
sourced observation that RF libraries are signature-matched against known
protocols and miss custom ones.

**Burst / low duty cycle.** A transmitter radiating **5% of the time** is
detectable 5% of the time by any receiver not staring at it. This is the
cheapest counter-detection measure that exists and the game should let units use
it.

**Implementable:** `reliability × duty_cycle_factor`, where a 5% burst emitter
gets **0.05–0.20** depending on receiver dwell. Applies naturally to Shahed
telemetry bursts and to disciplined operators.

### 3A.3 Direction finding: bearings, baselines and fixes

**Bearing accuracy [High where cited]:**

| Method | Accuracy (σ_θ) | Source |
|---|---|---|
| SDR + MUSIC, static target | **1.15°** | *Measurement* 2024, DJI Mavic Air **[High]** |
| SDR + MUSIC, dynamic target | **1.86°** | Same **[High]** |
| Commercial Ku satcom detector (claim) | 1.0° | Vendor marketing **[Low]** |
| Practical fielded C-UAS RF DF | **3–6°** | **[Inference — the lab figures are single-target, high-SNR]** |
| *(For comparison: radar angular accuracy)* | *<0.5°* | Echodyne **[Med]** |

**Cross-range error from a bearing:** `error ≈ σ_θ(rad) × R`.
At σ_θ = 1.5° (0.026 rad): **26 m per km** — so ±130 m at 5 km, in the
cross-range axis only, and **unbounded along the bearing**.

**How many baselines you need:**

| Sensors | What you get | Usable for |
|---|---|---|
| **1** | A **line of bearing**. No range at all. | Cueing another sensor. **Never a weapon.** |
| **2** | A **fix**, quality entirely dependent on crossing geometry | Weapon-quality *only* if the crossing angle is good |
| **3+** | Over-determined fix, GDOP manageable across a wide area | Yes |
| **3 (TDOA, 2D)** / **4 (TDOA, 3D)** | Hyperbolic fix; needs **sub-nanosecond sync**; fails in multipath without a dominant LoS component | Yes, where sync holds **[High]** |

**Fix error from two bearings:**

```
error ≈ σ_θ(rad) × R / sin(Δbearing)
```

| Crossing angle Δ | Error at R = 5 km, σ_θ = 1.5° |
|---|---|
| 90° | **131 m** |
| 45° | 185 m |
| 20° | **383 m** |
| 10° | 754 m |
| 5° | **1,500 m** |

**Implementable rule.** Passive RF produces a **bearing-only contact** by
default. Promote it to a **fix** when two or more RF sensors hold the same
emitter *and* `sin(Δbearing) > 0.35` (≈20° crossing). Weapons may engage only
fixes. This single rule turns passive RF from a radius check into a sensor
*placement* problem, which is the interesting version.

---

## 3B. Two functions, assembled

This is the deliverable the model needs. Coefficients are drawn from the sections
above; every one marked **[est]** is my estimate rather than a published value.

```python
def radar_detect(target, sensor, env):
    # 1. RCS in m², with aspect  [High for measured classes, Inference for aspect]
    sigma = target.rcs_mean_m2 * aspect_db_to_lin(
        {"nose": -5, "quarter": 0, "beam": +7}[target.aspect])   # [est]
    if target.has_reflector:
        sigma = target.rcs_mean_m2 * 4000          # +36 dB, aspect-independent [est]

    # 2. Hard gates — these are binary, not multipliers
    if terrain_masked(sensor, target):        return NONE      # [High]
    if abs(target.v_radial) < 1.5:            return NONE      # Doppler notch [High]
    if not line_of_sight(sensor, target):     return NONE

    # 3. Free-space fourth-root range
    R = REF_RANGE * (sigma / REF_RCS) ** 0.25                  # [High]

    # 4. Environment losses, in dB, applied as 10^(-dB/40) on range
    L  = 20.0                                                   # baseline env loss [est, derived]
    L += clutter_loss_db(env.terrain_under_target)              # 0..15 dB  [Med]
    L += rain_loss_db(env.rain_mm_hr, sensor.band, R)           # ITU-R P.838 [Recalled]
    L += gaseous_loss_db(sensor.band, R)                        # ITU-R P.676 [Recalled]
    R *= 10 ** (-L / 40.0)

    # 5. Multipath: beyond R_transition, range stops extending
    R_t = 4 * sensor.height_m * target.alt_m / sensor.wavelength_m   # [High]
    if R > R_t:
        R = R_t + (R - R_t) ** 0.5                              # R^-8 regime [est]

    # 6. Reliability
    p = sensor.base_reliability                                  # 0.85 high-alt, 0.70 mid
    p *= velocity_gate(target.v_radial)                          # 0.3 .. 1.0  [High]
    p *= clutter_reliability(env.terrain_under_target)           # 0.4 .. 1.0  [est]
    p *= weather_reliability(env)                                # 0.4 .. 1.0  [est]

    # 7. False tracks — the missing mechanic
    p_false = 0.05 if sensor.micro_doppler else 0.30             # [High / Low]

    return Detection(range=R, solid_within=0.6 * R,
                     reliability=p, false_track_rate=p_false)


def rf_detect(emitter, sensor, env):
    if emitter.eirp_dbm is None:      return NONE   # fibre-optic / autonomous [High]

    # 1. Link budget  [Standard]
    allowable_path_loss = emitter.eirp_dbm + sensor.gain_dbi - sensor.mds_dbm
    L_env = {"open": 10, "mixed": 28, "forest": 38, "urban": 40}[env.terrain]  # [est]
    allowable_path_loss -= L_env
    R_km = 10 ** ((allowable_path_loss - 32.44
                   - 20 * log10(emitter.freq_mhz)) / 20.0)       # [Standard]

    # 2. Waveform penalties
    if emitter.freq_hopping: R_km *= 0.40;  rel_mult = 0.35      # 19 dB arithmetic [High] / [est]
    elif emitter.lpi_dsss:   R_km *= 0.25;  rel_mult = 0.50      # [Inference]
    else:                                   rel_mult = 1.00
    rel_mult *= emitter.duty_cycle_factor                        # burst emitters [High]

    # 3. Reliability by protocol familiarity
    p = {"analogue_video": 0.95, "jammer": 0.95, "known_digital": 0.85,
         "mesh_lpi": 0.60, "cellular": 0.15}[emitter.protocol_class]   # [est]
    p *= rel_mult

    # 4. Bearing only, unless cross-fixed
    bearing = true_bearing(sensor, emitter) + gauss(0, 3.0)      # sigma 3 deg [Med]
    return RFContact(range_estimate=None,          # bearing only: NO range
                     bearing_deg=bearing, bearing_sigma_deg=3.0,
                     max_reach_m=R_km * 1000, reliability=p,
                     kind="BEARING_ONLY")

# Promotion to a weapon-quality fix requires a second baseline with good geometry:
def promote_to_fix(contacts):                       # [High — 2 bearings minimum]
    for a, b in pairs(contacts):
        if sin(radians(abs(a.bearing_deg - b.bearing_deg))) > 0.35:   # >20 deg
            return Fix(position=intersect(a, b),
                       error_m=radians(3.0) * range_to(a) /
                               sin(radians(abs(a.bearing_deg - b.bearing_deg))))
    return None                                     # still bearing-only
```

**Additional verdicts this produces, beyond §4:**

- **Radar's `√`/`⁴√` argument is settled**: fourth root on RCS in the free-space
  term, then a **separate additive dB loss budget** for environment. Do not try
  to fold clutter and rain into the signature number.
- **Passive RF's `√` is settled too**: `10^((EIRP−30)/20)`. Same structure —
  a clean physical term plus an additive dB environment loss.
- **The 2-second track hold in the current model is too short** given that the
  dominant real-world failure is *intermittent* detection in clutter and against
  hopping emitters. **[Inference]** Recommend **4–6 s** for radar, **8–10 s** for
  RF (an emitter that transmitted once will transmit again), which lets the
  intermittency mechanics above read as "flickering contact" rather than "sensor
  broken".

---

## 4. Verdicts on the game's specific numbers

| Game assumption | Verdict | Recommended value |
|---|---|---|
| **Radar Mast reach 1400 map m (≈16.8 km)** | **Wrong as a reference; good as a ceiling.** 16.8 km exceeds every fielded C-UAS radar against anything smaller than a jet, but real radars *do* have hard instrumented-range caps (IRIS: 5 km, 12 km with LRM). | Keep **1400 as an instrumented cap**. Set **reference reach 900 map m** (10.8 km) against a `radar_sig = 50` target. |
| **Interceptor Battery radar 800 map m (9.6 km)** | Reasonable for a shorter-range fire-control set. | Keep 800 as cap; reference **550**. |
| **Fourth-root radar scaling** | **Correct physics, wrong application.** Applied to a linear 0–100 scale it compresses the whole roster into a 1.38:1 band. | Keep `^(1/4)`, convert the signature column to a **log/dBsm scale** (§2.5 Fix A). If the scale cannot change, use exponent **≈1.0** (Fix B). |
| **78% radar edge reliability** | **Too generous for small low targets; ignores false tracks entirely.** | Target-dependent: **0.85** high-altitude / large, **0.70** medium, **0.35** nap-of-earth. Add a **15–25% false-track rate** beyond 60% of reach. |
| **Radar +20% vs high-altitude targets** | **Directionally right, far too weak.** Low-altitude clutter is the defining problem of this sensor. | **×1.25 / ×1.00 / ×0.40** three-band (§2.3). |
| **Tank radar signature `—`** | **Wrong.** C-UAS and ground surveillance radars both detect vehicles; one vendor spec gives 12 km on vehicles vs 5 km on a Phantom 4. | Give the tank a radar signature, **gated on movement**, with a ground/air mode trade-off. |
| **Decoy radar signature 80 vs Shahed 60** | **Wrong, and it breaks the unit's purpose.** Real delta is ~+20 dB, ~3–4× range. | `radar_sig ≈ 90` on the log scale vs Shahed's 48 → **~3.3× reach**. |
| **Passive RF reach 900 map m (10.8 km)** | **Wrong in both directions.** Far too long vs an FPV control link (real: 1–5 km); far too short vs jammers, radars, satcom terminals. | **Reference 600 map m** at `rf_sig = 50`; **no cap** (or a very high one) so high-power emitters genuinely reach across the map. |
| **Passive RF √(sig/100) scaling** | **Correct.** One-way propagation gives R ∝ √P. | Keep it. Make `rf_sig` a **log-power** scale (20 points = 10 dB) so it can span a jammer and a micro VTX. |
| **95% RF edge reliability** | **Right for jammers and analogue video; too high for everything else.** RF libraries are signature-matched and miss unknown, LPI and cellular protocols. | **95** analogue video / jammers; **85** known digital; **60** mesh/LPI; **n/a** fibre & autonomous. |
| **Fibre-optic quad RF = 0** | **Correct and well sourced.** | Keep. Add the **cable-break → RF reverts to FPV level** case (2026 reporting). |
| **Shahed / turbojet / decoy RF = 0** | **Increasingly wrong.** Recovered Gerbera and Geran variants have carried cellular modems and cameras. | **15–25** for Shahed/turbojet (intermittent), **25** for decoy. |
| **Radar Mast RF signature (absent)** | **Clear omission.** Radars are the classic ESM target; intercept range exceeds radar range by ~6:1. | **`rf_sig` 90 while radiating.** Add an emissions-control toggle. |
| **RF detection treated same as radar track** | **Wrong.** RF gives bearing, not range. | Passive RF yields a **bearing-only contact** that cannot cue a weapon until corroborated or cross-fixed. |

---

## 5. Recommended signature values

Two scales, both logarithmic, both with an explicit physical meaning.

- **`radar_sig = 2 × dBsm + 60`** → `reach = min(cap, ref × 10^((radar_sig − 50)/80))`
- **`rf_sig` = 20 points per 10 dB of radiated power, 50 = reference emitter**
  → `reach = ref_rf × 10^((rf_sig − 50)/40)`

| Unit class | RCS (m², est.) | dBsm | **radar_sig** | Radar × vs ref | **rf_sig** | RF × vs ref | Confidence |
|---|---|---|---|---|---|---|---|
| Small electric quad (FPV), analogue video | 0.008 | −21 | **18** | 0.40 | **46** | 0.79 | Radar **[High]**, RF **[Inference]** |
| Small electric quad, digital link | 0.008 | −21 | **18** | 0.40 | **44** | 0.72 | **[Med]** |
| **Fibre-optic quad** | 0.008 | −21 | **18** | 0.40 | **0** (→46 on cable break) | **0** | **[High]** |
| **Fully autonomous quad (new class)** | 0.008 | −21 | **18** | 0.40 | **0** | **0** | **[High]** |
| Electric heavy multirotor ("Baba Yaga") | 0.08 | −11 | **38** | 0.71 | **52** | 1.12 | **[Med]** |
| Fixed-wing recon | 0.2 | −7 | **46** | 0.89 | **58** | 1.58 | Radar **[High]** (Björklund), RF **[Inference]** |
| Combustion loitering munition (Lancet-class) | 0.06 | −12 | **36** | 0.67 | **50** | 1.00 | **[Inference]** |
| Combustion heavy strike (Shahed/Geran-2) | 0.25 | −6 | **48** | 0.94 | **20** | 0.18 | Radar **[Inference]**, RF **[Med]** |
| Turbojet strike drone | 0.5 | −3 | **54** | 1.12 | **20** | 0.18 | **[Inference]** |
| **Decoy drone with reflector** | **32** | **+15** | **90** | **3.16** (→cap) | **25** | 0.24 | **[Inference from geometry]** |
| Main battle tank, **moving** | 50 | +17 | **94** | 3.98 (→cap) | **0** | 0 | **[Med]** |
| Main battle tank, **stationary** | — | — | **0** | 0 | 0 | 0 | Doppler gate **[High]** |
| Jammer, transmitting | 20 | +13 | **86** | 2.51 | **95** | **17.8** | RF **[High]** |
| **Radar, radiating (new)** | 20 | +13 | **86** | 2.51 | **90** | **10.0** | **[High]** |
| Satcom terminal / Command Post | 15 | +12 | **84** | 2.37 | **80** | 5.62 | **[Med]** |
| Drone operator position (new) | — | — | **0** | 0 | **55** | 1.33 | **[Med]** |

**The decoy multiplier, stated plainly:** a reflector-equipped decoy should carry
roughly **+40 dB of RCS over the bare airframe** and **+20 dB over the real
Shahed it escorts** — a **~3.3× radar detection range multiplier** relative to
the Shahed, or ~8× relative to a foam airframe. In game terms, on the recommended
scale: **decoy 90 vs Shahed 48**. If the team wants one number to carry the
finding, it is that a decoy should be seen **three times further away** than the
weapon it is protecting.

---

## 6. What changes by 2028

**Radar gets better, but not enough to close the low-altitude gap. [Med]**
Metamaterial and low-SWaP electronically-scanned arrays are already commercial
(Echodyne's MESA); Ku-band FPV-specific radars are proliferating in Ukraine;
CD-T10/T15-class domestic radars reached 15–22 km in 2026. Expect FPV detection
ranges to roughly **double** by 2028 and clutter suppression to improve. Expect
the physics gap at nap-of-earth altitude to **narrow but not close** — it is
geometry, and the sources are explicit that no amount of processing eliminates it.

**Multistatic and passive radar starts to matter. [Med]** NATO STO has studied
DVB-T-illuminated passive radar for drone detection (illuminator at ~45 km from
receiver). A large and fast-moving 2025–26 literature is building 5G ISAC
UAV detection — using NR synchronisation signal blocks as always-available
illuminators, multistatic ISAC resource allocation, and Kalman fusion of TDOA-RF
with radar tracks. Two consequences for the game: **bistatic geometry partially
defeats nose-on RCS reduction** (you get lit from one angle and observed from
another), and **the sensor becomes a network, not a mast**. If KILL ZONE ever
wants a late-game radar upgrade, "networked passive radar, no emissions, needs
two structures" is the historically correct one.

**Fourth-root physics does not change.** Nothing on the horizon alters R ∝
σ^(1/4). Decoys therefore remain effective indefinitely, because a 30 dB signal
advantage cannot be processed away — the only counter is **kinematic and
behavioural classification** (decoys fly differently), which is a
classification problem, not a detection one. Expect radars to increasingly report
"detected but assessed as decoy." That is a great late-game mechanic and a
terrible one to give the player for free.

**Passive RF degrades as an anti-drone sensor. [High]** This is the clearest
trend in the whole document. Fibre-optic drone share is rising; terminal autonomy
is operational as of 2026; links are moving to mesh, LPI and cellular. By 2028
the assumption should invert: **a competent attacker's drones are RF-silent by
default**, and passive RF detection of the drone itself becomes the exception
rather than the rule.

**Passive RF strengthens as an anti-emitter sensor. [High]** Jammers, radars,
satcom terminals, command posts and operators all still have to radiate, and the
one-way/two-way asymmetry means they are seen first and from furthest. The
~90 destroyed Russian EW systems figure is the leading indicator.

**Net effect on the game by 2028:** passive RF should shift from being the most
reliable anti-drone channel (95%, 900 m) to being a **counter-EW and
counter-battery channel** with enormous reach against emitters and near-zero
reach against modern drones. Radar should shift from a uniform detector to a
sensor that is excellent against Shaheds and decoys, mediocre against FPVs, and
near-blind against anything hugging terrain.

---

## 7. What is genuinely uncertain

- **Shahed-136 RCS.** No credible open figure exists. My 0.1–0.5 m² estimate is
  inference from airframe size, materials reporting and observed engagement
  ranges. Anyone who finds a measured number should override it.
- **Luneburg lens RCS at X-band for the specific devices used in Ukraine.**
  Published lens measurements are at mm-wave. My +12 to +16 dBsm figures come
  from corner-reflector geometry, which is a reasonable proxy for peak RCS but
  not for the angular pattern.
- **Field false-alarm rates.** The "1,000 targets/hour, >35% false positive"
  figure is from a single trade source with no stated methodology. The lab
  figures (0–1.23%) are solid but not transferable.
- **Mesh/MANET and LPI link detectability.** I found no open range figures at all.
  Everything I have said about this category is inference.
- **RF signature values generally.** Unlike RCS, there is no measurement
  literature giving comparable EIRP figures across drone classes. The `rf_sig`
  column is **inference calibrated to reported detection ranges**, not measurement.
- **Every coefficient in the propagation sections marked [Recalled — verify].**
  Clutter reflectivity σ0 by terrain, ITU-R P.838 rain k/α values, ITU-R P.676
  gaseous attenuation, required SNR for Swerling-1 detection at a given Pd/Pfa,
  and MTI/pulse-Doppler improvement factors are all quoted from memory of the
  standard references in §8. They are textbook values and I am confident in them
  to within a few dB, but I could not re-open the sources within this session's
  search budget. Verify before shipping.
- **The two "reality gap" figures (22 dB radar, 28 dB RF)** are my arithmetic
  reconciling a free-space calculation against sourced vendor ranges. The inputs
  are sourced; the subtraction is mine. Treat the *structure* (an explicit
  additive environment loss) as solid and the *magnitude* as calibratable.
- **The Doppler notch threshold (1.5 m/s)** follows from a recalled clutter
  spectral width of 0.1–1.0 m/s. The mechanic is certainly real; the exact cut-off
  should be tuned for feel.
- **LPI/DSSS and mesh detection penalties** have no published C-UAS figure I could
  find. The 10–20 dB / factor-of-3–10 range cost is standard EW reasoning, not a
  measurement.
- **RF EIRP values for military and improvised emitters** (mesh nodes, tactical
  jammers, Shahed telemetry) are estimates from typical hardware classes, not from
  published specifications.
- **Aspect-angle modifiers** (−5 dB nose, +7 dB beam) are my fit to the measured
  15–25 dB total azimuth spread, not a published decomposition.

- **Post-May-2026 sources.** Several items cited (Ukrainian CD-T10/T15, Robin LRM
  in Ukraine, Defense AI Center A1, fibre-drone radio fallback) are trade press
  from 2026 that I could only reach through search extracts, not the pages
  themselves. Treat them as **[Med]** at best.

---

---

## 8. Sources

**Standard engineering references used for the propagation physics**
*(named rather than linked: these are books and ITU recommendations, and the
coefficients drawn from them are marked [Recalled — verify] in the text.)*
- M. I. Skolnik, *Radar Handbook*, 3rd ed. — range equation, clutter, MTI improvement factors
- D. K. Barton, *Radar Equations for Modern Radar* — detection SNR for Swerling cases, loss budgets
- F. E. Nathanson, *Radar Design Principles* — surface clutter reflectivity σ0 tables by terrain and grazing angle
- ITU-R P.838, *Specific attenuation model for rain for use in prediction methods* — https://www.itu.int/rec/R-REC-P.838/en
- ITU-R P.676, *Attenuation by atmospheric gases and related effects* — https://www.itu.int/rec/R-REC-P.676/en
- ITU-R P.525, *Calculation of free-space attenuation* (Friis / FSPL) — https://www.itu.int/rec/R-REC-P.525/en

**Radar cross-section — measurement**
- Compact-Range RCS Measurements and Modeling of Small Drones at 15 GHz and 25 GHz — https://ar5iv.labs.arxiv.org/html/1911.05926
- Björklund et al., *Statistical analysis of the radar cross section of two small fixed-wing drones using typical flights*, IET Radar Sonar Nav 2024 — https://ietresearch.onlinelibrary.wiley.com/doi/full/10.1049/rsn2.12522
- Rahman & Robertson, *In-flight RCS measurements of drones and birds at K-band and W-band*, IET RSN 2019 — https://ietresearch.onlinelibrary.wiley.com/doi/10.1049/iet-rsn.2018.5122
- Numerical and experimental RCS analysis of the quadrocopter DJI Phantom 2 — https://people.computing.clemson.edu/~jmarty/projects/lowLatencyNetworking/papers/EmergingApplicationSystems/Drones-UAVs/DetectingMaliciousDrones/NumericalAndExpRadarCrossSectionAnalysisOnDJIDrones.pdf
- *Small Fixed-Wing UAV RCS Signature Investigation…*, Drones (MDPI) 7(1):39 — https://www.mdpi.com/2504-446X/7/1/39
- RCS Measurements of UAVs and Their Statistical Analysis (Cranfield) — https://dspace.lib.cranfield.ac.uk/server/api/core/bitstreams/b70b94cb-9ed7-4067-a2ae-913fa1950b41/content
- Drone RCS measurements 26–40 GHz (IEEE DataPort) — https://ieee-dataport.org/open-access/drone-rcs-measurements-26-40-ghz
- RCS reference values (GlobalSecurity) — https://www.globalsecurity.org/military/world/stealth-aircraft-rcs.htm
- Li & Ling, *Radar Signatures of Small Consumer Drones* — https://users.ece.utexas.edu/~ling/SmallDroneISAR_Li_Ling.pdf

**Radar — range law, detection, clutter**
- Radar Sensor Detection Range (RFbeam) — https://rfbeam.ch/understanding-detection-range-of-radar-sensors/
- *Introduction to Drone Detection Radar with Emphasis on ATR* — https://arxiv.org/pdf/2307.10326
- Countering Low-Flying and One-Way Attack Drones by Reducing Ground Clutter… (COTS Journal) — https://www.cotsjournalonline.com/countering-low-flying-and-one-way-attack-drones-by-reducing-ground-clutter-reflections-and-radar-radio-multipath-fading/
- Capabilities, Limitations, and Operational Reality in Defense-Grade C-UAS — https://www.moneyprouav.com/capabilities-limitations-and-operational-reality-in-defense-grade-counter-uas-systems/
- False Positive Signal Reduction using AI in Counter-UAS — https://xray.greyb.com/drones/false-positive-counter-uas
- Counter-UAS Radar Detection (drone-warfare.com — *proxy-blocked, search extract only*) — https://drone-warfare.com/counter-uas/radar-detection/
- Low-Altitude C-UAS Detection & Countermeasures (UST, Jan 2026) — https://www.unmannedsystemstechnology.com/2026/01/low-altitude-counter-uas-detection-countermeasures-solutions/

**Radar — micro-Doppler and bird discrimination**
- Rahman & Robertson, *Radar micro-Doppler signatures of drones and birds at K-band and W-band*, Sci Rep 2018 — https://www.nature.com/articles/s41598-018-35880-9
- *Classification of drones and birds using CNNs applied to radar micro-Doppler spectrogram images*, IET RSN 2020 — https://ietresearch.onlinelibrary.wiley.com/doi/10.1049/iet-rsn.2019.0493
- How Does Micro-Doppler Radar Classify Drones? (Robin Radar) — https://www.robinradar.com/blog/how-micro-doppler-radar-works
- Multi-Frequency Radar Micro-Doppler Based Classification of Micro-Drone Payload Weight — https://www.frontiersin.org/journals/signal-processing/articles/10.3389/frsip.2021.781777/full

**Radar — systems and fielded performance**
- Echodyne EchoShield — https://www.echodyne.com/radar-systems/echoshield
- Echodyne EchoGuard — https://www.echodyne.com/radar-systems/echoguard
- Robin Radar IRIS — https://www.robinradar.com/products/iris-radar
- Robin Radar unveils Shahed detection upgrade for Ukraine (UK Defence Journal) — https://ukdefencejournal.org.uk/robin-radar-unveils-shahed-detection-upgrade-for-ukraine/
- DSEI 2025: Robin Radar deploys LRM module for IRIS (EDR Magazine) — https://www.edrmagazine.eu/dsei-2025-robin-radar-systems-deploys-the-lrm-module-for-iris-radar
- O.W.L. Counter-Drone (GA9000 3D radar) — https://www.owlknows.com/counter-drone/
- Ukraine's CD-T10: 15 km dome (Euromaidan Press, Mar 2026) — https://euromaidanpress.com/2026/03/06/one-radar-to-rule-them-all-ukraines-cd-t10-spots-everything-from-fpv-drones-to-guided-aerial-bombs-inside-a-15-km-dome/
- Ukraine's domestic Pelikan radar (United24) — https://united24media.com/defense-tech/inside-the-domestic-radar-giving-ukraine-up-to-250-miles-of-warning-against-russian-air-attacks-21863
- Ukraine's F-16s may now spot Shaheds that sneak under radar — https://euromaidanpress.com/2025/12/05/f-16s-sniper-pods/
- PGSR-3i 'Beagle' ground surveillance radar — https://pperadar.com/radars/pgsr-3i-beagle
- Ground surveillance radar reference (Radartutorial) — https://www.radartutorial.eu/19.kartei/05.perimeter/karte026.en.html

**Shahed / Geran and decoys**
- CSIS Missile Defense Project, Shahed-131 and -136 — https://missilethreat.csis.org/missile/shahed-131-and-136/
- RUSI, *Russia's Iranian-Made UAVs: A Technical Profile* — https://www.rusi.org/explore-our-research/publications/commentary/russias-iranian-made-uavs-technical-profile
- GlobalSecurity, Shahed-136 / Geran-2 — https://www.globalsecurity.org/military/world/iran/shahed-136.htm
- CEPA, *The Phony War: Ukraine and Russia's Decoy Drones* — https://cepa.org/article/the-phony-war-ukraine-and-russias-decoy-drones/
- Defense Express, *How and Why Russia Uses Luneburg Lenses in Drones* — https://en.defence-ua.com/weapon_and_tech/how_and_why_russia_uses_luneburg_lenses_in_drones_and_whether_the_armed_forces_of_ukraine_have_them-12265.html
- Gerbera (drone) — https://en.wikipedia.org/wiki/Gerbera_(drone)
- Espreso, *Weapons of mass deception: what are decoy drones* — https://global.espreso.tv/russia-ukraine-war-weapons-of-mass-deception-what-are-decoy-drones-and-how-russia-and-its-allies-are-trying-to-bypass-air-defense
- RCS Frequency Response Analysis of Luneberg Lens Reflectors — https://www.sciencedirect.com/science/article/pii/S187705092100867X/pdf
- Luneburg lenses making stealth aircraft visible (Aviation Geek Club) — https://theaviationgeekclub.com/these-devices-make-stealth-aircraft-visible-on-radar-screens/

**Passive RF — systems and ranges**
- Dedrone RF-360 — https://www.dedrone.com/sensors/rf-360
- Dedrone RF sensors overview — https://www.dedrone.com/products/drone-detection/rf-sensors/overview
- DroneShield dismounted products (RfPatrol) — https://www.droneshield.com/products-dismounted
- AeroDefense, *A Technical Perspective on the DJI AeroScope Drone Detection Solution* — https://blog.aerodefense.tech/dji-aeroscope
- Coptrz, *A Closer Look at DJI's AeroScope* — https://coptrz.com/blog/a-closer-look-at-djis-aeroscope/
- DJI AeroScope product page — https://www.dji.com/aeroscope
- Counter-UAS 101 – RF Drone Detection (drone-warfare.com — *proxy-blocked, extract only*) — https://drone-warfare.com/counter-uas/rf-detection/
- Dedrone, *Counter-Drone: The Comprehensive Guide to C-UAS* — https://www.dedrone.com/white-papers/counter-uas

**Passive RF — signals, DF and localisation**
- *Drone Detection and Tracking Using RF Identification Signals*, Sensors 23(17):7650 — https://www.mdpi.com/1424-8220/23/17/7650
- *Direction-finding for UAVs using radio frequency methods*, Measurement 2024 — https://www.sciencedirect.com/science/article/pii/S0263224124007681
- *Drone Controller Localization Based on TDoA* — https://www.arxiv.org/pdf/2510.02622
- *Multi-Stage Fusion Architecture for Small-Drone Localization and Identification Using Passive RF and EO Imagery* — https://arxiv.org/pdf/2406.16875
- HSToday, *Beyond Drone Detection: Distributed Direction Finding and the Future of Layered Counter-UAS* — https://www.hstoday.us/subject-matter-areas/unmanned-vehicles/beyond-drone-detection-distributed-direction-finding-and-the-future-of-layered-counter-uas/
- Narda, *How Authorities Use RF Direction Finding to Detect Drones* — https://www.narda-sts.com/en/newsblog/how-authorities-use-rf-direction-finding-to-detect-drones-a-practical-use-case/

**Fibre-optic and autonomous drones (the undetectable cases)**
- Lowy Institute, *Fibre-optic drones reshape Ukraine's technological war* — https://www.lowyinstitute.org/the-interpreter/fibre-optic-drones-reshape-ukraine-s-technological-war
- Defense News, *Of fiber-optics and FPVs – 6 questions with a Ukrainian drone trainer* (Nov 2025) — https://www.defensenews.com/global/europe/2025/11/07/of-fiber-optics-and-fpvs-6-questions-with-a-ukrainian-drone-trainer/
- DroneXL, *Ukraine's Fiber Optic FPV Drones Now Switch to Radio When the Cable Snaps* (Mar 2026) — https://dronexl.co/2026/03/23/ukraine-fiber-optic-fpv-drones-radio-cable/
- Interesting Engineering, *Fiber-optic drones help Ukraine avoid Russian jamming* — https://interestingengineering.com/military/ukraine-fiber-optic-drones-jammers
- GlobalSecurity, *Ukraine UAV – Autonomous Guidance: The Rise of the Machines* — https://www.globalsecurity.org/military/world/ukraine/uav-autonomy.htm
- IEEE Spectrum, *The Coming Drone-War Inflection in Ukraine* — https://spectrum.ieee.org/amp/autonomous-drone-warfare-2676377272
- Defense Express, *How Ukrainian FPV Drones With Automated Terminal Guidance Work* — https://en.defence-ua.com/weapon_and_tech/how_ukrainian_fpv_drones_with_automated_terminal_guidance_work-14687.html
- Forbes, *Fully Autonomous Drone Warfare Is Coming To Ukraine — And Iran* (Mar 2026) — https://www.forbes.com/sites/craigsmith/2026/03/26/fully-autonomous-drone-warfare-is-coming-to-ukraineand-iran/
- Terminal Autonomy AQ-400 Scythe — https://en.wikipedia.org/wiki/Terminal_Autonomy_AQ-400_Scythe

**ESM, emitter hunting, and the one-way/two-way asymmetry**
- RF Cafe / EW and Radar Systems Engineering Handbook, *One-Way Radar Equation / RF Propagation* — https://www.rfcafe.com/references/electrical/ew-radar-handbook/one-way-radar-equation.htm
- Radartutorial, *Detectability range* (ECCM) — https://www.radartutorial.eu/16.eccm/ja51.en.html
- EMSOPEDIA, *Electronic Support* — https://www.emsopedia.org/entries/electronic-support-aka-electromagnetic-support/
- RF Essentials, *Minimum Detectable Signal of an ESM Receiver and Intercept Range* — https://rfessentials.com/rf-knowledge-base/what-is-the-minimum-detectable-signal-of-an-esm-receiver-and-how-does-it-determi/
- Militarnyi, *DETECT GROUP Develops SeekLevel Direction Finder to Detect Enemy EW Systems* — https://militarnyi.com/en/news/detect-group-develops-seeklevel-direction-finder-to-detect-enemy-ew-systems/
- Janes, *Ukraine conflict: Ukraine's electronic warfare systems in focus* — https://www.janes.com/defence-intelligence-insights/defence-news/c4isr/ukraine-conflict-ukraines-electronic-warfare-systems-in-focus
- IEEE Spectrum, electronic warfare in Ukraine — https://spectrum.ieee.org/amp/electronic-warfare-ukraine-2671772127
- Defense One, *Using Starlink Paints a Target on Ukrainian Troops* — https://www.defenseone.com/threats/2023/03/using-starlink-paints-target-ukrainian-troops/384361/
- Technology.org, *New Russian tactic aims to hunt Starlink terminals* — https://www.technology.org/2022/12/20/new-russian-tactic-aims-to-hunt-starlink-terminals-does-it-really-work/
- KeepTrack, *Volna / Kupol / Garant Starlink jamming* — https://keeptrack.space/deep-dive/volna-kupol-garant-starlink-jamming
- SZMD SL105 Starlink uplink detector (**vendor marketing, low confidence**) — https://www.szmdjammer.com/szmdsl105-starlink-detection-system/

**Near-future: passive, multistatic and ISAC sensing**
- NATO STO, *Drone Detectability Feasibility Study using Passive Radars* — https://publications.sto.nato.int/publications/STO%20Meeting%20Proceedings/STO-MP-MSG-SET-183/MP-MSG-SET-183-13.pdf
- *Utilizing 5G NR SSB Blocks for Passive Detection and Localization of Low-Altitude Drones* — https://arxiv.org/html/2504.02641v2
- *Fusion of Cellular ISAC and Passive RF Sensing for UAV Detection and Tracking* — https://arxiv.org/pdf/2512.14608
- *Adaptive 5G Resource Allocation for Multistatic ISAC-Based UAV Detection and Tracking* — https://arxiv.org/html/2606.21677v1
- *A Survey on Detection, Classification, and Tracking of UAVs using Radar and Communications Systems* — https://arxiv.org/pdf/2402.05909
- *An Overview of Cellular ISAC for Low-Altitude UAV* — https://arxiv.org/pdf/2412.19973
