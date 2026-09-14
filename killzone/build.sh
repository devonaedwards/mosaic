#!/usr/bin/env bash
# KILL ZONE - a real-time strategy video game.
# Builds the simulation and runs its tests, or a headless match.
#
#   ./build.sh              compile and run the test suite
#   ./build.sh headless     compile and run a scripted match
#   ./build.sh balance      run balance experiments and print the results
#   ./build.sh build        compile only
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
  test|*)
    compile
    $RUNNER "$OUT/KZ.Tests.exe"
    ;;
esac
