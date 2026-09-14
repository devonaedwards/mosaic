"""
KILL ZONE - a real-time strategy video game. This is content tooling that
generates a lookup table for the game's detection model. Nothing here is
operational guidance; it feeds hit points and cooldown timers.

Shared vocabulary for the propagation models.

The one idea worth explaining is Coeff. Every constant in these models arrives
with a different pedigree - some are published standards, some are quoted from a
textbook out of a researcher's memory, some are a designer's guess that makes the
model reproduce a known field anchor. Those are not the same kind of number and
the difference must survive into the generated table, because the whole argument
for building this tool is that it moves the conversation from "is 130 the right
acoustic range" to "is this the right absorption coefficient" - and that only
works if you can see which coefficients are actually in question.

So a Coeff carries its provenance, the generator reports the mix, and the
flagged ones are listed by name in the table header. See docs/PHYSICS-TOOL.md.
"""

from dataclasses import dataclass, field


# Provenance, worst to best.
GUESS = "G"      # calibrated to reproduce an anchor; no independent basis
ESTIMATE = "E"   # a researcher's reasoned estimate, labelled as such in the docs
RECALLED = "T"   # textbook value quoted from memory - right shape, needs checking
PUBLISHED = "P"  # a published standard or a measured figure with a source

PROVENANCE_NAMES = {
    GUESS: "calibrated guess",
    ESTIMATE: "reasoned estimate",
    RECALLED: "textbook, recalled - VERIFY",
    PUBLISHED: "published or measured",
}


@dataclass(frozen=True)
class Coeff:
    """A number with a pedigree."""
    value: float
    provenance: str
    what: str
    source: str = ""

    def __float__(self) -> float:
        return self.value


_REGISTRY: list = []


def coeff(value: float, provenance: str, what: str, source: str = "") -> float:
    """Declare a coefficient, record it, and hand back the bare number.

    Returning a float rather than the Coeff keeps the physics readable - the
    equations below look like equations, not like bookkeeping - while the
    registry still knows what went into them.
    """
    _REGISTRY.append(Coeff(value, provenance, what, source))
    return value


def registry() -> list:
    return list(_REGISTRY)


def provenance_summary() -> dict:
    counts = {GUESS: 0, ESTIMATE: 0, RECALLED: 0, PUBLISHED: 0}
    for c in _REGISTRY:
        counts[c.provenance] = counts.get(c.provenance, 0) + 1
    return counts


# ---- the game's units -------------------------------------------------------

# The game compresses real distance. A figure of 100 map metres stands in for
# something on the order of a kilometre of real ground. Everything in these
# models is computed in real metres and converted once, at the end.
MAP_COMPRESSION = 12.0


def real_to_map(metres: float) -> float:
    return metres / MAP_COMPRESSION


def map_to_real(metres: float) -> float:
    return metres * MAP_COMPRESSION


# ---- the quantisation the interface can actually draw -----------------------

# Deliberately coarse. A detection range that varies continuously with humidity
# is a detection range no player can plan around, and the game's own design rules
# say a mechanic that needs a submenu gets cut. The table is the place that
# decision gets made once, honestly, rather than leaking into the simulation.

DAY_PHASES = ["Day", "Dusk", "Night", "Dawn"]
WEATHER = ["Clear", "Haze", "Rain", "Fog"]
LAYERS = ["Ground", "Low", "High"]
