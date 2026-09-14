# Project brief: drone-era RTS (working title "Kill Zone")

## What we are making
A real-time strategy game in the Command & Conquer lineage (base building, sidebar build queue, resource economy, readable top-down combat, 20–40 minute matches, campaign plus skirmish/multiplayer), set on a fictionalised Eastern European front in 2027–2028, where the operational reality of the 2024–2026 drone war is the core loop rather than a garnish.

The research base is `kill-zone-bestiary.html` in this directory. Read it fully before doing anything. It contains every unit class fighting today, the control link each depends on, what kills it, cost/range/payload figures, the counters table, who runs it, and a draft RTS roster. Treat its numbers as balancing starting points, not spec.

## Design thesis (from the research)
1. **The control link is the unit's real stat block.** Radio / mesh / fiber / satellite / autonomy each buy jam resistance and pay in weight, speed, price, range or judgement. The counter always sits one column behind the link. That escalation ladder is the game.
2. **The front is a band, not a line.** A 10–25 km kill zone on each side where anything moving in daylight dies. Logistics, rotation, casevac and assault all reorganise around that.
3. **Nothing is targetable until seen.** Recon is the gate on every fire mission. Killing the recon layer turns off the enemy's lights.
4. **Pilots are the bottleneck, not airframes.** Airframes are cheap and plentiful; crews, radar cues, relays and terminals are scarce. Autonomy trades a pilot slot for target-selection error.
5. **Logistics is the real front.** Trucks die inside the kill zone; robots, motorcycles, night bombers and heavy drones carry the last kilometres.
6. **Tanks are rare, precious and situational.** Cages, EW cover, night, short dashes.
7. **Points-for-kills economies already exist** (Brave1 marketplace). Verified kills buy better drones. That is a resource model nobody has shipped.

## Hard constraints
- It must still *feel* like C&C: fast to read, click-to-command, a sidebar, a build order, a satisfying "my army vs your army" fantasy, a base to defend. Not a wargame with hex-level fidelity, not Broken Arrow's deck system. Modernise the genre, don't abandon it.
- Real-time. Match length 20–40 min for skirmish. Campaign missions can run longer.
- **Platform (revised):** iOS first, iPad as the reference device and iPhone as a compact layout; PC (Steam) as the second platform from the same codebase. Touch is the primary input; mouse/keyboard secondary; controller later.
- Two factions minimum with asymmetric doctrine (a satellite-uplink faction vs a mesh-relay/mass faction is one obvious split; agents may propose better). Fictional names, real hardware archetypes.
- Fictionalised setting ("Eastern Europe, near future"), no real unit insignia, no real casualties. Hardware archetypes are real.
- Tech: engine to be decided by the technical spec with iOS as the primary target (Unity, Godot, Unreal, or custom are all on the table; justify).
- Scope: a small studio (5–12 people) shipping an early-access-quality build in 18–24 months.

## Deliverables and their owners
- `spec-gameplay.md` — gameplay spec (the modernisation of C&C).
- `spec-futures-2027-2028.md` — tactical escalation forecast and how it feeds tech tiers / campaign eras.
- `spec-technical.md` — technical spec, written against the gameplay spec.
- `spec-ui.md` — UI/UX spec, written against the gameplay spec.

## Style for every spec
- Markdown, thorough, decisive. Make the call and say why; list alternatives briefly, don't survey.
- Real numbers wherever a number is needed (ranges in map metres, costs in the in-game currency you define, timers in seconds). They can be wrong; they must be present so they can be tuned.
- Every mechanic must trace to one of the seven thesis points or be cut.
- Flag open questions in a final section rather than hedging inline.
- No real-world unit insignia, no real names of serving people. Product names of hardware archetypes are fine as "inspired by" references.
