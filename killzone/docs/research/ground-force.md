# The ground force: vehicles, infantry and robots, 2026–28

## 0. How to read this, and what it rests on

This document exists because the research corpus is five documents deep on
sensors and drones and nearly silent on the ground force those drones hunt. It
covers vehicle self-protection, the question of whether a fighting vehicle can
carry its own counter-drone turret, infantry manning and infantry counter-drone,
and the ground robots that are not logistics carriers — legged machines in
particular, on which the corpus had nothing at all.

**Evidence quality.** Everything below rests on **WebSearch result snippets**.
Direct page retrieval is blocked at the gateway in this environment, so no
article text was read. Fifty-six searches were spent. Where a figure appears in
one snippet only, it is marked as such. Where snippets from different outlets
agree, that is noted, because agreement between snippets is the only
corroboration available. No URL is cited as though it were read.

Marks used throughout:

- **[M]** measured or vendor-specified
- **[R]** reported by press or analysts
- **[C]** claimed by a belligerent or a vendor with an interest
- **[I]** inference from the above
- **[E]** my estimate, for the designer, not from a source

Read `point-defence.md` for the counter-drone weapon families themselves,
`ground-logistics.md` for logistics UGVs (deliberately not repeated here), and
`front-2026.md` sections 8 and 10 for armour employment and position manning,
which this document builds on rather than restates.

---

## 1. Summary

1. **A counter-drone turret on a fighting vehicle is real, is being fitted, and
   does not restore armour's freedom of movement.** It roughly doubles or
   triples the price of killing a vehicle. It does not change the fact that a
   tank is detected the moment it moves, or that the attacker can send more
   drones than the turret has ammunition. `[I, from R]` Section 3.

2. **The thing that dies is the truck, not the tank.** Unarmoured vehicles are
   ~**90% of Russian vehicle losses**, nine soft vehicles per tank or BMP `[R]`.
   What is fitted to those vehicles is thinner, cheaper and more improvised than
   what is fitted to armour.

3. **Cages defeat a shaped charge by disrupting the warhead, not by adding hit
   points** — and a cage that only adds standoff can make penetration *worse*
   `[R]`. One quantified operator claim: a tank with cages **and** a jammer
   needs **six to eight drones** instead of the usual two or three `[C]`.

4. **Vehicle electronic warfare is losing.** One 900 MHz module sufficed in
   early 2024; by that summer four to five were needed, by autumn seven `[R]`.
   Fibre-optic FPVs — 30–50% of Russian use `[R]` — are immune outright.

5. **Hard-kill APS met FPVs in 2026 and the result was ambiguous.** One Arena-M
   downed **seven FPVs before its tank was destroyed**; **15–20 FPVs** are
   reportedly needed to kill an Arena-M tank `[R/C]`. Trophy's quoted **55°**
   elevation limit leaves the vertical drop attack open `[R]`.

6. **Infantry positions are two to four people and shrinking.** Strongpoints for
   30–100+ are being replaced by distributed positions for **three to eight**
   and by individual holes `[R]`, with **20–40 cm of compacted earth over steel**
   as standard overhead cover `[R]`, and increasingly by tunnels.

7. **What a person can do about a drone is hear it, hide, and occasionally shoot
   it.** Shotguns work to ~**50 m**, and the safe standoff for detonating a
   15-inch FPV is now **40–50 m** — the window is closing from both ends `[R]`.
   Detection is the half that works: pocket alerts and 4 km RF detectors.

8. **Legged and dog-type robots are, as of late 2026, essentially marketing.**
   Ukrainian operators called quadrupeds *"a nice toy with no application on the
   front line yet"* `[R]`; China's armed "machine wolves" have **no verified
   combat deployment as of August 2026** `[R]`; the PLA reportedly holds **100+
   procurement records for combat humanoids and zero deployments** `[R]`. The
   game should state this by *absence*.

9. **Armed UGVs, by contrast, are load-bearing.** One held a position for **45
   consecutive days** with no human present `[R]`; robots have forced Russian
   surrenders `[R]`; **1,000+ UGVs flew 66,000 missions** in 2026 `[R]`. Armed
   modules cost **$12,000–35,000** per platform `[R]`.

10. **Nothing in open reporting matches "Frogger."** Section 9.

---

## 2. Vehicle self-protection against drones

### 2.1 Cages, slat and screens — what they actually do

The mechanism is not "more armour." A slat or cage defeats a shaped charge by
**crushing the cone, preventing proper standoff detonation, or destroying the
fuze** before the jet forms `[R]`. The effect is therefore **probabilistic and
geometry-dependent, not a hit-point buffer**.

- A modelling source surfaced in search gives cage effectiveness from **below
  0.30 for coarse, poorly placed grids to above 0.80 for fine,
  disruption-dominated configurations** `[R, single snippet of unclear
  provenance — treat the *spread* as the finding, not the endpoints]`.
- Critically: **a cage that merely adds standoff without disrupting the warhead
  can raise penetration rather than lower it** `[R]`, because a shaped charge at
  its optimum standoff is more effective. This is the most game-relevant fact
  about cages.
