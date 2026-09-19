#!/usr/bin/env python3
# KILL ZONE - a real-time strategy video game.
#
# The mutation guard: check_dead_symbols.py's and check_experiment_drift.py's
# third sibling, and the one that answers the question neither of the other two
# can ask.
#
# WHAT IT IS FOR
#
# FINDINGS 40 found that the ten balance experiments had no coverage at all of
# four systems wired in the three sessions before it - interception vectoring,
# emission control, ground radar and fiber. Not thin coverage. None: the
# constants those systems are built on could be changed to values that visibly
# break them and every one of the ten printed the same bytes. The way that was
# found was not by reading the experiments. It was by breaking the simulation on
# purpose and seeing who complained:
#
#     Take a constant, change it, rebuild, run the experiments, compare each
#     one's output byte for byte against the same run on the unmodified tree.
#     An experiment that does not move has no path to that constant - not an
#     opinion about coverage, a measurement of it.
#
# That sweep was ad hoc and was thrown away, which is why this file exists.
# FINDINGS 40's own closing line is that the ten read as a broad suite, are ten
# variations on one engagement, and "there is no way to tell which those are by
# looking at it". This is the thing that can tell you.
#
# WHAT IT CHECKS
#
# Every experiment DECLARES the mutations it claims to survive, in its own
# source, on a line beside it:
#
#     // MEASURES radar: radar-notch, radar-ground-clutter, emission-order
#
# The declaration is in src/KZ.Balance/Program.cs rather than in a manifest
# beside this file on purpose. A manifest is a second place to remember, and the
# person rewriting an experiment does not open it; a line three lines above the
# method is in the diff of every rewrite. Each mutation is described once, in
# tools/experiment-mutations.txt, because the cost this file is designed around
# is the cost of adding the hundredth mutation, not the first.
#
# Then, per mutation: copy the tree, apply it, rebuild, run every experiment
# that claims it, and compare. A claimed mutation that an experiment does not
# notice is a FAILURE - the experiment is not measuring what it says it is.
#
# A mutation nothing claims is a WARNING and is the most useful line this tool
# prints: it is a named system with no experiment behind it, which is FINDINGS
# 40's finding in standing form rather than as a paragraph that goes stale.
#
# HOW OFTEN TO RUN IT
#
# Not in ./build.sh's default path, and that is a decision rather than an
# oversight. It is a rebuild and a full experiment run per mutation - minutes,
# not seconds - and docs/EXPERIMENT-DRIFT.md has already recorded this project's
# view of a slow check in the default path: it gets skipped, and a check that is
# skipped reads as evidence while being none. So:
#
#     ./build.sh mutations
#
# Run it: when you write or rewrite a balance experiment (it is that
# experiment's acceptance test - "it moves when the thing it measures is broken"
# is the claim being made, and this is the only thing that checks it); when you
# add a mutation for a system you have just wired; and before FINDINGS.md cites
# an experiment as evidence that a system works, because that citation is
# exactly the claim this tool exists to falsify. Otherwise about once a session.
# It is cheap to run while reading something else and expensive to skip for a
# year.
#
# WHAT IT CANNOT SEE
#
#   - It proves an experiment has a PATH to a constant. It does not prove the
#     experiment measures the constant well, or reports it honestly, or that the
#     number it prints means what the prose beside it says. A one-cell twitch
#     counts as movement.
#   - It reads stdout, like the drift guard, so two different internal states
#     that format identically read as unchanged.
#   - Its catalogue is written by hand, so it can only ever ask about systems
#     somebody thought to name. An unmutated constant is invisible to it. That
#     is the honest limit and it is why the uncovered-mutation warning is
#     phrased as a gap rather than a clean bill of health.
#
# Usage:
#   python3 tools/check_experiment_mutations.py                 the sweep
#   python3 tools/check_experiment_mutations.py --selftest        its own corpus
#   python3 tools/check_experiment_mutations.py --list            declarations
#                                                                 only, no build
#   ... --only <mutation>[,<mutation>]   sweep just these
#   ... --experiment <name>[,<name>]     sweep just these experiments' claims
#   ... --keep                           leave the scratch trees for inspection
#   ... --warn-only                      report but never fail
#   ... --root DIR / --catalogue FILE    point at a different tree

