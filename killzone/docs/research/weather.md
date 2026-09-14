# Weather

Research note for KILL ZONE. Compiled 14 September 2026. Answers §4 of
`BRIEF-navigation.md` and the weather gap the designer has flagged twice.

**Scope.** What weather does to flight, sensing, batteries, the ground and the men
on it; whether the extinction coefficients in `thermal-optical.md` §6.5 are right;
and how many weather states the interface should draw.

**Markers.** **[M]** measured or vendor-specified · **[R]** reported · **[I]**
inference or my arithmetic · **[E]** my estimate for the designer, i.e. a
placeholder you should be willing to overrule.

**The caveat that governs everything below.** The egress proxy blocked direct page
fetches for essentially every domain tried — Nature, PMC, MDPI, Wikipedia, Forbes,
The Conversation, Kyiv Independent, RAND, FLIR, AMS, Spire, Substack. **Every
figure below came from a search-engine summary, not the page.** Good enough to
design against; not good enough to ship with a citation. ±20% on any single number
until a human opens the source.

---

## 1. Summary

1. **Weather is a bigger lever than any masking measure in the project.** Ten years
   of ERA5 reanalysis put a "common" drone — 0–40 °C, 10 m/s wind, no rain — at a
   **median 5.7 flyable hours a day globally, 2.0 in daylight**, against **20.4 and
   12.3** for a weather-resistant one [M, snippet]. A **3.5× swing**; nothing in
   `decoys-masking.md` moves anything by 3.5×. And the most useful single number
   here: raising the wind threshold 10 → 15 m/s and precipitation 0 → 1 mm/h lifted
   flyability at major population centres **from 41% to 87%** [M, snippet].
   **Weatherisation is a ~2× availability upgrade** and should be purchasable tech.

2. **The bad states have different victims, and that decides the interface.** Wind
   kills the *small and cheap*; rain and icing kill the *electric*; fog and low
   cloud kill the *sensors* and ground nothing at all. Collapse them into one slider
   and the mechanic is gone; split them into three dials and you have a submenu.
   §8 resolves it with one axis, four states and a season stripe.

3. **The electric/combustion asymmetry is the real story.** Ukraine's interceptors
   are battery-powered and pilot-guided; Russia's Shahed/Geran are two-stroke or
   jet, self-heating, fly above the cloud deck and guide themselves [R]. Winter
   degrades the defender and not the attacker — interceptor "success rates fall off
   a cliff once winter or weather sets in" [R]. **Weather should be the one
   explicitly asymmetric system in the game.** Fog in particular grounds nothing and
   blinds everything: front-line fog cut visibility to about 50 yards, with an
   operator saying it "doesn't matter much whether a drone uses a thermal or night
   camera" [R], and Russian forces repeatedly timed assaults to it [R].

4. **The fog row of the `thermal-optical.md` §6.5 table is wrong by 4–6×, and it is
   already feeding a tool.** It puts LWIR extinction at 0.13× visible; the measured
   literature puts LWIR at 0.5–1.0× and MWIR at ~1.0× (my §6.3). The visible column
   of that table is exactly right and verifiable without a source — Koschmieder
   throughout (my §6.1) — but it mixes "add" and "total" conventions and will
   double-count if coded naively (my §6.5).

5. **Icing is a hard stop, confirmed.** Wind-tunnel work on a small-UAV propeller:
   **thrust down 27.6% in the first 60 s of accretion, power up 184%** [M, snippet].
   No loaded electric airframe has that margin, and none will by 2028.

6. **Mud runs on a different clock and belongs on a different control.** Rasputitsa
   forces movement into "narrow and often predictable corridors" [R] — the netted
   corridors of `ground-logistics.md` §8 — so mud does not add a mechanic, it
   *concentrates* one. And **deep winter is good for ground movement and bad for
   drones**: frozen ground beats dry ground off-road while cold halves electric
   endurance. A strategic window a player can plan an offensive around.

---

## 2. Flight, by drone class

### 2.1 Availability baseline

The ERA5 flyability study [M, snippet — *Scientific Reports*, 2021] is the only
thing close to "sorties versus measured weather" in the open literature, and its
finding that the binding thresholds are **wind and precipitation**, not
temperature, is the right shape: cold costs endurance, not sorties. **[E]** Assume a fair-weather FPV quad is weather-available
**55–65% of annual hours**, a weatherised or combustion airframe **85–90%**, with
the gap concentrated October–March.

### 2.2 Wind, by class

