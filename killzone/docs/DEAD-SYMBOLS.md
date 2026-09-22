# Dead symbols: the first run

KILL ZONE is a video game. This is the output of `tools/check_dead_symbols.py`,
the build guard added by WIRING-SPEC task 0.1, on the day it was added.

It found **86 symbols** that the simulation declares and does not use. That is
the point of the exercise, not a failure of it. Three systems shipped inert this
project — terrain occlusion, the two-second track hold, the gun mount's magazine
— and every one of them was invisible to careful reading and one `grep` away
from obvious. FINDINGS 30 put it plainly: *that is not a lesson yet, it needs to
be a build step.*

**Nothing here is fixed by this document or by the guard.** Other tasks own the
findings. This is the instrument and its first reading.

---

## How to read the guard

```
./build.sh               compile, test, then check   (the normal path)
./build.sh deadsymbols   the guard on its own
python3 tools/check_dead_symbols.py --all     every finding in full
python3 tools/check_dead_symbols.py --list    machine-readable, one per line
python3 tools/check_dead_symbols.py --selftest   the guard's own corpus
```

Three files control it:

| file | what it means |
|---|---|
| `tools/dead-symbols-allow.txt` | **justified.** Every entry carries a tagged reason. Suppressed silently. |
| `tools/dead-symbols-baseline.txt` | **recorded debt.** No reasons, because none of it is justified. Warns. |
| `tools/check_dead_symbols.py` | the checker, and its `EXPECTED` self-test corpus |

---

## The decision about warning versus failing, stated plainly

The first honest run found eighty-six findings in ten categories. Failing the
build on all of them would have stopped every other task in WIRING-SPEC dead,
for a backlog nobody in this session caused. A build step that does that gets
deleted within a week, and then the fourth dead system ships.

Nor is "make the big categories warn and the small ones fail" good enough: it
softens the future to excuse the past, and the future is the part worth
guarding.

So the guard is a **ratchet**, not a wall:

- everything in `tools/dead-symbols-baseline.txt` — these 86 — **warns**;
- anything **not** in that file **fails the build**, in every category, with no
  grace period. A newly dead symbol is always cheap to fix at the moment it is
  written, and never cheaper later;
- a baseline line that stops matching anything also warns, asking to be deleted,
  so the ledger only goes down;
- a malformed allowlist entry **fails the build**. An allowlist entry without a
  reason a reviewer can judge is a bug being suppressed.

The promotion condition is therefore not a date. It is: **the baseline reaches
zero and the file is deleted.** Every line in it is owned by a task in
`docs/WIRING-SPEC.md`, and when that task lands its line goes with it.

There is one deliberate softening inside the guard itself, and it is this: a
symbol only `src/KZ.Tests` touches is reported under `test-only` rather than as
"referenced". It is not silenced, and it is not allowed to hide in the allowlist
as "used by tests" — it gets its own named block, because green tests with no
production caller is exactly what produced FINDINGS 30.

---

## What it catches

| category | rule |
|---|---|
| `sim-public` | a `public` member in `src/KZ.Sim` with no reference outside its own declaration |
| `sim-constant` | a `SimConstants` value never referenced (or referenced only by other constants) |
| `sim-event` | a `SimEventKind` never pushed |
| `unitdef-field` | a `UnitDef` field never read outside `Defs.cs` |
| `unitdef-default` | a `UnitDef` field **no unit def sets**, so the declared default is the only value the game can ever see |
| `unitdef-single` | a `UnitDef` field **exactly one unit sets** — the Gun Mount shape |
| `enum-value` | an enum value never constructed or compared against |
| `component-flag` | a `ComponentMask` flag never tested with `Has`/`HasAny` |
| `write-only` | a public field assigned everywhere and read nowhere |
| `test-only` | any of the above that only `src/KZ.Tests` touches |

The first six plus `component-flag` are the checks WIRING-SPEC 0.1 asked for.
The last three were added because the three known-dead items would otherwise have
slipped through:

