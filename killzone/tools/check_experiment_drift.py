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
#   CEILING.  FINDINGS 32: three of the ten experiments produced byte-identical
#   output across six builds spanning a corrected weapon range, a magazine, a
#   traverse rate, a track hold, twenty corrected signature numbers, a realistic
#   world, and an engagement-commitment mechanic - and the reason was legible in
#   their own tables the whole time. "Every row of all three tables sits on a
#   ceiling ... and a measurement pinned to a ceiling is a constant, not a
#   result." An experiment whose answer is 100% before the run begins is a unit
#   test asserting true, and it is dead in exactly the sense a symbol nothing
#   calls is dead.
#
# This file used to infer that second failure from sibling behaviour: an
# experiment that sat still while at least three *other* experiments moved was
# reported "suspected inert". That signal is gone, and FINDINGS 40 records why.
# It conflated "cannot move" with "correctly unaffected" - sensors compares
# optics, acoustic and thermal and carries no radar at all, so a Doppler notch
# landing in the simulation *should* leave it byte-identical - and by the time
# seven of the ten were warning, six of the seven on two generations' worth of
# three siblings twitching, it was a warning nobody could act on. A checker that
# cries wolf is disabled within a week, which is this project's own stated
# standard and the reason the signal was replaced rather than retuned.
#
# What replaced it measures the property directly, in the experiment's own
# output, with no reference to any other experiment and no need for generational
# history: how much of this table is pinned to an extreme, and does any column
# of it vary at all. See "Ceilings" below.
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
#   ... --ceiling-allow FILE                 a different ceiling allowlist

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
DEFAULT_CEILING_ALLOW = os.path.join(HERE, "experiment-ceiling-allow.txt")

# --- the ceiling detector's three numbers, and why each is where it is -------
#
# A percentage cell in one of these reports is a share of trials: bounded at 0
# and at 100, so those two values are the only ones that can be a *ceiling*
# rather than a measurement. That is what makes percentages the class this
# detector reads and everything else the class it leaves alone; see the
# "Ceilings" section for what it therefore cannot see.
#
# A sweep is entitled to ONE rung at each end of its range. That is what
# bracketing a transition means: you want a rung the defence always survives
# and a rung it never does, so you know the interesting band is between them.
# A *second* rung on the same bound is a rung that told you nothing. So the
# first and last cell of a column are dropped when they sit on a bound, and the
# share below is measured on what is left - which is the difference between
# APPROACH (81, 86, 90, 86, 85, 94, 100: one rung off the end, six doing work)
# and SATURATION (0, 0, 37, 98, 100, 100, 100, 100: four rungs saying the same
# thing).
CEILING_TRIM_ENDS = True

# PINNED_SHARE at one half, stated as a rule rather than tuned to a corpus:
# after the ends are trimmed, more of this column is a constant than is a
# result.
CEILING_PINNED_SHARE = 0.5

# A column shorter than this cannot distinguish "pinned" from "small": two
# cells at 100% are one coincidence, and accusing an experiment on two cells is
# how a checker earns a reputation for noise. Measured on the column as
# printed, before trimming, so shortening a sweep is not a way to get out from
# under this check.
CEILING_MIN_COLUMN = 4

# ...and at least this much column has to survive the trim for the share to
# mean anything.
CEILING_MIN_AFTER_TRIM = 2