import os
import re
import shutil
import subprocess
import sys
import tempfile

try:
    import signal
    signal.signal(signal.SIGPIPE, signal.SIG_DFL)
except (ImportError, AttributeError, ValueError):
    pass

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
FIXTURE = os.path.join(HERE, "testdata", "experiment-mutations")

DEFAULT_CATALOGUE = os.path.join(HERE, "experiment-mutations.txt")

BALANCE_SOURCE = os.path.join("src", "KZ.Balance", "Program.cs")

# A build and a full experiment run, on a tree that has just been copied, on
# whatever machine somebody is doing this on. STACKING alone is over two minutes
# on the reference machine, so this is deliberately far looser than the drift
# guard's 180: a timeout here is meant to catch an infinite loop introduced by a
# mutation, not to police how long an honest experiment takes.
BUILD_TIMEOUT_SECONDS = 600
RUN_TIMEOUT_SECONDS = 900

# The same invariant-culture forcing check_experiment_drift.py does, for the
# same reason: Fix.ToString and the harness's own format strings run under .NET's
# current culture, which is an environment property and not a simulation one. A
# decimal comma must never read as a mutation having been noticed.
RUN_ENV_OVERRIDES = {
    "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT": "1",
    "LC_ALL": "C",
    "LANG": "C",
}

# What gets copied into a scratch tree. build.sh cds to its own directory and
# needs nothing else; the guards and their baselines are not involved in a
# `build.sh build`, and leaving them out keeps a copy fast enough to do once per
# mutation without thinking about it.
SCRATCH_CONTENTS = ["src", "build.sh"]


# ---------------------------------------------------------------------------
# Discovering which experiments exist.
#
# Main()'s dispatch table, read rather than listed, exactly as
# check_experiment_drift.py does it. Duplicated rather than imported: these two
# files are siblings and not a library, and a shared import would mean deleting
# or moving one of them silently changes what the other one checks. Ten lines is
# a cheaper price than that coupling.

DISPATCH_RE = re.compile(r'which\s*==\s*"(\w+)"')


def discover_experiments(root):
    with open(os.path.join(root, BALANCE_SOURCE), "r") as fh:
        text = fh.read()
    names = []
    for m in DISPATCH_RE.finditer(text):
        name = m.group(1)
        if name != "all" and name not in names:
            names.append(name)
    return names


# ---------------------------------------------------------------------------
# The claims, read out of the experiment source.

MEASURES_RE = re.compile(r'^\s*//\s*MEASURES\s+(\w+)\s*:\s*(.+?)\s*$')


class ClaimSet(object):
    def __init__(self):
        self.by_experiment = {}   # experiment -> [mutation, ...]
        self.errors = []


def load_claims(root):
    """Every `// MEASURES <experiment>: <mutation>, ...` line in the balance
    source. Keyed by the experiment name it names rather than by whatever method
    it happens to sit above: a claim that drifts away from its experiment during
    an edit should stop matching loudly, not re-attach itself to the neighbour."""
    claims = ClaimSet()
    path = os.path.join(root, BALANCE_SOURCE)
    with open(path, "r") as fh:
        lines = fh.read().splitlines()

    for n, line in enumerate(lines, 1):
        m = MEASURES_RE.match(line)
        if not m:
            continue
        name = m.group(1)
        listed = [p.strip() for p in m.group(2).split(",")]
        listed = [p for p in listed if p]
        if not listed:
            claims.errors.append("%s:%d: MEASURES %s declares nothing"
                                 % (BALANCE_SOURCE, n, name))
            continue
        existing = claims.by_experiment.setdefault(name, [])
        for mut in listed:
            if mut in existing:
                claims.errors.append("%s:%d: %s claims %s twice"
                                     % (BALANCE_SOURCE, n, name, mut))
            else:
                existing.append(mut)
    return claims