| Class | Published limit | Type |
|---|---|---|
| Small FPV quad (freestyle build) | 12 m/s "and beyond" — has thrust margin a camera drone lacks | [R] |
| Camera quad (Mavic 3 class, widely used for frontline recon) | **12 m/s** rated; "only effective below about 12 m/s" in service | [M]/[R] |
| Enterprise quad (Matrice 4) | **12 m/s**, IP54, battery −10 to 40 °C | [M] |
| Heavy multirotor (Vampire class, ~15 kg payload, ~23 min) | none published | [U] |
| Fixed-wing recon (Shark / Shark-M) | **20 m/s flight, 10 m/s takeoff, 7 m/s landing, dry only** | [M] |
| Fixed-wing recon (RQ-20 Puma AE) | **25 kt ≈ 12.9 m/s**, −29 to 49 °C, **25 mm/h rain** | [M] |
| Combustion strike / loitering munition | none published; inferred high | [I] |
| Interceptor (electric, $1,000–4,000 class) | none published | [U] |

The rule: **rated wind limits cluster at 12 m/s for everything electric and
rotary, and rise to 20 m/s only for fixed-wing with real wing loading.** Note the
Shark's three tiers — 20 m/s in flight, **7 m/s to land**. For a game that models
sortie generation the *landing* number binds, not cruise.

Two corrections before those numbers reach the simulation. **Rated is not
operational** — operators work at 60–70% of rated wind [R], so **[E]** apply 0.7
(12 m/s rated → 8.4 m/s working). And **wind costs endurance before it costs the
sortie**: a hovering multirotor in wind is flying continuously at the wind speed,
and one planning source puts **−10 °C plus 20 km/h wind at ~40% of the warm, calm
spec** [R]. **[E]** `endurance × (1 − 0.04 × wind_ms)`, floor 0.4, half that slope
for fixed-wing.

**Heavy multirotor**: no vendor figure surfaced. **[I]/[E]** More disc area than a
Mavic but far more frontal area and worse loaded thrust margin, and the binding
limit is *drop precision*, not airframe survival. Put it at **14–16 m/s empty,
10–12 m/s loaded**.

**Fibre-optic quads deserve an extra wind penalty the game does not model.** Fibre
drones "don't fly well in high winds, heavy rain or snow — rain can weigh down or
partly obscure the fibre line, and strong winds make low-level flight treacherous"
[R], and a multi-kilometre cable dragged through wind and vegetation breaks
routinely [R]. The game gives the fibre quad RF signature **0**, which reads as a
strict upgrade. **[E]** Give it −25% on the wind threshold plus a per-minute
link-loss probability scaling with wind and foliage: its unjammability should be
paid for in weather.

### 2.3 Rain

The cleanest hard stop in the subject, because it is ingress, not aerodynamics.
Most consumer and enterprise quads are **IP54 at best** — splash-resistant — and
vendors say plainly not to fly them in rain [M]; the flyability study's
common-drone precipitation threshold is **0 mm/h** [M]. The exception is built for
it: **Puma AE, 25 mm/h** [M], the right reference for what "weatherised" costs.
Ukrainian practice at the cheap end is **conformal coating** — silicone or acrylic
over the flight controller and ESCs, two coats for real rain, negligible mass, ~$12
of material — good for "splashes, drizzle and short dunkings", not sustained heavy
rain [M/R].

**[E] Make rain two-tier, not continuous:**

| Rain | FPV quad | Heavy multirotor | Fixed-wing recon | Combustion strike | Interceptor |
|---|---|---|---|---|---|
| Drizzle < 1 mm/h | Coated flies; uncoated 20%/sortie loss | Degraded | Fine | Fine | Degraded |
| Moderate+ ≥ 4 mm/h | **Grounded** unless weatherised | **Grounded** | Endurance ×0.7 | Fine | **Grounded** |

Make the FPV stop hard rather than probabilistic: the video link and camera fail
before the airframe does, and a blind FPV is not degraded, it is lost.

### 2.4 Icing — confirmed hard stop

- **Thrust down 27.58% in the first 60 seconds of ice accretion; power consumption
  up 184%** [M, snippet, icing wind tunnel, small-UAV propeller]. Torque rises and
  thrust falls roughly linearly with accretion.
- UAVs are *more* icing-sensitive than manned aircraft — smaller, slower [M].
- Field confirmation both sides: "batteries lose charge faster, the cameras and
  wires freeze, they just ice up" [R, Feb 2026]; icing "makes propeller blades
  thicker, heavier and less aerodynamic" and "can ground large numbers of drones"
  [R].
- **Even combustion strike drones are not immune.** Ice forms on Geran front
  surfaces; Shahed-type winter variants heat some internal components but have **no
  heating on aerodynamic surfaces** [R].

A 27.6% thrust loss inside a minute is outside the margin of any loaded multirotor.
**[E] Icing is a hard stop for every propeller-driven airframe, with no upgrade
that removes it** — the purchasable mitigation should be an *icing forecast*, since
real ice protection on a 15 kg expendable is a research programme, not a field
modification. Icing conditions are narrow, roughly **0 to −15 °C with visible
moisture**, which makes it the intersection of cold and wet — hence §8 folds it into
the wet state with a winter flag rather than giving it a state.

### 2.5 Heat and density altitude — small, and it is fine to ignore

