# KILL ZONE — Gameplay Specification

**Version 0.9 · Lead Game Design · September 2026**
Written against `brief.md` and `kill-zone-bestiary.html`. Every mechanic below traces to one of the seven thesis points in the brief, or it is not here.

**Units of measure.** All distances are *map metres* unless stated. The real front's 10–25 km kill zone compresses roughly **12:1** onto our maps, so the bestiary's 12 km FPV radius becomes a **1,000 m** game radius, its 40 km fiber spool becomes a ~3,300 m leash (we cut it to 1,400 m for playability — see Roster). One build tile is **8 m**. All costs are in **Materiel (MAT)**. All times are in seconds unless written as m:ss.

---

## 1. Vision and pillars

**Kill Zone is Command & Conquer where the unit you build is not the thing that fights.** You build airframes by the dozen. What actually fights is a *sortie*: an airframe, plus a crew, plus a link back to that crew, plus somebody who has eyes on the target. Take away any one of those four and the most expensive drone on the map is a paperweight sitting in a hangar. That sentence is the whole game.

A C&C player will recognise minute one completely: a sidebar on the right, a Command Post, a harvester trundling out to a resource field, a build order, credits ticking. By minute six they will hit the new idea from the wrong end — their strike flight will go black inside a jamming bubble and fall out of the sky, and they will learn that in 2027 the counter-unit is not a unit, it's a *column on the link ladder*. By minute fifteen they are running four simultaneous economies: materiel, merit, crews, and the supply throughput of a forward position that is one interdicted road away from starving.

### Pillars

**1. The link is the stat block.** *(Thesis 1)* Radio, mesh, fiber, satellite, autonomy. Each column buys jam resistance and pays in speed, weight, price, range or judgement. The counter always sits one column behind. Players climb this ladder over 20 minutes and the ladder *is* the tech tree. Nothing else in the tech tree matters as much.

**2. Your army is your sortie rate.** *(Thesis 4)* Airframes are cheap stock. Crews are the cap. You do not lose army value permanently when a drone dies — you lose a crew for eight seconds and some materiel. This changes the emotional shape of an RTS fight: you are never wiped, you are only ever *suppressed*, until someone hits your crew quarters and your entire air force becomes inventory.

**3. Nothing is targetable until somebody looks at it.** *(Thesis 3)* Direct fire needs vision. Indirect fire — loitering munitions, bombers, the superweapon — needs **Designation**, which only recon assets and pinning units provide. Killing the enemy's recon does not slow them down; it turns their lights off.

**4. The kill zone is drawn by casualties, not by the level designer.** *(Thesis 2)* No map has a painted "danger" region. The band exists because somebody has ready airframes, idle crews, and a recon footprint over a road. The Threat overlay shows your *estimate* of the enemy's band, built from where you have died. It decays. It is wrong. It is the most useful thing on the screen.

**5. Logistics is the front.** *(Thesis 5)* A forward position is a supply bar and a 700 m radius. Feeding it is a four-way choice between a fast fat truck, a slow quiet robot, a suicidal motorcycle, and a night bomber that flies over everything but only after dark. Cutting a position off is a visible, achievable, winnable objective.

**6. Kills are currency.** *(Thesis 7)* Verified kills and recon pins pay **Tasking Points**. TP buys quality, never quantity: upgrades, veteran crews, autonomy modules, catalogue one-offs. The comeback player still has materiel; the winning player has better gear. That asymmetry is deliberate.

**7. Readable, fast, fun.** *(Hard constraint)* **One select, one command, one overlay toggle.** Every action in this game must be expressible as *pick a thing, then pick a place or a target*, with a single toggle for the one overlay that matters. If a mechanic needs a submenu it gets cut or folded into a stance. That rule is what lets the same design ship on a tablet, a phone and a desktop without a second game underneath it (see §19). We are modernising C&C, not shipping a wargame.

### Anti-pillars (things we are explicitly not doing)

No hex fidelity. No supply *convoy routing UI*. No separate control-channel and video-channel jamming. No naval. No manned aviation. No casevac or body-count bookkeeping. No deck-building. No per-unit ammunition counters.

And one input rule that governs all of the above: **no mechanic may assume a keyboard, a hover state, a scroll wheel, or a third input button.** Keys, right-clicks and hover tooltips are *accelerators* layered on top of a design that works with a single pointer; they are never the only way to reach a capability. Any mechanic that cannot be driven by select-then-command is wrong on every platform, not just on touch.

---

## 2. Core loops

### The 30-second loop — the sortie

1. A **Recon Wing** orbiting 900 m out paints an enemy EW Truck. A **Designation** diamond appears on it.
2. Player selects a **flight card** from the hangar rack — say, three ready Fiber FPVs.
3. Player designates the target. The game grabs the three highest-readiness idle crews, the airframes leave the workshop pad, and three orange tether threads start unspooling across the map.
4. The flight crosses a jamming bubble. Because they are fiber, nothing happens; a radio flight would have gone amber then black here. One drone's thread snags on a power-line corridor (9%/s in that tile) and it drops.
5. Two hits. The EW Truck dies. Because it died inside a friendly recon footprint, the kill is **verified**: +84 TP (12% of its 700 MAT). It drops **Salvage 245** — 35% of its cost — on the spot.
6. The two surviving crews go amber for 8 s recovery, then green. Two more airframes are already finished in the hangar.

That is the atomic unit of play — two inputs, on any device. Target, link, crew, commit, watch, recycle. Everything else in the game exists to make this choice interesting at a different scale.

### The 5-minute loop — the phase

The day/night cycle is 6:00 long. One full cycle is one phase and it has a shape:

- **Day (3:00).** Harvest. Recovery UGVs push toward the middle wreck fields. Build the next structure. Trade FPV sorties over the road. Nobody moves a tank.
- **Dusk (0:45).** Reposition. Forward Nodes get topped up. Night Bombers spin up on the pad.
- **Night (1:45).** The violent part. Optical detection collapses to 35%. Bombers fly (they cannot fly in daylight at all). Tanks dash. Supply throughput doubles. Assaults go in.
- **Dawn (0:30).** Everything caught in the open is now visible, and the overwatch flights launch. Dawn punishes anybody who over-extended at night.

A phase ends with an assessment: did I take the wreck field, did I move my EW Post forward, did I get the Spool Plant up before their jammers went live.

### The 20-minute loop — the ladder

The match is a race up the link ladder, and each rung has a hard counter one rung behind:

| Minute | Rung | The play | The answer |
|---|---|---|---|
| 0:00–4:30 | **Radio** | Cheap FPV volume, harvester raids, road control | More FPVs; nets; dispersal |
| 4:30–9:00 | **EW** | Jam bubbles blank the enemy's radio flights | Fiber; move the strike vector |
| 9:00–14:00 | **Fiber / Recon** | Unjammable anti-armour, real ISR footprints, first tanks | Nets, forests, recon-hunters |
| 14:00–20:00 | **Satellite / Autonomy** | Kestrel uplinks ignore the ground war; Obsidian swarms fly without crews | Uplink jamming; **deception** — decoys beat autonomy, not jammers |
| 18:00+ | **Super** | Arclight / Blackout | Deny the recon gate; spread out |

Victory usually arrives when one side owns three Grid Taps and the other side's crews are all recovering at once.

---

## 3. Economy and resources

Four resources. Two are global currencies, one is a local stock, one is a concurrency cap.

### 3.1 Materiel (MAT) — the credits

**What it is:** fuel, batteries, airframe kits, explosive. Everything you build costs MAT.

**Where it comes from:**

**a) Salvage (the harvestable).** The map is covered in wrecks — burnt vehicles, downed airframes, collapsed net tunnels. **Recovery UGVs** drive to a **Salvage Cluster**, spend 9 s stripping it, and drive 90 units of salvage back to the **Salvage Yard**, which converts at **1 salvage = 1 MAT**. A full round trip on a medium map is 40–55 s; a Recovery UGV pays for itself in about 5 trips.

Two kinds of cluster:

- **Wreck Fields** — pre-placed, 6–10 clusters each with 2,400 salvage, and they do *not* regenerate. Placed by the level designer: two safe fields in each base pocket (900 salvage each, the opening economy), and three to five rich fields in the contested middle band.
- **Combat Debris** — **every unit that dies drops salvage where it died**, worth **35% of its MAT cost**, decaying at 2%/s and gone in 50 s.

That second rule is the heart of the economy and answers the question "why does harvesting create map conflict." *The richest salvage on the map is wherever the most fighting has happened, and it is worth the most in the first 20 seconds after it appears.* You are not just fighting over a static field like a Tiberium patch. You are fighting over the corpse of the last fight, in daylight, with a slow unarmoured robot, under enemy overwatch. The loop teaches itself: win a fight, immediately send a harvester into the place you just won, and then defend that harvester.

**b) Grid Taps.** Five neutral captured points per 1v1 map — electrical substations and pumping stations. A **Motorcycle Squad** or **Net Engineer** captures one in 12 s. Each tap yields **+22 MAT / 10 s** passively and provides 40 free power. They are the anti-turtle clock and they feed the Grid Control victory condition.

**c) Command Post trickle.** +10 MAT / 10 s, unconditional. Just enough that a player who has lost everything can still rebuild a Net Engineer. Non-negotiable anti-frustration rule.

**Starting bank:** 4,000 MAT, one Command Post, two Recovery UGVs, one Net Engineer.

### 3.2 Tasking Points (TP) — the merit currency

Lifted directly from the Brave1 marketplace (thesis 7). TP is earned, never harvested.

**Earning:**

| Event | TP |
|---|---|
| Verified kill (unit) | 12% of the victim's MAT cost |
| Verified kill (structure) | 8% of MAT cost |
| Unverified kill | 50% of the above |
| Target designated by you, killed by an ally within 60 s (a **pin payout**) | 6% of victim's cost, to the *spotter*, in addition to the shooter's |
| Enemy crew killed (crew quarters strike) | 40 TP each |
| Grid Tap held, per minute | 15 TP |

**"Verified" means somebody watched it die.** A kill is verified if, at the moment of death, the victim was inside a friendly recon footprint, inside 400 m of a friendly unit with line of sight, or killed by a piloted drone with a live link (the crew saw the feed). Kills by autonomous munitions in the dark, with no observer, pay half. This makes the recon layer pay for itself twice — once as the targeting gate, once as the accountant — and it is the cleanest possible expression of thesis 3.

**Spending (the TASKING tab — a catalogue, not a tech tree).** Items are one-click, instant or short-timer, and unlock by tier. Representative catalogue:

| Item | TP | Effect |
|---|---|---|
| AI Last-Mile Module | 300 | All FPV-class units: black link no longer aborts the run; drone continues to the last designated point. Autonomy Q 70. |
| Dual-Link Conversion | 350 | Fiber FPVs fall back to radio when the tether cuts; radio FPVs get +15 Link Robustness. |
| Veteran Crew | 220 | +1 crew slot, arrives at Rank 2. Repeatable, price +80 each purchase. |
| Thermal Optics | 400 | Recon Wing, Multirole Quad, Night Bomber: full detection range at night. |
| Cage Kits | 260 | All ground vehicles gain a 600 HP shaped-charge cage. |
| Spool Extension | 300 | Fiber leash 1,400 → 2,000 m. |
| Emergency Flight (one-off) | 180 | Instantly completes 6 airframes in the hangar. |
| Priority Recovery | 150 | Crew recovery 8 s → 5 s for 90 s. |
| Deception Package | 380 | Decoy Kits deploy 5 decoys instead of 3; decoy Plausibility 0.9 → 1.0. |
| Autonomy Stack (Obsidian only) | 600 | Unlocks crew-free stances on Multirole Quads and Loitering Munitions. |
| Uplink Priority (Kestrel only) | 550 | +3 satellite uplink capacity. |

**Why TP does not snowball.** It buys upgrades and crews, never army. A player 4,000 MAT ahead can field more stuff right now; a player 900 TP ahead has better stuff forever. In practice the losing player often earns *more* TP per minute, because defenders kill attackers inside their own recon umbrella and therefore verify everything. The mechanic has a built-in rubber band that costs us nothing in artificial catch-up.

### 3.3 Supply — the local stock

Not a global currency. Each **Forward Node** holds 0–200 Supply and sustains everything in a 700 m radius. The Command Post holds 600 and sustains 900 m. Details in §9.

### 3.4 Crews — the cap

Population equivalent. See §6.

### 3.5 Sinks

MAT sinks, in expected order of spend across a match: airframes (~35%), structures (~30%), ground vehicles (~20%), defences and nets (~10%), decoys and consumables (~5%). The dominant sink is deliberately *consumable*: one-way FPVs are burned every sortie, which means the economy never saturates and there is always a reason to harvest at minute 19. Power is a soft sink — low power adds +40% to build times and cuts EW/radar radius by 30%, so Generators are a recurring tax on expansion.

---

## 4. Base building and the build queue

### 4.1 The sidebar

Six tabs down the right edge, C&C-classic, with the radar/minimap above them.

1. **BUILD** — structures. Ghost-cursor placement inside the build radius (240 m from any friendly structure).
2. **DEFENCE** — turrets, nets, traps, Forward Nodes, decoys. Some entries place directly; nets and decoys require a Net Engineer within 120 m of the placement.
3. **CREW** — Motorcycle Squad, Net Engineer, Designator Team. Source: Crew Quarters.
4. **GROUND** — Recovery UGV, Supply Truck, Logistics UGV, EW Truck, IFV, Tank, Interceptor Battery. Source: Vehicle Depot.
5. **AIR** — every drone in the game, in one tab. Source structures vary (Drone Workshop, Spool Plant, Airfield, Launch Rail) and each icon carries a small **source glyph** in its corner so the player instantly knows which building unlocks it. This is the central UI call: *many buildings, one air tab*, because a C&C player expects to look in one place for aircraft.
6. **TASKING** — the TP catalogue.

**Queue rules.** One active item per tab (classic C&C, not StarCraft parallel queues). Each additional source structure for that tab reduces build time by **25%**, capped at **−50%** (so three workshops is the practical ceiling). Queue depth 9 per tab. Cancelling refunds 100% of unspent progress.

**The hangar.** This is the one real departure. Ground units and crews exit their factory and walk onto the map exactly as in C&C. **Air units do not.** A finished airframe goes into the **hangar stock** of its source building (capacity 12 per Drone Workshop, 4 per Airfield, 6 per Launch Rail) and appears as a **flight card** on a horizontal rack above the sidebar. Each card shows type, count ready, and a link-type stripe.

To fly, you click a card (or press its number) and click a target or a point on the map. The game assigns the highest-readiness idle crews, the airframes launch from the pad, and they are now normal selectable units. **One click, one sortie.** If you have no idle crews the card greys out and the cursor shows a crossed-out headset — the single clearest teaching moment in the game.

Airframes in the hangar are safe from everything except the destruction of the building holding them (which destroys the stock — a real reason to raid workshops).

### 4.2 Structure list

Footprints in tiles (1 tile = 8 m). "Pwr" is power drawn; Generators supply 100 each; the Command Post supplies 50.

| Structure | MAT | Build | Footprint | Pwr | Prereq | Function |
|---|---|---|---|---|---|---|
| **Command Post** | 2,000 | 60 | 8×8 | +50 | — | HQ. Build radius 240 m. Supply 600, sustain 900 m. Loss = defeat (default rules). |
| **Generator** | 400 | 14 | 3×3 | +100 | CP | Power. Explodes for 220 frag damage in 60 m when killed. |
| **Salvage Yard** | 1,400 | 35 | 6×5 | 30 | CP | Salvage → MAT. Ships with one free Recovery UGV. |
| **Crew Quarters** | 700 | 22 | 4×4 | 20 | CP | **+4 crew slots.** Max 6 built (24 crews). Killing one kills its crews. |
| **Drone Workshop** | 900 | 28 | 5×4 | 35 | CP | AIR tab T1. Hangar 12. |
| **Vehicle Depot** | 1,100 | 32 | 6×5 | 40 | Salvage Yard | GROUND tab. |
| **Radar Mast** | 1,000 | 30 | 3×3 | 60 | Drone Workshop | Air detection 1,400 m (ignores terrain). Enables minimap, interceptor cueing, T2. |
| **Spool Plant** | 800 | 26 | 4×4 | 45 | Workshop + Radar | Unlocks fiber units and Spool Extension. |
| **EW Post** | 750 | 24 | 3×3 | 70 | Radar | Jam bubble J70 / r450 m. **Emits**: permanently visible to enemy RF sensors even in shroud. |
| **Relay Mast** | 500 | 18 | 2×2 | 25 | Radar | Extends radio/mesh control +700 m. Chains up to 4. |
| **Airfield** | 1,500 | 40 | 9×5 | 50 | Radar + Vehicle Depot | Fixed-wing (Recon Wing, Mid-Range Striker, Mothership). Hangar 4. |
| **Launch Rail** | 900 | 26 | 5×3 | 35 | Vehicle Depot | Loitering munitions. Hangar 6. |
| **Net Works** | 600 | 20 | 4×3 | 20 | Drone Workshop | Unlocks Net Tunnel, Net Trap, Decoy Kit, cages. |
| **Repair Pad** | 500 | 18 | 4×4 | 25 | Vehicle Depot | Repairs ground units at 45 HP/s for 2 MAT/HP; rearms Multirole Quads and Night Bombers. |
| **Forward Node** | 300 | 15 | 3×3 | 0 | Net Engineer on site | Supply 200, sustain 700 m. Buildable anywhere on the map. The forward-projection primitive. |
| **Gun Mount** | 450 | 16 | 2×2 | 30 | Radar | AI .50 turret. 550 m, frag, tracks air. The last-line answer to FPVs. |
| **Net Trap** | 90 | 6 | 1×1 | 0 | Net Works | One-shot. Kills one rotary drone entering 60 m. Rebuilds in 25 s if a Net Engineer is within 120 m. |
| **Net Tunnel** | 220 / 100 m | 14 | road strip | 0 | Net Works | Road segment immune to rotary drone attack from above. Burns to thermite in 6 s. |
| **Uplink Terminal** *(Kestrel)* | 2,200 | 50 | 5×5 | 90 | Airfield | Satellite link. +6 uplink capacity. T3 gate. |
| **Autonomy Lab** *(Obsidian)* | 2,000 | 48 | 5×5 | 85 | 2× Drone Workshop | Autonomy upgrades, crew-free stances. T3 gate. |
| **Mission Control** | 3,000 | 70 | 6×6 | 120 | Uplink Terminal / Autonomy Lab | Hosts the faction super. 7:00 cooldown. |

**Placement rules.** Structures need flat ground and 240 m proximity to a friendly structure — except the Forward Node, Net Trap, Net Tunnel and Decoy Kit, which a Net Engineer can place anywhere. Buildings block line of sight for ground units but not for air. The Radar Mast and EW Post are *tall*: they are visible from 1,800 m regardless of terrain, which is the point.

---

## 5. The control-link layer

This is the game's signature system. It must be legible in one glance and it must never require a submenu.

### 5.1 The five links

| Link | Robustness R | Control source | Range | Cost paid |
|---|---|---|---|---|
| **Radio** | 40 | Crew Quarters, Relay Mast, Mothership | 1,000 m from nearest node | Cheapest. Dies to any jammer. |
| **Mesh** | 65 (+5 per available alternate hop, max +15) | Any friendly Mesh-capable unit or Relay Mast | 700 m per hop, up to 4 hops | +0.4 s acquisition per hop. **Kill any link in the chain and every child drops.** |
| **Fiber** | ∞ (unjammable) | Launch point, physically | Hard leash 1,400 m (2,000 with Spool Extension) | −25% speed, −20% turn rate, snag risk, visible thread. |
| **Satellite** | 95 | Uplink Terminal, anywhere under coverage | Unlimited | 1 **uplink capacity** per airborne unit; capacity is scarce. Broken only by J ≥ 95. |
| **Autonomy** | n/a (nothing to jam) | none | Unlimited within its assigned box | **Consumes no crew.** Pays in target-selection error (§5.4). |

### 5.2 Jamming

Every jammer has a **Jam Strength J** and radius **r**.

| Emitter | J | r |
|---|---|---|
| EW Post | 70 | 450 m |
| EW Truck | 55 | 350 m |
| Obsidian Jammer Post *(faction upgrade)* | 85 | 520 m |
| Kestrel Uplink Jammer *(T3, anti-satellite)* | 100 | 300 m |
| BLACKOUT (Obsidian super) | 140 | 900 m |

Effective strength at distance *d*: **J_eff = J × (1 − d/r) × 1.3**, so a bubble bites hardest at the centre and frays at the edge — you can skirt it.

If **J_eff > R** the drone's link pip goes **amber**: acquisition time ×2, speed −30%, and the player loses fine control (the drone keeps its current order but will not take a new one). After **4 s** amber, it goes **black**.

**Black behaviour** depends on what you bought:

- Default: orbit in place 12 s, then crash (airframe lost, crew returns).
- With **AI Last-Mile Module**: continue to the last designated point and detonate. This is why that 300 TP purchase is the single most important item in the catalogue and why a jammer is a *partial* defence, not a total one.
- With **Dual-Link Conversion**: fall back to the alternate link (fiber → radio, radio → mesh) after 2 s, at −20% speed.
- Veteran crews add **+8 R** ("push through"): a Rank 3 crew flying a digital-radio FPV (R 55 + 8 = 63) will survive the edge of an EW Truck bubble that kills a rookie.

### 5.3 How it is shown, and how it stays out of the spreadsheet

Four display rules, and nothing else:

1. **The link pip.** Every linked unit carries a three-state pip above it: green / amber / black. That's it. No signal-strength bars, no channel indicators.
2. **Jam bubbles are terrain.** Once an emitter is detected — and emitters self-reveal, because emitting raises Signature to 85 and is picked up by any friendly Radar Mast without line of sight — its bubble draws as a translucent red dome permanently. You plan around it the way you plan around a cliff. New players learn "red dome = my cheap drones die here" in one match.
3. **Link View** (held modifier on desktop, two-finger tap on touch — §19). Everything else desaturates and the control graph draws: thin lines from every friendly drone to its controlling node, relay chains as bright segments with hop counts, satellite units with a small up-arrow, autonomous units with no line at all and a dashed box showing their hunt area. Three seconds in Link View tells you the entire state of your control network. Selected units draw their line always, with no toggle at all.
4. **The fiber thread is a real object.** It draws as a persistent thin orange line from the launch point to the drone and it *stays on the map for 30 s after the drone dies*. It is physically traceable: an enemy unit that finds a thread gets a directional arrow toward the launch site. And it snags: **4%/s in forest tiles, 9%/s under power-line corridors, 1.5%/s crossing a road with traffic.** Snag = thread cut = link black.

The player's actual decision at any moment is only ever "which of four flight cards do I click" — the complexity lives in the world, not in a panel.

### 5.4 The autonomy error model

Autonomy trades a crew slot for judgement. Here is the exact model, because it is the counter-counter that closes the ladder.

Each autonomous unit has **Autonomy Quality Q** (0–100): AI Last-Mile 70, generic autonomous munition 55, Obsidian Autonomy Stack 68, Kestrel optical-nav striker 72.

On terminal approach the unit builds a candidate list: every object inside a **120 m seeker cone** matching its class filter. Each candidate has a **Plausibility P**:

| Candidate | P |
|---|---|
| Real high-value target (tank, EW truck, radar, launcher) | 1.00 |
| Real low-value target (infantry, light truck) | 0.60 |
| **Decoy** (inflatable + thermal signature) | **0.90** |
| Friendly unit | 0.25 |
| Neutral structure / civilian object | 0.15 |

Resolution: with probability **Q/100** the unit correctly rejects the decoys and picks the highest-P *genuine* target. Otherwise it draws from the full candidate list weighted by P.

Modifiers to Q: **each decoy in the seeker cone −5** (a dense decoy field degrades classifier confidence — this is the term that makes deception work); **night without Thermal Optics −20**; **rain or smoke −15**. Modifier to P: a **Thermal Blanket** on a target reduces its P by 0.40 at night, 0.20 by day.

So: a lone tank in an open field dies to an autonomous munition. The same tank sitting between three Decoy Kits (9 decoys at P 0.90, one real at P 1.00) drags a Q 55 munition down to Q 10, so it picks the real tank about 20% of the time — the tank survives four attacks in five — and a few per cent of runs will hit a friendly vehicle if one is standing in the cone. **The counter to autonomy is not electronic warfare; it is lying to a camera.** This is the T3-beats-T3 rung and it is an entirely fresh strategic space.

### 5.5 The counter for each link

| Link | Countered by | Cost of the counter |
|---|---|---|
| Radio | EW Post / EW Truck | The emitter self-reveals and gets hunted by fiber drones and loitering munitions. |
| Mesh | Kill any node in the chain — interceptors on the Mothership, FPVs on Relay Masts | Interceptors need radar cueing; relays are cheap to replace. |
| Fiber | Nets (tunnels, traps), forest and wire terrain, and running *away* past the leash | Nets are static and burn to thermite; terrain is fixed. |
| Satellite | Uplink Jammer (J100, 300 m — you must get close), or kill the Uplink Terminal | Very expensive and very short-ranged; a raid, not a posture. |
| Autonomy | **Decoy Kits, thermal blankets, signature spoofers** | Cheap, but they only work against machines, never against a pilot with a clear feed. |

---

## 6. Pilot slots and crews

### 6.1 The scarcity model

**Crews are the population cap, but they cap *concurrency*, not army size.** A crew is consumed only while it is actively flying a linked drone. Autonomous units consume none. Ground units consume none.

- Starting crews: **6**.
- Crew Quarters: **+4** each, maximum 6 buildings = **24**.
- Veteran Crew (TASKING, 220 TP escalating): **+1** each, arriving at Rank 2. Hard ceiling **30**.
- A typical mid-game army has 14 crews and 40 airframes in stock. You are never limited by aircraft; you are limited by hands.

**Crew states:** Ready (green) → Flying (blue) → Recovering (amber, 8 s, or 5 s with Priority Recovery) → Ready. Plus **Fatigued** (a crew that has flown 5 sorties in 120 s: recovery 8 → 14 s and −10% acquisition until it idles 30 s) and **KIA** (permanent; only from Crew Quarters destruction).

**Crew veterancy.** Rank 1 → 2 at 3 verified kills, 2 → 3 at 9, 3 → 4 (Ace) at 20.

| Rank | Effect |
|---|---|
| 1 Rookie | baseline |
| 2 Operator | +8% drone speed, −15% acquisition time |
| 3 Veteran | +8 Link Robustness, +12% damage |
| 4 Ace | −2 s recovery, +1 free re-attack pass on interceptors, gold chevron |

### 6.2 What a pilot slot feels like to click on

It is a physical object in the UI. Along the bottom of the sidebar sits the **crew rack**: a row of small tiles, one per crew, each showing a two-letter callsign, rank chevrons, a thin fatigue bar, and a state colour. The rack is always visible. It fills up as you build Crew Quarters. It is the first thing a returning player looks at.

- **Click a crew tile** → the camera snaps to the drone that crew is currently flying and selects it. If the crew is idle, the tile pulses and its home Crew Quarters flashes on the minimap.
- **Double-tap** → take manual control of that drone via **Guide**: trace a path and the drone follows it, gaining +15% speed and evasion only while you are actively tracing (§19.2). Fully optional, never required, but it gives the game its highlight-reel moments — threading a fiber drone into a dugout doorway by hand. Cut on iPhone.
- **Drag a crew tile onto a flight card** → force that specific crew onto that sortie. Advanced players drag their Aces onto interceptor runs.
- **Secondary action on a tile** (long-press on touch) → hold the crew in reserve; it will not be auto-assigned, and it is the same reservation Standing Task draws from (§19.4).
- When all tiles are amber or blue, every flight card in the AIR tab greys out simultaneously. The screen tells you, without a word of text, that your air force is grounded. That moment is the game's best teacher.

### 6.3 What consumes slots, and what doesn't

| Consumes 1 crew while airborne | Consumes 0 crews |
|---|---|
| FPV Team, Fiber FPV Team, Multirole Quad, Interceptor FPV, Scout Quad, Recon Wing, Night Bomber, Mothership, Loitering Munition (piloted) | Every ground unit, every structure, Interceptor Battery, Mid-Range Striker (optical nav), Autonomous Munition, any unit running an Autonomy Stack stance |

A Mothership consumes 1 crew of its own and each FPV it releases still consumes its own crew — the Mothership extends *range*, not *capacity*. Obsidian's Autonomy Stack is therefore the only real way to break the crew ceiling, and it is priced accordingly.

### 6.4 Decapitation

Crew Quarters are 4×4, 1,400 HP, and cost 700 MAT. Destroying one **kills its four crews permanently** (they do not come back with the rebuilt structure — you must rebuild and re-staff, 22 s + no crews for 30 s while they arrive). This makes the enemy's rear base a real strategic target for the first time in a C&C-lineage game: a successful deep raid on four Crew Quarters does not cost your opponent buildings, it costs them their ability to fly anything at all for two minutes.

**Kestrel's counter-asymmetry:** Kestrel's Crew Quarters are "Remote Piloting Bays" — they may be built anywhere in the build radius, cost 30% more, and Kestrel crews are **never KIA**; a destroyed bay merely benches its crews for 45 s. Obsidian crews are in forward dugouts, cheaper, and die for real. That single difference gives the two factions completely different rear-area risk profiles.

---

## 7. Recon and fog of war

### 7.1 Three visibility states plus one targeting state

- **Shroud** — never seen. Black.
- **Fog** — seen once, not currently observed. Terrain visible; enemy units shown as **ghosts** with a decay timer (ghosts fade after 25 s and drift a "last known heading" arrow).
- **Live** — currently detected.
- **Designated** — currently detected *and* inside a recon footprint or a pin. Drawn as a white diamond bracket.

**Direct fire requires Live. Indirect fire requires Designated.** That one rule is thesis 3.

Indirect = Loitering Munitions, Night Bomber precision drops, Mid-Range Strikers, Interceptor Batteries against ground, and both superweapons.

### 7.2 Signature and detection

Every unit has a **Signature** value, 0–100, which determines detection range against a given sensor.

Base signatures: Tank 70, Supply Truck 65, IFV 60, EW Truck 50 (**85 while emitting**), Logistics UGV 30, Motorcycle Squad 25, Net Engineer 20, dug-in Net Engineer 8, FPV 18, Recon Wing 45 (it is high and slow).

Modifiers: **moving +30**, **firing +40 for 5 s**, **emitting +35 (and detectable without line of sight)**, **on a road +20**, **in forest −25**, **dug in −40**, **thermal blanket −25 at night**, **night ×0.35 to all optical sensors** (thermal sensors ignore this).

A sensor detects a unit if `Signature ≥ Threshold(range)`, where Threshold rises linearly from 0 at the sensor to 100 at its maximum footprint radius. Practical result: a dug-in Net Engineer is invisible at 200 m; a moving tank on a road at midday is visible at 1,100 m from a Recon Wing orbit. **Movement is the thing that kills you**, which is exactly the doctrinal truth the setting is built on.

### 7.3 Footprints

| Sensor | Footprint | Notes |
|---|---|---|
| Recon Wing | 900 m moving orbit | High altitude, only killable by interceptors and Gun Mounts. The main Designation source. 6:00 endurance. |
| Scout Quad | 250 m | Cheap, disposable, 4:00 battery. |
| Multirole Quad (Recon stance) | 350 m | Reusable, retaskable mid-flight. |
| Designator Team *(Kestrel)* | 600 m, static, dug in | Signature 8. Designates and earns pin payouts. The cheapest persistent Designation on the map. |
| Radar Mast | 1,400 m, **air targets only** | Ignores terrain. Cues interceptors. |
| Forward Node | 300 m ground | Free with the structure. |
| Ground units | 180–300 m | Line of sight blocked by forest and buildings. |

### 7.4 Pins — "Uber targeting"

Any player can right-click-drag on a Live enemy, or on any point inside a friendly footprint, to drop a **Pin**: a 120 m circle or a specific unit lock, lasting **60 s**, costing nothing, limited to 6 active.