- The 2022 Russian "cope cages" were **largely ineffective** `[R]`. The 2025–26
  generation is purpose-fabricated, modular, lower and more rigid, made per
  vehicle type rather than welded ad hoc `[R]`.

**Angle dependence is the failure mode to model.** Reporting on the Russian
T-80BVM describes **layered protection built specifically to counter multi-angle
FPV attacks** `[R]` — an admission that single-plane cages were being flown
around. An FPV attacking the engine deck, running gear or low flank is attacking
a different protection state from one diving on the turret roof.

**The variants in use**, all `[R]`: rigid cage ("mangal", "barbecue") over roof
and turret; **cable curtains** ("hedgehog", "porcupine") of heavy aluminium
cable unwound into strands and welded to the cage — one account puts this at
**around three tonnes** on a single tank; chains and grilles over the engine
deck; full sheet-metal "turtle" shells; **Romashka ("Daisy")**, a Russian 2026
design of radially arranged cables that lets the turret rotate freely without
blocking optics; and **hinged screens** (an Omsk design) that the crew can unpin
and fold back to free about half the turret's arc `[R, both Jan 2026]`.

### 2.2 What it costs the vehicle

The reporting is unusually blunt, and this is where the game gets its trade-off:

- **Mobility.** A Russian driver described a tank with hedgehog armour failing a
  drivetrain component **before it had covered 10 km** `[R]`.
- **Vision.** Turtle-tank crews could see **only a narrow forward arc**; the
  shell "blocked the views from side to side and above" `[R]`.
- **The gun.** Many field-fitted installations **prevent the turret rotating
  fully** — which is why the 2026 hinged and cable designs exist. A turtle tank
  is described as reduced to "a mode of transport for troops" `[R]`.
- **Escape.** Crews can be trapped by their own screens if immobilised `[R]`.
- **Signature.** Nothing suggests a cage reduces thermal, acoustic or visual
  signature. **Cages reduce damage taken, not the probability of being seen** —
  a caged tank is, if anything, a more distinctive silhouette.

### 2.3 Vehicle-mounted electronic warfare

Specifications, all vendor- or press-sourced:

| System | Side | Figures | Mark |
|---|---|---|---|
| **Volnorez** | RU | **13 kg**, **900–3,000 MHz** | `[M, vendor/press]` |
| **Sania / Saniya** | RU | detect and jam single FPVs and swarms **to 1.5 km**; claims to respond only to FPV signals, suppressing false positives | `[C]` |
| **Piranha AVD 360** | UA | **150–250 W**; protective dome **600–700 m radius, 360°**; equipment inside the armoured hull with only compact antennas exposed | `[M, vendor]` |

The Volnorez story is the one to learn from: promoted heavily, its effectiveness
"turned out to be more than modest", and Russia moved on to promoting Sania
instead `[R]`. Treat single-vehicle jammer performance claims as `[C]`.

**The arms race is the real finding.** Mounted omnidirectional jammers worked
well against FPVs in early 2024 — drivers reported drones simply dropping in
front of their pickups — but **by spring the standard 900 MHz module was no
longer enough; by summer four to five modules were needed; by autumn, seven**
`[R]`. Multi-band kits became heavy and bulky enough that infantry could barely
carry them `[R]`. That is a dated, sourced description of a defence being routed
around, and it is the shape the game should model: **a jammer whose
effectiveness decays over the campaign.**

**And then fibre.** No jammer touches a fibre-optic FPV; Russian adoption is put
at **30–50%** of FPV use, with **35+ Ukrainian manufacturers** by early 2026
`[R]`. A vehicle EW dome in 2026 covers, at best, half the threat — and it
emits, announcing itself to passive RF direction-finding.

### 2.4 Hard-kill active protection

2026 is the year APS met FPVs, and the result is genuinely mixed:

- **Arena-M (RU), first front-line FPV intercepts, mid-2026.** The systems "shot
  down drone after drone, and only stopped when they ran out of projectiles."
  **One downed seven FPVs before its tank was destroyed**; it reportedly takes
  **15–20 FPVs** to kill an Arena-M tank `[R/C — several outlets carry the same
  figures, which is corroboration of circulation, not of measurement]`.
- **But the first tanks carrying it burned anyway** — crews can send more drones
  than the magazine holds `[R]`.
- **The design mismatch is explicit**: a system built and tested against fast
  ATGMs and RPGs struggles with a **slow, low-signature, erratically
  manoeuvring** threat `[R]`.
- **Trophy's quoted elevation limit is 55°**, leaving the near-vertical drop
  attack outside the envelope entirely `[R]`.
- **Cost:** Trophy retrofit ~**$350,000 per vehicle** `[R]`; a four-nation NATO
  procurement of roughly **€300–330 m** for Leopard 2A8 fleets, January 2026
  `[R]`.

**Read this as: APS converts a one-drone kill into a ten-to-twenty-drone kill,
at $350k per vehicle, and does not close the vertical.** A real effect at a poor
exchange rate against a $400 airframe.

### 2.5 The soft-skin fleet — what actually dies