Eastern Europe is low-lying, so density altitude is almost purely a temperature
effect. Rule of thumb **~3% thrust loss per 1,000 ft** of density altitude [R;
sources give 2.7–3.5%]; a 30 K rise at sea level is worth roughly 3,000 ft [I], so
a 35 °C afternoon against a 5 °C morning costs about **9% of thrust** and hence of
payload margin. **[E] Not worth a weather state** — fold it in as a flat −8%
multirotor payload/endurance in June–August, or drop it. It is an order of
magnitude smaller than everything else here, and the heat effect that matters is
thermal crossover, which `thermal-optical.md` already handles.

### 2.6 The asymmetry that should drive the system

Reporting on winter 2025–26 is consistent: Ukraine's interceptors are
lithium-powered and guided by active link to a pilot; Shahed-types are two-stroke
or jet, generate their own heat, spend more time above cloud, and have more
automated guidance [R]. Every clause is a mechanic — battery → cold penalty; pilot
link → weather-sensitive video; combustion → self-heating; above cloud → immune to
murk; autonomy → immune to the operator-blinding that neutralises the interceptor.
**[E] This is the cleanest available answer to "why buy the expensive airframe",
and it is true.**

---

## 3. Cold

### 3.1 Battery capacity — the number the game needs

Assembled from several sources agreeing within a few points [M/R, all snippet]:
+10 °C ≈ 90%; 0 °C **80–85%**; −10 °C **50–70%** ("can exceed 50% reduction");
−18/−20 °C **~50%** (Battery University: 100% at 27 °C → 50% at −18 °C); below
−20 °C, cell capacity collapses roughly an order of magnitude by −30 °C.
Practitioner rules land in the same place: plan **15–25% less flight time**, cap
sorties at **60–70% of mild-weather duration**.

**[E] The ladder to ship:**

| Ambient | Electric endurance × |
|---|---|
| ≥ +10 °C | 1.00 |
| 0 °C | 0.85 |
| −10 °C | 0.65 |
| −20 °C | 0.50 |
| −20 °C with pre-heat kit | 0.70 |

Two refinements worth the code. **Packs self-heat under load**, so practical loss
is milder than cell-test data — the first minutes are the worst. And **the
counter-measure is real, cheap and fielded**: Ukrainian units wrap batteries in
disposable chemical heated insoles immediately before launch, **under 100 g on a
45 kg drone**, preventing the voltage sag that costs range [R, Feb 2026]. That
should be an item — **a cheap consumable that buys back most of the cold penalty
and must be stocked forward** — which ties cold to logistics instead of leaving it
a passive debuff.

### 3.2 Cold by class — the counter-intuitive one

> "In cold weather, FPVs are more capricious, bomb-droppers more stable." — Maj.
> Maksym Taran, 4th Rapid Response Brigade "Rubizh" [R, Jan 2026]

Cold "sharply reduces FPV flight time, decreases power system stability, and
increases the likelihood of losing a drone before it reaches its target", while
bomber drones show more stability [R].

**[I]** Thermal mass and C-rate explain it: a small FPV pack is light, cools fast
and is discharged hard, so high current through high cold internal resistance gives
voltage sag and brownout, while a heavy multirotor's pack is large, self-heats and
runs at a lower C-rate. **[E] Apply the §3.1 ladder to heavy multirotors and a 1.3×
steeper version to small FPV and interceptors**, plus a per-sortie loss probability
rising with cold. It inverts the usual "bigger is more fragile" instinct, which is
what makes it worth modelling.

### 3.3 Cold and the optics

Cold is **good** for thermal and the game should reward it: a cold background
raises the contrast `thermal-optical.md` §6.2 computes, and Ukrainian night-bomber
operations are reported as *more* effective in winter for that reason [R, Jan
2026]. Three offsetting effects:

- **Snow blocks infrared** during snowfall, and a snow structure is nearly
  invisible to thermal because snow insulates [R].
- **Fresh footprints in snow read as a contrast anomaly** and are trackable from
  the air [R]. Cheap to implement, true, and a lovely mechanic.
- **Hardware freezes** — cameras and wires "just ice up"; one interceptor mission
  was lost mid-flight when the camera froze [R].

**[E]** In the frozen state: thermal reach **×1.15** on top of the night
multiplier; ground units leave a thermal trail with a 10–20 min half-life; 3–5%
per-sortie sensor-failure chance for unheated optics.

### 3.4 Cold and the crews

**Dexterity, not hypothermia, is the binding constraint**: modelling of Russian
troops at −20 °C did *not* support mass hypothermia, it predicted **peripheral
freezing cold injury, especially to the feet** [M], and hand dexterity is lost in
roughly **65 minutes** at low thermal protection [M]. **Non-freezing cold injury
needs only wet and cool**: immersion foot occurs at **0–15 °C over more than 12
hours** of wet exposure [M] — the rasputitsa condition, not the deep-winter one.
**[E]** So put the crew penalty on the **wet** state and on **exposure duration**,
not on the thermometer: a unit wet for a day degrades, a dry unit at −20 °C with
shelter does not. It also gives the player a reason to rotate forward positions.

