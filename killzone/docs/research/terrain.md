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

---

## 3. Trenches and field fortification

### 3.1 What changed, and why

The short history from the reporting: *"by late 2022 and into 2023 it became
clear that many early positions were too shallow and exposed, and once FPVs
became widespread, brigades started asking for deeper shelters, stronger
overhead cover and better camouflage"*; *"with drones, FPVs and guided bombs
constantly in the air, trenches must now be deeper, stronger and properly
camouflaged"* [R]. The ICDS assessment frames 2025 as the year Ukraine
*"discarded Cold War-era doctrines to embrace a paradigm of scattered,
low-observable and fluid strongholds supported by distant fire control"* [R,
ICDS Oct 2025].

The design principles ICDS lists are worth quoting because they are the
specification a game can build from: *"robust overhead cover for all elements
above ground, blast entrances, and the internal segmentation of trenches to
mitigate shrapnel and blast effects"*, plus *"multiple concealed entry and exit
points to covered or subterranean passages, and as great a shelter depth as
local hydrogeological conditions permit"*, and — the one that connects
fortification to terrain — *"careful siting of positions within natural cover
like tree lines, and the construction of access and egress routes to permit safe
manoeuvre"* [R].

**That last clause is the design-relevant one.** A position is not a thing you
place on open ground. It is a thing you place *in a tree line*, with a *covered
approach*. Reporting on failure says the same from the other side: Ukrainian
journalists explaining that some fortifications *"proved ineffective because
they are built in open fields and cannot actually be used by soldiers"* [R].

### 3.2 Dimensions and overhead cover

I looked hard for current Ukrainian trench profile dimensions and **did not find
them**. Searches returned WWI figures (which I am discarding as irrelevant) and
2026 commentary without numbers. What I did find:

| Figure | Value | Confidence |
|---|---|---|
| Overhead cover on fighting positions | **20–40 cm of compacted earth over steel reinforcement**, stated as sufficient to defeat drone-dropped grenades and PG-7 warheads | [R] — **single aggregator source, not corroborated. Treat as indicative, not measured.** |
| Overhead cover on Russian command posts | **~3 m**, reinforced with concrete slabs, wooden decking and packed earth, *"intended to absorb the effects of air-delivered bombs"* | [R] |
| Prefabricated shelter module | corrugated steel, with drainage, ventilation, waterproofing and internal passageways | [R] |
| A specific modular build | six expanded steel bunkers, each **7.6 m long × 2.5 m wide**, assembled into an underground hospital | [R] |
| Materials for improvised overhead cover | timber, compacted earth, metal sheeting, lightweight concrete — *"survive most conventional munitions"* | [R] |

**The two-tier structure is the useful pattern.** [I] A fighting position gets
tens of centimetres — enough for fragmentation and a dropped grenade. A command
post gets metres — enough for a glide bomb. The game should have exactly two
cover tiers and not three, because that is what the evidence supports.

### 3.3 What overhead cover actually defeats

| Threat | Defeated by 20–40 cm? | Basis |
|---|---|---|
| Artillery fragmentation and airburst | **Yes** — *"defensive positions must have overhead protection to shield occupants from direct impacts and air bursts"*, and indirect fire is *"responsible for almost two thirds of casualties on both sides"* | [R] |
| Drone-dropped grenade (top attack, small shaped or frag charge) | **Yes**, and this is the stated design case | [R] |
| PG-7-class shaped charge dropped or flown in | **Claimed yes** | [R, same single source] |
| Overhead thermal observation | **Yes** for the covered volume; **no** for entrances, vents and anyone outside | [I, §2.2d] |
| Optical observation | **Yes** for the covered volume; **no** for the spoil — *"fresh soil mounds at the edges of fields"* are named as something drones observe | [R] |
| Purpose-built penetrator | **No.** A 2026 Ukrainian *"square, fence-post-like penetrator drone bomb"* with a reinforced nose and delayed-action fuse is described as designed to *"bust bunkers and overcome nets and cages"*, rendering previous shelters unsafe, and causing visible alarm on Russian channels | [R] |
| Thermobaric | **No.** *"The munition of choice for attacking troops in bunkers, trenches and buildings"*; precision allows one to *"fly into a bunker entrance"* | [R] |

**[I, high] The game rule this supports is a scissors:** overhead cover should
give a large, flat damage reduction against the *cheap, numerous* munitions and
essentially none against two *expensive, specific* ones. That makes cover a
real investment with a real counter, rather than a hit-point multiplier.

### 3.4 How much of a position is actually covered

The honest answer is that I could not close this. The strongest statement found
was *"all fighting positions now have reinforced overhead cover"* from the same
single aggregator [R, low confidence], against ICDS's normative *"robust
overhead cover for all elements above ground"* [R] — a requirement, not an
observation — and against a long tail of 2024–2026 reporting on inadequate
fortifications: *"relentless Russian shelling, lack of equipment and crippling
bureaucracy plague construction across the vast 1,000 km front"*; *"rudimentary
earthen fortifications, often with only a connecting trench for infantry to
reach firing positions, but little else"* [R].

**[E, and marked clearly as an estimate]** For a designer who must pick a
number: model a prepared position as **roughly 50–70% of its length under
overhead cover**, with the fighting bays and the rest bay covered and the
connecting runs open, and make the uncovered fraction a *buildable upgrade*
rather than a constant. That fits the evidence in both directions — doctrine
demands full cover, reality delivers partial — and it makes "finish your
position" a decision the player makes with time they may not have.

### 3.5 Digging: time and equipment

This is the best-sourced part of the fortification material.

