#!/usr/bin/env python3
# KILL ZONE - a real-time strategy video game.
#
# The dead-symbol build guard.
#
# Three systems shipped inert because a symbol was written and then never used
# by anything: Terrain.BlocksGroundSight (defined, zero callers, so detection
# had no line of sight at all), SimConstants.TrackHoldTicks (zero call sites,
# and a findings document drew a conclusion on top of it), and the Gun Mount's
# traverse rate and magazine (the fields exist and the code reads them, but the
# only unit that sets either is the Interceptor Battery, so for the Gun Mount
# the slew code returns early and reload accounting is skipped).
#
# Each was invisible to careful reading and one grep away from obvious. This is
# that grep, run by build.sh, so it stops being a lesson and becomes a step.
#
# It is plain Python on purpose. This is build tooling, not simulation: it is
# not on the determinism path, and the repository has to keep building with mcs
# or dotnet and nothing else - no analyser package, no package manager.
#
# What it checks:
#
#   sim-public       a public member in src/KZ.Sim with no reference outside its
#                    own declaration
#   sim-constant     a SimConstants value never referenced
#   sim-event        a SimEventKind never pushed
#   unitdef-field    a UnitDef field never read outside Defs.cs
#   unitdef-default  a UnitDef field no unit def ever sets, so the simulation
#                    only ever sees the declared default
#   unitdef-single   a UnitDef field exactly one unit sets - the Gun Mount shape
#   enum-value       an enum value never constructed or compared against
#   component-flag   a ComponentMask flag never tested
#   write-only       a public field in src/KZ.Sim assigned everywhere and read
#                    nowhere - the value goes in and never comes out
#   test-only        any of the above that only src/KZ.Tests touches
#
# test-only is deliberately its own category rather than an allowlist reason.
# "referenced only by tests" is precisely the failure mode that produced
# FINDINGS 30: three systems with passing tests and no production caller. A
# symbol in that state is not exonerated by its test, it is accused by it - so
# it gets named out loud in its own block rather than disappearing into an
# allowlist as "used by tests".
#
# Usage:
#   python3 tools/check_dead_symbols.py             report, and set exit status
#   python3 tools/check_dead_symbols.py --list      one finding per line, for
#                                                   regenerating DEAD-SYMBOLS.md
#   python3 tools/check_dead_symbols.py --selftest  run the checker's own
#                                                   regression corpus
#   ... --root DIR                                  analyse a different checkout
#   ... --warn-only                                 report but never fail

import os
import re
import sys

try:                      # so piping the report into head is not a crash
    import signal
    signal.signal(signal.SIGPIPE, signal.SIG_DFL)
except (ImportError, AttributeError, ValueError):
    pass

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
FIXTURE = os.path.join(HERE, "testdata", "dead-symbols")

# ---------------------------------------------------------------------------
# What fails the build, and what only warns.
#
# A checker that cries wolf gets disabled within a week, and then the next dead
# system ships. The first honest run of this one found eighty-odd symbols across
# nine categories - a wall of failures nobody caused and nobody could clear in a
# sitting, which is the fastest known way to have a build step deleted.
#
# So it is a ratchet rather than a wall:
#
#   - every finding already in tools/dead-symbols-baseline.txt is recorded debt.
#     It warns. It is in docs/DEAD-SYMBOLS.md with an owner in WIRING-SPEC.
#   - every finding NOT in that file is new, and fails the build, in every
#     category. Introducing a fresh dead symbol is the thing worth stopping, and
#     it is always cheap to fix at the moment you introduce it.
#   - a baseline entry that stops being found warns too, asking you to delete
#     the line. The ledger only goes down.
#
# That answers "should some categories warn rather than fail" better than a
# per-category severity would: nothing is softened for the future, only for the
# past, and the past is written down and shrinking.
CATEGORIES = {
    "sim-event":       "an event kind nothing pushes is a system that never reports itself",
    "component-flag":  "a flag nothing tests is a component that does nothing",
    "unitdef-field":   "a def field nothing reads is the stat block lying about the game",
    "unitdef-default": "a def field no unit sets means the default is the only value in the game",
    "sim-constant":    "a constant nothing reads is the TrackHoldTicks failure exactly",
    "unitdef-single":  "a stat one unit carries is the Gun Mount failure exactly",
    "test-only":       "green tests and no production caller is the FINDINGS 30 failure exactly",
    "enum-value":      "a state nothing can enter",
    "write-only":      "a value that goes in and never comes out",
    "sim-public":      "a public member with no caller - the strongest signal of unfinished work",
}

CATEGORY_ORDER = ["sim-event", "component-flag", "unitdef-field", "unitdef-default",
                  "sim-constant", "unitdef-single", "test-only", "enum-value", "write-only",
                  "sim-public"]

# Reason tags an allowlist entry may use. The tag is the point: a reviewer
# judges the entry from the tag alone and only reads the prose when the tag
# surprises them. Free text with no tag is rejected.
REASON_TAGS = {
    "interface-layer": "API for the renderer, interface or telemetry layer that does not exist yet",
    "planned":         "a named WIRING-SPEC task will wire it - write planned:<task>",
    "symmetry":        "kept so a small vocabulary is complete (an enum's None, a paired accessor)",
    "external":        "consumed outside src/ - tools, data authoring, future mods",
    "language":        "the C# language or the runtime requires it to exist",
    "unique":          "a stat only one unit is meant to carry, and the reader handles the default",
}

MIN_REASON_CHARS = 12