`ground-logistics.md` establishes the scale: a ~90-day front-line lifespan for
pickups and SUVs, one brigade losing up to ten vehicles a week, and unarmoured
vehicles rising to ~90% of Russian vehicle losses from ~25% a year earlier.
Searching for what is actually fitted to those vehicles produced a thinner, more
improvised picture than for armour:

- **Netting and cable mesh are the main measure, and they are improvised.**
  Reporting describes "everything from metal cages to flexible nets" on
  vehicles `[R]`. A commercial market is forming — a UAE maker offers a Q-NET
  anti-drone net kit `[R]` — but that is a *vendor product for armour*, not a
  truck-fleet standard.
- **Net launchers on pickups, specifically for casevac** — described as "highly
  effective during medical evacuations in pickup trucks, where the vehicle has
  time to drive away from the impact zone while the drone is falling" `[R]`.
  Note what that does: it does not kill the drone, it **displaces the
  detonation**. A distinct and modellable effect.
- **Road infrastructure substitutes for vehicle protection.** The dominant
  Ukrainian answer is netting the road, not the truck: **1,300+ km of logistics
  routes protected in 2026, 266 km in August alone** `[C]`. See
  `ground-logistics.md` §8; not repeated here.
- **Deception instead of hardware, on the Russian side.** Dazzle and zebra paint
  aimed at drone computer vision, military trucks repainted as civilian dump
  trucks, "fake milk trucks" `[R]`. This is what you do when you cannot afford a
  jammer per vehicle.
- **Squad and convoy EW rather than per-vehicle EW.** The Ukrainian pattern is a
  layered "mesh": personal jammers for squads, vehicle-mounted systems for
  **convoys and mobile patrols**, static installations for positions `[R]` — the
  phrasing implies vehicle EW is allocated to movements, not issued per vehicle.

**Is there a fleet-wide standard?** No source found says there is. The honest
answer is **both pictures at once** `[I, Medium]`: a small procured layer
(armour, command vehicles, convoy escorts, integrated systems like Piranha)
sitting on a large scavenged one (welded mesh, chains, volunteer-funded jammers,
a net thrown over a pickup).

### 2.6 Does protecting a truck beat sending a robot? — direct answer

`FINDINGS.md` item 27 has a pickup losing ~2–3% per run and a robot 10–33%, with
robots used because nobody is aboard. If a cheap cage or jammer meaningfully
cuts the 2–3%, the case for robots weakens.

**Answer: no. Protecting the truck does not displace the robot, and the
arithmetic is not close.** `[I, Medium-High]` Three reasons:

1. **The 2–3% is not the constraint.** A truck's survival is dominated by
   *whether it enters the kill zone at all*, not by what is bolted to it.
   Halving a 2–3% per-run loss saves one vehicle in a hundred runs; it does
   nothing about the fact that the run has to happen and a human is in the cab
   when it does.
2. **The protection that works on a truck is not fittable at fleet scale.** The
   two best-evidenced measures are road netting (infrastructure, not vehicle
   kit) and multi-band EW. The escalation from one module to seven `[R]` means a
   current vehicle jammer is heavy, power-hungry and expensive — and fibre-optic
   FPVs, 30–50% of the threat, ignore it entirely `[R]`. Cages on a pickup are
   close to pointless: a shaped-charge disruptor in front of a sheet-steel cab is
   protecting nothing that was going to survive anyway.
3. **The exchange rate never inverts.** Item 27's insight holds: the robot is a
   *currency conversion*, materiel spent instead of people, at a poor rate worth
   taking because the other currency is unobtainable. Manpower, not vehicles, is
   the binding constraint `[R, front-2026 §10]`. Even if a $3,000 jammer cut
   truck losses by a third, it would not put a driver back in a seat where a
   driver cannot be spared.

**Design consequence:** protection kit for soft-skins should be **cheap, modest,
and not a route out of the robot economy.** Give the game a truck upgrade worth
perhaps a 20–30% reduction in per-trip loss `[E]`, with no effect against
fibre-guided attackers, and make sure it never becomes cheaper per delivered
tonne than the robot. If it does, the player will rebuild the 2023 logistics
model and the game will stop describing 2026.

---

## 3. The central question: can a fighting vehicle carry its own counter-drone turret?

### 3.1 What exists, and at what maturity

**Fielded, on vehicles, now:**

- **Khyzhak / Lyut-Khyzhak (UA, UFORCE).** Remote module, **7.62 mm FN MAG**,
  **800 m effective**, **700 rounds**, AI detection and tracking, mountable on
  ground vehicles, boats and helicopters. **Dozens in service since late March
  2026**, protecting **more than ten units** `[R/C]`. Ukraine's MoD states that
  *"kinetic interception is one of the few effective ways to counter
  fiber-optic-controlled drones"* `[C]`.
- **Bullfrog (US, Allen Control Systems).** **~300 lb (136 kg)**, **24 V DC**,
  **850 rpm**, **800 m** maximum effective, Group 1–3, wrapping a standard M240
  `[M]`. Marine Corps pairing it with **L-MADIS** under a ~$6.2 m prototype
  agreement; **12 turrets bought**; $120 m+ in contracts; a claimed **100%
  success rate at T-REX 26-1** `[R/C]`.
