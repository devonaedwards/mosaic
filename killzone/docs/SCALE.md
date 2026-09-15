# How big is a map, and what is a campaign?

KILL ZONE is a **video game** — a real-time strategy game with fictional
factions, hit points and build timers. This is a design document about map
dimensions and match structure.

The short version: **the map size the game already uses is right, and it is
right for three independent reasons.** What follows is the argument, because a
number that happens to be right by luck will get changed by someone later who
does not know why.

## The one conversion everything hangs off

The game compresses distance **twelve to one**. A 2048-map-metre map is
therefore **24.6 km of real ground**.

## Why 2048 is the right number

**One. It is exactly one kill zone.** The band either side of the front where
nothing moves in daylight is 20–25 km deep and projected at 30 by the end of
2026. In map metres that is 1,700–2,100. The map *is* the kill zone, with
almost nothing to spare — which means every part of it is contested and none of
it is a safe rear area to build in peace. That is the game.

**Two. It is one sortie in about two minutes.** An FPV crosses the whole map in
2.0 minutes, a heavy strike drone in 2.4, a supply truck in 6.2. In a forty
minute match that is roughly twenty map-crossings of time — enough for many
sorties, each of which is a decision, and short enough that a mistake is
recoverable. Double the map and a sortie becomes a commitment rather than a
choice; halve it and there is no transit worth defending.

**Three. It fitted an iPad, and this is now the wrong device.** ~~At a readable
zoom the device shows something like 600–900 map metres, so a 2048 map is four to
nine screens.~~ The target is an **iPhone**, and the arithmetic does not survive
the move.

A fingertip is about 44 points on any device, so the number of distinguishable,
tappable things across a screen is set by logical width — 1024 points on an iPad
against 393 on a phone, a ratio of 2.6. At equal legibility a phone therefore
shows **230–345 map metres** where an iPad showed 600–900. A 2048 map is then
**seven screens across and fifty screens of area**, against the two to four a
Command & Conquer map spanned.

So the third argument has flipped from supporting 2048 to opposing it, and the
sentence above about it winning any disagreement still stands. Two answers are
coherent and they are genuinely different games.

### The rescale

Compression is a free parameter. Nothing in the simulation knows what a map metre
is worth; it is a number in a comment. For the kill zone to fit a two-to-four
screen map on a phone, compression has to run somewhere around **19:1 to 38:1**
rather than 12:1 — call it 26 — which shrinks the map to roughly 850 map metres
while representing exactly the same ground.

Done properly this changes no gameplay at all. Halve the distances and halve the
speeds and every relationship in argument two is preserved: the same sortie takes
the same two minutes, the same twenty crossings fit in a match. It is a units
change wearing a content change's clothes.

The cost is real but bounded: it moves every distance in the catalogue and every
number recorded in `FINDINGS.md`, and the drift guard will light up like a
switchboard. It is a day of careful work, not a redesign.

### Or the reframe, which I think is better

**On a phone you cannot see the battlefield — and this game is about not being
able to see the battlefield.**

That is not a consolation. The entire research corpus is about what you can and
cannot detect: five sensor channels, an aperture budget where seeing far and
seeing wide are the same money, terrain masking, a turret that is mute because
its head was pointed the wrong way for fifty-five ticks. A drone operator does
not have a god's-eye view. They have a single camera feed and a map with contacts
on it.

**A phone screen is a soda straw, and so is the real thing.**

The form factor also pushes toward the game the research already describes rather
than away from it. Crews cap concurrency at six to twelve sorties. A sortie is the
atomic action — a decision at launch and a decision at the terminal moment. That
is a queue of decisions, not a field to survey, and a queue is native to a phone
in a way a base-building overview never will be.

Taking this seriously means the interface owes real work: a contact list that is
the primary view rather than a decoration, alerts that bring you to the thing
instead of expecting you to find it, and drop-in feeds. It also means the map is
a *reference*, not the playfield.

### Which, and what else moves

The two are not exclusive — a modest rescale plus the reframe is probably the
answer — but the reframe is the one that decides what the game *is*, so it goes
first and the rescale falls out of whatever it implies.

One consequence to face either way: **a forty-minute match is an iPad assumption.**
Phone sessions are shorter and more interrupted. If a match has to survive being
put down at three minutes' notice, that is a design constraint on par with the map
size, and argument two above is written as though it is not.

## What a match is, and what it is not

