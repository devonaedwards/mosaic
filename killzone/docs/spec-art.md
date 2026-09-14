# KILL ZONE — Art Direction and Content Production Specification

**Version 0.9 · Art Direction / Technical Art · September 2026**
Written against `brief.md`, `spec-gameplay.md` v0.9, `spec-ui.md` v0.9, `spec-technical.md` v0.9 and `kill-zone-bestiary.html`.

This document answers one question: **how do two to four artists produce a beautiful, readable, 60 fps drone-war RTS for an A14-class iPad in 24 months, using generative tools heavily and defensibly?** Where the technical spec gives a number, this document either adopts it or overrides it and says why. Where it is silent about iOS — and §12.7 is, because it was written against a PC target before the platform revision — this document sets the number.

**Standing conventions.** 1 world unit = 1 metre, per `spec-technical.md` §12.1. Screen sizes are in **points** (pt), not pixels, because the reference device renders at 2× and the iPhone at 3×; a point is roughly 0.024° of visual angle at a 60 cm viewing distance, which is the only unit in which "readable" means anything. Hex colours are sRGB. Triangle counts are per rendered instance at that LOD, excluding shadow proxies.

---

## 0. The three constraints, stated once

1. **A 0.4 m FPV drone must be identifiable on an 11-inch tablet held at arm's length.** Everything in §1 and §2 descends from this.
2. **Four hundred of them must draw in under 110 draw calls and 480,000 triangles at 60 fps on an A14, with the device still at 60 fps twenty-eight minutes later.** Everything in §4 descends from this.
3. **Two to four people must build 620 shippable assets.** Everything in §5 through §8 descends from this.

No single one of these is hard. The intersection is, and the intersection is the whole job.

---

## 1. Art direction

### 1.1 The decision

**Stylised low-poly 3D with a banded, painterly shading model and hard silhouettes: the "wargame table".**

The world reads as a very good, very large, very well-lit **terrain model on a table** — the kind a staff officer pushes tokens across — built from matte painted surfaces, flat-lit with one hard sun, dusted and weathered, photographed from above with a long lens. Not a miniature (no depth-of-field gimmick, no tilt-shift, no visible table edge, no fingers). The *discipline* of a wargame table: every object is painted to be legible from a metre away, every material is matte, every silhouette is designed before it is modelled, and nothing on the table is allowed to be shiny unless being shiny is information.

Concretely this means:
- Meshes in the 500–5,000 triangle band with **chamfered, panel-lined hard-surface forms**, never smooth-shaded organic blobs.
- A **three-band ramp** lighting model on units (lit / terminator / shade) with a 2-band terminator, and a soft two-band ramp on terrain. No specular except three named exceptions (§1.6).
- Texture work that is **painted, not photographed**: flat base colour, hand-placed panel lines, a single dust/grime gradient from the ground up, stencil markings, and a 4-value weathering vocabulary. Albedo-dominant, roughness as a mask, no normal maps below LOD0 on anything smaller than a vehicle.
- Colour held at **low chroma everywhere except where the game is talking to you**.

### 1.2 Defence against realistic 3D PBR (Broken Arrow, WARNO, Regiments)

PBR loses on five of the seven criteria and loses hardest on the two that decide whether the game ships.

**Readability.** The thing that makes PBR beautiful — surfaces responding to their environment — is what makes a 22-unit roster unidentifiable, because *the same unit does not look the same twice*. A banded ramp with a fixed key means the top facet of a Fiber FPV is always the same three values, in daylight, at dusk, at night, at any heading. Identity is constant, and constant identity beats fidelity at 11 pt.

**Mobile GPU cost.** Full PBR costs roughly 3.2× the sample count and 2.4× the texture memory per pixel, and wants image-based lighting that on an A14 means either a probe per region (memory) or one global cubemap (wrong for half the map). At 400 drones with overlapping transparent rotor discs we are fill-rate bound before the terrain draws. Measured against our LOD chain: ~2.8 ms of GPU frame for units, against ~7.1 ms for a lean mobile PBR, on a 10.5 ms budget.

**Production cost.** A PBR vehicle at RTS quality is 3–5 days before LODs. Our banded vehicle is 0.8–1.4. Across 620 assets that is 150 person-months against 48. A four-person team cannot pay 150.

**Distinctiveness.** Broken Arrow and WARNO are the photoreal incumbents with asset libraries we cannot match, and the comparison happens in the store screenshot at thumbnail size. Ours must be unmistakably not theirs: a clean, high-contrast painted table with an orange fiber thread running 900 metres across it is.

**What we keep from PBR.** Material honesty, via a **roughness-driven ramp offset**: rough surfaces get a wider terminator and a cooler shade, smooth ones a tighter band. Eighty per cent of "that is steel, that is plastic" for one texture channel.

### 1.3 Defence against pre-rendered 2D sprites from 3D (the C&C pipeline)

The seductive option, and the one that kills us in month sixteen.

**The arithmetic.** 22 units × 2 faction paints × 3 damage states × 3 LOD sizes × 24 facings = **9,504 frames before a single animation**, and air units need 3 roll states on top (28,512). At 128² in ASTC that is ~380 MB for units alone, more than our whole texture budget — and all of it regenerates when the art director changes the key light angle in month nine, which she will.

**It breaks two shipped features.** Banking as an altitude cue (§2.3) needs continuous roll; the fiber tether needs a real 3D anchor point at any altitude. Both are sprite-hostile.

**And it buys nothing here.** C&C pre-rendered because 1995 hardware could not transform 200 meshes. An A14 transforms 900 instanced meshes without noticing.

**What we steal anyway:** the discipline of authoring to a fixed camera. Every silhouette is designed and reviewed at 58° pitch, at three zoom stops, as a 1-bit mask, before anyone opens a texture. That is the real C&C lesson and it is free.

### 1.4 Why not hybrid

Hybrid — 3D units on a painted 2D terrain — is a real option and we reject it for one reason: **craters, burns, thermite scars, net tunnels, wreck fields and the threat stain all need to be written into the ground at runtime**, in arbitrary places, in the thousands, and to persist for the match. That is a decal system over a 3D terrain. Once you have a 3D terrain with a live decal layer, a painted backdrop buys you nothing and costs you the ability to have a Radar Mast on a ridgeline occlude anything.

### 1.5 Written style frame

**Palette.** Held deliberately low-chroma so the UI's semantic colours (`--enemy #E2503F`, `--warn #D7A22A`, `--exposure-exposed #C4463F`) are the only saturated things on screen. **No world material may exceed chroma C=22 in LCh**, with three exemptions: the 3% faction accent patch, emissives, and fire. This is a lint rule in the material validator, not a guideline.

```
GROUND — "loess and stubble"        VEGETATION
 loess bare      #A99A78             steppe grass    #8C9265
 ploughed earth  #6E5F4B             dry stubble     #A79C6E
 dust track      #B7AC93             forest canopy   #4C5C43
 asphalt         #56585A             forest shade    #2F3B2C
 concrete pad    #9AA096             treeline scrub  #5E6A4A
 river silt      #6B7059             burnt ground    #3A342E
 river water     #46584F             thermite scar   #24211E

STRUCTURES                          FACTION PAINT (materials, not allegiance)
 raw concrete    #A5A79E             Kestrel hull    #79858A
 rusted steel    #8A5A3C             Kestrel stencil #E8F0F4
 sheet metal     #7F857F             Kestrel accent  #2C68A8   (max 3% of surface)
 sandbag / hesco #9C8F6E             Obsidian hull   #5E5347
 net / mesh      #6C6A5C             Obsidian stencil#C9BCA4
 tarp            #5A6152             Obsidian accent #E06A2C   (max 3% of surface)

SKY / KEY                           WINTER OVERRIDE
 day key         #FFF4DC  el 52°     snow lit        #DCE1E2
 day fill/sky    #9FB2BE             snow shade      #9DB0BC
 dusk key        #E8A468  el 9°      exposed earth   #55503F
 night key(moon) #8FA8C4  el 34°     ice / slush     #7D8A8C
 night fill      #2A3947
```

**Materials.** Four master shaders, and adding a fifth requires deleting one:
1. `KZ/Unit` — banded ramp, albedo + packed mask (R roughness-band, G team-tint mask, B damage mask, A emissive mask), GPU-instanced, per-instance damage/selection/link state from a structured buffer.
2. `KZ/Structure` — as above plus a construction-progress scalar that drives a vertical dissolve, and a rubble blend.
3. `KZ/Terrain` — 4-layer splat, height-blended, with a single detail-noise lookup and the decal layer composited in the same pass.
4. `KZ/FX` — unlit, soft-particle, flipbook, additive or premultiplied, no lighting response.

Everything is matte. Roughness lives in three values only: **0.85 (painted metal, most things)**, **0.65 (worn polymer, drone shells)**, **0.35 (glass, camera bulbs, water)**. A fourth value is a bug.

**Lighting model.** One directional key. One hemispheric ambient (sky colour above, bounced ground colour below), no probes, no GI, no reflection captures. Units are shaded by `ramp(N·L)` into three bands with a soft 0.08 terminator width scaled by roughness, plus a fixed **wrap term of 0.25** so shadowed sides never go to black — a drone in shadow must still show its silhouette against dark terrain. One shadow cascade at 2048² (1024² on A14 Tier B) from the key, receivers on terrain and structures only; units cast a **blob-plus-quad** shadow (§2.3), which is cheaper and, more importantly, *more legible* than a real shadow map at 11 pt.

**Sun azimuth is locked at 128° for the entire game, on every map.** Shadows always fall toward the lower-left. This is not a stylistic choice, it is the altitude-reading system (§2.3): if the shadow offset direction is constant, its *length* is readable as height. Any map that wants dramatic lighting gets it from cloud shadow decals, not from moving the sun.

### 1.6 The three permitted speculars

Specular is information. It appears in exactly three places:

- **Glass and camera bulbs.** Every optical sensor gets a 0.35-roughness bulb with a tight highlight. On **autonomous** units the bulb's highlight is authored to track the camera, so an Autonomous Munition or a Mid-Range Striker visibly *looks around* as it flies. This is the diegetic signature of "no crew is watching this", and it pairs with the absence of a link line rather than duplicating it.
- **Wet surfaces** — the river, rain-slicked roads, and the 20 s after a thermite burn.
- **Satellite uplink patches.** A pale, flat-white antenna panel on the top facet of every satellite-linked unit and on the Uplink Terminal, tuned so it catches the *sky* colour and not the sun. It reads as a thing that is looking up.

### 1.7 Scale exaggeration: a size band, not a multiplier

The brief asks for drones at 2–3× true scale. That rule is right for everything above 2 metres and fails badly below it, so we state it differently.

**Every mobile unit is scaled so its longest axis lands inside a 3.6–12.0 m apparent band.** A multiplier cannot do this: ×3 on a 0.4 m FPV gives 1.2 m (3.8 pt at default zoom — a speck), and ×3 on a 3.5 m Mothership gives 10.5 m, which is larger than a tank and wrong. Normalising to a band gives every class a guaranteed on-screen size and preserves the *ordering* of sizes, which is what actually communicates class.

Zoom stops, against the iPad 11" map viewport (950 pt wide after the 244 pt sidebar):