| Task | Without machines | With machines | Source character |
|---|---|---|---|
| Infantry platoon bunker | **2 weeks, 8 people** | **~3 days, 3 people** | [R] — the same pairing appeared in two separate snippets, so it is at least consistently reported |
| One-man fighting hole (~60 ft³) | **2.5 hours, 1 soldier**; **1.25 hours, 2 soldiers** | — | [R, doctrinal rather than Ukrainian] |
| Continuous trench, purpose-built digger (BTM-3 class) | — | **~730 m/hour** at ~1.5 m deep × ~0.9 m wide (quoted as "800 yards an hour", "five feet deep and three feet wide") | [R] |
| Theatre-wide output | — | *"more than 300 excavators digging 23 km of fortifications a day across all sectors"* | [R, OSINT monitoring group — **claimed**, methodology unknown] |
| Equipment base | — | Soviet MDK-3 and BTM-3 trenchers, BAT-2; ~**500 engineering vehicles** from Western partners, including German Dachs | [R] |
| Funding | — | **₴17.5 bn** allocated by the Cabinet of Ministers for engineering and fortification works | [R] |

**The ratio that matters, and it is stark:** machines make a platoon position
**roughly 12× cheaper in man-days** (112 man-days → 9). **[M, arithmetic on the
reported pairing]** That is the single best argument for making engineering
vehicles a distinct, valuable, targetable unit class in the game rather than an
abstract build timer. It also explains why they are hunted.

**The counterweight.** Digging is done under observation. *"Speed is essential
in drone-saturated conditions"* [R], and once troops leave cover they are
*"often spotted within minutes and engaged"* [R]. An excavator is large, hot,
loud and stationary — in the game's own signature terms, close to a main battle
tank on thermal, acoustic and visual. **[I]** Building fortifications forward
should be a high-risk activity conducted at night, not a background process.

### 3.6 Anti-drone netting over trenches specifically

Road netting is covered in `ground-logistics.md` §8 and I am not repeating it.
Netting over *positions* is a different, smaller, older practice:

- Russian forces *"started covering trenches with anti-drone screens"*; both
  sides use *"protective netting over trench positions and physical anti-drone
  obstacles — 'dacha' frame structures over trench tops"* [R].
- *"Canopies and improvised screen protections over their positions... enclose
  trenches from above and the sides"* [R].
- The mechanism is entrapment or premature detonation: *"an incoming drone can
  be harmlessly captured, as flying into the net stops it without detonating its
  warhead. Soft structures are preferred as these can catch a drone; harder
  structures are more likely to set it off, but can be effective if they are far
  enough from what is being defended"* [R].
- Russian field manuals reportedly advise *"everything from fishing nets to
  chain-link fencing to purpose-built metal structures"* [R].
- Specific defeat: *"attacks by drones with vertically falling droppable
  grenades become more difficult, the same goes for manually directed FPV drone
  strikes"* [R].
- The counter is already fielded: the penetrator bomb described in §3.3 is
  explicitly designed to *"overcome nets and cages"* [R].

**[I] Netting over a position is a stand-off layer, not armour.** The right game
model is a **fixed probability that an arriving top-attack munition is stopped
or detonates early**, applied before damage, with the penetrator munition
bypassing it. A value of **0.35–0.55** is my estimate [E]; the sources support
"materially helps, does not solve".

**Does netting block sensors?** I found no source either way. **[I, medium]**
Physically, a nylon or chain-link mesh is a partial optical screen at shallow
angles and nearly transparent at normal incidence; it is transparent to LWIR and
to radio. So: a small optical concealment bonus, nothing else. I would not give
it more than that without evidence.

---

## 4. Tunnels and underground positions

**The honest characterisation is "exceptional but recurring, and now
industrialising".** It is not a Gaza-style tunnel war, and it is not a handful of
anecdotes either. Three distinct things are going on and the game should treat
them separately.

**(a) Using infrastructure that is already there.** This is the best-attested
and it is opportunistic:

- **Avdiivka, 2024**: Russian troops *"spent several days clearing and preparing
  an infiltration route through several kilometres of sewer pipe approximately
  1.4 m in diameter"* [R] — the dimension appears in two independent snippets.
- **Sudzha, March 2025**: *"Ukrainian forces were surprised by Russian soldiers
  who crept through an unused underground gas pipeline"* [R].
- **Toretsk**: *"the enemy trying to use underground utilities to enter the rear
  of Ukrainian troops on certain streets"* [R].
- **Kupiansk**: *"Russian troops using underground gas and water pipelines to
  bypass drone surveillance and frontline observation, forcing Ukrainian units
  to adapt by damaging, flooding, mining and fortifying the pipelines while
  monitoring entrances and exits with drones and thermal surveillance"* [R].
- **Azovstal, 2022**: an 11 km² complex with *"a massive, multi-level system of
  Soviet-era underground tunnels and bunkers"* [R] — the outlier case, and a
  steelworks, not a tunnel network anyone dug for this war.

**(b) Digging tunnels as an assault technique.** Attested but thinner: Russian
forces *"digging tunnels for logistical support or to carry out demolitions of
Ukrainian defensive structures"*, tunnelling in the Kharkiv direction, a 2018
Avdiivka industrial-zone tunnel [R]. One snippet headline records a Toretsk
tunnel assault being destroyed. **[I]** This is a sapper technique with a long
history and a poor success rate; I would not build a game system around it.

**(c) Deliberate linked underground positions — the one that matters.** This is
new, it is 2026, and it is engineering:

- Ukrainian construction firms built **2 km of defensive infrastructure linking
  12 underground fortified positions with internal communication routes**, using
  corrugated steel shelters with *"drainage, ventilation, waterproofing and
  internal passageways"*, reportedly already in use by troops [R, Jan 2026].
- Broader description: *"increasingly sophisticated underground systems
  connecting trench networks, fighting positions, command nodes and logistics
  hubs, equipped with fibre-optic communications, ventilation systems,
  reinforced concrete protection and concealed corridors"* [R].
- The stated purpose is unambiguous: *"tunnelling allows movement without
  exposure to drone observation"*; positions *"connect through tunnel systems
  designed specifically to defeat persistent drone surveillance, allowing troops
  to move between firing positions and command centres without exposing
  themselves to aerial observation"* [R].
- Russian higher headquarters are doing the same: *"drones and missiles made
  rear areas a myth, so HQs dig in underground"* [R].

**What underground defeats:** all five sensor channels, completely, for the
volume that is actually underground. What it does not defeat: entrances (see
§2.2d and the Kupiansk counter-tactic of watching exits with thermal), and the
approach to the entrance.