# Members every C# type has or needs, which say nothing about whether the type
# is alive. These are structural, not judgement calls, so they are exempt here
# rather than in the allowlist.
SKIP_NAMES = {"Equals", "GetHashCode", "ToString", "Main", "Dispose"}


# ---------------------------------------------------------------------------
# Source loading and cleaning.


def strip_noise(text):
    """Blank out comments, string and char literals, and nameof() arguments,
    preserving every byte offset and newline so reported line numbers stay true.

    nameof(X) goes because it is a mention of a name, not a use of the thing: a
    symbol kept alive only by a nameof is still dead."""
    out = list(text)
    i, n = 0, len(text)

    def blank(a, b):
        for k in range(a, b):
            if out[k] != "\n":
                out[k] = " "

    while i < n:
        c = text[i]
        if c == "/" and i + 1 < n and text[i + 1] == "/":
            j = text.find("\n", i)
            j = n if j < 0 else j
            blank(i, j)
            i = j
        elif c == "/" and i + 1 < n and text[i + 1] == "*":
            j = text.find("*/", i + 2)
            j = n if j < 0 else j + 2
            blank(i, j)
            i = j
        elif c == '"':
            verbatim = i > 0 and text[i - 1] == "@"
            interp = i > 0 and text[i - 1] == "$"
            j = i + 1
            while j < n:
                ch = text[j]
                if verbatim:
                    if ch == '"':
                        if j + 1 < n and text[j + 1] == '"':
                            j += 2
                            continue
                        j += 1
                        break
                    j += 1
                else:
                    if ch == "\\":
                        j += 2
                        continue
                    if ch == '"':
                        j += 1
                        break
                    if ch == "\n":
                        break
                    j += 1
            if interp:
                # Keep the code inside {...} holes; blank the literal parts.
                depth = 0
                for k in range(i, j):
                    if text[k] == "{" and depth == 0:
                        depth = 1
                    elif text[k] == "}" and depth == 1:
                        depth = 0
                        if out[k] != "\n":
                            out[k] = " "
                    elif depth == 0 and out[k] != "\n":
                        out[k] = " "
            else:
                blank(i, j)
            i = j
        elif c == "'":
            j = i + 1
            while j < n:
                if text[j] == "\\":
                    j += 2
                    continue
                if text[j] == "'":
                    j += 1
                    break
                if text[j] == "\n":
                    break
                j += 1
            blank(i, j)
            i = j
        else:
            i += 1

    cleaned = "".join(out)

    def kill_nameof(m):
        keep = m.group(0)[: m.start(1) - m.start(0)]
        return keep + " " * len(m.group(1)) + m.group(0)[m.end(1) - m.start(0):]

    return re.sub(r"\bnameof\s*\(\s*([\w\.]+)\s*\)", kill_nameof, cleaned)


class SourceFile(object):
    def __init__(self, root, path, text):
        self.path = path
        self.rel = os.path.relpath(path, root)
        self.lines = strip_noise(text).split("\n")
        parts = self.rel.split(os.sep)
        self.project = parts[1] if len(parts) > 1 else "?"
        self.is_sim = self.project == "KZ.Sim"
        self.is_test = self.project == "KZ.Tests"
        self.base = parts[-1]


def load_sources(root):
    files = []
    src = os.path.join(root, "src")
    for base, _dirs, names in os.walk(src):
        for name in sorted(names):
            if name.endswith(".cs"):
                path = os.path.join(base, name)
                with open(path, "r") as fh:
                    files.append(SourceFile(root, path, fh.read()))
    files.sort(key=lambda f: f.rel)
    return files


# ---------------------------------------------------------------------------
# Declaration parsing.
#
# Regex rather than a C# parser, which is the right trade here: these sources
# are one hand-written style with no preprocessor, no partial classes and no
# generated code, and the alternative is a package-manager dependency the
# repository is not allowed to grow. Three rules keep it honest:
#
#   - parse only cleaned text, so a name in a comment or a string is not a use;
#   - match whole tokens only, so a short field name cannot match a substring;
#   - know each declaration's full span, so a symbol cannot be kept alive by
#     its own body (a recursive call is not a call site).

MODIFIERS = (r"(?:public|private|protected|internal|static|readonly|const|sealed|abstract|"
             r"virtual|override|extern|unsafe|new|partial|async|volatile|ref|event)")
TYPE_DECL_RE = re.compile(
    r"^\s*(?P<mods>(?:%s\s+)*)(?P<kind>class|struct|enum|interface|delegate)\s+(?P<name>\w+)" % MODIFIERS)
MEMBER_RE = re.compile(r"^\s*public\s+(?P<body>.*)$")


class Decl(object):
    def __init__(self, kind, name, owner, sf, line, end_line):
        self.kind = kind          # type | method | property | field | enumvalue
        self.name = name
        self.owner = owner        # declaring type, "" for a top-level type
        self.sf = sf
        self.line = line          # 1-based
        self.end_line = end_line  # 1-based, inclusive
        self.key = (owner + "." + name) if owner else name

    @property
    def span(self):
        return (self.sf, self.line, self.end_line)

    def __repr__(self):
        return "<%s %s %s:%d>" % (self.kind, self.key, self.sf.rel, self.line)


def find_span(sf, start_idx):
    """The span of the declaration starting on line start_idx (0-based): to its
    matching close brace if it has a body, otherwise to its terminating ;."""
    depth = 0
    seen_brace = False
    i = start_idx
    while i < len(sf.lines):
        for ch in sf.lines[i]:
            if ch == "{":
                depth += 1
                seen_brace = True
            elif ch == "}":
                depth -= 1
                if seen_brace and depth <= 0:
                    return i
            elif ch == ";" and not seen_brace and depth == 0:
                return i
        i += 1
    return len(sf.lines) - 1