- `unitdef-single` exists because the gun mount's traverse rate and magazine are
  not unreferenced fields — the code reads them, and the Interceptor Battery sets
  them. What is wrong is that the Gun Mount does not, so `SlewTicks` returns
  early and reload accounting is skipped for it. "One unit carries this stat" is
  the mechanical shadow of that.
- `write-only` exists because half a dead wire looks exactly like a live one from
  the writing end. `JamEmitter.Source` and `DecoyState.Mimics` are filled in at
  spawn and never read.
- `test-only` exists for the reason above.

### Three rules keep it from crying wolf

1. It parses **cleaned** source: comments, string literals and `nameof(...)`
   arguments are blanked out before anything is counted, with byte offsets
   preserved so line numbers stay true. A name in a comment is not a use.
2. It matches **whole tokens** only, so a short field name cannot be kept alive
   by a substring somewhere else, and enum values and component flags are matched
   only in their **qualified** form (`AmmoType.Buckshot`), which C# requires
   nearly everywhere. Twelve enums declare a `None` and three declare a `Ground`;
   bare-name matching would have called all of them live.
3. It knows each declaration's **full span**, so nothing is kept alive by its own
   body. A recursive call is not a call site.

Two exemptions are built into the checker rather than the allowlist, because they
are structural and not judgement calls: the members every C# type has or needs
(`Equals`, `GetHashCode`, `ToString`, `Main`, `Dispose`, constructors, operators,
indexers), and an enum's `None = 0`, which the language hands to every
default-initialised field whether or not anything writes it by name.

### What it cannot see, stated so nobody trusts it too far

- **Two types with the same field name defeat it.** `Entities.Rank` is written
  and never read, but `CrewRecord.Rank` is read constantly and the checker
  counts tokens, not types. `SensorSuite.Quality` and `JamEmitter.Team` hide
  behind `AutonomyState.Quality` and `Entities.Team` the same way. All three are
  in audit F20 and none is in the list below. A type-aware analyser would catch
  them; a type-aware analyser is a package dependency this repository is not
  allowed to grow.
- **An enum value carried by no unit is not an unreferenced enum value.**
  `AmmoType.Airburst`, `AmmoType.Buckshot` and `DamageType.Incendiary` are fully
  implemented in `CombatSystem` and carried by no unit in the game (audit F20).
  The checker sees the implementation and calls them live. The `unitdef-single`
  and `unitdef-default` checks catch this shape for *fields*; the equivalent for
  enum-typed fields is not built.
- **It cannot tell a value that is read from a value that matters.** A stat read
  into a component and then multiplied by zero is live as far as this is
  concerned. Only a production-path test catches that, which is why the guard
  does not replace WIRING-SPEC's acceptance condition, it sits beside it.

The guard's own regression corpus is `tools/testdata/dead-symbols/` — a
miniature of `src/` carrying every failure shape above, each beside a live
counterpart that must stay clear. `./build.sh` runs it before the real check,
because a checker that has quietly stopped catching things is worse than no
checker: it reads as evidence.

---

## The findings

86 symbols. Cross-references are to `docs/AUDIT-UNWIRED.md`. Items marked
**new** were not in that audit — nine of them, which is the argument for having
an instrument rather than a careful reader.

### `sim-event` — pushed by nothing (2)

| symbol | where | consequence |
|---|---|---|
| `SimEventKind.SalvageCollected` | `SimEvents.cs:33` | **F3.** The economy has no income. `Materiel` only ever falls. |
| `SimEventKind.TetherFound` | `SimEvents.cs:24` | **F14.** Finding a filament gives the enemy nothing. |

### `component-flag` — never tested (2)

| symbol | where | consequence |
|---|---|---|
| `ComponentMask.Producer` | `Entities.cs:67` | **F20.** Never set either. There is no production component. |
| `ComponentMask.Tethered` | `Entities.cs:65` | **F20.** Tethers are tracked entirely outside the mask. |

### `unitdef-field` — written in `Defs.cs`, read nowhere else (6)

All six are audit **F20**, and the audit's list and the checker's agree exactly.

