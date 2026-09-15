# Sector command: owning one piece of a battle that is larger than you

KILL ZONE is a **video game** — a real-time strategy game with fictional
factions, hit points and build timers. This is a design note on an idea that is
not built and not researched.

The idea: the player is responsible for **one sector**, while a real battle
continues across many others. You can go and look. You can take over. And when
you leave, you leave orders behind.

## Why it fits what is already established

This is not a new direction so much as the missing middle of one the project has
already reasoned its way into from three separate directions.

**The scale document already says the campaign is sectors.** The front is
forty-nine maps end to end and the whole country is nine hundred and ninety-three;
refineries sit sixty-five map widths away with a strike-to-trench lag of nine to
thirteen months. The conclusion there was that the campaign cannot be a bigger
tactical map — it has to be sectors and months. Sector command is what makes that
campaign layer something you *play* rather than something you configure between
matches.

**The crew cap already sizes a sector.** Six to twelve concurrent sorties is a
drone battalion holding one piece of front, which is a real formation. The player
was already scoped to that echelon; this makes the rest of the war visible from
it.

**The phone form factor already wants a queue of decisions.** A sortie is a
decision at launch and a decision at the terminal moment. Sector command makes
attention itself the scarce resource — which is both the honest constraint on a
phone and, as it happens, the honest constraint on the subject.

**And "you cannot see the battlefield" becomes true at a second level.** The
research corpus is entirely about detection: five channels, an aperture budget,
terrain masking, a turret mute because its head faced the wrong way. Sector
command applies the same idea one layer up — you cannot watch every sector, so
you find out what happened in the ones you left the same way a commander does,
from reports that are late and partial.

## The hard problem, stated plainly

**Delegation has to be good enough to trust and worse than you.**

Get it wrong in one direction and the AI plays the game better than the player,
who correctly concludes there is no reason to be there. Wrong in the other and
leaving a sector is a punishment, so nobody ever uses the mechanic and the whole
thing is dead weight.

The resolution that seems right: **the delegate executes doctrine faithfully and
cannot improvise.** It handles what you anticipated. It fails on what you did
not. That produces exactly the behaviour the design wants — a reason to go and
look, triggered by something novel rather than by a timer — and it reuses the
alerting the interface already owes on a small screen: *bring the player to the
thing, do not expect them to find it.*

## On learning, which I would argue against

The obvious reading of "leaving tactics behind" is a delegate that learns how you
play. I think that is the wrong build, for three reasons.

**It breaks determinism**, which is the product. The simulation is fixed-point so
that two devices agree bit for bit over tens of thousands of ticks. A learned
policy is a large blob of state that must be identical on both machines and
identical on replay, and every strategy game that has tried this has regretted it.

**It is illegible.** The player cannot tell why the delegate did something, so
they cannot correct it, so they stop trusting it — and a delegate you do not trust
is one you never use.

**And it solves a problem the player would rather solve themselves.** The fantasy
of command is not "the computer imitates me". It is "my intent was carried out
while I was elsewhere".

So: **doctrine you author, explicitly.** A playbook — what to engage, what to
ignore, what to spend, when to ask. Deterministic, inspectable, editable, and it
makes the player's skill into something transferable, which is the actual pleasure
on offer. It is also the one version that gets *better* the more the player knows
about the game, rather than flattening it.

That has a real cost: doctrine needs a vocabulary, and a vocabulary is a design
problem an order of magnitude harder than a unit stat. It should not be attempted
until the tactical game is worth delegating.

## What this needs before it can be designed properly

Research, once the budget allows. Added to the queue rather than guessed at:

1. **How delegation actually works.** Mission command and commander's intent as
   practised — what is specified, what is left open, what a subordinate is
   expected to decide alone. This is a large and well-documented literature and
   the game should borrow its vocabulary rather than invent one.
2. **What drone units specifically delegate**, which may be very different from
   the doctrinal answer. Crews operate semi-autonomously; the question is what
   they are told and what they choose.
3. **Reporting.** What a commander actually learns about a sector they are not in,
   how late it arrives, and how wrong it is. The gap between what happened and
   what gets reported is a mechanic, not a nuisance.
4. **Span of control.** How many sectors one commander plausibly holds, which
   sets how many the player can have.

## What I would not do yet

Build it. The tactical game is still being wired — a turret was mute until an hour
ago — and a delegation layer over a tactical game nobody has played is a layer
over an unknown. The order is: make one sector worth playing, play it, then ask
what you would want to leave behind.