def parse_member(sf, idx, body, owner):
    """Classify one `public ...` line. Returns a list, because one field line
    may declare several names."""
    body = re.sub(r"^(?:%s\s+)*" % MODIFIERS, "", body.strip())
    if not body or body.startswith("(") or re.match(r"^(class|struct|enum|interface|delegate)\b", body):
        return []

    head = re.split(r"[=;{(]", body, 1)[0]
    sep = body[len(head): len(head) + 1]
    names = re.findall(r"[A-Za-z_]\w*", head)
    if not names:
        return []
    name = names[-1]
    if name == owner:                                   # constructor
        return []
    if "operator" in head or re.search(r"\bthis\s*\[", body):
        return []
    if name in SKIP_NAMES:
        return []

    end = find_span(sf, idx)
    if sep == "(":
        kind = "method"
    elif sep == "{" or body[len(head):].lstrip().startswith("=>"):
        kind = "property"
    else:
        kind = "field"

    out = [Decl(kind, name, owner, sf, idx + 1, end + 1)]
    if kind == "field":
        tail = body[len(head):]
        m = re.match(r"^\s*(?:=[^;,]*)?((?:,\s*\w+(?:\s*=[^;,]*)?)+)\s*;", tail)
        if m:
            for extra in re.findall(r",\s*(\w+)", m.group(1)):
                out.append(Decl("field", extra, owner, sf, idx + 1, end + 1))
    return out


def parse_declarations(sf):
    """Every public member and every enum value declared in this file."""
    decls = []
    stack = []           # (type name, kind, brace depth of its body)
    depth = 0
    pending = None       # a type declaration awaiting its opening brace

    for idx, line in enumerate(sf.lines):
        stripped = line.strip()
        in_enum = bool(stack) and stack[-1][1] == "enum"
        owner = stack[-1][0] if stack else ""

        if in_enum and stripped and not stripped.startswith("}") and "{" not in stripped:
            m = re.match(r"^(\w+)\s*(=[^,]*)?,?\s*$", stripped)
            if m:
                decls.append(Decl("enumvalue", m.group(1), owner, sf, idx + 1, idx + 1))
        elif not in_enum:
            tm = TYPE_DECL_RE.match(line)
            if tm and tm.group("kind") != "delegate":
                if "public" in tm.group("mods"):
                    decls.append(Decl("type", tm.group("name"), owner, sf,
                                      idx + 1, find_span(sf, idx) + 1))
                # Non-public types are not reported, but their bodies still have
                # to be tracked so their members are attributed correctly.
                pending = (tm.group("name"), tm.group("kind"))
            else:
                mm = MEMBER_RE.match(line)
                if mm and stack:
                    decls.extend(parse_member(sf, idx, mm.group("body"), owner))

        for ch in line:
            if ch == "{":
                depth += 1
                if pending is not None:
                    stack.append((pending[0], pending[1], depth))
                    pending = None
            elif ch == "}":
                if stack and stack[-1][2] == depth:
                    stack.pop()
                depth -= 1

    return decls


# ---------------------------------------------------------------------------
# Reference index.


class Index(object):
    def __init__(self, files):
        self.tokens = {}     # name -> [(sf, line)]
        self.qualified = {}  # (lhs, rhs) -> [(sf, line)]
        for sf in files:
            for lineno, line in enumerate(sf.lines, start=1):
                for m in re.finditer(r"[A-Za-z_]\w*", line):
                    self.tokens.setdefault(m.group(0), []).append((sf, lineno))
                for m in re.finditer(r"\b([A-Za-z_]\w*)\s*\.\s*([A-Za-z_]\w*)", line):
                    self.qualified.setdefault((m.group(1), m.group(2)), []).append((sf, lineno))

    @staticmethod
    def _filter(hits, exclude):
        out = []
        for sf, lineno in hits:
            if not any(xsf is sf and a <= lineno <= b for xsf, a, b in exclude):
                out.append((sf, lineno))
        return out

    def refs(self, name, exclude=()):
        return self._filter(self.tokens.get(name, ()), exclude)

    def qrefs(self, lhs, rhs, exclude=()):
        return self._filter(self.qualified.get((lhs, rhs), ()), exclude)


# ---------------------------------------------------------------------------
# Findings.


class Finding(object):
    def __init__(self, category, key, sf, line, what, detail, fix):
        self.category = category
        self.key = key
        self.sf = sf
        self.line = line
        self.what = what      # "public method", "enum value", ...
        self.detail = detail  # why it is being reported
        self.fix = fix        # what to do about it

    def sort_key(self):
        return (self.sf.rel, self.line, self.key)


def where(refs, limit=3):
    seen = []
    for sf, line in refs:
        s = "%s:%d" % (sf.rel, line)
        if s not in seen:
            seen.append(s)
        if len(seen) >= limit:
            break
    return ", ".join(seen)


TEST_FIX = ("a test is not a production path. Exercise it through World.Spawn and "
            "World.Step, or allowlist it with a reason")


# ---------------------------------------------------------------------------
# The checks.


