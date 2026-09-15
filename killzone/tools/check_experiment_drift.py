#!/usr/bin/env python3
# KILL ZONE - a real-time strategy video game.
#
# The experiment-drift build guard: check_dead_symbols.py's sibling.
#
# Two failures motivate it, both found by hand, both mechanical:
#
#   DRIFT.  FINDINGS.md 15's saturation table read 10/72/98. Re-measured on the
#   tree immediately before this session's changes, the same experiment gave
#   37/92/100. Nobody edited the experiment; the simulation moved underneath it
#   and the document kept the old numbers. Nothing re-ran an experiment and
#   compared it to what was recorded.
#
#   INERTIA.  Three of the ten experiments - Stacking, Vertical, Decoy Escort -
#   produced byte-identical output across six builds spanning a corrected weapon
#   range, a magazine, a traverse rate, a track hold, twenty corrected signature
#   numbers, a realistic world, and an engagement-commitment mechanic. FINDINGS 32.
#   They are nonetheless the sole evidence behind five findings.
#
# An experiment whose output never moves is dead in exactly the sense a symbol
# nothing calls is dead. This is that check, run by build.sh.
#
# Why a byte comparison is even valid: KZ.Balance calls the real, shipped
# simulation, which is fixed-point only, draws randomness solely from
# DetRandom's four named streams seeded per trial, and never reads the wall
# clock, a GUID, or dictionary/hash iteration order for anything the harness
# prints (KZ.Sim/Math/DetRandom.cs, KZ.Sim/Content/Defs.cs). Two identical
# invocations of the same binary on the same tree therefore produce identical
# stdout - determinism is the product, per WIRING-SPEC's hard constraints, and
# this checker leans on that guarantee rather than re-arguing it. It still
# checks its own assumption: --update-baseline runs every experiment twice and
# fails loudly, without writing anything, if the two runs disagree. The one
# real non-determinism risk this checker owns is number formatting: Fix.ToString
# and the harness's "0.0"/"0" format strings run under .NET's *current* culture,
# which is an environment property, not a simulation one - so every invocation
# this file makes forces the invariant culture (see RUN_ENV below), and a
# genuine culture-dependent diff would be a bug in the harness, not a balance
# change, and is exactly the kind of noise a checker must not report as drift.
#
# What this file does NOT do: it does not know what any experiment's numbers
# mean, whether a change is good, or which finding a drifted table backs. That
# judgement belongs to whoever owns docs/FINDINGS.md. This is the instrument.
#
# Usage:
#   python3 tools/check_experiment_drift.py                 report, exit status
#   python3 tools/check_experiment_drift.py --list           one line per finding
#   python3 tools/check_experiment_drift.py --selftest        the regression corpus
#   python3 tools/check_experiment_drift.py --update-baseline capture, verify
#                                                             determinism, record
#   ... --exe "mono build/KZ.Balance.exe"    how to run the balance harness
#   ... --warn-only                          report but never fail
#   ... --all                                full detail, not just counts
#   ... --root DIR / --ledger FILE / --snapshots DIR    point at a different tree

import hashlib
import os
import re
import shlex
import subprocess
import sys

try:
    import signal
    signal.signal(signal.SIGPIPE, signal.SIG_DFL)
except (ImportError, AttributeError, ValueError):
    pass

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
FIXTURE = os.path.join(HERE, "testdata", "experiment-drift")

DEFAULT_LEDGER = os.path.join(HERE, "experiment-drift-ledger.txt")
DEFAULT_SNAPSHOTS = os.path.join(HERE, "experiment-baselines")

# An experiment that has not moved across this many *other* experiments'
# changes is reported as suspected-inert. Set at 3 rather than 1 so a single
# coincidental change elsewhere never accuses an experiment that simply had
# nothing to do with it - see "How inertia is judged" below.
INERTIA_MIN_OPPORTUNITIES = 3

TIMEOUT_SECONDS = 180

# Force the invariant culture on every child process, regardless of which
# runtime built it. dotnet honours the env var; mono honours LC_ALL/LANG. None
# of these is a simulation input - Fix is fixed-point and never consults any
# of them - so forcing them cannot change a real result, only strip out a
# formatting difference that is not one.
RUN_ENV_OVERRIDES = {
    "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT": "1",
    "LC_ALL": "C",
    "LANG": "C",
}


