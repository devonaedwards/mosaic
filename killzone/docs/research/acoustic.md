# Acoustic detection

Research for the KILL ZONE sensor and signature model. This is a video game
document: everything below exists to justify a number in a balance table.

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

---

## 4. Degradation: wind, noise, city

- **Wind.** Severe degradation above ~5 m/s is the figure that recurs. The
  mechanism is twofold: turbulent pressure fluctuations on the diaphragm (a
  noise-floor problem, fixable with windshields) and refraction (a propagation
  problem, not fixable at all). In the outdoor range tests, a RØDE NTG-2 with a
  WS6 windshield extended detection range **31–131%** over the bare microphone
  in strong wind, depending on azimuth, lowered the low-frequency noise floor by
  **2–3 dB**, and gained **1.8–4.4 dB** of wideband SNR. **[High]**
  Refraction produces an **upwind shadow zone**: sound rays bend upward into
  the wind and downward with it. A sensor downwind of a target hears it far
  further than one upwind — the azimuth dependence in the windshield result is
  partly this. I would model a 2:1 or worse downwind/upwind asymmetry.
  **[Medium — the effect is textbook, the ratio is my estimate]**
- **Ambient noise.** Detection accuracy falls from 99.88% clean to **77.76% at
  6 dB SNR**; classification accuracy in the same test fell to 25.27%, a 13.86
  point increase in classification error and a 16.3 point drop in detection.
  **[Medium — one study]** Note the asymmetry: noise degrades *identification*
  far faster than *detection*. A noisy sensor still knows something is up there;
  it stops knowing what.
- **Urban.** Reported urban accuracy ~81% with a **14.7% false-alarm rate**, and
  8–15% false positives in realistic environments generally. Airports, stadiums
  and dense traffic are described as environments where acoustic simply does not
  work. Multipath off buildings additionally corrupts time-difference-of-arrival,
  so localisation degrades faster than detection does. **[Medium]**
- **Battlefield.** Outgoing artillery, generators and vehicle engines are
  broadband, loud and continuous. There is published work specifically on
  "acoustic UAV detection in battlefield scenarios: handling noise, domain shift
  and weak labels", which tells you the problem is recognised and unsolved.
  **[Medium]**

---

## 5. Altitude, slant range and whether a high target is findable

Take a small quad: about 80 dBA at 1 m. Spherical spreading alone costs
20·log₁₀(r). At 2,000 m that is 66 dB, leaving 14 dB before absorption; the
kilohertz content then loses another 20–60 dB to the air. The result is far
below any plausible noise floor. **A small electric drone at 2 km is not
acoustically detectable, by anything, ever.** **[High — this is arithmetic]**

Now take a Shahed at 2 km. Reporting has Shaheds cruising at 2–2.8 km and diving
from above 2 km specifically to get above machine guns and small arms. Zvook
claims 3–5 km on this target class. So a Shahed at 2–3 km slant range sits
*inside* the claimed envelope — degraded, but not lost. **Acoustic remains a
usable early-warning channel against piston drones at their operating altitude,
and that is exactly what Sky Fortress is for.** **[Medium]**

Jet Shaheds at 7 km are the edge case. Running the same arithmetic with a
turbojet source level and low-frequency-weighted content, the signal arrives
somewhere in the tens of decibels — detectable on a quiet rural night, marginal
otherwise, and with a very short engagement window given 600 km/h. Ukrainian
reporting notes these fly above what most air defence can reach. **[Low — my
calculation, not a measurement]**

The general rule the game should encode: **altitude does not attenuate acoustic
detection by a fixed percentage; it separates targets into those whose spectrum
survives kilometres of air and those whose spectrum does not.**

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