def check_sim_events(files, index, decls):
    findings = []
    pushes = {}
    for sf in files:
        for lineno, line in enumerate(sf.lines, start=1):
            for m in re.finditer(r"\bPush\w*\s*\(\s*SimEventKind\s*\.\s*(\w+)", line):
                pushes.setdefault(m.group(1), []).append((sf, lineno))

    for d in decls:
        if d.kind != "enumvalue" or d.owner != "SimEventKind" or d.name == "None":
            continue
        hits = pushes.get(d.name, [])
        prod = [h for h in hits if not h[0].is_test]
        if not hits:
            reads = index.qrefs("SimEventKind", d.name, exclude=[d.span])
            extra = " (read at %s, but nothing raises it)" % where(reads) if reads else ""
            findings.append(Finding(
                "sim-event", "SimEventKind." + d.name, d.sf, d.line, "event kind",
                "no Push(SimEventKind.%s, ...) anywhere in src/%s" % (d.name, extra),
                "push it where the thing it names happens, delete it, or allowlist it with a reason"))
        elif not prod:
            findings.append(Finding(
                "test-only", "SimEventKind." + d.name, d.sf, d.line, "event kind",
                "pushed only from src/KZ.Tests (%s)" % where(hits), TEST_FIX))
    return findings


def check_component_flags(files, index, decls):
    """A flag that is set and never tested is a component that does nothing."""
    tested, was_set = {}, {}
    for sf in files:
        for lineno, line in enumerate(sf.lines, start=1):
            for m in re.finditer(r"\bComponentMask\s*\.\s*(\w+)", line):
                before = line[:m.start()]
                name = m.group(1)
                if re.search(r"\b(?:Has|HasAny|HasAll)\s*\([^()]*$", before) or before.rstrip().endswith("&"):
                    tested.setdefault(name, []).append((sf, lineno))
                elif re.search(r"\bAddComponent\s*\([^()]*$", before) or before.rstrip().endswith("|"):
                    was_set.setdefault(name, []).append((sf, lineno))
                else:
                    tested.setdefault(name, []).append((sf, lineno))

    findings = []
    for d in decls:
        if d.kind != "enumvalue" or d.owner != "ComponentMask" or d.name == "None":
            continue
        t, s = tested.get(d.name, []), was_set.get(d.name, [])
        prod = [h for h in t if not h[0].is_test]
        if not t:
            extra = (" (set at %s, and nothing ever asks)" % where(s)) if s else " (and never set either)"
            findings.append(Finding(
                "component-flag", "ComponentMask." + d.name, d.sf, d.line, "component flag",
                "never tested with Has/HasAny%s" % extra,
                "make a system branch on it, delete it, or allowlist it with a reason"))
        elif not prod:
            findings.append(Finding(
                "test-only", "ComponentMask." + d.name, d.sf, d.line, "component flag",
                "tested only from src/KZ.Tests (%s)" % where(t), TEST_FIX))
    return findings


def check_sim_constants(files, index, decls):
    findings = []
    for d in decls:
        if d.kind != "field" or d.owner != "SimConstants":
            continue
        refs = index.refs(d.name, exclude=[d.span])
        outside = [r for r in refs if r[0] is not d.sf]
        prod = [r for r in outside if not r[0].is_test]
        if not refs:
            findings.append(Finding(
                "sim-constant", "SimConstants." + d.name, d.sf, d.line, "constant",
                "never referenced anywhere in src/",
                "wire it to the system it was tuned for, delete it, or allowlist it with a reason"))
        elif not outside:
            findings.append(Finding(
                "sim-constant", "SimConstants." + d.name, d.sf, d.line, "constant",
                "referenced only inside SimConstants.cs (%s)" % where(refs),
                "a number that only feeds other numbers is still not wired to a system"))
        elif not prod:
            findings.append(Finding(
                "test-only", "SimConstants." + d.name, d.sf, d.line, "constant",
                "referenced only from src/KZ.Tests (%d refs: %s)" % (len(outside), where(outside)),
                "the simulation does not read this number; a test asserting on it proves nothing"))
    return findings


def unitdef_receivers(sf):
    """Names in this file that hold a UnitDef."""
    names = set()
    for line in sf.lines:
        for m in re.finditer(r"\bUnitDef\s+(\w+)\b", line):
            names.add(m.group(1))
        for m in re.finditer(r"\bvar\s+(\w+)\s*=\s*(?:Catalog|Defs)\s*\.\s*(?:Get|ByName)\s*\(", line):
            names.add(m.group(1))
    names -= {"Get", "ByName"}
    return names