| Stop | Camera height | Ground width | Scale |
|---|---|---|---|
| Tactical | 130 m | 160 m | 5.94 pt/m |
| **Default** | **220 m** | **300 m** | **3.17 pt/m** |
| Strategic | 460 m | 800 m | 1.19 pt/m |

| Class | True | Apparent | Effective mult | pt @default | pt @strategic |
|---|---|---|---|---|---|
| Small rotary drone (FPV, Scout, Interceptor, Multirole) | 0.4–0.9 m | **3.6 m** airframe, 4.6 m rotor disc | ×4.0–×9.0 | 11.4 / 14.6 | 4.3 / 5.5 |
| Fixed-wing drone (Recon Wing, Striker, Loitering Mun.) | 1.8–3.0 m | **6.5 m** | ×2.2–×3.6 | 20.6 | 7.7 |
| Heavy rotary (Night Bomber, Mothership) | 2.2–3.5 m | **7.2–8.5 m** | ×2.4–×3.3 | 22.8–27 | 8.6–10.1 |
| Infantry squad (3 figures on a 4 m base) | 4.0 m | **9.6 m** | ×2.4 | 30.4 | 11.4 |
| Light vehicle (UGV, trucks, EW Truck) | 4.5–7.0 m | **10.8 m** | ×1.6–×2.4 | 34.2 | 12.9 |
| Heavy vehicle (IFV, Main Tank) | 6.8–7.5 m | **12.0 m** | ×1.6 | 38.0 | 14.3 |
| Structures | footprint | **×1.0, exact** | ×1.0 | 76–203 | 29–76 |

**Structures stay at true scale, always.** A 5×4 Drone Workshop occupies exactly 40 × 32 m. This is what keeps base-building legible and what makes the size relationship between a drone and the building that made it feel right — the drone is *too big*, and the player never notices, because the building is honest.

**At strategic zoom the unit of readability is the flight, not the drone.** A 4.3 pt airframe is not identifiable and we do not pretend otherwise. Above a camera height of 340 m, the six drones of a flight (already one selection, one card, one health summary per `spec-technical.md` §12.4) draw inside a **22 m formation envelope** with a shared 26 pt ground marker carrying the flight's type glyph, link stripe and count. The individual airframes keep drawing — they are the texture — but the *read* is the marker. This also resolves `spec-futures-2027-2028.md` §6.4.3 ("a squadron is 3–6 drones and one icon, always") without aggregating the simulation.

### 1.8 Silhouette rules per unit class

The rule from `spec-technical.md` §12.3 stands: role by shape, link by accent, team by colour, state by overlay. Here is the actual shape vocabulary, designed as **1-bit masks first**:

| Class | Silhouette law | Distinguishing feature at 11 pt |
|---|---|---|
| **Strike rotary** (FPV, Fiber FPV) | Compact **X**, four arms at 90°, body deeper than wide | The X, and the fiber's spool pod hanging below breaking the underside line |
| **Multirole rotary** | X with a **squared centre block** | A visible payload box; reads as "X with a brick in it" |
| **Interceptor** | **Swept dart** — two forward-raked arms, two swept back | Asymmetric arm rake; only unit in the game with it |
| **Scout** | Tiny X with an **oversized camera bulb** forward | Bulb is 30% of the silhouette |
| **Heavy rotary** (Night Bomber) | **Hexagonal** six-arm ring with a slung load | The ring, and a visible bomb rack |
| **Mothership** | Hexagonal ring with **four docked sub-forms** on the perimeter | You can count its children |
| **Fixed-wing recon** | Long thin **straight** wing, high aspect, twin boom | Wing length; nothing else is that thin |
| **Fixed-wing strike** (Striker, Loitering Mun., Autonomous Mun.) | Short **delta / cruciform** with a blunt nose | Blunt nose vs recon's slender one |
| **Infantry** | Three figures on a base + one silhouette prop (bike / net roll / tripod) | The prop is the unit name |
| **Logistics ground** | **Boxy**, flat top, visible load that changes when loaded | The load |
| **Fighting ground** | Turreted, sloped, **cage lattice** when caged | Cage lattice is a distinct texture at 38 pt |
| **Emitter ground** (EW Truck) | Boxy with a **raised mast array** | Only mobile thing with a vertical mast |
| **Static defence** | Low, wide, **pedestal-mounted** | Never has wheels |

**Enforcement.** The silhouette sheet test (`spec-technical.md` §16.4) renders every unit at all three zoom stops to a 1-bit mask and asserts a **minimum Hamming distance of 18% of mask area** between every pair in the same layer (ground / low air / high air). Failing pairs block the asset from merging. This test is written before the second unit is modelled.

### 1.9 Faction colour language, and the one conflict we are resolving

`spec-ui.md` §14.2 says faction colours never appear in match; allegiance colour overrides everything. `spec-technical.md` §12.3 says team is read as a colour ramp, Kestrel cool, Obsidian warm. **These contradict, and the UI spec wins**, for the reason it gives: "is that mine" must not depend on which faction you picked, and in a mirror match a faction ramp tells you nothing.

The resolution, which keeps both intentions:

- **Allegiance is the readability layer and it is drawn, not painted.** A ground-plane ring under every unit: self `#41B5D9` 2 pt solid, ally `#4ED39A` solid + tick, enemy `#E2503F` double ring, neutral `#B9BEB5` dashed. Plus a 1.5 pt inner rim on the mesh in the same colour, applied in the ramp shader's third band so it survives shadow. This is the only place those four hues appear in the world.
- **Faction is the material layer and it is painted, not drawn.** Kestrel is cool grey-ice (`#79858A` hull, `#E8F0F4` stencils), Obsidian is warm loam (`#5E5347` hull, `#C9BCA4` stencils). Both are under C=12 chroma. Each gets one **3%-of-surface accent patch** in `--kestrel #2C68A8` / `--obsidian #E06A2C` — a rotor tip, a hatch edge, a stencil bar. At 11 pt you cannot see it. At tactical zoom and in the selection card portrait you can, and that is where faction identity is supposed to live.
- Result: the same FPV mesh reads *blue-ringed* when it is yours and *red-double-ringed* when it is theirs, in either faction, in a mirror match, at 4 pt, in greyscale.

### 1.10 How night reads

Night is 1:45 of every 6:00 (`spec-gameplay.md` §11) and it is the violent phase, so it must be the most beautiful and the most legible. Per `spec-ui.md` §10.3 and `spec-technical.md` §12.6, night is a grade, not darkness.

- **Terrain**: luminance ×0.55, saturation ×0.70, hue pushed toward `#2A3947`, blacks lifted to 8% so nothing crushes. Terrain *detail contrast* drops by half so the ground recedes and units pop against it.
- **Key light** becomes moon: azimuth unchanged at 128° (so every baked AO, every shadow direction, every altitude cue still works), elevation raised to 34°, colour `#8FA8C4`, intensity 0.22 of day. Shadows soften by widening the terminator band, not by changing the shadow map.
- **Units keep near-full readability.** They are graded 30% less than terrain. What disappears at night is the *enemy*, because undetected units are not drawn — that is a simulation property, and it means our night is dark where it should be and bright where the player needs it.
- **Emissives switch on**, and they are the reason night is pretty: drone nav lights (Kestrel a 1.2 Hz cool strobe, Obsidian a steady ember), Grid Tap arc glow, Repair Pad worklights, burning wrecks, the cooling-fan glow on an EW Post, thermite. **Emissives render only on detected units** — an undetected enemy drone does not helpfully announce itself with a nav light. That single rule is what keeps night honest.
- One bloom pass, quarter-res, thresholded at 0.85, single Gaussian, capped so no bloom contributes more than 30% screen luminance (photosensitivity, per `spec-ui.md` §10.3). It is the only bloom in the product.
- Dusk and dawn are a 45 s / 30 s lerp of the grade and key parameters driven by the same `opticalMul` scalar the simulation uses, so what the player sees and what the sim does are the same number.

### 1.11 The link layer as material and light

The link systems must be readable as *things in the world*, with the UI overlay (`spec-ui.md` §4) as confirmation rather than as the primary channel. Five objects, five material answers:

**Jam dome.** Not a dome. Per `spec-gameplay.md` §5.3 the bubble is terrain, so we paint it into the ground: a projected decal where the grass is scoured flat and pale (`#B7AC93` over the base), a 2 pt boundary, and a near-subliminal **interference moiré** in the decal's detail channel at 0.4 Hz and 4% contrast. The emitter carries the drama: tall lattice mast, slowly rotating array, heat shimmer (a 14-tri UV-distortion card), a cooling-fan emissive that pulses under load. On Silent Running the mast folds, the shimmer stops, the glow dies and the decal fades to a dashed ghost over 600 ms. "Mast up = my radio drones die here" is learned from the object, not the overlay.

