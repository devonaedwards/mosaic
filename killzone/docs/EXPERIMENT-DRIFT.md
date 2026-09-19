# Experiment drift: the first run

KILL ZONE is a video game. This is the output of
`tools/check_experiment_drift.py`, the build guard added alongside
WIRING-SPEC's dead-symbol guard, on the day it was added — `check_dead_symbols.py`'s
sibling, same build step, same ratchet philosophy, a different failure mode.

**Nothing here is fixed by this document or by the guard.** `src/KZ.Balance`
belongs to other agents. This is the instrument and its first reading.

---

## The two failures it catches

**Drift.** FINDINGS.md 15's saturation table was recorded as 10/72/98.
Re-measured on the tree immediately before this session's changes landed, the
same experiment gave 37/92/100. Nobody had touched the experiment; the
simulation moved underneath it and the document kept the old numbers, because
nothing re-ran an experiment and compared it against what was written down.

**Ceiling.** FINDINGS 32: Stacking, Vertical and Decoy Escort produced
byte-identical output across six builds spanning a corrected weapon range, a
magazine, a traverse rate, a track hold, twenty corrected signature numbers, a
realistic world, and an engagement-commitment mechanic. They are nonetheless
the sole evidence behind five findings. The reason was legible in their own
tables the whole time: *“every row of all three tables sits on a ceiling ... and
a measurement pinned to a ceiling is a constant, not a result.”* An experiment
whose output cannot move is dead in exactly the sense a symbol nothing calls is
dead — the dead-symbol guard is the argument that this class of problem is worth
catching mechanically, applied to the balance harness instead of the
simulation's public surface.

**That second signal was replaced in FINDINGS 40, and everything below is
written as the guard now stands.** It used to infer deadness by correlation — an
experiment that sat still while at least three *other* experiments moved was
reported “suspected inert” — which conflated *cannot move* with *correctly
unaffected* and reached seven warnings out of ten before anybody acted on one.
It now measures the property directly in the experiment's own output. The old
form is described where it is needed to read this document's history.

## How to read the guard

```
./build.sh                    compile, test, dead symbols, experiment drift
./build.sh driftcheck          the guard on its own
python3 tools/check_experiment_drift.py                    report, exit status
python3 tools/check_experiment_drift.py --list              machine-readable
python3 tools/check_experiment_drift.py --all                full detail
python3 tools/check_experiment_drift.py --selftest            its own corpus
python3 tools/check_experiment_drift.py --update-baseline      acknowledge current output as the record
```

Three files hold the record:

| file | what it means |
|---|---|
| `tools/experiment-drift-ledger.txt` | compact history: which generation each experiment's output last changed in, and what its digest is now. |
| `tools/experiment-baselines/<name>.txt` | the *current* accepted output for one experiment, in full. `git diff` on this file is the drift report for anyone who does not want to run the tool. |
| `tools/experiment-ceiling-allow.txt` | ceilings that are the comparison rather than a defect. One line per column, naming it by value and carrying a tagged reason. `tools/dead-symbols-allow.txt`'s shape and its rule: an entry without a reason is a warning being suppressed, and the checker rejects one. |

A **generation** is one accepted `--update-baseline` — not a build, not a
commit, not a clock tick. It means "someone looked at the current numbers and
said these are now the record", which is exactly the act FINDINGS 15 shows
never happened.

## Why a byte comparison is even valid here

Before trusting equality of two text blobs as a meaningful signal, it is worth
confirming the thing being compared is actually deterministic rather than
assuming it. `KZ.Balance` calls the real, shipped simulation, and the
simulation's determinism is a stated hard constraint in WIRING-SPEC: fixed-point
only, no `System.Math`, no loop bound by a converged value, no dependence on
dictionary or hash iteration order, and randomness drawn solely from
`DetRandom`'s four named streams, each seeded explicitly per trial
(`RunAssault(... ulong seed ...)`). Two runs of the same binary against the
same tree therefore produce identical stdout, and this checker leans on that
guarantee rather than re-deriving it from first principles.

It still checks its own assumption instead of trusting the argument alone:
every `--update-baseline` runs each experiment **twice** and refuses to record
anything if the two runs disagree. That would mean either a stream is not
actually seeded somewhere it should be, or something the harness prints is not
deterministic in the first place — and a checker whose whole method is a byte
comparison has to know the difference before it writes one down as fact.

