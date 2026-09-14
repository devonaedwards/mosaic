# Decoys and signature masking

Research input for the KILL ZONE detection model. Written for a designer who has
to pick a number and defend it. Everything here is drawn from open-source
reporting; where I extrapolate or reason from physics rather than from a source,
I say so.

Confidence markers used throughout:
**[H]** high — measured figure, vendor specification, or physics with a published
formula. **[M]** medium — consistent reporting from more than one credible
outlet, but no measurement. **[L]** low — single source, vendor claim, or an
outlet of uncertain provenance. **[X]** my own extrapolation, not a source.

---

## Summary

**The single biggest error in the current model is not a signature value — it is
the radar reach formula.** The game applies `reach ∝ (signature/100)^(1/4)` on
the correct physical basis that radar range goes as RCS^(1/4), but it feeds that
fourth root a *linear 0–100 index*. Real decoys beat real strike drones on radar
cross-section by factors of **100× to 1,000×** [H, physics]. On a linear 0–100
scale the largest ratio expressible is 100:1, and the game's actual decoy-to-
strike-drone ratio of 80:60 is 1.33:1 — which the fourth root then collapses into
a **7% detection-range advantage**. A decoy drone in the game today is
functionally not a decoy. Fixing the formula matters far more than retuning any
number, and the good news is that the existing signature values are close to
correct once you reinterpret the scale as decibels (Section 6).

Other findings:

- Air decoys are **cheap, crude and effective at the radar layer**. A Gerbera is
  foam plastic over a plywood-reinforced core, costed at $10,000 and possibly as
  low as $2,000 [M/L], carrying a 3D-printed foil-wrapped Luneburg lens or a
  corner reflector. Decoys have made up roughly **one third to one half of
  Shahed-type salvos** through 2025–26 [M].
- Decoys fool **radar reliably, image-matching seekers often, and human
  operators at close range almost never** [M]. The failure mode is consistent:
  reflectors inflate the *amplitude* of a return but not its *kinematics*,
  *acoustics* or *emissions*.
- The decoy economics are **inverting**. Against a $3.8M Patriot round a $10k
  decoy is dominant; against a $1,000–$5,000 interceptor drone it is a losing
  trade for the attacker [H on costs, M on the conclusion]. This is the most
  important 2027–28 change.
- Ground decoys are **not a single plausibility number**. An inflatable that is
  0.95 convincing to radar and 0.85 to a satellite pass is 0.25 to an FPV
  operator's camera at 200 m in daylight [X, reasoned from the reporting].
- Thermal masking of *people* is very strong and bimodally so — worn correctly a
  blanket produces near-disappearance, worn badly it produces a cold silhouette
  that is **easier** to see than bare skin [M]. Thermal masking of *running
  vehicles* is far weaker. The game's flat 40% is the wrong shape as well as the
  wrong value.
- **Masking is not one number and never was.** Section 5 gives a per-channel
  multiplier matrix for every measure. The pattern that falls out: most masking
  buys one or two channels and nothing on the rest, several measures make a
  target *more* detectable on a channel they do not cover, and the effectiveness
  of every thermal measure swings by a factor of three or more with how long the
  vehicle has been stopped, the weather, and the sun.
- **Only one masking measure can be applied reactively, under observation:
  switching the emitter off.** Everything else has to be in place before contact.
  That asymmetry should drive how the game prices masking far more than cost does.
- **No published measured figure exists** for thermal blanket signature
  reduction, for multispectral net per-band performance, or for any of the
  environmental dependencies. All the reporting is qualitative video comparison.
  Anyone who tells you "blankets cut thermal signature by X%" is guessing,
  including previous versions of this model and most of Section 5.

---

## 1. Decoys that fly

### 1.1 Gerbera

Construction is deliberately crude: a **plywood-reinforced internal compartment
inside a painted foam-plastic body** [M], produced to mimic the planform and
radar return of the Shahed-136/Geran-2. First sightings mid-July 2024 [M].

Cost reporting is inconsistent and worth flagging to whoever tunes this later.
The most-cited figure is **$10,000 per unit**, with some estimates as low as
**$2,000** [M/L]. The Shahed comparison figure in the same reporting ranges from
**$20,000–$50,000** to **$200,000** [L on the high end] — the spread reflects
Iranian-import pricing versus Russian serial production at Alabuga, and the
lower band is the more credible one for 2026.

Signature presented:

- **Radar**: a Luneburg lens (a 3D-printed plastic sphere wrapped in metal foil)
  or a trihedral corner reflector, producing a return "similar to a Shahed-136"
  and often much larger [M].
- **Optical**: roughly Shahed-like planform, which matters for gun-camera and
  operator identification but not for early warning.
- **Thermal and acoustic**: *weaker* than the real thing. The Gerbera's small
  piston or electric powerplant is not an MD-550. This is the decoy's tell.
- **RF**: early pure decoys emit nothing. Later variants carry Western-sourced
  telemetry and control modules [M], which gives them an RF signature the real
  Geran often lacks.

Ratio flown per real strike drone: of roughly **6,500–8,200 Shahed-type UAVs
launched per month** in 2025–26, about **one third are decoys** [M]; some monthly
analyses put the decoy fraction at **40–60%** [L]; Ukrainian official statements
have quoted "about 50% false targets" [M]. A defensible planning figure is
**0.5 to 1.0 decoys per armed drone**, varying by raid.

Crucially, the Gerbera has stopped being a pure decoy. By 2026 it is reported
carrying warheads, reconnaissance payloads, and acting as a mothership launching
FPV drones [M]. This is exactly what you would expect once interceptor costs
fall (Section 1.4), and it is the trend the game should reflect by 2028.

### 1.2 Italmas / BM-35

ZALA's Italmas is a **plywood fuselage with a delta wing, a plastic bottle for a
fuel tank, and an off-the-shelf DLE-60 60cc twin boxer model-aircraft engine**
[M]. Range 200 km, warhead 40 kg [M]. Captured examples have carried radar
reflectors [M]. No credible unit cost is published; the design is explicitly
described as assemblable "at any aeromodelling club" [M], which puts it in the
same low-thousands band as the Gerbera [X].

The Italmas is the cleanest example of the category the game should be modelling:
**a dual-role decoy that is also a weapon**. You cannot "waste" an interceptor on
it, because if you ignore it, it detonates.

### 1.3 Corner reflectors and radar augmentation — how much inflation is possible

This is the one area with hard, checkable physics.

