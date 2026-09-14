# Terrain, line of sight, and the engagement clock

*Research document for KILL ZONE, a real-time strategy video game. Nothing here
is operational guidance; it is input to a simulation with hit points, build
costs and cooldown timers.*

---

## How to read this document

Every substantive claim carries one of four marks:

- **[M]** — measured or specified. A published measurement, a standards figure,
  a vendor specification, or a piece of geometry that follows exactly from
  stated inputs.
- **[R]** — reported. Journalism, think-tank analysis, or official statements
  carried by several outlets.
- **[I]** — inference. My reasoning from [M] or [R] material, stated as such.
- **[E]** — estimate. A number I am handing the designer because the designer
  needs one and the literature does not supply it. Defensible, not sourced.

**A hard limit on all of it.** This environment's network policy denies outbound
HTTPS to every content domain; the gateway refuses the connection before the
site is contacted. WebSearch works and is the only channel. **Everything below
rests on search-result snippets, not on article text.** I have not opened a
single source. Where a figure appeared in several independent snippets I say so;
where it appeared once, I say that too. Where a snippet gave a number without
units, a date, or a statement of what was being measured, I flag it rather than
guessing the missing part.

**Scope.** This document was commissioned for terrain and line of sight, then
extended twice during the work: once to cover thermal occlusion and RF
control-link obstruction properly, and once to cover the engagement clock from
detection to intercept, including target evasion. It is therefore longer than
the original brief asked for. Sections 1–7 and 12 are the terrain brief;
sections 8–9 are the extensions; section 10 is the design recommendation that
everything else feeds.

**What this document does not re-do.** `radar-rf.md` finding 13 and §2B.5 have
already settled radar occlusion, and I treat that as decided rather than
re-litigating it. `ground-logistics.md` §8–§9 covers road netting, cut-off
positions and drone resupply. `front-2026.md` §10 covers position manning and
the 15 km approach march. `acoustic.md` §4–§5a covers acoustic propagation.
I cite all four rather than duplicating them.

---

## Summary of findings

1. **The game does not need an elevation model, and adding one would be a
   mistake.** [I, high confidence] The relief that exists in this theatre is
   real but slow: the Donets Ridge tops out at 369 m over a 370 km run, and the
   plain either side sits at 100–300 m. Over a tactical map of a few kilometres
   the natural vertical variation is metres. The vertical structure that
   actually breaks sight lines is **vegetation and buildings**: field
   shelterbelts 19–23 m tall, planted 800 m to 1.6 km apart across the whole
   agricultural east [R], and settlements every 2–4 km [R/I]. A tree line is
   taller than the terrain undulation it stands on. **Model the tree lines and
   the buildings; do not model the ground.**

2. **The one relief feature worth a tile class is the balka** — the dry steppe
   gully, 10–50 m deep, 100–500 m wide, 1–10 km long [R]. It is a *negative*
   feature, a linear trench the landscape provides for free, and it is where
   things hide and move. One tile class, "Cut", buys the whole of elevation's
   tactical effect without a height field.

3. **A single "blocked" flag is wrong for at least three of the five channels,
   and the differences are not subtle.** Optical is hard-blocked. Thermal is
   hard-blocked by the same solids *but not by the same obscurants*, and its
   relationship to foliage and to overhead cover is genuinely different from
   optical's. Radar is hard-blocked and should be absolute (already settled in
   `radar-rf.md`). Acoustic **diffracts**, losing 8–20 dB depending on
   frequency — it is attenuated, never cut. Passive RF is the least blocked, but
   *for reasons that do not transfer to the control link*.

4. **The acoustic diffraction figures in `acoustic.md` §5a are very close to
   right, with one correction.** ISO 9613-2 caps screening attenuation at
   **20 dB for a single barrier and 25 dB for multiple diffraction** [M], and
   practical outdoor barrier insertion loss is quoted at **20–24 dB** [M]. The
   frequency dependence is correct in shape and sign: a fixed detour is many
   wavelengths at 4 kHz and a fraction of one at 63 Hz [M], so the same screen
   shields high frequencies far better. **Change the 22 dB term to 20 dB** for a
   single obstacle and allow 25 dB only where two obstacles stack. The 8 dB
   low-band and 15 dB mid-band terms stand.