- **Sky Sentinel (UA).** M2 Browning, 360°, trailer-mounted, **$150,000**,
  claiming targets at "200, 400, even 800 km/h" `[C]`. Not a fighting-vehicle
  fit.

**Prototype or demonstrator:** the **Leopard 2 A-RC 3.0** (30 mm RWS above the
turret roof, marketed for counter-drone); the **CTAS 40 mm unmanned turret**
shown at Eurosatory 2026 with an **85° elevation arc** explicitly for FPVs and
loitering munitions; the **M1E3 Abrams** with an unmanned turret, three crew in
a hull cell and an **R400 Mk2 RWS paired with an EchoGuard radar** (first
prototype late 2025, **IOC around 2030**); **Dune** (AimLock + FN America, SOF
Week 2026) — an FN DEFNDER Medium RWS, **~660 lb (300 kg)**, 30 mm, out to about
**2 km**, bolting onto a utility vehicle you already own, **with a human still
approving the shot**; the Leonardo DRS **counter-drone Stryker** (laser plus
candidate Coyote/APKWS II/XM914); and **Iron WASP** (Rafael + SpearUAV),
capsule-launched interceptor drones fired from the vehicle — the only genuinely
vehicle-organic *interceptor* found. All `[R]`.

**Dedicated air-defence vehicles** (Skyranger 30 and kin) belong in
`point-defence.md`, but set the price ceiling: turret **1.8–2.3 t with
ammunition**, **3,000 m** with AHEAD airburst, organic AESA plus EO, and
Germany's 19 Boxer-based systems for about **€595 m** — **~€31.3 m each** `[M]`.

**And the cheapest option of all: the gun already on the tank.** The US Army's
revised, 432-page **Tank Platoon tactics manual (July 2026)** for the first time
orders M1A2 crews to engage drones with the **120 mm main gun firing M1028
canister** and with the coaxial machine gun; drone engagement is now two of a
tanker's twelve critical tactical tasks; the **loader is designated air guard**
`[R]`. Milbloggers ridiculed it `[R]`, and the ridicule is informative: a tank
shooting canister at quadcopters is a doctrine of last resort dressed as a
capability.

### 3.2 What it costs the vehicle

| Cost | Figure | Mark |
|---|---|---|
| Mass, light RWS | 136–300 kg (Bullfrog, Dune) | `[M]` |
| Mass, autocannon turret | 1.8–2.3 t (Skyranger 30) | `[M]` |
| Power | 24 V DC vehicle power suffices for a light RWS `[M]`; an autocannon turret with organic AESA does not | `[M]`/`[I]` |
| Roof space | Direct competition with the main turret, commander's sight, APS launchers, cage and ERA | `[I]` |
| Crew | Doctrinally one crewman on air guard `[R]`; an autonomous turret shifts this to supervision, but Dune still requires human shot approval `[R]` | `[R]` |
| Money | ~$150k (Sky Sentinel class) to ~€31 m (Skyranger on Boxer) | `[M]` |

### 3.3 Why it does not restore freedom of movement

Four arguments, and I think they are decisive:

**(a) The engagement range is shorter than the detection range that killed you.**
Every fielded light turret tops out around **800 m** `[M]`, and an FPV covers
**100 m in 2–3 seconds** `[R]` — roughly a **20-second** window against one
attacker, if you see it at 800 m. A recon drone found the vehicle minutes
earlier at several kilometres. The turret shortens the last twenty seconds of a
kill chain whose first ten minutes are untouched.

**(b) A fighting vehicle cannot afford the sensor the turret needs.** The
strongest argument, and it is visible in the engineering: the Marine Corps does
not put Bullfrog on one vehicle, it pairs **two — one carrying sensors, one the
effector** `[R]`. Skyranger carries its own AESA and costs €31 m. The M1E3
bolts an **EchoGuard radar** to the RWS `[R]` — and a radar on a tank is an
emitter on a tank, which the game's passive RF channel should punish. Passive EO
tracking of a 25 cm quadcopter against ground clutter at 800 m is a problem no
snippet claims to have solved on a *moving* vehicle.

**(c) Magazine depth is the binding constraint, and the attacker sets the
tempo.** Commentary is consistent that magazine depth "recurs as the binding
constraint on every layer," and that Ukraine's Gepards are limited by ammunition
supply rather than performance `[R]`. Arena-M is the cleanest empirical version:
**seven kills, then the tank died** `[R]`. Against an opponent massing airframes
at a few hundred dollars each, a serial engagement system with a finite magazine
loses by arithmetic.

**(d) The cost ratio still runs the wrong way.** Expected cost per armoured
vehicle killed by FPV is put at ~**$1,300**, accounting for a 30–60% hit rate
`[R]`. Suppose turret plus cages plus jammer triples that to ~$4,000 —
consistent with the six-to-eight-drone operator claim `[C]`. You have spent
$150,000–$350,000 per vehicle, plus mass, roof space and a crewman's attention,
to move the exchange ratio from about **1:2,000 to 1:600**. Still catastrophic.