A pin makes everything inside it **Designated** for its duration even if the observer dies — a crucial mechanic, because it means recon can call in a strike and then leave. And when anything dies inside your pin, **you** get the pin payout in TP even if your ally fired the shot. In 2v2 this produces the real thing: a dedicated recon player who never builds a strike unit and out-earns the shooter.

### 7.5 What artillery and loitering munitions actually need

A Loitering Munition launch requires a valid Designation **at launch**. During flight:

- If the Designation is lost and the munition has no autonomy module, it **loiters at the last point for 20 s** waiting for re-designation, then self-destructs (you get 40% MAT back as salvage at your Launch Rail).
- If it has autonomy, it proceeds and rolls the §5.4 target error.

This produces the loop the bestiary describes: recon finds, recon holds the pin, the shooter fires, and the defender's correct response is to *kill the spotter*, not the shell.

---

## 8. Unit roster

### 8.1 Design calls before the table

- **The FPV Team is one-way.** It does not return. It is ammunition with a pilot. The Multirole Quad is the reusable one.
- **The AI last-mile FPV is not a unit**, it is a 300 TP upgrade to all FPVs. Collapsing it saves a roster slot and makes the upgrade feel enormous, which it should.
- **All ISR fixed-wings collapse to one Recon Wing** with a Thermal Optics upgrade.
- **Sting-class and recon-hunter interceptors collapse to one Interceptor FPV** with two stances: *Low* (fast, chases rotary and cruise-class targets under 400 m altitude) and *High* (climbs to hunt Recon Wings, 60 s climb, cannot be recalled).
- **Lancet / Molniya / KUB collapse to one Loitering Munition** with an autonomy upgrade.

### 8.2 Compact stat table

Armour classes: **Sf** soft, **Lt** light, **Hv** heavy, **St** structure, **Ar** air-rotary, **Af** air-fixed. Damage types: **Shp** shaped charge, **Frg** fragmentation, **AP** kinetic, **Inc** incendiary, **Ram**. "Crew" = pilot slot consumed while active.

| # | Unit | Fac | T | MAT | Build | HP | Arm | Speed | Link | Range | Damage | Crew | Counters |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Recovery UGV | both | 1 | 500 | 18 | 420 | Lt | 5.5 | radio | — | — | no | FPVs, mines |
| 2 | Net Engineer | both | 1 | 250 | 12 | 220 | Sf | 4.5 | — | — | — | no | Frag, any drone |
| 3 | Motorcycle Squad | both | 1 | 300 | 12 | 260 | Sf | 17.0 | — | 160 m | 45 Frg | no | Mines, nets, Gun Mounts |
| 4 | Scout Quad | both | 1 | 120 | 6 | 40 | Ar | 16.0 | radio | 250 m sight | — | **yes** | Anything, it's paper |
| 5 | FPV Team | both | 1 | 200 | 8 | 55 | Ar | 22.0 | radio | 1,000 m | 260 Shp | **yes** | EW, nets, Gun Mount |
| 6 | Supply Truck | both | 1 | 350 | 14 | 520 | Lt | 14.0 | — | — | — | no | Everything, in daylight |
| 7 | Fiber FPV Team | both | 2 | 420 | 12 | 70 | Ar | 15.0 | fiber | 1,400 m leash | 340 Shp | **yes** | Nets, forest, wires |
| 8 | Multirole Quad | both | 2 | 380 | 14 | 110 | Ar | 19.0 | radio+AI | 900 m | 180 Shp / 90 Frg / Ram | **yes** | EW, interceptors |
| 9 | Interceptor FPV | both | 2 | 300 | 10 | 60 | Ar | 34.0 | radio | 1,200 m | Ram | **yes** | Needs radar cue; EW |
| 10 | Logistics UGV | both | 2 | 400 | 16 | 500 | Lt | 5.0 | radio/fiber | — | (turret mod 60 Frg) | no | EW (stops dead), mines |
| 11 | EW Truck | both | 2 | 700 | 24 | 380 | Lt | 8.0 | — | J55 / 350 m | — | no | Fiber FPVs, loiterers |
| 12 | Recon Wing | both | 2 | 900 | 30 | 200 | Af | 12.0 | radio/mesh | 900 m footprint | — | **yes** | Interceptor FPV (High) |
| 13 | Night Bomber | both | 2 | 1,100 | 34 | 480 | Ar | 8.0 | radio/sat | 1,600 m | 4 drops: 300 Frg / mines / Inc / 80 supply | **yes** | Interceptors, Gun Mounts |
| 14 | IFV | both | 2 | 900 | 26 | 1,250 | Hv | 9.0 | — | 420 m | 130 AP | no | Shaped charge, top attack |
| 15 | Main Tank | both | 2 | 1,600 | 40 | 2,300 | Hv | 7.5 | — | 620 m | 340 AP / 260 St | no | Top attack, mines, fiber FPVs |
| 16 | Loitering Munition | both | 2 | 550 | 16 | 90 | Af | 24.0 | mesh | 2,200 m | 300 Shp | **yes*** | Interceptors, lost Designation |
| 17 | Designator Team | **K** | 2 | 280 | 12 | 180 | Sf | 4.0 | sat | 600 m desig. | — | no | Must be found first |
| 18 | Interceptor Battery | **K** | 3 | 1,200 | 32 | 600 | Lt | 6.0 | terminal AI | 2,000 m | 220 Frg (prox) | no | Loitering munitions, raids |
| 19 | Mid-Range Striker | **K** | 3 | 700 | 20 | 120 | Af | 28.0 | optical nav | 3,600 m | 420 Shp | **no** | Interceptors, **decoys** |
| 20 | Mothership | **O** | 3 | 1,400 | 38 | 520 | Af | 13.0 | relay | +1,200 m control, carries 4 | — | **yes** | Interceptors — kill it, all children drop |
| 21 | Autonomous Munition | **O** | 3 | 620 | 18 | 110 | Af | 22.0 | autonomy | 2,800 m, 400 m hunt box | 320 Shp | **no** | **Decoys**, interceptors |
| 22 | Swarm Flight | **O** | 3 | 1,050 | 26 | 4×90 | Ar | 20.0 | mesh+auto | 1,400 m | 4×200 Shp | **1 for four** | Decoys, Gun Mounts, Blackout-proof |

\* Loitering Munition consumes a crew only until it commits to terminal dive; with the Autonomy upgrade it consumes none.

### 8.3 Unit notes — role, signature ability, day/night

**1. Recovery UGV.** The harvester. Signature ability **Strip**: 9 s to load 90 salvage; can strip combat debris in 4 s. Night: −40% signature, so night harvesting of the contested middle is a real strategy. Two per Salvage Yard is the standard opening; four is greedy; six means you have given up on early aggression.

**2. Net Engineer.** Builder of everything forward. Signature ability **Dig In**: 6 s, Signature 20 → 8, immobile until ordered to move. A dug-in engineer inside forest is functionally invisible and is how you hold a Forward Node.

**3. Motorcycle Squad.** The only unit that captures Grid Taps quickly and the only infantry that matters. Signature ability **Dash**: 6 s at 24 m/s, 40 s cooldown. Its whole reason to exist is thesis 5 — it crosses an exposed stretch faster than an overwatch flight can react. Day: extremely risky. Night: the primary assault vector.

**4. Scout Quad.** 120 MAT of vision. Signature ability **Perch**: land on a roof or hilltop, Signature 18 → 4, becomes a static 250 m sensor until killed or recalled, and the crew is **released** while perched. This is the cheapest persistent vision in the game and the answer to "I have no crews spare."

**5. FPV Team.** The workhorse. One-way. Signature ability **Terminal Commit**: manually order the final dive; the drone ignores new orders but gains +25% speed and cannot be net-trapped in the last 40 m. Night: acquisition +30% slower without Thermal Optics. This unit is why you build EW.

**6. Supply Truck.** 120 supply, fast, huge. Dies inside the kill zone, which is the entire lesson.

**7. Fiber FPV Team.** Unjammable anti-armour. Signature ability **Thread the Door**: against a structure or dug-in position, +60% damage but a 3 s vulnerable hover. Leash and snag rules per §5.3. The correct answer to an EW Post, and the reason the EW Post is not an auto-win.

**8. Multirole Quad.** The generalist, and the one unit that rewards attention. Four stances, switchable in flight (1.5 s):
  - *Recon* — 350 m footprint, no weapon.
  - *Strike* — 180 Shp, one-way.
  - *Drop* — 90 Frg over an area, reusable, must return to a Repair Pad to rearm.
  - *Air-to-Air* — ram, reusable if it survives.
  Signature ability is the stance system itself: retasking a flight of six from Drop to Air-to-Air the instant an enemy Recon Wing shows up is the most satisfying single input in the game.

**9. Interceptor FPV.** Air defence. Requires a **radar cue** to engage at full effectiveness (see §10.3). Two stances: *Low* (34 m/s, hunts rotary and loitering munitions) and *High* (climbs 60 s to reach Recon Wings and Motherships; cannot be recalled once climbing). Signature ability **Second Pass**: if the first engagement roll misses and fuel remains, it makes one more pass at −15% hit chance. Aces get a third.

**10. Logistics UGV.** The kill-zone truck. 60 supply, slow, quiet. Signature ability **Turret Module** (+180 MAT): mounts a 60 Frg gun, turning it into a mobile Forward Node guard. Jammed → it stops dead and sits there being salvage.

**11. EW Truck.** Mobile jam bubble. **It emits**, so it is permanently visible to enemy Radar Masts through shroud. Signature ability **Silent Running**: shut down the bubble, Signature 85 → 50, 4 s to restart. Skilled players pulse their EW rather than leaving it on — that is the EW minigame, and it is two clicks, not a panel.

**12. Recon Wing.** The lights. 900 m orbit, 6:00 endurance, then it must return and rearm (35 s). Signature ability **Stare**: halve the orbit radius to 450 m and double detection threshold quality — used to hold a pin on a specific target through an assault. Night: blind without Thermal Optics, which makes that 400 TP purchase the single biggest tempo swing in the mid-game.

**13. Night Bomber.** **Grounded in daylight — a hard rule, not a penalty.** Reusable, returns to the Repair Pad, four drops per sortie chosen at launch:
  - *HE* — 300 Frg in 70 m.
  - *Scatter Mines* — a 140 m minefield, 90 s to place, kills or cripples ground movement.
  - *Thermite* — burns forest and net tunnels in 6 s, denies a 90 m circle for 45 s.
  - *Resupply* — 80 supply delivered to a Forward Node, **and it is the only carrier that can reach a cut-off position.**
  Signature ability **Dragon Run**: a linear thermite strip 300 m long, used to burn a net tunnel off a road before an assault.

**14. IFV.** Carries one Motorcycle Squad safely (they ride inside). 420 m autocannon that is genuinely good against rotary drones (90 Frg vs Ar). The cheap mobile answer to FPV pressure.