**[E] The game mechanic.** Not a tunnel *terrain type* — a **buildable link
between two structures** that (i) transfers units between them with no exposure
and no transit through the kill zone, (ii) costs a lot of build time and
engineering material, (iii) has two endpoints that are themselves detectable
structures, and (iv) can be severed by destroying either endpoint. That
reproduces all three reported behaviours — resupply, rotation without loss, and
command-post survival — with one system. It also directly addresses the
`ground-logistics.md` §9 finding that the binding constraint on a position is
*"whether anyone can physically reach it to relieve or evacuate"*.

Scale it by the reported figure: **2 km of link per 12 positions** [R] — so a
link should be short, expensive, and connect adjacent positions, not distant
ones.

---

## 5. Tree cover and forest

### 5.1 The geometry that makes this the most important section

Two numbers decide the whole terrain model for this game.

**Ukraine's field shelterbelts are spaced 800 m to 1.6 km apart** and **mature
windbreaks reach 19–23 m tall** [R, from agricultural and forestry sources;
the spacing figure appeared with the context that the belts were planted
perpendicular to prevailing winds under the 1948 planting programme, with
**446,000 ha of belts protecting 13 million ha of cropland** before 2022].

Put those against `radar-rf.md`'s worked masking case — a **15 m** treeline at
1 km from a 10 m mast blanks everything below 35 m out to 5 km — and the
conclusion is immediate and, I think, the single most useful finding in this
document:

> **[M/I] In this theatre a ground-based sensor of any kind can expect a 19–23 m
> vertical obstruction across its entire field of view every 800–1,600 m, by
> design, everywhere there is farmland. The terrain is not the occluder. The
> agriculture is.**

At the game's 12:1 compression, 800–1,600 m real is **67–133 map metres**. On a
map where the Gun Mount reaches 600 map metres optically and the Radar Mast
1,400, a sensor is looking through **five to twenty** tree lines at its own
maximum range. The reason the current flat-empty model produces recognisable
behaviour at all is that the designers tuned the ranges down to compensate
without knowing that was what they were doing. **[I]**

CEPA's framing of the southern battlefield matches: *"much of the southern
Ukrainian battlefield is flat and open, with dense forests sprinkled into the
landscape, with large fields separated by treelines. Treelines and forests are
where the Russian army has dug its forward defences, while tree cover hides
supporting arms including tanks and artillery"* [R].

And the tactical unit of the war is sized to it: *"fighting positions hidden in
tree lines can consist of just two fighting holes dug into the undergrowth, with
three riflemen in each, connected by two radios and supported by a stationary
drone"* [R] — which lines up precisely with `front-2026.md` §10's two-to-four
soldiers per position.

### 5.2 Per-channel, and seasonally

| Channel | Tree line in leaf (summer) | Tree line bare (winter) |
|---|---|---|
| **Optical** | Near-total block for a static target; partial for movement. *"Bare trees in winter offer fewer opportunities for concealment"*; *"leafless landscapes left Russian infantry exposed to Ukrainian drones"* [R] | Substantially degraded screen — trunks only. **[E] ~50–60% of the summer screening value** |
| **Thermal** | **Worse than optical**, per §2.2c — an opaque, sun-heated, high-variance clutter field | **Better than optical** — bare branches are thermally near-transparent and the loss of background clutter *"can increase target detection"* [R] |
| **Radar** | Clutter σ0 −15 dB; reach multiplier **0.65**; and as a physical obstruction, an absolute block [`radar-rf.md`] | Marginally better clutter; the **obstruction is unchanged** — trunks and branches block a 3 cm wave nearly as well as leaves do. **[I]** |
| **Acoustic** | **Nearly season-independent**, because the mechanism is *"scattering from the trunks and limbs; absorption by leaves is not a significant contributor"* [M] | Same |
| **Passive RF / control link** | ITU-R P.833: **0.3–0.8 dB/m at 2 GHz, 0.5–1.5 dB/m at 5 GHz** in leaf [M]; in-leaf ~**20% higher dB/m** than leafless at 1 GHz [M]; a broader claim of **3–10 dB more with full foliage** [R] | ~20% less per metre; **still enough to kill a link through 20 m of belt** |

**The seasonal inversion is the design gift here.** [I, high] Summer helps
optical concealment and hurts thermal detection; winter does the reverse. The
game already has a day/night cycle driving optical and thermal scalars in
opposite directions. A season (or a single "canopy state" flag on Forest tiles)
that does the same thing spatially is the same mechanic rotated 90°, costs
almost nothing, and reproduces a documented rhythm of the war: *"the leaves of
2022 protected the Ukrainians from the Russian onslaught; in the summer of 2023
the leaves gave the Russians protection from Ukrainian precision artillery and
drones"*, and the whole *"Russian obfuscation doctrine... relies on a single
factor — an eternal summer providing a thicket of protective leaves above"* [R].

### 5.3 Do drones fly under canopy, and what does it cost

Yes, and the cost is the link.

- *"Traditional radio-controlled FPV drones find it very difficult to descend
  into the middle of forests because the radio signal disappears"* [R].
- *"FPV drones struggle if the operator is behind a hill or if the drone flies
  into a building or deep into woods, because the radio signal is blocked"* [R].
- Fibre changes this completely: *"fibre-optic drones can fly low between
  buildings or through forests without losing signal, which means areas
  previously safe from drone attacks can now be hit"* [R].
- And it is not free: the Serebryanskyi forest is described as wrapped in
  discarded fibre — *"silvery strands drape over the treetops... trailing across
  branches, bushes and grass until they wrap the forest like a cocoon"* [R].
  That is the game's `SnagRatePerSecond(Forest) = 0.040` rendered in prose.

Forest fighting itself is attested as a distinct, slower, ambush-heavy regime:
*"the sandy ground and thick blanket of the Serebryanskyi forest obstruct a
Russian advance and provide cover from drones"*; *"where there is forest, it's
harder for Russians to advance, and easier for Ukrainians to defend"* [R].

### 5.4 What Forest should do in the game beyond snag rate

**[E] Six things, in priority order:**