def check_unitdef_fields(files, index, decls, raw_defs_text):
    findings = []
    defs_file = None
    for sf in files:
        if sf.base == "Defs.cs":
            defs_file = sf
    if defs_file is None:
        return findings

    fields = [d for d in decls if d.kind == "field" and d.owner == "UnitDef"]

    # --- read outside Defs.cs --------------------------------------------
    reads = {}
    for sf in files:
        if sf is defs_file:
            continue
        recv = unitdef_receivers(sf)
        for lineno, line in enumerate(sf.lines, start=1):
            for m in re.finditer(r"\b(\w+)\s*\.\s*(\w+)", line):
                if m.group(1) in recv:
                    reads.setdefault(m.group(2), []).append((sf, lineno))
            for m in re.finditer(r"\b(?:Catalog|Defs)\s*\.\s*(?:Get|ByName)\s*\([^()]*\)\s*\.\s*(\w+)", line):
                reads.setdefault(m.group(1), []).append((sf, lineno))

    # --- set by which units ----------------------------------------------
    # Which unit defs set which field. A unit whose name starts with "Test " is
    # a harness fixture, and a harness unit carrying a stat is not evidence that
    # the game uses it - that is the rule this whole checker exists to enforce,
    # applied to the catalogue instead of to the test suite.
    carriers = {}          # field -> [real unit name]
    harness = {}           # field -> [harness unit name]
    stamped = set()        # fields assigned outside an initializer, e.g. d.Id
    text = raw_defs_text
    for m in re.finditer(r"new\s+UnitDef\s*\{", text):
        depth, i = 1, m.end()
        while i < len(text) and depth:
            if text[i] == "{":
                depth += 1
            elif text[i] == "}":
                depth -= 1
            i += 1
        block = text[m.end():i - 1]
        nm = re.search(r'\bName\s*=\s*"([^"]*)"', block)
        unit = nm.group(1) if nm else "(unnamed def)"
        bucket = harness if unit.startswith("Test ") else carriers
        for field in sorted(set(re.findall(r"(?:^|[,{])\s*(\w+)\s*=[^=]", block))):
            bucket.setdefault(field, []).append(unit)
    for m in re.finditer(r"\b\w+\s*\.\s*(\w+)\s*=[^=]", "\n".join(defs_file.lines)):
        stamped.add(m.group(1))

    for d in fields:
        got = reads.get(d.name, [])
        prod = [r for r in got if not r[0].is_test]
        held = carriers.get(d.name, [])
        rig = harness.get(d.name, [])

        if not got:
            findings.append(Finding(
                "unitdef-field", "UnitDef." + d.name, d.sf, d.line, "def field",
                "written in Defs.cs and read nowhere outside it",
                "make a system read it, delete it, or allowlist it with a reason"))
        elif not prod:
            findings.append(Finding(
                "test-only", "UnitDef." + d.name, d.sf, d.line, "def field",
                "read only from src/KZ.Tests (%s)" % where(got),
                "the simulation ignores this stat; the test reads the number back out of the table"))
        elif not held and not rig and d.name not in stamped:
            findings.append(Finding(
                "unitdef-default", "UnitDef." + d.name, d.sf, d.line, "def field",
                "read by the simulation and set by none of the %d unit defs, so the "
                "declared default is the only value the game can ever see" % count_defs(text),
                "give the units that should carry it a value, delete it, or allowlist it with a reason"))
        elif not held and rig:
            findings.append(Finding(
                "unitdef-single", "UnitDef." + d.name, d.sf, d.line, "def field",
                "carried only by the harness unit %s; every unit the game actually "
                "fields gets the default" % rig[0],
                "a harness fixture carrying a stat is not the game using it - give a real "
                "unit the value, or allowlist it with a reason"))
        elif len(held) == 1:
            also = " (and the harness unit %s)" % rig[0] if rig else ""
            findings.append(Finding(
                "unitdef-single", "UnitDef." + d.name, d.sf, d.line, "def field",
                "carried by exactly one unit, %s%s; every other unit gets the default, "
                "and the code that reads this stat is written to skip the default"
                % (held[0], also),
                "check the units that ought to carry it too - this is the Gun Mount shape "
                "from audit F4 - then allowlist it as unique: if one carrier is right"))
    return findings


def count_defs(text):
    return len(re.findall(r"new\s+UnitDef\s*\{", text))


def check_enum_values(files, index, decls):
    """An enum value has to be constructed or compared against somewhere.

    Matched only in its qualified form (EnumType.Value). C# requires that
    qualification nearly everywhere - switch labels, initialisers, comparisons -
    so this is both accurate and immune to short-name collisions between the
    dozen enums that all declare a None or a Ground."""
    findings = []
    own = {"SimEventKind", "ComponentMask"}   # have their own, sharper checks
    for d in decls:
        if d.kind != "enumvalue" or d.owner in own or not d.sf.is_sim:
            continue
        if d.name == "None":
            # The zero value. C# hands it to every default-initialised field, so
            # instances of it exist whether or not anything writes it by name.
            # Structural, not a judgement call, so it is exempt here rather than
            # in the allowlist. Any *other* zero-valued member is a judgement
            # call and stays flagged.
            continue
        refs = index.qrefs(d.owner, d.name, exclude=[d.span])
        prod = [r for r in refs if not r[0].is_test]
        if not refs:
            findings.append(Finding(
                "enum-value", d.owner + "." + d.name, d.sf, d.line, "enum value",
                "never constructed or compared against anywhere in src/",
                "give it a unit or a code path, delete it, or allowlist it with a reason"))
        elif not prod:
            findings.append(Finding(
                "test-only", d.owner + "." + d.name, d.sf, d.line, "enum value",
                "used only from src/KZ.Tests (%d refs: %s)" % (len(refs), where(refs)),
                "nothing in the simulation ever puts an entity in this state"))
    return findings


