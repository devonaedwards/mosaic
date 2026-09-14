# KILL ZONE — Technical Specification

**Version 1.0 · Technical Direction · September 2026**
Written against `brief.md`, `spec-gameplay.md` v0.9, and `spec-futures-2027-2028.md` §6–7.

**Platform change, v1.0:** the primary target is **iOS — iPad first, iPhone as a compact layout** — with **PC on Steam as the second platform from the same codebase**. This supersedes the "PC first" line in `brief.md`. Every budget, cap, input assumption and milestone in this document is written mobile-first and desktop-second. Cross-play between iOS and PC is a requirement, not a stretch goal, and it is the reason several decisions below (fixed point, lockstep, relay transport) are non-negotiable rather than merely preferred.

This document tells a 5–12 person studio what to build, in what order, over 18–24 months to early access. Where the gameplay spec gives a number, this document says where that number lives, at what precision, and what it costs per tick. Where the gameplay spec is silent, this document makes the call.

**Standing conventions.** 1 world unit = 1 metre. All simulation distances are metres in fixed point. Ticks are the simulation quantum; 1 tick = 31.25 ms. "Budget" figures are worst-case per-tick or per-frame costs on the **target device (A14/A15-class iPad, §12.7)**, not averages and not desktop.

---

## 1. Engine and language

### 1.1 The decision

**Unity 6 LTS, URP Mobile renderer, C#, IL2CPP → ARM64 on iOS, with the entire simulation living in a plain-C# assembly (`KZ.Sim`) that has zero references to `UnityEngine`.** Hot simulation loops are Burst-compiled jobs over `Unity.Collections` native arrays — Burst emits ARM64 NEON for iOS from the same source that emits AVX2 for PC. Presentation is a separate assembly (`KZ.View`) that reads sim state and never writes it.

We are not using Unity DOTS/Entities. We are using Unity as a renderer, an asset pipeline, a platform abstraction and an editor host, and writing our own entity storage. That distinction is the whole architectural bet and §2 explains it.

iOS as primary does not change the engine choice. It changes the *strength* of the choice: the gap between Unity and the runners-up is wider on iOS than it was on PC.

### 1.2 Why, against the two runners-up, with iOS as the primary target