A **square trihedral corner reflector** has maximum RCS
`σ_max = 12πL⁴/λ²`, where L is the panel side length and λ the wavelength [H].
At X-band (λ ≈ 3 cm), a reflector with L = 15 cm gives
σ ≈ 12π(0.15⁴)/(0.03²) ≈ **21 m²** [H, my arithmetic on the published formula].
That is from an object you could hold in one hand. Corner reflectors backscatter
up to **100× the energy of a cylinder of the same size** [M].

A **Luneburg lens** does better still per unit volume and, unlike a trihedral,
gives wide-angle coverage. Published figures: RCS **several hundred times that of
a metal sphere of the same diameter** [M]; a **44 cm lens can reach ~100 m² at
X-band** [M]; a planar variant measured **>25 dB enhancement** [H, peer-reviewed].

The fielded reference point is Ukraine's **SPECTR** decoy (OM Defense Systems,
shown at Eurosatory, 16 June 2026): a **1.2 kg adjustable corner reflector
covering 3–20 GHz, giving a selectable RCS from 1 m² to over 10 m²** [H, vendor
spec]. The airframe is PVC, 1400 × 2690 × 400 mm, 12 kg MTOW, 500 km range,
4 hours endurance, >120 km/h [H, vendor spec].

Against that, the target being imitated: the **Shahed-136's RCS is variously
reported at ~0.01 m²** ("smaller than a European blackbird") [M] and elsewhere in
the 0.1 m² band. It is built with a Nomex or foam honeycomb structure, carbon-
fibre-reinforced polymer, and — per Ukrainian Air Force statements — a **radar-
absorbent liner**, plus matt black paint on later batches [M].

**So the achievable inflation factor is 100× to 1,000× in RCS, i.e. 3.2× to 5.6×
in detection range.** That is the number the game has to be able to express.

### 1.4 The economics, and how they change

Defender's cost per shot [H on published prices]:

| Interceptor | Cost per round |
|---|---|
| Patriot PAC-3 / MSE | $3.8M–$4M (one report quotes $12M for a full engagement) |
| NASAMS with AIM-9X | ~$1M+ |
| IRIS-T SL | ~$485,000 |
| Ukrainian interceptor drone | $1,000–$5,000 |
| Effective cost per Shahed killed, drone-on-drone | ~$10,000 |

Attacker's cost per decoy: **$2,000–$10,000** (Gerbera) [M/L]; **~$4,000** (PARS
Trembita pulsejet decoy, explicitly built to be expended baiting engagements) [L].

Over a campaign this is the whole argument. A decoy stream that reliably draws
SAM rounds is a **100:1 to 1,000:1 exchange in the attacker's favour** and
empties magazines that take years to refill — reporting through 2025 suggested US
Army interceptor stocks had fallen to roughly a quarter of requirement, with a
**three-to-eight-year restock timeline** [L]. Ukraine's answer was to push the
engagement down to cheap interceptors: roughly **6,300 interceptor sorties and
1,500+ drones destroyed in February 2026** [L].

The consequence for the model is not subtle. **When the interceptor costs less
than the decoy, the decoy stops being an economic weapon and becomes only a
saturation and decision-latency weapon.** That flip has already happened at the
low-altitude layer and will be general by 2028.

### 1.5 What decoys actually fool, and where each fails

| Defender element | Fooled? | Why it fails |
|---|---|---|
| Search radar (amplitude only) | **Yes, reliably** | A reflector is a genuine large return. There is nothing to see through. |
| Radar with kinematic/micro-Doppler classification | **Increasingly not** | The lens inflates amplitude, not flight profile, propeller modulation or speed. A 12 kg PVC glider does not fly like a 200 kg Geran. |
| Image-matching / template seekers | **Often yes** | The US military has confirmed that tyres laid on Russian bomber wings are specifically about **confusing image-matching seekers** [M]. Template matching has no shadow reasoning and no context. |
| Multi-spectral autonomous seekers | **Less and less** | Geran Seeker-type payloads pair day and thermal cameras with a laser rangefinder and onboard machine vision [M]. Thermal and range both catch decoys. |
| Human operator, long range / night / compressed downlink | **Usually yes** | Poor resolution, time pressure, no parallax. |
| Human operator, close range, daylight | **Almost never** | This is the hardest sensor to beat and the cheapest one the defender has. |
| Acoustic and passive RF | **No — decoys are usually worse** | An unarmed decoy is quiet and silent. Its *absence* of signature is itself discriminating. |

The design lesson: a decoy should score **high on radar, medium on optical, and
below the real article on thermal, acoustic and RF**. The game's current decoy
line (RF 0 / Thermal 25 / Acoustic 35 / Visual 30 / Radar 80) already has this
shape right. Only the radar figure and the formula are wrong.

---

## 2. Decoys on the ground

### 2.1 Inflatables and thermal decoys

The reference vendor is **Inflatech** (Czech Republic), which makes inflatable
HIMARS, Abrams, artillery and aircraft. Published figures [M]:

- Cost **$10,000–$100,000** per decoy depending on type and fidelity.
- Weight **25–90 kg**, requiring **2–4 people** to handle.
- The **inflation engine emits heat that presents an infrared signature**.
- Vendor claim: **30–40% of the HIMARS Russia claimed destroyed were decoys**
  [L — this is a decoy salesman describing his own product and should be
  discounted accordingly].

The comparison that matters: a GMLRS rocket costs **~$160,000** before launcher
operating costs [M]. A $10,000 inflatable that absorbs one is a 16:1 trade even
before counting the sortie, the targeting cycle, and the real launcher that
survived.

### 2.2 Documented cases of decoys working

- **Wooden HIMARS mockups drew Russian cruise-missile fire** in 2022 [M].
- **June 2023**: Russian channels circulated video of a "destroyed Ukrainian
  tank"; Ukrainian forces released footage of a soldier laughing that "they've hit
  my wooden tank" [M].
- Decoy kits in service include **flat-pack assemblies, inflatables, 2D
  silhouettes and bare radar reflectors** [M].

### 2.3 Documented cases of decoys failing

- **Painted 2D Tu-95 silhouettes on the apron at Engels** were assessed as
  fooling essentially nobody: they cast no shadow, and commercial satellite
  imagery resolves them trivially [M]. This is the clearest published failure.
- **Tyres laid on real Tu-95 and Tu-160 airframes** did not prevent Operation
  Spiderweb in June 2025, in which around 20 Russian aircraft were hit by
  close-in quadcopters [M]. The measure targeted image-matching seekers and was
  irrelevant to a human operator flying a quadcopter into the wing root.

The pattern is consistent and useful for design: **decoys degrade with sensor
resolution, with observation time, and with dwell.** They work against a fast,
distant, single-look engagement and fail against a slow, close, repeated one.

### 2.4 Operators versus autonomous seekers