---

## 4. Mud and the seasonal ground state

`ground-logistics.md` already has the band, the 10–14% per-mission UGV loss rate,
the mover mix and the 822 km of netted corridors. This section adds only what that
document lacks: what the ground underneath it is doing.

### 4.1 Rasputitsa

The semi-annual mud season across Belarus, Russia and Ukraine, when unpaved roads
and open ground become effectively impassable [R]. **Autumn**, rain-driven, roughly
**late October into November**, ended by the freeze. **Spring**, snowmelt over
still-frozen subsoil, roughly **March into April**, and consistently the worse of
the two because meltwater cannot drain [R].

The soil is why it is severe rather than annoying: chernozem covers about
**two-thirds of Ukraine**, absorbs water readily and sits over **clay-laden, poorly
draining** subsoil [R]. Saturated, it "can immobilise tanks, stall convoys, and
confine heavy artillery to hardened roads" [R].

### 4.2 By mover

Ground pressure is the discriminator: tracked AFV peak pressure under roadwheels
**~90–150 kPa** [M, modelling] against a heavy wheeled truck's **~460 kPa** [M/I].
That 3–5× gap is the whole mechanic, and Ukrainian forward-leg UGVs are
preferentially tracked, with purpose-built mud-and-snow variants (Bizon-L, 300 kg /
50 km) [R]. Doctrinal baseline: infantry make **4 km/h on roads, 1.6 km/h
cross-country** in good weather [M] — a 2.5× penalty before any mud.

**[E] Ground-state multipliers:**

| | Firm | Mud | Frozen |
|---|---|---|---|
| Wheeled truck, on road | 1.00 | 0.75 | 1.00 |
| Wheeled truck, off road | 0.60 | **0.00 no-go** | 0.80 |
| Tracked vehicle, off road | 0.70 | 0.35 | 0.85 |
| Tracked UGV, off road | 0.70 | 0.40 | 0.85 |
| Wheeled UGV, off road | 0.65 | **0.10** | 0.80 |
| Infantry, off road | 0.40 | 0.20 | 0.50 |

Two things to notice. **Frozen ground beats firm ground** off-road — historically
correct, and it gives the player a reason to want deep winter. And **mud does not
slow the road network at all**; it removes the alternative to it.

### 4.3 The interaction that matters — corridors

The load-bearing sentence: in both thaws, fields and backroads become "sloppy pits
of mud and water, forcing troops and equipment into **narrow and often predictable
corridors**" [R]. `ground-logistics.md` §8 already notes that nets "defeat a casual
dive attack, not a determined one, and **they mark the route**". Mud is the
multiplier on that existing tension and needs no mechanic of its own.

**[E] In the mud state:** wheeled logistics is confined to the road graph, so
corridor throughput is the only throughput; the corridor's marking penalty worsens
as the attacker's search problem collapses; per-mission UGV loss rises from 10–14%
to **18–25%**, from route predictability and immobilisation rather than better enemy
drones; tracked UGVs keep the off-road option and become disproportionately
valuable, a clean "buy tracks" signal; and corridor construction (5–12 km/day in
early 2026 [R]) halves, then stops.

Ground assault is also *harder* for the attacker, partly offsetting the fog and
rain that arrive with autumn mud and make it easier. **The drones are blind but the
tanks are stuck** is the most interesting thing weather can do to this game.

---

## 5. Cloud and overcast

### 5.1 Frequencies

Kyiv as the theatre reference [M/R, climatological summaries, snippet]: **annual
mean cloud cover 67%** against a 65% global mean; **December overcast or mostly
cloudy about 71% of the time**, clear or partly cloudy ~6.5 h/day (27%); December
precipitation 45 mm over 9 days, mean −1.7 °C. **[E] Seasonal ladder**, interpolated
from the annual mean and the December figure and explicitly an interpolation:

| Season | Hours with usable clear sky |
|---|---|
| Jun–Aug | 0.45–0.55 |
| Apr–May, Sep | 0.35–0.45 |
| Oct–Nov | 0.25–0.30 |
| Dec–Feb | 0.20–0.30 |

### 5.2 Cloud base

Not found as a regional dataset. What is established: **stratus base is typically
below 300 m**, often between the surface and ~300 m, with stratus generally in the
surface-to-2 km layer [M]. Winter Eastern European low cloud is overwhelmingly
stratus and stratocumulus over a cold continental surface. **[E]** Two tiers are
enough: **low deck, base 100–300 m** (the drone is in it or under it) and **mid
deck, 600–1,500 m** (drones fly under it, satellites see nothing).

### 5.3 What cloud blocks