# ---------------------------------------------------------------------------
# Discovering which experiments exist.
#
# Read, never guessed: Main()'s dispatch table is the one place the set of
# experiments is declared, so a new experiment (or a renamed one) is picked up
# automatically the same way check_dead_symbols.py discovers new symbols by
# parsing, not by a hand-maintained list this file would go stale next to.

DISPATCH_RE = re.compile(r'which\s*==\s*"(\w+)"')


def discover_experiments(root):
    path = os.path.join(root, "src", "KZ.Balance", "Program.cs")
    with open(path, "r") as fh:
        text = fh.read()
    names = []
    for m in DISPATCH_RE.finditer(text):
        name = m.group(1)
        if name != "all" and name not in names:
            names.append(name)
    return names


# ---------------------------------------------------------------------------
# Capture and normalisation.


def normalize(text):
    """Strip the two things that can vary without the simulation having
    changed - line-ending style and trailing whitespace - and nothing else.
    Column alignment inside a table is content (it is how these reports show
    their numbers) so internal whitespace is untouched."""
    lines = text.replace("\r\n", "\n").replace("\r", "\n").split("\n")
    lines = [ln.rstrip() for ln in lines]
    while lines and lines[-1] == "":
        lines.pop()
    return "\n".join(lines) + "\n"


def digest_of(text):
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


class RunFailure(Exception):
    """An experiment did not produce output to judge - a crash or a timeout.
    Distinct from drift on purpose: a crash is not "the number changed", it is
    "there is no number", and it always fails regardless of --warn-only."""


def run_once(exe_argv, name):
    env = dict(os.environ)
    env.update(RUN_ENV_OVERRIDES)
    try:
        proc = subprocess.run(exe_argv + [name], capture_output=True,
                               timeout=TIMEOUT_SECONDS, env=env)
    except subprocess.TimeoutExpired:
        raise RunFailure("%s: timed out after %ds" % (name, TIMEOUT_SECONDS))
    if proc.returncode != 0:
        tail = proc.stderr.decode("utf-8", "replace").strip().splitlines()[-8:]
        raise RunFailure("%s: exited %d\n      %s" %
                          (name, proc.returncode, "\n      ".join(tail)))
    return normalize(proc.stdout.decode("utf-8", "replace"))


def capture(exe_argv, name, verify_determinism):
    """One experiment's normalised output. When verify_determinism is set (the
    case whenever this file is about to trust the result enough to write it
    down), the experiment is run twice and the two runs must agree byte for
    byte - the check described in the file header, applied rather than assumed."""
    first = run_once(exe_argv, name)
    if not verify_determinism:
        return first
    second = run_once(exe_argv, name)
    if first != second:
        raise RunFailure(
            "%s: produced two different outputs from two runs with nothing "
            "changed in between. Either it is not seeded (see DetRandom.Get "
            "call sites in the systems it exercises) or something it prints "
            "is not deterministic (wall clock, iteration order, a float). "
            "This checker's whole method depends on a byte comparison being "
            "valid, so it refuses to record a baseline for an experiment it "
            "just caught being non-deterministic." % name)
    return first


def capture_all(exe_argv, names, verify_determinism):
    outputs, failures = {}, []
    for name in names:
        try:
            outputs[name] = capture(exe_argv, name, verify_determinism)
        except RunFailure as e:
            failures.append(str(e))
    return outputs, failures


# ---------------------------------------------------------------------------
# The ledger.
#
# Deliberately the dead-symbol baseline's shape, adapted: a compact, append-
# mostly text file that is the whole point diffable in a code review. It does
# not hold every experiment's full text at every generation - that lives in
# tools/experiment-baselines/<name>.txt, one file per experiment, always the
# *current* accepted output, so `git diff` on that file is the drift report
# for anyone who does not want to run this tool. The ledger holds only what is
# needed to answer "how long has this been the same, and was anything else
# moving while it wasn't": a run-length-encoded history of digests per
# experiment, generation by generation.
#
# A "generation" is one accepted --update-baseline. It is not a build, a
# commit, or a clock tick - it is "someone looked at the current numbers and
# said these are now the record", exactly the human act that FINDINGS 15 never
# happened and this file exists to make cheap and visible when skipped.