| symbol | where | consequence |
|---|---|---|
| `UnitDef.Faction` | `Defs.cs:18` | faction restrictions are unenforced — either side can field Kestrel and Obsidian units at once |
| `UnitDef.Tier` | `Defs.cs:19` | no tech gating |
| `UnitDef.BuildTicks` | `Defs.cs:22` | no build queue |
| `UnitDef.FootprintTiles` | `Defs.cs:137` | structures have no footprint |
| `UnitDef.IsFlyingDecoy` | `Defs.cs:157` | the piloted-decoy rule is never applied |
| `UnitDef.Id` | `Defs.cs:16` | stamped by the catalogue and read by nobody |

### `unitdef-default` — no unit sets it, so the default is the whole game (2) — both **new**

| symbol | where | consequence |
|---|---|---|
| `UnitDef.BlackPolicy` | `Defs.cs:34` | **new.** No unit sets it, so every airframe is `Abort`. `LinkResolver.cs:290` and `:305` implement `LastMile` and `DualLink` for nothing — two of the three link-loss behaviours in the game are unreachable, and `SimConstants.DualLinkSwitchTicks` is tuned for one of them. |
| `UnitDef.SignatureWhileEmitting` | `Defs.cs:148` | **new.** Every emitter in the game radiates at exactly 85. `World.cs:1153` reads it faithfully. |

### `sim-constant` — tuned and never read (11)

| symbol | where | consequence |
|---|---|---|
| `VisionCellMetres` | `:36` | **F20.** The vision grid was never built; detection is brute-force O(n²) per tick. |
| `MeshAcquisitionPenaltyTicksPerHop` | `:79` | **F20.** A deep mesh chain is as responsive as a one-hop one. |
| `CrewRecoveryPriorityTicks` | `:102` | **F20.** The "Priority Recovery" upgrade it names does not exist. |
| `FrozenEnduranceScale` | `:265` | **F20.** There is no endurance or fuel field to attach it to. |
| `AutonomySmokePenalty` | `:353` | **F20.** No smoke or obscurant exists. |
| `MaxEntitiesStandard` | `:368` | **F20.** "Match rules, not device settings", and never enforced. |
| `MaxEntitiesCompact` | `:369` | **F20.** As above. |
| `MaxTethersStandard` | `:370` | **F20.** As above. |
| `MaxTethersCompact` | `:371` | **F20.** As above. |
| `TetherSegmentsPerTick` | `:97` | **new.** The two-segments-per-tick rule is real and implemented at `TetherSystem.cs:258-264` — with the number inlined. Change the constant and nothing changes. |
| `DawnTicks` | `:146` | **new.** `UpdateDayPhase` takes dawn as the leftover (`else p = DayPhase.Dawn`), so the constant is documentation. It agrees with the other three today and nothing would notice if it stopped. |

`TrackHoldTicks` — the reason this task exists — is **not** on this list. It was
wired while this guard was being built (WIRING-SPEC 1.1), and the guard now sees
its call site at `World.cs:865`. That is the instrument working in the direction
it is supposed to.

### `unitdef-single` — one unit carries the stat (11)

The Gun Mount shape. Several of these are probably correct and want an
allowlist entry tagged `unique:`; that judgement belongs to whoever owns the
balance, not to this task. A harness unit — any def named `Test ...` — does not
count as a carrier, because a fixture carrying a stat is not the game using it.

| symbol | carried by | consequence |
|---|---|---|
| `UnitDef.TraverseDegreesPerSecond` | Interceptor Battery (+ harness) | **F4.** The Gun Mount slews instantly. `SlewTicks` returns early on zero. |
| `UnitDef.AmmoCapacity` | Interceptor Battery (+ harness) | **F4.** The Gun Mount never reloads. FINDINGS §13–§18 were measured against it. |
| `UnitDef.Ammo` | Interceptor Battery | only one unit has chosen its ammunition; `AmmoType.Airburst` and `Buckshot` are implemented and carried by nobody |
| `UnitDef.SensorAcoustic` | Gun Mount (+ harness) | one unit in the game hears anything, while the acoustic table is fully applied to every unit's signature |
| `UnitDef.CanReachHigh` | Gun Mount (+ harness) | everything else can reach the high band, including things that should not |
| `UnitDef.SpoolLengthMetres` | Fiber FPV Team | plausible — one fiber airframe |
| ~~`UnitDef.HasCelestialHeading`~~ | Jet Strike Drone, Cruise Jet Drone | **off the ledger.** A second turbojet carries it - the 90 m/s rung FINDINGS 41 said the roster was missing - so the field is no longer one unit's. |
| `UnitDef.AutonomyQuality` | Autonomous Munition | correct by design, per FINDINGS 30 |
| `UnitDef.NightOnly` | Night Bomber | plausible |
| `UnitDef.MinesCarried` | Night Bomber | plausible |
| `UnitDef.MineDamage` | Night Bomber | plausible |

