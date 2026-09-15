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

**Inertia.** FINDINGS 32: Stacking, Vertical and Decoy Escort produced
byte-identical output across six builds spanning a corrected weapon range, a
magazine, a traverse rate, a track hold, twenty corrected signature numbers, a
realistic world, and an engagement-commitment mechanic. They are nonetheless
the sole evidence behind five findings. An experiment whose output cannot move
is dead in exactly the sense a symbol nothing calls is dead — the dead-symbol
guard is the argument that this class of problem is worth catching mechanically,
applied to the balance harness instead of the simulation's public surface.

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

Two files hold the record:

| file | what it means |
|---|---|
| `tools/experiment-drift-ledger.txt` | compact history: which generation each experiment's output last changed in, and what its digest is now. |
| `tools/experiment-baselines/<name>.txt` | the *current* accepted output for one experiment, in full. `git diff` on this file is the drift report for anyone who does not want to run the tool. |

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
- **Inertia only ever warns.** It is a heuristic inferred from sibling
  behaviour (see below), not a proof, and the checker says so in its own
  output ("suspected"). Failing the build on an uncertain signal is exactly
  how a checker earns the reputation that gets it disabled within a week — and
  concretely, it would mean this guard's own first commit could not land
  without either fixing Stacking, Vertical and Decoy Escort (another agent's
  work, out of scope here) or lying about them. Recorded debt, not a wall,
  same as the dead-symbol baseline.
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

## How inertia is judged, and what it cannot judge

The mechanism cannot read what any experiment measures, so it cannot ask "did
something relevant to this experiment change" directly — building that map by
hand would be exactly the kind of authority that goes stale silently, which is
the disease this whole project is trying to cure rather than a cure for it.
What it can ask, mechanically: across the generations this experiment's output
has sat unchanged, did **any of its nine siblings** move? If eight of the ten
balance experiments changed their numbers across the same span of
`--update-baseline`s and the ninth did not, the ninth was not obviously
insulated from everything that happened around it — which is the shape FINDINGS
32 describes in six real builds, turned into a standing check.

An experiment is reported **suspected inert** when it has been unchanged
across a run of generations in which at least three siblings changed (each
finding names the generations and which siblings moved, so the evidence is
inspectable rather than asserted). An experiment too young to have accumulated
that much evidence either way — new, or only recently settled — is reported as
having **insufficient history**, honestly, rather than folded into "clean".

What this cannot do, stated so nobody trusts it further than it goes: it
cannot distinguish an experiment that is *dead* from one that is *correctly and
permanently flat* when both sit in the same span of sibling movement. Aperture
or Mines could legitimately never need to move again; this heuristic has no way
to know that in advance, only that they haven't yet. That is why the finding
says "suspected" and shows every generation and sibling behind the number
rather than asserting "dead", and why it warns rather than fails — the
judgement belongs to whoever maintains `src/KZ.Balance`, not to this checker.

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
- It has no notion of *which* systems an experiment exercises, so its inertia
  signal is entirely sibling-relative — see "How inertia is judged" above. It
  will never say "Stacking should have moved because the gun's range changed
  and Stacking spawns a gun mount"; it can only say "everything else moved and
  this didn't."
- A change that only affects an experiment's *prose* (the commentary strings
  printed alongside a table) counts as drift the same as a change to its
  numbers, because the checker reads the whole of stdout. That is deliberate —
  the harness's own commentary is content the audit above already leans on —
  but it means a wording fix and a balance change look identical to this tool
  until a human reads the diff it prints.