# Three identical cells in one column is the flat-sweep signal: a variable was
# swept and the answer did not move. Three rather than four because a flat
# column is a stronger statement than a pinned one - it needs no assumption
# about where the bounds are - and because a three-rung sweep is a real thing
# this harness prints.
FLAT_MIN_COLUMN = 3

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
        "# behind it, and the only question asked of that history is whether an",
        "# experiment has ever moved at all (see 'frozen' in the checker).",
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
    "ceiling":     "part of this experiment's output is pinned to an extreme or does not vary",
    "frozen":      "output byte-identical across the whole recorded history",
    "stale":       "a recorded experiment no longer exists in KZ.Balance",
}
CATEGORY_ORDER = ["crash", "new", "drift", "ceiling", "frozen", "stale"]


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
# Ceilings, and the one thing history is still asked.
#
# FINDINGS 32 diagnosed three dead experiments by reading their tables: "every
# row of all three sits on a ceiling - 8 drones take any number of turrets, 6
# drones take any altitude split, every real drone gets through every decoy mix
# - and a measurement pinned to a ceiling is a constant, not a result". That
# diagnosis needed no sibling, no generation and no history. It needed the
# output.
#
# So this reads the output. Every percentage cell inside a *table* - a run of
# lines under one of these reports' own ---- rules, prose excluded, because
# prose says things like "loses 40% of its range" and that is a stat being
# quoted, not a trial being counted - is grouped into columns by the character
# offset its last digit lands on. These tables are printed through fixed-width
# format strings and every numeric column in them is right-aligned, so the end
# offset is the column key; a column parsed from a report whose alignment
# someone has broken simply splits into two short columns and falls under
# CEILING_MIN_COLUMN rather than producing a wrong accusation.
#
# Two things are then true of a column or they are not:
#
#   PINNED   - at least CEILING_PINNED_SHARE of its cells read exactly 0% or
#              100%. Those are the bounds of a share-of-trials, so a cell
#              sitting on one is a cell that could not have moved further in
#              the direction it is already at.
#   FLAT     - every cell in it is the same number. The sweep varied something
#              and the answer did not.
#
# What this deliberately cannot see, stated so nobody trusts it too far:
#
#   - It reads percentages only. MINES prints "0 of 4" and "yes, after 1
#     mine(s)" and has no percentage cell anywhere, so this detector says
#     nothing about it at all - correctly, because it has no way to know
#     whether "0 of 4" is a floor or a finding. VERTICAL's "quads lost" column
#     reads 0.08, 0.01, 0.00, 0.00, which is a floor in everything but type,
#     and this misses it. A bounded-measure detector needs to know the bounds,
#     and only the percent sign declares them.
#   - It cannot tell a ceiling that is a defect from one that is the point.
#     VERTICAL's Gun Mount column is 100% in all four rows *because a Gun Mount
#     cannot reach the high band at all*, which is the comparison that
#     experiment exists to draw, and its own commentary says so. The finding is
#     still correct - that column carries no information - and what to do about
#     it is a judgement for whoever owns src/KZ.Balance. That is why it warns.
#   - A ceiling is a property of the current numbers. An experiment can be
#     unpinned and still be exercising nothing; coverage is a different
#     question and this file has never been able to answer it (see FINDINGS 40
#     for what the ten actually touch).
#
# And one question that genuinely needs the ledger, asked without reference to
# any sibling: has this experiment's output *ever* changed? An experiment that
# has sat on one digest for the whole of a recorded history long enough to mean
# something is making a claim about itself, not about its neighbours. That is
# FROZEN below.

PERCENT_CELL = re.compile(r"(?<![\w.])(\d{1,3})%")
TABLE_RULE = re.compile(r"^\s*-{6,}\s*$")


def table_bodies(text):
    """The data rows of each table in one experiment's output: the lines after
    a ---- rule, up to the first blank line. Everything else is prose."""
    lines = text.split("\n")
    bodies, i = [], 0
    while i < len(lines):
        if TABLE_RULE.match(lines[i]):
            j = i + 1
            body = []
            while j < len(lines) and lines[j].strip():
                body.append(lines[j])
                j += 1
            if body:
                bodies.append(body)
            i = j
        else:
            i += 1
    return bodies


def percent_columns(text):
    """[(column key, [values...]), ...] for every percentage column in every
    table, in the order the columns appear."""
    columns = []
    for body in table_bodies(text):
        found = {}
        for line in body:
            for m in PERCENT_CELL.finditer(line):
                found.setdefault(m.end(), []).append(int(m.group(1)))
        for key in sorted(found):
            columns.append((key, found[key]))
    return columns