The important asymmetry: an autonomous seeker is *not* strictly better than a
human at decoy rejection. Template and contrast-based trackers are "vulnerable to
occlusion and viewpoint variation" [M], which is precisely why tyres work. What
does beat decoys is **fusion** — texture plus geometry plus thermal, as in
FLIR-class pixel-lock and Geran Seeker-type payloads [M]. So through 2027 the
game should model:

- Autonomous seeker, single-spectrum: **worse** than a human operator at decoy
  rejection.
- Autonomous seeker, fused day + thermal + range: **better** than a human
  operator, because consistent and tireless.

---

## 3. Masking

### 3.1 Thermal blankets and IR-suppressing covers

The fielded example is **STG Defence's Chuhaistyr Gen 2** — nylon embedded with
silver crystals, trapping heat on the inside and reflecting it on the outside
[M]. It exists because **Western thermal blankets cost ~$2,000 each**, which
Ukrainian units could not afford [M]. Ukrainian MoD has separately trialled an
anti-thermal poncho at **2.5 kg**, testing five IR-camouflage fabrics [M].

Reported effectiveness, all qualitative:

- Worn correctly, a well-made blanket makes the wearer **"practically disappear"**
  on the thermal cameras drones carry [M].
- Worn badly, it makes the wearer **stand out more, not less** [M].
- Thermal cloaks that nullify the heat signature completely end up **colder than
  the surrounding soil, producing a clearly visible dark spot** [M].
- The MoD poncho's results "were not flawless" — a **dappled outline remained
  visible** [M].

**There is no published measured percentage.** Not one source in this research
gave a figure. The game's "cuts thermal signature to 40%" is unsourced, and so is
any replacement number. What the evidence does support is the *shape*: the
distribution is bimodal, not a flat multiplier.

For vehicles the picture is much worse for the hider. A blanket over a cold-
soaked hull is effective; a running engine and exhaust radiate kilowatts that no
textile suppresses. Saab's **ULCAS quotes up to 80% reduction in solar loading**
[H, vendor spec] — note carefully that this addresses *solar-heated skin*, not
engine heat, and is the only hard number in the category.

### 3.2 Camouflage netting

Blunt finding: **conventional camouflage netting does essentially nothing in the
thermal band.** "Colour camouflage, camouflage nets, and even well-executed field
engineering offer little protection if a position remains thermally visible" [M].
Treat standard nets as an optical-only measure.

**Multispectral** nets are a different product. Saab Barracuda MCS and the
Barracuda net family are specified against **UV, visual, near-IR, short-wave IR,
thermal IR and radar** [H, vendor spec], and the Netherlands ordered MCS for its
Fennek and PzH 2000 fleets in 2026 [M]. Saab does not publish per-band reduction
figures, and I could not find them anywhere. Cost is likewise unpublished; the
order-of-magnitude is tens of thousands of dollars per vehicle kit [X].

Separately and not a signature measure at all: **nylon anti-FPV netting at
10–15 cm mesh is reported to cut FPV hit probability by ~70% in static
positions**, with no protection against diving attacks or fibre-optic drones that
cut cables [L]. It appears in the matrix below as a row of 1.00s, which is the
point.

### 3.3 Emission control

The governing principle is simple and well attested: **"an emitter announces its
own position every time it transmits"** [M], and **"a jammer that can be
geolocated in seconds has a short service life"** [M]. Roughly **90 Russian
jamming systems have been visually confirmed destroyed, damaged or captured**
[M], and both sides now field anti-radiation loitering munitions that home on
radar and jammer emissions [M].

Two practical mitigations appear in the reporting:

- **Reactive jamming** — transmitting only when a target is present — presents "a
  smaller and more intermittent emission than a system in continuous barrage" [M].
- **Fibre-optic control links**, which eliminate the RF signature entirely during
  approach [M]. The game already models this correctly with the fibre-optic quad
  at RF 0 — that is one of the best-grounded numbers in the current table.

No source gave a time-to-geolocation in seconds. "Seconds to minutes" is the
consistent qualitative claim [M]; any specific number would be invention.

### 3.4 Acoustic masking

This is the thinnest-evidenced area in the whole brief and it should be said
plainly: **I found no documented deliberate acoustic masking of ground units.**
The practical measures — generator siting, mufflers, working under artillery
noise — are not reported with any quantification.

On the air side quieting is real and measurable. **MIT Lincoln Laboratory's
toroidal propeller shows nearly 20 dB of axial noise reduction versus a
conventional propeller** [H]. More interesting for a detection model: toroidal
designs **shift noise from discrete tonal peaks to broadband "whoosh", defeating
detection algorithms trained on harmonic spike identification** [M]. That is a
*classifier* defeat, not just a loudness reduction, and it is the right way to
model it.

### 3.5 Low-observable drone design in the field

| Measure | Status | Figures |
|---|---|---|
| Carbon-fibre / foam-sandwich airframe, Nomex honeycomb | **Fielded** on Geran variants | Qualitative only [M] |
| Radar-absorbent liner + matt black paint | **Fielded** on Shahed/Geran | Typical RAM gives 20–30 dB; a research spray-on volcanic-rock formulation claims up to 43 dB [M — lab claim, not fielded] |
| Shaping | **Not meaningfully fielded** on expendable drones — cost and manufacturability dominate | — |
| Electric propulsion | Fielded (all FPV) | Low thermal and acoustic inherently |
| Hydrogen fuel cell | **First combat deployment claimed by Ukraine**, "negligible" heat signature | Vendor/press claim [L] |
| Toroidal / low-noise propellers | Prototype-to-early-field | ~20 dB [H] |

The honest summary: **stealth in this war is achieved by being small, cheap and
made of foam, not by being shaped.** Nothing in the reporting suggests faceting
or planform alignment on any expendable airframe.

---

## 4. Decoy reference table