# ---------------------------------------------------------------------------
# The mutation catalogue.
#
# One mutation is four facts and a reason:
#
#   mutation <name>
#     file    <path from the repository root>
#     from    <text that must appear in that file exactly once>
#     to      <what to replace it with>
#     breaks  <one line saying what this makes the simulation stop doing>
#
# `from` matching exactly once is the whole anti-rot mechanism and it is a hard
# failure in both directions. Zero matches means the constant has been renamed,
# retyped or deleted and the mutation has been quietly testing nothing; two
# means it is ambiguous and the tool cannot say which one it broke. Either way
# it stops the sweep rather than skipping the entry, because a mutation that has
# stopped applying is worse than no mutation - it reads as coverage.
#
# `breaks` is required. An entry without one is a mutation nobody can judge the
# result of, and the rule is dead-symbols-allow.txt's: the checker rejects it.

class Mutation(object):
    def __init__(self, name, line):
        self.name = name
        self.line = line
        self.path = None
        self.old = None
        self.new = None
        self.breaks = None

    def missing(self):
        return [f for f in ("file", "from", "to", "breaks")
                if getattr(self, {"file": "path", "from": "old",
                                  "to": "new", "breaks": "breaks"}[f]) is None]


class Catalogue(object):
    def __init__(self):
        self.mutations = []       # in file order, which is report order
        self.errors = []

    def by_name(self, name):
        for m in self.mutations:
            if m.name == name:
                return m
        return None

    def names(self):
        return [m.name for m in self.mutations]


FIELD_RE = re.compile(r'^\s{2,}(file|from|to|breaks)\s+(.*?)\s*$')
MUTATION_RE = re.compile(r'^mutation\s+([A-Za-z0-9_.-]+)\s*$')


def load_catalogue(path):
    cat = Catalogue()
    if not os.path.exists(path):
        cat.errors.append("no mutation catalogue at %s" % path)
        return cat

    with open(path, "r") as fh:
        lines = fh.read().splitlines()

    current = None
    for n, raw in enumerate(lines, 1):
        if not raw.strip() or raw.lstrip().startswith("#"):
            continue
        m = MUTATION_RE.match(raw)
        if m:
            if current is not None:
                finish(cat, current)
            if cat.by_name(m.group(1)):
                cat.errors.append("%s:%d: mutation %s declared twice"
                                  % (os.path.basename(path), n, m.group(1)))
            current = Mutation(m.group(1), n)
            continue
        f = FIELD_RE.match(raw)
        if not f:
            cat.errors.append("%s:%d: not a `mutation <name>` header or an "
                              "indented file/from/to/breaks field: %r"
                              % (os.path.basename(path), n, raw))
            continue
        if current is None:
            cat.errors.append("%s:%d: %s field before any mutation header"
                              % (os.path.basename(path), n, f.group(1)))
            continue
        key, value = f.group(1), f.group(2)
        if key == "file":
            current.path = value
        elif key == "from":
            current.old = value
        elif key == "to":
            current.new = value
        else:
            current.breaks = value

    if current is not None:
        finish(cat, current)

    for m in cat.mutations:
        gaps = m.missing()
        if gaps:
            cat.errors.append("%s: mutation %s is missing %s"
                              % (os.path.basename(path), m.name, ", ".join(gaps)))
        elif m.old == m.new:
            cat.errors.append("%s: mutation %s replaces text with itself, so it "
                              "breaks nothing" % (os.path.basename(path), m.name))
    return cat


def finish(cat, mutation):
    cat.mutations.append(mutation)


# ---------------------------------------------------------------------------
# Applying one.


class ApplyFailure(Exception):
    """The mutation no longer describes the source. Always fatal - see the
    catalogue's own header for why this is not a skip."""