1. **Occlude optical, thermal and radar** as a 20 m occluder.
2. **Attenuate acoustic** by the §2.4 band-dependent diffraction plus a
   per-metre forest term.
3. **Break Radio and Mesh control links** passing through it, and leave Fibre,
   Satellite and Autonomy untouched. This is the mechanic that makes fibre make
   sense.
4. **Carry a canopy state** (in-leaf / bare) that flips the optical and thermal
   terms in opposite directions.
5. **Reduce a unit's own signature while stationary inside it** — the treeline
   ambush is the single most-described tactic in the corpus.
6. **Keep the existing tether snag.** It is already right.

---

## 6. Urban and rubble

### 6.1 Intact settlement

Short sight lines are the defining feature, and the vertical dimension is now
genuinely used. Forbes, May 2026: *"FPVs are changing the rules of urban
warfare, with drones taking the lead instead of infantry. Increasing numbers of
videos show FPVs searching through buildings and trenches... videos show FPV
drones searching through large apartment blocks, with some equipped with thermal
imagers to locate enemies indoors. Small and agile, flying at 40 mph or more,
these drones are extremely difficult targets for small arms fire even at low
altitude. **Drones do not need stairs or ladders, and the sixteenth floor is as
accessible as ground level.**"* [R]

Sensor behaviour in the urban case:

- **Optical and thermal**: sight lines drop to street length. Micro-drones exist
  specifically to solve this — *"in the rubble of Bakhmut, micro-drones provide
  a bird's-eye view of whatever is around the next corner, over the next wall,
  or inside the next building"* [R].
- **Radar**: worst case on both terms. Clutter σ0 −8 dB gives reach ×**0.45** and
  edge reliability ×**0.40** [`radar-rf.md`], and independently, *"radar systems
  in urban environments often struggle to distinguish small plastic drones from
  birds or ground clutter — the urban canyon effect"* [R]. Multipath is the
  named mechanism: *"radars cannot avoid the dramatic fading effects of
  multipath when scanning the horizon"*, and the discrimination problem is
  *"a 1 m² moving target against a reflection potentially 115,000 times
  larger"* [R].
- **Acoustic**: clutter and reverberation, and the sources are specific.
  *"In urban areas, drones near activity are rarely the loudest source, with
  their signature potentially being partially masked, intermittent, and embedded
  in echoes and reverberation"*; *"buildings, vegetation and terrain features
  increase masking and multipath, allowing drones flying low and slow near
  roadways, industrial sites or urban noise to blend into the acoustic
  background"*; classical beamforming *"relies on simplifying assumptions — that
  the environment is anechoic... which frequently break down"* with *"multipath
  reflections from buildings"* [R]. Note that reverberation degrades
  *localisation* worse than *detection*, which fits `acoustic.md` §6's
  detection-versus-localisation split.
- **Control link**: *"signal can be easily blocked by trees, buildings, or even
  the pilot's body"* [R]. Urban is where fibre and relays earn their cost.

### 6.2 Rubble, which is a different material

This is the finding I would most want a designer to take away from §6.

Satellite assessment of Bakhmut: **97% of multi-storey housing destroyed**; of
344 buildings assessed, **6.7% irrecoverable ruin and only 2.6% still
functional** [R]. Mariupol: *"up to 90% of buildings damaged or destroyed"* [R].

And the structural specifics: *"entire rows of apartment buildings gutted, just
the outer walls left standing and the roofs and interior floors gone"* [R].

**[I, high confidence] That is a precise statement about which occlusions
survive destruction and which do not:**

| Property | Intact settlement | Rubble |
|---|---|---|
| Blocks horizontal sight lines | **Yes** | **Yes** — walls stand |
| Blocks overhead sight lines | **Yes** — roofs | **No** — roofs gone |
| Provides overhead cover from top attack | **Yes** | **No** |
| Blocks radio | **Yes** | **Mostly** — masonry remains |
| Interior fighting space, vertical | **Yes** — sixteen floors | **No** — floors gone |
| Ground mobility | Roads | **Degraded to impassable for vehicles** — rescue and resupply by vehicle into Bakhmut was cancelled and civilians told to *"flee by foot"* [R] |
| Acoustic character | Reverberant | **More** reverberant, more scattering surfaces, no absorbing interiors [I] |

The game has both `Rubble` and (implicitly) buildings-as-`Impassable`, and
currently distinguishes them only by tether snag rate. **Rubble should be the
tile that hides you from the side and not from above.** That is a genuinely
interesting tactical object: good against a ground assault, useless against a
drone, and it converts from the good version to the bad version when the
settlement is destroyed — which is something the player can cause.

---

## 7. Man-made masking that is not a fortification

### 7.1 Berms and revetments

Doctrinal, and the language is precise: *"proper placement of a berm or
revetment in front of an object can deflect or stop fragmentation and
direct-fired projectiles, and these structures may also be used to provide blast
protection"* [R, US engineering field guidance]. Aircraft revetments are
*"surrounded by protective blast walls on three sides, designed to shield parked
aircraft from blast overpressure, fragmentation and shrapnel, channelling
potential explosions upward and away"* [R].

**[I] What a berm stops, per channel:** everything at ground level from the
screened direction — optical, thermal, radar, control link — and nothing from
above. It is the purest example of *directional* occlusion in this document, and
it is the one object that makes an argument for direction-aware occlusion rather
than a simple "in cover" flag.

**[I] What it does not stop:** anything rising above it. A running engine's
exhaust plume, a heated shelter's vent, dust, and the vehicle's own antenna. A
berm plus a thermal sensor looking at the air above the berm is a real detection
mode and a nice tell.

### 7.2 Hardened shelters and cut-and-cover

Active construction, 2026, and the counter-drone motivation is explicit:
*"construction work at multiple bases has been adding many dozens of new
hardened aircraft shelters to better shield aircraft from drone attacks and
other indirect fire"*; four new arched aviation hangars begun late April 2026 at
Chkalovsk; reinforced hangars in Kaliningrad *"intended to reduce the
vulnerability of aircraft to potential attacks, including drone strikes"* — and,
usefully for this document, *"such shelters can also limit satellite and optical
reconnaissance"* [R].