class Run(object):
    __slots__ = ("first_gen", "last_gen", "digest")

    def __init__(self, first_gen, last_gen, digest):
        self.first_gen = first_gen
        self.last_gen = last_gen
        self.digest = digest


class Ledger(object):
    def __init__(self):
        self.generation = 0
        self.gen_label = {}          # generation -> free-text label (commit, etc.)
        self.experiments = {}        # name -> [Run, ...] ascending by first_gen
        self.errors = []             # malformed lines - fail the build like a bad allowlist

    def last_digest(self, name):
        runs = self.experiments.get(name)
        return runs[-1].digest if runs else None


def load_ledger(path):
    ledger = Ledger()
    if not os.path.exists(path):
        return ledger
    with open(path, "r") as fh:
        for lineno, raw in enumerate(fh, start=1):
            line = raw.split("#", 1)[0].rstrip("\n")
            if not line.strip():
                continue
            parts = line.split()
            if parts[0] == "generation":
                if len(parts) < 3:
                    ledger.errors.append("ledger:%d: malformed generation line: %r" % (lineno, line))
                    continue
                try:
                    gen = int(parts[1])
                except ValueError:
                    ledger.errors.append("ledger:%d: bad generation number: %r" % (lineno, line))
                    continue
                ledger.gen_label[gen] = " ".join(parts[2:])
                ledger.generation = max(ledger.generation, gen)
            else:
                if len(parts) != 4:
                    ledger.errors.append("ledger:%d: expected 'name first_gen last_gen digest', got %r"
                                          % (lineno, line))
                    continue
                name, fg, lg, digest = parts
                try:
                    fg, lg = int(fg), int(lg)
                except ValueError:
                    ledger.errors.append("ledger:%d: bad generation range: %r" % (lineno, line))
                    continue
                ledger.experiments.setdefault(name, []).append(Run(fg, lg, digest))
    for name, runs in ledger.experiments.items():
        runs.sort(key=lambda r: r.first_gen)
        for a, b in zip(runs, runs[1:]):
            if b.first_gen != a.last_gen + 1:
                ledger.errors.append(
                    "ledger: %s has a gap or overlap between generations %d and %d"
                    % (name, a.last_gen, b.first_gen))
    return ledger


def save_ledger(path, ledger):
    lines = [
        "# KILL ZONE - the experiment-drift checker's ledger.",
        "#",
        "# One row per (experiment, unbroken run of identical output). first_gen",
        "# and last_gen are --update-baseline generations, not builds or commits -",
        "# see the header of tools/check_experiment_drift.py for what a generation is.",
        "# A new row starts exactly when the digest changes; an unbroken run just",
        "# extends last_gen. The full text behind each digest is",
        "# tools/experiment-baselines/<name>.txt, always the *current* run - that",
        "# file is the diffable drift report; this ledger is the compact history",
        "# behind it that inertia is computed from.",
        "#",
        "# Regenerate with: python3 tools/check_experiment_drift.py --update-baseline",
        "",
    ]
    for gen in sorted(ledger.gen_label):
        lines.append("generation  %-4d %s" % (gen, ledger.gen_label[gen]))
    lines.append("")
    for name in sorted(ledger.experiments):
        for r in ledger.experiments[name]:
            # A literal space always separates fields, regardless of name
            # length - %-16s alone collided for "recently_settled" (16 chars)
            # against the digits that followed it, corrupting the round trip.
            lines.append("%-16s %-5d %-5d %s" % (name, r.first_gen, r.last_gen, r.digest))
    with open(path, "w") as fh:
        fh.write("\n".join(lines) + "\n")


def load_snapshot(directory, name):
    path = os.path.join(directory, name + ".txt")
    if not os.path.exists(path):
        return None
    with open(path, "r") as fh:
        return fh.read()