def apply_mutation(tree, mutation):
    """Rewrite one file in a scratch tree. Returns nothing; raises if the
    `from` text is not present exactly once."""
    path = os.path.join(tree, mutation.path)
    if not os.path.exists(path):
        raise ApplyFailure("%s: no such file as %s - the mutation has rotted"
                           % (mutation.name, mutation.path))
    with open(path, "r") as fh:
        text = fh.read()
    hits = text.count(mutation.old)
    if hits == 0:
        raise ApplyFailure(
            "%s: `%s` no longer appears in %s. The mutation has rotted: it has "
            "been testing nothing since whatever renamed or rewrote that line, "
            "and every experiment claiming it has been passing for free."
            % (mutation.name, mutation.old, mutation.path))
    if hits > 1:
        raise ApplyFailure(
            "%s: `%s` appears %d times in %s, so there is no saying which one "
            "this mutation breaks. Make the `from` text long enough to be "
            "unique." % (mutation.name, mutation.old, hits, mutation.path))
    with open(path, "w") as fh:
        fh.write(text.replace(mutation.old, mutation.new))


# ---------------------------------------------------------------------------
# Building and running a scratch tree.


class TreeFailure(Exception):
    """A scratch tree would not build, or an experiment would not run in it."""


def make_scratch(root, keep):
    tree = tempfile.mkdtemp(prefix="kz-mutation-")
    for item in SCRATCH_CONTENTS:
        src = os.path.join(root, item)
        dst = os.path.join(tree, item)
        if os.path.isdir(src):
            shutil.copytree(src, dst)
        else:
            shutil.copy2(src, dst)
            os.chmod(dst, 0o755)
    return tree


def build_tree(tree):
    env = dict(os.environ)
    env.update(RUN_ENV_OVERRIDES)
    try:
        proc = subprocess.run([os.path.join(tree, "build.sh"), "build"],
                              cwd=tree, capture_output=True,
                              timeout=BUILD_TIMEOUT_SECONDS, env=env)
    except subprocess.TimeoutExpired:
        raise TreeFailure("build timed out after %ds" % BUILD_TIMEOUT_SECONDS)
    if proc.returncode != 0:
        tail = proc.stderr.decode("utf-8", "replace").strip().splitlines()[-8:]
        if not tail:
            tail = proc.stdout.decode("utf-8", "replace").strip().splitlines()[-8:]
        raise TreeFailure("build failed:\n      " + "\n      ".join(tail))


def exe_argv(tree, runner):
    """build.sh's own choice, mirrored: an mcs-built exe needs `mono` in front
    of it and a csc- or dotnet-built one does not. Overridable with --runner for
    a machine where that rule is wrong, which is also how a caller pins it."""
    exe = os.path.join(tree, "build", "KZ.Balance.exe")
    if runner is not None:
        return ([runner] if runner else []) + [exe]
    return (["mono", exe] if which("mono") else [exe])


def which(prog):
    for d in os.environ.get("PATH", "").split(os.pathsep):
        p = os.path.join(d, prog)
        if os.path.isfile(p) and os.access(p, os.X_OK):
            return True
    return False


def normalize(text):
    """check_experiment_drift.py's normalisation, and only that: line-ending
    style and trailing whitespace can vary without the simulation having moved.
    Column alignment inside a table is content."""
    lines = text.replace("\r\n", "\n").replace("\r", "\n").split("\n")
    lines = [ln.rstrip() for ln in lines]
    while lines and lines[-1] == "":
        lines.pop()
    return "\n".join(lines) + "\n"


def run_experiment(argv, name):
    env = dict(os.environ)
    env.update(RUN_ENV_OVERRIDES)
    try:
        proc = subprocess.run(argv + [name], capture_output=True,
                              timeout=RUN_TIMEOUT_SECONDS, env=env)
    except subprocess.TimeoutExpired:
        raise TreeFailure("%s: timed out after %ds" % (name, RUN_TIMEOUT_SECONDS))
    if proc.returncode != 0:
        tail = proc.stderr.decode("utf-8", "replace").strip().splitlines()[-8:]
        raise TreeFailure("%s: exited %d\n      %s"
                          % (name, proc.returncode, "\n      ".join(tail)))
    return normalize(proc.stdout.decode("utf-8", "replace"))


