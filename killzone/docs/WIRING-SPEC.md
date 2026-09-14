# Wiring specification

KILL ZONE is a **video game** — a real-time strategy game with fictional
factions, hit points and build timers. This document specifies how the work in
`docs/AUDIT-UNWIRED.md` gets done.

The audit found 34 dead items, 32 missing, 14 contradicted. This is not a list of
features to add. It is a list of connections that were never made between
research that is good and systems that are written, and the order matters more
than the volume.

---

## The rule that caused all of this

> **A passing test proves the wire exists only where the test itself built it.**

Three systems shipped inert this session — navigation error, reference imagery,
territory — each with passing tests. The tests passed because the tests
constructed the state. Nothing else ever did.

So every piece of work below carries one non-negotiable acceptance condition:

**A wire is not wired until a test exercises it through the production path.**
Spawn a unit through `World.Spawn`, run `World.Step`, and observe the effect.
A test that hand-assigns a component, calls a system directly, or constructs the
state it is about to assert on does not count as evidence and must not be the
only test for that item.

---

## Hard constraints

These bind every task. An agent that breaks one has produced a defect, not a
feature.

**Determinism is the product.** The simulation must produce bit-identical results
on an iPad and a PC across tens of thousands of ticks. Therefore:

- Fixed-point only. No `float`, no `double`, no `System.Math` in `KZ.Sim`.
  `Fix.FromDoubleContentOnly` is for authoring constants at load time and is
  never valid inside a tick.
- No loop whose iteration count depends on a converged value.
- No dependence on dictionary or hash iteration order.
- Randomness only through `World.Random.Get(RandomStream.X)` on a named stream.
  Never add a call to an existing stream without checking who else draws from it;
  changing the draw order changes every subsequent match.
- `StateHash()` must include any new persistent state. The determinism tests will
  catch a miss, and they are the most valuable tests in the suite.

**The tick budget is real.** Detection is already the most expensive thing per
tick — every sensor against every target, every tick. Anything added to that
inner loop needs an argued cost, not an assumption. If a change is O(entities²)
per tick, say so in the pull request and propose the cheaper form.

**No invented numbers.** Every content value must cite its source in a comment:
the research document and section. If the research does not give a number, say
so in the comment and mark it as a designer estimate. Tuned data that nothing
reads is the disease this whole exercise exists to cure — do not add more.

**Comments explain why, not what.** Match the surrounding style. The codebase
explains its reasoning; a diff that adds mechanism without reasoning is a
regression in quality even when the mechanism is right.

---

## Phase 0 — stop the bleeding, and fix the instrument

Nothing else is trustworthy until these land. **Do these first, in parallel.**

### 0.1 The dead-symbol build guard  `[opus]`

The three worst findings — terrain occlusion, the track hold, the gun mount —
were all invisible to reading and trivially visible to grep. This must become a
build step, not a lesson learned a fourth time.

Add a checker that fails the build on:

- a `public` member in `KZ.Sim` with zero call sites outside its own declaration
- a `SimConstants` value never referenced
- a `SimEventKind` never pushed
- a `UnitDef` field never read outside `Defs.cs`
- an enum value never constructed or compared
- a `ComponentMask` flag never tested

It needs an explicit allowlist with a **written reason per entry** — some
surface is legitimately for the interface layer or for tests. An allowlist entry
without a reason is a bug being suppressed.

This is `[opus]` because the allowlist design is a judgement call and a checker
that cries wolf will be disabled within a week.

**Landed.** `tools/check_dead_symbols.py`, run by `./build.sh` after the tests.
It checks the six above plus four more — a `UnitDef` field no unit sets, a field
exactly one unit sets (the gun mount's traverse rate is not unreferenced, it is
carried by one unit and read behind a zero guard), a field written everywhere and
read nowhere, and, loudest, a symbol only `src/KZ.Tests` touches. That last is a
category rather than an allowlist reason on purpose: green tests with no
production caller is the failure that produced FINDINGS 30, and it deserves to be
named rather than excused.

**It does not fail the build on the backlog it found, and that is deliberate.**
The first honest run found 86 symbols. Failing on all of them would have stopped
every other task in this document for a backlog nobody in this session caused,
and a build step that does that is deleted within a week. So it is a ratchet: the
86 are recorded in `tools/dead-symbols-baseline.txt` and warn; **anything not in
that file fails the build, in every category, immediately**. The promotion
condition is not a date — it is the baseline reaching zero, one line at a time,
as the tasks below land. Full reading, per-category counts, and what the checker
cannot see: `docs/DEAD-SYMBOLS.md`.

### 0.2 The harness lies  `[sonnet]`  — audit F33, F34

Three balance experiments overwrite the Gun Mount's range back to **550**, the
figure corrected to 85. Every experiment world is `Fill(Open)`, clear weather,
firm ground, no territory owner, no imagery.

- Delete the overrides. An experiment that needs a different unit adds a unit to
  the catalogue; it does not mutate the catalogue behind the reader's back.
- Give experiments a realistic default world: mixed terrain, a weather state, a
  ground state, territory ownership, imagery coverage.
- Keep a deliberately flat control world, **named as such**, so a comparison
  against the old conclusions is still possible.

Until this lands, no experiment result means anything — including the ones
already recorded in `FINDINGS.md`.

---

## Phase 1 — cheap corrections, largest effect

All small, all high-consequence, all blocked on nothing.