def save_snapshot(directory, name, text):
    if not os.path.isdir(directory):
        os.makedirs(directory)
    with open(os.path.join(directory, name + ".txt"), "w") as fh:
        fh.write(text)


# ---------------------------------------------------------------------------
# Findings.


class Finding(object):
    def __init__(self, category, name, detail, fix, new_text=None):
        self.category = category
        self.name = name
        self.detail = detail
        self.fix = fix
        self.new_text = new_text   # drift only: the freshly captured text


CATEGORIES = {
    "drift":       "output changed since the last recorded baseline - amend the finding it backs",
    "new":         "an experiment with no recorded baseline yet",
    "crash":       "an experiment did not run to completion",
    "inertia":     "output has not moved across enough of its siblings' changes to trust as live",
    "stale":       "a recorded experiment no longer exists in KZ.Balance",
}
CATEGORY_ORDER = ["crash", "new", "drift", "inertia", "stale"]


def diff_summary(old_text, new_text, max_lines=12):
    old_lines = old_text.split("\n")
    new_lines = new_text.split("\n")
    out = []
    import difflib
    for line in difflib.unified_diff(old_lines, new_lines, lineterm="",
                                      fromfile="recorded", tofile="current"):
        out.append(line)
        if len(out) >= max_lines:
            out.append("... (truncated; see tools/experiment-baselines/<name>.txt for the rest)")
            break
    return out


# ---------------------------------------------------------------------------
# Inertia.
#
# "This experiment's output has not changed" is easy to see and answers the
# wrong question. FINDINGS 32's three experiments are not interesting because
# their number stayed the same across builds - Aperture and Mines did that too
# for stretches, legitimately, because nothing that feeds them changed. They
# are interesting because six substantive, unrelated changes to the simulation
# ran underneath them and *none of it showed*.
#
# This checker cannot read what any experiment measures, so it cannot ask "did
# something relevant change" directly - that would need a hand-maintained map
# from experiment to the systems it exercises, which is exactly the kind of
# thing that goes stale silently and is the disease this project is trying to
# cure, not a cure for it. What it can ask, mechanically, or the same
# principle check_dead_symbols.py already relies on for test-only symbols: did
# *anything in the same universe of evidence* move. If eight of the ten
# balance experiments changed their numbers across the same span of updates
# and the ninth and tenth did not, the ninth and tenth were not obviously
# insulated from everything that happened - they went through it and came out
# unmarked, which is the ceiling FINDINGS 32 describes ("8 drones take any
# number of turrets") in mechanical form.
#
# So an experiment is INERTIA-SUSPECTED when its output has been unchanged for
# a run of generations across which at least INERTIA_MIN_OPPORTUNITIES *other*
# experiments changed. That is evidence, not proof - an experiment can be
# correctly, permanently flat (Aperture's coverage arithmetic might never need
# to move again) and this heuristic cannot tell that apart from a ceiling.
# Which is exactly why it only ever warns (see report()) and why the finding
# says "suspected" and shows its work - the generations, the siblings, the
# count - rather than asserting dead. An experiment with too little history to
# judge either way is reported as such, honestly, rather than folded into
# "fine": three generations of silence is not evidence of anything yet.


def moved_generations(ledger, exclude=None):
    """Generations at which some experiment's digest changed - i.e. every run
    after that experiment's first. A brand-new experiment's first row is not a
    change (nothing preceded it to differ from), so it never counts here."""
    moved = {}
    for name, runs in ledger.experiments.items():
        if name == exclude:
            continue
        for r in runs[1:]:
            moved.setdefault(r.first_gen, []).append(name)
    return moved