| Decoy | Unit cost | Signature presented | Effectiveness | Conf. |
|---|---|---|---|---|
| Gerbera (RU) | $2,000–$10,000 | Radar via Luneburg lens/corner reflector; Shahed-like planform; weak thermal/acoustic; some RF | Composes ⅓–½ of Shahed-type salvos; now dual-role strike/recon/mothership | M |
| Italmas BM-35 (RU) | Not published; plywood + off-the-shelf 60cc engine | Radar reflector fitted; 200 km, 40 kg warhead | Dual-role: cannot be safely ignored | M |
| "Parody" (RU) | Not published | 3D-printed foil-wrapped Luneburg sphere | Appears Shahed-sized on radar | M |
| SPECTR (UA, 2026) | Not published | **1.2 kg reflector, 3–20 GHz, RCS selectable 1 → >10 m²**; PVC airframe, 12 kg, 500 km, 4 h | Purpose-built to bait SAM shots across bands | H (spec) |
| PARS Trembita (UA) | ~$4,000 | Pulsejet: loud and hot as well as radar-visible | Deliberately expendable bait | L |
| Inflatable vehicle (Inflatech) | $10,000–$100,000 | Optical + IR from inflation engine; radar if reflector added | Vendor claims 30–40% of "destroyed HIMARS" were decoys | M spec / L claim |
| Wooden or flat-pack mockup | Hundreds to low thousands | Optical only | Documented cruise-missile and FPV absorptions | M |
| Painted 2D silhouette | Cost of paint | Overhead optical only | **Assessed ineffective** — no shadow, resolvable from satellite | M |
| Tyres on a real airframe | ~Free | Breaks the seeker's image template | Works against template matching; irrelevant to close quadcopter attack | M |

## 5. Masking, channel by channel

The coordinator asked for multipliers per channel rather than verdicts. Here they
are. **Read the confidence column before using any of them.** Only three figures
in this entire matrix are published: ULCAS's 80% solar-loading reduction, the
toroidal propeller's ~20 dB, and the 20–30 dB coupon figure for conventional RAM.
Everything else is my allocation, reasoned from the qualitative reporting in
Section 3 and from signature physics, and marked **[X]**.

Multipliers apply to the unit's 0–100 signature on that channel. **1.00 = no
effect. Above 1.00 = the measure makes the target *more* detectable on that
channel** — those cells are the ones worth designing around.

### 5.1 The matrix

| Measure | RF | Thermal | Acoustic | Visible | Radar | Conf. |
|---|---|---|---|---|---|---|
| **Personal thermal blanket, correct use** | 1.00 | **0.18** | 1.00 | 0.85 | **1.20** | [X]; near-disappearance is [M] |
| **Personal thermal blanket, bad use / saturated** | 1.00 | **1.15** | 1.00 | 0.85 | **1.20** | Inversion to cold silhouette is [M] |
| **Vehicle IR-suppressing cover, cold hull** | 1.00 | **0.45** | 1.00 | 0.80 | 1.05 | [X] |
| **Vehicle IR-suppressing cover, hot powerpack** | 1.00 | **0.85** | 1.00 | 0.80 | 1.05 | [X] |
| **Multispectral net (Barracuda-class), cold** | 1.00 | **0.55** | 1.00 | **0.25** | **0.70** | [X]; band coverage is [H] vendor |
| **Multispectral net, hot vehicle under it** | 1.00 | 0.80 | 1.00 | 0.25 | 0.70 | [X] |
| **Standard garnished net (non-IR)** | 1.00 | **0.95** | 1.00 | **0.35** | 0.95 | Near-zero thermal effect is [M] |
| **IR-matched matt camouflage paint** | 1.00 | 0.90 | 1.00 | 0.75 | 1.00 | [X] |
| **Matt black drone paint, night** | 1.00 | 1.00 | 1.00 | **0.55** | 1.00 | [X]; the practice is [M] |
| **Matt black drone paint, daylight vs sky** | 1.00 | 1.00 | 1.00 | **1.15** | 1.00 | [X] |
| **RAM liner on a strike drone (as fielded)** | 1.00 | 1.00 | 1.00 | 1.00 | **0.55** | [X]; 20–30 dB coupon figure is [M] |
| **Shaping / planform alignment** | 1.00 | 1.00 | 1.00 | 1.00 | **0.30** | [X] — **not fielded on expendables**, so 1.00 in practice |
| **Vegetation and terrain concealment, fresh cut** | 1.00 | **0.60** | 0.90 | **0.20** | **0.60** | [X] |
| **Vegetation, cut and 48 h old** | 1.00 | 0.75 | 0.90 | **0.55** | 0.60 | [X] |
| **Revetment / berm / overhead cover** | 0.60 | **0.45** | 0.85 | **0.30** | **0.35** | [X] — this is line-of-sight blockage, not signature reduction |
| **Exhaust cooling and mixing** | 1.00 | **0.75** whole vehicle, **0.50** hot-spot | 0.80 | 1.00 | 1.00 | [X] |
| **Toroidal propellers** | 1.00 | 1.00 | **0.35** | 1.00 | 1.00 | ~20 dB is [H]; mapping to index is [X] |
| **Ducted fan** | 1.00 | 1.05 | **0.55** | 1.00 | **1.10** | [X] — a duct is a cavity, and cavities reflect |
| **Electric instead of combustion propulsion** | 1.00 | **0.20** | **0.45** | 1.00 | 1.00 | [M] direction, [X] magnitude |
| **Battery / motor thermal management** | 1.00 | **0.70** | 1.00 | 1.00 | 1.00 | [X] |
| **Hydrogen fuel cell** | 1.00 | **0.15** | 0.50 | 1.00 | 1.00 | "Negligible" claim is [L] |
| **EMCON — radar or jammer switched off** | **0.00** | 1.00 | 1.00 | 1.00 | 1.00 | [H] |
| **Reactive / low-duty-cycle transmission** | **0.25** | 1.00 | 1.00 | 1.00 | 1.00 | [M] direction, [X] magnitude |
| **Fibre-optic control link** | **0.00** | 1.00 | 1.00 | 1.05 | 1.00 | [H] on the zero |
| **Anti-FPV nylon netting** | 1.00 | 1.00 | 1.00 | **1.15** | 1.00 | Not a signature measure at all — see below |

### 5.2 What each measure does nothing for

Spelled out, because this is where most of the design value is:

- **Thermal blankets and IR covers do nothing for RF, acoustic or radar** — and a
  metallised blanket is plausibly *worse* on radar, since foil over a body is a
  reflector [X]. They do a little for visible, because they are patterned, but
  that is incidental.
- **Camouflage netting, standard type, does nothing for thermal, acoustic or RF.**
  This is the single most commonly held wrong belief and it is directly
  contradicted by the reporting: nets "offer little protection if a position
  remains thermally visible" [M].
- **Multispectral netting does nothing for acoustic or RF.** It is good on visible,
  useful on radar, moderate on thermal. It cannot hide a transmitting radio or a
  running generator.
- **Paint does nothing for RF, thermal, acoustic or radar.** Paint is a visible-band
  measure with a small emissivity side-effect. It is not stealth.
- **Vegetation and berms do nothing for RF** if the antenna is above the screen, and
  very little for acoustic. Their radar value is line-of-sight blockage, not
  absorption — a berm does not reduce RCS, it removes the target from the beam.
- **Exhaust cooling does nothing for RF, visible or radar.** It is a thermal
  hot-spot measure with an acoustic bonus from the muffler.