5. **The brief's claim that passive RF is "least blocked of all" is right for
   ESM and wrong for the control link, and the game is missing the difference.**
   Both are radio at the same frequencies and take the same diffraction loss.
   They differ in *margin*. An ESM receiver needs to detect energy; an FPV video
   link needs to carry a live picture. A 15–25 dB diffraction loss reduces an
   ESM detection range but leaves a contact; the same loss puts a video link
   into static. Snippets are unusually blunt about this: *"Where your eyes lose
   the drone behind a ridge, your control link often dies with them"*; *"clear
   visual sightline ≈ healthy link; grazing sightline ≈ marginal link;
   terrain-blocked ≈ dropout"* [R].

6. **Relay drones and masts exist precisely to restore line of sight, and in the
   game they only extend range. That is the single most significant missing
   mechanic this document found.** Fielded systems: an airborne repeater kit in
   Ukrainian and Lithuanian service quoting **25 km in mixed terrain** and
   describing its own purpose as *"elevating the FPV control and video link
   above terrain obstacles — forests, hills, urban structures"*; an aerial
   repeater quoting **up to 15 km**; telescopic ground masts of **4 m (13 kg)
   and 6 m (19 kg)** deployable in **1–2 minutes** [R]. The game has
   `MeshRepeater`, `MeshRangePerHopMetres = 700` and `MeshMaxHops = 4` — all
   range, no geometry.

7. **Fibre-optic drones are immune to occlusion of the control link, and that is
   most of why they exist.** *"Fibre-optic drones can fly low between buildings
   or through forests without losing signal"*; *"traditional radio-controlled
   FPV drones find it very difficult to descend into the middle of forests
   because the radio signal disappears"*; a fibre drone *"can sit powered down
   in a treeline for hours waiting to ambush a vehicle"* [R]. The game already
   prices fibre's cost (snag rate in Forest 0.040/s, PowerLine 0.090/s). It does
   not yet pay fibre its benefit, because there is nothing to be immune to.

8. **Overhead cover is the defining fortification fact of 2026, and it is
   thinner than it sounds.** Reported practice: **20–40 cm of compacted earth
   over steel reinforcement** on fighting positions [R, single aggregator
   source — treat as indicative]; Russian command posts at **about 3 m of
   overhead cover** with concrete slabs, timber decking and packed earth [R].
   The design doctrine now states *"robust overhead cover for all elements above
   ground"* and internal trench segmentation [R, ICDS]. But reporting also says
   plainly that positions built in open fields *"cannot actually be used by
   soldiers"* [R] — cover you cannot reach is not cover.

9. **Tunnels are a real category but a small one, and the honest word is
   "exceptional, recurring".** What exists: several kilometres of **1.4 m
   diameter sewer pipe** used to infiltrate Avdiivka [R]; the Sudzha gas
   pipeline infiltration [R]; utility and sewer movement in Toretsk [R];
   purpose-dug tunnels near Avdiivka and in the Kharkiv direction [R]; and, as
   deliberate construction, **2 km of linked underground infrastructure joining
   12 underground positions** built by Ukrainian construction firms [R]. That
   last is the one the game should model. It is engineering, not caves.

10. **Rubble is not a weaker building; it is a different material.** Satellite
    assessment of Bakhmut: **97% of multi-storey housing destroyed**, and of 344
    buildings assessed **6.7% irrecoverable ruin, 2.6% still functional** [R].
    The physical change is specific: *"entire rows of apartment buildings
    gutted, just the outer walls left standing and the roofs and interior floors
    gone"* [R]. **Walls survive; roofs do not.** Rubble therefore still blocks
    horizontal sight lines and still blocks radio, but it has stopped providing
    overhead cover. That is a clean, defensible, one-line game rule.

