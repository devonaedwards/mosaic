# Audit: research that was never wired into the simulation

A sweep of the research corpus (`docs/research/*`, `docs/FINDINGS.md`,
`docs/PHYSICS-TOOL.md`, `docs/SCALE.md`) against `src/KZ.Sim/`, looking for
recommendations that are specific enough to check and are not doing anything at
runtime.

Method, so the gaps in this audit are visible too:

- Read every "verdict on the game's numbers" / "recommendation" / "what to build
  instead" section in the corpus and traced each numbered recommendation into the
  code by name.
- Mechanical checks: every `public` member in `KZ.Sim` counted for call sites;
  every `SimEventKind` counted for pushes; every `UnitDef` field counted for
  readers outside `Defs.cs`; every enum value counted for constructions; every
  `ComponentMask` flag counted for tests.
- Everything below was grepped under at least two plausible names before being
  called missing. Where I could not trace something, it is in **Not traced**
  at the bottom rather than asserted either way.

---

## Summary

Counts are of distinct checkable items, not of the numbered findings — several
findings carry more than one item (F10 alone is three contradicted constants).

| Category | Count |
|---|---|
| **DEAD** — implemented, never called, or called with no effect | 34 |
| **MISSING** — never implemented | 32 |
| **CONTRADICTED** — the code does what the research explicitly says is wrong | 14 |
| **PARTIAL** — wired for some cases and quietly not for their siblings | 9 |
| **WIRED** — traced and genuinely working (listed in §F so this is falsifiable) | 14 |
| Unactionable here — campaign-layer, or blocked on something else missing | 10 |
| Harness gaps that corrupt the same conclusions (§E) | 2 |

34 findings, F1-F34, ordered by consequence.

### The three to fix first

1. **F2 — the two-second track hold does not exist.** `SimConstants.TrackHoldTicks`
   has zero call sites. `FINDINGS.md` §21 states it as implemented and draws the
   "nothing is a switch" conclusion on top of it. Every marginal contact re-rolls
   from scratch every tick, so an acoustic contact at the fringe flickers at 52%
   per tick: a mount stalls on the ticks its target vanishes, and thrashes its
   acquisition timer whenever a second candidate is in range. This affects every
   edge-of-envelope engagement in the balance harness, is roughly twenty lines to
   fix, and is the cheapest large correction available.
2. **F4 — the Gun Mount has no traverse rate and no magazine.** Both mechanics
   exist; the only unit that carries either is the Interceptor Battery. This is
   the unit `FINDINGS.md` §2, §13, §15, §16 and §18 were all measured against.
   §18's conclusion ("the re-laying penalty is badly understated") is wrong: for
   that unit the penalty is exactly zero, because `SlewTicks` returns early on a
   zero traverse rate. Two numbers in `Defs.cs`, then re-run the vertical and
   stacking experiments.
3. **F3 — salvage is spawned, decays, and is never collected; there is no
   materiel income of any kind.** `SimEventKind.SalvageCollected` is never
   pushed; `Players[].Materiel` only ever decreases. `SimConstants` calls salvage
   "the heart of the economy". As shipped, the economy is a one-way drain from a
   4,000 starting balance, which means no economic conclusion drawn from the
   simulation is about an economy.

---

# A. The simulation silently assumes something the research says is false

These are first because they corrupt conclusions rather than merely omitting
features.

## F1. Terrain occlusion — no line of sight anywhere

- **Research:** `radar-rf.md` finding 13 and §2B.5 — *"a single 15 m treeline
  1 km from a 10 m mast blanks everything below 35 m out to 5 km"*, occlusion
  "should be absolute" for ground targets. `acoustic.md` §4.5 and §5a — an 8 dB
  (≤250 Hz) / 15 dB (≤1 kHz) / 22 dB (>1 kHz) diffraction loss keyed off a
  `terrain_blocked` flag, producing the asymmetry *"a ridge line substantially
  hides a quadcopter and barely hides a Shahed"*. `SCALE.md` §"Different sectors
  should play differently" states as an existing capability that in forest
  "sightlines collapse".
- **Code:** `Core/Terrain.cs:91` `BlocksGroundSight()` — zero call sites.
  `Core/World.cs` contains every line of detection code and makes **zero** calls
  to `Terrain` (verified by grep for `Terrain.` in that file). The only `Terrain`
  consumers in `KZ.Sim` are `MovementSystem.cs:73-82,154`,
  `NavigationSystem.cs:165` and `TetherSystem.cs:92,281`.
- **Category:** MISSING (function DEAD).
- **To wire:** large. A per-pair ray-march over the tile grid inside
  `ComputeDetection`, or a cheaper abstraction — the research's own framing in
  `FINDINGS.md` §29 is that a per-pair per-channel test may not fit the tick
  budget on a phone.
- **Why it matters:** already recorded as FINDINGS 29, restated here because the
  audit found two further consequences. First, `Terrain.BlocksGroundSight` is the
  only terrain rule in the game that is frequency-independent; the acoustic
  research asks for a *frequency-dependent* term, which the current single-number
  acoustic channel cannot express even if occlusion existed. Second, every
  balance experiment runs on `t.Fill(TileClass.Open)` (see **F34**), so even a
  correct occlusion model would change none of the recorded numbers without new
  maps.

## F2. The track hold is documented, believed implemented, and dead

- **Research:** `acoustic.md` §7 Recommendation 4 — *"Give acoustic a longer
  track hold — 4 s rather than 2 s."* `FINDINGS.md` §21 — *"and a two-second
  track hold so a marginal contact does not strobe."*
- **Code:** `Core/SimConstants.cs:300` `TrackHoldTicks = 64` — **zero call
  sites** anywhere in `src/`. `World.RebuildDetection()` (`World.cs:791-813`)
  clears the whole cache every tick and recomputes from scratch; `Reaches()`
  (`World.cs:908-930`) keys its edge-of-envelope roll on
  `(targetIndex, Tick, channel)`, so the roll is *deliberately* re-drawn every
  tick with no memory.
- **Category:** DEAD.
- **To wire:** small — a per-team, per-entity `lastSeenTick` array and one
  comparison in `RebuildDetection`; the channel-dependent hold the research asks
  for needs the winning channel recorded too.
- **Why it matters:** `FINDINGS.md` §21's headline ("nothing is a switch") is
  half-true — the solid/intermittent band exists, the hysteresis does not. A
  target at 0.9× acoustic reach is currently detected 52% of *ticks*, i.e. it
  flickers 16 times a second. `CombatSystem.CanEngage` (`CombatSystem.cs:264`)
  gates firing on `IsDetectedBy`, and `StepOne` restarts the acquisition timer
  whenever the chosen target changes (`CombatSystem.cs:55-67`). With one
  candidate in range a mount simply stalls on the ticks its target vanishes;
  with two, `BestTargetInRange` returns a different handle on the missed tick and
  the timer restarts. Every "how many drones does it take" number in FINDINGS
  13-18 was measured with this behaviour, and the multi-drone cases are the ones
  it distorts most.

## F3. Salvage is never collected, and there is no income

- **Research:** `economics.md` §"Concrete recommendations" 1 and §"Is 'airframes
  are cheap and crews are the cap' the right model?" — **money gates production,
  crews gate employment**, two valves in series. `FINDINGS.md` §24 repeats it and
  says "worth adding before the economy is tuned".
- **Code:** `World.SpawnSalvage` (`World.cs:331-347`) creates the pile and pushes
  `SalvageDropped`. `World.UpdateSalvage` (`World.cs:518-529`) decays it and
  deletes it. `SimEventKind.SalvageCollected` — **never pushed** (0 occurrences).
  `Players[].Materiel` is written in exactly two places: `World.cs:91`
  (initialised to 4000) and `SortieSystem.cs:132` (`-=` the launch cost).
  `Players[].TaskingPoints` is credited at `World.cs:682` and read by nothing but
  the state hash.