### 3.4 Verdict

**A vehicle-mounted counter-drone turret raises the price of killing armour by
roughly two to three times. It does not restore armour's freedom of movement,
and nothing found suggests it will by 2028.** `[I, High confidence in the
direction; Medium in the multiplier]`

The strongest evidence *for* armour's survivability is not a turret at all. A
Ukrainian Leopard 1A5 absorbed **52 FPV and Molniya hits across a single day in
February 2026** and was repaired and returned to service `[R, multiple
outlets]`. What saved it was **passive** — roof cage, grilles and chains over
the engine deck, locally made ERA on hull, sides, turret and turret rear — and,
critically, **the crew was not in it.** They sheltered off-vehicle until the
strikes stopped, then ran back and drove it out.

That matches how the Leopard 1A5 is actually employed: **1,500–2,500 m behind
the forward line** as a fire-support platform, firing **indirect to 12 km** as
makeshift artillery, with a new turret expected to reach 18 km `[R]`. Ukrainian
crews call them "sniper tanks." The tank survives by being far away, stationary
and hidden — not by shooting back.

**A tank that can defend itself is a tank that can sit still safely. It is not a
tank that can advance.**

---

## 4. Infantry: how many, where, and doing what in 2026–28

`front-2026.md` section 10 establishes the core: positions held by **two to four
soldiers**, mandatory rotation capped at two months by Syrskyi's order of 30
April 2026, documented realities of 100+ days, and **15 km approach marches on
foot at night**. This section adds what that position physically is and where
the trend is heading.

**A position is no longer a hole with two people in it — it is a hole with two
people in it, with a roof.** `[R]`

- **Strongpoints designed for 30–100+ personnel are being replaced** by
  distributed networks of positions for **three to eight**, and by an even more
  granular system of individual fighting positions and autonomous weapon
  emplacements `[R]`.
- **All fighting positions now have reinforced overhead cover — typically
  20–40 cm of compacted earth over steel** — sized to defeat drone-dropped
  grenades and PG-7 warheads `[R, single well-specified snippet]`.
- **Mesh netting over trenches** detonates or entangles FPVs before they enter;
  Ukraine has procured *millions of metres* `[R]`.
- **The direction of travel is downward.** Ukrainian firms built **2 km of
  linked underground infrastructure** on the eastern front in corrugated steel
  with drainage, ventilation and internal passageways `[R, Jan 2026]`; a tunnel
  15–30 feet down is invisible to ISR and cannot be entered by a drone `[R]`.
  Finland's BASE 25 training site — **1.2 miles of trenches, half covered**, four
  underground shelters — shows the lesson institutionalised outside the war `[R]`.

**Is infantry density rising or falling? Falling, on both sides.** `[I, High]`
Ukrainian frontline units run at **50–60% of authorised manning, some as low as
30%** `[R]`; no fully manned brigade exists `[R]`. Both sides have abandoned
massed movement for "twos and threes," motorcycles and buggies `[R]`, with
motorcycle assault casualty rates described as ones **no Western army would
contemplate** `[R]`. The substitution is quantified by the people making it:
Brigadier General Biletskyi of 3 Army Corps estimated wider UGV adoption could
cut **frontline infantry requirements by as much as 30% within the year** `[C]`,
and the General Staff reports robots have **cut personnel casualties by up to
30%** `[C]`; some assault units report **more than 60% of deployed assets are
drones** `[R]`. The counterweight is equally plain: robots **cannot replace
infantry**, and ground is still occupied by people `[C]`.

**What they carry:** helmet, modular armour over torso, groin, neck, shoulders
and hips, AK-74, IFAK, and — new as standard — a **squad drone kit** `[R]`. The
loadout is "standardized where ballistics are regulated and improvised
everywhere else," with volunteers and the soldier's own money covering the
interface gear `[R]`. Resupply: see `ground-logistics.md` (≈9 kg per position
per day; do not double-count against theatre figures).

---

## 5. Infantry counter-drone: what a person can actually do

**Detection first — it is the half that works.** *Chuyka 3.0* and *ZORKO* are
pocket RF detectors identifying FPV video downlinks at **up to 4 km**, alerting
before a stable image is received `[M, vendor]`. *Hrafit ("Graphite")* delivers
the national radio-technical intelligence picture as a **push notification on a
protected smartphone**, needing only the phone and Starlink in the position
`[R, Sept 2026]` — the warning net is now a phone, and that is the most
interesting 2026 development here. Acoustic nets (Sky Fortress, Zvook, FENEK)
are covered in `acoustic.md`. **The warning is short**: an FPV covers 100 m in
2–3 seconds, and a slow-sweeping detector burns several of those `[R]`.

**Shooting at it — works narrowly, and is getting worse.** Shotguns are reliable
to roughly **50 m** with lead, further with tungsten; recommended load **36 g
No.1–000 at 420–440 m/s** `[R]`. But the window is closing from both ends: the
**safe standoff for detonating a 15-inch FPV is now 40–50 m** — the shotgun's
own range — and troops are often issued shot too fine to stop a heavy airframe
`[R]`. **Standard rifle ammunition is largely ineffective**; even direct hits
often fail `[R]`. Specialised natures change the numbers: an "AD-LER" shotgun
round claims **>80% catastrophic kills within 50 m** `[C]`; a 5.56 mm anti-drone
round is described as creating a **half-metre interception zone** instead of
requiring a 15 cm hit `[R]`; AI optics claim **95% hit probability** `[C]`.