**15. Main Tank.** Rare, precious, situational (thesis 6). Front armour Hv; the **Top** facet takes 2.2× from shaped charges, which is exactly what an FPV brings. Signature ability **Dash**: 8 s at 13 m/s plus an aerosol screen that blocks optical acquisition for 6 s, 60 s cooldown, two charges. A tank is not a line unit. A tank is 1,600 MAT spent to convert a 90-second window — created by an EW bubble, darkness, and a burnt net tunnel — into a broken enemy position. **Cage Kits** (260 TP) give it a 600 HP sacrificial layer against shaped charges only, at −8% speed. Sending a tank out in daylight without EW cover is the clearest "you have misunderstood the game" signal we can build.

**16. Loitering Munition.** Counter-battery and rear-area strike. Requires Designation at launch (§7.5). Signature ability **Hold Orbit**: park it over a pinned area for up to 40 s and strike on your command — the closest thing the game has to artillery, and it makes holding a pin through an enemy counter-attack genuinely tense.

**17. Designator Team (Kestrel).** Static satellite-linked spotter. 600 m Designation, Signature 8 dug in, ignores ground jamming entirely because its uplink goes up. Signature ability **Standing Pin**: maintains one pin indefinitely at no pin cost. The Kestrel economic engine — a pair of these in the middle of the map earn pin payouts all match.

**18. Interceptor Battery (Kestrel).** Merops-class: truck, radar cue, launcher. **No pilot slot** — this is Kestrel's structural advantage, because it means Kestrel can defend its rear without spending crews. 2,000 m, 3 ready rounds, 14 s reload. Signature ability **Cued Volley**: fire two at once at a single target for a combined hit roll. Vulnerable because the truck, its radar, and its launcher are one unit here (we simplified the bestiary's three-target version — see §17) and loitering munitions eat it.

**19. Mid-Range Striker (Kestrel).** The Hornet-class logistics killer. Optical-inertial navigation: **no link, no crew, immune to all jamming**, 3,600 m reach — far enough to hit the enemy's Salvage Yard and truck routes from your own half of the map. Slow enough (28 m/s) that interceptors get a look. Rolls target error (Q 72). Signature ability **Loft**: +1,200 m range and arrives from a random map edge, at the cost of 25 s extra flight time. This is the unit that makes the enemy's *logistics* a target, and it is the reason Obsidian has to invest in decoys and interceptors instead of pure aggression.

**20. Mothership (Obsidian).** A flying relay that carries four FPVs. Extends control +1,200 m from itself and releases its children on command. **Kill it and every child link drops at once** — the single highest-value air target in the game and the cleanest "protect the carrier / hunt the carrier" objective in either campaign. Signature ability **Release**: drop all four carried FPVs simultaneously at a designated point.

**21. Autonomous Munition (Obsidian).** V2U-class. No crew, no link, hunts within a 400 m box you draw, Q 55. Excellent against an unprepared enemy, comically bad against a decoy field. Signature ability **Pack Marking**: two or more in the same box will not double-target, effectively +12 Q against real targets — but they share decoy errors too.

**22. Swarm Flight (Obsidian).** Four quads on one crew via the Autonomy Stack. Mesh-linked to each other, so jamming one does not jam the group; if the link to base drops, they complete their assigned strike autonomously. Signature ability **Sequence**: the four stagger their approach by 1.5 s so a single Gun Mount can only engage one at a time. This is Obsidian's answer to the crew ceiling and its late-game identity.

---

## 9. Kill zone and logistics

### 9.1 How the kill zone emerges

There is no painted region. The band exists wherever an enemy can satisfy four conditions faster than you can cross: **detection** (a footprint over the road), **readiness** (airframes in the hangar), **crews** (an idle tile on the rack), and **flight time**.

We expose this as a single number. When you drag a move order, the path preview shows a per-segment **Exposure** figure in seconds: how long that unit will spend detectable in the open. Next to it the game shows your best estimate of the enemy's **Reaction Time** for that area:

**Reaction = detection latency (2–6 s) + crew ready (0–8 s) + flight time (distance ÷ drone speed).**

If Exposure > Reaction, the segment glows red. If it's marginal, amber. That is the entire logistics game expressed as one colour on a move order, and a C&C player understands it in four seconds.

### 9.2 The Threat overlay

Overlay toggle. It draws your *memory*, not the truth:

- Every friendly loss stains the map (radius 200 m, intensity by unit value).
- Every observed enemy drone launch stains its origin.
- Every known enemy workshop, Relay Mast and EW Post projects its theoretical reach.
- The whole thing decays at 1.5%/s, so a quiet sector fades in about 60 s.

The result is a heat map that looks exactly like a front-line kill zone and is generated entirely by what happened. Move your workshops forward and the enemy's overlay updates itself — with a 40 s lag, because they have to die there first.

### 9.3 Supply

**Nodes.** Command Post: 600 supply, 900 m sustain. Forward Node: 200 supply, 700 m sustain. Nodes drain based on what they are sustaining: **1 supply per ground unit per 20 s**, **3 per tank per 20 s**, **2 per drone sortie launched within the radius**, **1 per structure per 30 s**.

**States.** A unit inside a sustain radius is **Sustained**. Outside it, after 45 s, it goes **Dry**:

- −50% rate of fire
- No special ability recharge (no Dash, no Second Pass, no Terminal Commit)
- No repair, no rearm
- After 3:00 continuously Dry: **attrition**, −1% max HP per second.

Dry is shown as a small amber fuel glyph on the unit. Attrition shows a red one. No numbers, no bars, no per-unit ammo count.

**Carriers.** Supply moves from the Command Post or Salvage Yard to Forward Nodes. Four ways, and the choice is the game:

| Carrier | Load | Speed | Signature | Survives daylight in the band? | Notes |
|---|---|---|---|---|---|
| Supply Truck | 120 | 14.0 | 65 (+20 on road) | **No** | The cheap rear-area option. Fine behind your own line, instantly dead past it. |
| Logistics UGV | 60 | 5.0 | 30 | Usually, if unobserved | Halts permanently if jammed — it just stops and becomes salvage. |
| Motorcycle Squad (courier) | 40 | 17.0 (24 dashing) | 25 | Yes, if it dashes | Dies to mines and nets rather than to drones. The gambler's option. |
| Night Bomber | 80 | 8.0 | 45 | **Night only** | Flies over everything. **The only carrier that reaches a cut-off node.** |

**Cut-off.** A Forward Node that has received no delivery in **90 s** and has no ground route free of enemy control is flagged **CUT OFF**: a red chevron over the node, a voice line, and a minimap ping. From then on:

- Supply drains at double rate.
- At 0 supply the node itself begins losing 4 HP/s.
- Everything in its radius is Dry immediately, not after 45 s.

Encircling a forward position therefore kills it in about 2:30 without any further shooting, and the defender's only answer is a Night Bomber run — which means waiting for dark, which means the whole day/night cycle suddenly matters at a tactical level. This is the single mechanic that carries thesis 5, and it is one bar, one chevron and four carrier choices.

---

## 10. Combat resolution

### 10.1 Damage model

Deterministic damage with a multiplier table. No RNG on hits for ground and direct fire — C&C readability. RNG lives in exactly two places: interception rolls (§10.3) and autonomy target selection (§5.4).

`damage = base × class_multiplier × facet_multiplier × veterancy × (1 − armour_upgrade)`

### 10.2 Armour / damage multiplier table

| | Sf soft | Lt light | Hv heavy | **Top** | St structure | Ar air-rotary | Af air-fixed |
|---|---|---|---|---|---|---|---|
| **Shp** shaped | 0.45 | 1.30 | 1.10 | **2.20** | 0.90 | 0.30 | 0.30 |
| **Frg** frag | 1.60 | 0.75 | 0.20 | 0.35 | 0.30 | 1.40 | 1.20 |
| **AP** kinetic | 0.90 | 1.45 | 1.25 | 1.25 | 0.65 | 0.25 | 0.20 |
| **Inc** incendiary | 1.30 | 0.60 | 0.25 | 0.40 | 1.70 | 0.15 | 0.10 |
| **Ram** | — | — | — | — | — | **kill on hit** | **kill on hit** |

**Top facet.** Ground vehicles with Hv armour have a separate Top facet. Any attack arriving from a rotary drone or a diving munition hits Top. This is why a 200 MAT FPV team threatens a 1,600 MAT tank — 260 × 2.20 = 572 per hit, so four connected FPVs kill it — three will not, at any veterancy, which is exactly the margin we want. **Cage Kits** insert a 600 HP sacrificial layer against Shp only, taking the caged tank to 2,900 effective, so it needs six. That is the intended headline trade and the first number we will tune.

### 10.3 Interception model

Air-to-air is a roll, because it is the one place where "did it work?" should carry tension.

`P(hit) = base × cue × speed_ratio × veterancy`

| Term | Value |
|---|---|
| base — Interceptor FPV (ram) | 0.55 |
| base — Interceptor Battery (proximity warhead) | 0.75 |
| cue — radar-cued (target inside a friendly Radar Mast footprint) | 1.00 |
| cue — visually acquired by a friendly unit | 0.60 |
| cue — uncued | 0.30 |
| speed_ratio | `clamp(interceptor_speed / (target_speed × 1.6), 0.45, 1.15)` |
| veterancy | 1.00 / 1.05 / 1.12 / 1.20 by rank |

A radar-cued Rank 1 Interceptor FPV (34 m/s) against a Recon Wing (12 m/s) gets `0.55 × 1.00 × 1.15 × 1.00 = 0.63`, with a Second Pass at 0.54 → about 83% over the sortie. Against a Mid-Range Striker (28 m/s) it gets `0.55 × 1.00 × 0.76 = 0.42` and needs numbers. Against an uncued target it is `0.17` and is basically wasting a crew — which is why the Radar Mast is the real air-defence building and why killing it is the setup for every deep strike.

### 10.4 Decoys and deception

Three tools, all cheap, all T2, all useless against a human pilot with a clear feed.

- **Decoy Kit** (150 MAT, placed by Net Engineer): three inflatable + thermal decoys of a chosen archetype (tank / truck / radar) in a 60 m cluster, 4:00 lifespan. P 0.90 in the autonomy roll. Visually distinguishable to a *player* who zooms in and looks — decoys don't cast tracks and don't move — but not to a classifier.
- **Thermal Blanket** (80 MAT per vehicle): −25 signature at night, −0.40 P in the autonomy roll at night.
- **Signature Spoofer** (200 MAT, static): projects a false contact on the *enemy Threat overlay* and on their Radar Mast, 300 m from itself, for 3:00. Wastes a loitering munition, a pin and about 20 s of attention. The purest mind-game object in the game.

Rule that keeps this honest and matches the bestiary: **deception affects machine judgement only.** Any attack resolved by a crew with a green link inside 400 m of the target ignores decoys entirely. So the more your opponent leans on autonomy to break the crew ceiling, the more a 150 MAT plastic tank costs them. That is the closing rung of the ladder and it is, as far as we can find, unshipped territory.