- **Quieter propulsion does nothing for RF, visible or radar**, and a ducted fan is
  slightly *worse* on radar because the duct is a cavity.
- **EMCON does nothing for any channel but RF.** A silent jammer is still a large
  hot metal box with a visible antenna. This matters: in the game, switching a
  jammer off should take RF 100 → 0 and change nothing else, which means a jammer
  parked in the open is still trivially findable optically.
- **Anti-FPV netting does nothing for any signature channel.** It is a kinetic
  measure. It also marks the position — a 15 cm-mesh net over a trench is a
  conspicuous artificial geometry from above [X]. Worth modelling as a hit-
  probability modifier with a *visible signature penalty*, not as masking.

### 5.3 Counter-productive regimes

Every one of these is a mechanic worth having, because they make masking a
decision rather than a free upgrade.

| Measure | Counter-productive regime | Channel it hurts |
|---|---|---|
| Thermal blanket | Full nullification against a warmer background produces a **cold silhouette** [M] | Thermal |
| Thermal blanket, metallised | Foil layer is radar- and mmW-reflective [X] | Radar |
| Vehicle IR cover | Left on while moving → powerpack overheats, then radiates harder than uncovered [X] | Thermal |
| Multispectral net | Untied edges flap; motion in an otherwise static scene is the strongest optical cue there is [X] | Visible |
| Cut vegetation | Wilts within 24–72 h; near-IR reflectance of dead foliage diverges sharply from living plants, so it goes **dark** under NIR-sensitive cameras [X] | Visible / NIR |
| Matt black drone paint | Conspicuous silhouette against a bright daytime sky [X] | Visible |
| Ducted fan | Duct forms a cavity and raises RCS [X] | Radar |
| Overhead cover | Blocks your own drone launch, comms and sensor arcs | Own capability |
| EMCON | The unit's own radar reach goes to zero while silent | Own capability |
| Anti-FPV netting | Marks the position as occupied and worth striking [X] | Visible |
| Any decoy near a real position | Draws attention to the area even when the decoy is correctly identified [M] | All |

### 5.4 Environmental dependence

The coordinator's point is exactly right: a blanket on a vehicle that has been
cold-soaked in shade overnight is a different object from a blanket on a vehicle
that just drove twenty kilometres. The governing variable on the thermal channel
is **apparent ΔT against background**, and every environmental term below acts
through it.

Modifiers are multiplicative on the base multiplier in Section 5.1. All **[X]**
unless noted — I found no source that quantifies any of this, and I want that
stated plainly rather than buried.

| Condition | Effect on thermal masking | Effect on other channels |
|---|---|---|
| **Vehicle stationary >4 h, shaded** | Covers and nets near best case: ×0.8 on the multiplier (i.e. more effective) | — |
| **Vehicle stationary 1–4 h** | Hull cooled, powerpack still warm: ×1.0 | — |
| **Vehicle stationary <30 min after a 20 km road move** | Powerpack, running gear and exhaust all hot; a cover traps and re-radiates: ×1.8 (much less effective). Hot-spot suppression is nearly useless here | Acoustic irrelevant once stopped |
| **Ambient near freezing** | ΔT of a human or engine against background is largest — masking matters most and works least: ×1.3 | Radar and RF unaffected |
| **Ambient near body/skin temperature (hot summer day)** | ΔT is small anyway; masking is nearly redundant: ×0.7 | — |
| **Thermal crossover, ~30–60 min around dawn and dusk** | Background and target converge; everything looks flat. Masking multiplier ×0.6 — but note the *baseline* thermal signature should also drop. The game's twilight ×0.90 understates this; **×0.65 is the better figure [X]** | Optical falling, acoustic best |
| **High sun, clear sky, midday** | Solar loading dominates. A solar screen is worth most here — ULCAS's **up to 80% solar-load reduction [H]** applies to this case and only this case: ×0.6 | Visible detection at its best |
| **Rain** | Wets and cools everything, evaporative cooling flattens contrast, and LWIR transmission drops sharply. Rain is the strongest natural masker available: ×0.5 on the masking multiplier **and** a large cut to baseline thermal reach | Optical degraded; X-band radar picks up rain clutter; acoustic degraded by rain noise |
| **Wind >5 m/s** | Convective cooling shrinks contrast and disperses exhaust plumes: ×0.8. But nets flap | **Acoustic**: raises the ambient noise floor materially — the cheapest masking in the game is a windy day |
| **Snow cover** | Background is very cold and very uniform; any warm object is unmissable. Masking ×1.4 | Visible: dark equipment on snow is at maximum contrast |
| **Dense foliage, summer** | Canopy blocks line of sight outright | Visible ×0.4, radar ×0.7 from attenuation and clutter |
| **Bare trees, winter** | Vegetation concealment largely collapses | Visible ×1.6 relative to summer |

Two of these are worth promoting into the core model regardless of what happens
to masking: **rain** and **thermal crossover** are both larger effects than
anything a textile achieves, and the game currently models neither.

### 5.5 Emplacement time, and whether it can be done under observation

Drone observation is continuous on a modern front. A measure that takes forty
minutes to put up is a measure you can only use before you are seen, which in
practice means most vehicle-scale masking is emplaced at night or not at all.

| Measure | Time to emplace | Under observation? |
|---|---|---|
| EMCON — switch off | **Instant, reversible instantly** | **Yes** — the only measure that is |
| Personal thermal blanket | Seconds | **Yes** |
| Fibre-optic link | Built in; no field action | N/A |
| Matt paint | Depot or factory | No |
| RAM liner | Factory | No |
| Exhaust cooling kit | Depot fit | No |
| Cut vegetation over a vehicle | 10–30 min, 2 crew | Marginal — movement is visible |
| Standard garnished net | 15–30 min, 2–4 crew | No |
| Multispectral net kit (25–90 kg class handling, by analogy with decoy kits [M]) | **20–45 min, 3–4 crew** | **No** |
| Inflatable decoy | 10–20 min, **2–4 people** for a 25–90 kg kit [M] | No |
| Revetment or berm | Hours, needs an excavator | No |
| Overhead cover | Hours to days | No |

The design consequence is stark and worth stating: **EMCON is the only masking
measure a unit can apply reactively.** Everything else must be in place before
contact. That asymmetry should drive how the game prices them.

### 5.6 Cost, relative to what is protected, and why units skip it