The one real non-determinism risk this checker owns rather than the
simulation's: `Fix.ToString` and the harness's own `"0.0"`/`"0"` format calls
run under .NET's *current culture*, an environment property that has nothing
to do with the simulation. Every invocation this tool makes forces the
invariant culture (`DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`, `LC_ALL=C`,
`LANG=C`) so a decimal-comma locale can never be mistaken for a balance change.

## The decision about warning versus failing

Mirroring `check_dead_symbols.py`'s ratchet rather than inventing a new shape
for it:

- **Drift fails the build.** An experiment's output moving without anyone
  acknowledging it is not a hypothetical risk, it is the exact mechanism that
  produced FINDINGS 15's stale table. The fix is one command
  (`--update-baseline`) plus, separately, whoever owns `docs/FINDINGS.md`
  reading the diff and deciding whether a conclusion needs amending — cheap at
  the moment it is caught, and never cheaper later, which is the same argument
  `check_dead_symbols.py` makes for failing on a newly-dead symbol rather than
  warning.
- **A ceiling only ever warns.** It is a statement about the numbers and not
  about intent: the checker can see that a column is a constant and cannot see
  whether being a constant is the comparison the experiment exists to draw.
  VERTICAL's Gun Mount column reads 100% in all four rows because a Gun Mount
  cannot engage the High band at all, which is precisely the contrast the
  Autocannon arm beside it is read against; failing the build on that would be
  failing it on a correct experiment. So a ceiling is recorded debt or an
  allowlist entry with a reason, never a wall — same as the dead-symbol
  baseline, and for the same argument.
- **A crash always fails,** unconditionally. There is no number to compare, so
  there is nothing to warn about — either an experiment runs to completion or
  the checker cannot say anything at all.
- **A new experiment with no recorded baseline fails,** the same as drift:
  silently accepting whatever an unrecorded experiment happens to print right
  now is the same mistake as accepting a drifted one, just one generation
  earlier.
- **A stale ledger entry (an experiment `KZ.Balance` no longer produces) warns**
  and asks to be deleted, the same as a dead-symbols baseline line that stops
  matching anything.

## How a ceiling is judged, and what it cannot judge

The mechanism cannot read what any experiment measures, so it cannot ask “did
something relevant to this experiment change” directly — building that map by
hand would be exactly the kind of authority that goes stale silently, which is
the disease this whole project is trying to cure rather than a cure for it.

**What it used to ask instead, and why that is gone.** Across the generations
this experiment's output sat unchanged, did any of its nine siblings move? If
eight of ten changed and the ninth did not, the ninth was not obviously
insulated from what happened around it. That is a heuristic about a *population*
being used to accuse an *individual*, and it fails in both directions. SENSORS
compares optics, acoustic and thermal and carries no radar at all, so the
Doppler notch landing in FINDINGS 38 *should* have left it byte-identical; the
guard reported it as suspected inert. STACKING resolves from 11% to 100% across
its own table and moved at generation 13; the guard reported it as suspected
inert too, because the two most recent generations happened to move three other
experiments. By the time it was warning on seven of ten, six of them on that
same two-generation coincidence, it was a warning nobody could act on — which
is this project's own stated standard for a checker that should be replaced.
FINDINGS 40 has the measurement behind that paragraph.

**What it asks now.** Every percentage cell inside a table — a run of lines
under one of these reports' own `----` rules, prose excluded — grouped into
columns by the character offset its last digit lands on, because these tables
are printed through fixed-width format strings and every numeric column in them
is right-aligned. Then two properties of a column, either of which is a finding:

- **pinned** — at least half its cells read exactly 0% or 100%. Those are the
  bounds of a share-of-trials, so a cell on one is a cell that could not have
  moved further in the direction it is already at.
- **flat** — every cell in it is the same number. A variable was swept and the
  answer did not move.

A sweep is entitled to **one rung at each end of its range**, because that is
what bracketing a transition means: you want a rung the defence always survives
and a rung it never does, so you know the interesting band is between them. So
the first and last cell are dropped when they sit on a bound before the share is
taken. That is the difference between APPROACH (81, 86, 90, 86, 85, 94, 100 —
one rung off the end and six doing work) and the SATURATION table this guard
found (0, 0, 37, 98, 100, 100, 100, 100 — four rungs saying the same thing).