**Satellite optical reconnaissance.** Optical satellites "lose coverage over
Northern Europe in winter" [R]. Commercial tasking asks for a maximum cloud-cover
percentage, typically **10–20%** [R]. One regional study found **up to 57% of
images affected by cloud**, and effective revisit — the gap between *usable* images
— diverges badly from nominal: **6.9 days for Sentinel-2 against a nominal 5, over
21 days for Landsat** [M, snippet]. That **3–4× nominal-to-effective factor** is the
number a designer wants. **[E]** Do not model satellite optical recon as a timer;
model it as a timer **times a per-attempt success roll equal to the clear-sky
fraction**. In December the pass usually returns nothing, and the player learns to
buy the radar satellite.

**Star trackers** (for the celestial-navigation agent). Availability is a property
of **altitude, not airframe**: above a low stratus deck the sky is clear by
definition, so anything at 1,000 m+ has stars whenever the deck is low. Only mid
and high cloud denies a high star tracker, and that is a smaller fraction than
total cover. **[E] Two numbers: ~0.25–0.35 availability below 500 m in winter,
~0.55–0.70 above 2,000 m year-round** — inferred from §5.1, not measured.

**High-altitude drone observation.** Same geometry, opposite sign: a recon aircraft
above the deck cannot see through it. A low deck does not ground fixed-wing recon,
it makes it useless — mechanically different, because the airframe survives and can
be redirected. And the reverse advantage: Shahed-types "spend more flight time high
above cloud cover" [R], so the deck hides the raider from the ground-launched,
visually guided interceptor while the raider navigates on satellite and inertial.

---

## 6. Auditing the extinction table in `thermal-optical.md` §6.5

The table is marked "[T] textbook, recalled — verify". Here is the verification.

### 6.1 Visible column — verified correct

Every visible entry is Koschmieder, α = 3.912/V at 550 nm and 2% contrast. Check:
23 km → 0.170 (table 0.17); 10 km → 0.391 (0.39); 5 km → 0.782 (0.78); 200 m →
19.56 (19.6) [I]. The three clear rows back out to visibilities of 30, 23 and
15.6 km, all reasonable. **Leave alone.**

### 6.2 Clear-air and haze rows — accept, with the textbook marking kept

No fetchable MODTRAN table to check 0.08/0.11, 0.14/0.19, 0.30/0.50 against, but
the sign is right: the 8–12 µm window is bounded by the water vapour continuum, so
LWIR must degrade faster with humidity than MWIR, and it does. Clear-air extinction
near 10 µm is **absorption, not scattering** — clear-weather scattering there is
given as **4.5 × 10⁻³ km⁻¹** [M, snippet], two orders below the table's totals — so
these are purely a water-vapour budget, which is correct, and the 3× LWIR / 2× MWIR
swing across the three rows is the right order. The haze rows encode aerosol
scattering falling off roughly as λ^−q, right in kind [M]; the implied ratio
corresponds to **q ≈ 0.7** [I], inside the 0.5–1.5 range for continental haze.
**Keep both, keep the "textbook" marking, and do not read the third significant
figure as meaningful.**

### 6.3 Fog row — wrong, and this is the important one

The table gives fog at V = 200 m as **MWIR +3.0, LWIR +2.5** against visible 19.6 —
LWIR at **0.13× visible**, i.e. thermal seeing eight times further than the eye.
The literature does not support it:

- "The extinction coefficients in the visible (0.55 µm), the near IR (1.2 µm) and
  the mid IR (3.7 µm) are **comparable to** and roughly **twice as much as** that in
  the far IR (10.6 µm) **when visibility is less than a few hundred metres**" [M,
  snippet]. That is MWIR ≈ 1.0 × visible, LWIR ≈ 0.5 × visible — at exactly the
  V = 200 m case the table addresses.
- A Mie calculation over **236 measured droplet distributions**: at 11.5 µm, **60%
  of computed coefficients are smaller than the 0.55 µm reference, 5% below 0.7×,
  a few below 0.2×** [M, snippet]. Typical ratio **0.7–1.0**, not 0.13.
- The far-IR advantage **grows as visibility exceeds 500 m** and shrinks as fog
  thickens; the table's optimism would suit thin mist, not V = 200 m.
- Independent corroboration: front-line fog cut visibility to ~50 yards and "it
  doesn't matter much whether a drone uses a thermal or night camera" [R]. If the
  table were right, thermal would still work at a kilometre.

**[E] Replacement:**

| Condition | 3–5 µm | 8–12 µm | Visible |
|---|---|---|---|
| Fog, V = 200 m | **+18** | **+11** | +19.6 |
| Mist, V = 1 km | **+2.4** | **+1.2** | +3.9 |

Derived as MWIR = 0.9 × visible and LWIR = 0.55 × visible in dense fog, relaxing to
0.6× and 0.3× at V = 1 km as the far-IR advantage returns [I]. Inferences from two
snippet-sourced papers, and a human should check them — but the direction is not in
doubt and the current row is low by roughly **6× in MWIR, 4× in LWIR**.