| Measure | Cost relative to the protected asset | Why units do or do not use it |
|---|---|---|
| EMCON | **Free in money** | Used constantly. The cost is capability: a silent radar sees nothing |
| Personal thermal blanket | Western ~$2,000 vs an infantryman's whole kit — **prohibitive**, which is exactly why Ukrainian units built the cheap Chuhaistyr [M] | Price was the binding constraint, and it was solved domestically |
| Standard net | **<0.5%** of a vehicle | Universal. Cheap, light, and everyone already has one |
| Multispectral net kit | Unpublished; **~1–5% of an armoured vehicle [X]** | Weight, emplacement time, snagging on hatches, and crews strip them for maintenance and do not put them back |
| Matt paint | **<0.5%** | Universal, no downside |
| RAM liner on a $20k–$50k expendable drone | Even $2,000 of coating is **4–10%** — material on a one-way airframe | This is why fielded treatment is a thin liner plus black paint, not a proper multi-layer RAM stack [M] |
| Shaping | Large, through tooling and manufacturability | **Why it is not fielded on expendables at all.** Cheapness is the design goal; a shaped airframe defeats the point |
| Exhaust cooling | Low single-digit % | Fitted where it comes as standard; rarely retrofitted |
| Toroidal propellers | Marginal per unit | Thrust-efficiency penalty versus a conventional blade; adoption is early |
| Hydrogen propulsion | **High** — storage and fuel-cell cost dominate a cheap airframe | Niche; one claimed combat deployment [L] |
| Revetment or berm | Cost is **engineer time and plant**, the scarcest resource on a front | Used where the position is static and valuable enough to justify the hours |
| Anti-FPV netting | Low | Widely used despite marking the position, because the hit-probability gain is immediate |

The recurring practical reason for non-use across the whole table is not money.
It is **time under observation, weight, and the fact that crews remove things that
interfere with using the vehicle**. A game that charges only build cost for
masking will overstate its adoption; charging emplacement time and a maintenance
or mobility penalty gets the behaviour right.

---

## 6. Verdict on the game's current assumptions

### 6.1 "Decoy drone radar signature 80 against a real strike drone at 55–60"

**Wrong — but the numbers are nearly right and the formula is wrong.**

Under the current rule, `(80/60)^0.25 = 1.074`. The decoy gets detected 7% further
out than the drone it is protecting. Against a small FPV at 22 it is
`(80/22)^0.25 = 1.38` — the decoy is 38% more visible than a plastic quadcopter,
when in reality it is three orders of magnitude more visible. **The decoy unit
does not currently do the thing decoys do.**

The fix is to reinterpret the 0–100 radar signature scale as **decibels of RCS**,
which is what radar people actually use, and change the reach rule to match:

```
S_radar = 2 × RCS_dBsm + 80          (so S = 80 ⇔ 1 m²; 2 points = 1 dB)
radar reach = base_at_1m² × 10^((S − 80) / 80)
```

The exponent 80 falls straight out of R ∝ σ^(1/4): 40 dB of RCS is 10× in range,
and 40 dB is 80 signature points.

That mapping validates almost every existing number:

| Unit | Current S | Implied RCS | Sanity check |
|---|---|---|---|
| Small electric quad | 22 | 0.0013 m² | Correct for a 7-inch quad |
| Fixed-wing recon | 40 | 0.01 m² | Correct |
| Combustion heavy strike drone | 60 | 0.1 m² | Upper end of reported Shahed figures |
| Decoy drone | 80 | 1 m² | **Low** — SPECTR's *minimum* setting |

**Recommended changes:**

- **Decoy drone: 80 → 92.** That is 10^0.6 ≈ 4 m², mid-range for a fielded
  reflector, and gives the decoy `10^((92−52)/80) = 3.2×` the detection range of
  the strike drone it escorts. That is the real figure from Section 1.3.
- **Combustion heavy strike drone: 60 → 52** (0.016 m²) and **turbojet strike
  drone: 55 → 50**, reflecting the RAM liner, CFRP and foam sandwich now standard
  on Geran airframes [M]. Keeping them at 0.1 m² ignores three years of treatment.
- **Add a decoy variant band** rather than one value: bare airframe 60, small
  corner reflector 85, Luneburg lens 95. SPECTR's selectable reflector is exactly
  this, in hardware.
- Re-anchor the Radar Mast's 1400 m as "against a 1 m² target" rather than
  "against signature 100". Everything else follows.

If the formula change is too invasive, the fallback is to **allow radar signature
above 100 for decoys** (decoy = 400, i.e. RCS ratio 8:1 against a strike drone at
52, giving 1.68× range). Ugly, and still understates the effect, but it at least
makes the unit do something.

### 6.2 "A thermal blanket cutting thermal signature to 40%"

**Wrong in value and wrong in shape.**

Value: `√0.40 = 0.63`, a 37% reach reduction. The reporting describes near-
disappearance on drone thermal cameras when the blanket is used correctly [M].
For an infantry-scale target that argues for **15–20%** of baseline signature
(a 55–60% reach reduction), not 40%.

Shape: the outcome is **bimodal**, and the failure mode is not "no benefit" but
"negative benefit" — a body fully insulated against a warmer background reads as a
**cold silhouette**, more conspicuous than an uncovered one [M]. Recommended
model:

- **Correct use (~65% of the time [X]): thermal signature × 0.18.**
- **Incorrect use or thermal saturation over time (~35% [X]): thermal signature
  × 1.15** — i.e. worse than nothing.
- Both branches should interact with the existing day/night multiplier. A blanket
  is most valuable at night (thermal ×1.25) and least valuable at midday, when
  solar loading already swamps contrast.

For **vehicles**, do not use the same number, and do not use a single number at
all — see the matrix in Section 5.1 and the environmental modifiers in 5.4. A
multispectral net over a cold-soaked vehicle is ×0.55 on thermal; over one that
has just driven twenty kilometres it is ×0.55 × 1.8 ≈ **×0.99**, which is to say
it is doing nothing on that channel until the powerpack cools. That swing — a
factor of nearly two, driven entirely by time since the vehicle last moved — is
the most important thing the current flat 40% throws away.

### 6.3 "A ground decoy at plausibility 0.90 against a real target at 1.00"

**Wrong because it is a scalar.** A single plausibility number cannot be right,
because the evidence shows the same physical decoy scoring anywhere from 0.95 to
near zero depending on which sensor is looking and from how far. Painted 2D
aircraft fooled nobody [M]; wooden HIMARS absorbed cruise missiles [M]. Same
category, opposite outcomes, because the sensors differed.

**Recommended replacement — per-channel plausibility:**