This is a measurement of the experiment's own output. It needs no sibling, no
generation and no history, which means it says the same thing on the day an
experiment is written as it does five generations later, and it cannot be
confused by a busy commit or a quiet one.

**One question is still asked of the ledger**, and only one: has this
experiment's output *ever* changed, across a recorded history long enough for
the silence to be a statement? That is `frozen`, and it is deliberately not
sibling-relative — it is a claim about the experiment and nothing else. Nothing
is frozen today; every one of the ten moved at generation 13.

**What a ceiling cannot tell you.** Whether it is a defect or the point.
VERTICAL's Gun Mount column is 100% in all four rows and is *supposed* to be:
a Gun Mount cannot reach the High band, so one high drone in the flight is
outside the defence rather than evading it, and that column is the contrast the
Autocannon arm beside it is read against. Measured before it was excused —
flights of two, three and four quads across every split give 99—100% in every
cell, so it is the units and not the rungs. That is what
`tools/experiment-ceiling-allow.txt` is for, and why an entry there names the
column by its values: the day the numbers move, the entry stops matching and the
warning comes back.

It also reads **percentages only**, and says so rather than implying more.
MINES prints “0 of 4” and “yes, after 1 mine(s)” and has no percentage cell
anywhere, so this detector is silent about it — correctly, because it has no way
to know whether “0 of 4” is a floor or a finding. VERTICAL's quads-lost column
reads 0.08, 0.01, 0.00, 0.00, which is a floor in everything but type, and this
misses it. A bounded-measure detector needs to know the bounds, and only the
percent sign declares them.

And it says nothing at all about **coverage**. An experiment can be perfectly
unpinned and still exercise none of the systems that changed this week. That is
a different question and this guard has never been able to answer it. FINDINGS
40 answered it by hand for the ten that existed then, and the last section of
this document is the tool that asks it now.

## The first run

Generation 1 does not start this ledger empty. Ten balance experiments' worth
of history was backfilled from real commits already in this repository's
history — `git worktree` checkouts of each one, compiled and run through the
same `--update-baseline` path a live run uses, in chronological order — rather
than waiting months of organic builds for the inertia signal to mean anything.
Each generation below is tied to the real commit it was captured from, listed
in `tools/experiment-drift-ledger.txt`'s `generation` header lines, so the
history is auditable back to source rather than asserted:

| gen | commit | what changed in it |
|---|---|---|
| 1 | `2b37d16` | earliest commit where all ten experiments already exist |
| 2 | `ae2faae` | gun mount kill ring, altitude ceilings, magazines added |
| 3 | `00d7f42` | F4/F10/F23/F24/F22: traverse rate, magazine, altitude modifiers, channel reliabilities, the twenty-number thermal-optical signature table |
| 4 | `27c051c` | the track hold wired to `World.cs` |
| 5 | `90ee5fa` | one radar signature value reverted (a twenty-point scale error) |
| 6 | `602341c` | the harness stops lying: gun mount's real 85 m range, realistic world, override removed |
| 7 | `f6fb277` | `BlackPolicy` wired |
| 8 | `d979139` | the magazine field renamed to count engagements, not rounds |
| 9 | `ad7f15c` | the engagement-commitment mechanic (one channel per mount) |
| 10 | `ab00282` | the autocannon tier added |
| 11 | `cf66a42` | this session's tree at the time the guard landed (BlackPolicy, the gun-mount lay/hold fix, and the honest FINDINGS re-run all included) |

That is every substantive change FINDINGS 32 names, plus more, run through the
same instrument. What it found, at generation 11:

```
experiment-drift check: generation 11, 10 experiment(s) tracked

  WARN  7 experiment(s) suspected inert
    aperture    unchanged for 6 generations (gen 6-11), 4 other experiments changed
    decoys      unchanged for 6 generations (gen 6-11), 4 other experiments changed
    mines       unchanged for 6 generations (gen 6-11), 4 other experiments changed
    range       unchanged for 3 generations (gen 9-11), 3 other experiments changed
    sensors     unchanged for 6 generations (gen 6-11), 4 other experiments changed
    stacking    unchanged for 6 generations (gen 6-11), 4 other experiments changed
    vertical    unchanged for 6 generations (gen 6-11), 4 other experiments changed

  10 ok, 0 failing, 7 warning
```

