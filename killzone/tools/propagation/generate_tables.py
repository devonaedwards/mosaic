#!/usr/bin/env python3
"""
KILL ZONE - a real-time strategy video game. Content tooling, not operational
guidance.

Derives the game's acoustic signature column from the propagation model and
reports where it disagrees with what is currently shipped.

This does not write to the simulation. It prints a recommendation and a
provenance report, because the point of the exercise is to make the numbers
*arguable*, and an argument needs two sides in view at once. Wiring it to emit
content directly is the next step and should wait until the flagged coefficients
below have been checked.

Run:  python3 tools/propagation/generate_tables.py
"""

import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import acoustic
from common import (real_to_map, provenance_summary, registry,
                    PROVENANCE_NAMES, GUESS, RECALLED, ESTIMATE, PUBLISHED)

# A gun mount carries a small array on a noisy vehicle. Not a dedicated mast.
GUN_MOUNT_MICS = 8

# What the game ships today, after the decibel/linear correction.
IN_GAME = {
    "small_quad": ("FPV Team / Scout Quad", 12),
    "heavy_multirotor": ("Night Bomber", 35),
    "fixed_wing_electric": ("Recon Wing", 18),
    "two_stroke": ("Loitering Munition", 55),
    "piston_heavy": ("Heavy Strike Drone", 90),
    "turbojet": ("Jet Strike Drone", 95),
    "tank": ("Main Tank", 80),
}

IN_GAME_BASE_MAP_METRES = 200


def main() -> None:
    ranges = {}
    for key in IN_GAME:
        r = acoustic.detection_range_metres(key, GUN_MOUNT_MICS, 0.0, night=False)
        ranges[key] = real_to_map(r)

    loudest = max(ranges.values())

    print("ACOUSTIC SIGNATURE, DERIVED")
    print()
    print("The game reads acoustic signature as a fraction of a sensor's maximum")
    print("reach, so the column is just each airframe's modelled range scaled to")
    print("the loudest thing on the map.")
    print()
    print("%-22s %9s %9s %8s %8s" % ("unit", "real m", "map m", "derived", "in game"))
    print("-" * 62)
    for key, (name, shipped) in sorted(IN_GAME.items(), key=lambda kv: -ranges[kv[0]]):
        derived = round(100.0 * ranges[key] / loudest)
        flag = "  <-- " if abs(derived - shipped) > max(6, shipped * 0.4) else ""
        print("%-22s %9.0f %9.1f %8d %8d%s"
              % (name[:22], ranges[key] * 12, ranges[key], derived, shipped, flag))

    print()
    print("Base reach implied for a gun mount: %.0f map metres (shipped: %d)."
          % (loudest, IN_GAME_BASE_MAP_METRES))
    print("Spread, loudest to quietest: %.0f to 1 (shipped table spans %.0f to 1)."
          % (loudest / min(ranges.values()),
             max(s for _, s in IN_GAME.values()) / min(s for _, s in IN_GAME.values())))

    print()
    print("ANCHORS - what the model reproduces that it was not fitted to")
    print()
    checks = [
        ("single mic vs small quad", acoustic.detection_range_metres("small_quad", 1, 0, False),
         (35, 60), "measured, Drone Warfare C-UAS 101 / Acta Acustica"),
        ("128-mic array vs small quad", acoustic.detection_range_metres("small_quad", 128, 0, False),
         (300, 500), "vendor, Squarehead Discovair G2  [FITTED]"),
        ("mast vs piston heavy strike", acoustic.detection_range_metres("piston_heavy", 128, 0, False),
         (3000, 5000), "vendor/press, Zvook  [FITTED]"),
        ("small array vs small quad", acoustic.detection_range_metres("small_quad", 8, 0, False),
         (50, 200), "institute spec, Fraunhofer IDMT"),
    ]
    for what, got, (lo, hi), source in checks:
        ok = "ok " if lo <= got <= hi else "OFF"
        print("  %s  %-30s %6.0f m   expected %d-%d   %s" % (ok, what, got, lo, hi, source))

    print()
    print("PROVENANCE")
    print()
    counts = provenance_summary()
    for k in (PUBLISHED, RECALLED, ESTIMATE, GUESS):
        print("  %-26s %3d" % (PROVENANCE_NAMES[k], counts.get(k, 0)))
    print()
    print("  Coefficients that are not published, by name - these are what to")
    print("  argue about, and what to check first if the table looks wrong:")
    seen = set()
    for c in registry():
        if c.provenance in (GUESS, RECALLED) and c.what not in seen:
            seen.add(c.what)
            print("    [%s] %s" % (c.provenance, c.what))
            if c.source:
                print("         %s" % c.source)


if __name__ == "__main__":
    main()