# ---------------------------------------------------------------------------
# Verdicts.

CATEGORIES = {
    "blind": ("FAIL", "experiment(s) did not notice a mutation they claim"),
    "rotted": ("FAIL", "mutation(s) no longer describe the source"),
    "broken": ("FAIL", "mutation(s) produced a tree that will not build or run"),
    "declaration": ("FAIL", "declaration error(s)"),
    "unclaimed": ("FAIL", "experiment(s) declare nothing at all"),
    "uncovered": ("WARN", "mutation(s) no experiment claims"),
}
CATEGORY_ORDER = ["declaration", "rotted", "broken", "blind", "unclaimed", "uncovered"]


class Finding(object):
    def __init__(self, category, name, detail):
        self.category = category
        self.name = name
        self.detail = detail


def first_difference(a, b):
    """Where two runs stopped agreeing, for the report. A mutation that moves
    one cell of one table is a pass, and being shown which cell is how a reader
    decides whether the experiment noticed the thing or merely twitched."""
    al, bl = a.splitlines(), b.splitlines()
    for i in range(max(len(al), len(bl))):
        x = al[i] if i < len(al) else "<end of output>"
        y = bl[i] if i < len(bl) else "<end of output>"
        if x != y:
            return i + 1, x.strip(), y.strip()
    return None


def declaration_findings(experiments, claims, catalogue):
    """Everything that can be judged without building anything: a claim on an
    experiment that does not exist, a claim on a mutation that does not exist,
    an experiment that claims nothing, a mutation nothing claims."""
    findings = []
    known = set(catalogue.names())

    for e in sorted(claims.by_experiment):
        if e not in experiments:
            findings.append(Finding(
                "declaration", e,
                "MEASURES names %s, which Main() does not dispatch. Either the "
                "experiment was renamed and the claim was not, or the claim is "
                "for one that was deleted." % e))
    for e in sorted(claims.by_experiment):
        for mut in claims.by_experiment[e]:
            if mut not in known:
                findings.append(Finding(
                    "declaration", "%s/%s" % (e, mut),
                    "%s claims mutation `%s`, which is not in the catalogue."
                    % (e, mut)))

    for e in experiments:
        if not claims.by_experiment.get(e):
            findings.append(Finding(
                "unclaimed", e,
                "declares no MEASURES line, so nothing here can say whether it "
                "measures anything. Add one naming a mutation it survives, or a "
                "catalogue entry for the constant it is about."))

    claimed = set()
    for lst in claims.by_experiment.values():
        claimed.update(lst)
    for m in catalogue.mutations:
        if m.name not in claimed:
            findings.append(Finding(
                "uncovered", m.name,
                "no experiment claims it: %s can be broken and the harness "
                "will not print a different byte. %s" % (m.name, m.breaks)))
    return findings


# ---------------------------------------------------------------------------
# The sweep.