(`--all` shows the full generation-by-generation evidence behind each row;
run it to see exactly which siblings moved where.)

The three experiments FINDINGS 32 already named by hand — **Stacking, Vertical,
Decoy Escort** — are in that list, flagged without being told to look for them.
That is the guard working as intended. It also found **four more** FINDINGS 32
never named — Mines, Sensors, Aperture and Range — sitting in the same shape:
unmoved since generation 6 (Range since generation 9) while several siblings
changed underneath them. Whether all seven deserve the same verdict is a
judgement call for whoever owns `src/KZ.Balance`; what the guard contributes is
that none of the seven can hide behind "probably fine" any longer, and neither
can an eighth if one shows up the same way later. **Nothing about any of them
was fixed here** — `src/KZ.Balance` belongs to other agents; wiring one back
onto its knees, or confirming nothing it exercises has changed, is their call
to make, not this checker's.

Worth noting exactly what the ledger shows and does not show: all seven reset
together at generation 6 — a genuine, verified change, not a false read (`602341c`
removed the 550 m range override and switched the harness to a realistic
world). FINDINGS 33 already argues the *realistic-world* half of that commit
provably changes nothing on its own (terrain occludes nothing yet); the range
correction bundled into the same commit is the more likely cause of the reset.
Generations are real commits, and a commit can bundle more than one conceptual
change - the guard cannot separate them, only report that *something* in that
commit moved these seven. Since that reset, six of the seven have not moved
again across five further generations covering `BlackPolicy`, the magazine's
unit change, the engagement-commitment mechanic, the autocannon tier, and a
gun-mount lay/hold fix - which is the FINDINGS 32 shape, continuing.

One more thing this run turned up, from manual exploration alongside building
this guard rather than from the guard itself (the guard has no way to explain
*why*, only *that*): every one of these experiments launches its drones in a
single simultaneous wave, so a mount that can hold only a handful of
engagements gets exactly one engagement window per trial regardless of its
magazine size. Staggering the launch times by a few seconds was enough to move
a mount's reload count from zero out of sixty trials to sixty out of sixty in
a manual test. That is a plausible, concrete mechanism behind at least part of
this list - a magazine-relevant number cannot move in an experiment that never
gives the mount a second window to use it - but it is a hypothesis about *why*,
offered for whoever owns `src/KZ.Balance` to check, not a finding this guard
verified or a fix this task made.

## What it cannot see, stated so nobody trusts it too far

- It compares **stdout text**, not the simulation state the text summarises.
  Two different internal states that happen to format to the same string would
  read as unchanged. This is not expected given how the experiments print (raw
  counts and percentages straight from a trial loop, no rounding step hidden
  behind formatting) but it is a limit of the method, not a guarantee against
  it.
- It has no notion of *which* systems an experiment exercises. It will never
  say “Stacking should have moved because the gun's range changed and Stacking
  spawns a gun mount.” It used to substitute a sibling-relative guess for that,
  and the guess was wrong more often than it was right; it now says nothing
  about coverage at all. That is honest and it is also a real gap — FINDINGS 40
  worked the current ten out by hand, one mutation at a time, and the answer
  was worse than anybody had assumed. **That gap is what the third guard at the
  end of this document is**, and the division of labour is worth stating: this
  file asks whether an experiment's answer changed, and
  `check_experiment_mutations.py` asks whether it *could*.
- A change that only affects an experiment's *prose* (the commentary strings
  printed alongside a table) counts as drift the same as a change to its
  numbers, because the checker reads the whole of stdout. That is deliberate —
  the harness's own commentary is content the audit above already leans on —
  but it means a wording fix and a balance change look identical to this tool
  until a human reads the diff it prints.

---

# The third guard: mutation

`tools/check_experiment_mutations.py` answers the one question the two guards
above are structurally unable to ask, and the reason it exists is written in
this document already:

> It has no notion of *which* systems an experiment exercises. It will never say
> "Stacking should have moved because the gun's range changed and Stacking
> spawns a gun mount." ... FINDINGS 40 worked the current ten out by hand, one
> mutation at a time, and the answer was worse than anybody had assumed.