- **Category:** DEAD (salvage), MISSING (production valve).
- **To wire:** small for collection (a proximity check for a friendly
  `Recovery UGV` against `ComponentMask.Salvage`, `Materiel +=`, push the event);
  large for the production valve, which needs a stock/build-queue system that does
  not exist.
- **Why it matters:** `SimConstants.cs:123-130` calls salvage "the heart of the
  economy: the richest ground on the map is always the corpse of the last fight".
  It is inert. The `Recovery UGV` (`Defs.cs:318-325`) is named for a job it cannot
  do. And because Tasking Points are never spent, the verified/unverified kill
  distinction — which `World.cs:664-670` describes as making "the reconnaissance
  layer earn its keep twice over" — currently pays into a write-only counter.

## F4. The gun mount has neither traverse nor a magazine

- **Research:** `point-defence.md` §"Where the existing numbers break" 5-7 —
  traverse should be ~150°/s for a light AI EO turret, and the band-change cost
  should be mechanical time plus a 0.5 s re-acquisition charge.
  §"Suggested replacement units" — AI Gun Turret: **5 shots / 20 s reload**.
  §Q3 — "MG turret: 3-6 engagements (100-round belt)"; saturation should be
  "effector-channel exhaustion", not detection failure.
- **Code:** `Defs.cs:278-296` Gun Mount sets neither `TraverseDegreesPerSecond`
  nor `AmmoCapacity`, so both default to 0. `World.cs:233-240` therefore gives it
  `TraverseBamPerTick = 0` and `AmmoCapacity = 0`.
  `CombatSystem.SlewTicks` (`CombatSystem.cs:207-227`) returns 0 immediately on a
  zero traverse rate — **so the +20-tick elevation-band penalty is never applied
  to it either**. `CombatSystem.cs:75` skips all reload accounting when
  `AmmoCapacity == 0`. The only unit in the catalogue with either field is the
  Interceptor Battery (`Defs.cs:424-425`).
- **Category:** PARTIAL (both mechanics exist, fitted to one unit of eleven
  shooters).
- **To wire:** trivial — two fields on one def, plus per-class values for the
  rest. Then re-run `vertical` and `stacking`.
- **Why it matters:** this is the single most consequential wiring gap in the
  combat model, because the Gun Mount is the unit every turret finding was
  measured against. `FINDINGS.md` §18 concludes *"That is now modelled - traverse
  at ninety degrees a second, plus a two-thirds of a second penalty for changing
  height band - and it is too cheap to matter"* and guesses the penalty is
  "badly understated". It is not understated; it is zero. The entry's own
  instruction was to revisit it when real traverse figures arrived — they have
  (`point-defence.md` §"Azimuth and elevation rates by class"), and the reason the
  tactic failed was never the figure. Likewise `FINDINGS.md` §13-16's
  "can N drones take a turret" tables were all run against a turret with
  infinite ammunition, which is precisely the condition the research says makes
  saturation impossible to model.

## F5. Navigation error is computed and consumed by nothing

- **Research:** `navigation-denied.md` §5 Tier 0 — *"Demote these to autonomous
  selection **and** apply an accuracy penalty that grows with penetration depth.
  This is the unit that makes the geofence hurt."* 3% of distance flown; at 12:1
  compression, 3% over 20 km real ≈ 50 map metres of aimpoint error.
- **Code:** `NavigationSystem.cs` computes `nav.ErrorMetres` at lines 50, 71, 95
  and 119. Readers of `ErrorMetres` outside `NavigationSystem.cs`,
  `Components.cs` and the initialiser at `World.cs:153`: **only
  `KZ.Tests/SimTests.cs`**. Nothing in `CombatSystem`, `MovementSystem` or
  `World.ApplyDamage` reads it.
- **Category:** DEAD.
- **To wire:** small — offset the impact point (or roll a miss) by `ErrorMetres`
  in `ResolveDirectFire` for one-way airframes; medium if the drift should also
  steer `MovementSystem` toward a wrong destination.
- **Why it matters:** the whole `NavigationSystem` — three tiers, the lock
  cliff, the re-acquisition distance, the celestial drift-rate distinction that
  `Components.cs:48-63` argues for at length — produces a number with no
  consumer. The tests assert the number, so the suite is green. The one-line rule
  the system exists to express, *"crossing the border costs you the operator; it
  costs you your position only if you did not bring a map"*, currently has no
  second clause: a dead-reckoning drone and a scene-matching drone arrive equally
  accurately.

## F6. The reference-imagery resource loop has neither end connected

- **Research:** `navigation-denied.md` §6 — imagery as a **coverage** resource:
  acquired by reconnaissance sorties over ground you do not control, invalidated
  by events the player watched happen (bombardment), never by a timer.
- **Code:** `ReferenceImagery.GrantAround` (`ReferenceImagery.cs:73`) and
  `ReferenceImagery.Invalidate` (`ReferenceImagery.cs:94`) both have **zero
  callers in `KZ.Sim`** — only `KZ.Tests`. No sortie grants coverage; no
  explosion invalidates it. `Territory` is likewise never populated by anything
  in `KZ.Sim`, `KZ.Headless` or `KZ.Balance` (grep for `SetVerticalBorder`, `Fill`,
  `SetCell`: tests only).
- **Category:** DEAD.
- **To wire:** small — call `GrantAround` from a recon airframe's per-tick step
  and `Invalidate` from `ApplyDamage` above a damage threshold. The hard part is
  deciding which units count as reconnaissance.
- **Why it matters:** with coverage permanently empty, `CanMatchHere`
  (`NavigationSystem.cs:163`) always returns false, so **the `SceneMatching` tier
  can never hold a lock in any shipped scenario**. Five catalogue entries carry
  `NavAid.SceneMatching` and all five behave exactly as dead-reckoning airframes.
  Combined with the empty `Territory` (every cell owner 0), `IsDenied` returns
  true for every unit everywhere including over its own base, and
  `LinkResolver.InSatelliteCoverage` returns false everywhere — the condition
  `FINDINGS.md` §26 noticed in the tests is also true of the headless mission and
  every balance experiment.

## F7. Radar cannot see ground targets, and there is no Doppler notch

- **Research:** `radar-rf.md` finding 9 — *"Radar can see ground targets, and the
  game says it cannot… The tank's `—` in the radar column is wrong. The correct
  constraint is Doppler: a stationary vehicle is invisible, a moving one is
  loud."* Finding 12 — *"The Doppler notch is the missing mechanic"*: below about
  1.5 m/s radial velocity a target is not detected at all, whatever its RCS.
  §5 recommends tank radar signature 94 moving / 0 stationary.
- **Code:** `World.cs:857` gates the entire radar channel on
  `layer != Layer.Ground`; `World.DetectionRangeFor` returns `Fix.Zero` for a
  ground target at `World.cs:1141`. `Main Tank` (`Defs.cs:390-406`) has no
  `SigRadar`, i.e. 0. `Entities.Velocity` is written by `MovementSystem.cs:87`
  and **read by nothing**, so radial velocity is not available anywhere.
- **Category:** CONTRADICTED (ground blindness), MISSING (Doppler notch).
- **To wire:** small for both. Remove the layer gate, give ground units a radar
  signature, and add one radial-velocity test in `Reaches` using the already-
  written `Velocity` array.