**Jamming it — light, cheap, half-useless.** The MyDefence soldier kit
(detector + jammer) is **~2.5 kg** on the plate carrier `[M]`; hand-held
rifle-form jammers cover **400 MHz–6 GHz** with about five minutes of
instruction `[M/R]`. Same fatal gap as vehicle EW: **fibre-optic drones are
immune**, and multi-band escalation makes a current kit heavy `[R]`.

**Hiding from it has the best return** — overhead cover, netting, underground
shelter, not moving. It is what the Leopard crew did and what the trench system
is being rebuilt around.

**For the designer** `[E]`: a **15–30% chance for a warned, equipped, stationary
infantry team to defeat one incoming FPV**, falling towards zero if surprised,
with jamming ineffective against fibre-guided attackers.

---

## 6. Ground robots that are not logistics carriers

Logistics UGVs are covered in `ground-logistics.md` and are not repeated. What
follows is everything else.

### 6.1 Armed UGVs — real, fielded, and doing something new

- **Scale.** More than **1,000 UGVs flew 66,000 missions** for Ukraine in 2026
  `[R]`; the MoD contracted for **25,000 ground robotic systems** in the first
  half of 2026 `[R/C]`. No source breaks the fleet into armed versus logistics
  shares — **that split is genuinely unknown**, and I looked for it.
- **Weapon modules.** *Shablya M2* (7.62 PKT/PKM, 12.7 NSVT or M2) dates from a
  2014 volunteer project; *Burya* (serial from Feb 2025, *Burya 2.0* with combat
  experience folded in) mounts an Mk 19; a July 2026 module trades the heavy
  machine gun for **twin AK-74s with Starlink fallback when the link drops**
  `[R]`. Kalashnikov-armed UGVs entered combat testing in August 2026 `[R]`.
- **Cost:** a TerMIT with integrated Burya at **$12,000**, another armed example
  at **$35,000** `[R]` — orders of magnitude below any manned fighting vehicle.
- **The capability that matters is presence, not firepower.** A **DevDroid
  TW-12.7 held a frontline position for 45 consecutive days**, described by 3rd
  Army Corps as Ukraine's first fully robotic defensive operation —
  *"Only the UGV system was present at the position"* `[R/C]`. A TW-7.62
  captured three Russian soldiers near Lyman in January 2026 `[R]`; in July 2025
  the 3rd Assault Brigade took a position with robots and drones alone `[R]`.
- **Exotic employment is arriving fast**: a UGV landed on the Kinburn Spit by
  uncrewed boat; "marsupial" attacks with a heavy-lift drone delivering a
  mine-carrying UGV behind the lines; a UGV driven into a building and detonated
  `[R]`. **Ground drone motherships** — *Gnom-DC*, *Black Widow / Karakurt* (six
  FPVs, one operator flying two at once so the second observes and re-attacks)
  `[R]` — are a unit type the game lacks and should have.

### 6.2 Engineering and mine-laying robots — quietly significant

Both sides use UGVs to deliver demolition charges, **lay mines**, lay smoke and
conduct engineering reconnaissance `[R]`; Russia's Kuryer carries **10× TM-62**
among its loads `[R]`. The Ukrainian **NEO-1** demining robot is **60 kg**,
**7 km/h**, **8 h endurance**, **500 m control range (3 km optional)**, with a
pulse metal detector of **139 cm scan width to 60 cm depth** `[M, vendor]`.

**Counter-UGV is emerging**: machine-gun UGVs are used to pre-emptively destroy
Russian FPVs lying in wait along supply routes, and both sides task UAVs and
UGVs to hunt systems concealed in ambush positions `[R]`.

### 6.3 Legged and dog-type robots — say it plainly

**They are, as of late 2026, not a battlefield capability.** `[R, and I am
confident in this]`

The evidence:

- **Ukraine tried them and the operators rejected them.** Quadrupeds
  "underperformed"; operators called them **"a nice toy with no application on
  the front line yet"**; they **bog in ploughed fields** and **cannot hide**
  `[R, relayed April 2025 — nothing found since contradicts it]`.
- **The UK-supplied fleet was tiny**: **30+** "war dogs" at about **$9,000
  each**, carrying **up to 7 kg**, used for trench and building inspection and
  mine detection `[R]`. Thirty machines is a trial, not a capability.
- **China's are a parade capability.** The "Machine Wolf" family — one
  reconnaissance, one support, two rifle-armed attack machines, **~70 kg each**,
  **2 km range**, networks of **up to 30 nodes** — has appeared in urban drills
  (April 2026), an amphibious rehearsal and Steppe Partner 2026. **As of August
  2026 there is no verified combat deployment** `[R]`.