That hand sweep is now a tool. The method is FINDINGS 40's, unchanged: take a
constant, change it to something visibly wrong, rebuild, run the experiments,
compare byte for byte against the same run on the unmodified tree. **An
experiment that does not move has no path to that constant** — not an opinion
about coverage, a measurement of it.

## What it reads, and where each thing is written down

Two files, and which fact lives in which was the only real design decision.

| where | what | why there |
|---|---|---|
| `src/KZ.Balance/Program.cs`, a `// MEASURES <experiment>: <mutation>, ...` line beside each experiment | the **claim** — which mutations this experiment says it survives | a manifest is a second place to remember and the person rewriting an experiment does not open it. A line three lines above the method is in the diff of every rewrite. |
| `tools/experiment-mutations.txt` | the **mutation** — file, the exact text to replace, what to replace it with, and one line of prose saying what the simulation stops doing | described once, so the cost of the hundredth mutation is four lines. The experiments reference it by name. |

A claim is keyed by the experiment's dispatch name rather than by whatever
method it happens to sit above, so a claim that drifts away from its experiment
during an edit stops matching loudly instead of re-attaching itself to the
neighbour.

## The rules, and each one has a failure behind it

- **A `from` text must match its file exactly once.** Zero matches means the
  constant has been renamed, retyped or deleted and the mutation has been
  testing nothing ever since; two means there is no saying which one it broke.
  Both stop the sweep rather than skipping the entry — this repository's whole
  argument about checkers is that one which has quietly stopped catching things
  is worse than none, and a mutation that no longer applies is exactly that.
- **`breaks` is required.** It is the one thing the tool's output leaves to a
  person: whether this experiment *should* have noticed. An entry without it is
  `dead-symbols-allow.txt`'s unreasoned entry, and the checker rejects it the
  same way.
- **A claimed mutation the experiment does not notice fails.** That is the whole
  point and it is the acceptance test for a new experiment.
- **A mutation nothing claims warns**, and it is the most useful line the tool
  prints: a named system with no experiment behind it. FINDINGS 40's finding,
  standing rather than written down once and going stale. Adding a mutation for
  a system you have just wired, before any experiment covers it, is the intended
  use.
- **An experiment that declares nothing fails.** There is no baseline file and
  no ratchet, deliberately: unlike the dead-symbol guard this one did not
  inherit a backlog, because a claim is only ever added after the sweep has
  verified it. Everything unverified shows up as an uncovered mutation instead,
  which is a warning and a visible gap rather than a suppressed failure.

## Why it is not in `./build.sh`

It is a rebuild and a full experiment run per mutation — minutes, not seconds.
This document has already recorded what this project thinks of a slow check in
the default path, in the passage about the drift guard costing "roughly as long
as `./build.sh balance` does". A check that is slow enough to be skipped gets
skipped, and then it reads as evidence while being none. So it has its own
subcommand:

```
./build.sh mutations                         the sweep
./build.sh mutations --list                  the declarations, no build
./build.sh mutations --only <mutation>       one mutation
./build.sh mutations --experiment <name>     one experiment's claims
python3 tools/check_experiment_mutations.py --selftest    its own corpus
```

**How often.** When a balance experiment is written or rewritten — it is that
experiment's acceptance test, and "it moves when the thing it measures is
broken" is the claim being made. When a mutation is added for a freshly wired
system. And before `docs/FINDINGS.md` cites an experiment as evidence that a
system works, because that citation is precisely the claim this tool exists to
falsify. Otherwise about once a session.

It builds scratch copies of the tree and never touches the working tree, so it
is safe to run while reading something else, and `--keep` leaves the mutated
trees behind when a result wants looking at.

## What it cannot see, stated so nobody trusts it too far

- It proves an experiment has a **path** to a constant. It does not prove the
  experiment measures it well, reports it honestly, or that the prose beside the
  number means what it says. A one-cell twitch counts as movement.
- It reads stdout, like the drift guard, so two different internal states that
  format identically read as unchanged.
- **Its catalogue is written by hand**, so it can only ask about systems somebody
  thought to name. An unmutated constant is invisible to it. That is why the
  uncovered-mutation line is phrased as a gap and never as a clean bill of
  health: the tool's silence about a system is silence, not evidence.