CEILING_ALLOW_TAGS = ("structural", "control")


def load_ceiling_allow(path):
    """{(experiment, (cells...)): reason}, plus the malformed lines. An entry
    names the column by value, so it stops applying the moment the numbers move
    - the dead-symbol allowlist's rule (a reason per entry, a tag the reviewer
    can accept or reject on its own) with the one addition this domain needs."""
    allow, errors = {}, []
    if not os.path.exists(path):
        return allow, errors
    with open(path, "r") as fh:
        for lineno, raw in enumerate(fh, start=1):
            line = raw.rstrip("\n")
            if not line.strip() or line.lstrip().startswith("#"):
                continue
            if "#" not in line:
                errors.append("ceiling-allow:%d: no reason - an entry without one is a warning "
                              "being suppressed: %r" % (lineno, line))
                continue
            body, reason = line.split("#", 1)
            reason = reason.strip()
            parts = body.split()
            if len(parts) != 2:
                errors.append("ceiling-allow:%d: expected '<experiment> <cells>  # <tag>: <why>', "
                              "got %r" % (lineno, line))
                continue
            name, cells = parts
            try:
                values = tuple(int(c) for c in cells.split(","))
            except ValueError:
                errors.append("ceiling-allow:%d: cells must be comma-separated whole percentages, "
                              "got %r" % (lineno, cells))
                continue
            tag = reason.split(":", 1)[0].strip()
            if tag not in CEILING_ALLOW_TAGS:
                errors.append("ceiling-allow:%d: reason must open with one of %s, got %r"
                              % (lineno, "/".join(CEILING_ALLOW_TAGS), tag))
                continue
            if len(reason) < 40:
                errors.append("ceiling-allow:%d: reason too short to judge: %r" % (lineno, reason))
                continue
            allow[(name, values)] = reason
    return allow, errors


def trim_bracket(values):
    """Drop a single leading and a single trailing cell when they sit on a
    bound. One rung below the transition and one above it is the shape of a
    sweep that found its range; everything after that is repetition."""
    body = list(values)
    if body and (body[0] == 0 or body[0] == 100):
        body = body[1:]
    if body and (body[-1] == 0 or body[-1] == 100):
        body = body[:-1]
    return body


def ceiling_findings(current_texts, names, allow=None):
    allow = allow or {}
    findings = []
    excused = 0
    for name in names:
        text = current_texts.get(name)
        if text is None:
            continue
        pinned_cols, flat_cols = [], []
        for _key, values in percent_columns(text):
            if (name, tuple(values)) in allow:
                excused += 1
                continue
            if len(values) >= FLAT_MIN_COLUMN and len(set(values)) == 1:
                flat_cols.append(values)
                continue
            if len(values) < CEILING_MIN_COLUMN:
                continue
            body = trim_bracket(values) if CEILING_TRIM_ENDS else list(values)
            if len(body) < CEILING_MIN_AFTER_TRIM:
                continue
            pinned = [v for v in body if v == 0 or v == 100]
            if len(pinned) >= CEILING_PINNED_SHARE * len(body):
                pinned_cols.append((values, body, len(pinned)))
        if not pinned_cols and not flat_cols:
            continue
        parts = []
        for values in flat_cols:
            parts.append("a column that does not vary at all (%s)"
                         % ", ".join("%d%%" % v for v in values))
        for values, body, n in pinned_cols:
            parts.append("%d of %d cells at 0%% or 100%% once the bracketing ends are "
                         "allowed for (%s)"
                         % (n, len(body), ", ".join("%d%%" % v for v in values)))
        findings.append(Finding(
            "ceiling", name, "; ".join(parts),
            "FINDINGS 32's prescription: put it back on its knees - fewer drones, more "
            "turrets, a defence that starts with an advantage - or, if the ceiling is the "
            "comparison the experiment exists to draw, say so in the experiment's own "
            "commentary so the next reader does not have to rediscover it, and record it in "
            "tools/experiment-ceiling-allow.txt with a reason"))
    return findings, excused