def inertia_findings(ledger, names, threshold=INERTIA_MIN_OPPORTUNITIES):
    findings = []
    insufficient = []
    for name in names:
        runs = ledger.experiments.get(name)
        if not runs:
            continue
        current = runs[-1]
        born_at = runs[0].first_gen
        age = ledger.generation - born_at + 1
        moved = moved_generations(ledger, exclude=name)
        opportunities = sorted(g for g in moved if current.first_gen < g <= current.last_gen)
        if len(runs) == 1 and age < threshold + 1:
            # Too young to judge - not "stable", just unproven either way. This
            # is about the experiment's own age, not the project's: an
            # experiment born last generation gets the same benefit of the
            # doubt on generation 500 of a long-lived ledger as it would on
            # generation 5 of a new one.
            insufficient.append((name, age))
            continue
        if len(opportunities) >= threshold:
            distinct_siblings = sorted(set(n for g in opportunities for n in moved[g]))
            per_gen = ", ".join("gen%d (%d)" % (g, len(moved[g])) for g in opportunities)
            detail = ("unchanged for %d generation(s) (gen %d-%d) while %d other experiment(s) "
                       "changed across %d of those generations - %s" %
                       (current.last_gen - current.first_gen + 1, current.first_gen,
                        current.last_gen, len(distinct_siblings), len(opportunities), per_gen))
            findings.append(Finding(
                "inertia", name, detail,
                "cannot be fixed here - it means the experiment could not have shown a change "
                "even if the simulation invalidated it. Owned by whoever maintains "
                "src/KZ.Balance: put it back on its knees (FINDINGS 32's own prescription) or "
                "confirm nothing it exercises has changed and say so"))
    return findings, insufficient


# ---------------------------------------------------------------------------
# Assembling a result. Pure function of (ledger, current outputs, discovered
# names) so the self-test can drive it without running any C# at all - the
# same split check_dead_symbols.py makes between parsing and its EXPECTED
# corpus.


class Result(object):
    def __init__(self, findings, insufficient_history, ok_names, ledger_errors, generation, total):
        self.findings = findings
        self.insufficient_history = insufficient_history
        self.ok_names = ok_names
        self.ledger_errors = ledger_errors
        self.generation = generation
        self.total = total

    @property
    def failing(self):
        return ([f for f in self.findings if f.category in ("crash", "new", "drift")]
                + list(self.ledger_errors))


def analyse(ledger, current_texts, names, crashes=()):
    """crashes: the failure messages run_once/capture raise, each shaped
    "<name>: ...". Parsed back into names here rather than asking the caller
    to pass both a message list and a name set that could disagree."""
    findings = []
    ok = []
    crash_names = set(msg.split(":", 1)[0].strip() for msg in crashes)
    for name in names:
        if name in crash_names:
            continue
        text = current_texts.get(name)
        if text is None:
            continue
        recorded = ledger.last_digest(name)
        digest = digest_of(text)
        if recorded is None:
            findings.append(Finding(
                "new", name, "no recorded baseline",
                "run --update-baseline to record its current output as the baseline"))
        elif digest != recorded:
            findings.append(Finding(
                "drift", name,
                "output no longer matches the recorded baseline (tools/experiment-baselines/%s.txt)"
                % name,
                "expected - the simulation is supposed to change. Read the diff, amend whatever "
                "FINDINGS.md conclusion depended on this experiment, then run --update-baseline "
                "to acknowledge the new numbers as the record",
                new_text=text))
        else:
            ok.append(name)

    for msg in crashes:
        findings.append(Finding("crash", msg.split(":", 1)[0].strip(), msg,
                                 "fix the crash, or if it is expected right now, do not run this "
                                 "checker against this tree yet"))

    for name in sorted(ledger.experiments):
        if name not in names:
            findings.append(Finding(
                "stale", name, "no longer produced by KZ.Balance",
                "delete its rows from the ledger and its file from tools/experiment-baselines/"))

    inertia, insufficient = inertia_findings(ledger, [n for n in names if n not in crash_names])
    findings.extend(inertia)

    findings.sort(key=lambda f: (CATEGORY_ORDER.index(f.category), f.name))
    return Result(findings, insufficient, ok, ledger.errors, ledger.generation, len(names))


# ---------------------------------------------------------------------------
# Reporting.