**[I]** A hardened shelter is the only object in this document that provides
**both** complete occlusion on every channel **and** protection, at the cost of
being immobile, expensive and itself an obvious feature. That is a good building
in a game. It should be expensive, take a long time, and be visible from the
moment it exists.

### 7.3 Cope cages and vehicle screens

The consensus in the reporting is narrow and consistent:

**What they stop.** *"FPV drones frequently target thinner top armour and
vulnerable openings, and even relatively simple physical barriers can disrupt a
drone's fusing sequence or detonate a warhead before direct impact"*; they *"may
offer some level of protection against small bomblets dropped by commercial
drones"*; the mechanism is spaced armour *"designed to induce the premature
detonation or malfunction of incoming munitions"* [R].

**What they do not stop.** *"Useless against anti-tank guided missiles and large
air-dropped munitions"*; *"the cages provide no defence against side attacks"*;
and effectiveness *"varies, with some looking jury-rigged and crudely built"*
[R]. And the counter exists: the penetrator bomb of §3.3 is designed to
*"overcome nets and cages"* [R].

**[E] Game model:** a top-attack-only damage reduction of **0.3–0.5**, zero
against side attacks and against the two named munition classes, with a small
mobility or visibility penalty. Cope cages are also visually enormous, which
argues for a small **visual signature increase** — a caged tank is a
distinctively shaped tank.

### 7.4 Dazzle paint and computer-vision deception

`ground-logistics.md` §8 has this and I will not repeat it, except to note the
key framing that belongs in a terrain document too: the dazzle schemes are
assessed as *"aimed at the computer-vision systems in drone targeting workflows,
not at human eyes"* [R]. As autonomy becomes the norm (per `front-2026.md` §6A
tier one being routine), masking increasingly targets the *classifier*, not the
*sensor*. A terrain that confuses a classifier — clutter, edges, repeated
structures — is different from a terrain that blocks a sensor. I flag that as a
2027–28 direction, not something to build now.

### 7.5 Urban netting as a terrain feature

Now at civic scale: *"white nylon nets now stretch over roads and city
streets"*; Izium with *"roads and sidewalks completely covered in overhead
netting"*; a government plan for *"some 2,500 miles of drone nets on frontline
roads by the end of 2026"* [R, NPR March 2026]. `ground-logistics.md` §8 has the
kilometre series and the contested-effectiveness argument.

**[I]** For terrain purposes the relevant point is that netting is becoming a
*persistent map feature of settlements near the front*, not a temporary
expedient — which makes it a legitimate terrain overlay rather than a structure.

---

## 8. The engagement clock: detection to intercept

*Added at the coordinator's request. Terrain masking decides how far out a
defence sees something; this section asks whether that warning converts into a
shot. It is the same problem from the other end.*

**The units caveat, stated once and applied throughout.** This simulation has no
single coherent time scale — movement implies roughly 7:1 compression, the
day/night cycle roughly 240:1, against a 12:1 distance scale. **Absolute seconds
cannot be transplanted.** Everything below is given as a ratio.

### 8.1 The stages, and which one dominates

| Stage | What is published | Character |
|---|---|---|
| **Detection** | A single fixed radar-RF-EO-IR node gives *"meaningful detection coverage out to roughly 2–5 km"* [R]. Ukraine's national acoustic layer — Sky Fortress and Zvook — reportedly totals *"north of 24,000 individual listening posts for a combined cost under $5 million"* [R] | Wildly scale-dependent; see §8.4 |
| **Track and classify** | *"The acoustic network hears a Shahed enter Ukrainian airspace near the border and starts a track; radar picks it up and refines that track with speed, altitude and heading"* [R]. Radar *"immediately shows what type of drone is approaching"* [R] | Aided by the target's own behaviour: Shahed-class drones *"fly on a predictable course and do not manoeuvre"*, and are *"much more visible on radar due to their larger size"*, which *"reduces the time from launch to detection"* [R] |
| **Decide and authorise** | *"Positive identification is mandatory before any kinetic engagement can be authorised"* in most frameworks [R]; but the kill chain *"must function at machine speed with human oversight rather than human decision-making at every step"* [R] | Being automated away — see §8.2 |
| **Alert and prepare** | An interceptor crew is *"three or four people: the pilot, the navigator who tracks the target's movement and assists with targeting, the technician who prepares the drone for launch, and an engineer who equips the warhead before flight"* [R] | The warhead is fitted before flight, which is why readiness state matters |
| **Launch** | One tailsitter interceptor quotes *"deployment time of launch within 30 seconds"* [R, vendor] | Small |
| **Fly to intercept** | Interceptor speeds **315–343 km/h** (Sting) and **326 km/h** (Zerov) [R, vendor]; climb rates **30 m/s** and *"up to 40 m/s, reaching 6,000 m in a few minutes"* [R, vendor]; engagement range *"up to 25 km"*, tactical radius with return **18.5 km**, combat radius **up to 20 km** [R, vendor] | **This is the dominant stage** |

**The dominant stage is flight, by a wide margin, against a high-altitude
crosser.** Work it: a Geran-4 class target cruises at **300–350 km/h at
4,000–5,000 m** [R]. An interceptor climbing at 30 m/s needs **~2.2 minutes** to
reach 4,000 m, and transiting 20 km at 315 km/h needs **~3.8 minutes**; the two
overlap, so call the interceptor's flight **3–6 minutes**. [M, arithmetic on
vendor figures]

Against that, published total counter-UAS latency from first detection to defeat
is **15–40 seconds** [R], and launch alone is ~30 s [R].

> **[I, the ratio the designer needs] Pre-launch latency is on the order of
> one-tenth to one-sixth of interceptor flight time for a high-altitude
> engagement. Flight time is the clock; reaction time is a rounding error on
> it.**

**And the ratio inverts completely at short range.** The same literature gives
*"for FPV drones travelling above 100 km/h, response timelines are measured in
seconds"* and, at the extreme, *"three seconds is the window from detection to
decision to destruction"* [R]. In that regime the interceptor's flight time is
also seconds, so **pre-launch latency is 30–100% of the total** and reaction
time is the whole game.

