"""
KILL ZONE - a real-time strategy video game. Content tooling, not operational
guidance.

Acoustic detection: how far away a microphone array hears an airframe.

Implements the model in docs/research/acoustic.md section 5a. The headline
result that model produces, and the reason it was worth building, is that
detection range is set by *frequency*, not by loudness. A small quadcopter and a
two-stroke engine are within a few decibels of each other at one metre. At two
kilometres the quad is gone and the engine is not, because the quad's energy sits
at 2-6 kHz where air absorbs 33-117 dB per kilometre, and the engine's sits at
80-250 Hz where it absorbs less than one.

That single fact is what the game's old signature table got wrong, and it is not
something you can patch by moving a number - it needs the bands.
"""

import math

from common import (coeff, real_to_map, PUBLISHED, ESTIMATE, GUESS)

# Octave band centres, Hz.
BANDS = [63, 125, 250, 500, 1000, 2000, 4000, 8000]

# ISO 9613-1 atmospheric absorption at 15 C / 70% RH, dB/km. This is the single
# most important row in the whole file and it is a published standard.
ALPHA = [
    coeff(0.1, PUBLISHED, "air absorption 63 Hz, dB/km", "ISO 9613-1"),
    coeff(0.4, PUBLISHED, "air absorption 125 Hz, dB/km", "ISO 9613-1"),
    coeff(1.0, PUBLISHED, "air absorption 250 Hz, dB/km", "ISO 9613-1"),
    coeff(1.9, PUBLISHED, "air absorption 500 Hz, dB/km", "ISO 9613-1"),
    coeff(3.7, PUBLISHED, "air absorption 1 kHz, dB/km", "ISO 9613-1"),
    coeff(9.7, PUBLISHED, "air absorption 2 kHz, dB/km", "ISO 9613-1"),
    coeff(32.8, PUBLISHED, "air absorption 4 kHz, dB/km", "ISO 9613-1"),
    coeff(117.0, PUBLISHED, "air absorption 8 kHz, dB/km", "ISO 9613-1"),
]

# Ambient noise floor per band, unweighted dB. Two profiles: an active daytime
# position and a calm night. The swing between them is the largest environmental
# effect in this channel and the game modelled none of it.
AMBIENT_DAY = [58, 55, 52, 48, 44, 40, 36, 32]
AMBIENT_NIGHT = [46, 43, 40, 36, 32, 28, 25, 22]
coeff(12.0, ESTIMATE, "day-to-night ambient fall, dB",
      "acoustic.md sec 4.7 gives 10-15 dB; 12 taken as the midpoint")


def _spectrum(overall_db: float, centre_hz: float, spread_octaves: float = 1.2):
    """Spread an overall level into octave bands around a dominant frequency.

    A crude shape - a rounded triangle in log-frequency - but the thing that
    matters for range is which bands carry the energy, and that this is honest
    about being a shape rather than a measurement. Nobody publishes band levels
    for any of these airframes.
    """
    out = []
    for f in BANDS:
        octaves_away = abs(math.log2(f / centre_hz))
        rolloff = coeff(9.0, ESTIMATE, "spectral rolloff, dB per octave from peak",
                        "acoustic.md sec 3a describes dominant bands, not shapes") \
            if f == BANDS[0] else 9.0
        level = overall_db - rolloff * max(0.0, octaves_away - spread_octaves / 2)
        out.append(level)
    return out


# Airframes. Overall level is unweighted at 1 m; the published consumer figures
# are A-weighted and A-weighting discounts exactly the low frequencies that
# survive distance, so combustion rows are corrected upward per acoustic.md.
AIRFRAMES = {
    "small_quad":        (_spectrum(86, 3000), "small electric quad, 7-10 inch"),
    "heavy_multirotor":  (_spectrum(100, 700), "large electric multirotor, 15 kg+"),
    "fixed_wing_electric": (_spectrum(82, 1500), "electric fixed-wing recon"),
    "two_stroke":        (_spectrum(108, 200), "two-stroke fixed-wing, Orlan class"),
    "piston_heavy":      (_spectrum(120, 150), "piston heavy strike, Shahed class"),
    "turbojet":          (_spectrum(130, 800), "small turbojet strike"),
    "tank":              (_spectrum(118, 90), "main battle tank"),
}
coeff(83.0, PUBLISHED, "consumer multirotor SPL at 1 m, dBA",
      "DJI Air 3S 81 dB, Mavic 4 Pro 83 dB - measured analogues")
coeff(16.0, PUBLISHED, "A-weighting discount at 125 Hz, dB",
      "definition of A-weighting; why combustion rows are corrected upward")
