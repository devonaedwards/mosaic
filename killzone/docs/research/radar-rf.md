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
- **Post-May-2026 sources.** Several items cited (Ukrainian CD-T10/T15, Robin LRM
  in Ukraine, Defense AI Center A1, fibre-drone radio fallback) are trade press
  from 2026 that I could only reach through search extracts, not the pages
  themselves. Treat them as **[Med]** at best.

---

## 8. Sources

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