| Channel / condition | Inflatable w/ heater | Wooden mockup | 2D silhouette |
|---|---|---|---|
| Radar (reflector fitted) | 0.95 | 0.95 | 0.90 |
| Optical, >1000 map m or satellite | 0.88 | 0.85 | 0.55 |
| Optical, 300–1000 map m | 0.65 | 0.55 | 0.20 |
| Optical, <300 map m, daylight (FPV terminal) | 0.30 | 0.20 | 0.05 |
| Thermal | 0.60 | 0.10 | 0.05 |
| Acoustic | 0.00 | 0.00 | 0.00 |
| Passive RF | 0.00 | 0.00 | 0.00 |

All of these are **[X] — my reasoned allocation**, anchored on three sourced
points: reflector-equipped decoys beat radar reliably [H]; 2D silhouettes are
resolvable from commercial satellite imagery and cast no shadow [M]; inflation
engines produce a genuine IR signature [M].

Two mechanics worth adding, both supported by the reporting:

1. **Plausibility decays with observation.** A decoy that survives repeated
   passes while everything around it is destroyed, and that never moves, emits or
   is resupplied, becomes obvious. Decay ~15% per sustained observation [X].
2. **A recognised decoy still has value.** "Even when a drone operator correctly
   suspects a decoy, the uncertainty itself becomes a weapon" [M]. Model this as
   an engagement-delay penalty, not a strike, so that identifying a decoy still
   costs the attacker a sortie's worth of time.

---

## 7. What changes by 2028

1. **Decoy economics invert.** Once interceptors cost $1,000–$5,000 [H], a
   $10,000 decoy is a losing trade. Expect the pure decoy to die out and the
   **dual-role decoy-weapon** (Gerbera with a warhead, Italmas) to dominate. In
   game terms: by 2028, a decoy should cost roughly what a strike drone costs, or
   be able to strike.
2. **Radar learns to classify, not just detect.** Micro-Doppler and kinematic
   discrimination reject a reflector-carrying glider that flies at 120 km/h while
   claiming to be a 185 km/h Geran. **This is the most important change for the
   model**: add a classification stage separate from detection, where radar has
   high detection probability against decoys and steadily improving *rejection*
   probability. Suggested trajectory: 2026 rejection ~20%, 2028 ~55% [X].
3. **Fused seekers kill ground decoys.** Day + thermal + laser rangefinder on a
   cheap microcomputer is already fielded on Geran Seeker payloads [M]. By 2028
   an unheated inflatable is near-worthless against terminal guidance; a heated
   one holds partial value. Drop the thermal-channel plausibilities by ~0.2.
4. **Multispectral masking standardises.** Netherlands' 2026 Barracuda MCS order
   [M] is the leading indicator. By 2028 assume multispectral nets are the default
   on high-value vehicles, not an upgrade.
5. **Acoustic detection gets harder faster than it gets better.** Toroidal
   propellers defeat harmonic-trained classifiers [M], electric and hydrogen
   propulsion cut the source. The game's acoustic reliability of 52 is already
   the lowest channel; that is the right call, and it should not rise.
6. **EMCON tightens.** Anti-radiation loitering munitions are being
   industrialised. Emitting continuously should become close to suicidal — model a
   transmit **duty cycle** rather than a static RF signature, with passive-RF
   detection probability scaling with time-on-air.

---

## 8. Genuinely uncertain or contested

- **Shahed/Geran RCS** varies across sources by an order of magnitude (0.01 vs
  0.1 m²), and legitimately varies with aspect, band and production batch. Do not
  treat any single figure as settled.
- **Gerbera unit cost** ($2,000 vs $10,000) and the Shahed comparison ($20,000 to
  $200,000) are both wide. The low end is more credible for 2026 Alabuga output.
- **Decoy fraction of salvos** ranges from one third to 60% depending on month and
  analyst. Use a band, not a point.
- **Inflatech's 30–40% claim** is a vendor claim with an obvious commercial
  interest and no independent verification.
- **Thermal blanket reduction has no measured figure anywhere in open source.**
  Every number in Section 6.2 is reasoned, not reported.
- **Multispectral net performance is unpublished.** Saab quotes solar loading and
  nothing else. Treat all per-band reduction figures as estimates.
- **The entire per-channel matrix in Section 5.1 is estimated except for three
  cells** — the 80% solar-loading figure, the ~20 dB toroidal propeller figure,
  and the 20–30 dB coupon figure for conventional RAM. The *relative ordering* of
  the matrix is well supported by the qualitative reporting; the absolute
  multipliers are not measurements and should not be quoted as such outside this
  document.
- **Every environmental modifier in Section 5.4 is reasoned from thermal physics,
  not sourced.** Direction and rough magnitude are defensible; the specific
  numbers are placeholders for playtesting to move.
- **Emplacement times in Section 5.5 are extrapolated** from the one published
  handling figure in the research (25–90 kg decoy kits needing 2–4 people). No
  source gives net emplacement times.
- **Time-to-geolocation for an emitting jammer** is described qualitatively
  ("seconds", "short service life") and never quantified.
- Several results in this research came from outlets of uncertain provenance
  (battlepolicy.com, drone-warfare.com, globalmilitary.net,
  ukraine-war-analytics.com, defenceukraine.com, theboard.world). Figures sourced
  only to those are marked [L] above and should be re-verified before anything
  load-bearing is built on them.

---

## Sources

Decoy drones and construction
- https://en.wikipedia.org/wiki/Gerbera_(drone)
- https://www.kyivpost.com/post/46692
- https://global.espreso.tv/russia-ukraine-war-what-is-inside-cheap-gerbera-drone-russia-uses-as-decoy-for-ukrainian-air-defense
- https://en.defence-ua.com/weapon_and_tech/all_electronics_from_the_russian_foam_drone_gerbera_featured_in_one_photo-12376.html
- https://www.forbes.com/sites/vikrammittal/2026/08/02/the-russian-gerbera-drone-is-evolving-beyond-being-a-simple-decoy/
- https://www.forbes.com/sites/davidhambling/2026/02/10/russia-is-launching-fpvs-from-drone-motherships/
- https://en.wikipedia.org/wiki/Italmas
- https://newsukraine.rbc.ua/news/inside-russia-s-italmas-drone-deadly-secret-1753549486.html
- https://statewatch.org.ua/en/publications/novynu/sanktsii/how-sanctions-loopholes-allow-the-makers-of-the-italmas-drone-to-keep-operating/
- https://isis-online.org/isis-reports/russian-decoy-drones-that-depend-on-western-parts-pose-a-great-challenge/
- https://isis-online.org/isis-reports/monthly-analysis-of-russian-shahed-136-deployment-against-ukraine
- https://ukranews.com/en/news/1046580-russian-armed-forces-launch-about-50-of-false-targets-for-air-defense-of-ukraine-and
- https://global.espreso.tv/russia-ukraine-war-weapons-of-mass-deception-what-are-decoy-drones-and-how-russia-and-its-allies-are-trying-to-bypass-air-defense