def check_write_only(files, index, decls, already):
    """A field the simulation fills in and never reads back.

    Half of a dead wire looks exactly like a live one from the writing end -
    World.Spawn copying a stat into a component reads as wiring, right up until
    you look for the other end. JamEmitter.Team is the audit's example: captured
    at spawn, ignored by SignalGrid, so a jammer jams its own side's drones
    identically to the enemy's. That may be intended, but nothing said so."""
    findings = []
    for d in decls:
        if d.kind != "field" or not d.sf.is_sim:
            continue
        if d.owner in ("SimConstants", "UnitDef") or d.key in already:
            continue  # those have their own, sharper checks
        refs = index.refs(d.name, exclude=[d.span])
        if not refs:
            continue  # zero references at all: sim-public owns that finding
        reads, writes = 0, []
        for sf, lineno in refs:
            line = sf.lines[lineno - 1]
            for m in re.finditer(r"\b%s\b" % re.escape(d.name), line):
                # `X = v`, and `X[i] = v` - an array slot written is still a write.
                if re.match(r"\s*(?:\[[^\]]*\])?\s*=(?!=)", line[m.end():]):
                    writes.append((sf, lineno))
                else:
                    reads += 1
        if reads == 0 and writes:
            findings.append(Finding(
                "write-only", d.key, d.sf, d.line, "public field",
                "assigned in %d place(s) (%s) and read nowhere - the value goes in "
                "and never comes out" % (len(writes), where(writes)),
                "read it where it was meant to matter, delete it, or allowlist it with a reason"))
    return findings


def check_sim_public(files, index, decls, already):
    findings = []
    sim = [d for d in decls if d.sf.is_sim]
    dead_types = set()

    for d in [x for x in sim if x.kind == "type"]:
        refs = index.refs(d.name, exclude=[d.span])
        if not refs:
            dead_types.add(d.name)
            findings.append(Finding(
                "sim-public", d.key, d.sf, d.line, "public type",
                "no reference anywhere in src/ outside its own body",
                "give it a caller, delete it, or allowlist it with a reason"))
        elif all(sf.is_test for sf, _l in refs):
            findings.append(Finding(
                "test-only", d.key, d.sf, d.line, "public type",
                "referenced only from src/KZ.Tests (%d refs: %s)" % (len(refs), where(refs)),
                TEST_FIX))

    for d in [x for x in sim if x.kind in ("method", "property", "field")]:
        if d.owner in dead_types or d.key in already:
            continue  # its type is already reported, or a sharper check owns it
        refs = index.refs(d.name, exclude=[d.span])
        if not refs:
            findings.append(Finding(
                "sim-public", d.key, d.sf, d.line, "public " + d.kind,
                "no reference anywhere in src/ outside its own declaration",
                "give it a caller, delete it, or allowlist it with a reason"))
        elif all(sf.is_test for sf, _l in refs):
            findings.append(Finding(
                "test-only", d.key, d.sf, d.line, "public " + d.kind,
                "referenced only from src/KZ.Tests (%d refs: %s)" % (len(refs), where(refs)),
                TEST_FIX))
    return findings


# ---------------------------------------------------------------------------
# Allowlist.


def load_allowlist(path):
    """Returns (entries, errors). An allowlist entry without a reason is a bug
    being suppressed, so a malformed line fails the build on its own."""
    entries, errors = {}, []
    if not os.path.exists(path):
        return entries, errors
    rel = os.path.basename(path)
    with open(path, "r") as fh:
        for lineno, raw in enumerate(fh, start=1):
            line = raw.rstrip("\n")
            if not line.strip() or line.lstrip().startswith("#"):
                continue
            if "#" not in line:
                errors.append("%s:%d: no reason given. An allowlist entry without a reason is a "
                              "suppressed bug. Format: <category>  <Symbol>  # <tag>: <why>"
                              % (rel, lineno))
                continue
            head, reason = line.split("#", 1)
            reason = reason.strip()
            parts = head.split()
            if len(parts) != 2:
                errors.append("%s:%d: expected '<category>  <Symbol>  # <tag>: <why>', got %r"
                              % (rel, lineno, line.strip()))
                continue
            category, symbol = parts
            if category not in CATEGORIES:
                errors.append("%s:%d: unknown category %r (known: %s)"
                              % (rel, lineno, category, ", ".join(CATEGORY_ORDER)))
                continue
            m = re.match(r"^([a-z-]+)(?::[\w.\-/]+)?\s*[:-]\s*(.+)$", reason)
            if not m or m.group(1) not in REASON_TAGS:
                errors.append("%s:%d: the reason must open with a known tag - %s. Got: %r"
                              % (rel, lineno, ", ".join(sorted(REASON_TAGS)), reason))
                continue
            if len(m.group(2).strip()) < MIN_REASON_CHARS:
                errors.append("%s:%d: reason too short to judge (%r). Name what reads it, or will."
                              % (rel, lineno, reason))
                continue
            entries[(category, symbol)] = (reason, lineno)
    return entries, errors


def load_baseline(path):
    """The debt ledger: findings that already existed when the guard was added.

    No reasons here, deliberately. A baseline entry is not justified, only
    recorded - the justified ones live in the allowlist and say why. This file
    exists so that a backlog nobody caused does not fail everybody's build,
    and it is expected to shrink to nothing."""
    entries = set()
    if not os.path.exists(path):
        return entries
    with open(path, "r") as fh:
        for raw in fh:
            line = raw.split("#", 1)[0].strip()
            if not line:
                continue
            parts = line.split()
            if len(parts) == 2:
                entries.add((parts[0], parts[1]))
    return entries


def write_baseline(path, findings):
    lines = [
        "# KILL ZONE - the dead-symbol checker's debt ledger.",
        "#",
        "# Every finding here already existed when tools/check_dead_symbols.py was",
        "# added. They warn; they do not fail the build. Anything NOT in this file is",
        "# new, and fails.",
        "#",
        "# These entries carry no reason on purpose. A reason would make this an",
        "# allowlist, and none of these is justified - they are recorded, owned by a",
        "# task in docs/WIRING-SPEC.md, and listed with their consequences in",
        "# docs/DEAD-SYMBOLS.md.",
        "#",
        "# When you wire or delete one, delete its line here too. The checker warns",
        "# about lines that no longer match anything, so the ledger only goes down.",
        "# Regenerate with: python3 tools/check_dead_symbols.py --update-baseline",
        "",
    ]
    for f in sorted(findings, key=lambda f: (CATEGORY_ORDER.index(f.category), f.key)):
        lines.append("%-16s%s" % (f.category, f.key))
    with open(path, "w") as fh:
        fh.write("\n".join(lines) + "\n")