- **Humanoids are further back.** Two **Phantom MK-1** units went to Ukraine in
  February 2026 for logistics trials and showed **limited suitability** —
  inadequate payload, no waterproofing, insufficient battery `[R]`. The PLA
  reportedly holds **100+ procurement records for combat humanoids and zero
  deployments** `[R]`.
- **The one genuine programme signal** is the Australian-designed **CODiAQ**
  armed quadruped: a **$6.5 m** US research contract and a limited safety
  release for SOCOM test and evaluation, live fire scheduled October 2026 `[R]`.
  A 2028-at-earliest special-operations capability, not a line-of-battle unit.
- **Why they fail is structural.** Legs buy stairs, rubble and tunnels; this war
  is mud, open fields and overhead observation. A quadruped is slower than a
  wheeled UGV, carries a fraction of the load, delivers perhaps **60–70% of
  claimed runtime** under real payload and terrain `[R]`, and stands taller than
  the ditch it is meant to hide in.

**Trajectory 2027–28: niche and stable** `[E, Medium-High]` — tunnels, buildings,
EOD and base perimeter patrol, where their sourced successes are, and not in the
field. I searched for a 2026 reversal of the Ukrainian verdict and found none.

---

## 7. The 2028 ground force — a projection

**This section is projection, marked `[E]` throughout.** It extends sourced
2026 trends; it is not reported.

A company-equivalent slice of a competent 2028 force, holding roughly 3–5 km of
frontage:

| Element | Count | Share of combat power `[E]` |
|---|---|---|
| **People, total** | 60–90 (against a nominal 120–150) | — |
| — in forward positions | 20–30, in 8–12 teams of 2–4 | 10% |
| — drone and robot crews | 25–40 | **45%** |
| — command, EW, medical, sustainment | 15–20 | 10% |
| **Air drones** | 150–400 airframes/month consumed; 20–40 airborne-capable at any moment | included above |
| **Logistics UGVs** | 15–30 | 5% (enabling) |
| **Armed / position-holding UGVs** | 4–10 | 10% |
| **Engineering / mine-laying UGVs** | 1–3 | 5% |
| **Legged robots** | **0–1, and probably 0** | ~0% |
| **Fighting vehicles** | 2–5, kept 1.5–3 km back | 10% |
| **Counter-drone mounts (turret/EW/net)** | 2–4, mostly on the logistics and command vehicles | 5% |

Reasoning: manning at 50–60% of establishment is already 2026 reality `[R]`, and
Biletskyi's further 30% `[C]` gives 60–90; "more than 60% of deployed assets are
drones" in some assault units today `[R]` makes a 45% crew share conservative by
2028; and US Army C-UAS integration reaching **maneuver platoons by 2026 and
companies by 2027** `[R]` puts a counter-drone mount at company level in 2028 as
programme record rather than speculation.

**The headline: by 2028 the majority of a company's combat power sits with
people who never see the enemy, and the people who hold ground are a minority of
a minority.** Directionally true already in 2026.

---

## 8. Design recommendation

The game currently has: Main Tank, IFV, trucks, logistics robots, and three
soft-skinned specialist teams (engineer, designator, motorcycle squad). No
vehicle can engage air. There is no line infantry.

### 8.1 The counter-drone turret: build it as an **upgrade**, not a unit, and make it disappointing on tanks

**Recommendation: a buildable module fittable to any vehicle, priced so that
fitting it to a tank is usually the wrong choice.** `[E]`

- **Vehicle Counter-Drone Mount**: engage reach **65 map metres** (800 m real at
  12:1), low band only, one target at a time, **magazine of about 6 engagements**
  before a long reload, hit chance falling sharply with range.
- **It requires a cue.** Without a Radar Mast, acoustic net or accompanying
  sensor vehicle in range, halve its reach and add a 2–3 second acquisition
  delay. Not a balance fudge — it is why L-MADIS is two vehicles and why the
  M1E3 needs EchoGuard `[R]`. An **organic radar option** removes the cue
  requirement at the price of an **RF signature of 60–80**.
- On a tank the module should visibly *not* solve the problem — the tank still
  dies, to five drones instead of two. On a **truck or command vehicle** it
  should be clearly worth it, because those are attacked by ones and twos rather
  than by saturation.

**Do not add a "counter-drone tank" unit.** The research does not support one.

### 8.2 Vehicle armour upgrades — three separable modules, not one stat

- **Cage / Screen.** Reduces shaped-charge damage **from the covered arcs only**;
  give it a directional coverage value and an explicit chance that the cage
  *fails to disrupt*, detonating the warhead at optimum standoff for full or
  greater damage `[R — the real mechanism]`. Costs **speed** (−10 to −15%) and
  **turret traverse rate or arc** unless the expensive hinged/cable variant is
  bought. **Changes no signature.**
- **Vehicle EW Dome.** Radius ~50 map metres `[Piranha: 600–700 m real]`, good
  probability against radio-linked FPVs, **zero effect on fibre-optic**, RF
  signature 100 while active, and **effectiveness decaying across the campaign**
  as the enemy fibre share rises. The most authentic mechanic in this document.