def sweep(root, catalogue, claims, experiments, only, only_experiments, keep,
          log=print):
    """One clean build, then one build per mutation, running only the
    experiments that claim it. Returns findings plus a per-pair verdict table."""
    findings = []
    pairs = []          # (mutation, experiment) to check
    for m in catalogue.mutations:
        if only and m.name not in only:
            continue
        for e in experiments:
            if only_experiments and e not in only_experiments:
                continue
            if m.name in claims.by_experiment.get(e, []):
                pairs.append((m.name, e))

    if not pairs:
        return findings, []

    needed = sorted(set(e for _, e in pairs))
    verdicts = []

    clean = make_scratch(root, keep)
    try:
        log("  building the unmutated tree")
        build_tree(clean)
        argv = exe_argv(clean, RUNNER)
        baseline = {}
        for e in needed:
            log("    running %s" % e)
            baseline[e] = run_experiment(argv, e)
    except (TreeFailure, ApplyFailure) as err:
        findings.append(Finding("broken", "(unmutated tree)", str(err)))
        if not keep:
            shutil.rmtree(clean, ignore_errors=True)
        return findings, verdicts
    finally:
        if keep:
            log("  scratch tree kept at %s" % clean)

    try:
        for m in catalogue.mutations:
            mine = [e for name, e in pairs if name == m.name]
            if not mine:
                continue
            log("  %s: %s" % (m.name, m.breaks))
            tree = make_scratch(root, keep)
            try:
                try:
                    apply_mutation(tree, m)
                    build_tree(tree)
                except ApplyFailure as err:
                    findings.append(Finding("rotted", m.name, str(err)))
                    continue
                except TreeFailure as err:
                    findings.append(Finding(
                        "broken", m.name,
                        "the mutated tree does not build, so nothing can be "
                        "learned from it. %s" % err))
                    continue

                margv = exe_argv(tree, RUNNER)
                for e in mine:
                    try:
                        got = run_experiment(margv, e)
                    except TreeFailure as err:
                        findings.append(Finding(
                            "broken", "%s/%s" % (m.name, e),
                            "%s does not run under %s: %s" % (e, m.name, err)))
                        continue
                    diff = first_difference(baseline[e], got)
                    if diff is None:
                        findings.append(Finding(
                            "blind", "%s/%s" % (e, m.name),
                            "%s claims to measure %s and printed identical "
                            "bytes with it broken (%s). The experiment has no "
                            "path to that constant."
                            % (e, m.name, m.breaks)))
                        verdicts.append((m.name, e, None))
                        log("    %-12s did NOT notice" % e)
                    else:
                        verdicts.append((m.name, e, diff))
                        log("    %-12s moved at line %d" % (e, diff[0]))
            finally:
                if keep:
                    log("    scratch tree kept at %s" % tree)
                else:
                    shutil.rmtree(tree, ignore_errors=True)
    finally:
        if not keep:
            shutil.rmtree(clean, ignore_errors=True)

    return findings, verdicts


RUNNER = None


# ---------------------------------------------------------------------------
# Reporting.


def report(findings, verdicts, experiments, claims, catalogue, swept):
    print("experiment-mutation check: %d experiment(s), %d mutation(s), "
          "%d claim(s)"
          % (len(experiments), len(catalogue.mutations),
             sum(len(v) for v in claims.by_experiment.values())))
    print("")

    if verdicts:
        print("  %d claim(s) checked by breaking the simulation:" % len(verdicts))
        for mut, exp, diff in verdicts:
            if diff is None:
                print("    %-24s %-12s  UNMOVED" % (mut, exp))
            else:
                print("    %-24s %-12s  moved: line %d, %r -> %r"
                      % (mut, exp, diff[0], diff[1][:40], diff[2][:40]))
        print("")

    status = 0
    for category in CATEGORY_ORDER:
        rows = [f for f in findings if f.category == category]
        if not rows:
            continue
        severity, label = CATEGORIES[category]
        print("  %s  %d %s" % (severity, len(rows), label))
        for f in rows:
            print("    %-28s %s" % (f.name, f.detail))
        print("")
        if severity == "FAIL":
            status = 1

    checked = len([v for v in verdicts if v[2] is not None])
    print("  %d claim(s) hold, %d failing, %d warning"
          % (checked,
             len([f for f in findings if CATEGORIES[f.category][0] == "FAIL"]),
             len([f for f in findings if CATEGORIES[f.category][0] == "WARN"])))
    if not swept:
        print("  (no sweep run - declarations only)")
    return status


def list_mode(experiments, claims, catalogue):
    for e in experiments:
        listed = claims.by_experiment.get(e, [])
        print("%-12s %s" % (e, ", ".join(listed) if listed else "-"))
    print("")
    claimed = set()
    for lst in claims.by_experiment.values():
        claimed.update(lst)
    for m in catalogue.mutations:
        who = sorted(e for e in claims.by_experiment
                     if m.name in claims.by_experiment[e])
        print("%-24s %-6s %s" % (m.name, "" if who else "UNCLAIMED",
                                 ", ".join(who) if who else m.breaks))