def report(res, show_all=False):
    print("experiment-drift check: generation %d, %d experiment(s) tracked"
          % (res.generation, res.total))

    if res.ledger_errors:
        print("")
        print("  FAIL  the ledger is malformed (%d issue(s))" % len(res.ledger_errors))
        for e in res.ledger_errors:
            print("    %s" % e)

    by_cat = {}
    for f in res.findings:
        by_cat.setdefault(f.category, []).append(f)

    for cat in ("crash", "new", "drift"):
        items = by_cat.get(cat, [])
        if not items:
            continue
        print("")
        print("  FAIL  %d %s - %s" % (len(items), cat, CATEGORIES[cat]))
        for f in items:
            print("    %s" % f.name)
            print("        %s" % f.detail)
            if cat == "drift":
                old = load_snapshot(DEFAULT_SNAPSHOTS, f.name) or ""
                new = f.new_text or ""
                for dline in diff_summary(old, new):
                    print("        %s" % dline)
            print("        fix: %s" % f.fix)

    items = by_cat.get("inertia", [])
    if items:
        print("")
        print("  WARN  %d experiment(s) suspected inert - %s" % (len(items), CATEGORIES["inertia"]))
        for f in items:
            print("    %s" % f.name)
            print("        %s" % f.detail)
            if show_all:
                print("        %s" % f.fix)
        if not show_all:
            print("        (--all shows what to do about each)")

    if res.insufficient_history:
        print("")
        print("  %d experiment(s) have too little history to judge for inertia yet: %s"
              % (len(res.insufficient_history), ", ".join(n for n, _g in res.insufficient_history)))

    items = by_cat.get("stale", [])
    if items:
        print("")
        print("  WARN  %d stale ledger entr(y/ies)" % len(items))
        for f in items:
            print("    %s - %s" % (f.name, f.fix))

    print("")
    fail_n = len(res.failing)
    warn_n = len(by_cat.get("inertia", [])) + len(by_cat.get("stale", []))
    print("  %d ok, %d failing, %d warning" % (len(res.ok_names), fail_n, warn_n))

    if res.failing:
        print("")
        print("  An experiment whose output moved without anyone acknowledging it is exactly")
        print("  how FINDINGS 15's table went stale: the simulation changed and the document")
        print("  did not. Run --update-baseline once you have looked at what moved.")
        return 1
    return 0


def list_mode(res):
    for f in res.findings:
        print("%s\t%s\t%s" % (f.category, f.name, f.detail))
    for name, gen in res.insufficient_history:
        print("insufficient-history\t%s\tonly %d generation(s) recorded" % (name, gen))
    for name in res.ok_names:
        print("ok\t%s\tmatches recorded baseline" % name)


# ---------------------------------------------------------------------------
# Self-test.
#
# No C# is compiled or run here - analyse() takes already-captured text and a
# ledger, both supplied directly, exactly the split check_dead_symbols.py makes
# between parsing real sources and its EXPECTED corpus. The scenario below is a
# miniature project history: four fictional experiments walked across enough
# synthetic generations to exercise every shape this file has to get right.
#
#   steady            - changes at every generation. Never flagged.
#   frozen            - never changes once, across a span where `steady`
#                       changed five times. This is the Stacking/Vertical/
#                       Decoy-Escort shape and must be flagged.
#   recently_settled  - changed at generation 5 and has been flat for exactly
#                       one generation since. Only one sibling change (gen 6)
#                       has happened since it last moved - below the
#                       opportunity threshold, so it must NOT be flagged. This
#                       is the honest middle case: not enough evidence yet,
#                       which is different from "flat and cleared".
#   newcomer          - born at generation 5, so it has existed for only two
#                       generations total. Must be reported as insufficient
#                       history, never as suspected or as clean - there has
#                       not been time to tell.
#
# What this fixture deliberately does NOT try to test: a "legitimately still"
# experiment sitting in the exact same generation span as `frozen`, with the
# exact same siblings moving around it. That case is genuinely
# indistinguishable from this file's own vantage point - it has no way to know
# `frozen`'s output was *supposed* to move and `quiet`'s was not, only that
# something else did. Claiming this self-test can tell them apart would be
# asserting a capability the checker does not have; see the comment above
# inertia_findings for what it does instead (warn with evidence, never fail).