| | item | size | model |
|---|---|---|---|
| 1.1 | **F2** track hold — `TrackHoldTicks` has zero call sites; every fringe contact re-rolls 32×/s | ~20 lines | `[sonnet]` |
| 1.2 | **F4** gun mount traverse + magazine — two numbers in `Defs.cs` | 2 lines | `[fable]` |
| 1.3 | **F10** three altitude modifiers carry the sign the research says is wrong | 3 constants | `[fable]` |
| 1.4 | **F23** channel reliabilities reversed for aerial targets | small | `[fable]` |
| 1.5 | **F24** radar signature ordering inverted for the heavy multirotor | 1 number | `[fable]` |
| 1.6 | **F22** `thermal-optical.md` §11 signature table never applied — 20 numbers, and the visual column feeds the gun's hit probability | 20 numbers | `[fable]` |

**1.2 has a consequence that must be stated in its commit:** FINDINGS §2, §13,
§15, §16 and §18 were all measured against this unit with infinite ammunition
and instant slew. Once it lands, the vertical-attack and stacking experiments
must be re-run and those findings amended. Expect conclusions to change.

---

## Phase 2 — connect what I built and left inert

My own work from this session. Ordered because 2.3 unblocks the other two.

### 2.3 first — `Territory` is never populated  `[sonnet]`  — audit F6

Nothing outside a test ever calls `SetVerticalBorder` or `Fill`. So in any real
match every satellite link is permanently black and scene matching can never
lock. Mission and experiment setup must place a border. This is the single
highest-leverage line in Phase 2: three systems are dark behind it.

### 2.1 Navigation error consumed by nothing  `[sonnet]`  — audit F5

`NavState.ErrorMetres` is computed by the whole `NavigationSystem` and read only
by tests. It must displace the aimpoint of a munition arriving under denial.
Per `navigation-denied.md` §5: the error applies at the moment of arrival, and
the navigation penalty and the autonomy penalty are **separable** — a drone that
brought a map keeps its position and loses only its operator.

### 2.2 Reference imagery has neither end connected  `[sonnet]`  — audit F6

`GrantAround` and `Invalidate` have zero callers. Wire the supply end to
reconnaissance sorties and the destruction end to heavy bombardment, per
`navigation-denied.md` §6 — a **coverage** resource invalidated by visible
events, never a freshness timer.

---

## Phase 3 — the economy has one direction  `[sonnet]`  — audit F3

Salvage is spawned, decays, and is never collected. `SalvageCollected` is never
pushed. `Materiel` only decreases, from a 4,000 starting balance. Tasking Points
are credited and never spent.

So **no economic conclusion drawn from this simulation is about an economy**,
including anything in `economics.md` that was checked against it. Wire
collection, then the second valve from FINDINGS §24: money gates production,
crews gate employment.

---

## Phase 4 — structural absences

Larger, and two are blocked on research still landing.

- **4.1 F1 terrain occlusion** `[opus]` — blocked on `terrain.md` completing.
  Per-channel, not a single flag: radar absolute, acoustic a frequency-dependent
  diffraction loss, passive RF a 0.3–0.5 multiplier, control link a binary drop
  restored by an elevated relay. The open question is whether a per-pair
  per-channel test fits the tick budget at all; a cheaper abstraction that
  captures most of the behaviour is an acceptable and possibly correct answer.
- **4.2 F7** radar cannot see ground targets; no Doppler notch `[opus]`
- **4.3 F8, F9** radiating radars invisible to ESM; nothing can switch off; an
  ESM bearing treated as a firing solution `[opus]`
- **4.4 F11** no ground-clutter term, which the thermal research calls the
  largest factor in its model `[sonnet]`
- **4.5 F13, F15, F16, F17, F18, F19** reusable airframes never land so crews
  never return; four structures do nothing; blankets and cages cannot be fitted;
  the target box has no producer; the piloted-decoy rule is never applied;
  staggered egress is computed and ignored `[sonnet]`

---

## Phase 5 — the remainder

F14, F20, F25–F32. Work them in audit order once Phase 4 is done.

---

## How work is dispatched

**`[fable]`** — mechanical and fully specified. Constants, table transcription,
sign flips, field population. No judgement, no architecture, no new tests beyond
the production-path assertion.

**`[sonnet]`** — a contained system with a clear specification and a known
answer. Wiring an existing mechanism to an existing caller.

**`[opus]`** — anything touching the detection inner loop, the determinism
guarantees, the tick budget, or a design decision the research leaves open.

Every task ships: the change, a production-path test, a commit message that says
what the research said and what the code was doing, and an explicit note of any
`FINDINGS.md` conclusion the change invalidates.

Findings get amended, never quietly deleted. The record of having been wrong is
the most useful thing in this repository.

---

## Process note: do not `git add -A` while agents are working

Added after it went wrong. The track-hold work (task 1.1) was complete and
uncommitted in the working tree when I ran `git add -A` to commit an unrelated
research document. It swept the agent's code and both its tests into a commit
about ground-force research, under a message that says nothing about detection.

The code is intact and the tests pass, so nothing was lost — but the history now
misattributes a significant simulation change, and history is the only record of
*why* a number is what it is. This repository's whole method depends on being
able to read back the reasoning behind a change.

The history was not rewritten to fix it: a later commit already builds on top,
and the tree is shared with running agents. Amending would have been the more
expensive mistake.

**The rule, from here:** while any build agent is running, stage explicitly by
path — `git add docs/research/foo.md` — never `git add -A` or `git commit -a`.
If the tree must be cleared and the ownership of a change is unclear, commit it
as an explicit work-in-progress snapshot that says whose work it is and that it
is mid-flight.