**A match is not about taking ground.** The front moved 37.85 km² in the whole of
July 2026 — about 1.2 km² a day. Across a 24.6 km map that is a couple of hundred
metres of creep per day. Forty minutes of it is invisible, and a game that
awarded territory for a match would be inventing a rate of change twenty times
what the subject does.

So a match is a **raid, a defence, a convoy, or a hunt**. Something specific
happens in one sector and then stops. That matches the research's own conclusion
about the subject: movement, not ground, is the scarce thing.

## What the player actually commands

This is the real answer to "what scale is this game", and everything above is
downstream of it.

The cap is **crews, not airframes** — thirty in the data, realistically six to
twelve in play. That is a drone battalion in one sector, which is a real
formation: both sides now field formal unmanned branches, and the brigades and
detachments they are built from are exactly this size.

So the player is **a drone battalion commander holding one sector**. Not a
general, not a squad leader. Every other number in this document follows from
that, and if the echelon ever changes, all of them change with it.

## Where the map ends and the campaign begins

Measured in map widths, the real distances sort themselves into three tiers with
obvious gaps between them. The gaps are the design.

| | real | map widths |
|---|---|---|
| Forward field depot | 15 km | 0.6 |
| Robot logistics leg | 15 km | 0.6 |
| Kill zone depth | 22 km | 0.9 |
| Trucks start dying | 40 km | 1.6 |
| Railhead sustainment limit | 160 km | 6.5 |
| Depot standoff from the front | 200 km | 8.1 |
| Refinery | 1,600 km | 65 |

**Tier one, on the map (under 1).** The robot logistics leg, the forward field
depot, the kill zone itself. This is the tactical game and it is already built.

**Tier two, a few maps away (1.6 to 8).** Truck logistics, rear depots, the
railhead. Too far to fly to inside a match, close enough to matter to it. This is
**adjacency between sectors** — what the neighbouring map is doing to yours.

**Tier three, sixty-five maps away.** Refineries, power, rail, arsenals. There is
no honest compression that puts these on a tactical map, and the time constants
are worse than the distances: the fuel campaign's lag from strike to trench is
nine to thirteen months, against a forty-minute match. A refinery button with a
falling fuel bar would be both false and dull.

## Do not scale the tactical map to the country

Tempting and wrong. The front is 1,200 km end to end — **49 maps** at one map
deep. All of Ukraine is **993 maps** of 2048 squared.

The campaign is therefore **not a bigger tactical map**. It is a different
representation — sectors as nodes, months as turns, allocation rather than
manoeuvre — and the two connect through what each hands the other: the campaign
picks which map you play, in what weather, with what stockpile, on which side of
the border; the match hands back whether the sector held.

That is also the answer to the deep-strike research's verdict. Strikes that take
months to bite belong on a layer whose clock runs in months. **A strike made in
match three shows up in match nine.** The lag is the mechanic.

## Different sectors should play differently, and the systems for it already exist

This is the part that justifies a campaign at all rather than a map pool. Two
axes, both already simulated:

**Where the border is.** This is the strongest of the two and it is not obvious
until you see it listed:

- *Deep inside your own territory.* Satellite links work everywhere. Navigation
  is never denied. A defensive game about interception and point defence.
- *Astride the border.* The satellite line and the front line are in different
  places and both matter. The most complex of the three.
- *Deep inside theirs.* No satellite coverage at all, navigation denied from the
  first second, everything flying on what it brought. A raid.

Three genuinely different games, from one line on a map, with no new mechanics.

**What the ground is.** Also already simulated, and it cuts in more directions
than terrain usually does:

- *Open steppe* — long sightlines, no tether hazard, and scene matching fails
  because flat ground has no shape to match. The navigation hazard.
- *Forest* — tethers snag, sightlines collapse, matchable ground.
- *Power lines* — where fiber drones go to die.
- *Rubble and urban* — short everything, matchable, acoustically hostile.
- *River line* — the crossing, and the pontoon timer.

Five terrains against three border relations is **fifteen distinct tactical
problems** before weather and season are applied, and weather is four states
against three ground states on its own. Fifteen to twenty handmade maps covers
the space without repeating itself.

## What this means for the build

1. Keep 2048 × 2048 as the standard map. It is defensible three ways.
2. 4096 exists for a reason and should stay rare — it is two kill zones, which is
   a different and slower game. Use it for river crossings and deep raids.
3. Do not add a "capture territory" win condition to a match.
4. Build the campaign as sectors and months, not as a bigger map.
5. Build maps against the fifteen-cell grid above, not by taste.