**Effect on the game.** Thermal in fog loses an e-folding every 90 m instead of
every 400 m, and the 2.4 km sky-backed quad detection from `thermal-optical.md` §1
collapses to a few hundred metres. Fog becomes **the state in which nobody can
see** — which is what the reporting describes and what the game needs.

### 6.4 Smoke rows — right numbers, wrong labels

"Carbon / HC battlefield smoke: +2.0 / +1.5 / +20" describes an obscurant 13× more
effective in the visible than the long-wave. Correct for **fog oil, HC and white
phosphorus** — droplet aerosols around 0.3–1 µm, with "a completely clear window"
in the IR (WP defeats thermal only while still hot, and it cools within seconds)
[M/R]. **Not** correct for carbon: measured mass extinction is **graphite ~0.72 m²/g
in both spectral bands, brass flake 0.34 m²/g** [M, snippet, NRL]. "In both bands"
is the entire point of engineered carbon.

**[E] Split the row:**

| Condition | 3–5 µm | 8–12 µm | Visible |
|---|---|---|---|
| Fog oil / HC / WP screen | +2.0 | +1.5 | +20 |
| Graphite / carbon-fibre obscurant | **+8** | **+8** | +12 |
| Multispectral obscurant (metal flake) | +15 | **+15** | +30 |

I have also equalised the multispectral bands (the table had MWIR 15, LWIR 20):
measured flake obscurants are quoted as comparable in both, and at 2.1–2.3 µm
mass-median aerodynamic diameter [M, brass flake EA 5763] you would expect the peak
nearer 3–5 µm if anything. **Sanity check on magnitude**, since these look large:
α = σ_mass × C, so at σ = 0.34 m²/g an α of 15 km⁻¹ needs **44 mg/m³** [I] — an
ordinary concentration inside a screen. The row is physically achievable.

### 6.5 A latent coding bug

The table mixes conventions in one column. Haze and fog rows say "add" for MWIR and
LWIR but give visible as a **total** ("+0.39 total", "19.6 total"), because
Koschmieder already includes clear air. If the propagation tool adds every row,
**visible extinction in haze and fog is double-counted** by the clear-air baseline —
a 0.13–0.25 km⁻¹ error, negligible in fog, material in light haze. **[E]** Make
every row an increment (visible: +0.22 light haze, +0.61 moderate haze, +19.43 fog,
over a 0.17 clear baseline), or special-case the column. Someone should check what
the tool currently does.

### 6.6 Rain rows — form right, magnitude a little low

Structurally correct: raindrops are far larger than these wavelengths, so
scattering is near-geometric and band-independent, and the table having MWIR = LWIR
is right, with visible slightly higher also right. The standard form is β = A·R^B.
The table's two points imply **A ≈ 0.16, B ≈ 0.82** [I: ln(1.40/0.45)/ln 4 = 0.82].
Common stratiform fits sit nearer A ≈ 0.2–0.4, B ≈ 0.6–0.75, putting 4 mm/h at
0.5–0.9 km⁻¹ rather than 0.45. **[E] The 4 mm/h row is fine; consider raising
16 mm/h from 1.40 to ~1.8.** Relevant provenance: Middleton's classical expression,
still widely reproduced, **underestimated rain extinction by nearly a factor of
ten** through a misapplication of Stokes' law, and was corrected [M, snippet, RAND
R-1523]. If the recalled value came down that lineage, low is the expected error.

### 6.7 What the table is missing

1. **No snow row** — the dominant winter obscurant in this theatre, absent. **[E]**
   Snow attenuates more per unit equivalent liquid water than rain (more geometric
   cross-section per gram) and is likewise near-band-independent: suggest
   **moderate +0.8, heavy +2.5, all bands**, marked [E] honestly — I found no
   measured figures. Largest remaining hole for a winter theatre.
2. **No in-cloud row.** A drone inside a stratus deck is in fog (LWC 0.1–0.5 g/m³),
   and §5.2 puts the winter deck at 100–300 m — inside every airframe's operating
   band. **[E]** Use the corrected fog row (§6.3) for any path through cloud.
3. **No slant-path correction.** These are horizontal sea-level coefficients;
   applying them to an upward slant **over-attenuates**, worst in the warm-humid
   row. **[E]** Multiply by `0.55 + 0.45·cos(elevation)` — a shape, not a
   measurement.
4. **Humidity is quantised into three rows.** Fine for a game; the real variable is
   precipitable water.
5. **No turbulence/scintillation term** — not extinction, but it degrades
   recognition range on long sunlit horizontal paths. Relevant to the 600 m Gun
   Mount, irrelevant to anything airborne.

---

## 7. What changes by 2027–28