**[E] The game rule that follows.** Give the interceptor a **reaction delay
expressed as a fraction of its own expected flight time to the target**, not as
a constant. Something like: delay = max(a small floor, 0.12 × flight time)
against a Layer.High crosser, and a flat, large fraction against a Layer.Low
close-in target. That is one multiply, it produces both regimes, and it survives
any rescaling of the game's time base.

### 8.2 Readiness states

The evidence says the difference is real and that it is being engineered away.

- Mobile fire groups have an explicit posture: *"Readiness number one sends all
  mobile fire groups to positions after Shaheds are launched"*, and *"the drones
  arrive in 40 minutes"* [R]. That is a named readiness state with a named
  warning interval.
- Interceptor crews are described as needing infrastructure to be ready at all:
  *"interception is possible only if there is a ground station with a control
  console, a catapult for launching aircraft-type drones, the interceptor drones
  themselves, specialised vehicles for mobile teams, and access to radar data
  that tracks both the target and the interceptor drone"* [R].
- The warhead is fitted by a dedicated crew member *before* flight [R] — so a
  cold airframe is not minutes from launch, it is a preparation task.
- Automation is collapsing the difference on the decision side: one Ukrainian
  system *"automates 95 percent of the interception process from launch to
  impact"* and *"integrates directly with the country's radar network"*; the
  operator *"simply selects the target in a specialised interface and issues the
  strike command"* [R].

**[I, medium] Two readiness states, not three.** *Ready* — crew present,
airframe armed, radar feed live, launch within the small floor delay. *Cold* —
everything else, and the transition costs a substantial, visible time. The
scarce resource is the **crew**, not the airframe: *"a salvo of 500 drones
saturates the number of trained pilots available to fly intercepts long before
it saturates the number of airframes on the shelf"* [R]. That matches
`front-2026.md` §10's finding that trained crews are the binding constraint, and
it is a better game economy than airframe count.

### 8.3 Vectoring: ground control to a predicted point, then handoff

**Both, in sequence, and the handoff range is published.**

The human description: *"a pilot flies the interceptor to the right point using
the radar network, then finds the target by eye, chases it, and attacks"* [R].
The automated description: *"in lock mode, the operator selects a target,
triggering a zoomed identification window for confirmation. Once verified, the
system transitions to automatic guidance mode, in which the drone independently
navigates toward the object"* [R]. And the cueing sources: *"Ukraine's most
common intercepts start with cueing through radar tracks, acoustic spotters, and
feeds from Ukraine's Mission Control battlefield management system that put a
pilot in the right place at the right time"* [R].

**Handoff ranges — the numbers that decide whether the game needs a real
predicted-intercept-point calculation:**

| Seeker | Lock-on range | Source |
|---|---|---|
| LITAVR optical/vision terminal guidance | **up to 2 km** | [R] |
| Commercial 24 GHz radar seeker (25° × 14° FoV), offered at ~$4,000 | **1 km** | [R] |
| Thermal acquisition of a Shahed-type target | **up to 500 m** | [R] |

Terminal guidance itself is neural-network "pixel lock" trained on *"thousands
of hours of real flight and simulation data"*, with GPS-free navigation [R].

**[I, high] The design answer.** The terminal seeker covers the **last 0.5–2 km**
of a **20 km** engagement — **2.5% to 10% of the run**. Everything before that is
ground-controlled flight to a computed point. So **yes, the game needs a real
predicted-intercept-point calculation**; the seeker does not rescue a bad
vector, it only closes a good one. But the PIP calculation is cheap: a constant-
velocity lead solve is one quadratic per engagement, computed once at launch and
refreshed on the mesh-rebuild cadence, not per tick.

The failure mode when the vector is good and the seeker is not is documented and
is worth a mechanic: *"the Shahed might be two hundred metres away and ground
radar knows it's there, but the pilot can't see a damn thing"* in fog or cloud,
and *"interceptor success rates fall off a cliff once winter or weather sets
in"* [R]. The game's `WeatherScale` should apply to the **terminal phase
specifically**, not just to search.

### 8.4 How much warning time is actually available

This has two answers and the gap between them is the finding.

**Strategic warning: enormous, and not the constraint.** 40 minutes from launch
to arrival for mobile fire groups [R]. Confirmed intercepts of Russian drones
*"over 50 km from their launch point"* [R]. Against a 3–6 minute interceptor
flight, that is a warning-to-reaction ratio of roughly **7:1 to 13:1**. [M,
arithmetic] Detection range is not the constraint here; **crew count and
positioning are**.

**Local warning: catastrophically short, and it is the constraint.** A single
sensor node sees 2–5 km [R]. A target at 350 km/h (97 m/s) crosses 5 km in
**51 seconds**; a Geran-5 class at 450–600 km/h cruise [R] crosses it in
**30–40 seconds**. Against an interceptor that needs minutes of flight, local
detection provides **less warning than the interceptor needs to arrive** — the
ratio is roughly **0.2:1**. [M, arithmetic]

> **[I, high] The binding constraint is not detection range and it is not
> reaction time. It is whether detection happens far enough out that the
> interceptor can be launched to a point *ahead of* the target. Extra detection
> range is worth almost nothing until it crosses the threshold at which a lead
> solution becomes feasible, and then it is worth everything.**

That is a step function, not a gradient, and it is precisely the thing the
current game cannot express — because with no reaction delay and no lead
pursuit, early warning buys nothing, which is why the radar-reflector decoy
measured zero effect.

### 8.5 How often the chain fails, and why

The best available arithmetic: Ukrainian interceptor teams flew *"roughly 6,300
sorties in February 2026, destroying more than 1,500 Russian drones"* [R,
carried by several outlets]. That is **one kill per 4.2 sorties — a 24% sortie
success rate** [M, arithmetic on reported figures].

Set that against *"interceptors accounted for more than 70 percent of Shahed
downings over Kyiv that month"* [R], a unit-reported *"95 percent intercept
rate"* for one system [C, operator-reported, explicitly flagged as
unverified in the source], and *"overall system intercept rates across
documented C-UAS networks run closer to 70 percent"* [R].