---

## 11. Day/night cycle

Full cycle **6:00**, running from match start, synchronised for both players, with a clock on the top bar and a 20 s warning chime before each transition.

| Phase | Length | Effects |
|---|---|---|
| **Day** | 3:00 | Optical detection ×1.00. Night Bombers grounded. Supply carrier throughput ×1.00. Signature penalties for movement at full value. |
| **Dusk** | 0:45 | Optical ×0.65. Night Bombers may launch in the last 15 s. Transition warning. |
| **Night** | 1:45 | Optical ×0.35 (thermal sensors unaffected). Night Bombers active. Supply throughput ×2.00 (drivers run with lights off and the roads are free). Autonomy Q −20 without Thermal Optics. Verified-kill TP ×0.75 unless the killer had thermal or an observer in 400 m. Tank Dash cooldown −20%. |
| **Dawn** | 0:30 | Optical ramps ×0.35 → ×1.00 over the phase. Everything that over-extended at night is suddenly lit. |

Roughly four cycles per 24-minute match. This is the metronome for the 5-minute loop and the reason the Night Bomber and the Main Tank exist as distinct assets rather than as generic units with a stat penalty. It also gives the campaign its structure: half our missions are night missions, and they play completely differently.

---

## 12. Factions and doctrine

### 12.1 KESTREL PACT

*A Western-backed expeditionary coalition running on satellite uplink, veteran crews and precision.*

**Fantasy:** *I see everything and I can reach anything, and I never have to be close.*

**Doctrine:** Fewer, better, safer. Kestrel's crews sit in remote piloting bays deep in the rear and are never permanently killed. Its recon layer is the best in the game (Designator Teams, Thermal Optics early, a 900 m Recon Wing orbit it can afford to keep up all match). Its satellite link (R 95) shrugs off every ground jammer Obsidian can build, but uplink capacity is scarce — six airborne satellite units from one 2,200 MAT terminal — so Kestrel is structurally a *precision* faction that cannot flood.

**Asymmetries:**

- Crew Quarters are Remote Piloting Bays: +30% cost, crews bench for 45 s instead of dying.
- Satellite link on Designator Teams, Night Bombers and (via the terminal) any one selected flight.
- **Interceptor Battery** costs no crew — Kestrel defends its rear without spending sortie capacity.
- **Mid-Range Striker**: 3,600 m, unjammable, crew-free, hits the enemy economy from home.
- Crews gain veterancy 25% faster.
- Starting crews 6, hard ceiling 26 (four fewer than Obsidian).
- Airframes cost ~15% more; build times ~10% longer.

**Weakness:** Kestrel cannot win a volume fight. If Obsidian survives to 14:00 with an intact economy, Kestrel loses on sortie count. And a successful raid on the Uplink Terminal collapses a third of its army's link layer at once.

**Super — ARCLIGHT** (Mission Control, 7:00 cooldown). Designate a 250 m circle **inside a friendly recon footprint** (the recon gate applies even here — this is deliberate and is the best statement of thesis 3 we can make). After 45 s, nine satellite-cued optical-nav munitions arrive from off-map, each independently selecting the highest-value genuine target in the circle, ignoring all jamming. 420 Shp each. Q 85, so decoys still eat roughly one in six. It deletes a clustered army or a structure cluster, and it cannot be fired into the dark unless Kestrel has spent on Thermal Optics.

### 12.2 OBSIDIAN DIRECTORATE

*A continental power running on mesh relay chains, industrial volume and a sovereign autonomy stack.*

**Fantasy:** *I turn off your radio, I outnumber you four to one, and my swarm carries its own network on its back.*

**Doctrine:** Obsidian lost satellite access and answered with mesh and autonomy — exactly the historical pivot in the bestiary. Every Obsidian drone is also a repeater, so **Obsidian's control range grows with its army**: a flight of six pushed forward is its own relay chain, and the deeper it goes the deeper it can go. Its jammers are the strongest in the game. Its Autonomy Lab is the only way in the game to break the crew ceiling.

**Asymmetries:**

- **Mesh repeaters:** every Obsidian air unit acts as a mesh hop (700 m, 4 hops). No other faction gets control range for free.
- **Jammer Post** upgrade: EW Post J70 → J85, r450 → 520.
- **Autonomy Stack** (600 TP): Swarm Flights (4 quads / 1 crew), crew-free Loitering Munitions, Autonomous Munitions.
- **Mothership**: the relay-in-the-sky that puts short-range FPVs 2,600 m deep.
- Airframes ~15% cheaper, workshop build times ~10% faster, hangar capacity 16 not 12.
- Starting crews 6, hard ceiling 30.
- Crew Quarters are forward dugouts: −20% cost, but crews **die permanently** when the building dies.

**Weakness:** Obsidian's recon is worse (no Designator Team, thermal is a later purchase) so it verifies fewer kills and earns less TP per kill; its autonomy is a decoy magnet; and its rear is genuinely fragile — four successful Fiber FPV runs on four dugouts remove sixteen crews forever.

**Super — BLACKOUT** (Mission Control, 7:00 cooldown). A 900 m dome, 60 s. Inside it: **J140**, which severs radio, mesh *and satellite* links (only fiber and autonomy survive); enemy structures lose sidebar access for 20 s; and all Obsidian units inside gain **crew-free autonomy** at Q 60 for the duration. It is the Iron Curtain of this game: 60 seconds during which your opponent's army is inert stock and yours flies itself. The counter is to have bought fiber, to have decoys down, and to not be standing in one place.

### 12.3 Why this split works

The two factions answer the same question — "how do I control a drone at range?" — with opposite architectures, and every asymmetry falls out of that one choice rather than being bolted on. Kestrel goes *up* (satellite, few, precise, safe crews, superb recon, poor volume). Obsidian goes *sideways* (mesh, many, cheap, fragile crews, poor recon, superb volume and jamming). Their supers are mirror images: one reveals and strikes, one blinds and swarms.

---

## 13. Tech tiers

### Tier 1 — Radio (0:00 – ~4:30)
**Gate:** Command Post. **Structures:** Generator, Salvage Yard, Crew Quarters, Drone Workshop. **Units:** Recovery UGV, Net Engineer, Motorcycle Squad, Scout Quad, FPV Team, Supply Truck. **Feel:** pure C&C. Harvest, expand, raid. Everything is radio, nothing is jammed yet, the whole map is open. **Tasking unlocks:** Veteran Crew, Priority Recovery.

### Tier 2 — Electronic warfare and fiber (~4:30 – ~12:00)
**Gate:** Radar Mast. **Structures:** Spool Plant, EW Post, Relay Mast, Vehicle Depot, Net Works, Airfield, Launch Rail, Repair Pad, Gun Mount, Forward Node. **Units:** Fiber FPV, Multirole Quad, Interceptor FPV, Recon Wing, Night Bomber, Logistics UGV, EW Truck, IFV, Main Tank, Loitering Munition, Designator Team (K). **Feel:** the map partitions. Red domes appear. Your radio flights start dying and you buy fiber, or you buy AI Last-Mile and push through. Recon becomes the gate on everything. This is the longest and most important tier and it is where most matches are actually decided. **Tasking unlocks:** AI Last-Mile, Dual-Link, Thermal Optics, Cage Kits, Spool Extension.

### Tier 3 — Autonomy and satellite (~12:00 – end)
**Gate:** Uplink Terminal (K) / Autonomy Lab (O). **Structures:** Mission Control, Uplink Jammer (K). **Units:** Interceptor Battery (K), Mid-Range Striker (K), Mothership (O), Autonomous Munition (O), Swarm Flight (O). **Feel:** the crew ceiling breaks, links stop being jammable, and the counter-play shifts entirely to deception and to killing high-value single nodes (Uplink Terminal, Mothership, Mission Control). **Tasking unlocks:** Autonomy Stack, Uplink Priority, Deception Package.

### Tier 4 — 2028 escalation (STUB — owned by `spec-futures-2027-2028.md`)
Reserved. Structural hooks already in place for the futures spec to fill:

- A fourth gate structure per faction, prereq Mission Control, cost band 3,500–4,500 MAT.
- Three empty Tasking catalogue rows per faction in the 700–1,200 TP band.
- An `AutonomyQuality` ceiling currently capped at 85 that can be raised.
- A `LinkRobustness` slot above satellite (R 95) for whatever succeeds it.
- A speed band above 40 m/s that current interceptors cannot reach (the jet-Geran problem from the bestiary), to be answered by a T4 interceptor.
- A campaign-era flag on every mission so the campaign can be re-skinned "one year later."

Candidate content, not yet specified: jet-class one-way attack drones, autonomy-versus-autonomy engagements, counter-autonomy as a full doctrine, orbital denial events that switch off satellite links mid-match, and surge-production economics.

---

## 14. Match structure

### 14.1 Modes

- **Skirmish / 1v1** — the reference mode. Target length **20–28 minutes**.
- **2v2** — the mode the pin economy is designed for; expect one player to specialise into recon and TP while their partner carries strike. Target length **28–40 minutes**.
- **Co-op vs AI** — 2 players vs 2 AI, same rules.
- **Campaign** — §15.

No free-for-all at launch (the kill zone logic and the Threat overlay assume two sides).

### 14.2 Victory conditions

Default skirmish uses all three, first to trigger:

1. **Command Post destroyed** — classic, immediate.
2. **Grid Control** — hold 3 of 5 Grid Taps continuously for **4:00**. The anti-turtle clock. A banner counts down on both screens from 4:00, so it is a visible, contestable threat rather than a surprise.
3. **Annihilation** — no structures and no production capability remaining.

Surrender is always available. A player whose last Crew Quarters dies is *not* defeated — they can still fight with ground units, Interceptor Batteries and autonomous units — which keeps the endgame interesting rather than a formality.

### 14.3 Map design rules

**Sizes.** 1v1: **2,400 × 2,000 m** to **3,000 × 2,400 m**. 2v2: **4,000 × 3,200 m**. Base pockets sit in opposite corners or opposite short edges, centre-to-centre **1,800–2,400 m** on 1v1 — deliberately just outside a Relay-extended radio FPV's reach (1,000 + 700 = 1,700 m) so that hitting the enemy base requires either forward structures, fiber, a Mothership, or a Mid-Range Striker. That one number governs the whole early game.

**The contested band.** Maps are laid out so the middle **900–1,400 m** contains: 3–5 rich Wreck Fields, 3 of the 5 Grid Taps, and the only two through-roads. We do not paint a kill zone; we build a map where a kill zone is the inevitable emergent consequence of those three facts.

**Roads.** Movement +60%, Signature +20. Trucks travel at full speed only on roads. Roads are the anchor points for Net Tunnels. Each 1v1 map has exactly two through-routes across the band plus one slow off-road route, so interdiction is achievable but never total.

