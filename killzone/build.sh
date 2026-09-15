#!/usr/bin/env bash
# KILL ZONE - a real-time strategy video game.
# Builds the simulation and runs its tests, or a headless match.
#
#   ./build.sh              compile, test, check dead symbols, check experiment drift
#   ./build.sh headless     compile and run a scripted match
#   ./build.sh balance      run balance experiments and print the results
#   ./build.sh play         compile and serve the playable interface on :8080
#   ./build.sh build        compile only
#   ./build.sh deadsymbols  run only the dead-symbol guard
#   ./build.sh driftcheck   run only the experiment-drift guard
#
# Works with either the .NET SDK or Mono. The simulation deliberately has no
# engine dependency, so it builds anywhere a C# compiler exists.

set -euo pipefail
cd "$(dirname "$0")"

OUT="build"
mkdir -p "$OUT"

find_compiler() {
  if command -v csc >/dev/null 2>&1; then echo "csc"; return; fi
  if command -v dotnet >/dev/null 2>&1 && dotnet --version >/dev/null 2>&1; then echo "dotnet"; return; fi
  if command -v mcs >/dev/null 2>&1; then echo "mcs"; return; fi
  echo "none"
}

COMPILER="$(find_compiler)"
if [ "$COMPILER" = "none" ]; then
  echo "No C# compiler found. Install the .NET SDK (https://dot.net) or Mono (mono-devel)." >&2
  exit 1
fi

# Mono's compiler defaults to an old language version; ask for the newest it has.
LANGFLAG=""
if [ "$COMPILER" = "mcs" ]; then LANGFLAG="-langversion:latest"; fi

RUNNER=""
if [ "$COMPILER" = "mcs" ]; then RUNNER="mono"; fi

# The dead-symbol guard. Three systems shipped doing nothing at runtime -
# terrain occlusion, the track hold, the gun mount's magazine - and every one
# was invisible to reading and one grep away from obvious. This is that grep.
#
# It runs after the tests, deliberately: a guard that reports before the test
# results would hide them, and this one is the least urgent thing in the build.
# It fails only on a symbol that is not already in tools/dead-symbols-baseline.txt,
# so the existing backlog warns and a newly dead symbol stops the build.
#
# Python is not a build dependency of the game - the simulation still compiles
# with mcs or dotnet and nothing else. If there is no python3 here, the build
# carries on and says loudly that it went unchecked, because a missing
# interpreter is not a reason to block a game build. It is a reason to notice.
check_dead_symbols() {
  if ! command -v python3 >/dev/null 2>&1; then
    echo ""
    echo "!! dead-symbol guard SKIPPED: no python3 on this machine."
    echo "!! Nothing checked that this build's public surface is actually reachable."
    return 0
  fi
  echo ""
  # The guard's own regression corpus first. A checker that has quietly stopped
  # catching things is worse than no checker, because it reads as evidence.
  if ! python3 tools/check_dead_symbols.py --selftest >/dev/null; then
    echo "!! the dead-symbol guard failed its own self-test - it is not to be trusted"
    python3 tools/check_dead_symbols.py --selftest
    return 1
  fi
  python3 tools/check_dead_symbols.py "$@"
}

# The experiment-drift guard, check_dead_symbols.py's sibling. It re-runs every
# balance experiment (compiled above), compares each one's output against the
# baseline recorded in tools/experiment-drift-ledger.txt, and reports:
#
#   drift    - an experiment's output no longer matches its recorded baseline.
#              Fails the build. Not a sign anything is wrong - the simulation
#              is supposed to change - but it must never pass unacknowledged,
#              because an unacknowledged one is exactly how FINDINGS 15's table
#              went stale (recorded 10/72/98, tree gave 37/92/100, nobody
#              noticed). Fix: read the diff it prints, amend whatever finding
#              depended on the old numbers, then run --update-baseline.
#   inertia  - an experiment that has not moved across enough of its siblings'
#              changes to trust as live. Warns only - it is a heuristic, not a
#              proof, and FINDINGS.md 32's own examples (Stacking, Vertical,
#              Decoy Escort) are already flagged here and are not this
#              checker's to fix.
#
# It runs the compiled KZ.Balance.exe once per experiment (real trials, real
# ticks), so it costs roughly as long as `./build.sh balance` does - slower
# than the dead-symbol guard, and worth it for the same reason that one is.
check_experiment_drift() {
  if ! command -v python3 >/dev/null 2>&1; then
    echo ""
    echo "!! experiment-drift guard SKIPPED: no python3 on this machine."
    return 0
  fi
  echo ""
  if ! python3 tools/check_experiment_drift.py --selftest >/dev/null; then
    echo "!! the experiment-drift guard failed its own self-test - it is not to be trusted"
    python3 tools/check_experiment_drift.py --selftest
    return 1
  fi
  EXE="$OUT/KZ.Balance.exe"
  if [ -n "$RUNNER" ]; then EXE="$RUNNER $EXE"; fi
  python3 tools/check_experiment_drift.py --exe "$EXE" "$@"
}

compile() {
  echo "building with $COMPILER"

  $COMPILER -target:library $LANGFLAG -out:"$OUT/KZ.Sim.dll" \
    $(find src/KZ.Sim -name '*.cs')

  $COMPILER -target:exe $LANGFLAG -r:"$OUT/KZ.Sim.dll" -out:"$OUT/KZ.Tests.exe" \
    $(find src/KZ.Tests -name '*.cs')

  $COMPILER -target:exe $LANGFLAG -r:"$OUT/KZ.Sim.dll" -out:"$OUT/KZ.Headless.exe" \
    $(find src/KZ.Headless -name '*.cs')

  $COMPILER -target:exe $LANGFLAG -r:"$OUT/KZ.Sim.dll" -out:"$OUT/KZ.Balance.exe" \
    $(find src/KZ.Balance -name '*.cs')

  # The playable interface. Its web assets are not compiled - they are served
  # from src/KZ.Play/web at run time, so the page can be edited without a build.
  $COMPILER -target:exe $LANGFLAG -r:"$OUT/KZ.Sim.dll" -out:"$OUT/KZ.Play.exe" \
    $(find src/KZ.Play -name '*.cs')
}

case "${1:-test}" in
  build)
    compile
    echo "built into $OUT/"
    ;;
  headless)
    compile
    shift || true
    $RUNNER "$OUT/KZ.Headless.exe" "$@"
    ;;
  balance)
    compile
    shift || true
    $RUNNER "$OUT/KZ.Balance.exe" "$@"
    ;;
  play)
    compile
    shift || true
    $RUNNER "$OUT/KZ.Play.exe" "$@"
    ;;
  deadsymbols)
    shift || true
    check_dead_symbols "$@"
    ;;
  driftcheck)
    compile
    shift || true
    check_experiment_drift "$@"
    ;;
  test|*)
    compile
    $RUNNER "$OUT/KZ.Tests.exe"
    check_dead_symbols
    check_experiment_drift
    ;;
esac