**[I, medium-high] Roughly three sorties in four do not produce a kill.** The
sources do not decompose that, so I will not pretend they do — but they do name
the causes:

- **Speed deficit.** *"The interceptor drones that closed Ukraine's
  cost-exchange gap were built against a 185 km/h target and cannot catch a
  500 km/h one"* [R]. Ratio: **0.37**. The game's 34 vs 55 is **0.62** — better,
  still not enough. See §9.
- **Geometry.** *"In a tail-chase scenario, the maximum target speed is
  120 km/h before the geometry becomes unfavourable"*; against head-on targets
  *"closing speed sums to 200+ km/h and the engagement window shrinks to 2–3
  seconds"* [R].
- **Weather at the terminal phase** [R, §8.3].
- **Crew saturation** [R, §8.2].
- **Low ingress defeating the cue** — under-100-foot flight *"defeats radar line
  of sight and shortens the engagement window to seconds"* [R]. This is the
  direct link back to terrain masking: **the target's altitude choice is a
  choice about your detection range, and therefore about your warning time,
  and therefore about whether a lead solution exists at all.**

**[E] The single number a designer can take:** make an interceptor sortie
against a fast crosser succeed about **one time in four** when everything is
nominal, and let the player's decisions — mast height, sensor siting, readiness
state, launch position relative to the threat axis — move that. Today the game
has no dial that moves it at all.

---

## 9. Evasion, routing, and the geometry of a slow pursuer

*Second extension. The question put to me was whether evasion effectiveness
scales with speed ratio rather than turn rate. It does, and the reason is exact
enough to implement directly.*

### 9.1 Does an attack drone know it is being engaged?

**The assumption needs correcting, and the correction is recent and datable.**

The baseline is as assumed: *"the baseline Geran-2 navigates autonomously along
a pre-programmed route using an inertial navigation system aided by satellite
navigation, requiring no operator control after launch"* [R]. No radar warning
receiver appears anywhere in the corpus, and I would not expect one on a
$20–50k expendable airframe.

**But rearward-facing cameras are now standard-ish, and they are explicitly for
this.** The sequence from the reporting:

- Interceptor drones became *"regular and widespread"* from around **late summer
  2024** [R].
- Russia's counter-measure: *"installing a rearward-facing camera on fixed-wing
  drones of various types"* [R]. Observed on Geran-2 by around **December 2025**
  — *"Geran-2 UAVs are increasingly being equipped with rear-facing cameras,
  both underneath and on top. Video from these cameras is transmitted via radio
  mesh modems to Russia. This may allow evasive manoeuvring from interceptors,
  or for flight corridor reconnaissance"* [R — the "may allow" is the source's
  own hedge and I keep it].
- By **mid-to-late July 2026**, analysis of a recovered *"Seeker"* Geran-2 near
  Odesa describes *"a dual-camera arrangement connected to a mesh modem through
  an onboard Ethernet switch: one camera provides a rearward view, allowing
  operators to monitor the surrounding airspace and maintain situational
  awareness during flight; the second forward-facing camera is designed for
  target acquisition and includes zoom"* [R].
- The intent is stated: *"Russians have added cameras and computer-vision
  systems to enable their drones to undertake evasive manoeuvres to avoid
  counter-drones"*; *"updated Geran drones can manoeuvre in response to threats,
  alter their routes, and take evasive action when confronted by Ukrainian air
  defences"*; *"a rearward-facing camera can be used to trigger evasive
  manoeuvres as the interceptor drone approaches"* [R].
- Separately, **infrared countermeasures** have been added *"to defeat drone
  interceptors and possible missiles fired by fighters, employing electrically
  heated cylindrical blocks to generate blooming infrared energy"* [R].

**[I, medium-high] So the correct model is three tiers, not two:**

| Tier | Threat awareness | Reaction |
|---|---|---|
| Cheap one-way attack drone, pre-2025 baseline | None | **Pre-planned route only** |
| Camera-equipped Geran-class, late 2025 onward | **Optical, rearward, over a live mesh link to a human or a classifier** | Route change plus terminal evasion; plus IR decoying against seekers |
| FPV / crewed drone | Operator sees everything the drone sees | Full reactive |

The coordinator's instinct is still the better *game* design and is still
correct for most of the roster: **ingress routing as a pre-launch player
decision with a cost** is well-supported and is the older, better-attested
behaviour. Routing is real and deliberate: *"circuitous routes rather than
directly to their target city, which appears to be an attempt to confuse the
defenders and further complicate the placement of defensive assets"*; *"routes
previously selected by reconnaissance UAVs where it is hoped Ukraine will have
fewer defending assets"*; *"taking indirect routes to the target to avoid known
air defences"*; and even *"a data link to reprogram the drones with a new flight
plan during an attack"* [R]. Add terrain following: *"routing along highways,
riverbeds and estuaries"* [R].

**[E]** Build routing as the primary mechanic — waypoints, a fuel/range cost for
indirection, and terrain-following as a speed-and-exposure trade. Then make
reactive evasion a **late-game upgrade on specific airframes**, not a universal
behaviour. That is both truer and better.

### 9.2 The geometry, which is exact

Here is the analytic result, and it is the most implementable finding in this
document.

