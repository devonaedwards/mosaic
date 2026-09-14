# Design documents

KILL ZONE is a real-time strategy video game for iPad and iPhone, with PC as a
second platform. These are its design specifications. They are working
documents: the numbers in them exist to be argued about and changed, and several
already have been.

| Document | What it covers |
|---|---|
| `brief.md` | The one-page project brief everything else was written against. |
| `spec-gameplay.md` | The game: loops, economy, the control-link layer, crews, the unit roster, the two factions, tech tiers, campaign shape. |
| `spec-technical.md` | Engine choice, the deterministic simulation, the link subsystem, touch input, multiplayer, tooling, a twenty-four month plan. |
| `spec-ui.md` | Screen anatomy for iPad, iPhone and desktop; the hangar and sortie flow; how the link layer is drawn; onboarding. |
| `spec-art.md` | Visual direction, the readability system, mobile performance budgets, and the asset pipeline. |
| `spec-futures-2027-2028.md` | Where the subject matter is heading, and how that becomes a fourth tech tier and a set of campaign eras. |
| `FINDINGS.md` | **What implementing the specs revealed about them.** Arithmetic that did not work, rules that were never enforced, and one balance problem still open. |

## Read these two first

If you are picking this up cold, `spec-gameplay.md` sections 1 and 5 explain what
the game is and why its central system is unusual. `FINDINGS.md` is short and
tells you which parts of the rest are already known to be wrong.

## A note on the specs and the code

Where the code disagrees with a document, the code is usually right and the
document has not caught up - but every such divergence should be written down in
`FINDINGS.md` rather than left for the next person to rediscover. Two of the
entries there were found only by running a match and watching what happened,
which is the argument for the headless runner existing at all.