for _name in ("two_stroke", "piston_heavy", "turbojet", "tank", "heavy_multirotor",
              "fixed_wing_electric"):
    coeff(0.0, ESTIMATE, "source level for %s" % _name,
          "acoustic.md sec 3a - no published source level exists for any military airframe")


# The calibration term, and the most important number in this file to be honest
# about. Every coefficient above is free-field physics, and free-field physics
# over-predicts what anyone actually fields - by about a factor of two here, and
# by 20-28 dB in the radar and RF equivalents (see radar-rf.md). Terrain, real
# ambient, imperfect beamforming and the difference between "a band clears the
# floor" and "an operator gets a usable track" all live in this one number.
#
# It is fitted, not derived. Eight decibels reproduces BOTH published anchors at
# once - a 128-microphone array at 370 m against a small quad, where the vendor
# claims 300-500, and 4,170 m against a piston heavy strike drone, where the
# vendor claims 3-5 km. Two independent anchors from one constant is the most
# this model can offer as evidence that its shape is right.
#
# Tune this before touching anything above it.
ENVIRONMENT_PENALTY_DB = coeff(
    8.0, GUESS, "flat SNR penalty, free-field to fielded, dB",
    "fitted to Squarehead 128-mic quad range and Zvook mast Shahed range")


def detection_range_metres(airframe: str, n_mics: int, altitude_m: float,
                           night: bool, wind_ms: float = 0.0,
                           inversion: bool = False) -> float:
    """Real metres at which this array holds this airframe.

    Walks outward and returns the last range at which any band clears the noise
    floor. Slow and obvious on purpose - it runs once, offline.
    """
    src = AIRFRAMES[airframe][0]
    ambient = AMBIENT_NIGHT if night else AMBIENT_DAY

    # Coherent array gain against uncorrelated noise. Published, and it is what
    # makes a 128-microphone tower a different instrument from a single mic.
    gain = coeff(10.0, PUBLISHED, "array gain multiplier in 10*log10(N)",
                 "standard beamforming result") * math.log10(max(n_mics, 1))
    gain -= ENVIRONMENT_PENALTY_DB

    best = 0.0
    for step in range(1, 1200):
        ground = step * 10.0
        r = math.hypot(ground, altitude_m)
        theta = math.degrees(math.atan2(altitude_m, max(ground, 1e-6)))

        audible = False
        for i, f in enumerate(BANDS):
            level = src[i] - 20 * math.log10(max(r, 1.0)) - ALPHA[i] * r / 1000.0

            # Ground effect. Grazing paths lose a chunk of the mid band to
            # interference with the ground reflection; steep ones do not.
            if theta < 5:
                level -= 10.0 if 200 <= f <= 600 else 4.0

            # Refraction, grazing paths only. A dawn inversion bends rays back
            # down; wind does the same downwind and the opposite upwind.
            if theta < 20:
                if inversion:
                    level += 7.0
                elif wind_ms > 2:
                    level += 3.0

            floor = ambient[i]
            if wind_ms > 3 and f <= 250:
                floor += 6.0 * (wind_ms - 3)   # wind pseudo-noise on the mics

            if level - floor + gain >= 0:
                audible = True
                break

        if audible:
            best = r
        elif best > 0:
            break
    return best


# Those ground, refraction and wind constants are the researcher's estimates,
# labelled as such in acoustic.md. Right sign, roughly right size, not measured.
for _what, _v in [("ground effect, grazing, mid band, dB", -10.0),
                  ("ground effect, grazing, other bands, dB", -4.0),
                  ("dawn inversion gain, dB", 7.0),
                  ("downwind refraction gain, dB", 3.0),
                  ("wind pseudo-noise slope, dB per m/s over 3", 6.0)]:
    coeff(_v, ESTIMATE, _what, "acoustic.md sec 5a - explicitly the author's estimate")


def map_range(airframe: str, n_mics: int, altitude_m: float, night: bool) -> float:
    return real_to_map(detection_range_metres(airframe, n_mics, altitude_m, night))


if __name__ == "__main__":
    print("acoustic detection range, real metres (map metres in brackets)")
    print()
    print("%-24s %10s %10s %10s %10s" % ("airframe", "1 mic", "8 mics", "128 mics", "map/128"))
    for key, (_, label) in AIRFRAMES.items():
        row = [detection_range_metres(key, n, 0.0, False) for n in (1, 8, 128)]
        print("%-24s %10.0f %10.0f %10.0f %10.0f"
              % (label[:24], row[0], row[1], row[2], real_to_map(row[2])))