Let the pursuer have speed `v_p`, the target `v_t`, and let `γ = v_p / v_t`.
The set of points the pursuer can reach before the target is bounded by the
**Apollonius circle** — the locus of points `X` with `|PX| / |EX| = γ`. For
`γ < 1` this is a finite circle *around the pursuer*, and the target is outside
it. [M, classical pursuit-evasion geometry; the Apollonius construction appeared
in the search results with exactly this framing: *"even when a pursuer is slower
than the evader, there exists a circular region where the pursuer can reach any
point before the faster evader can, defining a dominated angular sector of the
evader's possible escape routes."*]

Placing the pursuer at the origin and the target at distance `d` along the axis,
the circle has centre at `−γ²d/(1−γ²)` and radius `γd/(1−γ²)`. [M, derivation]
The half-angle subtended at the target is:

> **θ_max = arcsin(γ) = arcsin(v_p / v_t)**

and it is **independent of range `d`**. [M]

**Read that carefully, because it is the whole mechanic.** An intercept solution
exists if and only if the target's velocity vector lies within `±arcsin(γ)` of
the bearing from the target to the pursuer. Outside that cone there is **no
feasible intercept at any range, under any guidance law, however well the
interceptor flies**. Turn rate does not enter. Range does not enter.

**Applied to the game's own numbers:**

| Pairing | `v_p / v_t` | `θ_max` | Consequence |
|---|---|---|---|
| Interceptor 34 vs jet strike drone 55 | **0.618** | **38.2°** | Target must be within 38° of head-on. A **39° course change breaks the intercept entirely.** |
| Interceptor 34 vs combustion heavy strike (slower) | > 1 | undefined (all aspects) | Capture feasible from any aspect; **only then does turn rate matter** |
| Real analogue: 343 km/h interceptor vs 185 km/h target | 1.85 | all aspects | The regime the interceptors were designed for |
| Real analogue: 343 vs 500–600 km/h Geran-3/5 | **0.57–0.69** | **35–43°** | Matches the reporting exactly |

And the reporting does match, independently: *"a small interceptor at 343 km/h
cannot catch a Geran-3 cruising at 500–600 km/h from behind since the closing
speed is negative, so the chase is physically impossible — the Geran-3 has to be
engaged head-on"*; *"an interceptor that must be positioned ahead of its target
needs a track good enough to compute where the target will be, delivered early
enough to fly there"*; *"interceptors slower than the fastest jet-powered
variants remain useful only where launch geometry can be arranged in advance"*
[R]. Three independent statements of the same 38-degree fact in operational
language.

### 9.3 So: does evasion effectiveness scale with speed ratio rather than turn rate?

**Yes, and the inversion the coordinator's user proposed is correct.** The
reasoning, laid out:

1. **A fast target turns badly in radius and fine in what matters.** Turn radius
   `R = v² / (g·n)` scales with the *square* of speed, so a 55-unit target turns
   in **2.7× the radius** of a 34-unit one at the same load factor. But turn
   *rate* `ω = g·n / v` scales as `1/v`, so it is only **1.6× slower in
   degrees per second**. [M, standard kinematics]
2. **The heading change required to escape is set by `arcsin(γ)`, not by how
   fast you can execute it.** At γ = 0.618 it is under 40°. A sluggish fast
   target turning at, say, 5°/s executes that in **8 seconds** — trivially
   inside a 3–6 minute engagement. [I]
3. **Therefore the fast target's poor agility costs it nothing, and its speed
   buys it the entire escape cone.** A nimble slow target with γ > 1 against it
   has *no* escape cone at all; it can only degrade the intercept by forcing
   the pursuer to expend energy and by out-turning the seeker's terminal
   geometry. **The fast sluggish target is strictly harder.** [I, high]
4. **The second-order effect reinforces it.** A heading change displaces the
   predicted intercept point by roughly `v_t × t_flight × sin(Δψ)`. At
   `t_flight` of minutes and `v_t` of 97 m/s, a 20° change displaces the PIP by
   **kilometres** — far beyond a 0.5–2 km seeker acquisition envelope [R, §8.3].
   So even *inside* the feasible cone, a modest heading change can push the
   interceptor's arrival point outside its own seeker's field of regard. [I]

**What actually defeats an interceptor against a fast crosser, ranked:**

1. **Pursuer speed deficit** — sets the escape cone, and is the only one that
   can make intercept *impossible* rather than merely unlikely. [M]
2. **Guidance-law and cueing limits** — a lead solve needs a track good enough
   and early enough; low ingress defeats the cue [R].
3. **Seeker field of view and acquisition range** — 0.5–2 km, 25° × 14° on one
   published radar seeker [R]; a PIP error larger than that is a clean miss.
4. **Target manoeuvre** — effective *because of* (1), not independently of it.
5. **Energy** — an interceptor that has climbed to 4,000 m and chased has little
   left; relevant, but the sources do not quantify it and I will not.

### 9.4 Terminal manoeuvres and transit weaving

Thinner evidence, honestly reported.

**Terminal dive is real and is a tactic, not an evasion:** *"Russia has a new
drone tactic: dive-bombing with Shaheds from on high to avoid small arms fire"*
[R], and the Geran-5 is described as *"cruising 450–600 km/h, maximum ~650, up
to 800 km/h in a terminal dive"* [R]. So the terminal dive is a **speed
increase**, which by §9.2 *widens the escape cone further* at exactly the moment
the defence most needs it narrow. [I]

**Programmed weaving or corkscrewing in transit: I found no evidence.** Searches
for corkscrew, jinking and weaving returned nothing on these airframes. What
they returned instead was *circuitous routing at the operational scale* — a
different thing, measured in tens of kilometres rather than metres. **I am
recording this as not found rather than filling the space.** The fuel cost of
indirection is likewise unquantified in anything I saw; the only anchor is that
route length trades directly against the 450–850 km reported range of the
Geran-4 class [R].

### 9.5 The game rules this section produces

**[E] Four changes, in descending order of value per line of code:**

1. **The aspect gate.** Before committing an interceptor, test
   `cos(angle between target velocity and bearing-to-interceptor) ≥ cos(θ_max)`
   where `θ_max = arcsin(min(1, v_p/v_t))`. If it fails, **there is no
   engagement** — the interceptor should not launch, or should be recalled. One
   normalised dot product and one precomputed constant per unit pair. This alone
   makes speed a meaningful stat, makes decoys work, and makes launch position
   a player decision.
2. **A reaction delay expressed as a fraction of flight time** (§8.1).
3. **Lead pursuit to a predicted intercept point**, solved once at launch and
   refreshed on a slow cadence, not per tick (§8.3).
4. **Evasion as a heading change with a magnitude, not a dodge**, available to
   airframes that carry rear-facing observation, and gated by whether the target
   currently holds a detection on the interceptor (§9.1). Its effect falls out
   of rule 1 automatically — which is the elegant part. You do not need an
   evasion stat; you need a heading and a speed.

