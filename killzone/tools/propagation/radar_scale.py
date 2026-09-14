#!/usr/bin/env python3
"""
KILL ZONE - a real-time strategy video game. This generates a lookup table for
the game's detection model; it is content tooling, not anything operational.

Generates the radar reach table baked into KZ.Sim/Core/World.cs.

Why a table at all. The simulation runs in fixed-point integers so that an iPad
and a PC agree bit for bit over tens of thousands of ticks, and the radar law
below is a power of ten. Implementing pow() in deterministic fixed point means
building a table anyway, so the choice is not "formula versus table" but "table
computed at runtime" versus "table computed once, here, in ordinary floating
point, where it can be read and checked". See docs/PHYSICS-TOOL.md.

The law. The game's 0-100 radar signature is read as decibels of cross-section:

    S = 2 * RCS_dBsm + 80        so S=80 is one square metre, 2 points = 1 dB

and radar range goes as the fourth root of cross-section, so

    reach = base_at_one_square_metre * 10 ** ((S - 80) / 80)

The 80 in the exponent is not a coincidence with the 80 offset: 40 dB of
cross-section is a factor of ten in range, and 40 dB is 80 signature points.

Run:  python3 tools/propagation/radar_scale.py
"""

FRAC_BITS = 32


def reach_multiplier(signature: int) -> float:
    return 10.0 ** ((signature - 80) / 80.0)


def main() -> None:
    rows = []
    for base in range(0, 101, 5):
        chunk = [str(round(reach_multiplier(s) * (1 << FRAC_BITS)))
                 for s in range(base, min(base + 5, 101))]
        rows.append("            " + ", ".join(chunk) + ",")
    print("\n".join(rows).rstrip(","))

    print()
    for s, what in [(22, "small quad"), (40, "fixed-wing recon"),
                    (50, "turbojet strike"), (52, "heavy strike drone"),
                    (80, "one square metre"), (92, "decoy with reflector")]:
        print(f"  S={s:3d}  {reach_multiplier(s):6.3f}x   RCS {10 ** ((s - 80) / 20.0):8.4f} m2   {what}")
    print(f"\n  decoy/strike reach ratio: {reach_multiplier(92) / reach_multiplier(52):.2f}x")


if __name__ == "__main__":
    main()