# ---------------------------------------------------------------------------
# The self-test.
#
# The project's rule, from build.sh: a checker that has quietly stopped catching
# things is worse than no checker, because it reads as evidence. So every branch
# of the judgement here is exercised against a fixture that does not need a
# compiler - the parsing, the exactly-once rule in both its directions, and the
# verdict a sweep reaches - and build.sh runs this before it runs the sweep.


def quiet_report(findings):
    """report()'s exit status without its output."""
    import io
    import contextlib
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        return report(findings, [], ["x"], ClaimSet(), Catalogue(), True)


def selftest():
    ok = True

    def check(label, got, want):
        if got != want:
            print("SELFTEST FAIL: %s\n  got:  %r\n  want: %r" % (label, got, want))
            return False
        return True

    # --- the catalogue parser ----------------------------------------------
    good = os.path.join(FIXTURE, "catalogue-good.txt")
    cat = load_catalogue(good)
    ok &= check("good catalogue parses clean", cat.errors, [])
    ok &= check("good catalogue names", cat.names(), ["alpha", "beta"])
    ok &= check("beta's breaks line", cat.by_name("beta").breaks,
                "the second thing, so that a mutation with no claimant exists")

    bad = os.path.join(FIXTURE, "catalogue-bad.txt")
    cat_bad = load_catalogue(bad)
    joined = " | ".join(cat_bad.errors)
    ok &= check("a mutation with no `breaks` is rejected",
                "missing breaks" in joined, True)
    ok &= check("a mutation with no `from` is rejected",
                "missing from" in joined, True)
    ok &= check("a mutation that replaces text with itself is rejected",
                "breaks nothing" in joined, True)
    ok &= check("a stray field before any header is rejected",
                "before any mutation header" in joined, True)

    # --- the exactly-once rule, in both directions -------------------------
    tree = tempfile.mkdtemp(prefix="kz-mutation-selftest-")
    try:
        os.makedirs(os.path.join(tree, "src"))
        target = os.path.join(tree, "src", "Thing.cs")
        with open(target, "w") as fh:
            fh.write("A = 1;\nB = 2;\nB = 2;\n")

        m = Mutation("hit", 0)
        m.path = "src/Thing.cs"
        m.old, m.new, m.breaks = "A = 1;", "A = 9;", "a"
        apply_mutation(tree, m)
        with open(target) as fh:
            ok &= check("a unique `from` is replaced", fh.read(),
                        "A = 9;\nB = 2;\nB = 2;\n")

        m = Mutation("gone", 0)
        m.path = "src/Thing.cs"
        m.old, m.new, m.breaks = "Z = 0;", "Z = 1;", "a"
        try:
            apply_mutation(tree, m)
            ok &= check("a `from` that is absent raises", False, True)
        except ApplyFailure as e:
            ok &= check("an absent `from` says it has rotted",
                        "rotted" in str(e), True)

        m = Mutation("twice", 0)
        m.path = "src/Thing.cs"
        m.old, m.new, m.breaks = "B = 2;", "B = 3;", "a"
        try:
            apply_mutation(tree, m)
            ok &= check("an ambiguous `from` raises", False, True)
        except ApplyFailure as e:
            ok &= check("an ambiguous `from` says how many it matched",
                        "appears 2 times" in str(e), True)

        m = Mutation("nofile", 0)
        m.path = "src/Missing.cs"
        m.old, m.new, m.breaks = "x", "y", "a"
        try:
            apply_mutation(tree, m)
            ok &= check("a missing file raises", False, True)
        except ApplyFailure as e:
            ok &= check("a missing file says it has rotted",
                        "rotted" in str(e), True)
    finally:
        shutil.rmtree(tree, ignore_errors=True)

    # --- claims, read out of a fixture balance source ----------------------
    claims = load_claims(FIXTURE)
    ok &= check("claims parse clean", claims.errors, [])
    ok &= check("claims are keyed by experiment name",
                claims.by_experiment, {"one": ["alpha"], "two": ["alpha"]})
    ok &= check("experiments come from the dispatch table",
                discover_experiments(FIXTURE), ["one", "two", "three"])

    # --- the declaration verdicts ------------------------------------------
    found = declaration_findings(discover_experiments(FIXTURE), claims, cat)
    got = sorted((f.category, f.name) for f in found)
    ok &= check("three declares nothing; beta is claimed by nobody",
                got, [("unclaimed", "three"), ("uncovered", "beta")])

    stale = ClaimSet()
    stale.by_experiment = {"one": ["alpha"], "ghost": ["alpha"],
                           "two": ["nosuchmutation"]}
    found = declaration_findings(["one", "two"], stale, cat)
    got = sorted((f.category, f.name) for f in found)
    ok &= check("a claim on a dead experiment and on an unknown mutation both "
                "fail",
                got, [("declaration", "ghost"),
                      ("declaration", "two/nosuchmutation"),
                      ("uncovered", "beta")])

    # --- the verdict a sweep reaches ---------------------------------------
    same = normalize("  drones   killed\n  ---------------\n       2       0%\n")
    moved = normalize("  drones   killed\n  ---------------\n       2      41%\n")
    ok &= check("identical output is no difference",
                first_difference(same, same), None)
    ok &= check("a moved cell is located",
                first_difference(same, moved)[0], 3)
    ok &= check("a truncated run counts as a difference",
                first_difference(same, "  drones   killed\n") is not None, True)

    # --- and that a FAIL category actually fails ---------------------------
    # Reported into a bin rather than onto the terminal: what is being checked
    # is the exit status the build reads, not the text a person reads.
    ok &= check("a blind experiment fails the run",
                quiet_report([Finding("blind", "x/y", "z")]), 1)
    ok &= check("an uncovered mutation only warns",
                quiet_report([Finding("uncovered", "m", "z")]), 0)

    print("selftest: %s" % ("ok" if ok else "FAILED"))
    return 0 if ok else 1