**1. Radar seekers on cheap interceptors — already happening.** Interceptors are
visually guided and "success rates fall off a cliff once winter or weather sets
in"; pilots vectored onto a target "look through the camera and see a wall of grey"
[R]. The stated fix is a radar seeker head, and a US firm was reported in **August
2026** selling one for **$4,000** [R]. Against a $1,000–4,000 interceptor that is a
2–5× cost increase, so it will not be universal — which is what makes it a good
tech choice. **[E] A per-unit upgrade that removes the murk penalty at 2–4× airframe
cost, on a minority of interceptors by 2028.**

**2. Machine vision does not solve weather, and the reporting says so.** A
Brave1-backed system automated roughly **95% of the engagement** in Kharkiv trials
in June 2026 [R], but machine vision is still vision — "a computer can spot a target
faster than a human, but it still can't see one if photons never reach it" [R].
**[E] Autonomy upgrades should improve the clear-weather kill chain and do nothing
at all in murk.**

**3. SAR from orbit gets cheaper and more frequent.** Small SAR constellations cut
revisit "from days to hours" since 2020 [R]; ICEYE, Capella and Umbra are all
expanding, and at least one operator targets a 16-satellite constellation at full
capacity **by 2027** [R]. **[E] By 2027–28 the radar satellite is the default
reconnaissance buy and the optical one is the cheap thing that fails in winter.**

**4. Weatherised airframes spread from the bottom up.** 24 and 77 GHz
millimetre-wave modules are cheap COTS [M]; conformal coating is a $12 consumable;
battery pre-heating is being done with boot warmers [R]. None of that is research,
it is a production decision. **[E] Expect the cheap end to weatherise faster than
the expensive end.** What will *not* appear is propeller de-icing on expendables —
the power budget does not exist, so **icing stays a hard stop through the game's
horizon.**

---

## 8. The design recommendation

### 8.1 One axis, four states, plus a season stripe

**One weather axis with four states, and one separate three-value ground state on a
season clock the player does not control.** Two controls, no submenu.

Four states on one axis rather than three sliders, because **the bad states have
different victims**: a single badness slider makes rain and fog interchangeable, and
they are opposites — one grounds the aircraft and leaves the sensors working, the
other grounds nothing and blinds everything. A player who cannot tell those apart
cannot plan, and planning is the whole point. Ground state stays separate because it
runs on a different clock: weather turns over in an hour, rasputitsa lasts six
weeks, and one control for both makes both illegible.

### 8.2 The four states

**CLEAR** (45–55% of summer hours, 20–30% of winter) is the baseline. The other
three as a matrix; bold marks a state change the player should feel:

| | WIND (12–18 m/s) | WET (rain ≥ 4 mm/h) | MURK (fog, or base < 300 m) |
|---|---|---|---|
| Small FPV quad | endurance ×0.5, reach ×0.6, P(hit) ×0.7; **grounded** at the top of the band | **Grounded** (weatherised: flies) | Flies, cannot acquire: **P(hit) ×0.3** |
| Fibre-optic quad | as FPV, −25% threshold, cable-loss risk | **Grounded** — wet fibre | as FPV |
| Heavy multirotor | endurance ×0.6, drop accuracy badly degraded | **Grounded** | Degraded; can still drop on surveyed coordinates |
| Interceptor | **Grounded** — light, must chase, must land | **Grounded** | **≈ zero** unless radar-seeker-equipped (§7) |
| Fixed-wing recon | flies, **cannot land**: sorties continue, none launch | endurance ×0.7, camera degraded; grounded in heavy rain | Flies above the deck, **zero recon value** |
| Combustion strike | unaffected | unaffected (icing face: degraded) | **Unaffected, and hidden** |
| Optical sensing | ×1.00 | ×0.55 | **×0.15** |
| Thermal sensing | ×1.00 | ×0.60 | **×0.30** |
| Radar / passive RF | ×1.00 | ×0.90 / ×1.00 | **×1.00** |
| Acoustic | **off above 12 m/s** (per `acoustic.md`) | ×0.80 | ×1.10 — fog comes with calm air |
| Ground movement | unaffected | advances ground state toward MUD | **Assault bonus, both sides** |

Three notes on the matrix. **WET carries a winter flag**: in freezing conditions it
becomes icing, which grounds every propeller-driven airframe including the
weatherised ones, and degrades the combustion strike drone too (internal heating,
no heated aerodynamic surfaces [R]). Crews in wet positions accumulate a penalty
after ~12 hours. **MURK is the state designers get wrong** — nothing is mechanically
grounded, the player still pays for every useless sortie, and one reported Ukrainian
response was to hunt by radio emission instead of camera [R], which the passive RF
channel already supports. Satellite optical and sub-deck star trackers are
unavailable. It is the state Russian forces repeatedly timed assaults to [R].
The thermal figure of ×0.30 uses the corrected §6.3 coefficients; with the old
table it would have been ×0.75, which is exactly the error this document exists to
catch.

### 8.3 The season stripe