### `test-only` — the simulation never touches it; the test suite does (11)

The category that produced FINDINGS 30. Each of these has a passing test and no
production caller.

| symbol | where | consequence |
|---|---|---|
| `ReferenceImagery.GrantAround` | `ReferenceImagery.cs:73` | **F6 / WIRING-SPEC 2.2.** Nothing grants imagery in a real match. |
| `ReferenceImagery.Invalidate` | `ReferenceImagery.cs:94` | **F6 / WIRING-SPEC 2.2.** Nothing destroys it either. |
| `AutonomyClassifier.IsPilotedWithClearFeed` | `AutonomyClassifier.cs:232` | the piloted-decoy distinction is only ever asked about by a test |
| `TetherSystem.AnySegmentNear` | `TetherSystem.cs:316` | nothing in the game can stumble onto a filament — the other half of `TetherFound` |
| `CrewPool.SetHomeQuarters` | `CrewPool.cs:56` | no match ever places the crews' quarters |
| `GroundState.Frozen` | `Enums.cs:264` | no match ever freezes the ground |
| `EventRing.CountOf` | `SimEvents.cs:93` | expected — it is an assertion helper |
| `Fix.Ratio` | `Fix.cs:42` | maths primitive, tested and unused |
| `Fix2.ClampMagnitude` | `Fix2.cs:54` | maths primitive, tested and unused |
| `Trig.Direction` | `Trig.cs:97` | maths primitive, tested and unused |
| `DetRandom.ChancePercent` | `DetRandom.cs:91` | maths primitive, tested and unused |

`Territory.SetVerticalBorder` was on this list when the guard was written and
came off it during the session: the balance harness now draws a border
(WIRING-SPEC 0.2). Recorded because it shows the category behaving.

### `enum-value` — a state nothing can enter (7)

| symbol | where | consequence |
|---|---|---|
| `CrewState.Reserved` | `Enums.cs:75` | **F20.** The player cannot hold a crew back. |
| `SortiePhase.Engaged` | `Enums.cs:84` | **F20.** Never entered. |
| `SortiePhase.Terminal` | `Enums.cs:85` | **F20.** "Committed, ignores new orders, cannot be recalled" — never entered. Commitment is driven only by link pip. |
| `SortiePhase.Returning` | `Enums.cs:86` | **F20.** Never entered; nothing comes home (see `SortieSystem.Recover`). |
| `CommandKind.DropPin` | `Commands.cs:26` | **new.** No command reaches the simulation. |
| `CommandKind.SetAutonomyBox` | `Commands.cs:30` | **new.** The other end of the four dead `AutonomyState.Box*` fields below. |
| `RandomStream.AIJitter` | `DetRandom.cs:22` | **new.** A named deterministic stream nothing draws from. Harmless today; the moment something draws from it, every subsequent match changes, so it is worth knowing it is unused. |

### `write-only` — filled in and never read (18)