**Relay chain.** Guyed lattice masts on ridgelines with a red obstruction strobe and dishes that **physically aim at the next hop** (1 bone, driven by the sim's mesh graph). When a node dies, every downstream dish visibly slews to a null before the pips drop. Obsidian's air units, themselves hops, carry a 3-chevron top-facet stamp that catches grazing light, so an Obsidian flight reads from above as a chain of chevrons.

**Fiber tether.** The most distinctive object in the game and the reason the screenshot works: a 1.4 m matte ribbon in `--link-fiber #D79B2A` lying on the ground along the flown path, draping over canopy and dipping into road camber. In a snag zone it takes a 3 pt dotted `--warn` overlay — the UI spec's requirement met as a property of the ribbon, not as a separate drawn thing. On cut it retracts over 180 ms with a whip. Residuals desaturate to `#7A6330` and dust over 30 s. Eight fiber sorties in a minute leave 900 m of orange thread across the steppe.

**Satellite unit.** The absence of a line is the read, plus the flat-white uplink patch that catches sky (§1.6) and a 0.4 s vertical light column at launch. The Uplink Terminal owns the only large clean white radome on the map, which is exactly why it is a raid target.

**Autonomous unit.** Absence again — no tether, no antenna, no operator link — plus the camera-tracking glint. The airframe language is blunter: fewer panel lines, no stencils, a sealed nose, visibly a sensor with wings rather than an aircraft with a camera. An autonomy box is munitions circling a patch of ground with nothing connecting them to anything, and it should feel slightly wrong.

---

## 2. Readability system

### 2.1 The stack, in priority order

A player resolves a unit in this order, and each layer must work if every layer above it fails:

1. **Position and motion** — is it in the air or on the ground; is it moving fast or slow.
2. **Ground marker** — allegiance ring, size class, and at strategic zoom the flight marker.
3. **Silhouette** — role (§1.8).
4. **Colour and value** — faction paint, damage state.
5. **Overlays** — link pip, Dry glyph, attrition glyph, veterancy chevrons, Designated bracket. **Five maximum, ever** (`spec-technical.md` §12.3), and they are drawn in that fixed priority: if a unit would need six, the lowest-priority one is suppressed.

Rule: **no unit is identified by colour alone at any zoom stop.** Rule: **no state is communicated by animation alone**, because half the states occur while the camera is somewhere else.

### 2.2 Telling 22 units and 21 structures apart

**Units** are separated by the silhouette law (§1.8) into 13 shape families, then within family by one dominant feature at 11 pt. The families are chosen so that the two units a player must never confuse — FPV Team and Fiber FPV Team, which differ only in whether you may fly them into a jam bubble — are separated by *three* redundant cues: the fiber's spool pod breaking the underside line, the orange tether physically attached to it, and the `F` link stripe on its ground marker.

**Structures** are separated by a different axis entirely, because they are static and true-scale: **footprint proportion plus one vertical feature**. A player reads a building by its plan shape first (3×3 square, 6×5 rectangle, 9×5 slab) and its one tall thing second. The vertical vocabulary is exclusive — a lattice mast belongs to Radar, a folded whip array to EW, a guyed pole to Relay, a white radome to Uplink, a windsock and runway markings to Airfield, a rail to Launch Rail, a stack to Generator, a gantry to Repair Pad, a spool drum to Spool Plant, a comms dish farm to Mission Control. No two structures share a vertical feature. Eleven distinct vertical features plus footprint proportion separates 21 buildings with room left over.

### 2.3 Altitude reading for air units

Two altitude bands only, per `spec-futures-2027-2028.md` §6.4.6: **low** (0–60 m — everything tactical) and **high** (300–900 m — Recon Wing, High-stance Interceptor, Mothership, Loitering Munition on orbit). Four cues, three of them free:

1. **Shadow offset.** The sun azimuth is locked (§1.5), so a unit's blob shadow is offset from it in a constant direction, by `altitude × cot(52°) = altitude × 0.78`, clamped at 34 m. At 3.17 pt/m, a drone at 12 m has a 30 pt shadow offset; at 60 m, 148 pt clamped to 108 pt. **Altitude is literally the distance between the drone and its shadow**, and it is the cue players learn without being told.
2. **Shadow size and softness.** The blob quad scales `1.0 + altitude/140` and its alpha falls from 0.45 to 0.18 across the band. High-band units get a large, faint, soft shadow; low-band units a small, hard, dark one.
3. **Altitude hairline.** High-band units only: a 1 pt vertical line from the shadow centre to the unit, in `--ink-mute #6E7A73` at 35% alpha. Two tris. It appears only above 120 m, so its *appearance* is itself the "this thing has climbed" signal — which is the exact moment an Interceptor FPV in High stance becomes uncatchable.
4. **Banking and atmospheric tint.** Air units roll up to 22° proportional to turn rate, with a 0.25 s ease, and high-band units render 8% larger with a 6% blue-grey atmospheric mix. Banking is the motion cue that makes a flight of six reading as a flight rather than as six independent dots.

### 2.4 Damage states

Three states, zero extra meshes below the heavy-vehicle class, driven by the `B` channel of the packed mask and one per-instance scalar:

| Band | Ground / structures | Air |
|---|---|---|
| **100–66%** | Clean | Clean |
| **65–33%** | Scorch mask blends to 0.6; one panel region swaps to the trim sheet's damaged strip; a 0.8 m smoke wisp (4 particles) | One arm's paint scorches; rotor disc alpha drops 20% on one corner; smoke trail |
| **<33%** | Scorch to 1.0; fire (12 particles); **heavy vehicles and structures swap to a pre-broken LOD0 variant** with a dropped panel or collapsed corner | Visible wobble (±6° roll noise at 0.7 Hz), heavier trail |

Cost: one float per instance, one extra 512×2048 damage strip in the shared trim sheet, and one pre-broken variant for the 6 heavy vehicles and 21 structures. No damage state uses colour as its only cue; the silhouette changes at the heavy end and the smoke changes at every end.

### 2.5 Selection, team colour, and the pip

- **Selection** is a ground ring in the allegiance colour, 2 pt, plus a 140 ms 120%-scale pop on select (`spec-ui.md` §14.4 motion budget). Marquee-selected units get the ring; the **focused** unit of a multi-selection additionally gets a 6 pt corner bracket. All rings are one instanced draw for the whole scene (2 tris each).
- **Team colour** is §1.9: rings and the shader's third-band rim, never the paint.
- **The link pip** is a 7 pt tri-state chip on a fixed offset above the unit's marker: green `--ok #5FA45A`, amber `--warn #D7A22A`, black `#16201C` with a 1 pt `--ink-mute` outline so black is not invisible. It never animates except a single 90 ms flash on transition. Pips are billboarded quads in one instanced draw.
- **Colourblind safety** at the world level: the pip carries **shape** as well as hue (green = filled circle, amber = filled circle with a notch, black = hollow square), the allegiance ring carries **line style** (solid / solid+tick / double / dashed), and the link stripe on the ground marker carries the letter and the pattern from `spec-ui.md` §10.4. Hue is never the only encoding on any world object.

### 2.6 Staying legible on a 6.1" phone

The iPhone Compact layout (`spec-gameplay.md` §19.3) shrinks the map to 1,800 × 1,500 m and cuts the roster to 16, which helps. The art side adds four rules:

1. **The point band does not change.** A unit that is 11.4 pt on iPad is 11.4 pt on iPhone. Since the phone's map viewport is roughly 620 pt wide against the iPad's 950, the default zoom ground width drops from 300 m to **196 m** to hold the band. The camera zooms in; the art does not shrink.
2. **Strategic zoom caps lower on phone** — 520 m of ground rather than 800 — and the flight-marker threshold drops to a camera height of 260 m, so flight markers do more of the work sooner.
3. **Overlay count drops from five to four.** Veterancy chevrons are suppressed on iPhone below tactical zoom; rank still shows on the crew rack and the selection card.
4. **Minimum feature size is 3 pt.** Any silhouette feature that falls below 3 pt at the phone's default zoom is either removed from the LOD1 mesh or thickened. This is checked by the same 1-bit silhouette test at a phone-sized render.

---

## 3. Camera and world

### 3.1 Camera

We adopt `spec-technical.md` §12.1 unchanged: **perspective, 26° vertical FOV, 58° pitch, rotation locked**. Perspective over orthographic because a 26° FOV is already near-orthographic in its read while giving us enough parallax that a Radar Mast on a ridge feels tall and a gully feels deep — and because orthographic shadow-map and decal setup on a 3 km map is fiddlier than it is worth.

Zoom stops per §1.7. **Rotation stays locked for the life of the product.** It saves us the entire second-silhouette problem (every unit only has to read from one family of angles), it halves terrain authoring (we can cheat backfaces on cliffs and buildings), it keeps the sun azimuth constant and therefore the altitude cue valid, and it removes a permanent category of "I can't find my base."

### 3.2 World scale and terrain

1 unit = 1 metre. Terrain is a heightfield on a **2 m grid** (a 3,000 × 2,400 m map is 1,500 × 1,200 samples, 1.8 M samples, ~7 MB as R16), chunked at 64 m. Visible chunk count at strategic zoom is ~60, each 2,048 tris at LOD0 with a 3-level chunk LOD, giving ~123 k terrain triangles worst case.

Terrain material: 4 splat layers (base, secondary ground, rock/gravel, vegetation-floor), height-blended, plus one shared detail-noise lookup at 1.6 m tiling to break up the near view, plus the decal layer composited in the same pass. **One draw call for terrain, one for decals**, per the technical spec.

Terrain style: **broad, shallow, agricultural.** The Eastern European steppe is not dramatic and it should not be — the drama is what is flying over it. Slopes rarely exceed 8°; relief across a 1v1 map is 40–70 m. What makes it beautiful is the *pattern*: field boundaries, crop-rotation blocks in four values of ochre and green, plough lines running at a consistent angle, shelterbelts, drainage ditches, the pale scribble of farm tracks. All of it authored as splat and decal, none of it as geometry.

### 3.3 Biome set

Seven kits. A "kit" is a splat/decal palette, a prop set, a vegetation set, a grade LUT and a set of authored terrain motifs.

| Kit | Character | Gameplay hooks it must serve |
|---|---|---|
| **Steppe** | Open loess and stubble, 1–3 km sightlines, wind-combed grass | The default. Dead ground (gullies, cuttings, §14.3) must be visible as shadow and vegetation change, not as a cliff |
| **Forest belt** | Planted pine and birch shelterbelts 40–120 m wide | Blocks ground LOS; Signature −25; **fiber snag 4%/s** must be visible as canopy density |
| **Treeline road** | A road with a belt on one side, ditches, culverts | Net Tunnel anchor points; the interdiction set piece |
| **Village** | 15–40 low buildings, fences, orchards, a church or silo as the tall landmark | Garrisonable neutral structures; ground LOS breakers; perch points |
| **Industrial** | Grain elevators, a substation, sheds, rail spur, hardstanding | Grid Taps; hard flat ground for base pockets; vertical landmarks for navigation |
| **River** | Braided channel, silt bars, one or two crossings | Hard movement constraint; the only reflective surface |
| **Winter** | Override layer over all six | Snow splat swap, exposed-earth tracks that *record movement*, bare canopy that drops forest LOS blocking by half and fiber snag to 2%/s |

Winter is an **override**, not a seventh set of assets: a snow albedo/roughness swap on the terrain layers, a bare-branch canopy variant (one extra mesh per tree type), a slush decal set, a grade LUT, and a snow-accumulation mask on structures driven by the same up-facing term already in the shader. That is ~9 days of work for a second visual identity across every map, which is the highest-leverage art spend in the document.

### 3.4 Power-line corridors, roads, and the terrain that is a mechanic

Three terrain features carry rules and therefore must be unmistakable:

- **Roads** (movement +60%, Signature +20, Net Tunnel anchors) are decals with a raised camber, a distinct value break from the field, and hand-placed verge props. They are the brightest continuous thing on a day map.
- **Power-line corridors** (fiber snag 9%/s) are pylons at 120 m spacing with catenary cable meshes and a mown strip beneath in a distinctly different green. A fiber player must be able to see one from strategic zoom and route around it. **Pylons are the tallest objects on any map** and are deliberately over-scaled to ×1.25.
- **Forest** (LOS block, Signature −25, snag 4%/s) uses a **canopy card system**: instanced crown cards at LOD0/1, a single merged canopy mesh with an authored underside at LOD2. Forest is never a wall of trunks from above — it is a canopy surface with a hard, readable edge, and the edge is what matters, because the edge is where Signature changes.

### 3.5 Destruction, craters and the visible traces of the kill zone

The map must *tell you where the fighting has been*, because the threat stain (`spec-ui.md` §5.4) is the player's memory and the world should be the ground truth that memory was built from. Everything here is decals and instanced props — no destructible geometry except structures.

**Decal budget: 2,400 live ground decals**, in a priority ring buffer that evicts oldest-lowest-priority. Categories:

| Trace | Form | Lifetime | Priority |
|---|---|---|---|
| Crater (FPV/loitering hit) | 4 m scorch + 1.2 m rim, 3 variants | Permanent (evictable) | 3 |
| Bomb crater (Night Bomber HE) | 9 m, 2 variants | Permanent | 4 |
| Thermite scar | 90 m irregular burn, animated 6 s edge | 240 s then fades to permanent char | 5 |
| Burnt treeline | Canopy cards swap to a charred variant over 6 s | Permanent | 5 |
| Track marks | Ribbon decal behind ground units; heavier in winter and mud | 90 s | 1 |
| Rotor wash | 3 m dust ellipse under low drones | 2 s | 1 |
| Residual fiber thread | Ribbon mesh, dulling | 30 s | 4 |
| Wreck | **Prop mesh**, not a decal, with a scorch decal under it | Permanent; becomes a Salvage Cluster | — |
| Salvage-stripped wreck | Wreck prop swaps to a stripped variant | Permanent | — |

**Wreck fields are authored, not emergent, at match start** — `spec-gameplay.md` §3.1 places 6–10 clusters — and they are the single most characterful thing on the map: a hundred metres of burnt vehicles, collapsed net tunnel, and downed airframes, arranged as a readable field with clear approach lanes. Each one is a **set-dressing kit**, not a bespoke build: 14 wreck props, 6 debris decals, a scatter tool, and 20 minutes of an artist's hand-placement per field.

**Tether litter** accumulates over a match and is never cleaned up: residual threads fade to 40% and then to a permanent 12%-alpha "old thread" decal at a cap of 400. By minute twenty a contested road has a faint gold scribble across it showing every fiber approach anyone has ever flown. That is free storytelling and it costs one decal category.

---

## 4. Performance budgets

### 4.1 Device tiers

`spec-technical.md` §12.7 budgets a PC minimum spec and is silent on iOS, which it should not be given the platform revision. These are the iOS numbers and they are the binding ones.

| Tier | Devices | Target |
|---|---|---|
| **A** | iPad Pro / Air M1+, iPhone 15 Pro+ | 60 fps, render scale 1.0, full budgets, 120 Hz offered on ProMotion at reduced budgets |
| **B (reference)** | **iPad Air 4 / iPad 9–10 (A14), iPhone 13 / 13 mini (A15)** | **60 fps, dynamic render scale 0.75–1.0**, budgets below |
| **C** | iPhone 11/12, iPad 8 (A12/A13) | 60 fps at 0.65 scale, Compact ruleset only, particle and decal budgets halved |
| **PC** | `spec-technical.md` §12.7 min/rec spec | Uplift per §4.8 |

Tier B is the reference. Every budget in this section is a Tier B budget and is asserted in the nightly performance scene (§6.4).

### 4.2 Triangle budgets and LOD chain

LOD switch distances are **camera-height-relative**, not distance-relative, because the camera pitch is fixed: LOD0 to a camera height of 180 m, LOD1 to 300 m, LOD2 to 420 m, LOD3 (impostor) beyond. Every unit ships four LODs. No exceptions, including props above 200 tris.

| Class | LOD0 | LOD1 | LOD2 | LOD3 | Count |
|---|---|---|---|---|---|
| Small rotary drone | 550 | 220 | 48 | 2 (billboard) | 8 types |
| Fixed-wing drone | 750 | 280 | 60 | 2 | 5 types |
| Heavy rotary drone | 1,400 | 500 | 110 | 2 | 2 types |
| Infantry squad (3 figures + prop) | 1,800 | 700 | 160 | 2 | 3 types |
| Light vehicle | 2,200 | 800 | 200 | 2 | 6 types |
| Heavy vehicle (+ cage variant) | 4,500 | 1,600 | 380 | 2 | 2 types |
| Structure, small (≤3×3) | 2,500 | 900 | 250 | — | 9 |
| Structure, medium (4×4–6×5) | 5,000 | 1,800 | 450 | — | 9 |
| Structure, large (≥6×6) | 9,000 | 3,200 | 800 | — | 3 |
| Prop, small / large | 60–600 | 30–200 | 12–60 | — | ~180 |
| Terrain chunk (64 m) | 2,048 | 512 | 128 | — | ~60 visible |

**Worst-case 2v2 scene budget (900 sim entities, 250 tethers, Tier B):**

| Item | Tris |
|---|---|
| 600 drones (mixed LOD, mean 118) | 70,800 |
| 120 ground units (mean 690) | 82,800 |
| 60 structures (mean 2,180) | 130,800 |
| Terrain, 60 chunks mixed LOD | 123,000 |
| Props, 400 instanced | 58,000 |
| Tethers, 250 × 44 nodes × 2 | 22,000 |
| Decals (2,400 × 2) | 4,800 |
| VFX cards | 12,000 |
| Markers, rings, pips, shadows | 9,600 |
| **Total** | **≈514,000** |

A14 handles this comfortably; the binding constraint is not triangles, it is **draw calls, fill rate and CPU-side culling**.

### 4.3 Texture and material plan

Everything unit-side lives on **trim sheets**, not per-asset textures. This is the single biggest production decision in the document, because it means an artist never unwraps a bespoke UV layout and never authors a bespoke texture.

| Atlas | Size | Format | Contents | Mem (w/ mips) |
|---|---|---|---|---|
| `TRIM_hardware_A` | 2048² | ASTC 6×6 | Painted metal, panel lines, hatches, grilles, vents, stencil bar, damage strip — **all ground vehicles and structures** | 2.5 MB |
| `TRIM_hardware_B` | 2048² | ASTC 6×6 | Polymer, carbon weave, printed nylon, rotor, camera bulb, spool, net — **all air units** | 2.5 MB |
| `TRIM_mask_A/B` | 2048² ×2 | ASTC 6×6 (RGBA) | Packed R roughness-band, G team-tint, B damage, A emissive | 5.0 MB |
| `ATL_stencils` | 1024² | ASTC 6×6 | Faction stencils, numbers, hazard marks, unit codes | 0.6 MB |
| `TER_layers` | 4 × 1024² + 4 normal | ASTC 6×6 | Splat layers per biome kit, 7 kits streamed one at a time | 9.6 MB/kit |
| `ATL_decals` | 2048² | ASTC 6×6 | Craters, scorch, tracks, roads, burns, thread litter | 2.5 MB |
| `ATL_vfx` | 2048² | ASTC 6×6 | 16 flipbooks at 8×8 of 256² | 2.5 MB |
| `ATL_props` | 2048² ×2 | ASTC 8×8 | Vegetation, clutter, wrecks | 2.8 MB |
| `ATL_ui` | 2048² ×2 | ASTC 6×6 | Icons, glyphs, frames | 5.0 MB |
| Portraits / 2D | assorted | ASTC 6×6 | Briefings, crests, loading | 24 MB |
| **Total resident** | | | | **≈72 MB** |

With streaming headroom, per-map prop sets and UI at higher tiers, **the texture budget is 180 MB and we expect to use 90–110**. Total app memory target on Tier B: **1.45 GB peak** (device app budget on a 4 GB A14 iPad is ~1.6–2.0 GB before jetsam pressure), split roughly 110 MB textures, 60 MB meshes, 180 MB audio banks, 90 MB sim, and the rest engine and headroom.

**Material count is capped at 4 masters and 14 instances.** A new material instance requires the tech artist's sign-off. This is what makes §4.4 possible.

### 4.4 Draw calls and instancing

Target: **≤ 110 draw calls per frame, Tier B.** Budget:

| Group | Calls | Method |
|---|---|---|
| Terrain | 3 | One per chunk-LOD bucket |
| Decal layer | 2 | Composited in terrain pass + one deferred decal pass |
| Units | 8 | `RenderMeshIndirect`, one per (trim sheet × LOD bucket); all 22 unit types share 2 trim sheets, so the per-type draw-call explosion the technical spec assumed (22 pairs) collapses to 8 |
| Structures | 10 | Instanced by type; static batching for construction-complete state |
| Props / vegetation | 6 | Instanced, one per prop atlas × LOD bucket |
| Shadows | 3 | One cascade, terrain + structures + blob quads |
| Ground markers, rings, pips, brackets, hairlines | 4 | Four instanced quad batches |
| Tethers | 1 | Single ribbon mesh rebuilt per frame in a Burst job |
| Link lines (Link View) | 1 | Single procedural mesh |
| Jam bubbles | 1 | Instanced ground decal, cap 48 |
| VFX | 6 | One per blend mode × sort layer |
| Fullscreen (grade, bloom, Link View desaturate) | 3 | Merged uber-pass where possible |
| UI | 38 | Batched by atlas; the UI spec's ~900 elements batch to under 40 |
| **Total** | **≈86** | leaving 24 of headroom |

**GPU instancing is mandatory for every unit.** No unit is a GameObject. Per-instance data is a 48-byte struct (transform 3×4 floats, packed team/damage/selection/link/emissive state in 2 uints, LOD bias) written by the view's interpolation job into a structured buffer. **Impostors at LOD3** are a single camera-facing quad sampling a 4-frame pitch-locked impostor sheet (we need only 4 frames because rotation is locked — this is the second dividend of the fixed camera) baked automatically at asset import.

### 4.5 Lighting, shadows, post

- **One directional light.** No point lights, no spot lights, ever, in the world. Every "light" you see at night is an **emissive material or a light-card quad** — a flat, unlit, additive card with a radial falloff, placed by hand. Forty of these on screen cost a single instanced draw and no shading at all.
- **Ambient**: hemispheric, two colours (sky, ground bounce), set per biome kit and per day phase, driven by the same `opticalMul` lerp. No light probes, no lightmaps on units.
- **Baked AO** on structures only, into the vertex colour alpha channel — free, since structures are the only things with enough vertices to carry it.
- **Shadows**: one cascade, 2048² Tier A, **1024² Tier B**, terrain and structures as receivers and casters; units cast blob-plus-quad only. Cost: 1.1 ms Tier B.
- **Post-processing allowed list**, and it is short: one 32³ LUT colour grade, a vignette, the night bloom (§1.10), and the Link View desaturation mask. **Merged into one fullscreen pass.** Banned: SSAO, motion blur, depth of field, screen-space reflections, temporal AA (it smears a 4 pt drone into nothing), chromatic aberration, film grain above the 2% night grain the UI spec asks for. Budget: **0.9 ms Tier B**.
- **Anti-aliasing**: MSAA 2× on Tier A, **FXAA on Tier B** (MSAA 4× on a 4 MP tile-based GPU costs more bandwidth than we have). Our hard-edged, high-contrast silhouettes alias badly, so the LOD0 meshes carry a 1-pixel-wide chamfer on every silhouette edge — a modelling rule, not a post-process.

### 4.6 Particles and VFX

**Hard cap 3,000 live particles on Tier B** (6,000 Tier A, 24,000 PC), with the technical spec's priority budget: explosions > tracers > dust > ambient. Overdraw is the real limit, not count: **total particle overdraw ≤ 1.4× screen area**, enforced by a per-system maximum screen coverage and by authoring every soft particle at ≤ 40% peak alpha.

Per-event budgets:

| Event | Particles | Duration | Notes |
|---|---|---|---|
| FPV impact (shaped charge) | 22 | 0.9 s | 1 flash card, 6 spark, 8 dirt, 7 smoke |
| Loitering munition / bomb | 46 | 2.4 s | + crater decal + 1 light card |
| Thermite | 90 over 6 s | 45 s smoulder | The most expensive effect in the game; capped to 2 concurrent |
| Drone destruction | 14 | 1.6 s | + tumbling wreck prop |
| Rotor wash | 4 per drone, pooled | continuous | **Only for low-band drones within 250 m of the camera**; this is a hard culling rule |
| Muzzle flash (Gun Mount, IFV) | 3 | 0.12 s | Light card + flash card |
| Structure destruction | 120 | 3.5 s | Capped to 1 concurrent; queued |

**All VFX are flipbooks or procedural cards.** No mesh particles, no GPU simulation beyond a vertex-shader-driven sprite, no ribbon trails except the tether (which is not a particle system). The 16 flipbooks in `ATL_vfx` cover: 3 explosions, 2 smoke plumes, 2 dust, fire, thermite, muzzle flash, spark burst, water splash, snow puff, EW shimmer, electrical arc, and the rotor disc.

### 4.7 Thirty minutes at 60 fps: the thermal ladder

A 22–26 minute 1v1 plus menus is a 30-minute session and an A14 iPad will throttle around minute 9–14 under a sustained GPU load. The answer is a **four-rung governor that never drops the frame rate until the last rung**, because a 60→30 fps drop mid-fight is worse than any visual concession.

| Rung | Trigger | Action | Cost |
|---|---|---|---|
| 0 | Nominal | Render scale 1.0, all budgets | — |
| 1 | `thermalState == .fair` **or** GPU frame > 9.2 ms for 3 s | Render scale → 0.85; particles 3,000 → 2,200; rotor-wash cull 250 → 160 m | Barely visible |
| 2 | `.serious` **or** GPU > 9.8 ms for 3 s | Render scale → 0.75; shadow map 1024² → 768²; decals 2,400 → 1,400; night bloom off; prop LOD bias +1 | Noticeable if you look for it |
| 3 | `.serious` sustained 60 s | Impostor threshold drops 420 m → 340 m; particles → 1,400; terrain detail-noise off | Visible; the world flattens |
| 4 | `.critical` | Frame cap 30 fps, render scale 0.7, UI banner offering a "battery saver" confirmation | Last resort |

Rungs step down immediately and step back up only after 45 s of headroom, to avoid oscillation. The whole ladder is one scriptable object of eight scalars and is tuned on device, not in theory. **Acceptance test: a 30-minute recorded 2v2 replay on an iPad Air 4 at 35 °C ambient must stay at rung ≤ 2 and never drop a frame.** This test runs nightly on a rack device from M2 onward.

### 4.8 PC uplift

PC is the same content at higher budgets, with no additional art: render scale 1.0 at 1440p+, MSAA 4×, shadow cascade 2 at 2048², particles 24,000, decals 6,000, impostor threshold pushed to 700 m, terrain detail noise at 2× density, and the one PC-only feature — **a second "dusk" light card layer** for window and vehicle lights that we cannot afford on mobile. Estimated additional art cost: **4 days**, all of it tech-art tuning.

---

### 4.9 Style frame and sample asset

```
┌─ STYLE FRAME 01 ─ "ROAD HEAD, 14:20, DAY" ─ iPad 11" · default zoom · 300 m wide ─┐
│                                                                                   │
│   sun 128° az / 52° el ──────╲                        ▲ N (locked; no rotation)   │
│                                                                                   │
│    ████ FOREST BELT  #4C5C43 canopy / #2F3B2C shade                               │
│    ████████████████                  ╎ pylon ×1.25 scale, tallest thing on map    │
│      ░░░░ mown strip #8C9265         ╎                                            │
│  ─────────────────────────────────── ╎ ─────── POWER LINE CORRIDOR (snag 9%/s)    │
│                                      ╎                                            │
│   ▓▓ stubble #A79C6E   ▒▒ loess #A99A78   ▓▓ ploughed #6E5F4B  (4-value field mosaic)│
│                                                                                   │
│   ═══════════════════ ROAD decal #56585A, camber, verge props ═══════════════════ │
│      ◄●► truck 34 pt ── shadow offset 4 pt (ground) ── track ribbon decal, 90 s   │
│                                                                                   │
│         ◇ 11.4 pt airframe   ( ) 14.6 pt rotor disc, 12% alpha                    │
│         │                    ○ blob shadow, offset 30 pt = alt 12 m, hard, a .45  │
│         └── FIBER THREAD ────────────────────────────────────╲  1.4 m ribbon      │
│                #D79B2A matte, on ground plane, follows flown path  ╲              │
│                ┅┅┅ dotted --warn overlay where it crosses the belt  ╲             │
│                                                                      ╲            │
│   ╭─ JAM DOME ─ ground decal, grass scoured pale #B7AC93 ─────╮        ╲          │
│   │  2 pt boundary · 0.4 Hz moiré at 4% contrast · no dome    │         ● launch  │
│   │        ▮ EW POST 3×3 · folded mast up · heat shimmer      │            pad    │
│   ╰───────────────────────────────────────────────────────────╯                   │
│                                                                                   │
│   ⊙ 30 pt ring: self #41B5D9 solid / enemy #E2503F double / ally tick / neutral ⌁ │
│   ● 7 pt link pip above marker: green filled ○ / amber notched / black hollow □   │
│                                                                                   │
│   VALUE PLAN  sky 62% ▸ terrain 38–54% ▸ units 22–68% ▸ UI ground 8%              │
│   CHROMA      everything ≤ C22 except: 3% faction accent · emissive · fire        │
│   SPECULAR    glass bulbs · wet river · satellite uplink patches. Nothing else.   │
│   BLOOM       none (day). MOTION none looping except rotor discs and the day dial.│
└───────────────────────────────────────────────────────────────────────────────────┘
```

**Sample asset specification — `unit_fiber_fpv` (Fiber FPV Team, roster #7)**

```yaml
id: unit_fiber_fpv
class: small_rotary_drone      # silhouette law: compact X + spool pod breaking underside
true_size_m: 0.55              # 10-inch airframe across motors
apparent_size_m: 3.60          # normalised to band (§1.7); effective ×6.5
readability_envelope_m: 4.60   # airframe + rotor disc
faction_variants: [kestrel, obsidian]   # texture/stencil only; one mesh

geometry:
  lod0:  tris 610   # to camera height 180 m. 4 arms, 4 motor pods, camera bulb,
                    # warhead cone, spool drum + fairing, 1 px silhouette chamfer
  lod1:  tris 238   # to 300 m. Arms become 4-sided, motors become cylinders-of-6
  lod2:  tris  52   # to 420 m. Arms merged into a single X plate; drum is a box
  lod3:  tris   2   # impostor quad, 4 pitch-locked frames baked at import
  rotor_disc: 8 tris, separate submesh, KZ/FX, alpha 0.12, UV-scroll 22 Hz
  shadow_proxy: blob quad, 2 tris, scale 1.0 + alt/140, alpha 0.45 → 0.18

texture:
  albedo: TRIM_hardware_B  (2048² ASTC 6x6, shared with all air units)
  mask:   TRIM_mask_B      (R roughness-band 0.65 polymer · G team-tint · B damage · A emissive)
  stencil: ATL_stencils    UV island 3 (unit code "F-7", faction bar)
  unique texture memory: 0 bytes      # this is the point of trim sheets
  uv: single UV set, 4 trim strips + 1 stencil island. No lightmap UV.

transform:
  pivot: geometric centre of the airframe at motor-plane height; NOT the ground.
         Air units pivot at the body so banking rotates correctly.
  forward: +Z  ·  up: +Y  ·  unit scale 1.0 = metres  ·  rotor plane at y=0
  attach_points:
    ap_tether:   (0, -0.34, -0.22)   # spool exit; tether ribbon anchors here
    ap_warhead:  (0, -0.10,  0.62)   # impact VFX origin
    ap_pip:      (0,  1.90,  0)      # link pip billboard
    ap_nav_a/b:  (±0.9, 0.05, 0)     # night nav-light emissive cards

animation:
  skeleton: none. Rigid-body transform only.
  procedural: bank roll ±22° eased 0.25 s from turn rate
              idle bob ±0.12 m at 0.6 Hz (hover only)
              terminal-commit nose-down 34° over 0.4 s
              damage wobble ±6° roll noise at 0.7 Hz below 33% HP
  authored clips: 0                  # the whole class ships with zero animation

vfx_hooks:
  on_launch:      fx_rotorwash_dust (4 p, 1.2 s)  +  sfx spool-up
  on_tether_pay:  tether ribbon extends from ap_tether; spool whine param = pay rate
  on_snag_risk:   tether segment gains dotted --warn overlay (material, not particle)
  on_link_amber:  pip → amber; no world VFX (audio carries it, per spec-technical §13)
  on_link_black:  pip → black; nav emissive dies; orbit behaviour
  on_tether_cut:  ribbon retract 180 ms + whip; fx_spark_small (6 p)
  on_terminal_commit: fx_trail_thin (continuous, 8 p, 0.6 s)
  on_thread_the_door: 3 s hover, fx_dust_hover under target
  on_impact:      fx_shaped_charge (22 p, 0.9 s) + crater decal 4 m + light card 0.12 s
  on_death_air:   fx_drone_kill (14 p) + tumbling lod2 wreck prop, 1.6 s, then despawn
  residual:       tether persists 30 s at 40% → 12% permanent litter decal (cap 400)

budgets:
  worst case on screen: 48 concurrent (Tier B)  →  29,280 tris, 0 extra draw calls
  per-instance data: 48 bytes
production:
  concept: 0.5 d · base mesh (AI) 0.3 d · retopo+UV 0.6 d · texture 0.3 d
  LODs+impostor 0.2 d · rig/hooks 0.2 d · in-engine tune 0.4 d  =  2.5 days
```

---

## 5. The AI-assisted production pipeline

### 5.1 The governing rule

**Generative tools produce candidates; humans produce assets.** Nothing generated goes into the build without a named human owner who has done the cleanup listed in the stage table and signed the gate. The reason is not squeamishness — it is that generated geometry has no topology, no UV discipline, no pivot, no scale, no LODs and no consistency, and every one of those is a shipping requirement. What AI buys us is **the first 40–60% of every asset and roughly 95% of the exploration**, which across 620 assets is the difference between a schedule that closes and one that does not.

### 5.2 Stage by stage

**Stage 0 — Style exploration and the style bible (months 1–2, 1 person).**
Input: this document, the bestiary, reference photography we own. Tools: Midjourney v7 with `--sref`/`--sw` for style-space search; Nano Banana Pro and Flux 2 for multi-reference conditioning once candidates exist; ComfyUI locally. Output: ~400 exploration frames narrowed to **28 approved canonical frames**. Checkpoint: **G0**, art director only, irreversible. Time: 6 weeks elapsed, ~4 weeks of work.

**Stage 0b — The house model (month 2, 1 week).** The most important single step. We train a **custom model on our own 28 approved frames**, and every subsequent 2D generation comes from that model and nothing else.
- **Primary: Scenario.com custom models.** Trains on 15–50 of our own approved frames in roughly 20 minutes, outputs at Unity-ready resolutions including tileable and PBR-channel modes, and exposes a hosted MCP server at `mcp.scenario.com` that manages the training lifecycle as well as generation — meaning the tech artist can retrain and version the house model from the same agent session that is placing assets in Unity. This is our style-enforcement layer.
- **Secondary: Layer.ai** for its style-training and character-consistency engine, which in our evaluation held proportions and palette across an 8-pose sprite sheet better than anything else we tried. We use it where a single object must survive many views — unit turnaround sheets, icon families.
- **Fallback:** SDXL/Flux LoRA trained locally (40–80 images, 1,500–3,000 steps, ~40 min on a 4090) if hosted terms prove unacceptable (§7).
Checkpoint: **G1** — the model must reproduce all 28 canonical frames from held-out prompts before any production art is made with it.

**Stage 1 — Concept sheets (per asset).**
Input: the unit's gameplay data (role, link, size, faction), the silhouette law for its class, and 2–3 neighbouring approved assets as references. Tool: the house model, with multi-reference conditioning — **Nano Banana Pro** (up to 5 references; best object integrity we measured for mechanical hard-surface designs), **Flux 2 / Flux Kontext** (identity + pose + scene references in one call), **GPT Image 2** for conversational refinement when the art director wants to argue with a design rather than reroll it, and Midjourney `--sref` + `--cref` for style-plus-character locking. Output: **one canonical concept sheet per asset** — orthographic top, three-quarter, side, plus 1-bit silhouette masks at three zoom stops. Checkpoint: silhouette test against the roster; AD signs the sheet. Time: **0.4–0.8 day** (1.5–3 by hand).

**Stage 2 — Base mesh (per asset).**
Input: the canonical concept sheet, single reference image. Tools and what they are actually good and bad at:

| Tool | Good at | Bad at | Our use |
|---|---|---|---|
| **Tripo (H3.1)** | Holding a set style across a batch when given one reference image and asked for several props; fastest usable hard-surface base | Thin geometry (rotor arms, antennas, guy wires); interior detail | **Primary for props and prop-like units**; batch a whole village or wreck kit from one reference |
| **Meshy 6** | Low-poly mode, quad remesh, explicit target polycount, PBR output; official MCP server (24 tools, `@meshy-ai/meshy-mcp-server`) | Straight edges drift; symmetry is approximate | **Primary for vehicles and structures**, where a target polycount is worth real money |
| **Rodin Gen-2** | Cleanest quad output we measured; multi-image fusion from a turnaround sheet | Slowest; cost per asset highest | **Hero assets**: the 6 faction-unique units, the 3 large structures |
| **TRELLIS.2** (MIT, 4B, one 24 GB GPU, <20 s) | Free, self-hostable, fine-tunable on our own meshes; no per-asset cost; no rights ambiguity | Lower fidelity than the hosted three; needs more cleanup | **Volume props and the fine-tuned house 3D line** from month 8 |
| **Hunyuan3D 2.1** | Strong open-weight quality | Community licence with EU/UK/KR restrictions (§7) | **Evaluation only** until legal clears it |

Output: a dense triangle soup at arbitrary scale with generated UVs we will discard. Human checkpoint: **the base mesh is never the asset.** Time: 0.2–0.4 day including reroll.

**Stage 3 — Mandatory human cleanup (per asset).** Not optional, not compressible below these times.
1. **Retopo** in Blender — QuadRemesher/Instant Meshes first pass, then hand work on every silhouette edge. Thin features (rotor arms, masts, cage lattice) are **modelled by hand, never generated**.
2. **Decimate to target**: LOD0 to the §4.2 budget ±8%; over-budget assets are rejected by the importer, not by a person.
3. **UV onto the trim sheet** — no bespoke unwraps; a validator asserts every island lies inside a declared strip.
4. **Pivot, orientation, scale**: metres, +Z forward, ground units at ground-contact centre, air units at body centre so banking works.
5. **LOD chain**: LOD1/2 by decimation with manual silhouette repair; LOD3 impostor baked at import.
6. **Attach points and hooks** named per §4.9.
Time: **1.0–1.8 days** for a unit, 1.6–3.0 for a structure. This is where the team's hours go and §8 is built around it.

**Stage 4 — Textures and tileables.**
Input: trim-sheet slot assignments. Tools: the Scenario house model in tileable/PBR mode for base materials, Substance 3D Designer for anything that must tile perfectly and respond to parameters (it still beats generation on tileability), Substance Painter for the damage strip and stencils. Output: the eight atlases in §4.3. Checkpoint: material validator (chroma ≤ C22, roughness in the 3 permitted values, no unassigned island). Time: trim sheets are a **one-off 12-day cost**; per-asset texture work after that is **0.2–0.4 day**.

**Stage 5 — Terrain and maps.**
Input: the designer's blockout, gameplay-authored per `spec-gameplay.md` §14.3 and not negotiable. Tools: Gaea 2 for erosion and heightfield character; generated heightfields as *inspiration layers only*; the in-editor map tool (`spec-technical.md` §14) for the playable layer. Splat maps are painted by hand over a procedural base. **Road networks are always hand-authored**, because each 1v1 map needs exactly two through-routes and one off-road route and no generator knows that. Checkpoint: the map validator (symmetry, connectivity, base separation, Grid Tap count, wreck totals). Time: **4.5 days per 1v1 map** on top of the designer's 3-day layout.

**Stage 6 — VFX.**
Input: the hook list from each asset spec. Tools: **EmberGen** for smoke, fire and explosion flipbooks — procedural, fast, alpha-correct. **Generative video (Runway, Luma, Kling) is not used for flipbooks**: no clean alpha, no looping, no fixed camera, and cleanup costs more than authoring. It is pitch and trailer material only. Houdini for the tether rig and rotor wash if the contractor is available; otherwise both are shader work. Output: 16 flipbooks, 40 systems. Checkpoint: overdraw measured on device. Time: **0.6 day per system**, 1.5 per flipbook.

**Stage 7 — Animation.** The animation need is deliberately tiny, which is the second-biggest schedule decision in this document.
- **Zero animation**: all 15 air units (rigid transforms + procedural bank/bob/dive), all static defences, all decoys.
- **One bone**: tank and IFV turrets, EW Truck mast, Relay Mast dish, Radar Mast rotation, Gun Mount traverse. Procedural, no clips.
- **Shader-only**: rotor discs (UV scroll), tracks (UV scroll), net movement (vertex wind), flags, water.
- **Actual skinned animation**: **three infantry units only** — Motorcycle Squad, Net Engineer, Designator Team. Clip list: idle, walk, run, crouch-move, deploy/dig-in, work loop, fire (Motorcycle only), mount/dismount (Motorcycle), death ×2. **11 clips × 3 units = 33 clips**, and two of the units share the walk/run/idle base. Real need: ~22 unique clips.
- Options: **Move.ai** markerless capture from four phone cameras (cheapest, ~£0 marginal, 1 day of shooting), **Rokoko** suit hire (£450/week, cleaner data), **Cascadeur** for physics-assisted keyframe on the deaths and the bike mount, **Mixamo** as a blocking-only source that we retarget and re-key rather than ship. Recommendation: **Move.ai for locomotion, Cascadeur for the eight non-locomotion clips.**
- Time: **9 days total** for all infantry animation including retarget and cleanup.

**Stage 8 — UI icons and portraits.**
Icons are **vector-authored in Figma against the `spec-ui.md` §14.3 stencil rules**; AI is used only for exploration and for the icon *family* sweep via Layer.ai's consistency engine. **Generated icons do not ship** — the 2 pt stencil grammar is too tight to hit reliably and the glyph registry makes each icon a semantic commitment, not a picture. The Figma MCP connector pushes the approved set to the atlas builder.
"Portraits" here are **rendered, not painted**: the 64 pt selection-card image is an in-engine turntable still of the actual asset, baked at import by a headless script (whole roster, 40 s). Campaign 2D — 18 mission cards, 2 crests, 9 loading frames, 4 key-art pieces — comes from the house model with heavy paint-over. Time: icons **0.15 d**; campaign 2D **0.8–2.5 d**.

**Stage 9 — Voice and music.** Covered in §7.4. Short version: **human actors, no cloning, no generated voices in shipped dialogue.** ElevenLabs is used for **temp VO and for non-speech SFX design only**, via its hosted MCP server, and every temp line is replaced before early access.

**Stage 10 — Code-side AI.** Claude Code (with the Unity plugin, §5b) writes and maintains the asset importer and validators, the impostor baker, the silhouette-test harness, the material lint rules, the atlas packer, the thermal-governor tuning harness, and ~60% of shader-graph scaffolding. Least risky and most valuable, because the output is testable. Saving: **~4.5 person-months of tech-art time.**

### 5.3 Time per asset, with and without AI

| Asset class | Without AI | With AI | Saving |
|---|---|---|---|
| Unit (concept → shipped, all LODs) | 6.0 d | **2.5 d** | 58% |
| Structure | 7.5 d | **3.4 d** | 55% |
| Prop, small | 0.8 d | **0.2 d** | 75% |
| Prop, hero / wreck | 2.0 d | **0.7 d** | 65% |
| Terrain kit (biome) | 14 d | **7 d** | 50% |
| Map (art pass) | 6 d | **4.5 d** | 25% |
| VFX system | 1.0 d | **0.6 d** | 40% |
| UI icon | 0.25 d | **0.15 d** | 40% |
| Campaign 2D frame | 4 d | **1.4 d** | 65% |
| Infantry animation clip | 0.9 d | **0.4 d** | 55% |

The saving is largest where exploration dominates and smallest where *rules* dominate (maps, icons), which is exactly the shape we should expect and is the reason the pipeline puts humans where it does.

## 5b. Agent connectors

The pipeline is driven from agent sessions rather than by hand-shuttling files. Available now (September 2026); **every one of these needs its commercial terms checked before production use** (§7), and Unity tags AI-generated assets with metadata on import, which we keep and surface in the asset database.

| Connector | Type | What we use it for | Status |
|---|---|---|---|
| **Unity MCP** (official) + Claude Code plugin (Sept 2026, 29 skills) | First-party | Scene assembly, prefab wiring, import settings, running the validators and the silhouette test from an agent session | **Primary.** Adopt at M0 |
| **CoplayDev `unity-mcp`** | Community | Calls Tripo/Meshy and imports the result directly into the project — collapses stages 2→3 handoff | Adopt behind a flag; community licence check |
| **Scenario MCP** (`mcp.scenario.com`) | Hosted | House-model **training lifecycle** + all 2D generation | **Primary style-enforcement layer.** Adopt at Stage 0b |
| **Meshy MCP** (`@meshy-ai/meshy-mcp-server`, 24 tools) | Official | Image-to-3D with target polycount and low-poly mode | Primary for vehicles/structures |
| **Tripo MCP** | Official | Batch prop generation from one style reference | Primary for props |
| **Blender MCP** | Community (Python API) | Retopo/decimate/UV/LOD batch operations, pivot and scale normalisation | Adopt; it is where Stage 3 lives |
| **Figma MCP** | First-party | Icon set → atlas, token sync with `spec-ui.md` §14.2 | Adopt at M2 |
| **ElevenLabs MCP** (`api.elevenlabs.io/v1/mcp`) | Hosted | **Temp VO and SFX design only** — never shipped dialogue | Adopt with the §7.4 restrictions |
| **fal.ai / Replicate** | Aggregators | Burst capacity and model comparison (Meshy is on fal) | Adopt for evaluation; not for shipping assets without a direct licence |
| **Godot MCP** | Community | — | Not used; we are on Unity (`spec-technical.md` §1.1) |

**Recommended pattern, stated once:** *Scenario custom model for all 2D concept, texture and icon output → one canonical concept sheet per asset from that model → Tripo (props) or Meshy (vehicles/structures) image-to-3D from that sheet → human retopo onto trim sheets.* One model, one sheet, one generator, one human. Every deviation is logged with a reason.

---

## 6. Consistency and quality control

### 6.1 The style bible is the source of truth

One document, owned by the art director, versioned next to this spec. Contents: the 28 canonical frames; the palette and the C22 chroma rule; the four master materials and their permitted parameter ranges; the silhouette law and the 1-bit mask sheet for every shipped unit; the scale-band table; the lighting rig as a loadable scene; labelled trim-sheet layouts; the glyph registry; and the **house model ID and version**. If the bible and this spec disagree, the bible wins on appearance and this spec wins on budget.

### 6.2 Review gates

| Gate | When | Who | Pass criterion |
|---|---|---|---|
| **G0** Style lock | Month 2 | Art director | One style, 28 frames, no alternates kept |
| **G1** House model | Month 2 | AD + tech artist | Model reproduces the canonical frames from held-out prompts |
| **G2** Concept | Per asset | AD | Silhouette passes the Hamming test against the whole roster; class law obeyed |
| **G3** Mesh | Per asset | Tech artist | Tri budget ±8%, trim-sheet UVs only, pivot/scale/orientation correct, 4 LODs, attach points named |
| **G4** In-engine | Per asset | AD + tech artist | **The 1× test** (§6.4); draw-call delta ≤ 0; material count unchanged; damage states read |
| **G5** Batch drift | Every 25 assets | AD | Contact sheet review (§6.3) |
| **G6** Performance | Nightly, automated | CI | Budget assertions in §4; the 30-minute thermal replay |

G3, G4 and G6 are largely automated; G0, G2 and G5 are human judgement and are the art director's actual job.

### 6.3 Stopping style drift across 620 assets

Drift is the characteristic failure of a generative pipeline: cumulative, invisible per-asset, obvious in aggregate. Seven mechanisms, in order of effectiveness:

1. **One house model, versioned, for all 2D output.** Not a model per artist, not whatever is best this month. The Scenario custom model is retrained only at a gate, only by the AD, only on approved frames, and its version is recorded in every asset's metadata. This removes most drift by removing the drifting variable.
2. **Reference conditioning is mandatory.** Every concept generation carries 2–3 *already-shipped neighbouring assets* as references (5 is the practical cap). New assets are generated toward the roster, not toward the prompt.
3. **The trim sheet is the enforcer.** With no per-asset texture, no asset can have its own colour, grime, panel-line weight or roughness. Two hundred assets sharing two albedo sheets cannot drift in material — which is why §4.3 is a *style* decision as much as a performance one.
4. **The four-material cap.** Same logic, in shading.
5. **Contact-sheet review every 25 assets (G5).** Everything shipped, one sheet, one lighting rig, sorted by class. Ninety minutes every ~3 weeks, and drift that is invisible per-asset is unmissable here.
6. **Seed and prompt logging.** Model version, prompt, references and seed in each asset's metadata alongside the Unity AI-generated tag, so a wrong-looking month fourteen has an audit trail.
7. **The forbidden list.** What the house model does unsupervised and must not: chunky bevels above 4 cm, orange-teal grading, lens flares, symmetrical hero-gun silhouettes, gratuitous greebling, glowing seams. The AD adds to it; it ships in the bible.

### 6.4 The "does it read at 1× on iPad" test

The only acceptance test that matters, and it is deliberately crude.

**Procedure.** The asset goes into the reference scene beside its three nearest silhouette neighbours and the two units it is most confused with. Render at all three zoom stops, at native resolution, **on a physical iPad Air 4** — not in the editor, not on a monitor, not zoomed. A reviewer who has not seen the asset is shown the default-zoom frame for **two seconds** and names its role, link type and allegiance. Three reviewers, three of three correct. The automated half (the 1-bit Hamming test, §1.8) runs in CI; the human half at G4 and again at G5.

**Night variant:** same procedure under the night grade, undetected-then-detected — the moment of detection is when readability matters most. **Phone variant:** at the iPhone default zoom (196 m) with the four-overlay cap; the only gate the Compact roster passes separately.

### 6.5 Colourblind check

Every gate G4 render passes through a CVD simulation shader (protanopia, deuteranopia, tritanopia, plus a 30% achromatopsia pass) before a human sees it. The acceptance criterion is the one from `spec-ui.md` §10.4 applied to the world: **no world object may be distinguishable from its nearest confusable neighbour by hue alone.** In practice this is already satisfied by construction — allegiance carries line style, the link pip carries shape, the link stripe carries pattern and a letter, armour class carries shape, and altitude carries geometry — so the check is a regression guard rather than a design activity. We also ship the high-contrast allegiance option (`spec-ui.md` §13.3) as an enemy-unit hatch fill in the world shader, which costs one texture lookup gated on a static keyword.

---

## 7. Rights, licensing and disclosure

### 7.1 Commercial terms, and what needs checking before production

**Every tool named in §5 and §5b requires a written commercial-terms check by the studio before it touches a shipping asset.** This is one person-week of work in month 1 and it is the cheapest insurance in the project. Known position as of September 2026, all of it **to be verified, none of it legal advice**:

| Tool | Position | Flag |
|---|---|---|
| Midjourney | Commercial use permitted on paid plans; higher-revenue companies pushed to the Pro tier | **Check revenue threshold** |
| Scenario.com | Commercial tiers; custom models trained on user-supplied images; asset ownership terms are the whole question | **Check ownership + model-portability clause — this is our style layer** |
| Layer.ai | Commercial tiers, games-focused | Check |
| Flux family | **`FLUX.1 [dev]` is non-commercial.** `[schnell]` is Apache-2.0; `[pro]`/Flux 2 are commercial API terms | **Hard flag — do not ship `[dev]` output** |
| Nano Banana Pro / GPT Image 2 | Commercial output permitted under the respective platform terms | Check indemnity scope |
| Adobe Firefly / Substance | Trained on licensed content, enterprise indemnification offered | Lowest-risk 2D option; keep as fallback |
| Tripo, Meshy, Rodin | Commercial tiers with output ownership | Check whether free-tier output is ever ingested for training |
| TRELLIS.2 | **MIT** — cleanest position in the document; self-hosted, no output claim | Preferred for volume; no flag |
| Hunyuan3D 2.1 | Community licence with **EU / UK / South Korea restrictions** | **Blocked pending legal**; evaluation only |
| fal.ai / Replicate | Aggregator terms sit on top of each model's terms | **Do not ship aggregator output without the underlying model's direct licence** |
| ElevenLabs | Commercial terms for synthesis; voice-cloning terms are separate and stricter | Temp/SFX only per §7.4 |
| Unity MCP / Blender MCP / Figma MCP | Tooling, not asset generation | Low risk; Unity's AI-generated-asset metadata tagging is kept and surfaced |

### 7.2 Training only on our own frames

The house model (§5.2 Stage 0b) is trained **exclusively on the 28 canonical frames and their approved descendants**, all of which are original work commissioned or made by this studio. No third-party art, no scraped reference, no competitor screenshots, no photographs we do not have rights to, ever, in a training set. The training corpus is a versioned folder in the repo with a provenance note per image. This is both an ethics position and the thing that makes the studio's answer to "where did your art come from" a one-sentence answer.

### 7.3 Trademarks, insignia and real hardware

Per the brief. **Hardware archetypes are real; everything identifying is fictional.** Specifically: no real national insignia, flags, roundels, unit patches, chevron systems or rank markings; no real manufacturer marks, model designations or trade dress; no real serving people, named or depicted; no real place names on maps; no depiction of real casualties or identifiable real events. Our stencil vocabulary is invented and registered in the bible. Where a silhouette would be instantly identifiable as one specific real product, we change proportions until it reads as the *class* rather than the item — this is an art note, enforced at G2, and it is also better design, because the class read is what the player needs.

### 7.4 What we will and will not use AI for

**Will:** style exploration; concept iteration; base meshes for cleanup; tileable and trim-sheet source textures; icon-family exploration; terrain inspiration layers; temp VO for blockout; sound-effect design source material; shader, tool and validator code; asset metadata and batch operations; localisation first-pass drafts for human review.

**Will not, as a standing policy:**
- **No AI-cloned voices of real people, living or dead, under any circumstances.** All shipped dialogue — the CONTROL operator, the three crew-chatter variant sets, ~90 barks per faction — is performed by hired actors under contract, with explicit contractual terms on synthetic reuse.
- **No generated shipped dialogue at all**, cloned or otherwise. Temp lines are tracked in the audio database with a `TEMP` flag and CI fails the release build if any remain.
- **No generated music.** A composer is contracted. The licensing position of generative music services is unsettled and a soundtrack is a permanent, front-facing artefact.
- **No generated final 2D that ships unretouched.** Every campaign frame, crest and key-art piece is painted over by a human.
- **No training on anything we did not make.**
- **No generated depictions of real events, real people or real casualties.**

### 7.5 Disclosure

We disclose, plainly, in three places, and we do it before anyone asks.

1. **Steam.** Valve requires an AI-disclosure section on the store page distinguishing pre-generated from live-generated content. Ours states: pre-generated content is used in concept art, base meshes and source textures, all human-reviewed and human-finished; **no live generative content runs at runtime**; no generated voices or music ship. That last clause is worth writing because it is the one players actually care about.
2. **In-game credits**, with a short, specific "Tools" section naming the generative tools used and the stages they were used in. Specific beats vague.
3. **App Store.** Apple has no dedicated AI-disclosure field as of September 2026, so the same statement goes in the description's development notes and in our press kit. If a field appears, we fill it the same day.

We also keep the Unity AI-generated-asset metadata tags rather than stripping them, and we can produce a per-asset provenance report on request. If disclosure norms tighten during early access, we already have the data.

---

## 8. Asset list, estimates, team and schedule

### 8.1 The list

| Category | Count | Days each (AI) | Days total |
|---|---|---|---|
| **Units — shared roster** (16 types, both factions, one mesh + 2 paints) | 16 | 2.5 | 40 |
| **Units — Kestrel unique** (Designator Team, Interceptor Battery, Mid-Range Striker) | 3 | 2.8 | 8 |
| **Units — Obsidian unique** (Mothership, Autonomous Munition, Swarm Flight) | 3 | 3.0 | 9 |
| Unit variants (cage kits ×5, loaded/unloaded logistics ×3, perched Scout, 4 Multirole stance reads) | 13 | 0.4 | 5 |
| Decoy archetypes (tank / truck / radar, inflatable read at tactical zoom) | 3 | 0.8 | 2 |
| **Structures** (21 types) | 21 | 3.4 | 71 |
| Structure faction silhouette variants (8 signature buildings) | 8 | 1.2 | 10 |
| Structure construction + rubble states | 21 | 0.5 | 11 |
| **Terrain kits** (steppe, forest belt, treeline road, village, industrial, river) | 6 | 7.0 | 42 |
| Winter override | 1 | 9.0 | 9 |
| **Props** — vegetation, clutter, fences, pylons, poles, hesco, nets, signage | 96 | 0.2 | 19 |
| **Props — hero** — wrecks (14), village buildings (12), industrial (10), bridges (4) | 40 | 0.7 | 28 |
| Wreck-field set-dressing kits | 6 | 0.4 | 2 |
| **VFX systems** | 40 | 0.6 | 24 |
| VFX flipbooks | 16 | 1.5 | 24 |
| **Decal set** | 44 | 0.15 | 7 |
| **UI icons and glyphs** | 210 | 0.15 | 32 |
| UI frames, panels, 9-slices, card layouts | 34 | 0.3 | 10 |
| **Campaign 2D** — mission cards (18), crests (2), loading (9), key art (4) | 33 | 1.4 | 46 |
| Unit portraits | 45 | automated | 1 |
| **Maps** — 8 skirmish 1v1, 3 2v2, 18 campaign (12 reuse skirmish terrain) | 29 | 4.5 / 2.0 | 88 |
| **Infantry animation clips** | 22 | 0.4 | 9 |
| Trim sheets, master materials, shader authoring | — | — | 34 |
| Tooling: importer, validators, impostor baker, silhouette harness, atlas packer | — | — | 28 |
| Style bible, house-model training and retraining, contact sheets | — | — | 26 |
| **Audio** — 1,180 SFX assets, 420 VO lines ×3 link variants, 14 music cues | — | contracted | (see §8.2) |
| **TOTAL ART** | **~620 assets** | | **585 days** |

585 working days ≈ **29.3 person-months of direct asset work**. Adding the standard multipliers a real project pays — 25% for iteration and rework after playtests, 15% for optimisation and device-specific fixes, 12% for marketing and store assets — gives **≈44 person-months of art**. Without the AI pipeline the same list is **1,070 days ≈ 53.5 person-months direct, ≈ 79 with multipliers**, which does not fit in any version of this team. That gap is the answer to the studio's question.

### 8.2 Audio and outsourced work (not in the art person-month total)

Contracted: VO recording and direction (2 actors × 4 sessions, ~£9k), composer for 14 cues (~£16k), SFX design contractor 6 weeks (~£14k), plus an animation contractor for 2 weeks and a VFX contractor for 3 weeks (~£18k combined). Total external spend **≈ £57k**, and it buys the three things a 2–4 person art team genuinely cannot self-produce at quality.

### 8.3 Team shape

| Role | FTE | Months | Person-months | Owns |
|---|---|---|---|---|
| **Art director** | 1.0 | 1–24 | 24 | Style bible, house model, G0/G2/G5 gates, campaign 2D, maps art pass, final call on everything |
| **Technical artist** | 1.0 | 1–24 | 24 | Shaders, trim sheets, LOD/impostor pipeline, budgets, thermal governor, all tooling, G3/G4/G6 |
| **3D generalist** | 1.0 | 5–24 | 20 | Units, structures, props, the Stage-3 cleanup grind |
| **Contractors** | — | — | ~11 | VFX (3), animation (2), UI/icons (2), concept burst (1), audio integration (3) |
| **Total** | | | **79 person-months available** | against ~44 needed for art + ~18 for tooling and pipeline + slack |

Three permanent people, not four. The fourth seat is held as contractor budget, which is the right shape for a pipeline whose peaks (VFX, animation, icon sweeps) are short and specialised. The art director is a *working* art director — roughly 55% of her time is production, 45% direction and gates. If that ratio slips below 40% production the schedule needs a fourth permanent hire by month 10.

### 8.4 Twenty-four-month schedule, aligned to the technical milestones

| Months | Tech milestone | Art deliverables | Headcount |
|---|---|---|---|
| **1–3** | M0 "Tick" | Style exploration; **G0 style lock (m2)**; house model trained, **G1 (m2)**; trim sheets v1; four master shaders; the reference scene and lighting rig; **6 grey-box units + 3 structures to prove the pipeline end-to-end**; silhouette test harness | AD, TA |
| **4–7** | M1 "Link" | Steppe terrain kit; the link-layer art in full — jam dome decals, tether ribbon, relay masts, satellite patches, autonomy glint; **all Tier-1 units (6) and Tier-1 structures (5)**; night grade and dusk/dawn lerp; first device performance pass | AD, TA, **+generalist (m5)** |
| **8–11** | M2 "Sortie" | Tier-2 units (11) and structures (9); forest belt + treeline road kits; VFX core set (18 systems, 8 flipbooks); decal system and crater/burn vocabulary; icon set v1 (120); **first vertical-slice screenshot for the publisher**; thermal governor tuned on device | 3 + VFX contractor |
| **12–15** | M3 "Lockstep" | Tier-3 units (6) and structures (4); village + industrial kits; damage states across the roster; infantry animation (contractor); 4 skirmish maps art-passed; **G5 contact-sheet review #1**; colourblind pass | 3 + anim contractor |
| **16–19** | M4 "Doctrine" | River kit; **winter override**; remaining VFX (22 systems, 8 flipbooks); hero props and wreck kits; 4 more skirmish maps + 3 2v2; campaign 2D begins (mission cards, crests); icon set complete; engine-upgrade art revalidation | 3 + UI contractor |
| **20–22** | M5 "Content" | 18 campaign map art passes; loading frames and key art; store and press assets; **full-roster readability audit at 1× on all three device tiers**; polish pass driven by playtest telemetry; G5 #2 and #3 | 3 + concept burst |
| **23–24** | M6 "Early Access" | Bug-fix and optimisation only; **feature freeze on new assets at month 22**; day-one patch art; the 30-minute thermal sustain test on every tier device as a release gate | 3 |

Two rules make this schedule survivable. **First, the pipeline is proven end-to-end in month 3 on nine assets** — if trim sheets, the house model, retopo, LODs and the importer do not work together at month 3, we find out when it costs nine assets rather than two hundred. **Second, new-asset production stops at month 22**, eight weeks before early access, because every project that does not set that date discovers it anyway, later, worse.

---

## 9. Risks and mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **A14 thermal throttling makes 60 fps unsustainable for 30 minutes** | High | High | The four-rung governor (§4.7) built at M1, not M5; nightly 30-minute device test from M2; render scale and particle budgets are the first two rungs precisely because they are invisible. Fallback: 60 fps on Tier A, 40 fps cap on Tier B, announced honestly |
| 2 | **Small drones do not read at strategic zoom despite the size band** | Medium | High | The flight-marker rule (§1.7) is the mitigation and it is built at M1. If it still fails, the fallback is to raise the flight-marker threshold to default zoom, i.e. individual drones are never the read above tactical |
| 3 | **Style drift across 620 assets** | Medium | Medium | §6.3, seven mechanisms; the trim-sheet and four-material caps make the worst drift structurally impossible |
| 4 | **A generative tool's terms change mid-project, or a licence turns out to be unusable** | Medium | Medium | TRELLIS.2 (MIT, self-hosted) and Substance are maintained as licence-clean fallbacks for 3D and 2D respectively from month 6; the house model's training corpus is ours, so we can retrain elsewhere; no asset depends on a single vendor's continued goodwill |
| 5 | **Stage-3 cleanup is slower than 1.0–1.8 days per unit** | Medium | High | This is the schedule's load-bearing estimate. Measured on the nine month-3 proving assets; if it lands above 2.2 days the response is to cut structure faction-variants (10 days) and hero props (28 days) before cutting units |
| 6 | **The 22-unit roster cannot be made mutually distinguishable** | Low | High | The Hamming test runs from unit two, so failures surface at design time. If two units genuinely cannot be separated, the correct fix is a gameplay one — merge them, as the gameplay spec already did eleven times in §17 |
| 7 | **Night is beautiful in stills and unreadable in play** | Medium | Medium | The emissive-only-on-detected rule and the units-graded-30%-less rule are both testable at M1; the night variant of the 1× test (§6.4) is a gate, not a review |
| 8 | **Fiber tether rendering is too expensive at 250 concurrent on mobile** | Medium | Medium | Three tether LODs and the single-mesh rebuild are specified; if it still costs more than 0.6 ms on Tier B, the fallback is to draw full polylines only for the local player's own tethers and straight anchor-to-drone lines for observed enemy ones |
| 9 | **The art director becomes a full-time gatekeeper and stops producing** | Medium | Medium | Automate G3/G4/G6 completely (they are already specified as automated); if her production time drops below 40%, hire the fourth seat at month 10 rather than at month 18 |
| 10 | **Two rulesets (iPad and iPhone Compact) become two art targets** | Low | Medium | The point band does not change between devices (§2.6); only zoom stops and overlay count do. This is enforced by making both device profiles the same asset set with two camera configs |
| 11 | **Disclosure norms tighten and we are caught without provenance** | Low | Medium | Per-asset metadata from day one (§6.3, §7.5); we can produce a provenance report on demand |
| 12 | **Wreck-field and decal accumulation makes late-game maps visually noisy** | Medium | Low | Decal priority ring buffer with eviction (§3.5) and the 400-thread litter cap; tunable in one scriptable object; playtest-driven |

---

## 10. Open questions

1. **Is the locked sun azimuth worth what it costs the maps?** Every map is lit from 128° forever, which means no map can use dramatic backlighting or a dawn silhouette as a set piece. We think the altitude-cue payoff is worth it. If map artists rebel, the fallback is a per-map azimuth that is constant *within* a map, which keeps the altitude cue valid and costs us only the ability to share baked AO across maps.

2. **Does the 3.6 m minimum airframe size look absurd next to true-scale structures?** A drone that is nine times life-size hovering beside an honestly-scaled Drone Workshop is a lie the player may notice. Every RTS tells it; ours tells it harder because our smallest unit is smaller than any RTS unit has been. Needs a side-by-side test at month 3, and the answer may be to push structures to ×0.85 to close the gap.

3. **Should faction paint be more visible than 3%?** `spec-ui.md` is right that allegiance must dominate in match, but a player who never sees their faction's identity in the world may feel the factions look the same. Candidate compromise: faction-distinct *structure silhouettes* (already budgeted, 8 buildings) carry the identity, and units stay near-neutral. Needs a playtest read.

4. **Is the wargame-table read too clean for a war game?** The style is legible and distinctive; it may also be bloodless. The grime and weathering vocabulary is our only lever and it is deliberately restrained for readability. If the game reads as toy-like, the fix is more value variation in the ground plane and heavier accumulated decals, not more saturated units.

5. **Can one 3D generalist really carry 71 days of structures plus 57 days of units?** That is 128 days of a 20-month, ~400-day seat, which looks comfortable until the rework multiplier lands. The month-10 hiring trigger in §8.3 may need to be a month-8 trigger.

6. **Do we ship the Compact roster's six cut units as art anyway?** They exist on iPad and PC, so the assets are made regardless. The question is whether iPhone downloads them. Recommendation: yes, one asset set, because a 90 MB saving is not worth two build configurations — but this is a producer's call, not an artist's.

7. **What happens to the art at Tier 4?** `spec-futures-2027-2028.md` reserves hooks for jet-class drones, balloon relays, laser nodes and AI turrets. None of them are budgeted here. A Tier-4 era pack is roughly **9 units, 4 structures, 6 VFX systems ≈ 48 days**, which is a post-early-access content drop and should be scheduled as one.

8. **Does the house model survive its own success?** By month 18 the model will have been retrained on descendants of descendants. Generational drift in a self-trained model is a real and under-studied failure. Mitigation is to always retrain from the original 28 frames plus a curated addition, never from the previous model's output wholesale — but we should measure this at G5 #2 and be willing to freeze the model entirely from month 16.

9. **Is `spec-technical.md` §12.7 going to be updated?** It currently specifies a PC minimum spec and no iOS budgets, which contradicts the platform order in `brief.md` and `spec-gameplay.md` §19. §4 of this document is written as the authoritative iOS budget in the meantime. The two documents need reconciling before M1, and the technical spec should adopt these numbers rather than the reverse.