def build_fixture_ledger():
    ledger = Ledger()
    gens = 6
    ledger.generation = gens
    for g in range(1, gens + 1):
        ledger.gen_label[g] = "fixture"

    def add(name, runs):
        ledger.experiments[name] = [Run(a, b, d) for a, b, d in runs]

    add("steady", [(1, 1, "s1"), (2, 2, "s2"), (3, 3, "s3"), (4, 4, "s4"), (5, 5, "s5"), (6, 6, "s6")])
    add("frozen", [(1, 6, "f0")])
    add("recently_settled", [(1, 4, "r0"), (5, 6, "r1")])
    add("newcomer", [(5, 6, "n0")])
    return ledger


def selftest():
    ok = True
    ledger = build_fixture_ledger()
    names = ["steady", "frozen", "recently_settled", "newcomer"]

    # inertia_findings is a pure function of the ledger alone (it never looks
    # at current output, only history), so it is exercised directly here with
    # no captured text needed.
    findings, insufficient = inertia_findings(ledger, names)
    got = set(f.name for f in findings)

    print("self-test: inertia over a synthetic six-generation history")
    checks = [
        ("frozen", "frozen" in got, "unchanged while a sibling changed five times - must flag"),
        ("steady", "steady" not in got, "changes every generation - must never flag"),
        ("recently_settled", "recently_settled" not in got,
         "settled one generation ago, only one sibling change since - not enough evidence yet"),
        ("newcomer", "newcomer" not in got, "too little history - must not flag as inert"),
    ]
    for name, cond, why in checks:
        print("  %s %-10s %s" % ("PASS" if cond else "FAIL", name, why))
        ok = ok and cond

    insuff_names = set(n for n, _g in insufficient)
    cond = insuff_names == {"newcomer"}
    print("  %s insufficient-history bucket is exactly {newcomer}, got %s"
          % ("PASS" if cond else "FAIL", sorted(insuff_names)))
    ok = ok and cond

    # --- drift, new, stale, crash, and the digest/text plumbing -------------
    ledger2 = Ledger()
    ledger2.generation = 1
    ledger2.gen_label[1] = "fixture"
    baseline_text = "SATURATION\n  1 2 3\n"
    ledger2.experiments["saturation"] = [Run(1, 1, digest_of(normalize(baseline_text)))]
    ledger2.experiments["retired"] = [Run(1, 1, digest_of(normalize("gone\n")))]

    current2 = {
        "saturation": normalize(baseline_text),          # unchanged
        "stacking": normalize("STACKING\n  frozen\n"),    # new: no ledger entry
    }
    res = analyse(ledger2, current2, ["saturation", "stacking"])
    by_name = {f.name: f for f in res.findings}

    cond = "saturation" in res.ok_names
    print("  %s unchanged output against a matching baseline is OK, not drift" % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    cond = "stacking" in by_name and by_name["stacking"].category == "new"
    print("  %s an experiment absent from the ledger is reported 'new' and fails"
          % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    cond = "retired" in by_name and by_name["retired"].category == "stale"
    print("  %s a ledger entry for a since-removed experiment is reported 'stale'"
          % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    current3 = {"saturation": normalize("SATURATION\n  9 9 9\n")}
    res3 = analyse(ledger2, current3, ["saturation"])
    cond = any(f.name == "saturation" and f.category == "drift" for f in res3.findings)
    print("  %s output that no longer matches its recorded digest is reported 'drift' and fails"
          % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    res4 = analyse(ledger2, {"saturation": normalize(baseline_text)}, ["saturation"], crashes=["stacking: exited 1\n      boom"])
    cond = any(f.category == "crash" and f.name == "stacking" for f in res4.findings)
    print("  %s a crashed experiment is reported 'crash', never silently skipped"
          % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    # --- normalize() ---------------------------------------------------------
    n1 = normalize("a\r\nb  \n\nc\n\n\n")
    cond = n1 == "a\nb\n\nc\n"
    print("  %s normalize() fixes line endings, trailing spaces and trailing blank lines"
          % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    # --- malformed ledger ------------------------------------------------------
    bad_path = os.path.join(FIXTURE, "ledger-malformed.txt")
    if os.path.exists(bad_path):
        bad = load_ledger(bad_path)
        cond = len(bad.errors) > 0
        print("  %s a malformed ledger line is rejected rather than silently ignored"
              % ("PASS" if cond else "FAIL"))
        ok = ok and cond
    else:
        print("  FAIL  missing fixture %s" % bad_path)
        ok = False

    # --- round-trip save/load --------------------------------------------------
    tmp_path = os.path.join(FIXTURE, ".roundtrip.tmp")
    try:
        save_ledger(tmp_path, ledger)
        back = load_ledger(tmp_path)
        cond = (not back.errors and back.generation == ledger.generation
                and set(back.experiments) == set(ledger.experiments)
                and all(len(back.experiments[n]) == len(ledger.experiments[n]) for n in ledger.experiments))
        print("  %s ledger round-trips through save/load unchanged" % ("PASS" if cond else "FAIL"))
        ok = ok and cond
    finally:
        if os.path.exists(tmp_path):
            os.remove(tmp_path)

    print("self-test: %s" % ("PASS" if ok else "FAIL"))
    return 0 if ok else 1


# ---------------------------------------------------------------------------
# Live run.


def default_exe_argv(root):
    """Mirrors build.sh's own choice: mcs's output needs `mono` in front of it,
    a dotnet- or csc-built exe does not. Overridable with --exe; build.sh
    always passes one explicitly so this only matters for a standalone run."""
    exe = os.path.join(root, "build", "KZ.Balance.exe")
    return (["mono", exe] if which("mono") else [exe])


def which(prog):
    for d in os.environ.get("PATH", "").split(os.pathsep):
        if os.path.isfile(os.path.join(d, prog)) and os.access(os.path.join(d, prog), os.X_OK):
            return True
    return False


def git_label(root):
    try:
        sha = subprocess.run(["git", "rev-parse", "--short", "HEAD"], cwd=root,
                              capture_output=True, timeout=10).stdout.decode().strip()
        dirty = subprocess.run(["git", "status", "--porcelain", "--", "src"], cwd=root,
                                capture_output=True, timeout=10).stdout.decode().strip()
        return sha + ("+dirty" if dirty else "") if sha else "unknown"
    except Exception:
        return "unknown"


def main(argv):
    root = ROOT
    if "--root" in argv:
        root = os.path.abspath(argv[argv.index("--root") + 1])
    ledger_path = DEFAULT_LEDGER
    if "--ledger" in argv:
        ledger_path = os.path.abspath(argv[argv.index("--ledger") + 1])
    snapshots_dir = DEFAULT_SNAPSHOTS
    if "--snapshots" in argv:
        snapshots_dir = os.path.abspath(argv[argv.index("--snapshots") + 1])

    if "--selftest" in argv:
        return selftest()

    exe_argv = default_exe_argv(root)
    if "--exe" in argv:
        exe_argv = shlex.split(argv[argv.index("--exe") + 1])

    names = discover_experiments(root)

    if "--update-baseline" in argv:
        ledger = load_ledger(ledger_path)
        if ledger.errors:
            for e in ledger.errors:
                print(e)
            print("refusing to update a malformed ledger")
            return 1
        outputs, failures = capture_all(exe_argv, names, verify_determinism=True)
        if failures:
            print("not recording a baseline - %d experiment(s) failed:" % len(failures))
            for f in failures:
                print("  %s" % f)
            return 1
        new_gen = ledger.generation + 1
        label = argv[argv.index("--label") + 1] if "--label" in argv else git_label(root)
        for name in names:
            digest = digest_of(outputs[name])
            runs = ledger.experiments.setdefault(name, [])
            if runs and runs[-1].digest == digest:
                runs[-1].last_gen = new_gen
            else:
                runs.append(Run(new_gen, new_gen, digest))
            save_snapshot(snapshots_dir, name, outputs[name])
        ledger.generation = new_gen
        ledger.gen_label[new_gen] = label
        save_ledger(ledger_path, ledger)
        print("recorded generation %d (%s): %d experiment(s)" % (new_gen, label, len(names)))
        return 0

    ledger = load_ledger(ledger_path)
    outputs, failures = capture_all(exe_argv, names, verify_determinism=False)
    res = analyse(ledger, outputs, names, crashes=failures)

    if "--list" in argv:
        list_mode(res)
        return 0

    status = report(res, show_all="--all" in argv)
    return 0 if "--warn-only" in argv else status


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