| symbol | written at | consequence |
|---|---|---|
| `EntityTable.Velocity` | `MovementSystem.cs:87` + 4 | **new, and the largest of these.** Movement computes a velocity, stores it, and nothing ever reads it — not the next tick's integration, not the state hash. |
| `EntityTable.HpMax` | `World.cs:121`, `:295` | **F20.** No damage ratio, no health bar, no repair. |
| `AutonomyState.BoxMin` | `World.cs:265` | **F20-adjacent.** The target box has no producer and no consumer. |
| `AutonomyState.BoxMax` | `World.cs:266` | as above |
| `AutonomyState.BoxExpiryTick` | `World.cs:267` | as above |
| `AutonomyState.HasBox` | `World.cs:268` | as above |
| `JamEmitter.Source` | `World.cs:422` | **F20.** Nothing can trace a jammer back to the unit radiating. |
| `DecoyState.Mimics` | `World.cs:301` | **F20.** A decoy does not mimic anything in particular. |
| `MoverState.RadiusClass` | `World.cs:142` | **F20.** No collision radius is ever consulted. |
| `MineState.LaidByTeam` | `World.cs:326` | **new.** A mine does not know whose it is — so it cannot avoid killing its owner. |
| `SalvageState.CreatedTick` | `World.cs:343` | **new.** Salvage decay is driven from elsewhere; this is the second, unread clock. |
| `SortieState.SpawnTick` | `World.cs:189` | **new.** |
| `SortieState.EgressUntilTick` | `World.cs:190` | **F19.** Staggered egress is computed and ignored, exactly as the audit says. |
| `MeshNode.IsRepeater` | `World.cs:452` + 5 | **new.** Six writes. The mesh graph records which nodes relay and never asks. |
| `TetherNode.Tile` | `TetherSystem.cs:92` + 2 | **new.** The terrain under each filament node is recorded and never used — and the snag rate is the one place terrain currently matters. |
| `Tether.Anchor` | `TetherSystem.cs:86` | **new.** |
| `SimEvent.A` | `SimEvents.cs:81` | expected: the event payload has no consumer until the interface exists |
| `SimEvent.B` | `SimEvents.cs:82` | as above |

### `sim-public` — public and never called (16)

| symbol | where | consequence |
|---|---|---|
| `Terrain.BlocksGroundSight` | `Terrain.cs:91` | **F1 / FINDINGS 29.** The one that started this. Zero callers, so nothing occludes anything, ever. |
| `SortieSystem.Recover` | `SortieSystem.cs:208` | **F13.** Reusable airframes never land, so crews never come home. |
| `SignalGrid.JamAtCell` | `SignalGrid.cs:65` | the jam field can be sampled and nothing samples it |
| `SignalGrid.EmitterCount` | `SignalGrid.cs:43` | interface-shaped |
| `SignalGrid.GetEmitter` | `SignalGrid.cs:44` | interface-shaped |
| `TetherSystem.LiveCount` | `TetherSystem.cs:65` | interface-shaped |
| `MeshGraph.MaxNeighbourCandidates` | `MeshGraph.cs:30` | a limit on a neighbour search the graph does not perform |
| `SimConstants.Millis` | `SimConstants.cs:22` | the sibling of `Seconds`, which is used constantly |
| `Fix.Half`, `Fix.ToInt`, `Fix.Max`, `Fix.Lerp`, `Fix.Sign` | `Fix.cs` | five unused fixed-point primitives |
| `Fix2.FromInts`, `Fix2.Perpendicular` | `Fix2.cs` | two more |
| `Trig.BamPerEntry` | `Trig.cs:19` | a table constant nothing indexes with |

---

## The allowlist, in full

Four entries. It is short on purpose: a large opening allowlist would have made
the instrument agree with the codebase, which is not what it is for.

| category | symbol | reason |
|---|---|---|
| `sim-public` | `SortieSystem.LaunchableCount` | `interface-layer` — the sidebar's honest "how many can I launch" count; audit F20 records it as expected until there is a UI |
| `sim-public` | `Fix.CompareTo` | `language` — `Fix` declares `IComparable<Fix>` |
| `sim-constant` | `SimConstants.TicksPerCommandTurn` | `planned:networking` — the lockstep turn length; audit F20 records it as expected |
| `enum-value` | `NavAid.DeadReckoning` | `symmetry` — the zero value and the documented default in `UnitDef`; constructed constantly, never written by name |

Everything else went into the baseline rather than the allowlist, including
things that are probably fine (the maths primitives, `SimEvent.A`/`B`). A
warning that somebody has to look at once is cheaper than a suppression nobody
will ever look at again.