# ---------------------------------------------------------------------------
# Run.


class Result(object):
    def __init__(self, new, known, suppressed, stale, fixed, allow_errors, stats):
        self.new = new              # not in the baseline: these fail the build
        self.known = known          # recorded debt: these warn
        self.suppressed = suppressed
        self.stale = stale          # allowlist entries that match nothing
        self.fixed = fixed          # baseline entries that match nothing any more
        self.allow_errors = allow_errors
        self.stats = stats

    @property
    def kept(self):
        return self.new + self.known


def analyse(root, allowlist_path, baseline_path):
    files = load_sources(root)
    index = Index(files)
    decls = []
    for sf in files:
        decls.extend(parse_declarations(sf))

    raw_defs = ""
    for sf in files:
        if sf.base == "Defs.cs":
            with open(sf.path, "r") as fh:
                raw_defs = fh.read()

    findings = []
    findings += check_sim_events(files, index, decls)
    findings += check_component_flags(files, index, decls)
    findings += check_sim_constants(files, index, decls)
    findings += check_unitdef_fields(files, index, decls, raw_defs)
    findings += check_enum_values(files, index, decls)
    owned = set(f.key for f in findings)
    findings += check_write_only(files, index, decls, owned)
    owned |= set(f.key for f in findings)
    findings += check_sim_public(files, index, decls, owned)

    seen, unique = set(), []
    for f in sorted(findings, key=lambda f: f.sort_key()):
        if f.key not in seen:
            seen.add(f.key)
            unique.append(f)

    allow, allow_errors = load_allowlist(allowlist_path)
    kept, suppressed = [], []
    for f in unique:
        if (f.category, f.key) in allow:
            suppressed.append((f, allow[(f.category, f.key)][0]))
        else:
            kept.append(f)

    live = set(f.key for f in unique)
    stale = [(c, s, r) for (c, s), (r, _n) in sorted(allow.items()) if s not in live]

    baseline = load_baseline(baseline_path)
    new, known = [], []
    for f in kept:
        (known if (f.category, f.key) in baseline else new).append(f)
    still = set((f.category, f.key) for f in kept)
    fixed = sorted(e for e in baseline if e not in still)

    stats = {
        "files": len(files),
        "members": sum(1 for d in decls if d.sf.is_sim and d.kind in ("type", "method", "property", "field")),
        "enums": sum(1 for d in decls if d.sf.is_sim and d.kind == "enumvalue"),
        "units": count_defs(raw_defs),
    }
    return Result(new, known, suppressed, stale, fixed, allow_errors, stats)


def show(findings, indent="    "):
    for f in findings:
        print("%s%s:%d  %s" % (indent, f.sf.rel, f.line, f.key))
        print("%s    %s - %s" % (indent, f.what, f.detail))
        print("%s    fix: %s" % (indent, f.fix))


def report(res, show_all=False):
    s = res.stats
    print("dead-symbol check: %d source files, %d public members, %d enum values, %d unit defs"
          % (s["files"], s["members"], s["enums"], s["units"]))

    if res.new:
        by_cat = {}
        for f in res.new:
            by_cat.setdefault(f.category, []).append(f)
        print("")
        print("  FAIL  %d symbol(s) not in the baseline - something new is dead" % len(res.new))
        for cat in CATEGORY_ORDER:
            items = by_cat.get(cat)
            if not items:
                continue
            print("")
            print("    %s - %s" % (cat, CATEGORIES[cat]))
            show(items, indent="      ")

    if res.allow_errors:
        print("")
        print("  FAIL  the allowlist is malformed (%d). An entry without a reason a reviewer"
              % len(res.allow_errors))
        print("        can judge is a bug being suppressed, so it is rejected.")
        for e in res.allow_errors:
            print("    %s" % e)

    if res.known:
        by_cat = {}
        for f in res.known:
            by_cat.setdefault(f.category, []).append(f)
        print("")
        print("  WARN  %d symbol(s) of recorded debt - docs/DEAD-SYMBOLS.md, owned by WIRING-SPEC"
              % len(res.known))
        for cat in CATEGORY_ORDER:
            items = by_cat.get(cat)
            if not items:
                continue
            if show_all:
                print("")
                print("    %s (%d) - %s" % (cat, len(items), CATEGORIES[cat]))
                show(items, indent="      ")
            else:
                print("    %-16s %3d   %s" % (cat, len(items), CATEGORIES[cat]))
        if not show_all:
            print("        (--all lists them; --list is the machine-readable form)")

    if res.fixed:
        print("")
        print("  WARN  %d baseline entr(y/ies) no longer found. Wired or deleted - good." % len(res.fixed))
        print("        Delete the line from tools/dead-symbols-baseline.txt so the ledger goes down.")
        for cat, sym in res.fixed:
            print("    %-16s%s" % (cat, sym))

    if res.stale:
        print("")
        print("  WARN  %d allowlist entr(y/ies) match nothing. Drop them." % len(res.stale))
        for cat, sym, _reason in res.stale:
            print("    %-16s%s" % (cat, sym))

    print("")
    print("  %d new, %d known, %d allowlisted with a reason"
          % (len(res.new) + len(res.allow_errors), len(res.known), len(res.suppressed)))

    if res.new or res.allow_errors:
        print("")
        print("  A dead symbol is not a tidiness problem. Three shipped systems did nothing at")
        print("  runtime for exactly this reason - terrain occlusion, the track hold, and the")
        print("  gun mount's magazine. Wire it, delete it, or put it in")
        print("  tools/dead-symbols-allow.txt with a reason a reviewer can judge in one line.")
        return 1
    return 0