- **Active Protection.** Expensive, 10–20 intercepts, **cannot engage above
  ~55° elevation** `[R]`, useless against a drone that hovers and drops.

All three layered should reach the operator-reported **six to eight drones
instead of two or three** `[C]`.

### 8.3 Units to add

1. **Rifle Team (2–4 figures)** — the missing primitive. Cheap, slow, digs in,
   holds ground, terrible in the open. Give it **Dig In** (overhead cover: large
   reduction against air-dropped munitions, none against artillery), a very
   short-range low-probability drone defence, and a **pocket detector** granting
   short warning. Manning it should hurt.
2. **Armed UGV / Position Holder** — holds a position indefinitely without food,
   water or rotation (the 45-day robot `[R]`). Cheap, poor at taking ground,
   good at denying it. The most sourced and distinctive ground unit available.
3. **Engineer UGV** — mine-laying and clearance, cheap, slow, expendable.
4. **Drone Mothership Vehicle** — carries and launches 4–6 FPVs forward
   (Gnom-DC / Black Widow class `[R]`); projects drone reach without a human
   crossing ground.
5. **Warning Net** — cheap passive pre-alert for friendly ground units, the
   Hrafit/Chuyka layer `[R]`. The cheapest survivability purchase in the game,
   and it should apply to infantry rather than vehicles.

### 8.4 Units that are wrong

- **The Main Tank as a manoeuvre unit is wrong.** Rebuild it as a **standoff
  fire-support platform**: excellent direct fire, an **indirect-fire mode**
  (12 km real `[R]`), severe penalties for moving inside the kill zone, and a
  natural station **1,500–2,500 m behind the line** `[R]` — 125–210 map metres.
  If the player can drive it forward profitably, the model is wrong.
- **The IFV is the weakest unit in the roster.** The vehicle that carries
  infantry into contact has been substantially displaced by motorcycles,
  buggies, quad bikes and civilian cars — **90% of Russian vehicle losses are
  unarmoured, nine soft vehicles per tank or BMP** `[R]`. Cut it, or recast it as
  a **dismount-point transport** that runs to a drop-off short of the objective
  and leaves.
- **The motorcycle squad is right and should be expanded** — it is the modal
  assault transport of the period `[R]`.
- **The Gun Mount as a static system is defensible** (Sky Sentinel and Khyzhak
  really are trailer- and mount-based), but it should be joined by the fittable
  module above and by an **acoustic sensor net** (`point-defence.md`).
- **Add per-trip attrition to logistics vehicles**, with the truck-protection
  upgrade worth only a modest reduction (§2.6). Do not let it beat the robots.

---

## 9. "Frogger"

**No such system, programme or nickname appears in open reporting.** Two
searches, framed differently — one for a counter-drone/EW system by that name,
one for a Ukrainian military vehicle nickname — returned nothing matching.
Results were dominated by **Bullfrog** (the Allen Control Systems autonomous
weapon station, section 3.1), which is a plausible source of a
misremembered name, and by unrelated EW systems.

I am **not** asserting that "Frogger" means Bullfrog. It might be a
mis-transcription, an internal or unit-level nickname that never reached English
reporting, a game or media reference, or something from a paywalled source this
environment cannot reach. **Recommendation: ask the user where they encountered
it before anything is built on it.** `[R — on the absence]`

---

## 10. What is genuinely uncertain

1. **Cage effectiveness numbers.** The 0.30–0.80 spread comes from one
   unreadable modelling source of unclear provenance. The *mechanism* is well
   corroborated; the numbers are not. Do not quote them as measured.
2. **The "six to eight drones with cages and a jammer" figure** is one Ukrainian
   operator's claim in one snippet. It is the only quantified statement of
   layered passive protection value found — hence `[C]`, not `[M]`.
3. **The Arena-M numbers** circulate widely but originate in Ukrainian and
   Western reporting on Russian equipment. Cross-outlet agreement here is
   corroboration of circulation only.
4. **Whether a light counter-drone RWS actually tracks small quadcopters from a
   moving vehicle is unresolved.** Every claimed success (Bullfrog's T-REX 100%,
   Khyzhak's front-line use) is a test event or static employment. No snippet
   describes a *moving* fighting vehicle defeating an FPV with its own turret.
   This is the weakest joint in section 3's argument, and it happens to point the
   same way as the conclusion.
5. **The 2028 projection in section 7 is mine.** Its trend lines are sourced;
   its arithmetic is not.
6. **The legged-robot verdict could age badly** if CODiAQ or a Chinese system
   sees real employment in 2027. Revisit if a quadruped is reported *holding or
   taking ground* — none has.

**What was searched for and not found** (recorded so nobody repeats it):
troops per kilometre of front in 2026; the armed-versus-logistics split of the
Ukrainian UGV fleet; the fabrication cost of anti-drone cages (only the ~3 tonne
cable mass); any measured before/after loss rate attributable to a vehicle
jammer — so §2.6 rests on structure, not a measured delta; standard IFV dismount
distances; "Frogger"; and any 2026-dated reversal of the Ukrainian verdict on
quadrupeds. That last silence is itself the finding.