def frozen_findings(ledger, names, min_generations=6):
    """An experiment whose digest has never changed across a recorded history of
    at least min_generations. Unlike the sibling heuristic this replaces, it
    makes no claim about what else was happening - only that this experiment has
    one row in the ledger and the ledger is long enough for that to be a
    statement. Six generations because that is the span FINDINGS 32 drew its own
    conclusion from, and because anything shorter is a new experiment."""
    findings = []
    for name in names:
        runs = ledger.experiments.get(name)
        if not runs or len(runs) != 1:
            continue
        span = ledger.generation - runs[0].first_gen + 1
        if span < min_generations:
            continue
        findings.append(Finding(
            "frozen", name,
            "byte-identical output across the whole of its recorded history "
            "(gen %d-%d, %d generations)" % (runs[0].first_gen, ledger.generation, span),
            "check whether anything it exercises has changed in that span. If something "
            "has, the experiment cannot see it; if nothing has, say so and the row is "
            "evidence rather than a gap"))
    return findings



# ---------------------------------------------------------------------------
# Assembling a result. Pure function of (ledger, current outputs, discovered
# names) so the self-test can drive it without running any C# at all - the
# same split check_dead_symbols.py makes between parsing and its EXPECTED
# corpus.


class Result(object):
    def __init__(self, findings, ok_names, ledger_errors, generation, total, excused=0):
        self.findings = findings
        self.ok_names = ok_names
        self.excused = excused
        self.ledger_errors = ledger_errors
        self.generation = generation
        self.total = total

    @property
    def failing(self):
        return ([f for f in self.findings if f.category in ("crash", "new", "drift")]
                + list(self.ledger_errors))


def analyse(ledger, current_texts, names, crashes=(), ceiling_allow=None,
            allow_errors=()):
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

    judged = [n for n in names if n not in crash_names]
    ceilings, excused = ceiling_findings(current_texts, judged, ceiling_allow)
    findings.extend(ceilings)
    findings.extend(frozen_findings(ledger, judged))

    findings.sort(key=lambda f: (CATEGORY_ORDER.index(f.category), f.name))
    return Result(findings, ok, list(ledger.errors) + list(allow_errors),
                  ledger.generation, len(names), excused)


# ---------------------------------------------------------------------------
# Reporting.


def report(res, show_all=False):
    print("experiment-drift check: generation %d, %d experiment(s) tracked"
          % (res.generation, res.total))

    if res.ledger_errors:
        print("")
        print("  FAIL  the ledger or the ceiling allowlist is malformed (%d issue(s))"
              % len(res.ledger_errors))
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

    for cat in ("ceiling", "frozen"):
        items = by_cat.get(cat, [])
        if not items:
            continue
        print("")
        print("  WARN  %d experiment(s) %s - %s"
              % (len(items), "on a ceiling" if cat == "ceiling" else "frozen", CATEGORIES[cat]))
        for f in items:
            print("    %s" % f.name)
            print("        %s" % f.detail)
            if show_all:
                print("        %s" % f.fix)
        if not show_all:
            print("        (--all shows what to do about each)")

    items = by_cat.get("stale", [])
    if items:
        print("")
        print("  WARN  %d stale ledger entr(y/ies)" % len(items))
        for f in items:
            print("    %s - %s" % (f.name, f.fix))

    print("")
    fail_n = len(res.failing)
    warn_n = (len(by_cat.get("ceiling", [])) + len(by_cat.get("frozen", []))
              + len(by_cat.get("stale", [])))
    print("  %d ok, %d failing, %d warning, %d ceiling(s) allowlisted with a reason"
          % (len(res.ok_names), fail_n, warn_n, res.excused))

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
    for name in res.ok_names:
        print("ok\t%s\tmatches recorded baseline" % name)