**Forests.** Block ground line of sight. Signature −25. **Fiber snag 4%/s.** They are the safe approach for ground units and the dangerous approach for tethered drones — a genuinely novel terrain trade-off.

**Power-line corridors.** Long thin strips, no line-of-sight effect, **fiber snag 9%/s**, and they usually run parallel to the roads. A fiber player's route planning becomes real terrain reading.

**Dead ground.** Gullies and cuttings: Signature −20 and hidden from ground sensors but *not* from a Recon Wing overhead. Teaches the difference between ground and air observation in one push.

**Hilltops and rooftops.** Perch points for Scout Quads (+80 m to the perched footprint).

**Neutral structures.** Villages and industrial sheds: block line of sight, garrisonable by a Motorcycle Squad (+30% HP, 240 m sight), and destructible, so they are not permanent.

**Symmetry.** 1v1 maps are rotationally symmetric at launch. Campaign maps are not.

---

## 15. Campaign shape

Two campaigns, nine missions each, crews persist between missions with their veterancy, and TP carries over at 50%. Missions run 20–45 minutes. Every mission teaches exactly one mechanic and then tests it.

### KESTREL PACT — "THE LIGHTS"

1. **Road Head.** A relief convoy is crossing the band in daylight and being picked apart. No base; you control four vehicles and one Scout Quad and must read Exposure against Reaction to get three of four across. *Teaches: the kill zone.*
2. **Scrap.** First base. Contest a wreck field against a raiding force; learn combat debris and the value of harvesting a battlefield you just won. *Teaches: economy.*
3. **The Red Dome.** Your radio flights start dying to an EW Post you cannot see until your Radar Mast goes up. Kill the jammer with anything that isn't radio. *Teaches: EW and the link ladder.*
4. **Spool.** First Spool Plant. Fiber drones through a forested valley strung with power lines — the tether snag tutorial, with a timer. *Teaches: fiber's costs.*
5. **Eyes Off.** Obsidian recon-hunters are stripping your Recon Wings. Escort, intercept, and learn that losing ISR means losing the ability to fire at all. *Teaches: the recon gate.*
6. **Starvation.** A friendly garrison is cut off. Hold until dark, then run four Night Bomber resupply sorties through a defended corridor while keeping your own line open. *Teaches: supply, cut-off, day/night.*
7. **Decapitation.** Obsidian raids your rear: three simultaneous strikes on your piloting bays while your army is committed forward. Defensive, frantic, and the mission where players finally understand what crews are. *Teaches: crews as a target.*
8. **Middle Strike.** Offensive. Uplink Terminal up; use Mid-Range Strikers and Designator Teams to shut down a supply road 3,400 m away while defending against a conventional push. *Teaches: T3 Kestrel identity.*
9. **Arclight.** Hold a line for twelve minutes while Mission Control charges under constant attack, then break an Obsidian corps headquarters in a single designated strike. *Finale.*

### OBSIDIAN DIRECTORATE — "THE WEAVE"

1. **Volume.** Mass radio FPVs against a road. No subtlety, no jamming, pure C&C. *Teaches: the sortie loop.*
2. **The Ridge.** Put an EW Post on a ridge and take a position under its cover with motorcycles and an IFV. *Teaches: EW as an offensive tool.*
3. **Lockdown.** Interdict an enemy supply road for six continuous minutes. The win condition is denial, not destruction. *Teaches: logistics as an objective.*
4. **Chain.** Your mesh keeps getting cut — Relay Masts falling to fiber drones. Defend the chain while pushing it forward. *Teaches: mesh's fragility.*
5. **Carrier.** Escort a Mothership 2,600 m deep, release four FPVs onto an Interceptor Battery, and get the Mothership home. *Teaches: motherships and high-value nodes.*
6. **Night Work.** Night-only mission. Bombers, mines, thermite, a tank dash through a burnt net tunnel. *Teaches: night and the tank's window.*
7. **No Hands.** Scripted: all your crews are killed in the opening cinematic. Fight the entire mission with autonomous units — against an enemy who has read the manual and deployed decoys. *Teaches: the autonomy error model, by making it hurt.*
8. **The Terminal.** Destroy a Kestrel Uplink Terminal under full enemy air superiority, using Blackout-precursor jamming to make the approach. *Teaches: countering satellite.*
9. **Blackout.** Final assault under a 60-second dome, repeated three times as the cooldown cycles, against a dug-in Kestrel position. *Finale.*

Both campaigns share four maps from opposite sides, which is cheap content and makes the asymmetry land.

---

## 16. Balancing philosophy and first tuning levers

### 16.1 Philosophy

**1. Cheap things trade up, but only with enablers.** A 200 MAT FPV Team should kill a 1,600 MAT tank — but only four of them, only with a live link, only from a vector that isn't jammed, and only if the tank's cage is gone. The enabler stack is the skill. A player who can assemble "EW suppressed + recon up + four crews idle" is *supposed* to delete a tank for 800 MAT. A player who clicks four FPVs at a caged tank sitting under a Gun Mount inside a friendly EW bubble should lose all four for nothing.

**2. No unit is a hard counter to another unit; every unit is a hard counter to a *link*.** We never want "build X to beat Y." We want "their strike layer is radio, so I buy jammers; they bought fiber, so I buy nets and fight in the forest; they went autonomous, so I buy decoys."

**3. Engagement swing cap.** No single engagement should move more than **18%** of the losing player's current army value. If our telemetry shows fights routinely swinging 30%+, damage numbers come down and HP goes up before anything else changes.

**4. Comebacks come from TP, not from handicaps.** No rubber-banded income, no scaling bonuses. The defender verifies more kills, so the defender out-earns on merit. That's the whole system.

**5. Readability beats fidelity, every time.** If a mechanic cannot be understood from watching the screen with the sound off, it is wrong, however true to the research it is.

### 16.2 Target metrics

| Metric | Target |
|---|---|
| Match length (1v1, even skill) | 22–26 min |
| First contact | 2:40–3:30 |
| First Radar Mast | 4:30–5:30 |
| First fiber sortie | 8:00–10:00 |
| T3 gate structure complete | 13:00–16:00 |
| Super fired | at most twice per match |
| Peak concurrent crews used | 70–85% of ceiling |
| APM required to play competently | ≤ 80 |
| Recovery UGVs alive at 15:00 | 4–7 |

### 16.3 First tuning levers, in the order we will pull them

1. **Crew recovery time (8 s).** The master throttle on the whole game's tempo. ±2 s changes everything downstream. Touch this before any unit stat.
2. **Combat debris yield (35%) and decay (2%/s).** Controls how violent the mid-map is. Raise the yield and the map becomes a brawl; lower it and the game turns passive.
3. **Jam radius (450 m) and strength (70).** Controls how fast the ladder is climbed. If fiber comes out before 8:00, cut the radius.
4. **Top-facet shaped multiplier (2.20).** The single number that decides whether tanks exist.
5. **TP per verified kill (12%).** Controls how much the merit economy matters relative to materiel. Currently tuned to make TP roughly 25% of a player's total "spend value" across a match.
6. **Interceptor base hit chance (0.55 / 0.75).** Decides whether air is contested or dominant.
7. **Night length (1:45 of 6:00).** Controls how much of the match is bomber-and-tank time.
8. **Uplink capacity (6) and Obsidian crew ceiling (30).** The faction balance dials of last resort — we change these only after everything above is stable, because they alter identity, not just numbers.

We will ship an in-game replay tool with a per-engagement value-swing readout on day one of internal playtesting. We tune from telemetry, not from arguments.

---

## 17. What we cut from the research, and why

**Naval drones, in full.** Magura, Sea Baby, Bullfrog-on-hull, deck-launched FPV swarms. Cut. They require a water movement domain, hull physics, a second pathing solution, coastal map authoring, and a whole set of units that interact with none of the game's six pillars. The kill zone is a land phenomenon. For a 5–12 person studio this is the single largest scope saving available and it costs us nothing thematically. *Revisit as a post-launch campaign set piece, never as a skirmish domain.*

**Strategic deep strike.** Shahed/Geran-class, FP-1/FP-2, the Flamingo cruise missile, 1,000–3,000 km ranges, factory and refinery strikes. Cut from skirmish entirely. These are campaign-map weapons and we do not have a campaign map — we have a linear campaign. Keeping them would require an off-map strategic layer and a separate air-defence minigame to support something that cannot meaningfully happen inside a 24-minute tactical match. What survives is the *echo*: Kestrel's Arclight arrives from off-map, and two campaign missions are framed as interdiction of a strategic corridor.

**The campaign strategic map.** Cut. Nine linear missions with persistent crews gives us 80% of the emotional payoff for 15% of the cost.

**Separate control-channel and video-channel jamming.** Cut. Real and interesting and exactly the kind of fidelity that turns the game into a spreadsheet. One Jam Strength, one Link Robustness, one three-state pip.

**The three-part interceptor battery** (separate truck, radar and launcher as three targets to hide and hunt). Cut to a single vehicle. The bestiary is right that it's more interesting, but it triples the micro cost of one unit for a nuance most players will never see. The Radar Mast already carries the "kill their cue" idea at base scale.

**The AI last-mile FPV as a distinct unit.** Folded into a 300 TP global upgrade. Better as a moment ("everything I own just got better") than as a sidebar entry.

**Dual-link FPVs as a distinct unit.** Same — a 350 TP upgrade.

**Sub-variant sprawl.** One Recon Wing, not Orlan/ZALA/Supercam/Leleka/Furia/Shark. One Loitering Munition, not Lancet/Molniya/KUB/V2U (V2U's autonomy becomes Obsidian's Autonomous Munition, which is a genuinely different unit). One Interceptor FPV with two stances, not Sting-class and recon-hunter. One Night Bomber, not Vampire/Kazhan/Nemesis. Roughly 40 real systems become 22 buildable units, which is already at the top of what a C&C sidebar reads cleanly.

**Mine-clearing vehicles (Uran-6 class).** Cut. Mines become a Night Bomber drop and a Net Engineer clearance action; a dedicated de-mining unit is a chore, not a decision.

**Casevac and body-count management.** Cut. Crews die or they don't. We are not building a medical logistics simulator.

**Manned aviation entirely.** No helicopters, no fixed-wing strike aircraft, no MANPADS units, no crewed SAM. The setting's whole argument is that airframes replaced airplanes; including both muddies the thesis and doubles the air-combat design. Air defence is Interceptor FPVs, Interceptor Batteries and Gun Mounts. Nothing else.

**Balloon launch as a unit mechanic.** Folded into Arclight's off-map arrival and the Mid-Range Striker's Loft ability, rather than a separately built balloon asset.

**The autonomous net turret (microphone-array class).** Merged into the Net Trap. It was a lovely detail and a redundant sidebar entry.