# ---------------------------------------------------------------------------


def main(argv):
    global RUNNER

    root = ROOT
    if "--root" in argv:
        root = os.path.abspath(argv[argv.index("--root") + 1])
    catalogue_path = DEFAULT_CATALOGUE
    if "--catalogue" in argv:
        catalogue_path = os.path.abspath(argv[argv.index("--catalogue") + 1])
    if "--runner" in argv:
        RUNNER = argv[argv.index("--runner") + 1]

    if "--selftest" in argv:
        return selftest()

    only = None
    if "--only" in argv:
        only = set(p.strip() for p in argv[argv.index("--only") + 1].split(","))
    only_experiments = None
    if "--experiment" in argv:
        only_experiments = set(p.strip() for p in
                               argv[argv.index("--experiment") + 1].split(","))

    experiments = discover_experiments(root)
    claims = load_claims(root)
    catalogue = load_catalogue(catalogue_path)

    findings = [Finding("declaration", "catalogue", e) for e in catalogue.errors]
    findings += [Finding("declaration", "claims", e) for e in claims.errors]
    findings += declaration_findings(experiments, claims, catalogue)

    if "--list" in argv:
        list_mode(experiments, claims, catalogue)
        return 0

    swept = False
    # A malformed catalogue is not a tree to sweep. Reporting the parse errors
    # and stopping is better than sweeping half of it and reporting a pass.
    if not [f for f in findings if f.category == "declaration"]:
        swept = True
        more, verdicts = sweep(root, catalogue, claims, experiments,
                               only, only_experiments, "--keep" in argv)
        findings += more
    else:
        verdicts = []

    status = report(findings, verdicts, experiments, claims, catalogue, swept)
    return 0 if "--warn-only" in argv else status


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