# ---------------------------------------------------------------------------
# Self-test.
#
# No C# is compiled or run here - analyse(), ceiling_findings() and
# frozen_findings() all take already-captured text and a ledger, both supplied
# directly, exactly the split check_dead_symbols.py makes between parsing real
# sources and its EXPECTED corpus.
#
# The ceiling corpus is six synthetic reports, each one a shape this detector
# has to get right and each one drawn from a real row of the ten:
#
#   pinned        - SATURATION's shape. One column, six of eight cells at 0%
#                   or 100%, two cells doing work in the middle. Must flag:
#                   more of that column is a constant than is a result.
#   flat          - VERTICAL's Gun Mount column. Four identical cells. Must
#                   flag, and must flag as "does not vary" rather than as
#                   pinned, because a flat column of 50% is just as dead and
#                   needs no assumption about where the bounds are.
#   live          - STACKING's shape. A full table that moves in every column
#                   and touches 100% once. Must NOT flag - one cell at a bound
#                   is a sweep reaching the end of its range.
#   edge          - APPROACH's shape: a seven-rung sweep whose last row is
#                   100%. One in seven is under half. Must NOT flag. This is
#                   the case the old sibling heuristic got wrong and the
#                   reason the threshold is a share rather than a count.
#   prose         - a live table with percentages in the paragraph under it
#                   ("loses 40% of its range", "30% of its hit chance", both
#                   from VERTICAL's own commentary). Must NOT flag: a stat
#                   quoted in prose is not a trial being counted, and reading
#                   it as one is how a detector starts producing numbers
#                   nobody can trace back to a cell.
#   short         - two cells, both 100%. Must NOT flag. CEILING_MIN_COLUMN
#                   exists because two coincidences are not evidence.
#   bracketed     - 0, 36, 75, 98, 100. The shape a sweep should have: one
#                   rung the defence always survives, three that move, one it
#                   never survives. Must NOT flag, and it is the pair with
#                   `repeated` that the trim rule exists for.
#   repeated      - 0, 0, 75, 100, 100. The same transition, with a rung
#                   wasted at each end. Must flag.
#
# The ledger fixture is a miniature project history for the drift, stale and
# frozen paths:
#
#   steady            - changes at every generation. Never frozen.
#   frozen            - one digest across all six generations. Must be flagged.
#   recently_settled  - two runs, so it has moved at least once. Never frozen,
#                       however long its current run is.
#   newcomer          - born at generation 5. One run, but a span of two
#                       generations, under the six a claim needs. Must NOT be
#                       flagged - there has not been time to tell, and saying
#                       so by silence is the honest reading.
#   mover1/2/3        - kept from the fixture this file had when its signal was
#                       sibling-relative. They are no longer evidence for
#                       anything, and they stay because the save/load round
#                       trip below is a better test with seven experiments in
#                       the ledger than with four.
#
# What this deliberately does not try to test: whether a flagged ceiling is a
# defect or the comparison an experiment exists to draw. VERTICAL's Gun Mount
# column is genuinely 100% in every row and genuinely the point of that table.
# This file has no way to tell those apart and does not claim to - it warns,
# with the cells printed, and the judgement belongs to whoever owns
# src/KZ.Balance.


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
    # Three more experiments, each moving exactly once. They were evidence for
    # the sibling-relative signal this file used to carry and are evidence for
    # nothing now; they stay so the save/load round trip below runs against a
    # ledger with seven experiments in it rather than four.
    add("mover1", [(1, 1, "m1a"), (2, 6, "m1b")])
    add("mover2", [(1, 2, "m2a"), (3, 6, "m2b")])
    add("mover3", [(1, 3, "m3a"), (4, 6, "m3b")])
    return ledger