The runners-up were **Godot 4.4 (C#)** and **Unreal 5.5**. Scoring against the criteria the brief cares about, now judged on an A15 iPad rather than a desktop:

**Deterministic simulation, and cross-play specifically.** Lockstep multiplayer, replayable telemetry and headless balance sims all die without determinism (§11, §16) — and shipping iOS *and* PC means the sim must produce bit-identical results on **ARM64 and x86-64**. That is the single hardest constraint in the project, and it eliminates float simulation outright (§2.5): ARM's fused multiply-add, its different `libm`, and Apple's compiler settings diverge from x64 within thousands of ticks. With integer fixed point the requirement becomes trivially satisfiable on both, because a 64-bit integer multiply is exactly defined everywhere (ARM64 gives us the high half in one `smulh`; x64 gives it in one `imul`). Unity and Godot both *permit* this architecture; Unreal actively fights it — its tick graph, actor lifecycle, `FMath`, physics and engine-owned worker threads mean determinism costs you 80% of the engine and then a permanent argument with the remaining 20%. **Unity and Godot tie, Unreal loses decisively.**

**iOS runtime and toolchain maturity.** This is where Godot loses the project. Unity's iOS path — IL2CPP AOT to ARM64, Burst for the hot loops, a first-party Metal backend, Xcode project generation, entitlements, Game Center, StoreKit, On-Demand Resources, TestFlight and App Store Connect upload from CI — is a solved, boring, decade-old pipeline. Godot 4's C# on iOS depends on .NET AOT for ARM64, which only became viable recently and remains the least-exercised corner of its export matrix; it has no Burst equivalent, so our fixed-point flow-field and grid jobs run on a plain AOT'd runtime with no auto-vectorisation (our estimate, based on comparable integer grid kernels, is **4–6× slower on exactly the code that has to fit in a thermally-limited mobile core budget**); its Metal backend is newer than its Vulkan/MoltenVK path; and Game Center, IAP and ODR all arrive via community plugins we would then own. On a desktop-only project that is an acceptable trade for an MIT licence. On an iOS-primary project with a nine-person team it is a second full-time platform engineer we have not hired. **Unity wins decisively; Godot second but not close.**

**Binary size, memory and thermals.** An App Store submission has a hard ceiling and a soft one: the technical limits are generous, but the **cellular-download threshold (200 MB) and user tolerance for a >2 GB install are real**, and iOS jetsam will kill an app that exceeds its device's resident budget (~1.4 GB on a 3 GB device). A minimal Unity iOS binary is roughly 45–70 MB of engine plus our code; a minimal Unreal 5 iOS binary is several hundred megabytes before a single asset, and UE5's mobile renderer, even with Nanite and Lumen off, carries a per-frame CPU and memory overhead that translates directly into thermal throttling in a 25-minute match. Godot's binary is the smallest of the three. **Godot wins on size, Unity is comfortably inside budget, Unreal is a problem we would spend months managing.**

**RTS pathfinding at scale on a mobile core.** Nobody's built-in navigation survives 800 ground agents plus 1,400 air agents, so we write our own (§3.5) in all three cases. What matters is how cheaply we can run wide, data-parallel integer code on an A-series performance core while an efficiency core handles audio and the OS. Burst is the whole argument: our flow-field integration measures 8–20× faster than plain C# on x64 and, critically, Burst's ARM64 backend keeps most of that on Apple Silicon. Godot would mean writing the solver in GDExtension C++ — a second language, a second build, and a marshalling boundary on the hottest data. Unreal gives us C++ natively, which is a genuine advantage, and then loses everywhere else. **Unity wins on effort-per-millisecond, and the millisecond matters more on mobile than it ever did on desktop.**

**Modding and data-driven units.** All three load YAML. The differentiator is shipping a *tool*. Unity's editor is scriptable in the same language as the game, so our map editor and balance dashboard (§14) are editor windows written in C# by gameplay engineers, not a separate application. Godot matches this closely. Unreal's Slate extension work is not something a nine-person team learns on the side. Note that iOS constrains what modding can *be* (§15): the App Store forbids downloading executable code, so the mod surface on iOS is data-only and curated, which reduces this criterion's weight but does not reverse it. **Unity ≈ Godot, Unreal loses.**

**Team size.** A 5–12 person team can carry one platform specialist, not three. Unity's C#-only stack means every engineer can touch every layer, including the Xcode-facing parts. Unreal mandates C++ for sim, Blueprints for glue, and someone who owns the mobile render path — three skill pools. Godot's younger C# tooling (debugger attach on device, on-device profiling, iOS export edge cases) costs days per quarter we do not have. **Unity wins on total engineering days.**

**Licensing.** Unity 6 Pro is a known per-seat subscription with no per-install component. Unreal takes 5% of gross above $1M — and on iOS that is 5% of gross *after* Apple's 15–30%, stacked. Godot is MIT and free, a real advantage we are declining to take. **Godot wins, Unity acceptable, Unreal worst.**

**Readability at C&C camera angles, on a 11-inch tablet.** We render a near-orthographic 58° view of up to ~2,000 small, similar-looking objects that must be distinguishable by silhouette and colour — now at roughly **1180 × 834 points** on an 11" iPad and **852 × 393 points** on an iPhone, which is far less screen real estate than a 1080p monitor and makes the readability problem harder, not easier (§12.3, §12.9). This wants excellent GPU instancing under Metal, cheap stylised lighting with a single directional light, a strong decal system, and total control over silhouette passes. URP's Mobile renderer with `RenderMeshIndirect` on Metal gives all of it, and Unity's shader variant stripping keeps the Metal pipeline-state count from exploding. Godot 4's Metal backend is newer and its instancing throughput is behind URP's. Unreal's strengths are precisely the features we disable. **Unity wins.**

**The summary judgement:** on PC-first this was Unity by a comfortable margin over Godot. On iOS-first it is Unity by a margin we would be reckless to ignore: Godot's iOS C# AOT path plus the absence of Burst plus community-plugin Game Center/StoreKit is, conservatively, six to nine engineer-months of platform work that produces no gameplay. Unreal fails on determinism first and on binary size, thermals and royalty stacking second. Unity's price is discipline about *not using* most of it: no `MonoBehaviour` in the sim, no `Physics`, no `NavMesh`, no `Animator` driving gameplay, no `Time.deltaTime` behind the sim boundary. Enforced by assembly definitions and a CI analyser (§16.1).

**A custom engine was considered and rejected in an hour.** Nine months of asset pipeline, renderer, audio integration, Metal backend, Xcode packaging and App Store plumbing, in exchange for determinism we get anyway. Custom engines are correct for studios whose product *is* the technology. Ours is a game about jamming.

### 1.3 Versions, platforms and third-party

**Shipping platforms:** iOS 17+ (arm64, Metal 3), iPadOS 17+, Windows 10+ x64 (Steam). **Development platforms:** macOS on Apple Silicon (primary workstation — it is the only host that builds iOS *and* runs the arm64 determinism corpus natively), Windows x64. **CI-only:** Linux x64 headless for balance sims.

- Unity 6000.0 LTS, pinned. One engine upgrade window is budgeted, at M4 (§17), and none after.
- URP 17 (Mobile renderer path), Burst 1.8, Collections 2.5, Mathematics 1.3.
- **FMOD Studio** for audio (§13) — iOS bank support, AAC/Vorbis per platform, and a mobile-friendly voice budget.
- **LiteNetLib** for UDP transport behind an `ITransport` interface, with our own relay (§11.8). Steam Datagram Relay is unavailable to iOS clients, which is why the relay is ours.
- **Steamworks.NET** on PC only, behind an `IPlatformServices` interface whose iOS implementation is **GameKit** (Game Center identity, friends, invites, achievements) and **StoreKit 2**.
- **YamlDotNet** for content authoring, build-time only; shipping builds read a compiled binary blob (§3.3).
- Nothing else. Every dependency added after M2 needs a written justification, because every dependency is a determinism risk, an upgrade tax, and on iOS a binary-size line item.

---

## 2. Architecture overview

### 2.1 The simulation / presentation split

Two assemblies, one direction of data flow.

```
KZ.Sim      pure C#, no UnityEngine, fixed point, deterministic
   |  (read-only snapshot + event stream)
   v
KZ.View     UnityEngine, floats, interpolation, VFX, audio, UI
   |  (intents)
   v
KZ.Input -> Command -> [network] -> KZ.Sim
```

`KZ.Sim` exposes:
- `Step(CommandBuffer turnCommands)` — advances exactly one tick.
- `ReadOnlySimState State` — struct-of-arrays views over the live state, valid until the next `Step`.
- `EventRing Events` — an append-only ring of typed sim events consumed by the view (unit died, link went black, tether cut, kill verified, node cut off). Ring capacity 4,096 events; overflow is a fatal assert, not a silent drop.

The view never mutates sim state and never asks the sim a question that costs a query — everything the UI needs is either in the state arrays or in the event ring. The only exception is the **exposure preview** (§9.3), which is an explicit, budgeted, read-only sim query invoked at most 10 times per second from the drag handler.

Why this hard split: it is what makes headless balance simulation (§16.3), 40× replay fast-forward (§11.5), deterministic AI (§10) and a stable modding surface (§15) all fall out of one decision instead of four.

### 2.2 Entity storage: not an ECS, an archetype-lite SoA table

We use a hand-rolled **struct-of-arrays entity table**, not a general ECS and specifically not Unity Entities.

The reasoning: a general ECS earns its complexity when you don't know your component composition ahead of time. We do. This game has roughly 30 component kinds and about 14 stable archetypes (ground vehicle, infantry squad, rotary drone, fixed-wing drone, structure, tether, projectile, salvage pile, decoy, pin, node, sensor, effect, munition). A dense SoA table with a fixed component set, a free list, and generation-stamped handles is ~600 lines, is trivially serialisable for snapshots, is trivially hashable for desync detection, and has no version-churn risk. Unity Entities would give us better parallel scheduling and cost us a rewrite every time the package changes, plus a determinism surface (structural change ordering, chunk iteration order) we would have to police anyway.

```
Entity handle:  uint32  = [ index : 20 ][ generation : 12 ]
Hard caps (Standard ruleset):  4,096 simulation entities (units + structures)
                               8,192 ephemeral (projectiles, salvage, decoys, pins, effects)
                                 512 tethers
                                  64 supply nodes per team
Hard caps (Compact ruleset):   1,536 / 3,072 / 192 / 48
```

**Entity caps are a match rule, not a device setting.** In lockstep every peer must simulate the same world, so a cap cannot vary by device at runtime — a cap that differed would be a desync, not a degradation. Instead there are two **rulesets**, `Standard` and `Compact`, which are part of the content hash and are negotiated at lobby time from the *lowest device tier present* (§12.7). Compact is 1v1-only, uses the small map pool (≤ 2,600 × 2,200 m) and caps at 1,536 entities — comfortably above the 700–1,100 a real 1v1 peaks at, so it is not a gameplay compromise, only a ceiling on absurdity. A 3 GB iPhone 11 and a desktop PC can therefore cross-play, at Compact. 2v2 requires every peer to be Tier B or better.

Component arrays are `NativeArray<T>` so Burst jobs can take them directly. Presence is a `BitSet512`-per-component over the index space. Iteration is over dense index lists rebuilt when membership changes, not over the whole table — a "who has `Link` and `Airborne`" list is maintained incrementally.

**Memory budget for full sim state: 6.0 MB (Standard) / 2.6 MB (Compact).** Breakdown at Standard: entity components 2.4 MB, vision grids 1.4 MB, pathfinding grids 1.1 MB, tethers 0.4 MB, jam/threat grids 0.1 MB, orders and queues 0.3 MB, misc 0.3 MB. This number matters three times over: it is what a reconnecting player downloads (§11.7), what a snapshot ring costs, and — on iOS — what we can afford to keep resident. The snapshot ring is **8 snapshots on PC (48 MB), 3 on iOS (18 MB / 7.8 MB Compact)**, because iOS resident memory is the scarce resource (§12.10) and scrubbing a replay is a desktop workflow.

### 2.3 Threading model

Three threads plus a job pool, sized for an A-series SoC's asymmetric cores as much as for a desktop.

1. **Sim thread.** Owns `KZ.Sim`. Runs `Step()` on its own cadence, decoupled from render. Dispatches Burst jobs for wide work (vision stamping, flow-field integration, jam rasterisation, mesh BFS, damage area queries) and blocks on their completion inside the tick. Job scheduling order is fixed and data-dependency-driven; job *completion* order never affects results because no two jobs write the same cell (we use per-team or per-index partitioning, never atomics that can race on order). Pinned to a performance core via Unity's job worker configuration; on iOS we cap `JobsUtility.JobWorkerCount` at **`min(performanceCores − 1, 3)`** so the sim never schedules onto efficiency cores, where a job that should take 0.3 ms takes 1.4 ms and blows the tick.
2. **Main/render thread.** Unity's. Reads the most recent completed sim snapshot, interpolates, submits draws to Metal.
3. **Network thread.** Socket I/O, packet assembly, command turn assembly. Hands complete turns to the sim thread through a lock-free SPSC queue. On iOS it also owns the background-task assertion that keeps the socket alive across a short app suspension (§11.6).
4. **Job pool.** Unity's worker threads, used only by Burst jobs the sim thread explicitly schedules and completes within the same tick. No fire-and-forget work crosses a tick boundary.

**Mobile consequence:** an A14 has two performance cores. With the render thread on one and the sim thread on the other, our effective parallel width inside a tick is roughly 1.6 cores, not 4. Every budget in this document assumes that, and the wide Burst jobs are written to be *useful at width 2* (partitioned by team, by grid band, by phase) rather than relying on an eight-way split that only exists on desktop.

**Determinism rule:** a job may be parallel if and only if its output is a pure function of its input and its write set is disjoint per work item. Reductions (e.g. "total supply drained") are done as per-partition partial sums combined in a fixed order on the sim thread. No `Interlocked` accumulate into a shared float or fixed value, ever.

### 2.4 Tick rate

**Simulation: 32 Hz. dt = 1/32 s = 0.03125 s exactly.**

32 Hz rather than 30 because `1/32` is exactly representable in binary fixed point (`0x0000_0000_8000_0000` in Q31.32) and `1/30` is not. A repeating-fraction dt introduces a per-tick rounding bias that accumulates over a 25-minute match into visible drift in unit positions — not a desync (both peers drift identically) but a tuning annoyance, because "speed 22 m/s" would not mean 22 m/s. Choosing a power of two makes every derived quantity clean: crew recovery 8 s = 256 ticks, amber-to-black 4 s = 128 ticks, salvage decay 2%/s = ×(1 − 1/1600) per tick, day/night cycle 6:00 = 11,520 ticks.

**Command turn: 4 ticks = 125 ms. Input delay: 2–6 turns = 250–750 ms, adaptive** (§11.2; the wider range exists because of cellular).

**Render: decoupled, frame-rate-capped per device tier** (30 / 60 / 120, §12.7). Presentation interpolates between the last two sim states with a fractional alpha; at 60 fps that is ~1.9 render frames per sim tick and at 30 fps it is ~0.94, so interpolation is mandatory in both directions — at 30 fps some frames advance the sim twice and must still present a smooth intermediate pose. Rotations interpolate in the *view's* float space from fixed-point sim angles; the sim's angle is authoritative, the interpolated one is decoration.

**The sim tick rate is identical on every platform and never changes, ever.** It is shared lockstep state. A thermally throttled iPhone drops *frames*, never ticks; if it cannot sustain 32 Hz of simulation it stalls the match and is reported as a slow peer (§11.2). This is the single most important consequence of mobile-primary lockstep: the thermal governor (§12.10) may touch resolution, frame cap, particles, shadows and tether LOD, and may touch nothing at all inside `KZ.Sim`.

Why not 20 Hz (cheaper, classic RTS, and tempting on mobile)? Because FPV drones move at 22–34 m/s and interception is a proximity test; at 20 Hz a 34 m/s interceptor moves 1.7 m per tick against a ~4 m engagement distance, and we would need continuous collision detection to compensate — which costs more than the ticks we saved. 32 Hz gives 1.06 m per tick. Why not 64 Hz? It doubles sim cost for no gameplay gain, halves our latency headroom, and on an A14 it is simply unaffordable.

### 2.5 Fixed point, not float

**All simulation arithmetic is `fix64` — `Int64` in Q31.32.** Range ±2.1×10⁹, precision 2.3×10⁻¹⁰. On a 4,000 m map that is absurd headroom; the precision is spent on intermediate products, not on world size.

The case is simple, and mobile-primary makes it overwhelming: we need deterministic lockstep **between an ARM64 iPhone and an x86-64 desktop**, across IL2CPP AOT and Mono JIT, across Burst versions, for 48,000 ticks without a single divergent bit. IEEE-754 gives you *most* of that and then loses it to FMA contraction (ARM64 fuses aggressively; x64 with AVX2 fuses differently), to compiler reassociation, and to transcendental implementations that differ by one ULP between Apple's `libm` and the Windows CRT. One ULP over 48,000 ticks with feedback loops (steering → position → steering) is a desync at minute nine. Cross-play on floats is not a thing we would be attempting carefully; it is a thing that does not work.

Integer fixed point makes the whole problem disappear, because a 64-bit integer multiply is exactly specified on every CPU that has ever shipped.

Implementation notes:
- Multiply needs the high half of a 64×64 product. **ARM64 provides it in one instruction (`smulh`); x86-64 provides it in one (`imul` r64, 128-bit result).** Burst emits both from a single intrinsic wrapper; the Mono/IL2CPP fallback is decomposed 32×32→64 partial products with explicit carry, which produces the identical value at ~4× the cost. Divide is a 128/64 long division. The sim is not multiply-bound on either architecture.
- `Sqrt` is a 32-iteration integer Newton-Raphson with a fixed iteration count (not convergence-terminated — convergence termination is a determinism hazard if the initial estimate differs).
- **Angles are `bam16`** — a 16-bit binary angle, 65,536 units per turn. `Sin`/`Cos` are a 4,096-entry Q31.32 table with linear interpolation between entries; max error 1.8×10⁻⁷, deterministic by construction. `Atan2` is a 1,024-entry table plus one Newton step.
- Random numbers are `xoshiro256**` seeded per-*stream*, never a global RNG. Streams: `Interception`, `AutonomyClassify`, `TetherSnag`, `AIJitter`, `CosmeticOnly`. The first four are in the sim and are hashed; `CosmeticOnly` lives in the view and is explicitly excluded from hashes so that particle variation cannot desync a match.
- Floats are legal in exactly three places: the view, the audio system, and telemetry export. A Roslyn analyser fails the build on `float`/`double`/`System.Math`/`UnityEngine.Mathf` inside `KZ.Sim`.

**The cost:** roughly 1.6× the arithmetic cost of float for the same operation count on x64, and about **1.4× on ARM64** (Apple's integer units are relatively stronger than their vector units for this workload, and `smulh` is cheap), plus a week of engineer time writing and testing the math library at M0. Both are trivial next to a cross-play mode that does not work.

---

## 3. Simulation core

### 3.1 The grid ladder

Four aligned grids, each a power-of-two multiple of the last, all origin-aligned at the map's south-west corner. Aligning them means any coordinate converts between grids with a shift, and any query at a coarse level maps to an exact 2×2 or 4×4 block at the finer one.

| Grid | Cell | 3,000 × 2,400 m map | Contents |
|---|---|---|---|
| **Build / path** | 8 m | 375 × 300 = 112,500 | terrain type, passability, road flag, occupancy, height class, snag class |
| **Vision** | 16 m | 188 × 150 = 28,200 | per team: visibility state, last-seen tick |
| **Signal** | 32 m | 94 × 75 = 7,050 | max effective jam, contributing emitter ids |
| **Threat** | 32 m | 94 × 75 = 7,050 | per team: threat intensity, source mask |

Map size cap: **4,096 × 4,096 m** (2v2 ceiling is 4,000 × 3,200 per the gameplay spec). That fixes the build grid at ≤ 512 × 512 = 262,144 cells, which is the number every buffer is sized against.

### 3.2 Entity model

Every unit and structure is one entity. A **squadron is not an entity** — the gameplay spec's flight cards launch N individual drones which are individually selectable, and the futures spec's "3–6 drones, one icon" is a *selection and UI* behaviour (§12.4), not a sim aggregation. Aggregating would break per-drone tether, per-drone link state and per-drone interception, which are the three things the game is about.

Core components (a representative subset; 31 total):

```
Transform    { fix2 pos; bam16 yaw; fix2 vel; byte layer }        // layer: Ground | Low | High
Health       { fix hp, hpMax; byte armourClass; fix cageHp }
UnitDef      { ushort defId; byte team; byte rank; byte stance }
Link         { see §4.1 }
Sortie       { ushort crewId; byte phase; int32 orderId; uint targetHandle }
Sensor       { fix footprint; byte kind; byte quality; uint lastStampTick }
Signature    { byte base; byte current; uint firingUntilTick; byte flags }
Mover        { byte radiusClass; fix speed, turnRate; ushort flowFieldId; fix2 steer }
Supply       { ushort nodeId; uint sustainedUntilTick; byte state }
Weapon       { ushort defId; uint nextFireTick; fix range; uint cueSourceHandle }
Tethered     { ushort tetherId }
Autonomy     { byte q; fix2 boxMin, boxMax; uint boxExpiryTick }
Producer     { byte tab; ushort queue[9]; uint completeTick; byte hangar[8] }
```

**Layers.** Three, not four (the futures spec asks for two visual bands; we need three in sim). `Ground` collides and pathfinds. `Low` (drones, 0–120 m altitude) ignores ground collision, is hit by Gun Mounts, IFVs and Low-stance interceptors, and is blocked by Net Tunnels from above. `High` (Recon Wings, Motherships, Loitering Munitions in cruise, Mid-Range Strikers) is reachable only by High-stance interceptors and Interceptor Batteries, and ignores all ground LOS occlusion. Altitude is not simulated as a continuous value; it is this enum plus a per-entity render offset. This is the single biggest fidelity cut in the sim and it is correct: continuous altitude would demand 3D pathfinding, 3D LOS and an altitude UI, and it buys nothing the layer enum doesn't.

### 3.3 Content as data

**Authoring format: YAML. Shipping format: a compiled binary blob (`content.kzb`) with a 64-bit content hash that is part of the lockstep handshake.** YAML is parsed only by the build step and by the editor's hot-reload path. This gives modders a diffable, commentable, hand-editable format and gives the shipping game a 4 ms load and a hash that makes "your mod differs from mine" a lobby error instead of a desync at minute six.

Every def is validated at compile against a JSON Schema (we author in YAML but validate with JSON Schema because the tooling is better) plus a set of semantic rules: referenced ids exist, costs are positive, armour class is in the enum, a unit with `link.kind: fiber` has a `spool` block, a unit consuming a crew has a `sortie` block.

#### Unit schema

```yaml
# content/units/fiber_fpv.yaml
id: unit.fiber_fpv
schema: unit/2
display:
  name: "Fiber FPV Team"
  icon: ui/icons/fiber_fpv
  silhouette: sil.quad_heavy          # §12.3 silhouette family
  source_glyph: struct.spool_plant    # sidebar corner glyph
faction: both                         # both | kestrel | obsidian
tier: 2
tab: AIR
built_at: [struct.spool_plant]
hangar: true                          # goes to hangar stock, not onto the map
cost:
  mat: 420
  build_ticks: 384                    # 12.0 s
  crew: 1                             # pilot slots consumed while airborne
  supply_per_sortie: 2
body:
  hp: 70
  armour: Ar                          # Sf | Lt | Hv | St | Ar | Af
  top_facet: false
  layer: Low
  radius_class: air_small             # §3.6
move:
  speed: 15.0                         # m/s
  accel: 22.0
  turn_rate: 180                      # deg/s
  road_bonus: false
link:
  kind: fiber
  robustness: 255                     # 255 == unjammable sentinel
  spool:
    length: 1400.0                    # m; +600 with upgrade.spool_extension
    node_spacing: 12.0
    taut_grace_ticks: 96              # 3.0 s at leash before cut
    speed_penalty: 0.0                # already baked into move.speed
  black_policy: abort                 # abort | last_mile | dual_link (upgradeable)
sensor:
  footprint: 220.0
  kind: optical
  quality: 55
signature:
  base: 18
  flags: [rotary]
weapons:
  - id: wpn.shaped_340
    damage: 340
    type: Shp
    range: 8.0                        # contact
    one_way: true                     # the unit is the munition
abilities:
  - ability.thread_the_door
ai:
  role: anti_armour
  utility_tags: [kills_heavy, ignores_jam, slow, snag_risk]
  preferred_targets: [Hv, St, Lt]
audio:
  loop: sfx/drone/fiber_loop
  death: sfx/drone/quad_death
telemetry_group: strike_fiber
```

#### Structure schema

```yaml
# content/structures/ew_post.yaml
id: struct.ew_post
schema: structure/2
display:
  name: "EW Post"
  icon: ui/icons/ew_post
faction: both
tier: 2
tab: BUILD
cost:
  mat: 750
  build_ticks: 768                    # 24.0 s
  power: -70                          # negative = draws
prereq: [struct.radar_mast]
footprint: { w: 3, h: 3 }             # build tiles (8 m)
placement:
  needs_flat: true
  build_radius_source: false          # does not itself extend the 240 m radius
  anywhere: false                     # true for forward_node, net_trap, decoy
body:
  hp: 1100
  armour: St
  tall: true                          # visible from 1800 m regardless of terrain
emits:
  jam:
    strength: 70
    radius: 450.0
    falloff: linear_1p3               # J*(1-d/r)*1.3, see §4.2
    toggleable: false
  signature_while_emitting: 85
  rf_visible: true                    # seen by enemy Radar Mast through shroud
provides: []
on_death:
  explosion: { damage: 0, radius: 0 }
  salvage_fraction: 0.35
ai:
  strategic_value: 8                  # 0-10, drives AI target priority
  utility_tags: [denies_radio, self_revealing]
```

#### Ability schema

```yaml
# content/abilities/terminal_commit.yaml
id: ability.terminal_commit
schema: ability/1
display: { name: "Terminal Commit", hotkey: T, icon: ui/icons/commit }
target: self
cost: { mat: 0, tp: 0, supply: 0 }
cooldown_ticks: 0
requires:
  - order_phase: [Engaged]
  - link_pip: [green, amber]
  - supply_state: [sustained]         # Dry units cannot use abilities
duration_ticks: 0                     # instant, permanent for this sortie
effects:
  - set_flag: { flag: terminal_commit, value: true }
  - modify_stat: { stat: move.speed, mul: 1.25 }
  - set_flag: { flag: ignores_new_orders, value: true }
  - set_flag: { flag: net_trap_immune_within, value: 40.0 }
ui:
  cursor: cursor/commit
  confirm_bark: bark.commit
```

#### Link type schema

Link kinds are data, not code, so the futures spec's Tier 4 ("Release Authority") and Era IV ("Relay Grid") can add rungs without a sim change. The resolution algorithm (§4.6) is generic over this table.

```yaml
# content/links/links.yaml
schema: linktable/2
kinds:
  - id: radio
    robustness: 40
    topology: anchored               # anchored | mesh | tethered | overhead | none
    anchors: [struct.crew_quarters, struct.relay_mast, unit.mothership]
    range_per_hop: 1000.0
    max_hops: 1
    acquisition_penalty_ticks: 0
    capacity_pool: null
  - id: mesh
    robustness: 65
    robustness_per_alt_hop: 5
    robustness_alt_cap: 15
    topology: mesh
    anchors: [struct.crew_quarters, struct.relay_mast, unit.mothership]
    relay_tag: mesh_repeater         # any entity with this tag is a hop
    range_per_hop: 700.0
    max_hops: 4
    acquisition_penalty_ticks: 13    # 0.4 s per hop
    drop_children_on_parent_loss: true
  - id: fiber
    robustness: 255
    topology: tethered
    capacity_pool: null
  - id: satellite
    robustness: 95
    topology: overhead
    anchors: [struct.uplink_terminal]
    range_per_hop: 0                 # unlimited under coverage
    capacity_pool: uplink            # scarce; 6 per terminal
  - id: autonomy
    robustness: null                 # nothing to jam
    topology: none
    consumes_crew: false
    classifier: { q_base: 55, cone: 120.0, decoy_q_penalty: 5 }
# Tier-4 reserve slot (spec-futures §6.1). Populated by the era pack, not the base game.
reserved_slots: [t4_release_authority]
```

#### Era modifiers

The futures spec's Eras (§6.2) are implemented as a **modifier stack** applied at content-compile time, not at runtime. An era pack is a YAML file of typed patches:

```yaml
# content/eras/era3_magazine_war.yaml
id: era.magazine_war
display: "III — The Magazine War"
stacks_with: [era.loop_closes, era.relay_grid]
patches:
  - select: "units[tab == AIR].cost.mat"          mul: 0.60
  - select: "units[tab == AIR].cost.build_ticks"  mul: 0.50
  - select: "units[role == interceptor].weapons[*].range"  mul: 2.0
  - select: "structures[id == struct.gun_mount].emits.coverage" mul: 2.0
  - enable_rule: rule.deep_strike_damages_build_rate
```

Compile-time patching means eras cost zero runtime branches and are covered by the content hash, so an era mismatch is a lobby error. Ranked play at early access ships Eras I + II only (per futures §7.4); the rest are lobby options.

### 3.4 The order system

Orders are the only way anything happens in the sim. Player input, AI decisions and campaign triggers all produce the same `Command` structs, which is what makes replays, AI-vs-AI headless matches and desync bisection all work off one mechanism.

```
Command  { byte team; byte kind; uint tick; uint32 selection[]; fix2 point;
           uint32 targetHandle; ushort param; byte modifiers }
```

Command kinds (24 total): `Move`, `AttackMove`, `Attack`, `Stop`, `Hold`, `Patrol`, `SetStance`, `LaunchSortie`, `Ability`, `Build`, `Place`, `Cancel`, `Rally`, `DropPin`, `ClearPin`, `AssignCrew`, `ReserveCrew`, `RecallFlight`, `SetAutonomyBox`, `Super`, `Surrender`, `Ping`, `Chat`, `Doctrine`.

`LaunchSortie` is the interesting one and carries a **flight card id plus a count plus a target**, not a list of entity handles — because the airframes don't exist as entities until the command resolves, and crew assignment must happen inside the sim so both peers pick the same crews. Crew selection is deterministic: highest readiness (Ready > Fatigued), then highest rank, then lowest crew id. Manual drag-assignment (gameplay §6.2) sets an explicit crew id in `param`.

**Per-entity order state** is a small stack, depth 4: a current order, an interrupt (e.g. auto-acquire a target of opportunity while moving), and a queued order from shift-click. Deeper queueing is deliberately not supported; C&C players shift-queue two or three orders, not ten, and a depth-4 stack keeps the per-entity order block at 48 bytes.

Orders resolve through a per-archetype `OrderExecutor` — a switch, not virtual dispatch, because the sim is a flat data pipeline and we want the branch predictable and Burst-friendly.

### 3.5 Pathfinding

**Ground: hierarchical flow fields on the 8 m build grid, with local steering. Air: direct-path with a thin obstacle test. No navmesh, no per-unit A*.**

Why flow fields: our worst case is 60–120 ground units moving to a handful of shared destinations (a wreck field, a Grid Tap, a Forward Node), which is exactly the case flow fields dominate — one solve amortised over many agents, and re-solving on terrain change (a burnt Net Tunnel, a destroyed building, a fresh minefield) is a single field rebuild rather than 120 invalidated paths. Navmesh would be cheaper per agent for a small number of agents on a mostly-static map; our map is not static (Net Tunnels burn, structures die, mines appear) and our agents are not few. Per-unit A* at this count on an 8 m grid is 3–5 ms per tick worst case, which we don't have.

**The stack, top to bottom:**

1. **Sector graph.** The build grid is partitioned into 64 × 64-cell **sectors** (512 m). Each sector precomputes its connected components and the portal edges to neighbours, per radius class. A coarse A* over portals (≤ 96 sectors on the largest map) gives a sector-level route in ~40 µs. This is how we avoid solving a full-map flow field for a destination on the other side of the world.
2. **Flow field cache.** A field is `(destinationCell, radiusClass, costProfile)`. It is computed by an Eikonal-style integration (Burst job, parallel over wavefront bands) but **only within the sectors on the coarse route, dilated by one sector**. On a typical cross-map move that's 9–14 sectors = 36,000–57,000 cells, ~0.9 ms in a Burst job, amortised over the units sharing it. Cache: LRU, each 8 bytes/cell (integration cost `fix32` + a packed 8-direction vector + flags). 32 live fields × 57,000 × 8 = 14 MB worst case; **we cap the cache at 12 MB / 32 fields on PC and 5 MB / 14 fields on iOS**, and evict. The mobile cap is affordable because Compact's smaller maps produce smaller fields and because 14 simultaneous distinct ground destinations is already more than a 1v1 generates.
3. **Cost profiles.** Three: `Combat` (default), `Logistics`, `Stealth`. This is where **road preference** lives, and it is not a hack — it is a per-profile multiplier table indexed by terrain class.

| Terrain | Combat | Logistics | Stealth |
|---|---|---|---|
| Road | 0.55 | **0.30** | 1.40 |
| Open | 1.00 | 1.00 | 1.00 |
| Forest | 1.45 | 2.20 | **0.70** |
| Dead ground | 1.20 | 1.80 | **0.65** |
| Rubble | 1.80 | 3.00 | 1.60 |
| Threat overlay (own team's) | +0 | **+0.9 × intensity** | +0.35 × intensity |

Supply Trucks, Logistics UGVs and Recovery UGVs use `Logistics`, which makes them hug roads (matching the gameplay spec's "trucks travel at full speed only on roads") *and* makes them route around the player's own Threat overlay stain — the truck avoids the road where trucks have been dying, without any new UI. Motorcycle Squads and Net Engineers default to `Stealth`. Everything else is `Combat`. The player can override per-order with a modifier key, which is the only routing UI in the game (anti-pillar: "no supply convoy routing UI").

4. **Local steering.** Per-tick, per-unit: sample the flow vector, blend with a separation force from the collision hash (§3.7), clamp to `turn_rate`, integrate. 1,200 agents × ~90 ns = 0.11 ms in a Burst job.
5. **Formations.** Slot-based, not physics-based. A move order on a multi-unit selection computes a formation shape (line, wedge, column — chosen automatically from unit mix and path width; column on roads, line in open, wedge for mixed armour) and assigns each unit a slot offset by a deterministic Hungarian-lite assignment (greedy nearest-slot with id tie-break, 2 passes). Units path to `destination + slotOffset` using the *same* flow field, so formations cost one field, not N. Arrival is "all slots within 1.5 × radius" or a 6-second timeout, whichever first.
6. **Air paths.** Air units do not use flow fields. A drone flies a straight line to its target, with two exceptions: (a) fiber drones run a **snag-aware route** (§4.4) — a 3-point Catmull spline biased away from forest and power-line cells, recomputed only at launch and on retarget; (b) Low-layer drones avoid Gun Mount and Interceptor Battery coverage discs when the order is `Move` rather than `Attack`, via a single tangent-avoidance waypoint. That is the entire air pathfinding system, and it is enough because air has no obstacles.

**Radius classes.** Five, because collision and passability tables are per-class and five is the most we can afford to precompute sector connectivity for.

| Class | Radius | Members |
|---|---|---|
| `inf` | 1.5 m | Net Engineer, Motorcycle Squad, Designator Team |
| `light` | 3.0 m | Recovery UGV, Logistics UGV, Supply Truck, EW Truck |
| `heavy` | 4.5 m | IFV, Main Tank, Interceptor Battery |
| `air_small` | 1.0 m | all rotary drones |
| `air_large` | 3.0 m | Recon Wing, Mothership, Night Bomber, Mid-Range Striker |

Air classes never consult the passability grid; they exist only for collision separation and for the visual footprint.

### 3.6 Collision

**Ground units: soft-body separation, no rigid physics.** A uniform spatial hash at 16 m cells holds ground entity indices. Each tick, each unit queries its 3×3 neighbourhood and accumulates a separation impulse proportional to overlap depth, capped at 40% of per-tick movement. Units can overlap transiently — this is a C&C-lineage game; units squeezing through a gap looks right and rigid resolution looks like a traffic jam. Structures and impassable terrain are *hard*: movement that would end inside one is clipped along the surface normal from the build grid.

**Air units: separation only, no blocking.** Drones pass through each other with a small visual separation impulse so a flight of six doesn't render as one drone.

**Weapon/entity intersection:**
- **Hitscan** for all direct-fire weapons with flight times under ~0.4 s: Gun Mount, IFV autocannon, Main Tank, Motorcycle Squad, turret-module UGVs. The sim resolves the hit at the fire tick; the view draws a tracer that arrives visually ~2 frames later. Deterministic, one segment-vs-hash query, and it makes "did it hit?" unambiguous, which the gameplay spec demands (no RNG on direct fire).
- **Simulated projectiles** for anything with a meaningful flight time or a proximity fuze: Interceptor Battery rounds (2,000 m at 120 m/s = up to 16 s — the tension is the point), Night Bomber drops, Loitering Munition terminal dives, Arclight munitions. These are ephemeral entities with position, velocity and a fuze, tested against the hash each tick.
- **The one-way drones are their own projectiles.** An FPV Team in terminal dive is a unit until the tick it intersects; there is no separate munition entity. This is why `weapons[].one_way: true` exists in the schema.

**Frag/area damage:** one radius query against the appropriate hash, linear falloff to zero at the stated radius, applied in ascending entity-index order so the death sequence — and therefore the salvage spawn order and the TP award order — is identical on both peers.

---

## 3A. Input model: touch first

Touch is the primary input. Mouse and keyboard are a secondary binding on PC (and on an iPad with a Magic Keyboard and trackpad, which we support because it is free once the PC binding exists). This section is placed here, immediately after the order system, because touch is fundamentally a **command-generation** problem, not a UI problem — and because the single most important architectural fact is that it does not change §3.4 at all.

### 3A.1 The invariant

**Every input path — touch, mouse, keyboard, AI, campaign trigger, replay — produces the same `Command` structs, and nothing else crosses into the sim.** There is no touch-specific sim code. A tap does not "do" anything; it resolves to a `Move`, an `Attack`, a `LaunchSortie`, exactly as a click does. This is what makes cross-play between a tapping iPad player and a clicking PC player fair, lockstep-safe and replayable from either side, and it is the reason we defined `Command` before we defined input.

It also constrains the assist layer (§3A.5): an assist may only ever choose *which legal command to emit*. It may never emit a command the player could not have issued, and it may never run inside the sim.

### 3A.2 The gesture vocabulary

Eight gestures, and a hard rule that we will not add a ninth.

| Gesture | Meaning | Notes |
|---|---|---|
| **Tap** on own unit / group | Select | Replaces click-select |
| **Tap** on ground or enemy, with a selection | Contextual order | Move / Attack / Enter / Capture, resolved by §3A.4 |
| **Drag** from empty ground | Marquee select | 12 pt movement threshold, 110 ms hold threshold, so a sloppy tap is still a tap |
| **Drag** from a selected unit | Order ribbon | Draws the path with the **exposure preview** (§9.3) live under the finger. This is the single best thing touch does for this game — the finger naturally dwells, which is exactly when the preview is useful. On mouse it needs a held button; on touch it is free. |
| **Press-and-hold** (350 ms) | Inspect | Replaces hover. Opens an info card, and on own units a **radial action menu** of at most 6 wedges (stances, abilities, stop, hold). Releasing on a wedge commits it; releasing elsewhere cancels. |
| **Double-tap** unit | Select all of type on screen | Replaces ctrl-click |
| **Two-finger drag** | Pan camera | Never conflicts with marquee |
| **Pinch** | Zoom | Clamped to the §12.1 range; iPhone's range is tighter (§12.9) |

Deliberately absent: swipe gestures, three-finger anything, rotation (the camera does not rotate, §12.1), long-press-drag combinations, and edge-swipes (they collide with iOS system gestures — Control Centre, the home indicator, and the iPadOS dock).

**iOS system gesture defence.** We declare `preferredScreenEdgesDeferringSystemGestures` on all edges so the first edge-swipe in a match shows the indicator rather than dismissing the app, and we keep no interactive UI within 20 pt of the bottom edge on iPhone (home indicator) or within the notch/Dynamic Island inset in landscape.

### 3A.3 The tap-tap sortie

The gameplay spec's core action — "one click, one sortie" — is already a two-step, target-last interaction, which is *the* pattern touch does well. On touch it becomes:

1. **Tap a flight card.** The card lifts, shows `readyCount` and a **recommended count badge** (§3A.5), and the world enters *targeting state*: valid targets get a subtle bracket, the jam bubbles that matter to this link type brighten, and the cursor-equivalent — a reticle that follows the finger during a drag — appears.
2. **Tap a target, or drag to place and release.** The `LaunchSortie` command is emitted on release.
3. **Tap the card again, or tap outside, to cancel.**

Two taps, no drag required, no precision required. A second tap on the card cycles the count (3 → 6 → 1 → 3), which is the one piece of quantity control the player needs and which on mouse is a scroll-wheel or a modifier.

The "no idle crews" failure state — which the gameplay spec calls the game's single clearest teaching moment — is stronger on touch than on mouse: the card does not lift, it gives a short lateral shake, the crew rack flashes, and a crossed-headset glyph appears on the card itself rather than on a cursor that touch does not have.

### 3A.4 Intent resolution: what a tap actually hits

At default zoom a drone renders 18–46 px, which on a 2× iPad is **9–23 points** — well under Apple's 44 pt minimum touch target and far under a fingertip's ~9 mm contact patch. Nearest-pixel picking would be unusable. So picking is a **scored resolve over an inflated radius**, not a raycast.

```
# PSEUDOCODE 4 — touch intent resolution (client-side, view layer, never in the sim)
# Runs on tap-up. Emits at most one Command. Deterministic-irrelevant: the OUTPUT
# command is what crosses into the sim, so any scoring change is a UX change, not a
# desync risk, and can differ between platforms and assist levels.

function ResolveTap(screenPt, selection, assistLevel):
    radiusPt = 44 + (assistLevel == High ? 14 : 0)          # 44 pt floor, Apple HIG
    world    = Unproject(screenPt)
    cands    = PickWithinRadius(screenPt, radiusPt)          # screen-space, from the
                                                             # instance buffer's picking IDs
    best = none; bestScore = -inf
    for c in cands:
        d      = ScreenDistance(screenPt, c) / radiusPt      # 0 at centre, 1 at edge
        s      = 0
        s += Lerp(1.0, 0.0, d) * 1.0                         # proximity
        s += IsEnemy(c) and SelectionCanAttack(c)  ? 0.9 : 0
        s += IsOwn(c)   and selection.isEmpty      ? 0.7 : 0
        s += StrategicValue(c) / 10.0 * 0.5                  # from the unit def; a Radar
                                                             # Mast beats the truck beside it
        s += IsDesignated(c)                       ? 0.25 : 0
        s -= IsFullHealthDecoyLike(c)              ? 0.20 : 0  # do not auto-target our own
                                                               # decoys or neutral props
        s -= c.layer == High and not SelectionCanHitHigh(c) ? 2.0 : 0   # never pick an
                                                               # unreachable target
        if s > bestScore: best = c; bestScore = s

    if best != none and bestScore > 0.55:
        return Command(Attack | Enter | Capture, target = best)   # kind from a 12-row table
                                                                   # of (selection class x target class)
    else:
        return Command(Move, point = SnapToWalkable(world, selection.radiusClass))
```

Three properties this buys:

- **A tap between a Main Tank and the Supply Truck parked beside it picks the tank**, because `StrategicValue` breaks the tie. On mouse the player would have clicked precisely; on touch they cannot, and picking the cheaper target would feel like a bug.
- **A tap that hits nothing meaningful becomes a move**, snapped to walkable ground for the selection's radius class, so units never refuse an order because the finger landed on a wall.
- **The scoring function is view-side.** It can be tuned, A/B-tested, and set differently per assist level without any sim consequence, because its only output is a command the player could have issued by hand.

### 3A.5 Touch assist

Touch RTS needs help. The rule that keeps it honest: **assists are input aids, they run in `KZ.View`, they only ever emit ordinary commands, and they are identical in their *effect ceiling* across platforms** — a PC player can enable exactly the same assists, and a maxed-out assist can never do something a fast human could not. Cross-play fairness depends on this and so does lockstep.

Three levels: **Off / Standard (default) / High.**

| Assist | Standard | High |
|---|---|---|
| **Pick radius** | 44 pt | 58 pt |
| **Auto-formation** | On — formation shape chosen from unit mix and path width (§3.5), already the sim default | On, plus automatic column-on-road |
| **Recommended sortie count** | Badge only: shows the count that, by the damage table, kills the tapped target (e.g. "4" on an uncaged tank) | Badge + the card pre-selects that count |
| **Retreat reusables** | Off | Emits a `Move` home for reusable drones (Multirole Quad, Night Bomber, Recon Wing) below 30% HP, at most once per unit per 20 s |
| **Stance suggest** | A one-tap suggestion chip appears when a Multirole Quad flight is in the wrong stance for what it is facing | Chip auto-commits after 1.5 s unless dismissed |
| **Idle-crew nudge** | Rack pulses after 8 s of a fully idle rack | Pulse + a single "3 crews idle" chip |
| **Order queue smoothing** | Taps within 220 ms of each other on the same selection coalesce into a queued order rather than replacing | Same |

The APM ceiling matters: the gameplay spec targets "≤ 80 APM to play competently", which is the number that makes this game viable on touch at all. Our internal target is **≤ 55 APM on touch at Standard assist for the same competence**, achieved almost entirely by the flight-card model (one sortie is one interaction, not a select-then-order-then-micro sequence) and by auto-formations. If M2 playtests show touch players needing materially more actions than mouse players to reach the same board state, the fix is *fewer, fatter commands* — e.g. a single `LaunchSortie` that carries a count — not more assist.

### 3A.6 Mouse and keyboard as a secondary binding

On PC (and iPad with a trackpad) the same commands are produced by: left-click select, left-drag marquee, right-click contextual order, right-drag order ribbon, hover for the inspect card, control-groups on number keys, `Tab` for Link View, `V` for the Threat overlay, and the classic sidebar hotkeys. Flight cards bind to `1`–`8`.

Two things exist only on mouse and are explicitly accepted as platform differences because neither affects the sim: **hover** (touch substitutes press-and-hold) and **the held-`Tab` Link View** (touch substitutes a toggle button in the dock, plus a two-finger tap). Two things exist only on touch: the radial action menu (mouse uses the sidebar and hotkeys) and pinch zoom.

Manual FPV piloting (gameplay §6.2, open question 3) is `WASD` on keyboard and a **virtual left-stick plus tap-to-commit** on touch, in a picture-in-picture panel. It is optional on both, and the gameplay spec's +15% speed/evasion bonus is unchanged — we will measure whether it is harder or easier on touch at M2 and tune that single number, not the mechanic.

---

## 4. The control-link simulation

This is the novel subsystem, the one that justifies the project, and the one most likely to be the reason we miss a milestone. It gets the most engineering care and the tightest budget.

**Cost target: 0.55 ms per tick worst case on an A14 iPad at sustained (throttled) clocks, with 700 linked entities, 48 emitters, 160 live tethers, and a 320-node Obsidian mesh. 0.45 ms on a desktop min spec at Standard caps.** The mobile number is the binding one; every optimisation below was chosen because it survives two performance cores and a thermal ceiling.

### 4.1 Link state

```
struct Link {                          // 24 bytes
  byte   kind;        // Radio | Mesh | Fiber | Satellite | Autonomy | None
  byte   rBase;       // from the link table
  byte   rEffective;  // rBase + veterancy(+8 at Rank 3) + upgrades + meshAltBonus
  byte   jSampled;    // last evaluated J_eff at this position
  byte   pip;         // Green | Amber | Black
  byte   hops;        // mesh depth
  byte   altHops;     // count of viable alternate parents, capped at 3
  byte   policy;      // Abort | LastMile | DualLink
  ushort amberTicks;  // black at 128 (4.0 s)
  ushort blackTicks;  // crash at 384 (12.0 s) under Abort
  uint   parent;      // entity handle of controlling node, or 0
  uint   lastEvalTick;
  ushort tetherId;    // 0xFFFF if none
  byte   lodTier;     // A | B | C
  byte   flags;       // Reparenting | Taut | CapacityDenied | ...
}
```

`rEffective` is recomputed only on veterancy change, upgrade purchase, stance change or mesh topology change — not per tick.

### 4.2 Jamming fields

**Representation: a 32 m signal grid, rebuilt at 8 Hz, plus exact analytic refinement at the boundary.**

```
struct SignalCell {                    // 6 bytes -> 7,050 cells = 42 KB
  byte jCentre;        // max J_eff over contributing emitters, at cell centre
  byte emitterCount;   // 0..4 (5th+ emitter in a cell is dropped; see below)
  byte e0, e1, e2, e3; // indices into the live emitter table
}
```

Rebuild (every 8 ticks = 250 ms, Burst job, split by emitter): for each live emitter, walk its bounding disc (radius `r` in cells, ≤ 29 cells for a BLACKOUT dome) and for each cell compute `J_eff = J × (1 − d/r) × 1.3`, clamped to [0, 255]. `jCentre = max(jCentre, J_eff)`; append the emitter index to the cell's list.

**Jamming does not stack.** Overlapping bubbles apply the maximum single `J_eff`, never a sum. This is a deliberate design call not present in the gameplay spec and it must be honoured: stacking would make two EW Trucks equal to an EW Post in a way that is invisible on screen, and the gameplay spec's own display rule ("jam bubbles are terrain") only reads if a bubble's edge is where it looks like it is. Dropping the 5th+ emitter per cell is safe for the same reason — only the maximum matters, and we insert in descending `J_eff` order.

Cost: 48 emitters × mean 620 cells = 30k cell-writes per rebuild, at 4 Hz effective (staggered halves) ≈ 60k writes/s. Negligible.

**The boundary problem and its fix.** A 32 m cell is coarse: across one cell, `J_eff` for an EW Post (J 70, r 450) changes by `70 × (32/450) × 1.3 = 6.5`. A drone with R 55 sitting near the threshold would flicker green/amber as it crossed cells. So:

- If `|cell.jCentre − link.rEffective| ≥ 12`, the cell value is authoritative. (About 94% of queries.)
- Otherwise the unit evaluates exactly against the ≤ 4 emitters listed in the cell: real distance, real falloff. ~35 ns per emitter.

This makes the common case a single byte read and the edge case exact, which is precisely where the exactness matters: the gameplay spec's skirt-the-edge play ("a bubble bites hardest at the centre and frays at the edge — you can skirt it") is *only* fun if the edge is a real, stable, learnable line.

### 4.3 Relay chains and mesh graph maintenance

Anchors per team: Crew Quarters, Relay Masts, Motherships, and (for satellite) Uplink Terminals. Obsidian's faction rule — every air unit is a mesh repeater — means the mesh graph can contain 300–450 nodes late game.

**Maintenance: full multi-source BFS every 4 ticks (8 Hz), Burst job, deterministic.**

- Neighbour candidates come from a dedicated **mesh spatial hash** at 350 m cells (hop range is 700 m, so a 5×5 cell query covers it). Candidates per node capped at **24**, selected by ascending squared distance with entity-id tie-break.
- BFS from all anchors simultaneously, depth-limited to `max_hops` (4). Parent selection: minimum hop count, then minimum distance-to-parent, then lowest parent entity id. Fully ordered, therefore identical on both peers.
- `altHops` = the number of *other* viable parents at the same or lower hop depth, capped at 3, feeding the gameplay spec's `+5 R per alternate hop, max +15`.
- `acquisition_penalty_ticks` accumulates as `13 × hops` (0.4 s/hop) and is applied to the unit's weapon/order acquisition timer.

**Drop on loss.** Two mechanisms, deliberately different:

1. **Soft re-parent (the normal case).** A Relay Mast dies. Its children set `flags |= Reparenting` and keep their current pip for up to one BFS period (≤ 125 ms). The next BFS either re-parents them (pip unchanged, a short amber flicker in the view) or finds nothing, at which point they go amber immediately and follow the normal amber→black timer. The 125 ms grace is what stops a mesh army from strobing every time a relay dies at the edge of the chain.
2. **Hard drop (the Mothership).** The gameplay spec is explicit: "kill it and every child link drops at once" — that is the Mothership's entire strategic identity. So Motherships maintain an explicit child list, and their death event sets every child straight to `pip = Black` on the same tick, bypassing the amber timer and the BFS. One special case, one line of code, one very large gameplay moment.

**Cost:** 450 nodes × 24 candidate tests × 8 Hz = 86k distance tests/s, plus a BFS over ≤ 450 nodes. Under 0.05 ms/tick amortised.

### 4.4 Fiber tethers

The tether is three things at once and each costs differently: a **length budget**, a **snag geometry**, and a **visible, traceable, collidable object**.

```
struct Tether {                                  // 1,096 bytes; 512 max = 561 KB
  uint  droneHandle, anchorHandle;
  fix2  anchorPos;
  fix   spooled;          // sum of segment lengths
  fix   spoolMax;         // 1400.0, or 2000.0 with upgrade
  ushort nodeCount;       // <= 64
  TetherNode nodes[64];   // { fix2 pos; byte tileClass; byte snagAccum; ushort tick }
  uint  tautSinceTick;    // 0 if not taut
  uint  cutTick;          // set on cut; the polyline lingers 30 s (960 ticks)
  byte  state;            // Live | Taut | Cut | Lingering
  ushort roundRobinCursor;
}
```

**Node emission.** A node is appended when the drone has travelled 12 m from the last node **or** turned more than 8° since it. Between appends we decimate: if node *n−1* lies within 0.5 m of the chord from *n−2* to *n*, it is removed and its length folded in. In practice a 1,400 m flight settles at 38–52 nodes; the 64 cap is hit only by a drone that spirals, and on overflow we force-decimate to a 1.5 m tolerance. Node spacing is the single tuning knob that trades snag fidelity against memory; 12 m was chosen because it is 1.5 build tiles, so a node's `tileClass` lookup is a single grid read and a segment never spans more than two tile classes.

**Spool accounting.** `spooled` is maintained incrementally (add the new segment on append, adjust the live last segment each tick). When `spooled ≥ spoolMax` the tether goes `Taut`: the drone's velocity is projected onto the circle tangent — it can fly sideways and inward but not outward, which reads on screen as the drone being on a leash, which it is. After `taut_grace_ticks` (96 = 3.0 s) continuously taut, the tether **cuts**. We considered hard-stopping at the leash forever; cutting is better because it punishes over-extension instead of merely preventing it, and because a player who wants the leash extended has a 300 TP answer.

**Snag.** Per the futures spec's explicit warning (§6.4.2), we do not simulate the tether against individual trees. Snag is a per-second hazard rate carried by the *tile class* each segment crosses:

| Tile class | Snag rate |
|---|---|
| Open / field | 0.0 %/s |
| Road with traffic | 1.5 %/s |
| Forest | 4.0 %/s |
| Power-line corridor | 9.0 %/s |
| Rubble / urban | 3.0 %/s |

Evaluating all 50 segments of all 200 tethers every tick would be 10,000 rolls/tick. Instead, **per tick each tether evaluates exactly two segments**: the newest (which is where the drone actually is, so it responds instantly to flying into a forest) and one older segment chosen by a round-robin cursor. A 50-node tether is therefore fully swept every 50 ticks (1.6 s), which is well inside the timescale of a snag being a meaningful event. Hazard for a segment this evaluation: `p = rate × (segLen / 12.0) × (ticksSinceLastEval / 32)`, drawn from the `TetherSnag` PRNG stream keyed by `(tetherId, segmentIndex)`. Cost: 400 rolls/tick = 12.8k/s. Nothing.

**The tether as a collidable.** Segments are registered in a **tether hash** at 64 m cells (segment index lists, rebuilt incrementally on node append and on cut). Three consumers:

1. **Thread-finding.** Any enemy ground unit within 24 m of a live segment for 32 continuous ticks (1 s) raises a `TetherFound` event: the finder's team gets a directional arrow toward `anchorPos` and a 20 s reveal of a 90 m disc around the anchor. This is the gameplay spec's "physically traceable" rule, and it is the reason fiber is not free.
2. **Thermite and Net Traps.** A Night Bomber Dragon Run or a thermite drop cuts every live segment crossing its strip. Net Tunnels do not cut tethers (they block attack from above, not cable).
3. **Rendering.** §12.5.

**Lingering.** On cut or drone death the polyline persists for 960 ticks (30 s) in state `Lingering`: still visible, still findable (a dead man's thread still points home), but no longer evaluated for snag. This is sim state, not view state, because thread-finding is a gameplay event.

### 4.5 Satellite uplink

The simplest link and the most load-bearing for Kestrel's identity. Satellite has no graph and no geometry — coverage is global. It has a **capacity ledger**:

```
struct UplinkPool { byte capacity; byte inUse; uint32 holders[16]; }
```

Capacity = 6 per Uplink Terminal, +3 per `Uplink Priority` purchase. Allocation happens at sortie launch, in command order, and is released on landing or death. If capacity is exhausted the flight card greys out with the same visual language as "no crews", because it is the same kind of failure. A satellite unit is jammed only when `J_eff ≥ 95` at its position — in practice only by a Kestrel-model Uplink Jammer (J 100, r 300) or by BLACKOUT (J 140, r 900). Because that test is a single comparison against the cell value, satellite units sit permanently in LOD tier C (§4.7) unless within 600 m of a J ≥ 95 emitter.

### 4.6 Per-tick link resolution

This is the core loop. It runs as a Burst job over the LOD-selected subset, writing only into each entity's own `Link` component (disjoint write set, therefore parallel-safe and order-independent).

```
# PSEUDOCODE 1 — link resolution for one entity, one tick
# Runs on: LOD tier A every tick; tier B every 4 ticks; tier C every 16 ticks.
# Writes only entity-local state. No allocation. No branches on float compare.

function ResolveLink(e, tick, dtTicks):

    L = link[e]
    if L.kind == Autonomy or L.kind == None:
        L.pip = Green                      # nothing to jam; see ResolveAutonomy
        return

    # ---- 1. Topology: does a control path exist at all? ----
    if L.kind == Fiber:
        t = tethers[L.tetherId]
        connected = (t.state == Live)
    elif L.kind == Satellite:
        connected = uplinkPool[team[e]].Holds(e)
    elif L.kind == Mesh:
        connected = (L.parent != 0) and Alive(L.parent) and L.hops <= meshMaxHops
        if not connected and (L.flags & Reparenting) and (tick - L.lastEvalTick) <= 4:
            connected = true               # 125 ms BFS grace, §4.3
    else:  # Radio
        connected = (L.parent != 0) and Alive(L.parent)
                    and Dist2(pos[e], pos[L.parent]) <= radioRange2

    # ---- 2. Field: how hard is it being jammed here? ----
    if L.kind == Fiber:
        J = 0                              # unjammable by definition
    else:
        cell = signalGrid[CellOf(pos[e])]
        J = cell.jCentre
        if Abs(J - L.rEffective) < 12:      # boundary refine, §4.2
            J = 0
            for i in 0 .. cell.emitterCount-1:
                em = emitters[cell.e[i]]
                d  = Dist(pos[e], em.pos)
                if d < em.radius:
                    J = Max(J, (em.strength * (ONE - d/em.radius) * 13) / 10)
    L.jSampled = J

    # ---- 3. Pip state machine ----
    jammed = (J > L.rEffective)
    if not connected or jammed:
        L.amberTicks += dtTicks
        if L.pip == Green:
            L.pip = Amber
            Emit(LinkAmber, e)             # view: pip colour, one-shot audio
        if L.amberTicks >= AMBER_TO_BLACK:  # 128 ticks = 4.0 s
            if L.pip != Black:
                L.pip = Black
                L.blackTicks = 0
                Emit(LinkBlack, e)
    else:
        if L.pip != Green:
            Emit(LinkRestored, e)
        L.pip = Green
        L.amberTicks = 0
        L.blackTicks = 0

    # ---- 4. Consequences ----
    if L.pip == Amber:
        mover[e].speedMul     = FIX(0.70)   # -30%
        combat[e].acqMul      = FIX(2.00)   # x2 acquisition
        order[e].acceptsNew   = false       # keeps current order, refuses new
    elif L.pip == Black:
        L.blackTicks += dtTicks
        switch L.policy:
            case Abort:                     # orbit 12 s then crash
                order[e].Set(OrbitInPlace)
                if L.blackTicks >= 384: Kill(e, cause=LinkLost)   # crew returns
            case LastMile:                  # AI Last-Mile Module, 300 TP
                order[e].Set(ProceedToLastDesignated)
                autonomy[e].q = 70
                ReleaseCrew(e)              # the crew is no longer flying it
            case DualLink:                  # Dual-Link Conversion, 350 TP
                if L.blackTicks >= 64:      # 2.0 s
                    SwitchKind(e, L.altKind)  # fiber->radio, radio->mesh
                    mover[e].speedMul = FIX(0.80)
                    L.pip = Amber; L.amberTicks = 0
    else:
        mover[e].speedMul = FIX(1.00)
        combat[e].acqMul  = FIX(1.00)
        order[e].acceptsNew = true

    L.lastEvalTick = tick
```

Note what is *not* here: no per-channel modelling, no signal-to-noise, no frequency. One field value, one robustness value, one three-state pip — exactly the fidelity the gameplay spec's §5.3 display rules can express. The sim must not be able to represent a state the UI cannot draw.

### 4.7 LOD and update cadence

```
Tier A — every tick (32 Hz):
    pip != Green, OR jSampled within 25 of rEffective, OR selected by local player,
    OR in terminal dive, OR within 1.25 x radius of any emitter's bubble.
    Budget: 128 entities. If exceeded, the excess demotes to B by ascending entity id.

Tier B — every 4 ticks (8 Hz), phase = entityIndex & 3:
    all other airborne linked entities.
    Typical population: 400-700.

Tier C — every 16 ticks (2 Hz), phase = entityIndex & 15:
    grounded / hangar / landed units, satellite units far from any J>=95 emitter,
    autonomy units (topology check only).
```

Tier promotion is evaluated in the same job as resolution, using the *previous* tick's `jSampled` plus a cheap bubble-proximity test against a 48-entry emitter array — 48 squared-distance tests per candidate, but only for entities that moved more than 20 m since their last tier evaluation.

**The tier A budget is a match rule, not a device setting** — same argument as entity caps (§2.2). It is 128 at Standard and 96 at Compact, and it is identical on every peer. What *does* vary by device is nothing at all in this system: link resolution is sim, and sim does not degrade.

Worst-case budget, desktop Standard: 128 (tier A, full path, ~1.2 µs each with refinement) + 175 (tier B phase slice) + 45 (tier C phase slice) ≈ 348 resolutions/tick × ~0.9 µs = **0.31 ms**, plus 0.05 ms amortised mesh BFS, plus 0.04 ms tether work, plus 0.03 ms jam rebuild amortised = **0.43 ms/tick**.

Worst-case budget, A14 iPad at sustained clocks, Compact caps: 96 + 128 + 35 = 259 resolutions × ~1.6 µs (roughly 1.8× the desktop per-resolution cost after throttling) = **0.41 ms**, plus 0.06 ms mesh BFS, plus 0.04 ms tether, plus 0.03 ms jam = **0.54 ms/tick**, inside the 0.55 ms target with 2% to spare. This is the tightest budget in the document and it is the one benchmark scene 2 (§16.4) exists to defend.

### 4.8 Autonomy target selection and the classifier model

Runs once per autonomous munition, at terminal approach (not per tick), plus once per Arclight submunition.

```
# PSEUDOCODE 2 — autonomy / decoy classifier, gameplay spec §5.4
function SelectTarget(m):
    Q = defQ[m]                                    # 55 generic, 70 LastMile, 72 optical, 85 Arclight
    cands = SpatialQueryCone(pos[m], heading[m], 120.0, filter = classFilter[m])

    decoys = 0
    for c in cands:
        if IsDecoy(c): decoys += 1
    Q -= 5 * decoys                                # the term that makes deception work
    if IsNight() and not HasThermal(m): Q -= 20
    if InSmokeOrRain(pos[m]):            Q -= 15
    Q = Clamp(Q, 0, 100)

    for c in cands:
        P[c] = basePlausibility[Class(c)]          # 1.00 / 0.60 / 0.90 decoy / 0.25 friendly / 0.15 neutral
        if HasThermalBlanket(c):
            P[c] -= IsNight() ? FIX(0.40) : FIX(0.20)
        P[c] = Max(P[c], 0)

    r = Rand(stream = AutonomyClassify, key = (entityId(m), tick)) % 100
    if r < Q:
        return ArgMaxP(cands where not IsDecoy(c) and not IsNeutral(c))   # correct rejection
    else:
        return WeightedDraw(cands, weight = P, stream = AutonomyClassify)  # may pick a decoy, or a friendly
```

Two rules that keep this honest and are enforced in code, not in data:

1. **Deception affects machine judgement only.** If the attack is resolved by a crew with `pip == Green` within 400 m of the target, `SelectTarget` is not called at all — the player's click is the answer. This is the closing rung of the ladder (gameplay §10.4) and it must be a hard branch, not a 100-Q value.
2. **Friendly fire from autonomy is real and is reported.** A misidentification that kills a friendly emits a distinct event with a dedicated sound, a red marker and a sidebar counter (futures §7.2's requirement). The sim never silently swallows it.

Cost: one cone query (spatial hash, ~12 cells) plus ≤ 24 candidates, ~4 µs, a few dozen times a minute.

---

## 5. Vision and fog of war

### 5.1 States and storage

Four states as per gameplay §7.1: `Shroud`, `Fog`, `Live`, `Designated`. The first two are **per-cell**; the last two are **per-entity**, because they are properties of a specific object being observed, not of ground.

```
VisionGrid (per team):  byte state; ushort lastSeenTick;      // 3 B x 28,200 = 85 KB
                        two teams + two spectator views = 340 KB
EntityVis  (per team):  bitset over entity index (Live), bitset (Designated)
GhostTable (per team):  { defId; fix2 lastPos; bam16 heading; uint seenTick } x 256
```

### 5.2 Detection

Detection is **unit-versus-sensor**, evaluated at full precision, not read off the cell grid. The cell grid governs terrain reveal and the shroud; whether you can *see that tank* is a per-pair test.

```
detected = Signature(target, tick) >= Threshold(sensor, dist)
Threshold(sensor, d) = 100 * d / sensor.footprint          # 0 at the sensor, 100 at the edge
Signature(t) = base
             + 30 if moving
             + 40 if fired within last 160 ticks
             + 35 if emitting
             + 20 if on road
             - 25 if in forest
             - 40 if dug in
             - 25 if thermal blanket and night
   then     x 0.35 if night AND sensor.kind == optical
```

The evaluation set is not all-pairs. Each sensor queries the unit spatial hash within its footprint; sensors number ≤ 120 per team and the largest footprint (Recon Wing, 900 m) covers ~56 hash cells at 16 m... which is why the **detection hash is a separate hash at 64 m cells** holding only entities with a `Signature` component. A 900 m query is then 15×15 = 225 cells, typically 40–90 candidates. At 8 Hz with 120 sensors: 120 × 65 = 7,800 pair tests per evaluation, 62k/s. 0.08 ms/tick amortised.

**Per-pair caching:** a sensor/target pair that resolved `detected` keeps that result for 8 ticks unless either moved > 16 m or the target's signature flags changed. This cuts the steady-state cost roughly in half.

### 5.3 LOS against terrain and structures

An **occluder grid** at 8 m carries a height class per cell: 0 open, 1 low (hedge, wall — blocks `inf` and `light` sightlines beyond 120 m), 2 high (forest, building — blocks all ground LOS), 3 tall (the Radar Mast and EW Post's "tall" flag, which does *not* block but *is* visible from 1,800 m regardless of intervening occluders).

- **Ground sensors** run a 2D DDA along the ray, early-out on the first class-2 cell. A 300 m ray is 38 cells; ~1.4 µs. Only run for pairs that pass the signature test — so it is the *second* gate, not the first, and it runs maybe 900 times per second.
- **`Low`-layer drones** are treated as ground observers with a +1 height class allowance (they see over walls, not over forest canopy or buildings).
- **`High`-layer sensors ignore LOS entirely.** This is deliberate and is the gameplay spec's "dead ground" lesson: gullies hide from ground sensors and not from a Recon Wing.
- **Radar Masts ignore LOS and see only `Low`/`High` layer entities**, at 1,400 m. Separate code path, no DDA, one distance test.

**Dead-ground cells** carry a flag rather than a height: `−20` signature for the occupant and excluded from ground-sensor detection regardless of LOS. One flag, one lesson.

### 5.4 Footprint stamping and the shroud

Reveal is stamped into the 16 m vision grid at 4 Hz using precomputed circle offset tables (16 radius buckets from 100 m to 1,600 m; a 900 m stamp is 9,850 cells). To avoid restamping a moving Recon Wing 4× per second over 10k cells:

- A sensor restamps only when it has moved ≥ 1 vision cell (16 m) since its last stamp, or its footprint changed.
- **Orbiting sensors** (Recon Wing) stamp their *orbit envelope* — the annulus swept by the orbit — once, and refresh at 1 Hz. This is exactly correct for gameplay (the orbit does cover that annulus) and costs one stamp instead of 240.
- Cells decay `Live → Fog` when no sensor has stamped them for 32 ticks; `Fog` never decays to `Shroud`.

Budget: worst case 8 sensors restamping simultaneously × 6,000 cells = 48,000 byte writes in a Burst job, 0.06 ms. Amortised well under 0.03 ms/tick.

**Ghosts.** When a `Live` entity leaves detection, an entry is written to the team's ghost table: def id, last position, last heading, tick. Ghosts render for 800 ticks (25 s) with a drift arrow and then vanish. 256 ghosts per team, evicted oldest-first. Ghosts are sim state (the AI reads them and only them, §10.2), not view state.

### 5.5 Designation and pins

`Designated` is the targeting gate for all indirect fire. An entity is Designated if:
- it is inside any friendly **recon footprint** (a sensor with `kind: recon` — Recon Wing, Scout Quad, Multirole Quad in Recon stance, Designator Team, Forward Node), **or**
- it is inside any friendly **Pin**.

Pins are ephemeral entities: `{ fix2 centre; fix radius=120; uint expiryTick; byte owner; uint lockedHandle }`, max 6 per team. The `lockedHandle` variant follows a specific unit. A pin makes its contents Designated even with no living observer — which is the mechanic that lets recon call a strike and leave — and it stamps the *pin owner's* id onto any death inside it for the pin-payout TP award.

Evaluation: `Designated` is recomputed as a bitset each 4 ticks by iterating recon footprints and pins (≤ 20 per team) and stamping the entities in each. 20 × ~30 entities = 600 bitset writes at 8 Hz. Free.

### 5.6 Verified kills

Evaluated exactly once, on the death event, in the death handler:

```
verified = InsideAnyFriendlyReconFootprint(victim.pos, killerTeam)
        or ExistsFriendlyUnitWithin(400.0, killerTeam) with LOS(unit, victim)
        or (killer is piloted drone and link[killer].pip == Green)
tp = victimCost * (isStructure ? 0.08 : 0.12) * (verified ? 1.0 : 0.5)
     * (IsNight() and not (killerHadThermal or observerWithin400) ? 0.75 : 1.0)
if pin covering victim.pos exists and pin.owner != killerTeam's shooter:
     award(pin.owner, victimCost * 0.06)
```

Cost: one footprint test, one 400 m hash query with ≤ 3 LOS rays, on death only. At a violent 12 deaths/second that is ~50 µs/s.

### 5.7 Night and day

The cycle is a single sim-global: `phase ∈ {Day, Dusk, Night, Dawn}` plus `opticalMul ∈ fix`, driven off `tick % 11520`. Dawn ramps `opticalMul` linearly 0.35 → 1.00 over its 960 ticks. Everything else reads that one multiplier: optical sensors scale threshold, autonomy Q takes −20, supply throughput doubles, Night Bombers gate their launch, verified-kill TP takes ×0.75. There is no separate "night system" — it is one number and eleven read sites.

**Total vision budget: 0.22 ms/tick worst case.**

---

## 6. Sortie and crew model

### 6.1 Hangar cards

A `Producer` component on each air-source structure holds `hangar[8]` — a small array of `(defId, count)` pairs, capacity 12 (Drone Workshop, 16 for Obsidian), 4 (Airfield), 6 (Launch Rail). Hangar stock is destroyed with the building, which is sim state and a real raid incentive.

The **flight card rack** is a per-team *derived view*: the union of all hangar stocks, grouped by def id, with a `readyCount` and a `launchableCount = min(readyCount, idleCrews, uplinkCapacityIfSatellite)`. It is recomputed on any hangar or crew change, not per tick, and cached in the sim so the AI reads the same structure the UI does.

### 6.2 Crew pool

```
struct Crew {                                  // 16 bytes; 30 max per team
  byte state;     // Ready | Flying | Recovering | Fatigued | Benched | Reserved | KIA
  byte rank;      // 1..4
  ushort kills;   // verified kills, drives veterancy thresholds 3 / 9 / 20
  uint  stateUntilTick;
  uint  flyingHandle;
  byte  sortiesInWindow;   // rolling count for fatigue
  uint  windowStartTick;
  ushort homeQuarters;     // entity handle of the Crew Quarters that owns it
  char  callsign[2];
}
```

State transitions, all in ticks:

```
Ready --LaunchSortie--> Flying
Flying --drone lands / dies / releases (LastMile)--> Recovering(256 ticks = 8.0 s)
Recovering --timeout--> Ready | Fatigued
Fatigued: entered when sortiesInWindow >= 5 within 3840 ticks (120 s).
          recovery becomes 448 ticks (14.0 s), acquisition -10%.
          exits to Ready after 960 ticks (30 s) idle.
Ready/Recovering --home quarters destroyed--> KIA (Obsidian) | Benched(1440 ticks = 45 s) (Kestrel)
Reserved: player right-clicked the tile; excluded from auto-assignment. Toggles only from Ready.
Priority Recovery (150 TP): Recovering duration 256 -> 160 ticks for 2880 ticks (90 s).
```

Crew assignment is deterministic (§3.4) and happens inside `LaunchSortie` resolution. A crew is released back to `Recovering` the instant its drone dies, its link goes Black under `LastMile` policy, or a Loitering Munition commits to terminal dive.

### 6.3 Sortie lifecycle

```
Requested ──crew+airframe+capacity available?──┬─ no ─> Rejected (card greys, cursor shows crossed headset)
                                               └─ yes
Reserved       reserve crew, decrement hangar, allocate uplink capacity
   │ (pad egress: 8..20 ticks staggered by launch index, so six drones don't spawn co-located)
Spawning       entity created on the pad, layer Low, link established, tether spooled if fiber
   ▼
Transit        flow of ResolveLink + move toward target; retargetable
   ▼
Engaged        within weapon range or at the designated point; acquisition timer runs
   ▼
Terminal       one-way: commit, ignore new orders, +25% speed, net-trap immune within 40 m
   │  or
Returning      reusable: fly home to a Repair Pad / Airfield, rearm 35 s (Recon Wing)
   ▼
Recovered      crew -> Recovering; airframe returns to hangar stock if reusable
   or Lost     crew -> Recovering; salvage pile spawned; kill verification runs
```

Every transition emits an event so the UI, audio and telemetry all hang off one stream.

**Cost:** the sortie state machine is per-entity, evaluated in the order phase, one switch, ~60 ns. At 900 airborne entities that's 0.054 ms/tick.

---

## 7. Combat and damage

### 7.1 Resolution

Damage is fully deterministic per gameplay §10.1:

```
damage = base
       * classMul[damageType][armourClass]         // §10.2 table, fix, from content
       * facetMul                                  // 2.20 for Top when attacker is rotary/diving
       * veterancyMul                              // 1.00 / 1.00 / 1.12 / 1.12 by rank
       * (1 - armourUpgrade)
```

Applied against `cageHp` first if the target has a Cage Kit and the damage type is `Shp`; the cage is a 600 HP sacrificial pool, not a multiplier, so the arithmetic stays legible ("six FPVs, not four").

The multiplier table lives in content (`content/combat/damage_table.yaml`) as a 5 × 7 grid of fixed-point values, so balance changes are a data patch and appear in the balance dashboard's diff.

### 7.2 Interception

The one place with RNG besides autonomy:

```
P(hit) = base * cue * speedRatio * veterancy
speedRatio = Clamp(interceptorSpeed / (targetSpeed * 1.6), 0.45, 1.15)
```

Rolled from the `Interception` PRNG stream keyed by `(interceptorId, targetId, passIndex)` — keyed, not sequential, so that a roll cannot be shifted by an unrelated event ordering difference. The `cue` term reads the target's presence in a friendly Radar Mast footprint at the tick of the roll; `Second Pass` re-rolls at ×0.85 with `passIndex = 1`.

Interceptor Battery rounds are simulated projectiles with a proximity fuze evaluated at closest approach (we detect the tick where the range derivative changes sign), which gives the 16-second flight its tension and makes evasion by a Loitering Munition's Hold Orbit a real thing.

### 7.3 Projectile vs hitscan

Settled in §3.7. The rule of thumb for content authors: **if a player could plausibly react to the shot, simulate it; otherwise hitscan it.** Gun Mount and IFV bursts against a 22 m/s FPV are hitscan; a Merops-class round crossing 2 km is simulated.

### 7.4 Salvage

On death, a `SalvagePile` ephemeral entity spawns at the death position:

```
struct SalvagePile { fix2 pos; fix value; uint spawnTick; byte team; }   // team = owner for TP attribution only
value(t) = spawnValue * (1 - 1/1600)^(t - spawnTick)     # 2%/s, applied as a per-tick fixed multiply
lifetime = 1600 ticks (50 s), then the entity is freed
```

Global cap 512 piles; on overflow we evict the lowest current value. A Recovery UGV stripping a pile takes 4 s (128 ticks) versus 9 s for a Wreck Field cluster. Wreck Fields are static entities with a 2,400 unit stock and no decay.

Cost: 512 piles × one fixed multiply per tick = negligible; we actually evaluate value lazily on read and store `spawnTick`, so the per-tick cost is zero.

---

## 8. Logistics simulation

### 8.1 Nodes and sustain

```
struct SupplyNode { uint entity; fix stock, stockMax; fix sustainRadius;
                    uint lastDeliveryTick; byte flags; }   // 64 per team
```

Sustain is evaluated at 2 Hz (every 16 ticks). Each node queries the unit hash within its radius and stamps `sustainedUntilTick = tick + 1440` (45 s) on every friendly unit found. A unit whose `sustainedUntilTick` has passed is **Dry**; a unit continuously Dry for 5,760 ticks (3:00) takes attrition at 1% max HP per second.

Consumption is charged at the same 2 Hz pass, accumulated in fixed point: 1 supply / ground unit / 20 s, 3 / tank / 20 s, 2 / drone sortie launched in radius (charged at launch, not per tick), 1 / structure / 30 s.

Cost: ≤ 24 nodes × ~45 units in radius × 2 Hz = 2,160 stamps/s. Free.

### 8.2 Cut-off detection

This is the mechanic that carries thesis 5, so it gets a real algorithm rather than a distance heuristic.

A **logistics graph** is derived from the build grid at 64 m resolution (47 × 38 = 1,786 nodes on the largest map), with edges weighted by the `Logistics` cost profile and **blocked** where the node's own team's threat intensity exceeds a threshold *or* an enemy structure's control radius covers it.

Every 32 ticks (1 Hz), for each friendly Forward Node, we run a single-source Dijkstra from the Command Post over this graph and check reachability. A node is flagged `CUT OFF` when **both**: no delivery in 2,880 ticks (90 s) **and** no route exists. Consequences per gameplay §9.3: drain rate doubles, everything in radius goes Dry immediately, and at 0 stock the node loses 4 HP/s.

Cost: one Dijkstra over 1,786 nodes with a bucket queue is ~90 µs; ≤ 12 nodes per team per second = 1.1 ms/s, i.e. 0.034 ms/tick amortised. We run it in a Burst job on the tick where `tick % 32 == teamId`.

The same graph, with the same threat weights, is what the `Logistics` cost profile consumes for routing (§3.5), so trucks route around danger and the cut-off test agrees with what the trucks are doing. One graph, two consumers.

### 8.3 Carriers

The four carriers (Supply Truck, Logistics UGV, Motorcycle courier, Night Bomber) are ordinary units with a `Carrier` component: `{ fix load, loadMax; uint sourceNode, destNode; byte phase }`. They run a three-phase loop (`ToSource → Loading → ToDest → Unloading`) as an order, not as a special system. The Logistics UGV's "halts permanently if jammed" is a one-line consequence in `ResolveLink`'s Black handler for ground units under `Abort` policy: stop, clear orders, become salvage-on-death. The Night Bomber's unique ability to reach a cut-off node is simply that it is an air unit and the cut-off test is a *ground* graph test — no special case at all, which is the sign the model is right.

---

## 9. Threat overlay and exposure preview

### 9.1 The threat field

One field per team, on the 32 m grid, in sim, fixed point.

```
struct ThreatCell { fix intensity; byte sourceMask; byte lastSourceTeam; }   // 8 B x 7,050 x 2 teams = 113 KB
sourceMask bits: Loss | ObservedLaunch | KnownStructure | Inferred
```

**Deposits:**
- A friendly loss stamps a Gaussian-ish disc (we use a 3-step linear falloff, cheap and adequate) of radius 200 m with peak intensity `unitMatCost / 400`, capped at 8.0.
- An observed enemy drone launch stamps its origin, radius 150 m, intensity 3.0.
- Each *known* enemy structure with a reach (Workshop, Relay Mast, EW Post, Gun Mount, Launch Rail) projects a ring at its theoretical reach, intensity 2.0, refreshed while the structure is remembered.

**Decay:** 1.5%/s, applied every 8 ticks as a single fixed multiply of `(1 − 0.015)^0.25 = 0.99623` over the whole grid — 7,050 multiplies at 4 Hz, 28k/s, one Burst job, 0.004 ms/tick.

The field is deliberately a *memory*, not truth. It never reads enemy entity positions directly; it reads only deposit events the team was entitled to observe. That property is what makes it safe to hand to the AI (§10.2) and honest to show the player.

### 9.2 What the player sees

Hotkey `V` renders `intensity` as a red stain through an additive screen-space pass over the terrain (§12.6). No numbers. The gameplay spec's open question 2 — does it become wallpaper — is answered structurally: the same field is what colours the move-order exposure preview, so a player who never presses `V` still consumes it.

### 9.3 The exposure preview

The move-order drag preview shows, per path segment, `Exposure` (seconds spent detectable in the open) against `Reaction` (the player's *estimate* of how fast the enemy can kill there). The critical constraint: **this must not cheat**. It may read only what the team legitimately knows.

```
# PSEUDOCODE 3 — exposure and reaction estimate for a dragged move order
# Invoked from the view at <= 10 Hz on the drag handler; read-only sim query.
# Inputs it is ALLOWED to read: own units, own threat field, own vision grid,
#   own ghost table, own known-structure list. It may NOT read enemy entities.

function PreviewPath(unit, pathCells, team):
    segments = SplitIntoSegments(pathCells, maxLen = 150.0)   # ~6-16 segments typical
    results = []

    for s in segments:
        # ---------- Exposure: how long am I detectable here? ----------
        speed   = unit.speed * TerrainSpeedMul(s) * (OnRoad(s) ? 1.60 : 1.0)
        travelT = Length(s) / speed

        sig = unit.baseSignature + 30                          # we will be moving
        if OnRoad(s):      sig += 20
        if InForest(s):    sig -= 25
        if InDeadGround(s):sig -= 20
        if IsNight():      sigOptical = sig * 0.35 else sigOptical = sig

        # "Detectable" means: could a plausible enemy sensor see me here?
        # We do NOT query enemy sensors. We use the threat field's KnownStructure
        # and ObservedLaunch bits as a proxy for "the enemy has eyes on this area",
        # plus a floor for open ground in daylight.
        observedP = Clamp(ThreatAt(team, s).intensity / 6.0, 0.0, 1.0)
        coverMul  = InForest(s) ? 0.35 : (InDeadGround(s) ? 0.55 : 1.0)
        exposure  = travelT * observedP * coverMul * (sigOptical / 100.0)

        # ---------- Reaction: how fast could they answer? ----------
        # detection latency: better-observed ground => faster
        detLatency = Lerp(6.0, 2.0, observedP)

        # crew readiness: estimated from OBSERVED enemy sortie cadence, not truth.
        # cadence = launches we have observed in the last 60 s, from the threat field's
        # ObservedLaunch deposits. More launches seen => we assume more ready crews.
        crewReady = Clamp(8.0 - 1.2 * ObservedLaunchRate(team, window = 60s), 0.0, 8.0)

        # flight time: from the nearest KNOWN or INFERRED enemy launch origin.
        origin = NearestThreatSource(team, s, mask = ObservedLaunch | KnownStructure)
        if origin == none:
            flightT = 999.0                                     # we know of nothing nearby
        else:
            flightT = Dist(origin, s) / ASSUMED_FPV_SPEED       # 22.0 m/s, a constant
            flightT += origin.confidenceAgeSeconds * 0.15       # stale intel is optimistic

        reaction = detLatency + crewReady + flightT

        results.add({ seg: s, exposure: exposure, reaction: reaction,
                      band: exposure > reaction         ? Red
                          : exposure > reaction * 0.65  ? Amber
                          :                               Green })
    return results
```

Three properties worth defending:

1. **It is wrong in the right direction.** With no intel, `flightT = 999` and everything reads green — the map looks safe because you have not looked. The first truck that dies deposits a stain and the same road turns amber. That is the gameplay spec's pillar 4 ("the kill zone is drawn by casualties") implemented as an equation.
2. **It is cheap, and touch makes it more valuable.** ≤ 16 segments × ~20 grid reads = 320 reads, ~8 µs, at ≤ 10 Hz. It is a sim query rather than a sim step, has no side effects, and is excluded from the desync hash. On mouse it appears during a right-drag, which most players never discover; on touch the order ribbon (§3A.2) *is* the drag, so every move order a touch player gives shows the preview under their finger by default. The mechanic the gameplay spec calls "the entire logistics game expressed as one colour" gets substantially more screen time on the primary platform than it would have had on the secondary one.
3. **The AI uses the identical function** with its own team's field (§10.3), which means the AI's sense of danger is exactly as wrong as the player's, and for the same reasons.

---

## 10. AI

### 10.1 Architecture: layered utility, with a scripted fallback

**Decision: utility-based scoring over a shared blackboard, in four layers, with HTN-style build-order templates as the strategic layer's action set.** Not a behaviour tree (they encode "how" and become unmaintainable when the game has five interacting economies), not GOAP (planning cost is unbounded and the plans are unreadable when they go wrong, which is the thing you debug most).

| Layer | Rate | Scope | Output |
|---|---|---|---|
| **Strategic** | 1 Hz | whole match | which tech path, which build template, when to commit to a doctrine, when to fire the super |
| **Operational** | 2 Hz | 3–6 task forces | task force missions: claim a wreck field, interdict a road, raid crew quarters, hunt recon |
| **Tactical** | 8 Hz | per task force | target selection, sortie requests, stance changes, ability use, retreat |
| **Reflex** | 32 Hz | per unit | separation, opportunistic fire, Terminal Commit, evasion from an incoming interceptor |

Every layer scores a fixed candidate set with a weighted sum of normalised considerations and picks the argmax with a hysteresis margin (a new option must beat the current by 15% to cause a switch — this single rule eliminates the "AI dithers between two targets" pathology). All scoring is fixed-point and runs on the sim thread; the AI is inside the determinism boundary and its decisions are part of the replay.

### 10.2 What the AI is allowed to know

**The AI reads its own team's vision grid, ghost table, and threat field. Nothing else.** This is enforced by giving the AI a `TeamKnowledgeView` handle rather than the sim state, and by a CI test that runs an AI-vs-AI match with enemy entities relocated randomly outside detection and asserts identical AI command streams.

Difficulty is expressed as **decision latency, action throughput and doctrine aggression** — never as income multipliers or map vision:

| Tier | Decision latency | APM cap | Hysteresis | Notes |
|---|---|---|---|---|
| Recruit | 2.5 s | 45 | 30% | ignores threat field; will walk trucks into the band |
| Regular | 1.2 s | 90 | 20% | full model, conservative weights |
| Veteran | 0.6 s | 140 | 15% | pre-positions interceptors on radar cues |
| Commander | 0.35 s | 190 | 12% | doctrine-switches mid-match; hunts crew quarters |

### 10.3 How it reasons about the three novel systems

**Links.** The AI maintains a `LinkDenialMap` — its own jam grid (which it knows exactly, they are its emitters) unioned with an *estimated* enemy jam grid built from observed EW structures and from the locations where its own drones went amber. Every strike consideration includes a term `linkViability = 1 − P(black before arrival)`, evaluated by walking the intended path against the denial map at 128 m steps. This is what makes the AI buy fiber: when `linkViability` for radio strikes on the highest-value target set drops below 0.5 for 30 continuous seconds, the strategic layer raises the utility of the Spool Plant build template by 2.5×. The AI therefore climbs the link ladder for the same reason the player does, from the same evidence.

**Kill zones.** The operational layer uses `PreviewPath` (§9.3) verbatim for every ground movement it orders, and refuses any move whose worst segment is Red unless the mission's priority exceeds a threshold. Its own interdiction missions are scored by *how much enemy threat-field deposit they generate* — the AI is explicitly trying to teach the enemy to be afraid of a road, which is a startlingly good proxy for playing the map correctly.

**Crews.** The tactical layer treats the crew rack as a scheduler with a horizon: it will not spend the last two idle crews on an opportunistic FPV run if a task force has a scheduled strike inside 12 s. It also values enemy Crew Quarters by `4 × 40 TP + estimated sortie denial`, which is what produces the deep raid behaviour the campaign teaches.

### 10.4 What the AI is not

The **touch assist layer (§3A.5) is not AI and does not live here.** It runs in `KZ.View`, it has no blackboard, it cannot see the sim's knowledge model, and its entire output is a command the player could have issued by tapping. Keeping the two apart is what lets us tune assist aggressively for touch without ever touching competitive balance — and what lets a PC player enable the same assists without anyone being able to claim the mobile build "plays itself".

### 10.5 Schedule honesty

The gameplay spec's open question 6 asks us to cost this honestly. The answer: **the full utility AI is 5.5 engineer-months** and is scheduled across M4 and M5. The risk mitigation is that the layered design degrades gracefully — if we are behind at M5, we ship **three scripted doctrines** (Volume Rush, Jam-and-Push, Recon-and-Strike) as fixed strategic+operational layers with the real tactical and reflex layers underneath. That is a 1.5-month deliverable and it plays acceptably because the tactical layer is where the game's texture lives. This fallback is a written milestone exit criterion, not a hope.

---

## 11. Multiplayer

### 11.1 Deterministic lockstep

**Decision: deterministic lockstep, UDP, with our own thin relay; no authoritative server.**

The case, and it is stronger on mobile than it was on desktop: our worst-case state is 900 entities × ~64 bytes of gameplay-relevant state = 58 KB, at 32 Hz, for up to 4 players. Server-authoritative replication of that is 200–600 kbit/s per client — which is a server fleet we would pay for through early access, *and* a cellular data bill we would hand to the player. A 25-minute 1v1 in lockstep produces 1,100–2,600 commands totalling **28–60 KB for the entire match**, i.e. about **3–8 MB/hour including overhead and heartbeats**, which is a number a player on a metered plan does not notice. Lockstep also gives us, free: perfect replays, headless balance simulation at 40×, AI-vs-AI CI, spectating, and — because commands are tiny — a reconnect story that survives a subway tunnel.

The price is that determinism becomes a correctness requirement rather than a nice property, and that one bad client desyncs everyone. §2.5 pays the first; §11.4 detects the second.

We are not doing rollback. Rollback is for 2-player fighting games with 20 entities, not 4-player RTS with 3,000 — and on a thermally-limited mobile core, resimulating N turns on every misprediction is precisely the workload we cannot afford.

### 11.2 Turn structure, and what cellular does to it

```
Simulation tick     31.25 ms  (32 Hz, identical on every platform, never adaptive)
Command turn        4 ticks = 125 ms
Input delay         2..6 turns = 250..750 ms, adaptive
Default by link     Wi-Fi / wired: 2 turns (250 ms)
                    5G:            3 turns (375 ms)
                    LTE:           4 turns (500 ms)
```

Commands issued during turn *N* execute at the start of turn *N+D*. The adaptive controller is driven by the **99th percentile** one-way delay over a rolling 10 s window, not the mean and not the 95th — because the thing that breaks lockstep on cellular is not average latency, it is the **jitter spike**: a 5G→LTE handover, a tower reselection or a carrier-aggregation change produces a 200–600 ms stall in an otherwise 40 ms connection. A p95 controller sits at 2 turns and then stalls the match four times a minute. A p99 controller sits at 4 and never stalls.

Delay is raised immediately on a single late turn and lowered only after **40 consecutive clean turns** (5 s). The asymmetry is deliberate: raising delay costs responsiveness, stalling costs the match.

250–500 ms of input delay is well above the ~120 ms where players consciously notice, and we hide it the way every good RTS does, and better than most because of the flight-card model: the **view responds instantly**. Tap a flight card and the card lifts, the bark plays, the targeting state engages, the reticle follows the finger; tap a target and the confirmation animation and audio fire on that frame. The drone leaves the pad up to half a second later. In a genre where the mental model is "I gave an order and the unit is now doing it", this is invisible; it is why RTS ships on lockstep and fighting games do not.

**Stall policy.** If a peer's commands have not arrived when a turn must execute, all peers stall together, with a "waiting for *player*" overlay naming them and a countdown. Stall tolerance is **2,000 ms on Wi-Fi, 3,500 ms on cellular**. After that the peer is dropped into an "away" state: their units go to hold-position AI, the match continues, and they have the full reconnect window (§11.7) to return. Stalling rather than speculating is the only choice compatible with no-rollback lockstep.

**Slow-peer detection.** A peer whose sim thread cannot sustain 32 Hz — a thermally throttled phone, a background-heavy device — will fall behind without any network cause. Each peer reports its own `simMsPerTick` in the turn packet; a peer averaging over 26 ms/tick for 5 s gets a client-side "your device is struggling" prompt offering to drop the frame cap and effect budget (§12.10), and if it persists past 20 s the match records it in telemetry so we can find the device class. We never silently reduce anyone's simulation, because we cannot.

### 11.3 Packet format

Per turn, per peer: a single UDP datagram containing turn number, command count, the commands (typically 0–6, ~14 bytes each), the peer's **state hash for turn N−8**, and a 2-byte `simMsPerTick` health field. Typical datagram 30–112 bytes at 8 Hz = **2–7 kbit/s per peer**.

Redundancy: each datagram repeats the **previous three turns'** commands (up from two on the PC-first design) — 375 ms of history. On cellular, loss arrives in bursts, and three turns of redundancy converts a 3-packet burst loss from a stall into a non-event at a cost of roughly 40 bytes per datagram. There is still no ACK/retransmit path; at this volume redundancy is strictly cheaper.

Heartbeats at 2 Hz when idle (lobby, loading) keep NAT bindings alive; on cellular, carrier NAT timeouts of 30–60 s are common and a silent socket is a dead socket.

### 11.4 Desync detection

Every 32 ticks (1 s) each peer computes a **hierarchical FNV-1a 64 hash**:

- One hash per system: `Transforms`, `Health`, `Links`, `Tethers`, `Orders`, `Crews`, `Economy`, `Vision`, `RNG streams`, `AI blackboard`.
- One combined hash.

Combined hashes ride along in the turn packet (8 bytes, lagged 8 ticks so there is no stall waiting for them). A mismatch triggers: (a) immediate pause, (b) exchange of the 10 per-system hashes to localise the divergent system, (c) exchange of a per-entity hash array for that system to localise the divergent entity, (d) a desync report bundle — replay file, both peers' last 8 snapshots, the localisation trail — uploaded automatically with player consent. This turns the historically worst bug class in the genre into a 20-minute debugging session with the exact entity and the exact system named.

During development, `-determinism-paranoid` hashes every tick and every system and asserts on the first divergent tick, which is how we bisect at M1–M3 (§16.1).

**Cross-architecture note.** The most likely source of a real desync on this project is not a gameplay bug; it is an **ARM64-vs-x64 codegen difference in the fixed-point library**. The hierarchical hash is therefore always compared *across platform pairs first* in CI: an iOS↔PC replay divergence is triaged as a math-library bug until proven otherwise.

### 11.5 Replay format

```
KZRP header (128 B): magic, format version, engine build id, content hash,
                     map id, era pack ids, rng master seed, player records,
                     start timestamp, match rules
Command stream:      varint-delta-encoded turn numbers + commands
Checkpoint index:    state hash every 32 ticks (for verification, 8 B x ~1,500)
Chat/annotation:     optional, excluded from the hash
```

Total for a 25-minute 1v1: **34–72 KB** — which means a replay fits in an iMessage, a Discord post, or a telemetry payload, and that is the whole reason the format is designed this way. Playback is `Step()` driven by the recorded stream; seeking is done by **resimulating from the start at up to 40× real time** (~37 s for a 25-minute match on a desktop core; **~95 s on an A15**, which is why scrubbing is a desktop-first feature and the iOS replay viewer offers bookmarks and forward-only playback rather than a scrub bar at EA). The in-memory snapshot ring is 8 on PC and 3 on iOS (§2.2). We deliberately store no keyframes in the file — it would take a 40 KB replay to 30 MB and destroy the best community feature we have.

Replays are version-locked on engine build id **and** content hash, and are architecture-independent: a replay recorded on an iPhone plays back bit-identically on a PC, which is both a feature and our primary cross-architecture determinism test (§16.1). A version-mismatched replay is rejected with the correct version named, never played back wrong.

### 11.6 iOS suspension, backgrounding and interruptions

This is the problem that does not exist on PC and that will generate more support tickets than anything else in this document. A phone is interrupted constantly: a call, a notification pulled down, an app switch, a lock, a low-battery alert. iOS gives a backgrounded app roughly **5 seconds** before suspension, extensible to about **25–30 seconds** with a `beginBackgroundTask` assertion, and no network guarantee after that. A suspended lockstep peer stalls everyone.

**The policy, in four cases:**

1. **Brief interruption (< 25 s), 1v1.** On `willResignActive` the client immediately sends a `PauseRequest` and enters `Away`. Both peers pause with a visible 45-second away-timer and the absent player's name. The network thread holds a background-task assertion so the socket survives; incoming turns are buffered. On `didBecomeActive` the client **fast-catches-up**: it resimulates the buffered turns at up to 16× (25 s of missed simulation in ~2 s on an A15, with rendering suspended and a progress ring), then rejoins the live turn. No snapshot transfer, no disconnect, no visible damage.
2. **Brief interruption, 2v2.** **No shared pause.** Three players do not wait for one. The away player's units go to hold-position AI, the match continues, and they fast-catch-up on return — which on a 2v2 means resimulating from the live turn backwards is impossible, so they take the snapshot path (§11.7) if they missed more than 8 s. This asymmetry between 1v1 and 2v2 is deliberate and is stated in the UI before the match starts.
3. **Long interruption (25 s – 3 min).** Suspension happens; the socket dies. On resume the client reconnects and takes a full snapshot (§11.7).
4. **Beyond 3 minutes or app termination.** The match is lost as a disconnect. In 1v1 the opponent is awarded the win after the away-timer expires; in 2v2 the AI continues the slot to the end.

**Each player gets two shared pauses per 1v1 match, 45 s each**, pooled across both causes (interruption and manual pause). Beyond that, further interruptions do not pause the opponent. Without this cap, "pause" is a grief tool.

**What we will not do:** request background audio, VoIP or location entitlements to stay alive. They are App Store rejection risks under Guideline 2.5.4 (using a background mode for a purpose other than its intent), and they drain battery. We take the suspension and engineer around it.

**Other interruptions handled explicitly:** incoming call (same as case 1), Control Centre / Notification Centre pull (does not resign active — we ignore it), Low Power Mode entering mid-match (thermal governor drops to the 30 fps profile and shows a one-line notice), and an iPad **Stage Manager resize** (a view-layer relayout at a breakpoint, §12.9; the sim never sees it).

### 11.7 Reconnection

A dropped peer can rejoin for **180 s**. On rejoin, the peer with the lowest player id serialises a full sim snapshot (6.0 MB Standard / 2.6 MB Compact, LZ4-compressed to ~1.4 MB / ~0.6 MB) over a TCP side channel via the relay, the rejoiner deserialises, verifies its hash against the donor, and resumes. All peers stall for the transfer with a progress bar: **1.5–4 s on Wi-Fi, 3–9 s on LTE** for a Compact snapshot. Compact's smaller snapshot is a second reason it is the default cross-play ruleset.

Snapshot serialisation is a straight memcpy of the component arrays plus the grids, which is why §2.2's hand-rolled SoA table pays for itself a third time. Endianness is not a concern (both targets are little-endian) but the serialiser writes an explicit byte order and struct-layout hash anyway, because a silent layout difference between the ARM64 and x64 builds would be the worst bug in the project.

### 11.8 Identity, cross-play and matchmaking scope for early access

**Identity: platform-native, with a thin linking service.** Game Center (`GKLocalPlayer.teamPlayerID`) is the identity on iOS; Steam ID is the identity on PC. Both map to an opaque `kz_player_id` in a small first-party service that holds nothing else but a rating and a display name. **We do not build accounts** — no email, no password, no recovery flow, no GDPR subject-access tooling we would then have to staff. A player who wants their PC and iPad progress linked signs in once on each with a 6-digit link code. Achievements, friends and invites are Game Center on iOS and Steam on PC, natively, via the `IPlatformServices` interface (§1.3).

**Cross-play: on by default, and structurally free.** The simulation is bit-identical across architectures (§2.5), content is hash-matched, commands are platform-agnostic (§3A.1). The only real work is transport and identity, both above. Two knobs:
- **Ruleset negotiation.** The lobby picks `Compact` if any peer is Tier C (§12.7); 2v2 requires all peers Tier B+.
- **Input parity preference.** Quick Match *soft-prefers* matching input methods (touch↔touch, pointer↔pointer) for the first 45 s of queueing, then widens. It is a preference, not a filter, and it is visible in the queue UI. We are not splitting an early-access population into two ladders on a theory about input advantage we have not measured; we are collecting the telemetry (win rate by input method, controlling for rating) and will revisit with data.

**Transport: our own relay.** Steam Datagram Relay is unavailable to iOS clients, so we run **one small UDP relay per region** (NA-E, NA-W, EU-W, EU-C, APAC-SE, APAC-NE) — commodity boxes, ~$20–40/month each. Peers attempt direct P2P first (ICE-style, 1.5 s timeout); cellular and CGNAT sessions almost always fall back to the relay, and that is fine: at 2–7 kbit/s per peer, **one box relays several thousand concurrent matches**. The relay forwards, never simulates, and holds no state beyond a session table. It also gives us DDoS isolation and hides peer IPs, which P2P does not.

**Matchmaking scope at early access.** Deliberately small.

- **Quick Match:** 1v1 (all tiers) and 2v2 (Tier B+), region-bucketed, matched on a hidden Glicko-2 rating with a widening band (±80 → ±400 over 90 s), soft input-parity preference. No visible rank, no ladder, no seasons.
- **Lobby browser and private lobbies:** named lobbies, map/era/rule selection, passwords, spectators up to 4. Game Center and Steam friend invites both route here.
- **Co-op vs AI:** 2 players vs 2 AI.
- **Custom games with mods:** PC only (§15), marked, excluded from Quick Match and rating.
- **Not at EA:** ranked ladder, tournaments, clans, free-for-all, more than 4 players, cross-region matching.

Backend total: six relay boxes plus one service box for ratings, link codes and telemetry ingest. Under $400/month at EA scale. No dedicated game servers, because lockstep does not need them.

---

## 12. Rendering and readability

### 12.1 Scale and camera

**1 world unit = 1 metre, exactly, throughout.** No scaling between sim and render.

The camera is perspective with a **narrow 26° vertical FOV at a 58° pitch**, which gives a near-orthographic C&C read while keeping enough perspective that hills and tall structures are legible. Rotation is **fixed** (no free rotation) — it costs nothing thematically, it saves silhouette design, terrain authoring and UI anchoring, and on touch it removes an entire gesture we do not have room for.

Camera height and zoom range are **per device class**, because the constraint is angular size on the physical panel, not pixels:

| Device | Default height | Ground shown (default) | Zoom range |
|---|---|---|---|
| iPad 13" | 220 m | ~430 × 240 m | 140–460 m |
| iPad 11" | 205 m | ~400 × 225 m | 130–430 m |
| iPhone (compact) | **155 m** | ~300 × 140 m | 110–300 m |
| PC 1080p+ | 220 m | ~430 × 240 m | 130–460 m |

The iPhone sits closer and has a shorter zoom-out range, deliberately: a 900 m-wide strategic view on a 6.1" panel is unreadable, and the player who needs it is better served by the **minimap overlay** (§12.9). Camera control is two-finger drag and pinch on touch; edge-scroll, drag-scroll, keyboard and minimap jump on pointer.

**Units are rendered at a scale multiplier, not at true scale.** An FPV drone is 0.4 m across; at default zoom on an 11" iPad that is under one physical millimetre. Render scale by class: infantry ×2.4, small drones ×3.6, large drones ×2.2, light vehicles ×1.8, heavy vehicles ×1.6, structures ×1.0. On iPhone, all mobile classes take an additional **×1.25** so the on-screen size band stays in the same *angular* range. Structures stay at true scale everywhere to preserve the base-building read (a 5×4 Drone Workshop occupies exactly its 40 × 32 m footprint). The target on-screen band is **18–46 px at default zoom on PC, and 2.2–5.4 mm of physical panel on iOS** — the second number is the one that matters and the one we test against, by printing a physical ruler overlay in the debug build.

### 12.2 Rendering path

**URP Mobile renderer on Metal** (forward, not forward+), with the same path used on PC so there is one set of shaders, one variant matrix and one set of bugs:

- **One directional light.** No realtime point or spot lights. Muzzle flashes, thermite, burning wrecks and nav lights are additive unlit quads and emissive material terms, not lights. This is the single biggest mobile GPU saving available and it costs us nothing at this camera angle.
- **No depth prepass, no SSAO, no post stack beyond the colour grade and a cheap bloom.** MSAA 2× on Tier A/B, FXAA on Tier C.
- **HDR off on Tier C**, on elsewhere.

and, on both platforms:

- **Terrain:** a single custom-shader mesh with a splat-map, plus a decal layer for roads, craters, burns and thermite scars. One draw call for terrain, one for decals.
- **Units:** `RenderMeshIndirect` with per-instance data (transform, team colour, damage state, selection flag, link pip state, **picking id**) in a structured buffer written from the view's interpolation pass. One indirect draw per (mesh, material) pair — typically 22 pairs for the whole roster. **No GameObjects for units.** Structures get GameObjects (few, static, per-building destruction states). Draw-call target: **≤ 190 per frame on iOS**, ≤ 400 on PC.
- **Picking:** the instance buffer carries a picking id, and touch resolution (§3A.4) queries a **CPU-side screen-space acceleration grid** rebuilt each frame from the same interpolation pass — not a GPU readback, which would cost a frame of latency and which Metal makes awkward on a tiler. 2,000 entities into a 32 × 32 screen-cell grid is ~0.15 ms.
- **Shadows:** one cascade, **1024² on iOS / 2048² on PC**, from a fixed sun/moon direction. Units cast a cheap blob-plus-directional-quad rather than shadow-map geometry below 200 m camera height — which is all the time on iPhone.
- **Effects:** a GPU particle system with a hard cap of **24,000 live particles on Tier A, 12,000 on Tier B, 6,000 on Tier C**, priority-budgeted (explosions > tracers > dust > ambient). Particles are view-only and excluded from the desync hash, so the cap can vary by device freely.
- **Tile-based GPU discipline:** we never sample the colour buffer mid-pass, never use a full-screen grab pass, and keep render-target switches to three per frame (opaque+transparent, UI, present). On Apple's tile-based deferred GPUs a stray resolve costs more than any amount of geometry, and the Link View desaturation (§12.5) is implemented as a **material term driven by a global uniform**, not as a blit, specifically for this reason.

### 12.3 Silhouettes

The readability rule: **a player must identify a unit's role, link type and team from its silhouette and colour alone, at default zoom, at arm's length on an 11-inch iPad, with the sound off.** The mobile framing is stricter than the 1080p-monitor framing it replaces, and it is the binding one — if it reads on an iPad it reads everywhere. Enforced by a "silhouette sheet" test (§16.4) that renders every unit at default zoom, on each layout target, to a 1-bit mask and asserts a minimum pairwise Hamming distance within each layer.

Design language:
- **Role by shape.** Recon = long thin wings. Strike = compact X. Interceptor = swept dart. Logistics = boxy with a visible load. Jamming = visible mast/antenna array.
- **Link by accent.** Radio = no accent. Mesh = a small repeated chevron on the hull. Fiber = the tether itself (orange line). Satellite = a pale up-arrow badge. Autonomy = no link line and a dashed hunt box, which is a *negative* signal and reads instantly in Link View.
- **Team by colour ramp**, not by hue alone: Kestrel is cool (ice blue → white), Obsidian is warm (amber → rust). Colour-blind modes swap the ramps for luminance-separated pairs and add a shape badge.
- **State by overlay:** the three-state link pip, the Dry fuel glyph, the attrition glyph, the veterancy chevrons, the Designated diamond bracket. Five overlays maximum, ever.

### 12.4 Squadron rendering

The futures spec's "a squadron is 3–6 drones and one icon, always" is honoured at the *selection and UI* level: launching a flight of six creates six entities that are auto-grouped into a **flight**, share one selection box, one health summary and one card in the selection panel, and are commanded as one. They render individually with a formation offset. This gives the readability the futures spec wants without aggregating the sim, which would break tethers and per-drone link state.

### 12.5 Link layer and tethers

**Link View** — held `Tab` on pointer, a toggle in the dock or a two-finger tap on touch — is a global uniform plus one procedural line mesh. No fullscreen pass.

1. A global `_LinkViewBlend` uniform, lerped over 140 ms, drives a desaturation + darkening term inside the terrain and unit shaders. On a tile-based GPU this is free; a screen-space pass would cost a resolve.
2. A single dynamic mesh containing every link line for the local team, built once per frame in a Burst job from the sim's `Link` components: a quad strip per line with per-vertex colour (green/amber/black), a scrolling dash texture for mesh segments, hop-count sprites at midpoints, up-arrow badges for satellite, dashed boxes for autonomy hunt areas. **One draw call for the entire control network.** At 900 links that mesh is ~7,200 vertices, rebuilt in 0.2 ms on desktop and 0.45 ms on an A14.
3. Selected units draw their own line always, without the overlay, from the same buffer with a selection mask.

**Tethers** render from the sim's tether polylines as a ribbon mesh with a fixed **1.4 m world width on tablet/PC and 2.0 m on iPhone** (so they survive the smaller panel) in a single additive draw call, with four LODs: full polyline inside 400 m of the camera, every second node from 400–900 m, every fourth node from 900–1,600 m, and a straight anchor-to-drone line beyond. Lingering (dead) tethers render at 40% alpha fading over their 30 s.

Vertex budgets: PC Standard, 200 tethers × ~44 nodes × 4 verts = 35,000 verts, one draw call, 0.4 ms including the Burst rebuild. iOS Compact, 120 tethers with the LOD thresholds pulled in by 30% ≈ 14,000 verts, 0.35 ms on an A14. This remains the single most expensive piece of game-specific rendering we do, and it is worth it, because the futures spec is right that an invisible fiber threat is unseeable death — and because on a tablet the tether is often the only thing telling the player where the enemy launch site is.

**Jam bubbles** render as a ground-projected ring with a soft interior gradient, drawn in the decal pass — they are terrain, per gameplay §5.3, so they are painted on the ground and not as a volumetric dome. Maximum 48 bubbles, one instanced draw.

### 12.6 Night rendering

Night is **not** darkness. A literally dark screen in an RTS is a readability catastrophe. Night is:

- A colour grade: desaturate to ~35%, shift toward blue-cyan, lift blacks, compress highlights.
- A reduction of terrain detail contrast, so terrain recedes and units pop.
- Units keep near-full readability. Their *detectability* is a sim property (the optical multiplier), not a rendering one. What actually disappears at night is the enemy — because they are not detected, so they are not drawn.
- Additive hot spots: muzzle flashes, thermite, burning wrecks, drone nav lights, Grid Tap glow. These become the visual anchors of the night phase.

The transition over Dusk/Dawn is a 45 s / 30 s lerp of the grade parameters, driven by the same `opticalMul` the sim uses, so what the player sees and what the sim does move together.

### 12.7 Device tiers

Three iOS tiers plus PC. Tier is detected at first launch from the device model and validated by a 6-second boot benchmark (a scripted sim + render burst), stored, and re-checked after an OS update. The player can override downward, never upward.

| | **Tier C — Minimum** | **Tier B — Target** | **Tier A — Recommended** | **PC** |
|---|---|---|---|---|
| Devices | iPhone 11/12/SE3, iPad 9th gen | iPhone 13/14/15, iPad 10th gen, iPad Air 4/5 | iPhone 15 Pro+, any M-series iPad | — |
| SoC | A13–A15, **3 GB RAM** | A14/A15/A16, 4–6 GB | A17 Pro / M1+, 8 GB+ | — |
| Ruleset | Compact (1,536 entities) | Standard | Standard | Standard |
| Modes | 1v1, campaign, skirmish | + 2v2 | + 2v2, spectate 4 | all |
| Frame cap | **30 fps** | 60 fps | 60 fps (120 opt-in on ProMotion) | uncapped |
| Render scale | 0.75 | 0.85–1.0 | 1.0 | 1.0 |
| Particles | 6,000 | 12,000 | 24,000 | 24,000 |
| Tethers drawn | 64 | 128 | 256 | 256 |
| Shadows | blob only | 1024² cascade | 1024² cascade | 2048² cascade |
| Map pool | small (≤ 2,600 × 2,200 m) | all | all | all |

**PC minimum:** i5-7400 / Ryzen 3 1200, GTX 1050 Ti 4 GB, 8 GB RAM, 12 GB SSD, Windows 10 x64 (Linux via Proton, unsupported but expected to work). **PC recommended:** i5-12400 / Ryzen 5 5600, RTX 3060, 16 GB.

**iPadOS 17 / iOS 17 minimum.** Devices below A13, and A12 iPads with 3 GB, are not supported: the sim tick alone does not fit in their sustained core budget, and we would rather have no build than a bad one. This excludes roughly the bottom of the installed base and we accept it.

### 12.8 Frame budgets

**Tier B (A15 iPad) at 60 fps — 16.67 ms, measured at sustained clocks, not burst:**

| Item | Budget |
|---|---|
| Sim tick worst case (1.9 frames per tick, so ~2.1 ms amortised per frame) | **4.0 ms/tick** |
| — link layer | 0.55 ms |
| — vision + detection | 0.30 ms |
| — movement, steering, collision | 0.65 ms |
| — pathfinding (amortised field builds) | 0.50 ms |
| — combat, orders, economy, supply | 0.70 ms |
| — AI (two opponents worst case) | 0.60 ms |
| — everything else + slack | 0.70 ms |
| View interpolation + instance buffer + picking grid | 1.9 ms |
| Culling + draw submission (Metal, ≤190 draws) | 1.3 ms |
| UI (retained-mode, ~400 elements) | 1.1 ms |
| GPU frame | 9.2 ms |
| Slack | 1.1 ms |

**Tier C (A13 iPhone) at 30 fps — 33.3 ms:** the sim still runs at 32 Hz, so the per-frame sim share roughly doubles to ~4.1 ms; Compact caps bring the worst-case tick to **3.2 ms**, GPU to 13 ms at 0.75 render scale, and total frame to ~24 ms with 9 ms of headroom for thermals.

**PC minimum at 1080p/60:** sim 3.6 ms/tick, GPU 10.5 ms, total ~15.8 ms.

**Sim entity caps are enforced, not aspirational.** At the ruleset cap, production queues stall with a "force cap reached" message rather than degrading the frame rate.

### 12.9 Layouts, resolutions and safe areas

**Landscape only, on every device.** A C&C sidebar plus a minimap plus a crew rack does not have a portrait layout worth building, and supporting one would halve the attention we can give the one that matters.

| Target | Points (landscape) | Scale | Layout |
|---|---|---|---|
| iPad 13" | 1376 × 1032 | @2x | Full: right sidebar 300 pt, minimap above it, crew rack along the sidebar foot, flight-card rack horizontal above the sidebar |
| iPad 11" | 1180 × 834 | @2x | Full, sidebar 268 pt |
| iPad, Stage Manager | down to 1024 × 768 | @2x | Full, sidebar 240 pt; below 1024 pt wide we switch to the Compact layout |
| iPhone 15/16 (and Pro) | 852 × 393 | @3x | **Compact:** no persistent sidebar. A bottom dock (72 pt) carries flight cards and the active build tab; the sidebar is a swipe-up sheet; the crew rack is a 28 pt strip above the dock; the minimap is a toggled overlay, not a permanent panel |
| iPhone Plus/Max | 932 × 430 | @3x | Compact, with the crew rack always visible |
| PC | ≥ 1280 × 720 | — | Full, sidebar scaled to 22% of width, clamped 280–420 px |

**Safe areas.** All persistent UI respects `safeAreaInsets`. In landscape that means the Dynamic Island / notch inset on the leading edge (up to 59 pt on iPhone Pro models) and the 21 pt home-indicator inset at the bottom. Concretely: the crew rack and dock sit **above** the home indicator with a 12 pt gutter; the minimap and resource bar sit **inboard** of the notch inset; and the marquee-select gesture is clipped to the safe rect so a drag that starts under the Island does not fight the system. The *world* renders edge to edge, under the insets — only interactive UI is inset.

We support **Stage Manager resizing** rather than declaring `UIRequiresFullScreen`, because Apple is deprecating that flag. The sim is untouched; a resize is a view relayout at one of three breakpoints, and the camera's ground coverage is preserved (we adjust FOV, not zoom, so a resized window shows the same world area at a different scale — resizing must never be a competitive advantage).

**Accessibility:** Dynamic Type support in all text UI up to a capped 1.4× (beyond that the sidebar cannot hold its labels and we substitute icon-only mode), a colour-blind mode that swaps team ramps for luminance-separated pairs and adds shape badges (§12.3), Reduce Motion (disables camera smoothing and overlay transitions), and a left-handed layout that mirrors the dock.

### 12.10 Thermal, battery and the governor

**Sustain target: a 30-minute match at the tier's frame cap with no drop below 80% of that cap after minute 10, on a device at 24 °C ambient, starting from cold, at 60% brightness.** This is the acceptance test, and it is run on physical devices in the nightly rig (§16.4), not in the simulator.

The governor reads `ProcessInfo.thermalState` and our own rolling frame-time p95, and steps through four profiles:

| Profile | Trigger | Actions |
|---|---|---|
| **Nominal** | `.nominal` | Tier defaults |
| **Warm** | `.fair`, or p95 frame > cap × 1.1 for 5 s | Render scale −15%, particles −40%, tether LOD thresholds −30%, shadow cascade off |
| **Hot** | `.serious` | Frame cap 60 → 45 → 30, bloom off, decal density −50%, minimap refresh 10 Hz |
| **Critical** | `.critical`, or battery < 10%, or Low Power Mode | Frame cap 30, render scale 0.7, particles 3,000, a one-line notice to the player |

**The governor never touches `KZ.Sim`.** Not the tick rate, not entity caps, not LOD tiers inside the link layer, not AI decision latency. Every one of those is shared lockstep state. What a throttled device gets is a worse-looking game at the same simulation fidelity, and if it cannot manage even that it is reported as a slow peer (§11.2) rather than silently desyncing.

**Battery target: ≥ 2.5 hours of continuous play on an iPad Air (Tier B) and ≥ 1.8 hours on an iPhone 13**, i.e. four to six full matches. Measured monthly from M2 on the same rig. The two levers that actually move this number are the frame cap and the particle budget, which is why both are in the governor.

**Memory.** iOS jetsam limits are the real ceiling, not the device RAM figure. Budgets, total resident:

| Tier | Device RAM | Resident budget | Of which sim | Of which textures/meshes | Of which audio | Engine + IL2CPP + slack |
|---|---|---|---|---|---|---|
| C | 3 GB | **1.05 GB** | 11 MB (Compact + 3 snapshots) | 480 MB (ASTC 6×6) | 90 MB | ~470 MB |
| B | 4–6 GB | **1.55 GB** | 24 MB | 780 MB | 140 MB | ~610 MB |
| A | 8 GB+ | **2.30 GB** | 24 MB | 1.25 GB | 180 MB | ~850 MB |

We respond to `didReceiveMemoryWarning` by dropping the replay snapshot ring to one, flushing the decal atlas, unloading the non-active faction's VO bank, and — if it repeats — dropping to the Critical thermal profile. We do not unload anything the sim owns.

**Download and install size.** Target **≤ 900 MB initial download (thinned)** and **≤ 1.6 GB installed**, using On-Demand Resources for campaign VO, the second faction's campaign assets, and maps beyond the starting four. This keeps the first-run experience under Apple's cellular-download threshold conversation and, more importantly, keeps the install from being the reason someone deletes the game.

---

## 13. Audio

FMOD Studio. **64 simultaneous voices on PC and Tier A, 32 on Tier B, 24 on Tier C.** Four buses (`Sim`, `UI`, `Voice`, `Music`) with ducking.

**iOS specifics.** `AVAudioSession` category `.ambient` with `.mixWithOthers`, so the game respects the silent switch and never stops the player's music — an RTS people play on a train must not seize the audio session. We handle `AVAudioSession.interruptionNotification` (call, alarm) by pausing FMOD and resuming on the `.shouldResume` option; a route change (headphones unplugged) ducks to 30% rather than continuing at speaker volume. Because the silent switch is respected, **every piece of critical information carried by audio must also have a visual channel** — the link-state stinger has the pip, the cut-off klaxon has the chevron and the minimap ping, the idle-crew chime has the rack pulse. That rule was good design on PC; on a muted iPhone it is mandatory. Banks are AAC on iOS and Vorbis on PC, built per platform, with the non-active faction's VO bank in On-Demand Resources.

**Hooks.** Audio subscribes to the sim event ring (§2.1) and to per-entity state, never to sim internals. Event categories:

- **Link state** is the highest-priority audio channel in the game and gets its own signature language: a clean two-tone drop when a flight goes amber, a harsher gated cut to black, a rising restore. These play at a fixed 2D level regardless of camera position for the *local player's own* flights, because losing a link is information that must not depend on where you happen to be looking.
- **Sortie lifecycle:** launch spool-up, arrival, terminal commit (a distinct rising tone the enemy also hears at reduced range — a deliberate tell).
- **Tether:** a faint spool whine tied to spool rate; a snap on cut.
- **Crew:** a soft chime when the last idle crew is consumed; a distinct tone when the rack goes fully amber.
- **Logistics:** cut-off klaxon, Dry glyph tick, node starvation.
- **Day/night:** a 20 s warning chime before each transition; ambience crossfades.
- **Barks:** ~90 lines per faction, priority-queued, hard-limited to one bark per 1.2 s to prevent the classic RTS bark pile-up.

**Voice limiting** is by priority class and distance, with per-class caps (e.g. max 6 concurrent drone loops, max 4 explosions) and a stealing policy that always favours the local player's own units. Drone loops use a single multi-instance event with parameter-driven pitch so 40 drones cost 6 voices.

Audio runs entirely in `KZ.View` on floats and is excluded from the desync hash. The `CosmeticOnly` RNG stream feeds all audio variation.

---

## 14. Tools and pipeline

Everything here is a Unity editor window or a headless C# CLI, written by the same engineers, in the same language. That is the main reason we chose Unity (§1.2).

**Map editor (in-editor, ships to players).** Paints the build grid's terrain classes, roads, forests, power-line corridors, dead ground; places Wreck Fields, Grid Taps, neutral structures, start positions; runs validators (symmetry check for 1v1, path connectivity per radius class, base-to-base distance within the 1,800–2,400 m rule, Grid Tap count, wreck field value totals). A map is a single `.kzmap` file: a compressed grid blob plus a placement list plus metadata, typically 180–500 KB. The shipped editor is the same tool we use, with the Unity-only bits behind a flag.

**Unit data hot-reload.** In the editor, `F5` re-parses the YAML tree, revalidates, and rebuilds the content blob in-place. Live matches accept the new data only at a match boundary or in **Sandbox mode**, where hot-reload is applied mid-match and the sim is explicitly marked non-deterministic (no replay, no hash) so designers can tune a number and watch it land in under two seconds. This split — instant in sandbox, boundary-only in real matches — is what keeps the content hash meaningful.

**Balance tuning dashboard.** A local web app (Postgres + Grafana, one small box) fed by telemetry from playtests, headless sims and, post-launch, opted-in players. Per-match rows plus per-event rows. Standing panels, chosen to match the gameplay spec's §16.2 target metrics: match length distribution, time-to-first-contact, time-to-Radar-Mast, time-to-first-fiber-sortie, time-to-T3, crew utilisation curve, **engagement value-swing histogram** (the gameplay spec's 18% cap — this panel is the single most important one and ships at M2), unit pick rate and win-rate delta, TP earned per minute by winner vs loser, salvage vs Grid Tap income share, tank appearance rate (open question 8 lives or dies on this number).

**Replay analysis CLI.** `kz-replay analyse <file>` produces a JSON event digest without rendering; `kz-replay verify <file>` resimulates and checks every checkpoint hash; `kz-replay diff <a> <b>` localises where two replays of the same commands diverged (our desync bisection tool). An in-game replay viewer with the per-engagement value-swing readout ships at M2 per the gameplay spec's commitment.

**Headless match runner.** `kz-sim --headless --ai veteran:kestrel --ai veteran:obsidian --map ridge_02 --seed N --out match.kzrp`. One match at ~40× real time on one core. This is the backbone of §16.3.

**Telemetry.** Opt-in, anonymised, one event batch per match end plus a heartbeat. Payload is the replay file (under 72 KB) plus a computed summary. That means every telemetry row is *reproducible* — we can replay any anomalous match locally, which is worth more than any amount of aggregate analytics.

**Cross-platform asset pipeline.** One source tree, per-platform overrides, no forked assets.

| Asset | iOS | PC |
|---|---|---|
| Albedo / mask textures | **ASTC 6×6** (2.67 bpp) | **BC7** |
| Normal maps | ASTC 5×5 | BC5 |
| UI atlases, icons | **ASTC 4×4** (8 bpp, no gradient banding on icons) | BC7 |
| Decals, particles | ASTC 6×6, alpha preserved | BC7 |
| Max texture dimension | 1024 (units), 2048 (terrain splat, UI atlas) | 2048 / 4096 |
| Meshes | 3 LODs, 16-bit indices, position quantised to 16-bit | same, LOD0 higher |
| Audio | AAC, 44.1 kHz | Vorbis q5 |
| Shaders | Metal, variants stripped to the Mobile renderer set | D3D11/D3D12/Vulkan |

ASTC is non-negotiable on iOS: every A-series GPU since the A8 supports it natively, it beats PVRTC on quality at the same rate and handles non-power-of-two and non-square textures, and it gives us a single block format across albedo, normal and UI at three different rates. The pipeline enforces it — a texture importer override that leaves an iOS platform entry at `Automatic` fails the content validator, because `Automatic` has historically silently chosen PVRTC or, worse, uncompressed.

Texture memory is budgeted per tier (§12.10) and enforced by a build-time report that fails CI if the resident set for a Tier C match exceeds 480 MB.

**Build matrix.**

| Target | Backend | Purpose | Cadence |
|---|---|---|---|
| iOS arm64 (device) | IL2CPP + Burst ARM64, Metal | Ship | Per-merge to `main` → TestFlight internal |
| iOS arm64 simulator | IL2CPP | Smoke only (no Metal perf signal) | Per-PR |
| macOS arm64 | IL2CPP + Burst ARM64 | **Determinism proxy for iOS** and dev workstation | Per-PR |
| Windows x64 | IL2CPP (ship) / Mono (editor) | Ship | Per-PR |
| Linux x64 headless | IL2CPP | Balance sims, CI | Nightly |

The macOS arm64 row is the important one: it runs the *same Burst ARM64 codegen path* as iOS on machines we can put in CI, which is how we get per-PR cross-architecture determinism coverage without a device farm in the loop. Device-level determinism is verified weekly (below).

**Build and CI.** GitHub Actions self-hosted runners: one Mac Studio (iOS + macOS builds, determinism, TestFlight upload via `altool`), one Windows (builds + determinism), one Linux (headless balance sims). Per-PR: compile all five targets, analysers (including the no-float-in-sim rule and the ASTC-override rule), unit tests, 20 determinism replays run **x64 vs arm64**. Nightly: full test suite, 200 determinism replays across both architectures, 1,500 headless balance matches, and the performance benchmark scenes with budget assertions run on **physical devices** (§16.4). Weekly: an automated on-device determinism build pushed through TestFlight to a rack of three parked devices (A13 iPhone, A15 iPad, M2 iPad) that resimulate 20 replays on launch and upload their hashes.

**TestFlight.** Internal (up to 100 testers, no review) from M1; external public link (up to 10,000, requires a lightweight Beta App Review per major build) from M4. Builds expire after 90 days, so we ship a TestFlight build at least monthly from M1 onward whether or not there is anything new — a lapsed build means testers lose access, and we have seen that kill feedback loops.

**App Store submission: content, ratings and the review risk.**

The setting is a fictionalised Eastern European front in 2027–28 with real hardware archetypes and no real insignia, people, places or casualty figures — that constraint came from `brief.md` and it also happens to be exactly what keeps us clear of the two guidelines that matter here:

- **Guideline 1.1.1 / 1.1.4** — "realistic portrayals of people or animals being killed, maimed, tortured, or abused" and defamatory or targeted content. We are a top-down RTS whose smallest unit is an abstracted squad marker a few millimetres across. There are no human figures rendered at a resolution where injury could be depicted, no blood, no gore, no ragdolls, and no death animation beyond a vehicle burning. This is already the art direction the camera angle demands; it is worth stating that it is also the compliance position.
- **Guideline 1.1.6 / 5.1** — no real-world political entities, no real conflict framed as such, no real serving people. The factions are fictional (Kestrel Pact, Obsidian Directorate), the geography is invented, and the campaign script is reviewed for this before VO is recorded, not after.

**Target rating: Apple 12+ ("Infrequent/Mild Realistic Violence"), ESRB T, PEGI 12, USK 12.** We are not designing toward 17+/M and we will cut content rather than accept it, because a 17+ rating on the App Store costs us discovery, the ability to be featured, and a meaningful slice of the tablet audience. The specific editorial rules that hold the line: no depiction of human casualties, no civilian-harm mechanics (the futures spec already removed the war-crimes meter for better reasons), misidentification friendly-fire reported as materiel loss, and no real-world atrocity references in mission text.

**Monetisation: premium, one purchase, no IAP at early access.** No ads, no loot boxes, no consumables — which also means no ATT prompt, and a privacy nutrition label that declares only opt-in diagnostics. Tasking Points are an in-match currency and can never be bought; that is a design rule with a compliance dividend.

**Review risk management:** a full submission dry-run at M5 against the real review pipeline (a private, feature-complete-enough build submitted for App Review and then withdrawn), so that the first time we meet a reviewer is not two weeks before launch. Budget two rejection-and-resubmit cycles into M6.

---

## 15. Modding and data exposure

**What is exposed:**

- The whole `content/` YAML tree: units, structures, abilities, links, damage tables, era packs, faction definitions, TP catalogue, AI weight tables.
- Maps (`.kzmap`) via the shipped editor.
- Art and audio via standard Unity asset bundles built by a provided mod template project.
- UI mods via a **Lua 5.4 sandbox** in `KZ.View` only — HUD layout, overlay styling, extra panels reading the read-only sim state.
- Campaign missions via the trigger graph (below).

**What is not exposed, and why:** *no scripting language that can touch the simulation.* A Lua VM inside a lockstep sim is a determinism hazard we cannot test our way out of — string hashing order, table iteration order, float arithmetic, garbage-collection-influenced behaviour, and an unbounded instruction budget. Instead, sim-affecting logic is a **trigger graph**: a data-defined set of `(conditions) → (actions)` nodes with a fixed, versioned vocabulary (~70 conditions, ~90 actions), evaluated at 4 Hz in a fixed node order. This is what our own campaign is built from, so modders get exactly the tool the designers use, and it is deterministic by construction. It is less powerful than Lua. That is the point.

```yaml
# content/campaign/kestrel/m06_starvation/triggers.yaml
- id: trg.first_cutoff_warning
  once: true
  when:
    all:
      - node_state: { node: obj.garrison_node, state: cut_off }
      - elapsed_gte: 120s
  then:
    - play_vo: vo.k06.cutoff
    - ping_minimap: { at: obj.garrison_node, colour: red }
    - set_objective: { id: obj.resupply, state: active }
    - grant_tp: 0
```

**Distribution: PC only.** Steam Workshop. A mod declares which content files it patches; the game computes a content hash including mods, so a mismatch is a lobby error rather than a desync. Modded matches are marked, excluded from Quick Match, and excluded from ranked telemetry.

**iOS gets no user-generated content at early access, and this is a platform decision, not a scope cut.** Three App Store constraints drive it:

1. **Guideline 2.5.2 / 4.7** — an app may not download and execute code. Our trigger graph and YAML are data, not code, which is defensible; but downloading arbitrary third-party content bundles that alter gameplay sits close enough to the line that a reviewer's judgement becomes a shipping dependency, and we will not put the launch date behind an argument we might lose.
2. **Guideline 1.2** — apps with user-generated content must provide content filtering, a report mechanism, blocking, and published contact information. That is a moderation function a nine-person studio does not have.
3. **Guideline 4.7's** requirement that the developer is responsible for all content in the app, which for a war-themed game means we would be underwriting whatever a modder names a unit.

What iOS gets instead: **curated content**. Community maps and balance mods that we vet ship inside app updates as first-party content, with attribution and, where appropriate, revenue share. This is more work per item and far less work in total than building moderation. If the mod scene is healthy at 1.0 we revisit with a curated in-app browser served from our own backend, which converts the problem from "arbitrary UGC" to "content we publish", and that is a solvable review conversation.

Cross-play is unaffected: modded matches are PC-only lobbies and are already excluded from Quick Match, so an iOS player never encounters one.

**Stability contract:** the YAML `schema:` version on every def. We commit to loading `unit/2` files for the life of early access; a breaking schema change ships with an automatic upconverter.

---

## 16. Testing

### 16.1 Determinism

The highest-priority test category from M0 onward.

- **Analyser gate.** A Roslyn analyser fails the build on `float`, `double`, `System.Math`, `UnityEngine.*`, `DateTime.Now`, `Dictionary` iteration without an ordered key, `HashSet` iteration, `string.GetHashCode`, LINQ over unordered sources, and `Parallel.For` inside `KZ.Sim`. This one gate catches about 80% of determinism bugs before they exist.
- **Replay verification.** A corpus of 200 replays (grown from playtests, AI matches and hand-built pathological cases: a 64-node tether snagging mid-flight, a Mothership dying with four children, simultaneous Crew Quarters destruction, BLACKOUT overlapping an EW Post) is resimulated nightly on **Windows x64, Linux x64 and macOS arm64** and asserted hash-identical at every checkpoint.
- **Cross-architecture pair test — the highest-value test in the project.** Every PR runs 20 replays x64-vs-arm64. Every night runs all 200. Every week, three parked physical devices (A13 iPhone, A15 iPad, M2 iPad) resimulate 20 replays via a TestFlight build and upload hashes. The macOS arm64 runner catches ~95% of what a device would catch, because it shares the Burst ARM64 backend; the device rack catches the remaining 5% — IL2CPP-specific codegen and any Apple-silicon errata.
- **Backend pair test.** The same corpus run under Mono, IL2CPP and Burst-disabled, asserting identical hashes. Burst-disabled matters: if Burst and non-Burst disagree, we have UB.
- **Interruption fuzz.** An iOS-only harness that injects synthetic `willResignActive` / `didBecomeActive` pairs at random points in a networked match, 200 times per night on the device rack, asserting that every fast-catch-up (§11.6) lands on the same hash as the uninterrupted peer.
- **Paranoid mode.** `-determinism-paranoid` hashes every system every tick. Used to bisect. Roughly 12× slower; still faster than real time headless.
- **Chaos harness.** An AI-vs-AI match with a fuzzer injecting random legal commands at 200 APM, run 500 times nightly with different seeds. This is how we find the order-of-death and order-of-event bugs that human play takes months to surface.

### 16.2 Simulation unit tests

Golden-master style, per system, ~600 tests at ship:

- **Link:** table-driven. For each (link kind, R, emitter set, position) expect a pip and a tick count. Includes the boundary-refinement cases at exactly R ± 11 and R ± 12.
- **Tether:** spool accounting under a scripted flight path; taut behaviour; cut at grace expiry; snag rate convergence over 10,000 seeded trials within 2% of the analytic expectation.
- **Mesh:** graph shapes with known parent assignments; child-drop on anchor death; re-parent within the grace window; Mothership hard drop.
- **Autonomy:** 100,000 seeded classifier trials per scenario, asserting the empirical pick distribution matches the model within 1% — specifically the gameplay spec's headline case (a tank behind 9 decoys survives roughly four attacks in five against Q 55).
- **Vision:** signature/threshold table cases, LOS DDA against hand-built occluder patterns, night multipliers, verified-kill truth table (all eight combinations).
- **Pathing:** flow-field correctness against a reference Dijkstra on 40 hand-built maps; formation slot assignment stability; road-preference regression (a Logistics-profile truck must prefer a 1.6× longer road route).
- **Economy/supply:** cut-off detection on hand-built graphs; drain arithmetic; salvage decay over 1,600 ticks.

### 16.3 Balance simulation

Nightly, 1,500 headless matches across the map pool and the difficulty matrix, ~8 core-hours. Outputs feed the dashboard automatically. Standing assertions that fail the nightly build:

- No unit has a pick rate below 4% or above 70% in matches over 15 minutes.
- Faction win rate at Veteran-vs-Veteran stays within 47–53%.
- Median match length stays within 20–30 minutes.
- No single engagement in the sampled set swings more than 18% of the loser's army value (the gameplay spec's §16.1 rule, tested, not asserted in prose).
- Median time-to-first-fiber-sortie stays within 7:30–10:30.

AI-vs-AI is not human play and we know it. Balance sims are a *regression detector* — "this patch changed something we did not intend" — not a balance oracle. The oracle is playtests plus player telemetry.

### 16.4 Performance and thermal testing

**The nightly performance rig is physical devices, not a simulator and not a desktop proxy.** Three parked devices on a powered hub in a temperature-logged cupboard: an iPhone 11 (Tier C), an iPad Air 5 (Tier B), an M2 iPad Pro (Tier A), plus the PC min-spec box. Builds are pushed via TestFlight and driven by a UI-automation harness.

Five scripted benchmark scenes, with hard budget assertions that fail the build:

1. **Peak sim.** Ruleset cap entities, cap tethers, 48 emitters, full mesh. Asserts sim tick ≤ **4.0 ms (Tier B)** / 3.2 ms (Tier C, Compact) / 3.6 ms (PC min).
2. **Link storm.** BLACKOUT fired over 600 linked drones simultaneously. Asserts link layer ≤ **0.55 ms on Tier B** and no frame over 2× the cap interval.
3. **Render load.** 1,200 visible units, full particle budget, Link View on. Asserts the tier frame cap is held.
4. **Pathing storm.** 200 ground units given simultaneous cross-map orders with two destinations. Asserts pathfinding ≤ 0.50 ms amortised and no field-cache thrash.
5. **Thermal sustain — the acceptance test.** A scripted 30-minute AI-vs-AI match played from a cold device at 60% brightness, logging frame time, `thermalState`, battery drain and governor transitions. **Asserts: no drop below 80% of the tier frame cap after minute 10, no `.critical` thermal state, and battery drain within the §12.10 target.** This test fails more often than any other and is the reason it runs nightly rather than before milestones.

Plus two static gates:

- **Silhouette distinctness** (§12.3): every unit rendered to a 1-bit mask at default zoom on **each layout target** (iPad 11", iPhone, PC 1080p), asserting a minimum pairwise Hamming distance within each layer. Running it per-layout is the mobile addition — a pair that separates on a monitor may collide on a phone.
- **Touch target audit:** every interactive element measured against the 44 pt minimum on each layout, and every persistent UI rect tested against the safe-area insets of the five device profiles. Fails the build on violation.

---

## 17. Milestones

24 months to early access, six milestones. Each names what it *proves*, because a milestone that does not retire a risk is a schedule. **Every milestone from M0 onward has a running build on a physical iPad**; nothing is "ported to mobile later", because the thing that kills mobile-primary projects is discovering the budget at month 18.

### M0 — "Tick" · Months 0–3 · Team 5
*Proves: we can run a deterministic fixed-point simulation at 32 Hz on an A14, with a Metal view on top, and that iOS↔PC cross-architecture determinism holds.*

Fix64 math library with ARM64 and x64 paths + tests. Entity table, component storage, snapshot/restore. Sim/view split with assembly enforcement and the Roslyn analyser. Build grid, terrain loading, one hand-made map. Flow-field pathfinding for one radius class. One unit (Recovery UGV), one structure (Command Post), move orders, collision. Replay record/playback. Xcode/TestFlight build pipeline from CI **in month 1**, not month 18. Determinism harness across Windows x64 and macOS arm64.
**Exit:** 400 units moving on one map on an iPad Air; a 10-minute replay reproducing hash-identically on Windows x64, macOS arm64 and a physical A14 device; sim tick under 1.2 ms on device.
*Team: 2 engine/sim, 1 gameplay, 1 tech art, 1 producer/design.*

### M1 — "Link" · Months 3–7 · Team 7
*Proves: the novel subsystem works, is cheap enough for a phone, and is readable on an 11-inch panel — the biggest project risk, retired first.*

The full control-link layer: jam grid and emitters, mesh graph and BFS, fiber tethers with spool/snag/trace, satellite capacity, autonomy classifier. Link View (uniform-driven), tether ribbons with LODs, the three-state pip, jam bubble decals. Six units (FPV Team, Fiber FPV, Scout Quad, EW Truck, Relay Mast, Recon Wing) and four structures. Vision, signature/detection, fog. First touch gesture set and the intent resolver (§3A.4). **Internal TestFlight begins.**
**Exit:** a playable 10-minute sandbox on an iPad where a designer drives drones through jamming, snags a tether in a forest, watches a mesh chain collapse when a relay dies, and beats an autonomous munition with a decoy field — at ≤ 0.55 ms link cost with 600 drones on an A15. An internal playtest votes on whether the link layer is *legible on a tablet*; a "no" is a design pivot we can still afford.
*+1 engineer (link/sim), +1 artist.*

### M2 — "Sortie" · Months 7–11 · Team 9
*Proves: the C&C loop plus the sortie model is a game, on a touchscreen, at a human action rate.*

Hangar stock, flight cards, crew pool and rack, the full sortie state machine, veterancy. Sidebar, build queues, placement, power — in **both** the full iPad layout and the iPhone compact layout. Tap-tap sortie flow, radial inspect, order ribbon with the exposure preview. Assist levels. Salvage economy, Grid Taps, TP and the TASKING catalogue. Supply, Dry, cut-off. Day/night. Tier 1 + most of Tier 2 (16 units). Threat field. Replay viewer with the value-swing readout. Balance dashboard v1. First battery and thermal measurements.
**Exit:** two humans play a complete 22-minute 1v1 on two iPads over LAN and it is fun without anybody explaining it; **touch APM at Standard assist measures ≤ 60** for equivalent board states against mouse players. Weekly external playtests begin.
*+1 gameplay engineer, +1 UI/UX (touch specialist — this hire is the difference between a port and a design).*

### M3 — "Lockstep" · Months 11–15 · Team 11
*Proves: it ships as a cross-play multiplayer product on a phone in a train tunnel.*

Networking: turn scheduling, adaptive p99 delay, desync detection and localisation, our relay in three regions, Game Center and Steam identity behind `IPlatformServices`, link codes. **Cellular tolerance, backgrounding, fast-catch-up and reconnection** (§11.6–11.7), plus the interruption fuzz harness. Full Tier 2 roster and structures. Formations, stances, abilities. Three maps. Audio pass 1 with the iOS session handling. Settings, rebinding, accessibility pass 1.
**Exit:** 200 external matches over the internet including **at least 60 on cellular and 60 cross-play iOS↔PC**, with zero unexplained desyncs; a match survives a 20-second phone call at both ends; nightly balance sims green for two consecutive weeks.
*+1 network/platform engineer, +1 audio/technical designer.*

### M4 — "Doctrine" · Months 15–19 · Team 12
*Proves: the factions are different games, something other than a human can play it, and it sustains 30 minutes without cooking the device.*

Tier 3: Uplink Terminal, Autonomy Lab, Mission Control, ARCLIGHT, BLACKOUT, Mothership, Mid-Range Striker, Autonomous Munition, Swarm Flight, Interceptor Battery, Designator Team. Full faction asymmetries. The utility AI, all four layers, four difficulty tiers. Three more maps (six total). **Device tiering, the thermal governor, and the physical-device performance rig.** The one budgeted engine upgrade. Era pack infrastructure and Era II. **External public TestFlight opens.**
**Exit:** Veteran AI beats a competent internal player 40% of the time without cheating; faction win rate in nightly sims 47–53%; **the 30-minute thermal sustain test passes on all three device tiers**; both supers land and are survivable.
*+1 AI engineer.*

### M5 — "Content" · Months 19–22 · Team 12
*Proves: there is a reason to buy it, a reason to keep playing it, and that Apple will let us.*

Campaign act 1: six missions (three per faction) via the trigger graph, with VO, briefings, persistent crews. Map editor shipped to PC players. Modding pipeline, mod template, Workshop (PC). Two more maps (eight total). Audio pass 2. UI polish, tutorial (touch-first), replay sharing. Telemetry live. On-Demand Resources split and download-size pass. **App Review dry-run submission**, age-rating questionnaire, privacy labels, App Store and Steam page assets.
**Exit:** a first-time player reaches the end of mission 3 on an iPad without external help, across 20 fresh testers; the dry-run submission clears review or returns feedback we can act on; installed size under 1.6 GB.

### M6 — "Early Access" · Months 22–24 · Team 12
*Proves: it runs on a stranger's phone.*

Performance and thermals to budget on all tiers. Localisation (EN/DE/FR/ES/PL/UA/pt-BR/zh-Hans, text only). Crash reporting, support tooling, day-one patch pipeline. Public beta with 2,000 TestFlight testers and 500 Steam playtest keys across two rounds. Balance from real telemetry. Two rejection-and-resubmit cycles budgeted.
**Launch shape:** **Steam Early Access and a paid App Store release on the same day.** Apple has no Early Access programme, so the iOS build ships as a 1.0-priced premium app whose store description, first-run screen and roadmap state plainly that it is in early access; the public TestFlight track continues alongside it for the beta branch. Post-EA roadmap published: campaign act 2, Tier 4 / Release Authority, era packs III–IV, ranked ladder, curated iOS content.
**Exit:** ship.

**Contingency.** Three things are cut first if we slip, and none costs us the thesis: **campaign act 1 drops from six missions to four** (M5); **the utility AI degrades to three scripted doctrines** (§10.5); and **the iPhone compact layout slips to a post-EA patch, launching iPad + PC only** (M2/M6) — the iPhone is the hardest layout and the least of the three audiences, and cutting it late is cheap because the sim, the commands and the assists are all shared. Three things are never cut: determinism, the link layer, and the 30-minute thermal sustain test.

---

## 18. Risks and mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **Cross-architecture desync.** iOS arm64 and PC x64 diverge in the fixed-point library or in Burst codegen. | Medium | Critical | Integer-only math with single-instruction high-multiply on both ISAs; per-PR x64-vs-arm64 replay tests from M0; macOS arm64 CI runner sharing the iOS Burst backend; weekly on-device verification; hierarchical hashing that localises to a system and an entity. This is risk #1 on the project and it is tested on day 30, not month 12. |
| 2 | **Thermal throttling makes long matches unplayable** on a phone or a fanless iPad. | **High** | **Critical** | The 30-minute sustain test is a nightly gate from M4 and a never-cut exit criterion. Budgets are written against *sustained*, not burst, clocks. The governor has four profiles and touches only presentation. Escape hatches in order: drop Tier C to 30 fps (already default), shorten the Compact map pool, and — last — make 2v2 Tier A only. |
| 3 | **The link layer is unreadable on an 11-inch panel.** | Medium | Critical | It is M1, not M4 — we find out at month 7 with 17 months left, on the actual device. Three-state pip only, bubbles as terrain, tether always drawn at a panel-relative width, no submenus. The M1 exit criterion is explicitly a tablet legibility vote. |
| 4 | **Touch cannot carry an RTS command load.** Players run out of fingers. | **High** | High | The flight-card model is already a two-step target-last interaction, which is the most touch-friendly shape an RTS action can have. Intent resolution with a 44 pt scored pick, auto-formations, radial inspect, assist levels. Measured at M2 against a ≤ 60 APM target; if we miss, the fix is fatter commands (a count-carrying sortie, a "commit flight" macro), not more assist. |
| 5 | **iOS suspension breaks matches** routinely enough that multiplayer feels unreliable. | **High** | High | Background-task assertion + buffered turns + 16× fast-catch-up covers everything under 25 s, which is the overwhelming majority of interruptions. Shared pause in 1v1, hold-AI in 2v2, two pauses per match. Interruption fuzz harness runs 200 injections nightly on device from M3. |
| 6 | **Link layer too expensive** at 2v2 on a mobile core. | Medium | High | Three-tier LOD with hard budgets; grid + boundary-refine instead of per-emitter-per-unit; benchmark scene 2 on physical devices. Escape hatch: tier A budget 128 → 96 → 64, mesh BFS to 4 Hz, and 2v2 restricted to Tier A. |
| 7 | **AI cannot play this game** (gameplay open question 6) — and now must fit in 0.6 ms on two cores. | Medium | High | Layered design degrading to three scripted doctrines; tactical and reflex layers shared by both paths. Costed at 5.5 engineer-months. The mobile budget is met by running strategic at 1 Hz and operational at 2 Hz, which is where the cost is. |
| 8 | **The hangar/flight-card model reads as a chore** (gameplay open question 1). | Medium | High | Tested at M2 with external players, on touch, where it is *more* natural than on mouse. Fallback (air units spawn on the map, crews become a conventional pop cap) is a two-week sim change because §6 keeps `Sortie` decoupled from `Producer`. |
| 9 | **App Review rejection** delays launch, or a rating comes back 17+. | Medium | High | Content rules written into the art and script bible, not applied at the end. Dry-run submission at M5. Two resubmit cycles budgeted in M6. No UGC, no IAP, no background-mode entitlements — we have removed every discretionary review surface we can. |
| 10 | **Scope: 22 units, 2 factions, 2 campaigns, three layouts, two platforms, an editor, modding** for ≤ 12 people. | **High** | High | Campaign act 1 is six missions, not eighteen. Naval, deep strike and manned aviation already cut. iOS modding cut. Milestone contingency names the iPhone layout as a late, cheap cut. |
| 11 | **Unity licensing or policy change.** | Low | High | The sim has zero Unity dependencies by construction. A move costs the view layer and the tools — painful, 5–7 months on two platforms, survivable — and is not a rewrite. Main dividend of §2.1. |
| 12 | **Memory pressure / jetsam** on 3 GB devices. | Medium | Medium | Compact ruleset, 480 MB texture budget enforced at build time, 1-snapshot ring under memory warning, ODR for campaign assets. Escape hatch: drop Tier C to 2-map rotation in skirmish. |
| 13 | **Pathfinding cost with 2v2 logistics traffic.** | Medium | Medium | Sector-limited flow fields, LRU cache sized 12 MB desktop / 5 MB mobile, three cost profiles. Benchmark scene 4. Escape hatch: path grid 8 m → 12 m for `light`/`heavy` only. |
| 14 | **Fiber tether memory and rendering** at high concurrency on a tiler. | Medium | Medium | 512/192 tether cap, 64-node cap with decimation, two-segment-per-tick snag evaluation, four render LODs, single additive draw. Measured 0.35 ms on A14. |
| 15 | **Determinism vs. modding.** A mod desyncs a match. | Medium | Medium | No sim-side scripting, ever. Trigger graph with a fixed vocabulary. Content hash includes mods. Modded matches are PC-only and excluded from Quick Match. |
| 16 | **Burst, Xcode or iOS upgrade changes codegen** mid-project. | Medium | Critical | One budgeted engine upgrade at M4 gated on the full determinism corpus. Xcode and iOS SDK bumps are treated as engine upgrades and run the same gate. No engine upgrades after M4. Integer-only math means the exposure is codegen bugs, not semantics. |
| 17 | **Telemetry volume** overwhelms one small box. | Low | Low | One ≤72 KB replay per match; 20,000 matches/day is 1.4 GB/day. Sample at 20% above that threshold. |

---

## 19. Open questions

1. **Is 250–500 ms input delay acceptable for the hangar model specifically, and does cellular make it worse?** In classic C&C the delayed action is a unit already on the map. Here, tapping a flight card *creates* units. At 500 ms on LTE, the gap between "tap" and "airframes on the pad" may read as unresponsive even with full view-side confirmation. Fallback: a view-side "launching" ghost on the pad, pure decoration. Cheap; measure at M3 on real cellular, not in a lab.

2. **Does 2v2 belong on mobile at all at early access?** It is the mode the pin economy is designed for, and it is also the mode with the largest entity count, the longest matches, the worst thermal profile and the no-shared-pause interruption rule. If the M4 sustain test says 2v2 cooks a Tier B iPad in 25 minutes, the honest answer may be "2v2 is Tier A and PC only at EA". Decide from the thermal rig, not from the roadmap.

3. **Where exactly does the iPhone layout stop being a compact version and start being a different game?** At 852 × 393 pt the sidebar is a sheet and the minimap is an overlay, which means base-building on a phone is genuinely more laborious than on a tablet. Options: accept it; add a phone-only "build wheel"; or make the phone build a skirmish/campaign-focused experience where base management is lighter. This is the largest unresolved *design* question the platform change creates, and it belongs to the UI spec as much as to this one. Prototype at M2.

4. **Should the touch assist levels exist on PC at all?** Fairness says yes (identical ceilings across platforms). Player culture in this genre says a "Standard assist" toggle will be read as training wheels and disabled by everyone, which then makes it an implicit mobile-only advantage in the opposite direction. Leaning toward: assists on by default everywhere, named as *quality-of-life* rather than *assist*, with per-item toggles. Revisit with M3 cross-play telemetry.

5. **Should the threat field be shared in 2v2?** It is a per-team memory. Sharing it is one line and is probably right for the recon-specialist fantasy, but it halves the value of a dedicated spotter. Needs a playtest, not an argument.

6. **Tether node spacing: 12 m or 8 m?** 8 m aligns the tether to the build grid, making snag lookup a single read with no straddle case, at the cost of 50% more nodes and a 96-node cap — which on Compact's 192-tether budget is a real memory and vertex cost. Decide at M1 with real flight data on device.

7. **Do we need a fourth LOD tier for link resolution in 2v2?** With two Obsidian players the mesh graph could reach 900 nodes. The BFS is depth-limited and cheap; the candidate-neighbour query is O(N × 24). If 2v2 benchmarks miss budget, either a per-team BFS at 4 Hz or a cached neighbour list invalidated on movement thresholds. Measure at M3 on an A15.

8. **Is compile-time era patching right, or do we need runtime switching?** Compile-time is faster, hashable and simpler, but a lobby cannot change era without a ~400 ms content rebuild — which on an A13 is closer to 1.2 s and may be visible. Likely answer: pre-compile all 12 valid era stacks at build time and select by hash. Confirm at M4.

9. **How much of the map editor do we ship, and only to PC?** The editor exposes terrain classes that are balance tools (snag rates, dead ground). It is also a desktop-shaped tool with no touch design budget. Current intent: PC only, with a validator that warns on maps outside our authored parameter envelope, and maps playable on iOS in private lobbies only. Decide at M5.

10. **Does the exposure preview need to be deterministic?** Today it is a read-only query outside the hash, correct because it has no sim effect. But the AI calls the same function *inside* the sim, and the High-assist auto-retreat (§3A.5) is a view-side consumer that emits commands. Keep the function fixed-point regardless — the cost is nil and it removes the question permanently.

11. **Cross-play input parity: preference or filter?** We ship it as a soft preference and collect win rate by input method. If the data shows a persistent gap above ~4% at equal rating, the options are a hard filter in Quick Match (fragments the population), a rating adjustment by input method (opaque), or a design fix to the assist layer (best, slowest). No decision until M6 telemetry.

12. **Tier 4 ("Release Authority") sim shape.** The futures spec makes it a *permission*, not a link — a one-time irreversible doctrine choice granting an Autonomous stance to every squadron. Structurally: a team-level flag, a per-unit stance, and an Engagement Box drawing tool — which on touch is a drag-rectangle and is actually easier than on mouse. Open question is whether `Doctrine` should be reversible for the AI's benefit. Current intent: irreversible for everyone, with the AI treating it as a high-hysteresis decision over a 90-second window.

13. **Campaign saves on iOS.** Snapshots give us save/load free, but a 6 MB save per slot across many slots is real storage on a phone, and iOS will not thank us for it. Delta-compressing against the mission start snapshot lands under 400 KB. Worth doing at M5; on iOS it may be worth doing regardless of whether anyone complains.