def list_mode(res):
    new = set(id(f) for f in res.new)
    for f in sorted(res.kept, key=lambda f: (CATEGORY_ORDER.index(f.category), f.sort_key())):
        print("%s\t%s\t%s\t%s:%d\t%s\t%s"
              % ("new" if id(f) in new else "known", f.category, f.key,
                 f.sf.rel, f.line, f.what, f.detail))


# ---------------------------------------------------------------------------
# Self-test.
#
# The checker's own regression corpus lives in tools/testdata/dead-symbols and
# is a miniature of this repository: the three shipped-inert failures in the
# shape they actually had, one instance of every other category, and a live
# counterpart of each that must stay clear.
#
# It is a fixture rather than an assertion about src/, deliberately. Asserting
# "Terrain.BlocksGroundSight is still dead" against the real tree would turn
# this self-test red the day somebody fixes it, which is the fastest known route
# to a checker being switched off.

EXPECTED = {
    # the three that shipped inert, in their original shapes
    ("sim-public", "Terrain.BlocksGroundSight"),
    ("sim-constant", "SimConstants.TrackHoldTicks"),
    ("unitdef-single", "UnitDef.TraverseDegreesPerSecond"),
    ("unitdef-single", "UnitDef.AmmoCapacity"),
    ("unitdef-single", "UnitDef.ReloadSeconds"),
    # one of each remaining category
    ("sim-event", "SimEventKind.SalvageCollected"),
    ("component-flag", "ComponentMask.Producer"),
    ("unitdef-field", "UnitDef.Tier"),
    ("unitdef-default", "UnitDef.BlackPolicy"),
    ("enum-value", "CrewState.Reserved"),
    ("write-only", "MoverState.RadiusClass"),
    ("test-only", "Territory.SetVerticalBorder"),
    ("test-only", "SimConstants.SceneMatchTicks"),
    ("test-only", "EventRing.CountOf"),
}


def selftest():
    res = analyse(FIXTURE, os.path.join(FIXTURE, "allow.txt"), os.path.join(FIXTURE, "baseline.txt"))
    got = set((f.category, f.key) for f in res.kept)
    ok = True

    print("self-test corpus: %s" % os.path.relpath(FIXTURE, ROOT))
    for cat, key in sorted(EXPECTED, key=lambda x: (CATEGORY_ORDER.index(x[0]), x[1])):
        hit = (cat, key) in got
        ok = ok and hit
        print("  %s must be caught   %-15s %s" % ("PASS" if hit else "FAIL", cat, key))

    extra = sorted(got - EXPECTED)
    for cat, key in extra:
        ok = False
        print("  FAIL false positive  %-15s %s" % (cat, key))
    if not extra:
        print("  PASS no false positives: every live symbol in the corpus stayed clear")

    ratchet = ([f.key for f in res.known] == ["SimConstants.TrackHoldTicks"]
               and res.fixed == [("sim-public", "World.GoneAway")])
    print("  %s the ratchet: a baselined finding warns, a vanished one asks to be removed"
          % ("PASS" if ratchet else "FAIL"))
    ok = ok and ratchet

    supp = sorted((f.key for f, _r in res.suppressed))
    print("  %s allowlist suppressed %s"
          % ("PASS" if supp == ["World.DebugDumpState"] else "FAIL", supp))
    ok = ok and supp == ["World.DebugDumpState"]

    if res.allow_errors:
        ok = False
        for e in res.allow_errors:
            print("  FAIL allowlist: %s" % e)

    bad = analyse(FIXTURE, os.path.join(FIXTURE, "allow-malformed.txt"),
                  os.path.join(FIXTURE, "baseline.txt"))
    caught = len(bad.allow_errors)
    print("  %s allowlist entries without a usable reason are rejected (%d of 4)"
          % ("PASS" if caught == 4 else "FAIL", caught))
    ok = ok and caught == 4

    print("self-test: %s" % ("PASS" if ok else "FAIL"))
    return 0 if ok else 1


def main(argv):
    root = ROOT
    if "--root" in argv:
        root = os.path.abspath(argv[argv.index("--root") + 1])
    allowlist = os.path.join(root, "tools", "dead-symbols-allow.txt")
    baseline = os.path.join(root, "tools", "dead-symbols-baseline.txt")
    if "--allowlist" in argv:
        allowlist = os.path.abspath(argv[argv.index("--allowlist") + 1])
    if "--baseline" in argv:
        baseline = os.path.abspath(argv[argv.index("--baseline") + 1])

    if "--selftest" in argv:
        return selftest()

    res = analyse(root, allowlist, baseline)

    if "--update-baseline" in argv:
        write_baseline(baseline, res.kept)
        print("wrote %d entries to %s" % (len(res.kept), os.path.relpath(baseline, root)))
        return 0
    if "--list" in argv:
        list_mode(res)
        return 0

    status = report(res, show_all="--all" in argv)
    return 0 if "--warn-only" in argv else status


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