def selftest():
    ok = True

    # --- the ceiling detector, over its corpus of report shapes -------------
    corpus = {
        # SATURATION's shape: six of eight cells on a bound.
        "pinned": normalize(
            "  drones   gun killed\n"
            "  ----------------------\n"
            "       1           0%\n"
            "       2           0%\n"
            "       3          37%\n"
            "       5          98%\n"
            "       8         100%\n"
            "      12         100%\n"
            "      16         100%\n"
            "      24         100%\n"),
        # VERTICAL's Gun Mount column: four cells, one value.
        "flat": normalize(
            "  attack            mount killed\n"
            "  ------------------------------\n"
            "  all low                   100%\n"
            "  three low, two high       100%\n"
            "  two low, three high       100%\n"
            "  all high                  100%\n"),
        # STACKING's shape: every column moves; one cell happens to reach 100%.
        "live": normalize(
            "  turrets  3 drones 4 drones 5 drones 6 drones\n"
            "  -------------------------------------------\n"
            "        1       41%      76%      99%     100%\n"
            "        2       42%      78%      96%      99%\n"
            "        3       41%      74%      95%      98%\n"
            "        4       41%      73%      95%      98%\n"),
        # APPROACH's shape: a sweep that runs off the end of its range.
        "edge": normalize(
            "  launch at   gun killed\n"
            "  ----------------------\n"
            "    14400 m          81%\n"
            "    17400 m          86%\n"
            "    18200 m          90%\n"
            "    18600 m          86%\n"
            "    18900 m          85%\n"
            "    19200 m          94%\n"
            "    19500 m         100%\n"),
        # A live table with percentages in the paragraph under it.
        "prose": normalize(
            "  attack       mount killed\n"
            "  -------------------------\n"
            "  all low               10%\n"
            "  two high              32%\n"
            "  three high            34%\n"
            "  all high              29%\n"
            "\n"
            "  The Autocannon loses 40% of its range and 30% of its hit\n"
            "  chance shooting upward, and 100% of nothing either way.\n"),
        # Two cells on a bound is a coincidence, not a ceiling.
        "short": normalize(
            "  arm      killed\n"
            "  ---------------\n"
            "  first      100%\n"
            "  second     100%\n"),
        # A sweep that brackets its transition: one rung the defence always
        # survives, three that move, one it never survives. Must NOT flag.
        "bracketed": normalize(
            "  drones   killed\n"
            "  ---------------\n"
            "       2       0%\n"
            "       3      36%\n"
            "       4      75%\n"
            "       5      98%\n"
            "       6     100%\n"),
        # The same sweep with a rung repeated at each bound. Two of the five
        # rungs are now saying what one already said. Must flag.
        "repeated": normalize(
            "  drones   killed\n"
            "  ---------------\n"
            "       1       0%\n"
            "       2       0%\n"
            "       4      75%\n"
            "       6     100%\n"
            "       8     100%\n"),
    }
    found, excused = ceiling_findings(corpus, sorted(corpus))
    got = dict((f.name, f.detail) for f in found)

    print("self-test: ceilings over a corpus of report shapes")
    checks = [
        ("pinned", "pinned" in got, "six of eight cells at a bound - must flag"),
        ("flat", "flat" in got, "a column with one value in four rows - must flag"),
        ("live", "live" not in got, "every column moves, one cell reaches 100% - must not flag"),
        ("edge", "edge" not in got, "one row in seven at 100% is a sweep ending, not a ceiling"),
        ("prose", "prose" not in got, "percentages in the commentary are not trial counts"),
        ("short", "short" not in got, "two cells at 100% is below CEILING_MIN_COLUMN"),
        ("bracketed", "bracketed" not in got,
         "one rung below the transition and one above it is a sweep that found its range"),
        ("repeated", "repeated" in got, "a second rung on the same bound told nobody anything"),
    ]
    for name, cond, why in checks:
        print("  %s %-7s %s" % ("PASS" if cond else "FAIL", name, why))
        ok = ok and cond

    cond = "does not vary" in got.get("flat", "")
    print("  %s a flat column is reported as flat, not as pinned" % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    cond = "0%" in got.get("pinned", "") and "37%" in got.get("pinned", "")
    print("  %s a ceiling finding prints the cells it is accusing" % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    cond = percent_columns(corpus["prose"]) == [(27, [10, 32, 34, 29])]
    print("  %s percent_columns() reads table bodies only, got %s"
          % ("PASS" if cond else "FAIL", percent_columns(corpus["prose"])))
    ok = ok and cond

    cond = excused == 0
    print("  %s nothing is excused when no allowlist is passed" % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    # --- the ceiling allowlist ----------------------------------------------
    reason = ("structural: a Gun Mount cannot engage the High band at all, so this column is "
              "the contrast rather than the measurement")
    allowed, excused = ceiling_findings(corpus, sorted(corpus),
                                        {("flat", (100, 100, 100, 100)): reason})
    names_allowed = set(f.name for f in allowed)
    cond = "flat" not in names_allowed and excused == 1
    print("  %s an allowlisted column is excused and counted, not reported"
          % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    cond = "pinned" in names_allowed
    print("  %s allowlisting one column does not excuse the rest of the corpus"
          % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    stale, _ = ceiling_findings(corpus, sorted(corpus),
                                {("flat", (100, 100, 100, 99)): reason})
    cond = "flat" in set(f.name for f in stale)
    print("  %s an allowlist entry whose cells no longer match stops excusing anything"
          % ("PASS" if cond else "FAIL"))
    ok = ok and cond

    allow_path = os.path.join(FIXTURE, ".allow.tmp")
    try:
        with open(allow_path, "w") as fh:
            fh.write("# a comment\n\n"
                     "good  100,100,100,100  # structural: measured across three flight sizes "
                     "and it is the units, not the rungs\n"
                     "noreason  100,100,100,100\n"
                     "badtag  100,100  # because I said so: it is fine\n"
                     "shortreason  100,100  # control: fine\n"
                     "notnumbers  a,b  # structural: the cells are not percentages at all here\n")
        entries, errors = load_ceiling_allow(allow_path)
        cond = list(entries) == [("good", (100, 100, 100, 100))]
        print("  %s only the well-formed allowlist entry is loaded, got %s"
              % ("PASS" if cond else "FAIL", sorted(entries)))
        ok = ok and cond
        cond = len(errors) == 4
        print("  %s no reason, a bad tag, a reason too short and unparseable cells each error "
              "rather than passing silently (%d)" % ("PASS" if cond else "FAIL", len(errors)))
        ok = ok and cond
    finally:
        if os.path.exists(allow_path):
            os.remove(allow_path)

    # --- frozen, over a synthetic six-generation history --------------------
    ledger = build_fixture_ledger()
    names = ["steady", "frozen", "recently_settled", "newcomer"]
    got = set(f.name for f in frozen_findings(ledger, names))

    print("self-test: frozen over a synthetic six-generation history")
    checks = [
        ("frozen", "frozen" in got, "one digest across all six generations - must flag"),
        ("steady", "steady" not in got, "changes every generation - must never flag"),
        ("recently_settled", "recently_settled" not in got,
         "has moved once, so it is not frozen however long its current run is"),
        ("newcomer", "newcomer" not in got,
         "one run, but only two generations old - too little history to claim anything"),
    ]
    for name, cond, why in checks:
        print("  %s %-16s %s" % ("PASS" if cond else "FAIL", name, why))
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
    ceiling_allow_path = DEFAULT_CEILING_ALLOW
    if "--ceiling-allow" in argv:
        ceiling_allow_path = os.path.abspath(argv[argv.index("--ceiling-allow") + 1])

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
    ceiling_allow, allow_errors = load_ceiling_allow(ceiling_allow_path)
    outputs, failures = capture_all(exe_argv, names, verify_determinism=False)
    res = analyse(ledger, outputs, names, crashes=failures,
                  ceiling_allow=ceiling_allow, allow_errors=allow_errors)

    if "--list" in argv:
        list_mode(res)
        return 0

    status = report(res, show_all="--all" in argv)
    return 0 if "--warn-only" in argv else status


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
