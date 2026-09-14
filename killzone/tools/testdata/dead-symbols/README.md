# The dead-symbol checker's regression corpus

Not a game. This is a miniature of `src/` that reproduces, in the shape it
actually had, every failure `tools/check_dead_symbols.py` exists to catch -
including the three that shipped inert and started all of this:

- `Terrain.BlocksGroundSight`, defined with no callers (FINDINGS 29)
- `SimConstants.TrackHoldTicks`, a constant nothing reads (FINDINGS 30, audit F2)
- `UnitDef.TraverseDegreesPerSecond` and `UnitDef.AmmoCapacity`, carried by one
  unit while the unit that needed them got the default and the reading code
  returned early (audit F4)

Beside each dead symbol sits a live counterpart that must stay clear, because
half the value of this checker is the findings it does *not* produce.

It is a fixture rather than an assertion about the real `src/` on purpose.
"`Terrain.BlocksGroundSight` is still dead" would go red the day somebody fixes
it, and a self-test that fails on success is the fastest known route to a
checker being switched off.

`python3 tools/check_dead_symbols.py --selftest` runs it. The expected findings
are the `EXPECTED` set in the checker; anything else the corpus produces is a
false positive and fails.