Radar augmentation, reflectors and RCS
- https://www.radartutorial.eu/17.bauteile/bt47.en.html
- https://onlinelibrary.wiley.com/doi/10.1002/mmce.23352
- https://en.wikipedia.org/wiki/Luneburg_lens
- https://en.defence-ua.com/weapon_and_tech/how_and_why_russia_uses_luneburg_lenses_in_drones_and_whether_the_armed_forces_of_ukraine_have_them-12265.html
- https://www.qinetiq.com/en/what-we-do/test-and-training/threat-representation/target-systems/other-products-and-services/passive-radar-enhancement
- https://militarnyi.com/en/news/ukraine-unveils-new-spectr-decoy-system/
- https://united24media.com/defense-tech/ukrainian-defense-company-unveils-spectr-smart-decoy-drone-for-radar-confusion-19883
- https://missilethreat.csis.org/missile/shahed-131-and-136/
- https://osmp.ngo/collection/shahed-131-136-uavs-a-visual-guide/
- https://www.csis.org/analysis/shahed-geran-how-russia-continues-reinvent-one-way-attack-drone
- https://militarnyi.com/en/news/russian-forces-employ-stealth-tactics-shahed-drones-go-black-to-elude-ukrainian-air-defense/
- https://www.tomshardware.com/tech-industry/researcher-develops-spray-on-stealth-coating-for-drones-volcanic-rock-formulation-claims-to-reduce-radar-return-signals-by-up-to-43db-compared-to-20-to-30db-for-typical-radar-absorbent-material

Interceptor and decoy economics
- https://www.nationaldefensemagazine.org/articles/2026/4/15/ukraine-flips-cost-imbalance-script-with-lowcost-interceptors
- https://www.twz.com/news-features/inside-ukraines-interceptor-drone-innovations-swatting-down-thousands-of-shahed-drones
- https://united24media.com/defense-tech/what-is-ukraines-interceptor-one-of-the-worlds-most-in-demand-drones-17055
- https://re-russia.net/en/analytics/0323/
- https://warontherocks.com/2026/03/lessons-from-ukraine-for-defending-gulf-airspace-from-shaheds
- https://www.kyivpost.com/post/72300
- https://migflug.com/afterburner/autonomous-drone-on-drone-interception-ukraine-maxon/

Ground decoys
- https://www.aljazeera.com/news/2023/3/7/czech-company-sees-boom-in-market-for-fake-tanks-himars
- https://www.taipeitimes.com/News/world/archives/2023/03/08/2003795735
- https://www.sandboxx.us/news/inflatable-tanks-and-artillery-a-wwii-trend-reappears-in-ukraine/
- https://san.com/cc/ukraine-is-using-inflatable-tanks-and-himars-against-russia/
- https://jifad.org/2025/09/08/fake-drones-and-plywood-tanks-ukraine-and-russias-war-of-deceptio/
- https://united24media.com/latest-news/how-this-100-year-old-war-trick-is-outsmarting-russian-drones-in-ukraine-15236
- https://www.twz.com/tu-95-decoys-are-being-painted-on-russian-air-bases-apron
- https://www.forbes.com/sites/petersuciu/2023/10/02/painted-tu-95-bomber-silhouettes-on-runway-arent-fooling-social-media/
- https://www.twz.com/air/russia-covering-its-aircraft-in-tires-is-about-befuddling-image-matching-seekers-u-s-military-confirms
- https://aerospaceglobalnews.com/news/russia-tyres-on-fighter-jets-bomber-airplanes/

Thermal masking and camouflage
- https://euromaidanpress.com/2025/12/12/new-thermal-blanket/
- https://euromaidanpress.com/2025/07/10/how-to-wear-a-thermal-blanket/
- https://euromaidanpress.com/2025/05/17/russian-troops-are-trying-to-hide-from-ukraines-night-vision-drones/
- https://www.trenchart.us/p/russian-troops-cheap-thermal-blankets
- https://en.defence-ua.com/weapon_and_tech/ukraine_makes_anti_heat_camouflage_suits_to_hide_from_russian_thermal_imagers_how_it_works_and_how_effective_it_is-8144.html
- https://en.defence-ua.com/weapon_and_tech/real_effectiveness_of_thermal_camouflage_on_full_display_in_ukrainian_field_video-11691.html
- https://en.defence-ua.com/weapon_and_tech/ukrainian_ir_camera_killer_drone_vs_russian_thermal_camouflage_video-11113.html
- https://www.kyivpost.com/post/28432
- https://stg-defence.com/en/how-to-hide-from-a-thermal-imager-effective-strategies-and-methods/
- https://www.saab.com/products/land/camouflage-systems
- https://www.saab.com/products/mcs-mobile-camouflage-system
- https://www.saab.com/products/ulcas
- https://www.armyrecognition.com/news/army-news/2026/saab-barracuda-mcs-camouflage-netherlands-deal

Emission control and electronic warfare
- https://www.strategypage.com/htmw/htecm/articles/202407020224.aspx
- https://spectrum.ieee.org/amp/electronic-warfare-ukraine-2671772127
- https://dsm.forecastinternational.com/2025/09/05/ukraine-war-highlights-new-role-for-loitering-munitions/
- https://www.armadainternational.com/2025/03/ukrainian-atlas-cuav-system-electronic-warfare/

Acoustic and low-observable design
- https://www.ll.mit.edu/partner-us/available-technologies/toroidal-propeller-0
- https://www.uasvision.com/2023/01/30/toroidal-propellers-a-noise-killing-game-changer/
- https://link.springer.com/article/10.1007/s42401-025-00375-9
- https://www.defenseadvancement.com/suppliers/stealth-propellers/
- https://interestingengineering.com/military/worlds-first-hydrogen-drone-into-combat-zone
- https://www.zmescience.com/future/ukraine-world-first-hydrogen-drone/
- https://oem.flir.com/learn/discover/missionautonomous-pixel-lock-for-fpv-drones/

Lower-confidence outlets used only where marked [L]
- https://www.battlepolicy.com/gerbera/
- https://drone-warfare.com/counter-uas/countering-the-shahed-136/
- https://drone-warfare.com/counter-uas/acoustic-detection/
- https://www.globalmilitary.net/aircraft/italmas/
- https://ukraine-war-analytics.com/battles/trench-warfare-innovations-2026.html
- https://www.defenceukraine.com/en/insights/russia-air-defence-exhaustion-2026/
- https://www.techtimes.com/articles/321932/20260729/russia-pairs-jet-drones-radar-decoys-outrun-ukraines-interceptors.htm