- **Why it matters:** two design conclusions rest on it. The radar mast is
  currently an air-defence-only building, which makes `FINDINGS.md` §25's
  conclusion ("early warning cannot matter when nothing can shoot at the range
  the warning arrives") narrower than it needs to be — a radar that sees moving
  vehicles is a ground-reconnaissance asset with a different value entirely. And
  without the notch, "low and slow is hard" is not modelled at all, so hovering
  and tangential flight are not tactics.

### WIRED. Measured in FINDINGS 38.

**Done, and the entry's diagnosis held.** Probed first: a Radar Mast against a
Main Tank read `SigRadar` 0, radar reach 0 m and `IsDetectedBy` false at every
range on the map, exactly as claimed. The line numbers had moved (`World.cs:857`
is a mine's trigger radius now) but both gates were where the entry said.

The layer gate is gone from `ComputeDetection` and from `DetectionRangeFor`, and
what replaces it is `World.RadarDetectionScale` — §2B.3's radial-velocity table,
read straight: under 1.5 m/s no contact at all, 1.5–4 m/s reach ×0.40 and
reliability ×0.30, 4–10 m/s ×0.80 and ×0.70, full above that. `Reaches` takes a
reliability scale so the second column is not folded into the first, because a
target barely clear of the clutter is intermittent as well as short. Ground
targets take a third altitude band at ×0.40, which is §4's nap-of-the-earth
figure applied to the layer that is definitionally in the clutter.

Seven ground units get a radar signature. Only the tank has a published figure —
§5's 94 moving, and §5's "0 stationary" is the notch doing it rather than a second
field — and the other six say in the comment that they are designer estimates
stepped off it. The two dismounted teams keep a zero on purpose: they walk at
1.5 m/s, which is the floor of the notch, so a signature for them would be data
nothing could ever read.

**One correction to the entry, and one to the brief.** `Entities.Velocity` is not
"read by nothing" — `MovementSystem.InterceptPoint` has read it since FINDINGS 36.
And `radar-rf.md` §5's table is on `2 × dBsm + 60` while `RadarReachTable`'s doc
comment says `2 × dBsm + 80`; the catalogue follows §5, so the tank is written at
94 to stay consistent with the fifteen airframes already in it. FINDINGS 38 has
the arithmetic and says why that mismatch is AUDIT F25's to close, not this one's.

**Measured, and the honest part: in the shipped scenario it changes nothing.**
The player's Radar Mast is 11,402 m from the nearest defender vehicle and reaches
8,044 m against a moving tank, so contact counts across ten play-minutes are
identical to the digit. The mechanic is live and tested through `Spawn`/`Step`;
the scenario's geometry does not reach it. A forward radar the player has to push
up and defend is a scenario change, and it is the next thing to try.

## F8. Radiating radars are invisible on the ESM channel, jammers nearly so, and nothing can switch off

- **Research:** `radar-rf.md` §3.6 — *"This is the strongest and best-sourced
  material in the whole document, and the game currently ignores it."* Three
  numbered recommendations: (1) a transmitting jammer detectable across
  effectively the whole map; (2) **a radiating radar at RF ≈ 90, "the clearest
  single omission in the signature table"**, giving 3-6× its own radar reach;
  (3) make emitting a choice. `decoys-masking.md` §3.3 and §7.6 add a transmit
  **duty cycle** rather than a static signature.
- **Code:** `World.EffectiveSignature` (`World.cs:1105-1122`) raises `sig.Radio`
  only for entities with `ComponentMask.Emitter`, which `World.cs:246` grants
  only when `def.JamStrength > 0`. **The Radar Mast has `JamStrength = 0`, so it
  gets no Emitter component and no radio boost at all**; its static
  `SigRadio = 25` (`Defs.cs:247`) is *lower than an FPV quad's 70*. Jammers get
  `SignatureWhileEmitting = 85` (the default, `Defs.cs:152`), which under the
  `√(sig/100)` law is 0.92× reach — 1.1× the reach against a quad, not the order
  of magnitude the research asks for. `EmitterState.Active` is set true at
  `World.cs:253` and set false **only in `KZ.Tests/SimTests.cs:1199`** — no
  command, system or upgrade toggles it.
- **Category:** MISSING (radar RF signature, duty cycle), CONTRADICTED (jammer
  RF magnitude), DEAD (`EmitterState.Active`).
- **To wire:** small. Give structures an `EmitsWhileActive` signature independent
  of `JamStrength`, add a `CommandKind.SetEmitting`, and convert the radio
  signature column to log-power so 95 vs 70 is 17× rather than 1.1×.
- **Why it matters:** the game's stated identity for the radar mast is "the
  loudest building a player owns" (`Components.cs:196-200`). It is the quietest.
  The "radiate and see, or stay silent and live" dilemma that `radar-rf.md` calls
  "free design value sitting in the physics" does not exist, because there is no
  off switch and nothing to hide from.

### Two of the three are WIRED; the log-power conversion is not, and is the one that is blocked

**Done.** `CommandKind.SetEmitting` exists, is ownership-checked like the two
upgrade orders, and `EmitterState.Active` is in `StateHash()`. The emitter
component is granted off a new `UnitDef.EmitsWhileActive` rather than off
`JamStrength`, so the Radar Mast has one — it takes the existing
`SignatureWhileEmitting` default of 85 in place of its static 25, and it can be
told to stop. A radar that has been told to stop cannot detect on its radar
channel (`World.IsRadiating`, read by `DetectionRangeFor` and `ComputeDetection`)
and therefore cannot hand an interceptor a `TrackQuality.Radar` solution, while
every passive channel it owns is untouched. Measured in FINDINGS 37.

**Deliberately not done: the log-power conversion of the radio column.** It means
re-deriving every radio value in the catalogue and it is blocked on research
queue brief M, which has not run. So the 85 the Radar Mast now takes is worth
1.84× ESM reach against it under the √ law rather than the order of magnitude
`radar-rf.md` §3.6 asks for, and measured against the scenario it changed nothing
at all. **The signature magnitudes were not touched**, and this entry stays open
for that reason.

**Also not done, deliberately: `JamEmitter.Team` is still not read by the
simulation.** This entry did not name it, but the obvious reading of "the jammer
sits on its owner's launch corridor" is to exempt your own side. Jamming is kept
fully team-blind; emission control is the mitigation instead, because a reduced
own-side coefficient is a number nothing measures and brief M question 4 is what
would give us one. One correction to the record while it is open: the field is
not unread. `KZ.Play/Snapshot.cs:185` reads it, to decide whether a jamming dome
is drawn at all and whether it is labelled as the viewer's own. It is unread *by
the simulation*, which is the part that matters and the part that stays true.

## F9. An ESM bearing is treated as a firing solution

- **Research:** `radar-rf.md` finding 8 and §4 — *"Passive RF gives you a
  bearing, not a firing solution… The game currently treats an RF detection as
  equivalent to a radar track. It should not be."* Recommended: a bearing-only
  contact that cannot cue a weapon until corroborated or cross-fixed (§3A.3 gives
  the two-baseline rule).
- **Code:** `World.ComputeDetection` (`World.cs:853`) returns `true` on the ESM
  channel with no qualification; `CombatSystem.CanEngage` (`CombatSystem.cs:264`)
  gates firing on exactly that boolean. So a Command Post's 500 m ESM reach makes
  any transmitting drone shootable by anything on the team.
- **Category:** CONTRADICTED.
- **To wire:** medium — `IsDetectedBy` currently returns a bool; this needs a
  quality/track-type so `CanEngage` can demand better than bearing-only.
- **Why it matters:** it inflates the value of every ESM-carrying unit and
  deflates the radar mast's cueing role, which is the exact asset
  `FINDINGS.md` §25 concluded was the missing piece ("what is actually missing is
  a cued launch").

### WIRED, including the cross-fix. Measured in FINDINGS 38.

**Done, and the entry understated its own case.** It says an RF detection was
treated as equivalent to a radar track. It was treated as equivalent to an
*optical* one, which is worse: `TrackQualityOf`'s second pass asked
`BestDetectionRange`, which includes the ESM channel, so a drone held on nothing
but its own transmitter read as `TrackQuality.Optical` — and an interceptor sent
at it flew 0.55 of a lead computed from a velocity nobody had measured, off a
contact whose range was never known, then rolled at the 0.60 optical cue rather
than the 0.30 for firing blind.

`TrackQuality` gains a rung below `Optical`: `None 0 · Bearing 1 · Optical 2 ·
Radar 3`. Both existing switches on it — `MovementSystem.InterceptPoint`'s lead
table and `CombatSystem.CueMultiplier` — name `Radar` and `Optical` and default
the rest, so a bearing-only track flies no lead and rolls at 0.30 with no change
to either number. That was checked before the member was added and is the only
reason inserting a middle rung was safe.

`CanEngage` now asks `World.HasFiringSolution` rather than `IsDetectedBy`. The
answer is computed once per team per target per tick alongside detection and held
for the same `TrackHoldTicks`, because `CanEngage` is asked once per candidate per
weapon per tick and a walk of the entity table in there would have made the whole
thing cubic. `lastSolutionTick` is persistent state and is in `StateHash()`.

**The cross-fix is built.** §3A.3's two-baseline rule: `World.EsmCrossFix`
promotes a bearing to a fix when two listeners hold the same emitter and their
bearings cross by more than `SimConstants.EsmCrossFixBam` (20°, which is §3A.3's
sin > 0.35). It reports as `Optical` rather than taking a rung of its own, because
the rung means "a position and no measured velocity" and that is exactly what two
crossed bearings deliver. It is geometry only — no edge roll — so a crossing does
not flicker thirty-two times a second. Cost is bounded: it is reached only for a
contact held on ESM and nothing else, and only by a side that owns two listeners,
counted once per tick.

**Measured:** an Interceptor Battery that used to empty its belt at a crossing
transmitting quad anywhere inside its 3,840 m envelope now cannot engage it at
any range. At night, a Gun Mount backed by a Command Post 2.4 km behind it fired
5 rounds at a quad at 700 m and 4 at 900 m; it now fires none, because its own
camera reaches 305 m and its microphone 576 m. FINDINGS 37's interception pair
(8/20 radiating, 0/20 silenced) reproduces to the trial.

**And the honest part: like F7, it is inert in the shipped scenario.** 340 of the
3,631 contact-seconds team 1 holds across ten play-minutes are now bearing-only —
9% of what the player sees is no longer shootable — and the player owns nothing
that shoots. Team 2's only listener is a Command Post eight kilometres behind the
fight, so it holds zero bearing-only contacts. The cross-fix never fires either:
both of team 1's listeners sit on the same line of latitude 1,800 m apart, which
crosses at under a degree against anything east of them. The branch is reachable,
tested and unused by this scenario's layout.

## F10. Three altitude modifiers have the sign the research says is wrong

- **Research:** `thermal-optical.md` §10 — *"'High-altitude' modifiers — **optical
  sign is wrong**… a sky-silhouetted target is the best optical background
  available… Recommend **optical ×1.00 and thermal ×1.30 at altitude**, and move
  the penalty to where it belongs — the ground-clutter modifier."* Also
  *"Optical night 0.35 — too generous, should be 0.20."*
- **Code:** `World.cs:887` and `World.cs:1158` apply optical **×0.70** at
  `Layer.High`. `World.cs:871` and `World.cs:1149` apply thermal **×0.80** at
  `Layer.High`. `SimConstants.cs:149` `NightOpticalDetectionScale = 0.35`.
- **Category:** CONTRADICTED ×3.
- **To wire:** trivial — three constants.
- **Why it matters:** the game currently makes altitude a *hiding* place from
  cameras and heat sensors, which is backwards, and it is the mechanic
  `Commands.SetAltitude` exists to sell ("height is cover from anything that
  shoots upward and from anything that listens, and it is exposure to anything
  with a radar" — `Commands.cs:332-337`). Half that sentence is implemented with
  the wrong sign. `FINDINGS.md` §18's vertical-split experiment measured "height
  helps a little" partly because of this.

## F11. There is no ground-clutter term, which the research calls the largest factor in the thermal model

- **Research:** `thermal-optical.md` §10 — *"The missing modifier — ground
  clutter, and it should be ×0.30. This is the big one. The game has no term for
  what the target is seen against, and it is the single largest factor in the
  entire thermal model."* Apply to any aerial unit flying nap-of-the-earth and to
  all ground units by default. Quoted effect: FPV quad at night against sky
  2.38 km vs against ground 0.53 km.
- **Code:** grepped for `clutter`, `Clutter`, `background`, `Backed` across
  `src/` — no occurrences. `World.ComputeDetection`'s thermal branch
  (`World.cs:865-874`) has terms for time of day, aperture and altitude only.
- **Category:** MISSING.
- **To wire:** trivial as a flat multiplier keyed on `EntityLayer` (Ground and
  Low get ×0.30, High does not); the honest version needs F1's occlusion work to
  know what is actually behind the target.
- **Why it matters:** combined with **F10**, the thermal channel currently gets
  its *best* results against exactly the targets the research says are hardest
  (low and ground) and its worst against the easiest (sky-backed high). The
  4.6% figure in the research table vs the game's effective 1.0 is a factor of
  twenty on the hardest case.

---

# B. Implemented but unreachable

## F12. `AutonomyQuality` is unreadable on three of the four autonomous strike drones

- **Research:** `autonomy.md` §9 and `FINDINGS.md` §23/§28 — two tiers with
  opposite signs; the classifier and its error rate belong to
  `TargetSelection` only.
- **Code:** `AutonomyClassifier.SelectTarget` (`AutonomyClassifier.cs:50-52`)
  returns `EntityHandle.None` whenever `Tier != TargetSelection`.
  `World.cs:258` grants `ComponentMask.Autonomy` when
  `AutonomyQuality > 0 || AutonomyTier != None`. Three catalogue entries set
  `AutonomyQuality` **without** setting `AutonomyTier`, so their tier defaults to
  `None`: `Mid-Range Striker` quality 72 (`Defs.cs:570`), `Heavy Strike Drone`
  quality 60 (`Defs.cs:592`), `Jet Strike Drone` quality 58 (`Defs.cs:612`). All
  three have `Link = LinkKind.Autonomy`, so `CombatSystem.FindTarget`
  (`CombatSystem.cs:283-289`) routes them into the classifier — which
  immediately returns nothing. They can only ever attack a target given by an
  explicit `OrderAttack`.
- **Category:** DEAD (three tuned numbers unreachable), PARTIAL (the tier split).
- **To wire:** trivial — set `AutonomyTier` on those three defs, or make the
  early return check `Link` rather than `Tier`. Decide deliberately which of the
  two behaviours is wanted; the research says only one unit should have target
  selection, which argues for demoting their `Link` instead.
- **Why it matters:** these are three of the game's four deep-strike airframes.
  A loitering strike drone that loses its designated target does nothing at all
  rather than re-acquiring — and the decoy experiments in `FINDINGS.md` §17/§25
  only exercised the one unit that does have `TargetSelection`.

## F13. Reusable airframes never land, so crews never come back

- **Research:** `economics.md` §"Concrete recommendations" 2 and `front-2026.md`
  §12.4 — trained humans are the binding constraint; crews should be the thing
  you regret losing.
- **Code:** `SortieSystem.Recover` (`SortieSystem.cs:208-222`) — **zero call
  sites**. `SortiePhase.Returning`, `Engaged` and `Terminal` are never assigned
  anywhere. The only paths that return a crew are `World.Kill`
  (`World.cs:658-664`) and `LinkResolver.ReleaseCrew` on the `LastMile` black
  policy (`LinkResolver.cs:302`).
- **Category:** DEAD.
- **To wire:** small — a proximity test against the home pad in
  `MovementSystem`, or an explicit recall command.
- **Why it matters:** the five reusable airframes (Scout Quad, Multirole Quad,
  Recon Wing, Night Bomber, Mothership) hold a crew permanently. Since crews are
  the declared population cap (`Components.cs:410-415`), the only way to free one
  is to lose the aircraft. That makes reconnaissance a permanent tax on offensive
  capacity and inverts the intended relationship between airframes and crews.

## F14. Nobody ever finds a fiber filament

- **Research:** `ground-logistics.md` §12.5 — *"give the enemy a counter: lasers
  that cut the cable"*; `TetherSystem.cs:5-9` states the thread is "a physical
  object lying across the map that an enemy can find and follow home", and
  `SimEvents.cs:24` defines `TetherFound` — *"an enemy stumbled onto a filament
  and has a bearing home"*.
- **Code:** `TetherSystem.AnySegmentNear` (`TetherSystem.cs:316`) — zero callers
  in `KZ.Sim`. `SimEventKind.TetherFound` — **zero pushes**.
- **Category:** DEAD.
- **To wire:** small — one loop in `World.Step` over live tethers against enemy
  units, pushing the event. The `Lingering` state and
  `SimConstants.TetherLingerTicks` already exist to support it.
- **Why it matters:** fiber's stated price is three liabilities — a leash, snag
  risk, and traceability. Two are implemented. The third is not, so fiber is
  currently cheaper than the design says, which affects every conclusion about
  the unjammable rung.

## F15. Four structures do nothing, and one of them gates a whole link tier

- **Research:** `economics.md` §"Concrete recommendations" 4-5 — the fiber spool
  as a strikeable consumable stockpile; one enemy factory worth striking.
- **Code:** `Uplink Terminal` (`Defs.cs:298-304`), `Autonomy Lab`
  (`Defs.cs:306-312`), `Spool Plant` (`Defs.cs:250-257`) and `Drone Workshop`
  (`Defs.cs:233-240`) are referenced nowhere outside `Defs.cs` — they have hit
  points and signatures and no effect. In particular `PlayerState.UplinkCapacity`
  is initialised to 0 (`World.cs:93`) and **never incremented anywhere**, while
  `SortieSystem.cs:95` refuses a satellite launch when
  `UplinkInUse >= UplinkCapacity`.
- **Category:** DEAD.
- **To wire:** trivial for Uplink Terminal (grant capacity on spawn, as
  `World.cs:278-282` already does for Crew Quarters); medium for Spool Plant if
  it is to be a real stockpile.
- **Why it matters:** **the satellite rung of the link ladder is unlaunchable as
  shipped.** `Designator Team` is the only unit carrying `LinkKind.Satellite` and
  every launch of it returns `NoUplinkCapacity`. `FINDINGS.md` §26 found that the
  rung had zero test coverage; this is the same gap one level up — the rung has
  no *reachable* code path at all.

## F16. Thermal blankets and cage armour cannot be fitted

- **Research:** `decoys-masking.md` §6.2 — the blanket's value, its bimodal
  failure (×0.18 correct, ×1.15 incorrect) and its vehicle/infantry split.
  `front-2026.md` §8 — tanks "wear cages".
- **Code:** `Entities.HasThermalBlanket` is set to `false` at
  `Entities.cs:185` and set to `true` **only in `KZ.Tests/SimTests.cs:1269`**.
  `Entities.CageHp` is set to `Fix.Zero` at `World.cs:122` and to a non-zero
  value **only in `KZ.Tests/SimTests.cs:1383`**. The consumers exist and work
  (`World.cs:1115`, `AutonomyClassifier.cs:177`, `World.cs:601-606`).
- **Category:** DEAD.
- **To wire:** trivial — an upgrade command, or a per-def flag.
- **Why it matters:** `Defs.cs:665-676` calls the shaped-charge-versus-top-plate
  margin "the single most important balance number in the game", and cage armour
  is described in `World.cs:588-591` as "the only thing that lets a tank survive
  a drone swarm long enough to matter". Neither can be brought to a match.

## F17. The autonomy target box exists as data and has no producer

- **Research:** `autonomy.md` §9.3 — *"Fratricide specifically: 1-3% of
  autonomous engagements, dropping to near zero if the player has designated a
  no-go bubble. Make the bubble a buildable."* §1 — autonomous selection means
  "a machine choosing what to attack from a class filter **and an area**".
- **Code:** `AutonomyState.BoxMin`/`BoxMax`/`BoxExpiryTick`/`HasBox`
  (`Components.cs:340-343`) are initialised at `World.cs:265-268` and read by
  nothing. `CommandKind.SetAutonomyBox` (`Commands.cs:30`) and
  `CommandKind.DropPin` (`Commands.cs:26`) have no factory method and no case in
  `CommandBuffer.Apply` (`Commands.cs:379-428`).
- **Category:** DEAD.
- **To wire:** small — a command, a factory, an `Apply` case, and a bounds test
  in `SelectTarget`'s candidate loop.
- **Why it matters:** the seeker cone is currently a fixed 120 m radius
  (`SimConstants.cs:323`) around wherever the munition happens to be, so the
  player has no control over what an autonomous munition may consider. The
  no-go bubble the research recommends as the fratricide counter has nowhere
  to live.

## F18. The piloted-decoy-immunity rule is stated, tested, and not applied

- **Research:** `decoys-masking.md` §2.4 and `autonomy.md` §9 — operators see
  through decoys that seekers do not; `AutonomyClassifier.cs:17-19` states it as
  the system's governing rule.
- **Code:** `AutonomyClassifier.IsPilotedWithClearFeed`
  (`AutonomyClassifier.cs:232`) and `SimConstants.PilotedDecoyImmunityRangeMetres`
  (`SimConstants.cs:330`) are used **only by `KZ.Tests/SimTests.cs:471,476`**.
  What actually happens is broader and cruder: `CombatSystem.cs:325` skips every
  `ComponentMask.Decoy` entity for *every* attacker, piloted or not, linked or
  not, at any range.
- **Category:** DEAD (function), PARTIAL (rule approximated).
- **To wire:** small — replace the blanket skip in `BestTargetInRange` with the
  function that already exists.
- **Why it matters:** the approximation gets the headline case right and the
  interesting case wrong. A radio FPV whose link has gone black is a machine
  flying on its last order, and the research says that is exactly when a decoy
  should work. Currently it is immune.

## F19. Staggered pad egress is computed and ignored

- **Code:** `SortieState.EgressUntilTick` is written at `SortieSystem.cs:142-144`
  from `SortiePadEgressBaseTicks` and `SortiePadEgressPerIndexTicks`
  (`SimConstants.cs:118-119`) and **read nowhere**.
- **Category:** DEAD.
- **To wire:** trivial — one check in `MovementSystem.StepOne`.
- **Why it matters:** low on its own; the balance harness works around it by
  hand (`KZ.Balance/Program.cs:771-775`, *"A small spread, so they are not
  literally stacked in one point"*). Worth recording because a workaround in a
  measurement harness is how a dead mechanic stays invisible.

## F20. Dead constants, enum values and flags

Each was grepped under at least two names. Consequence noted where it is more
than tidiness.

| Symbol | Where | Category | Consequence |
|---|---|---|---|
| `TrackHoldTicks` | `SimConstants.cs:300` | DEAD | see **F2** |
| `FrozenEnduranceScale = 0.50` | `SimConstants.cs:234` | DEAD | there is no endurance/fuel field on `UnitDef` or any component at all — `weather.md` §3.1's cold ladder has nothing to attach to |
| `AutonomySmokePenalty = 15` | `SimConstants.cs:322` | DEAD | no smoke or obscurant exists; `weather.md` §6.4 and `decoys-masking.md` §5.1 both build on obscurants |
| `MeshAcquisitionPenaltyTicksPerHop = 13` | `SimConstants.cs:79` | DEAD | a deep mesh chain is currently as responsive as a one-hop one; the mesh faction's stated fragility is only modelled as link loss, not latency |
| `CrewRecoveryPriorityTicks = 160` | `SimConstants.cs:102` | DEAD | the "Priority Recovery" upgrade it names does not exist |
| `VisionCellMetres = 16` | `SimConstants.cs:36` | DEAD | detection is brute-force O(n²) per tick; the vision grid this reserves was never built |
| `MaxEntitiesStandard/Compact`, `MaxTethersStandard/Compact` | `SimConstants.cs:337-340` | DEAD | documented as "match rules, not device settings" and never enforced — every caller passes its own capacity |
| `TicksPerCommandTurn = 4` | `SimConstants.cs:25` | DEAD | networking not built; expected |
| `AmmoType.Airburst`, `AmmoType.Buckshot` | `Enums.cs:126,141` | DEAD | fully implemented in `AirHitChance` (`CombatSystem.cs:120-141`) and carried by **no unit**. `point-defence.md` §"Suggested replacement units" asks for an Airburst Autocannon; the hit model for it already exists and the unit does not |
| `DamageType.Incendiary` | `Enums.cs:35` | DEAD | full damage table at `Defs.cs:723-731`, no weapon uses it |
| `CrewState.Reserved` | `Enums.cs:75` | DEAD | the player cannot hold a crew back |
| `SortiePhase.Engaged/Terminal/Returning` | `Enums.cs:83-86` | DEAD | `Terminal` ("committed, ignores new orders, cannot be recalled") is never entered; commitment is driven only by link pip |
| `ComponentMask.Producer`, `ComponentMask.Tethered` | `Entities.cs:65,67` | DEAD | never set, never tested |
| `SimEventKind.SalvageCollected`, `SimEventKind.TetherFound` | `SimEvents.cs:24,33` | DEAD | see **F3**, **F14** |
| `Entities.Rank`, `Entities.HpMax`, `MoverState.RadiusClass`, `SensorSuite.Quality`, `DecoyState.Mimics`, `JamEmitter.Team`, `JamEmitter.Source` | various | DEAD | all written, never read. `JamEmitter.Team` is the interesting one: it is captured at `World.cs:421` and ignored by `SignalGrid`, so **a jammer jams its own side's drones identically to the enemy's**. That may be intended, but nothing says so |
| `UnitDef.Faction`, `UnitDef.Tier`, `UnitDef.BuildTicks`, `UnitDef.FootprintTiles`, `UnitDef.IsFlyingDecoy`, `UnitDef.Id` | `Defs.cs` | DEAD | zero readers outside `Defs.cs`. **`Faction` means faction restrictions are unenforced** — `PlayerState.Faction` is set in `KZ.Headless/Program.cs:75-76` and only ever printed. Either side can field Kestrel and Obsidian units simultaneously. `BuildTicks` means there is no build queue; `Tier` means no tech gating |
| `SortieSystem.LaunchableCount` | `SortieSystem.cs:158` | DEAD | UI-facing; expected until there is a UI |

---

# C. The signature tables: one applied, one not

This is the clearest single pattern in the audit. `acoustic.md`'s recommended
table was applied unit-for-unit. `thermal-optical.md`'s was not applied at all.

## F21. `acoustic.md` §7 — WIRED, and worth recording as the counter-example

Verified value by value against `Defs.cs`: Scout Quad / FPV Team / Fiber FPV 12,
Night Bomber 35, Recon Wing 18, Loitering Munition 55, Heavy Strike Drone 90,
Jet Strike Drone 95, Decoy Drone 65, Main Tank 80, EW Post 20 — all exactly the
recommended figures. Recommendation 1 (linear scaling, signature read as fraction
of reach) is at `World.cs:962-963`. Recommendation 2 (Gun Mount base 200) is at
`Defs.cs:293`. Recommendation 5 (high-altitude 0.50) is
`SimConstants.AcousticHighScale`. Recommendation 7 (×1.0/×2.0/×2.5, ×1.5 for
high) is `SimConstants.cs:187-192` and `World.AcousticTimeScale`.
**Category: WIRED.**

## F22. `thermal-optical.md` §11 — MISSING in its entirety

| Unit | Thermal now / rec. | Visual now / rec. |
|---|---|---|
| FPV Team, Scout Quad | 8 / **8** ✓ | 15 / **6** ✗ |
| Fiber FPV Team | 8 / **9** ✗ | 15 / **6** ✗ |
| Night Bomber | 22 / **32** ✗ | 55 / **30** ✗ |
| Recon Wing | 25 / **14 or 38** ✗ | 30 / **40** ✗ |
| Loitering Munition | 45 / **52** ✗ | 25 / **20** ✗ |
| Heavy Strike Drone | 60 / **72** ✗ | 45 / **38** ✗ |
| Jet Strike Drone | 85 / **92** ✗ | 40 / **34** ✗ |
| Decoy Drone | 25 / **40** ✗ | 30 / **22** ✗ |
| Main Tank | 90 / **90** ✓ | 90 / **100** ✗ |
| EW Post (jammer) | 40 / **48** ✗ | 70 / **75** ✗ |

Plus, from the same section: Interceptor Battery thermal **500 → 520** and Main
Tank thermal **300 → 250** (`Defs.cs:430`, `Defs.cs:403`) — not applied; thermal
day **0.55 → 0.62** and night **1.25 → 1.15** (`SimConstants.cs:161-162`) — not
applied (the research calls this one minor).

- **Category:** MISSING.
- **To wire:** trivial — twenty numbers.
- **Why it matters:** the visual column is the one that bites. `AirHitChance`
  (`CombatSystem.cs:161-167`) uses `SigVisual` as the target's *size* for the
  gun's hit probability, so every recommended visual reduction is also a direct
  survivability change for small drones — and the research explicitly connects
  this to `FINDINGS.md` §2 ("the fix is not the signature table — it is an
  exponent below 0.5 for the optical channel"). Leaving the table unapplied
  leaves the FPV quad 2.5× more visible than recommended while the turret
  findings are being argued about.

## F23. Channel reliabilities are reversed for aerial targets

- **Research:** `thermal-optical.md` §10 — *"Channel reliability — thermal 84 vs
  optical 88 is reversed for aerial targets… Recommend thermal 88, optical 82."*
  `radar-rf.md` §4 — radar reliability should be target-dependent
  (0.85 / 0.70 / 0.35 by altitude band) plus a 15-25% false-track rate.
  `acoustic.md` §7 Rec. 3 — acoustic 52 baseline, 35 in rain/wind/near a road,
  60 on a quiet rural night.
- **Code:** `World.ChannelReliability` (`World.cs:1007-1017`) is a flat static
  switch: Esm 95, Optical 88, Thermal 84, Radar 78, else 52. No environment, no
  altitude, no target dependence, and no false contacts anywhere.
- **Category:** CONTRADICTED (thermal/optical order), MISSING (the three
  conditioning rules).
- **To wire:** small — the function already receives the channel and target
  index; it needs the world.

## F24. Radar signature ordering is inverted for the heavy multirotor

- **Research:** `radar-rf.md` §5 recommends, on its own scale, small quad 18,
  heavy multirotor 38, fixed-wing recon 46, loitering munition 36, Shahed-class
  48, turbojet 54, reflector decoy 90. The game's scale is that scale plus 20
  (`Components.cs:150-153`: `S = 2 × dBsm + 80` vs the paper's `2 × dBsm + 60`).
- **Code:** `Defs.cs` ships Scout/FPV/Fiber 22, Multirole Quad 26, Recon Wing 40,
  Loitering Munition 38, Night Bomber **58**, Heavy Strike Drone **52**, Jet
  Strike Drone **50**, Decoy Drone 92, Mothership 66.
- **Category:** PARTIAL / CONTRADICTED. The decoy-vs-Shahed pair (92 vs 52,
  3.16×) matches the research exactly — that is the correction
  `FINDINGS.md` §25 records. But the Night Bomber at 58 is given a **larger**
  radar cross-section than both the Shahed-class Heavy Strike Drone (52) and the
  Jet Strike Drone (50), inverting the research's ordering (0.08 m² vs 0.25 m²
  vs 0.5 m²). The Recon Wing at 40 is ~6 dB below its recommended equivalent.
- **To wire:** trivial — three numbers.
- **Why it matters:** the one unit whose entire purpose is radar visibility was
  fixed; its neighbours were not, so the *ordering* the fourth-root law is
  supposed to express is wrong in the middle of the table.

## F25. Radar reference reach vs instrumented cap

- **Research:** `radar-rf.md` §4 — *"Keep 1400 as an instrumented cap. Set
  reference reach 900 map m against a `radar_sig = 50` target."* Interceptor
  Battery: keep 800 as cap, reference 550.
- **Code:** `Defs.cs:246` `SensorRadar = M(1400)` and `Defs.cs:430`
  `SensorRadar = M(800)` are used directly as reference reaches at
  `World.cs:862`; there is no cap concept anywhere.
- **Category:** CONTRADICTED.
- **To wire:** trivial — two numbers plus a `Fix.Min` in `Reaches`.
- **Why it matters:** the uncapped reference is what produces the 2,373 m figure
  against the decoy that `FINDINGS.md` §25 quotes, and §25's conclusion ("the
  reflector buys nothing") turns on the ratio between detection reach and kill
  reach. Halving the detection side changes that argument.

---

# D. Smaller, but specific and checkable

## F26. Acoustic conditioning (weather and environment)

- **Research:** `weather.md` §8.2 matrix — acoustic **×0.80 in WET**, **×1.10 in
  MURK** ("fog comes with calm air"), off above 12 m/s. `acoustic.md` §7 Rec. 8 —
  a graded wind ladder ×1.0 / ×0.7 / ×0.4 / off. Rec. 6 — acoustic should be the
  only channel able to generate a *false* contact, ~10% per genuine alert in
  cluttered terrain.
- **Code:** `World.WeatherScale` (`World.cs:1029-1054`) returns `Fix.One` for
  acoustic under both Wet and Murk and `Fix.Zero` under Wind.
- **Category:** PARTIAL (wind on/off matches the four-state abstraction),
  MISSING (wet, murk, false contacts).
- **To wire:** trivial for the two multipliers; medium for false contacts, which
  need a contact object the game does not have.

## F27. Cold, icing and endurance

- **Research:** `weather.md` §3.1 gives an endurance ladder (1.00 / 0.85 / 0.65 /
  0.50, 0.70 with a pre-heat kit), §3.2 says apply a **1.3× steeper** version to
  small FPV and interceptors, §8.2 says WET carries a **winter flag** that
  becomes icing and grounds *every* propeller airframe including combustion ones.
  `front-2026.md` §7 asks for a **seasonal thermal term** — winter nights are the
  most lethal sensing condition of the year.
- **Code:** `SortieSystem.WeatherGrounds` (`SortieSystem.cs:229-241`) is a
  two-case switch on propulsion with no interaction with `GroundState`.
  `SimConstants.FrozenEnduranceScale` is dead (**F20**); there is no endurance
  field to scale. `World.ThermalTimeScale` (`World.cs:1081-1089`) has no season
  term.
- **Category:** MISSING.
- **To wire:** small for the icing flag and the seasonal thermal term; medium for
  endurance, which needs a new per-entity resource and a return-to-base behaviour
  that does not exist (**F13**).
- **Why it matters:** the weather system's stated purpose is asymmetry — "the
  side flying cheap quadcopters loses half its year and the side flying
  combustion strike drones does not" (`SortieSystem.cs:58-62`). Half of that
  asymmetry (grounding) is wired; the endurance half is not, so `GroundState.Frozen`
  currently *only* helps ground vehicles and costs nobody anything.

## F28. Weather does not touch hit probability, and grounds the wrong recon aircraft

- **Research:** `weather.md` §8.2 — small FPV in WIND: `P(hit) ×0.7`; in MURK:
  `P(hit) ×0.3`. Fixed-wing recon in WIND: *"flies, **cannot land**: sorties
  continue, none launch"*.
- **Code:** `AirHitChance` has no weather term. `Recon Wing` is
  `Propulsion.SmallElectric` (`Defs.cs:519`), so `WeatherGrounds` refuses its
  launch in Wind — which matches the "none launch" half and contradicts the
  "sorties continue" half only because there is no in-flight state to continue.
- **Category:** MISSING (hit-chance terms), PARTIAL (recon in wind).

## F29. Decoy plausibility is a scalar, and the blanket is a flat 40%

- **Research:** `decoys-masking.md` §6.3 — *"Wrong because it is a scalar"*, with
  a seven-row per-channel table and two additional mechanics (plausibility decays
  ~15% per sustained observation; a recognised decoy still costs the attacker an
  engagement delay). §6.2 — the blanket is *"wrong in value and wrong in shape"*:
  ×0.18 when used correctly (~65%), ×1.15 when not (~35%), and for vehicles it
  should depend on time since the powerpack last ran.
- **Code:** `DecoyState.Plausibility` is one `Fix` (`Components.cs:406`) set from
  `Catalog.BasePlausibility` (`Defs.cs:740-750`). `World.EffectiveSignature`
  applies a flat `× 40/100` to thermal (`World.cs:1117`).
- **Category:** CONTRADICTED (both), MISSING (decay, engagement delay, variant
  band).
- **To wire:** medium — per-channel plausibility means the classifier has to know
  which channel found the candidate, which it currently does not.

## F30. Autonomy: the operator ratio and the error rates

- **Research:** `autonomy.md` §9 — make autonomy convert a **1:1 operator
  requirement into 1:N** (N = 4 → 12 → 30) rather than free; wrong-target rates
  of **3%** (terminal guidance), **12%** (autonomous selection, clean), **35%**
  (against decoys); autonomous *interception* should get **2-4%** and no decoy
  penalty.
- **Code:** `UnitDef.ConsumesCrew` is a bool; there is no N.
  `AutonomyClassifier.EffectiveQuality` uses `AutonomyQuality` directly as the
  percentage chance of a correct pick, and the only unit that reaches it
  (`Autonomous Munition`, quality 55 — `Defs.cs:659`) therefore has a **45% base
  wrong-target rate** against the research's 12%. Interceptors never consult the
  classifier at all.
- **Category:** MISSING (1:N), CONTRADICTED (error rate, ~3.7× the recommended
  clean-environment figure).
- **To wire:** trivial for the quality numbers; medium for 1:N, which changes
  `CrewPool` accounting.

## F31. Point defence: what remains after F4

Recorded separately from F4 because these are model-shape items rather than
missing data.

- **Speed penalty range-dependence** — `point-defence.md` §"The speed penalty is
  range-dependent": **WIRED**, `CombatSystem.cs:186-189`
  (`severity = 0.55 + fraction × 1.45`).
- **Range falloff with a non-zero tail** — **PARTIAL**: the code uses
  `closeness² × 0.94 + 0.06` (`CombatSystem.cs:149-150`), which has the thin tail
  the research asks for but not the saturating head of the recommended
  `1 − exp(−k(R_ref/R)²…)`.
- **Hard ceiling per weapon class** — **WIRED**, `UnitDef.CanReachHigh` /
  `EffectiveReach` (`CombatSystem.cs:235-246`).
- **Drop the flat 30% upward-fire cut** — **CONTRADICTED**: the research says
  replace it with slant-range geometry plus the ceiling, keeping a 10-15%
  residual. `CombatSystem.cs:197-198` still applies ×0.70, *and*
  `EffectiveReach` applies a further ×0.60, so a high target is taxed twice.
- **Slant range** — **MISSING**, and structurally so: `Layer` is a three-value
  enum by deliberate design (`Enums.cs:6-12`), so there is no altitude to take a
  hypotenuse of. This one is a real design fork, not an oversight.
- **`engagement_channels` field** — **MISSING**. Every shooter is implicitly 1.
- **Airburst as a target-radius multiplier, not a range multiplier** —
  **PARTIAL**: implemented as a flatter range curve plus `aimForgiveness`
  (`CombatSystem.cs:134-141`), which is the same intent by a different mechanism.
  Moot while no unit carries it (**F20**).
- **A cheap networked acoustic sensor unit**, and **a cued interceptor launch** —
  **MISSING**. The second is the one `FINDINGS.md` §25 identified as the reason
  the decoy is tactically inert: *"Until a radar cue can launch something, this
  whole category of unit is buying a property the game has nowhere to spend."*
  Nothing has been built since.

## F32. Economics: the cost ratios

- **Research:** `economics.md` §"Concrete recommendations" 3 — FPV 1×,
  interceptor 3-8×, heavy multirotor ~25×, **decoy ~25×**, loitering/Shahed-class
  25-90×.
- **Code:** relative to FPV Team at 200 — Interceptor FPV 300 (1.5×), Night
  Bomber 1100 (5.5×), **Decoy Drone 130 (0.65×)**, Loitering Munition 550
  (2.75×), Heavy Strike Drone 800 (4×).
- **Category:** CONTRADICTED.
- **To wire:** trivial in isolation; a whole balance pass in practice.
- **Why it matters:** the decoy is the one that changes a conclusion. At 0.65× an
  FPV it is a spam unit; at the researched 25× it is a considered purchase. The
  packages in `FINDINGS.md` §17 ("1 real + 13 decoys" for ~2,600 Materiel) are
  only constructible at the shipped price — at the researched ratio that package
  costs about 66,000. Any conclusion about decoy mixes rests on a price the
  research contradicts.

---

# E. Not a research gap, but it corrupts the same conclusions

## F33. The balance harness measures a gun that is not in the catalogue

- `KZ.Balance/Program.cs:736-748` — `RunAssault` defaults to `F(550)` and
  overwrites `w.Entities.Weapon[gun.Index].RangeMetres` at line 767. The
  saturation (`:65`), approach (`:150`) and night (`:194,197`) experiments all
  run at **550 m**. The shipped Gun Mount is **85 m** (`Defs.cs:290`) — the
  correction `FINDINGS.md` §2 made, and the entry explaining it is in the def
  itself. Only `GunRangeExperiment` sweeps the range.
- **Why it matters:** three of the ten experiments are measuring a weapon 6.5×
  longer-ranged than the one in the game. The saturation table in
  `FINDINGS.md` §15 is a 550 m table.

## F34. Every experiment runs on an empty, clear, ownerless, uncovered world

- All ten balance worlds and the headless mission call `t.Fill(TileClass.Open)`;
  `Forest`, `Rubble`, `PowerLine` and `Water` appear only in `KZ.Headless`
  (`Program.cs:69-71`) and one determinism test.
- `Weather` is never set outside tests → always `Clear`.
- `Ground` is never set outside tests → always `Firm`.
- `Territory` is never populated → every cell owner 0.
- `Imagery` is never granted → coverage empty everywhere.
- `HasThermalOptics` is never set → always false.
- `UplinkCapacity` is never raised → always 0.
- **Why it matters:** five whole systems — weather, ground state, territory,
  reference imagery, and the satellite link — are provably inert in every number
  the project has recorded. This is the same shape as `FINDINGS.md` §26's
  observation that `Satellite` appeared zero times across the suite, generalised:
  **the systems that are never constructed in a harness are exactly the systems
  whose bugs a green suite cannot find.**

---

# F. Checked and genuinely wired

Recorded so this audit can be falsified, and because two of them are the
counter-examples that make the pattern legible.

1. `acoustic.md` §7 signature table, recommendations 1, 2, 5, 7 — **F21**.
2. `acoustic.md` Rec. 8, wind kills acoustic outright — `World.cs:1048`.
3. `decoys-masking.md` §6.1 decibel radar scale and the decoy 92 / strike 52 pair
   — `Components.cs:150-161`, `World.cs:954-1001`.
4. `radar-rf.md` §6 "fourth-root physics does not change" — the baked
   `RadarReachTable` is `10^((S−80)/80)`, i.e. σ^(1/4). Correct.
5. `thermal-optical.md` §10 twilight correction (0.90 → ~0.60) —
   `SimConstants.cs:170` ships 0.65.
6. `thermal-optical.md` §10 day/night direction (thermal is a night sensor) —
   `World.ThermalTimeScale`.
7. `weather.md` §8.2 optical and thermal columns for WET and MURK —
   `SimConstants.cs:217-231`, exactly the recommended values.
8. `weather.md` §8.1 four states plus a separate season stripe —
   `Enums.cs:216-265`.
9. `weather.md` §4.2/§8.3 mud deletes off-road movement, frozen beats firm —
   `MovementSystem.GroundScale`.
10. `point-defence.md` §Q2 hard per-class ceiling — `CanReachHigh`.
11. `point-defence.md` speed penalty scales with range — `CombatSystem.cs:186-189`.
12. `navigation-denied.md` §1.4 lock as a cliff with a re-acquisition distance,
    and §2.2 celestial as a drift-rate modifier rather than a position fix —
    `NavigationSystem.cs:63-119`, `SimConstants.cs:267-284`. (The mechanism is
    correct; nothing consumes its output — see **F5**.)
13. `ground-logistics.md` §12.5 fiber spool limit 400-1,700 map metres —
    `Defs.cs:474` ships 1,400.
14. `economics.md` §"Crews" — crews as the employment cap — `CrewPool`,
    `SortieSystem.Launch`.

---

# G. Unactionable against the current simulation

Listed rather than guessed at, per the brief. These are either campaign-layer by
the research's own argument, or too vague to check.

- **`deep-strike.md` §9** — explicitly says most of the document *"does not belong
  in a match"* and belongs on a campaign layer that does not exist. Its three
  in-match items (road denial as a contested state, bridges as a timer, the
  relocatable forward depot) are concrete and unbuilt, but none has a hook in
  the current sim.
- **`SCALE.md` §"What this means for the build"** — campaign structure,
  map-making guidance. Not code.
- **`beyond-ukraine.md` §13** items 1, 3, 4, 5, 6, 7, 9, 10, 11 — campaign
  parameters, magazine pools across matches, authority-to-engage, supply-chain
  modifiers. Item 2 (directed energy as a unit class that bypasses RF and kills
  fiber drones) is the one that is match-sized and buildable; it is MISSING.
- **`ground-logistics.md` §12** items 1-4 and 6-8 — a supply/stock/casevac model
  the game has no representation for at all.
- **`front-2026.md` §12** items 1, 2, 3, 5, 6 and the seventh-to-ninth additions
  — mission and campaign design, not simulation rules. Item 3 ("entered and held
  must be different states") would need `Territory` to be dynamic; it is
  currently static and unpopulated (**F6**).
- **`economics.md` §"What I could not establish"** — the research's own open
  questions.
- **`PHYSICS-TOOL.md`** — the architecture calls for `acoustic.py`, `thermal.py`,
  `radar.py`, `optical.py` and `generate_tables.py` writing `content/detection.kzb`.
  Shipped: `tools/propagation/{acoustic,common,generate_tables,radar_scale}.py`.
  `thermal.py` and `optical.py` do not exist, `content/` does not exist, and
  `generate_tables.py` prints a recommendation rather than emitting content — a
  limitation its own header states deliberately. **PARTIAL by design**; the
  document's closing advice is "build it when the research lands", so this is not
  a gap so much as a staged plan. Worth noting that the acoustic channel is
  exactly the one whose table *was* applied (**F21**), which is probably not a
  coincidence.
- **`thermal-optical.md` §10 "search versus track reach"** — a 0.35× search reach
  requiring a cue is concrete, but it depends on the cue mechanic in **F31**
  which does not exist, so it cannot be wired in isolation.
- **`acoustic.md` §4.4 "raise the microphone"** — needs an elevation model
  (**F1**).
- **`radar-rf.md` §2B.1/§3A.1 "carry the 22 dB and 28 dB environment penalties as
  explicit additive dB loss terms"** — the simulation has no dB domain; this is a
  recommendation to the offline tool, not to `KZ.Sim`.

---

# H. Not traced

Stated so the boundary of this audit is visible.

- I did not verify the `RadarReachTable` values against
  `tools/propagation/radar_scale.py` by running it. The law is right; the 101
  baked constants were spot-checked at S=80 (1.0) and S=100 (~1.78) only.
- I did not trace `Fix`/`Trig`/`DetRandom` for correctness — several of their
  public helpers (`Half`, `Lerp`, `Max`, `Sign`, `ToInt`, `FromInts`,
  `BamPerEntry`) have zero call sites but are ordinary library surface, not
  mechanics.
- I did not attempt to judge whether `front-2026.md` §4's kill-zone depth figures
  are consistent with `SCALE.md`'s 12:1 compression and the 2048 m map; that is a
  design question rather than a wiring one.
- `KZ.Tests` was read only to determine whether a symbol's *only* caller was a
  test. I did not audit test coverage itself, though **F34** is the beginning of
  that argument.