11. **On the engagement clock: reaction time is small next to flight time, and
    detection range is the binding constraint — but only for the class of
    engagement the game is modelling.** Ratios, since absolute times cannot
    transplant: interceptor pre-launch latency runs on the order of **one tenth
    to one sixth of the interceptor's flight time** against a high-altitude
    crosser [I from M/R inputs], while against a short-range FPV the same
    latency is **the whole engagement** [R: "three seconds from detection to
    decision to destruction"]. A published counter-UAS figure of **15–40 seconds
    total latency from first detection to defeat** [R] sits against interceptor
    flight times of minutes.

12. **The decisive geometric fact about a slow interceptor is not turn rate. It
    is an angle, and it is exact.** With pursuer speed `v_p` and target speed
    `v_t`, the Apollonius construction gives a capture region that exists only
    if the target's heading lies within **±arcsin(v_p/v_t)** of the bearing to
    the interceptor [M, classical pursuit geometry]. At the game's 34 versus 55
    that angle is **38.2°**. A jet target flying more than 38° off a
    head-on course **cannot be intercepted at all**, at any range, by any
    guidance law, regardless of how badly it turns. This confirms the
    hypothesis put to me: **evasion effectiveness scales with the speed ratio,
    not with turn rate.** It is also one dot product per pair, which is the
    cheapest correct mechanic in this whole document.

13. **Real-world Geran drones do now react to being engaged, so the "no threat
    warning" assumption needs correcting.** Not by radar warning receiver — by
    **rearward-facing cameras feeding a mesh modem**, observed on Geran-2 from
    around **late 2025** and described in mid-2026 analysis of a recovered
    "Seeker" variant with a dual-camera arrangement [R]. Reporting explicitly
    frames the rear camera as enabling *"evasive manoeuvring from
    interceptors"* [R] and separately notes added **infrared countermeasures**
    against interceptor seekers [R]. Pre-planned routing is also real and is
    the older, better-attested behaviour: circuitous routes, corridors chosen by
    prior reconnaissance, and in-flight reprogramming over a data link [R].

14. **My recommendation on the big question: do not build per-pair per-channel
    line of sight. Build a per-tile occluder height and a precomputed
    per-observer visibility stamp, run it on the existing unused 16 m vision
    grid, and apply it to three channels out of five.** Full detail in §10. The
    short version is that the correct abstraction here is not cheaper
    ray-casting; it is *fewer rays*, because occlusion in this theatre is
    dominated by a small number of long linear features whose shadows are
    stable for many seconds.

---

## 1. Elevation, and whether this theatre has any

### 1.1 What the relief actually is

The East European Plain reputation is earned. Donetsk and Luhansk oblasts sit
**100–300 m above sea level** across most of their area [R]. The one genuine
upland is the **Donets Ridge**, described as the easternmost upland in Ukraine,
running **370 km northwest to southeast** and **50–120 km wide**, with its
highest section (the Debaltseve–Ivanivka ridge) **above 300 m** and a high point
**Mechetna Mohyla at 369 m**; the ridge reaches "up to 350 m" and falls away to
**150 m** north and south [R, encyclopaedic sources, consistent across
snippets].

Do the arithmetic the designer needs. A 200 m rise spread over 50 km of ridge
width is an average gradient of **0.4%** — 4 m of rise per kilometre [M, from
those figures]. Over a tactical map of 3–4 km that is **12–16 m of total
elevation change**, and it is *smooth*. A smooth 15 m rise across 4 km does not
block anything: the sight line from a 10 m mast to a 2 m target 3 km away clears
it comfortably.

So at map scale the theatre has relief. At drone scale it does not. Both claims
in the brief are true and they resolve the same way: **the relief is real, and
it is irrelevant to line of sight at the scale this game plays at.**

### 1.2 The exception: local dissection

The one qualifier is erosion. The Donets Basin is described as *"an undulating,
monotonous plain with a maximum elevation of 369 m, frequently dissected by
gullies and depressions 100 m and more in depth"* [R, encyclopaedic]. **I flag
that 100 m figure as doubtful in context** — it fits a river-valley scarp
(the Donets valley itself, where the ridge "drops abruptly"), not a field gully,
and the snippet does not say which it means.

The better-scoped figure is the **balka**, the standard steppe dry valley:
**depths of 10–50 m, widths 100–500 m, lengths 1–10 km or more** [R, two
independent snippets broadly agreeing, one giving "several metres to tens of
metres"]. That is the relief a soldier experiences.

A balka is not a hill. It is a linear slot in flat ground. Its tactical effect
is entirely different from a ridge's:

- It **does not cast a shadow** across the landscape the way a ridge does.
- It **hides what is inside it** from ground-level observation completely, and
  from shallow-angle aerial observation substantially.
- It is a **movement corridor**, which is exactly how reporting describes
  Shahed routing: drones *"routing along highways, riverbeds, and estuaries,
  using the terrain that humans built roads alongside to mask the approach"*
  [R].

**[I, high]** For the game, a balka is a tile class, not a height. A unit in a
"Cut" tile is invisible to ground-level sensors outside the cut and has reduced
signature to shallow-angle air sensors. That captures the whole of its
behaviour with a byte.

### 1.3 Where relief does matter, it matters because it is scarce

The scarcity is the point. Because the ground is flat, the few places that are
not flat become disproportionately valuable. Chasiv Yar is the clearest case:
described as sitting on *"an elevated limestone plateau rising some 80–100 m
above the surrounding agricultural lowlands"*, average elevation 213 m, giving
*"direct line-of-sight to Kramatorsk approximately 12–15 km to the northwest"*
and *"artillery and drone dominance over approaches"* [R, though from an
analytics aggregator whose reliability I cannot check — the 80–100 m relative
height is the load-bearing number and is consistent with the 213 m absolute
against a ~120 m lowland].

ISW-derived analysis makes the general version: the fortress belt sits in an
area with *"steeper slopes, which provides Ukrainian defenders the ability to
rely on tactical heights"*, while west of it the ground is flat and leads to the
Dnipro lowland, so that losing the belt would leave Ukraine *"defending from
this lowland, giving the Russians a height advantage"* [R].

**[I]** This is a *campaign-map* fact, not a tactical-map fact. It belongs in
mission design — "this map contains the only high ground for 40 km, and both
sides know it" — not in a physics model. The game can deliver the entire feel of
it by making one or two map features exceptional, without a height field
anywhere.

### 1.4 Reverse slope

I searched specifically for reverse-slope positioning in this war and **found
essentially nothing**. Searches for reverse slope combined with drones, terrain
masking and FPV returned material on FPV radio horizon and treeline ambush but
no discussion of reverse-slope tactics. I am not going to manufacture a finding.

**[I, medium]** The reason is probably that reverse slope is a doctrine for
defeating *direct-fire and flat-trajectory observation from a specific
direction*, and the dominant threat here observes from **above**, from a drone
that can simply fly over the crest. Against a sensor at 500 m altitude a reverse
slope is not a reverse slope. What replaced it functionally is overhead cover
and treeline siting, both of which are heavily attested (§3, §5).

**Design conclusion for item 1. The game should not add elevation.** Add tile
classes that encode the vertical structure that matters — tree lines, buildings,
rubble, cuts — and give each an *occluder height* in metres. That is a
per-tile byte, it is what the ray needs anyway, and it produces the right
answers without a heightmap, a slope model, or a single extra square root.

---

## 2. Line of sight, channel by channel

The five channels in `World.ComputeDetection` are ESM, Radar, Thermal, Acoustic
and Optical. They do not occlude alike.

### 2.1 Optical — hard blocked, and the reference case

Optical is the easy one. Anything opaque in the visible band blocks it
completely. There is no diffraction worth modelling at these wavelengths and
these ranges: light does not bend round a tree line.

Two refinements the game should carry:

- **Smoke and dust block optical and do not block thermal.** *"Thermal energy
  passes through many visible obscurants including smoke, dust, light fog"*
  [R]. Smoke is in active use as an anti-FPV measure — the Ukrainian 902
  "Tucha" and M257 systems are named in reporting, and against fibre-optic
  drones smoke is described as one of the few things that works, because it
  *"blinds the fibre-optic drone's camera, eliminating the very video advantage
  that makes them effective"* [R]. `decoys-masking.md` does not cover smoke at
  all — I checked, the word does not appear — so this is an open gap somebody
  should fill.
- **Optical occlusion is partial far more often than binary.** A tree line in
  leaf is not a wall; it is a statistical screen. See §5.

### 2.2 Thermal — the genuinely unresearched channel

`thermal-optical.md` is thorough on radiometry, contrast, clutter and crossover,
and says nothing about anything being in the way. Here is that material.

**(a) Thermal is blocked by the same solids as optical.** Earth, masonry,
concrete, steel and timber are opaque in both 3–5 µm and 8–14 µm. A berm, a
parapet, a wall, the far lip of a balka: all hard blocks on both channels. There
is no thermal equivalent of "seeing through the wall", and marketing language
that implies otherwise is describing obscurants, not solids. **[M, physics;
uncontested in every snippet returned]**

**(b) It is *not* blocked by the same non-solids.** LWIR passes smoke, dust and
light fog where visible does not [R]. This is the first real per-channel
divergence and it is worth a game rule: **a smoke tile should zero optical and
leave thermal near-intact**, while a heavy-fog weather state degrades both (the
existing `WeatherScale` already separates these).

**(c) Foliage: thermal is *worse* than optical at seeing through a canopy, not
better.** This is the correction I most want on record, because the intuition
runs the other way. The snippets are consistent: thermal passes *"light
foliage"*; *"if foliage is dense enough to block the thermal signature of what
is behind it, the camera cannot see through it"*; and on the concealment side,
*"trees overhead can help break up an infrared signature, especially under a
heavy canopy of leaves"*, with the caveat that *"gaps, movement, warm air, and
edge leakage can still reveal temperature differences"* and that thermal
*"detects heat signatures through gaps between obstacles such as foliage"* [R].

The physics behind that [I]: a leaf is opaque in LWIR and, more importantly, a
sunlit leaf is *hot*. Canopy in summer is not a neutral screen — it is a bright,
high-variance thermal clutter field sitting between the sensor and the target.
Optical at least gets colour and shape cues through gaps; thermal gets a
warm, textured mess. **[I, medium-high]**

**(d) The overhead-cover case, which is the one that decides whether a trench
works.** This is a different geometry from the ground case and deserves its own
treatment.

A roofed dugout with 20–40 cm of earth over it [R] is thermally opaque *as a
roof*. Earth has high thermal mass; a body under half a metre of soil produces
no detectable surface anomaly on any tactical timescale. **[I, high]** So
overhead cover defeats overhead thermal — for the covered part.

What it does not defeat:

- **The entrance.** Every covered position has openings, and reporting on
  countermeasures against pipeline infiltration describes Ukrainian units
  *"monitoring entrances and exits with drones and thermal surveillance for ad
  hoc ambush opportunities"* [R]. The entrance is the signature.
- **The stove and the vent.** Underground fortification descriptions
  consistently list **ventilation, heating, boilers** as standard fit [R]. A
  heated, ventilated dugout in winter exhausts warm air. **[I, high]** This is
  the single best "you dug in but you still glow" mechanic available.
- **Anything that goes outside.** The hard limit on a covered position is that
  someone has to come out. Drone-operator reporting: once troops *"leave cover,
  they are often spotted within minutes and engaged"* [R].
- **The approach.** Winter thermal is described as following *"footsteps in the
  snow"*, which *"carry a different thermal signature than the surrounding
  area"* [R]. A Russian milblogger quoted in the same reporting: *"the
  battlefield is controlled not by the soldier's eyes, but by the matrix of a
  thermal imager mounted on a UAV"* [R, quoted in Forbes Jan 2026].

**(e) Residual heat is a real thermal-only phenomenon and the game has nothing
like it.** Peer-reviewed work on heat-trace tracking establishes that residual
surface temperature decays exponentially per Newton's law of cooling, and that
thermal cameras capture ground heat traces invisible to the eye [M]. Applied
material adds that *"a sensitive thermal sensor can detect a warm track in cool
soil or the residual heat from a recently passed subject"* [R], and that vehicle
*"engine compartment and exhaust temperatures remain high"* [R].

**[E, designer's mechanic]** Give thermal a **decaying ground stain**: when a
vehicle or a crewed position leaves a tile, that tile carries a thermal
signature of, say, 40% of the departed unit's, decaying exponentially with a
half-life of a few tens of seconds in game time. It is cheap, it is real, it
rewards thermal reconnaissance with something optical cannot give, and it
creates the "they were here, recently, and went that way" read that drone
operators actually get.

**(f) Camouflage nets.** Multispectral nets are real and fielded (a 2×2 m
reversible soldier net managing visible, NIR and thermal contrast is described
as *"a blanket that buys you time"* for short movements between covered
positions [R]). The published limitation is specific: *"camouflaging performance
becomes problematical when there are hot spots, such as the engine of a motor
vehicle, which can be localised by a thermal imager despite any camouflage net
placed on top"* [R]. **[I]** Net = large thermal reduction for a cold or
shut-down unit; small reduction for a running one.

**(g) One counterintuitive effect worth knowing.** Thermal loses the background
texture that optical uses. *"Landmarks such as trees, trails and contour
features are often lost. The loss of heat in background objects reduces scene
clutter, such as trees and rocks, and can increase target detection"* [R]. So a
warm target *partially* screened by cold winter vegetation can be **easier** on
thermal than on optical, because the screen is invisible and the target is not.
**[I, medium]** This argues for making thermal's foliage penalty
**season-dependent in the opposite direction from optical's**: optical is helped
by bare winter branches only a little; thermal is helped by them a lot.

### 2.3 Radar — settled elsewhere, reconciled here

`radar-rf.md` finding 13 and §2B.5 have this. I found nothing that contradicts
them and one thing that reinforces them, so I am restating and endorsing rather
than redoing.

- The geometric horizon is a red herring: 4/3-earth gives **20.2 km** for a 10 m
  mast against a 3 m target and **35.6 km** against a 30 m one, never binding at
  counter-drone ranges [M, `radar-rf.md` §2B.5].
- The binding constraint is **local masking**, and the anchor case is exact: a
  **10 m mast with a 15 m treeline at 1 km** has a masking ray rising at 0.29°,
  so **everything below 35 m out to 5 km, and below 60 m out to 10 km, is
  invisible** [M].
- Occlusion on this channel is **absolute, not a multiplier** — *"this is
  optics, not attenuation"* [`radar-rf.md` §2B.5].
- Ground targets are masked far more aggressively than air targets, and
  occlusion against ground targets *"should be absolute"* [`radar-rf.md` §2].
- The per-terrain clutter table (σ0 from −45 dB over water to −8 dB urban,
  giving radar reach multipliers **1.20 / 1.00 / 0.75 / 0.65 / 0.45**) is
  already derived and should be used as-is.

**The reinforcement.** Independent counter-UAS material says the same thing in
operational language: *"military battlefield radar systems are not capable of
detecting slow-moving targets at relatively low angles or altitudes above
buildings, trees and just above the horizon"*; low-altitude attack drones *"fly
below the radar at altitudes under 100 m, taking advantage of ground clutter and
propagation fading"*; and a single fixed radar-RF-EO-IR node gives *"meaningful
detection coverage out to roughly 2–5 km depending on local clutter"* [R].
That 2–5 km practical figure against a 20 km geometric horizon is the clearest
possible statement that horizon is not what limits these systems.

One operational detail worth adding to the game: published Geran-2 usable
altitude band is **60 to 4,000 m**, and *"extremely low ingress, sometimes under
100 feet and through terrain or between buildings, defeats radar line of sight
and shortens the engagement window to seconds"* [R]. Low ingress is a *choice*
with a cost, and the game's Layer enum already has the right three rungs to
express it.

### 2.4 Acoustic — the only channel that genuinely diffracts

**Verifying the 8/15/22 dB figures.** `acoustic.md` §5a applies −8 dB below
250 Hz, −15 dB to 1 kHz, −22 dB above, when `terrain_blocked`. Against the
standards:

- ISO 9613-2 limits screening attenuation to **20 dB for a single barrier** and
  **25 dB for multiple diffraction**; those bounds are unchanged in the 2024
  revision [M, multiple independent snippets].
- Practical outdoor barrier insertion loss is quoted at **20–24 dB** [M].
- Maekawa's construction makes the loss a function of the Fresnel number
  `N = 2δ/λ`, so attenuation rises with frequency for a fixed path detour:
  *"the same wall shields high frequencies far better than low ones because a
  fixed detour is many wavelengths at 4 kHz but a small fraction of one at
  63 Hz"* [M].
- Grazing incidence (`N = 0`) gives **6 dB**; `N = 1` about **16 dB**; `N = 2`
  about **22 dB** [M, from the closely analogous knife-edge diffraction
  tabulation].

**Verdict: the shape is right, the low band is right, the high band is 2 dB
optimistic.** Recommended correction, and it is small:

| Band | `acoustic.md` §5a | Recommended | Basis |
|---|---|---|---|
| ≤ 250 Hz | −8 dB | **−8 dB** — keep | Low `N`, grazing-to-modest diffraction [M/I] |
| ≤ 1 kHz | −15 dB | **−15 dB** — keep | Mid-range `N` [M/I] |
| > 1 kHz | −22 dB | **−20 dB** | ISO single-barrier cap [M] |
| Two stacked obstacles | not modelled | **−25 dB cap** | ISO multiple-diffraction cap [M] |

**Why this matters more than 2 dB suggests.** It is the *differential* that
creates gameplay. A Shahed-class piston engine radiates at 80–250 Hz
[`acoustic.md`], so a screen costs it **8 dB** — roughly a 60% range reduction
on a 20 log R law, still a solid contact. A small electric quad lives at
1.5–6 kHz, so the same screen costs it **20 dB** — a **90% range reduction**.
**[M/I]** One tree line therefore roughly halves your detection range against
the loud thing and nearly erases it against the quiet one. That is a real,
physically-grounded asymmetry and it is exactly the kind of thing that makes a
sensor model feel alive.

**Two operational confirmations.** *"A fundamental limitation of ground-based
acoustics is terrain masking, where elevated sensors hear over obstacles that
block ground-level propagation"* [R] — which is a direct argument for mast
height on the acoustic channel too, not just radar. And the counter-case:
*"line-of-sight sensors like radar and cameras may be obscured by mountains, but
acoustic sensors can be directed toward valleys, allowing them to detect UAS
before line-of-sight sensors"* [R]. Both are true and they are the same fact:
acoustic degrades where the others fail outright.

**Foliage attenuation of sound** is a separate, smaller term. ISO 9613-2 states
that foliage gives *"a small amount of attenuation and only if it is
sufficiently dense to completely block the view along the propagation path"*
[M]. The mechanism is *"mostly scattering from the trunks and limbs; sound
absorption by leaves is not a significant contributor"* [M] — which means, for
the game, that **forest acoustic attenuation is nearly season-independent**,
unlike every other channel. I could not retrieve the ISO Table A.1 values per
100 m and I am not going to invent them; the Hoover form `A = 0.01 · r · f^(1/3)`
appeared in one snippet [M, unverified transcription] and gives, for 200 m of
forest at 1 kHz, about 20 dB, which is the right order.

### 2.5 Passive RF and the control link — one physics, two consequences

This is where the brief's original framing needs correcting, and the correction
opens up a mechanic.

**The shared physics.** Control links and the ESM receivers that hear them use
the same bands. FPV video and control run at **2.4 GHz and 5.8 GHz**, with
900 MHz used for control where longer reach matters [R]. Both take the same
diffraction loss over the same obstacle. Knife-edge diffraction gives, as a
function of the Fresnel parameter ν: **0 dB well clear, 6 dB at grazing, ~16 dB
at ν = 1, ~22 dB at ν = 2** [M]. The 60%-of-first-Fresnel-zone clearance rule is
the standard threshold for "negligible diffraction loss" [M].

**Consequence one: the control link behaves like optics.** The snippets are
unusually direct about this and they agree:

- *"Both 2.4 GHz and 5.8 GHz RF signals are LOS signals, which means they will
  not function if there is any barrier between the drone and the remote
  control."*
- *"Where your eyes lose the drone behind a ridge, your control link often dies
  with them."*
- *"Control and video links at 2.4/5.8 GHz behave optically over these
  distances, so clear visual sightline ≈ healthy link; grazing sightline ≈
  marginal link; terrain-blocked ≈ dropout."*
- *"When terrain blocks the straight path, the link attenuates sharply or drops,
  typically triggering return-to-home."*
- *"5.8 GHz does not penetrate foliage, buildings or terrain well. If you fly
  behind a tree, your video will break before your RC link does."*
- *"900 MHz outperforms 2.4 GHz in obstructed environments because lower
  frequencies diffract around obstacles better."* [all R]

So: **"needs line of sight" is an approximation, but a good one** — there is
signal in the shadow, and it is 15–25 dB down, and a live video link does not
survive that. **It is not a hard cut in physics; it is a hard cut in
consequence.** [I, high]

Vegetation makes the same point quantitatively. ITU-R P.833 specific attenuation
through trees in leaf: **0.3–0.8 dB/m at 2 GHz, 0.5–1.5 dB/m at 5 GHz** [M,
single snippet, values not independently cross-checked]. In-leaf attenuation is
*"about 20% greater (dB/m) than for leafless trees"* at ~1 GHz [M], with a
broader claim of **3–10 dB higher with full foliage than bare branches** [R].
At 0.8 dB/m, **20 m of tree line costs 16 dB at 2.4 GHz** — one shelterbelt is
roughly one knife-edge. **[M/I]**

**Consequence two: passive ESM shrugs it off, and the reason is margin, not
propagation.** `radar-rf.md` §3 already establishes the one-way-versus-two-way
advantage over radar (the 6:1 intercept-range factor, R ∝ √P). The additional
argument here is different and specific to occlusion:

- An ESM receiver is performing **energy detection in a narrow band** with a
  quoted analogue-receiver sensitivity floor around **−85 to −93 dBm** [R].
- An FPV video link is carrying a **live analogue or digital picture over
  megahertz of bandwidth**, and needs tens of dB more SNR to do it.
- The same 20 dB diffraction loss therefore lands in completely different places
  on the two systems' margin curves. On ESM, R ∝ √P, so 20 dB costs a factor of
  **10 in range** — but from a baseline that was enormous. On the video link, it
  costs the picture.

**[I, high confidence — this is reasoning from established physics, not a
sourced finding]** The honest statement for the designer is: *passive RF is not
less attenuated by terrain; it is less **defeated** by terrain, because it has
tens of dB more margin to spend.* That is why the brief's original claim is
right in effect and wrong in mechanism, and the mechanism is what tells you how
to model it: **ESM gets a range multiplier when occluded, not a cut. Control
links get a cut.**

**Consequence three, the missing mechanic: relays restore geometry.** The
purpose of an airborne repeater is stated plainly by its makers and by reporting:

| System | Figure | Character |
|---|---|---|
| Airborne Drone Repeater Kit (repeater on a carrier drone + ground antenna rack); in combat use with Ukrainian units, in service with Lithuania | **up to 25 km in mixed terrain**, *"stable line-of-sight maintained from altitude"*, purpose given as *"elevating the FPV control and video link above terrain obstacles — forests, hills, urban structures, fortified positions"* | [R, vendor-sourced reporting] |
| Aerial FPV repeater on a carrier drone | **up to 15 km depending on conditions, terrain and configuration** | [R, vendor] |
| Ground station / repeater family | **up to 25 km depending on terrain, antenna height and equipment** | [R, vendor] |
| Telescopic "smart masts" | **4 m / 13 kg** and **6 m / 19 kg**, deployed in **1–2 minutes** by electric drive, with tilt-and-swivel antenna positioning | [R] |
| Russian airborne relay programme | claimed **4× range** for fibre-optic FPV | [C, claim] |

And the mechanism, from reporting rather than marketing: *"a repeater drone
bypasses radio horizon obstacles by transmitting the signal to the operator as
if it were a communications tower. Due to its high altitude, the repeater
remains in the visibility zone of both the operator and the main drone"* [R].

**[I, high]** In the game this should be: a mesh hop is valid only if **both
legs have line of sight**, and an elevated node (a mast, or a repeater at
Layer.Low/High) has line of sight to almost everything. That single change turns
`MeshRepeater` from "+700 m per hop" into "the thing you put up to see into the
next valley", which is what it is for. It also gives the 6 m mast a reason to
exist that is not a number on a card.

**Consequence four: fibre is the anti-occlusion link.** Fibre is unjammable
(`UnjammableRobustness = 255`) and the game already prices its snag cost. What it
does not yet get is immunity to terrain: *"fibre-optic drones can fly low
between buildings or through forests without losing signal, which means areas
previously safe from drone attacks can now be hit"*; *"traditional
radio-controlled FPV drones find it very difficult to descend into the middle of
forests because the radio signal disappears"*; and the tactically decisive one,
a fibre drone *"can sit powered down in a treeline for hours waiting to ambush a
vehicle"* [R]. Fibre's counterpart cost is also terrain-coupled: *"to maximise
range, operators need to fly low and in a straight line to prevent the cable
from sagging or snagging on obstacles"* [R], which is precisely what
`SnagRatePerSecond` models.

**Net effect on `LinkKind`:** once occlusion exists, the five rungs
differentiate properly for the first time. Radio dies behind terrain. Mesh
survives if you pay for altitude. Fibre ignores terrain and pays in snag.
Satellite ignores terrain entirely (it looks up). Autonomy has no link to lose.

### 2.6 The per-channel occlusion table

**[E, the designer's summary]** Occluder classes as recommended in §10.

| Channel | Behaviour at an occluder | Recommended rule |
|---|---|---|
| **Optical** | Hard block by solids; not blocked by thermal-transparent obscurants; partially blocked by foliage | **Binary block**, with a foliage class that is probabilistic rather than absolute |
| **Thermal** | Hard block by the same solids; *passes* smoke/dust/light fog; *worse* than optical through summer canopy, better through bare winter canopy; residual heat leaks past cover | **Binary block** for solids, **separate foliage term with opposite seasonal sign to optical**, plus a heat-stain and a vent/entrance leak |
| **Radar** | Hard block; already settled | **Absolute**, per `radar-rf.md` §2B.5. Plus the σ0 terrain reach multipliers |
| **Acoustic** | Diffracts. Never cut | **−8 / −15 / −20 dB by band**, −25 dB cap for stacked obstacles; forest attenuation nearly season-independent |
| **Passive RF (ESM)** | Diffracts; huge margin absorbs it | **Range multiplier ≈ 0.3–0.5 when occluded**, never a cut [E] |
| **Control link (not a sensor channel, but the same physics)** | Diffracts; no margin | **Binary drop** for Radio; restored by an elevated mesh node; **ignored entirely by Fibre and Satellite** |