Three values on a calendar, not a weather roll: **FIRM / MUD / FROZEN** — MUD in
late October–November and March–April, FROZEN December–February, FIRM otherwise.
Effects in §4.2 and §4.3. The rule the player must internalise: **mud does not slow
the roads, it deletes everything that is not a road** — and the roads are the netted
corridors that are already the most contested object in `ground-logistics.md`.

Cold rides on the stripe rather than being a state: apply the §3.1 endurance ladder
whenever FROZEN is set, steepened 1.3× for small FPV and interceptors, with the
heated-battery consumable as the counter.

### 8.4 Why not more, why not fewer

**Fewer** would merge rain and fog, which are opposites; two states makes weather a
binary debuff, the version the designer already suspects is not worth building.
**More** is unnecessary: icing is WET with the winter flag, which is why WET is
lethal in January and merely annoying in July; heat is worth 8% of payload; snow
sits between WET and MURK and should be drawn as whichever it resembles that day;
night is already a separate axis and should stay separate so it multiplies with
weather instead of competing.

**The test.** The player should be able to say, out loud, from the icon:

- "It's windy — FPVs are out, fixed-wing recon is up, push the heavy stuff."
- "It's raining — nothing electric flies, my combustion strike drones own the sky."
- "It's fogged in — everything flies, nothing sees. Move the infantry now."
- "It's March — nothing leaves the road, and the corridor is the war."

Four sentences, four states.

---

## 9. What is genuinely uncertain

1. **Every URL here is snippet-sourced.** The proxy blocked all of them. ±20% on
   any single number until a human opens the page, and **nothing here is measured by
   me** — the only claims I can stand behind unsourced are the Koschmieder checks
   (§6.1), the obscurant concentration check (§6.4) and the density-altitude
   arithmetic (§2.5), all marked [I].
2. **The corrected fog coefficients (§6.3) are inferences from two papers I could
   not read.** The direction is certain and independently corroborated by
   operational reporting; the values should be regenerated with Mie or MODTRAN
   access. §6.6's rain correction is likewise arithmetic on two table entries whose
   source I did not verify.
3. **Snow extinction is unsourced** — §6.7's suggestion is an estimate with no
   measurement behind it, in a winter theatre. The biggest remaining hole.
4. **No wind limit was found for heavy multirotors or interceptors.** Two of the
   five classes the brief names have no published envelope at all; §2.2 for them is
   reasoning from mass and disc loading.
5. **No published sortie-rate-versus-weather data exists that I could find.** War
   reporting is qualitative; the one quantitative anchor uses civilian thresholds.
   §2.1's availability fractions are extrapolations.
6. **Cloud base statistics for the theatre are absent** — §5.2 uses generic stratus
   climatology. A proper answer needs Ukrainian METAR archives, unreachable here.
7. **Soil trafficability thresholds are structural, not numeric.** NRMM and MMP are
   the right frameworks; the go/no-go water content for chernozem was not found.
   §4.2 is estimate disciplined by ground pressure, not derived from it.
8. **The electric/combustion asymmetry may be overstated as a general law.** Well
   evidenced for the interceptor-versus-Shahed matchup in winter 2025–26; whether it
   generalises to all electric and all combustion airframes is an inference.

### Sources consulted — all via search summary, none fetched

**Flight**: ukrspecsystems.com (Shark/Shark-M); enterprise.dji.com (Matrice 4);
en.wikipedia.org (RQ-20 Puma); goodtodrone.com, ukdronemap.app;
nature.com/articles/s41598-021-91325-w (drone flyability).
**Icing**: doi.org/10.3390/drones10030166; mdpi.com/2226-4310/10/3/261.
**Ukraine**: theconversation.com/…-268019; port.ac.uk;
forbes.com/sites/vikrammittal (Nov 2025, Jan 2026); kyivpost.com/post/70108;
kyivindependent.com; armyinform.com.ua (Jan 2026); newsukraine.rbc.ua; cnn.com
(Nov 2025); dronexl.co; euromaidanpress.com, wesodonnell.com (radar seeker, Aug
2026).
**Atmospheric optics**: opg.optica.org/ao (fog extinction–visibility, 2005);
mdpi.com/2076-3417/9/14/2843; rand.org/pubs/reports/R1523 (Middleton correction);
journals.ametsoc.org (rain); apps.dtic.mil/sti/pdfs/ADA375708.pdf (NRL obscurants);
ncbi.nlm.nih.gov/books/NBK224565, NBK224560.
**Ground and climate**: en.wikipedia.org/wiki/Rasputitsa; encyclopediaofukraine.com;
globalsecurity.org FM 5-430-00-1 ch.7; irp.fas.org ATP 3-21.18; meteoblue.com,
weatherspark.com, climatestotravel.com.
**Cold and crews**: tandfonline.com; ph.health.mil; batteryuniversity.com BU-502.
**Space**: eoportal.org; up42.com; eos.com, esa.int.