**Infantry as a real arm.** Reduced to three units (Motorcycle Squad, Net Engineer, Designator Team) with no cover system, no squad-member modelling and no garrison depth beyond a flat bonus. The infantry squad being "the smallest and most precious piece" is expressed by making it the only thing that captures Grid Taps, not by simulating it.

---

## 18. Open questions

1. **Is the hangar a step too far from C&C?** It is the mechanical expression of thesis 4 and it makes the sortie the atomic action, but it means air units do not appear on the map when built. The first playtest question is whether "click card, click map" reads as natural or as a chore. Fallback: air units spawn on the map as normal and crews become a conventional population cap, which costs us most of pillar 2.

2. ~~**Does the Threat overlay actually get used, or does it become wallpaper?**~~ **Resolved by the platform change (§19.2).** It is now always-on at 25% opacity and goes full-opacity during a move-order drag. No toggle, on any platform.

3. **How much manual FPV piloting do we support?** Double-clicking a crew tile to fly a drone by hand is a highlight-reel feature and a potential balance disaster if manual flight is meaningfully better than ordered flight. Current intent: manual flight gains +15% speed and evasion but the player loses macro time — a pure skill/attention trade. Needs measurement. **Touch has already forced a change (§19.2):** direct flight becomes the *Guide* path-trace gesture on iPad and PC, and is cut entirely on iPhone. Whether an iPhone player is measurably worse off for losing it is now a balance question across two rulesets, not one.

4. **Is TP legible enough as a second currency?** Two bars at the top of the screen is a real cognitive cost. Alternative under consideration: fold TP into a single "Tasking" tab meter with no top-bar presence, discovered rather than tracked.

5. **Do Grid Taps undercut the salvage economy?** If passive income is too strong the harvester loop — and with it the reason to fight over the middle — evaporates. 22 MAT / 10 s is a guess. This is the number most likely to be wrong on day one.

6. **Can the AI play this?** A competent skirmish AI needs to reason about links, recon gates, supply cut-off and day/night. That is substantially harder than a C&C build-order AI and is a genuine schedule risk for a 5–12 person team. The technical spec must cost this honestly, and we should be willing to ship a scripted "doctrine AI" with three fixed behaviours rather than a general one.

7. **Is Obsidian's free mesh repeater too strong in 2v2?** Two Obsidian players compound each other's control range in a way Kestrel cannot. May need a rule that allied mesh chains do not interlink.

8. **Does the tank survive contact with players?** It is expensive, situational, and requires three enablers. There is a real risk it is simply never built, which would be a shame. If telemetry says tanks appear in under 30% of matches, the first fix is dropping its cost to 1,300 MAT and making Cage Kits a T2 MAT purchase rather than a TP one.

9. **Match length.** Four day/night cycles in 24 minutes may make night feel frequent rather than special. The alternative — a 9:00 cycle with a 3:00 night, giving two-and-a-half cycles per match — makes each night a genuine event but risks a player never seeing one in a short game.

10. **Naming and tone.** "Kestrel Pact" and "Obsidian Directorate" are placeholders that we like. They must survive a legal pass and, more importantly, a read-aloud test with the voice-over script. The fictional Eastern European setting needs a name and a geography before the campaign can be written properly.

11. **Where does the fourth tier actually start?** We have reserved the hooks, but if T3 lands at 13:00–16:00 in a 24-minute match, a fourth tier may have nowhere to live in skirmish and may be campaign-only. That is a question for `spec-futures-2027-2028.md` to answer, and it should feel free to tell us the match needs to be longer.

12. **Does Compact fork the balance?** iPhone's 16-unit roster, shorter cycle, 20-crew ceiling and the forced radio/relay range cut (1,000→800 m, +700→+500 m) mean we are shipping two balance tables, not one. We believe the ladder (§2) survives compression, but this needs its own telemetry pass. If the two diverge badly, the fallback is to make Compact a separate *mode* on iPad and PC too, so everybody plays the same tuned ruleset when they play the short game.

13. **Is Standing Task a skill-killer?** The crew-reservation rule (§19.4) is designed to stop automation from buying sortie capacity, but a good player may still discover that reserving eight crews to three Engagement Boxes outperforms manual play on a phone. If it does, the fix is to make reserved crews cost a small standing penalty — recovery 8 s → 11 s while reserved — so automation is convenient but slightly slower, never free.

14. **Cross-play and input parity.** Squads, the always-on Threat overlay and the Engagement Box now ship on every platform, which keeps the designs converged. But a PC player with a keyboard still has faster card access than a thumb. Do we allow iPad–PC cross-play in ranked, or only in casual and co-op? Unresolved, and it should be settled before the UI spec locks the card rack layout.

---

## 19. Platform: touch-first implications

**Target order changed (client, Sep 2026): iOS first — iPad primary, iPhone compact second — then PC on Steam.** Nothing in §§1–18 changes except where noted here and flagged in §18. The design survives the move because pillar 7 was already "one select, one command, one overlay toggle"; what follows is the audit.

### 19.1 What gets better on touch

**The sortie.** Tap a flight card, tap a target. The game's primary verb is two taps with no travel and no click-drag, and the card rack sits under the thumb. By luck, the atomic action was already built for glass.

**Pins.** Long-press inside a friendly footprint drops a Pin (§7.4). On desktop this is a modifier-drag most players never find; on touch it is the most natural gesture on the device — and it carries thesis 3 and half the TP economy. Expect pin usage to roughly double on iPad, and to tune the 6-active cap downward if it does.

**The Engagement Box.** Drag a rectangle to assign a hunt or overwatch area. Already required by the Autonomous Munition's 400 m hunt box, we promote it to the universal primitive for any flight or ground group: units inside engage automatically, and tapping the box dismisses it. It does most of what control groups did, better, on a small screen.

**Link View** becomes a two-finger-tap toggle rather than a held key — cleaner, and it ships that way everywhere.

### 19.2 What gets harder, and the answer

**Control groups.** No number row. Answer: **Squads** — two-finger long-press binds a selection to a persistent chip above the card rack (max 6); tap to select, double-tap to centre. Plus **double-tap a unit to select all of that type on screen**. Bindings become visible objects, which is better for new players on every platform, so this ships everywhere.

**A 22-unit sidebar on a 6-inch screen.** iPad: unchanged. iPhone: the sidebar becomes an edge-swipe drawer with the same six tabs, the roster drops to 16 (§19.5), and only the card rack and crew rack stay permanently on screen.

**The Threat overlay toggle.** A toggle competing for a thumb mid-fight will not get used — which was already open question #2. Touch forces the resolution: **on all platforms the overlay is always on at 25% opacity**, going full-opacity for the duration of a move-order drag and carrying the Exposure colouring (§9.1). It appears exactly when the player is deciding to move something. Straight improvement; closes open question #2.

**Manual FPV piloting.** Virtual sticks are bad and a picture-in-picture flight view on a phone is worse. Answer: **Guide** — drag a path line the drone follows, gaining the same +15% speed and evasion *only while the player is actively tracing*. Same attention-versus-macro trade, expressed as a gesture. On iPhone, manual control is cut and the FPV keeps only Terminal Commit. Flagged in open question #3.

**Multirole Quad stances.** Four stances behind an in-flight menu is the submenu pillar 7 forbids. On iPad the stances become four flight cards drawing from one shared hangar stock — you choose at launch, not after — and in-flight retasking survives only as a single *Recon ↔ Air-to-Air* toggle. On iPhone the unit is cut.

### 19.3 Session shape: iPad vs iPhone

iPad runs the reference game unchanged. **iPhone runs a Compact ruleset**, because 24 minutes is not a phone session and 2,400 m is not a 6-inch map:

| | iPad / PC | iPhone Compact |
|---|---|---|
| 1v1 map | 2,400 × 2,000 m | **1,800 × 1,500 m** (−25% linear, −44% area) |
| Base centre-to-centre | 1,800–2,400 m | **1,400–1,700 m** |
| Match length | 22–26 min | **14–18 min** |
| Day/night cycle | 6:00 | **4:30** (Day 2:15 / Dusk 0:35 / Night 1:20 / Dawn 0:20) |
| Crew ceiling | 30 | **20** |
| Grid Taps | 5 | **3** |
| Grid Control hold | 4:00 | **2:30** |
| Roster | 22 units | **16 units** |

**One forced numeric change.** §14.3 sets base separation just outside a relay-extended radio FPV's reach (1,000 + 700 = 1,700 m). At Compact separations that guarantee breaks and the early game collapses into base-to-base FPV trading. On Compact maps only: **radio FPV control range 1,000 → 800 m, Relay Mast extension +700 → +500 m** (sum 1,300 m, safely under the 1,400 m floor). Every other number in this document is unchanged. Flagged in §18.

### 19.4 AI assist, and the line it must not cross

Four defaults ship on, every platform: **auto-target** (units engage in range without orders — already true via Overwatch), **auto-crew assignment** (highest-readiness idle crews; overriding it is always optional, never required), **smart rally** (a produced unit walks to the rally point and adopts the nearest friendly group's stance), and **auto-formation** (spacing and role ordering maintained; no formation UI exists). One is optional and dangerous: **Standing Task**, a flight card set to auto-launch at anything entering an Engagement Box.

**The rule that protects crew scarcity:** *assist may spend airframes, orders and attention, but never a crew the player has not explicitly committed.* Standing Task draws only from crews **reserved** to that box, and reserved crews show as unavailable on the rack. Reserve four crews to a road and you are down four crews everywhere else, visibly, until you release them. Assist buys convenience; it never buys sortie capacity.

Two supporting rules: assist **never chooses a link** — radio versus fiber versus autonomy stays manual on every platform — and assist **never earns the manual bonus**, so an auto-launched strike gets no Terminal Commit and no Guide bonus. A player who taps every sortie by hand stays meaningfully better, which is the skill gradient we want.

### 19.5 The iPhone roster cut

22 units → 16, iPhone only, with the capability preserved wherever it carries a pillar:

- **Multirole Quad** — cut (stance submenu; roles covered by FPV Team, Night Bomber Drop, Interceptor FPV).
- **IFV** — cut; Motorcycle Squads travel unmounted.
- **Supply Truck** — cut, leaving three carriers (Logistics UGV, Motorcycle courier, Night Bomber); Compact's shorter crossings made four redundant.
- **Designator Team (K)** — cut as a unit, but **Standing Pin** moves onto the Recon Wing, so Kestrel's pin economy survives.
- **Mothership (O)** — cut as a unit, but its +1,200 m control extension becomes an Obsidian Relay Mast upgrade, so the mesh identity survives.
- **Swarm Flight (O)** — cut; the Autonomy Stack instead grants crew-free stances to Loitering and Autonomous Munitions only.

Ability merges: Interceptor FPV Low/High becomes automatic by target altitude, and EW Truck Silent Running becomes a card toggle rather than a micro pulse. Both supers, both economies, all five link types and the full day/night cycle are unchanged — **Compact is a shorter match, not a shallower one.**
