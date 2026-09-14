# Research brief: navigation when there is no link and no satellite fix

**Status: BLOCKED.** Written up now so nothing is lost; the session's WebSearch
budget (200 calls) is exhausted and every call is refused. Raising
`CLAUDE_CODE_MAX_WEB_SEARCHES_PER_SESSION` unblocks it. Do not write this
document from memory - the questions below are empirical and a plausible-sounding
answer is worse than no answer.

KILL ZONE is a **video game** - a real-time strategy game in the Command &
Conquer tradition, set in a fictional near-future Eastern Europe, for iPad and
iPhone first. This brief asks for published open-source figures to ground a
simulation with hit points and cooldown timers. It is not a request for
operational guidance.

## Why this brief exists

The game now models satellite coverage as ending at a national border rather than
at the front line (see FINDINGS 26). That creates a question the game cannot
currently answer: **what does a drone do once it crosses out?** No control link,
no satellite fix, and whatever it can work out from its own sensors.

The design instinct is to model this as inertial drift plus optional scene
matching against stored imagery, with reference imagery as a consumable that
expires as the ground changes. Before building that, four things need grounding,
and all four came out of the same design conversation.

## 1. How much does terrain have to change to defeat scene matching?

This is the load-bearing question and the one most likely to be got wrong.

Terrain-referenced and scene-matching navigation works by comparing what a camera
or radar altimeter sees against a stored reference. The game wants to model that
reference **decaying** - but decaying how fast, and driven by what?

- What magnitude of change actually breaks a match: seasonal vegetation, snow
  cover, ploughing and harvest, flooding, new craters, destroyed buildings,
  smoke and fire?
- Is the failure gradual (accuracy degrades) or a cliff (lock is lost outright)?
- Does matching against **terrain shape** (contours, radar altimetry) behave
  differently from matching against **imagery** (optical scene)? Shape should be
  far more stable than appearance - confirm and quantify.
- How current does reference imagery have to be, in days or weeks, over ground
  that is being actively fought over?

## 2. Celestial navigation - who actually has it, and what does it buy?

Astro-inertial navigation is old and real, and there is recent reporting of it
appearing on drones as a satellite-denied fallback. What is needed:

- Which classes of airframe plausibly carry it by 2027-28, and at what cost and
  mass. Is this a strategic-strike-only capability or is it reaching cheap
  airframes?
- What accuracy does it deliver, and - critically - is it a **position** fix or
  only a **heading** reference? These are very different mechanics and the
  distinction is easy to blur.
- **Cloud dependence.** A star tracker needs to see stars. What fraction of the
  time is that available in that theatre, at what altitudes, and does daylight
  operation (bright-star tracking) work on cheap hardware or only expensive?
- Does it degrade with time like inertial navigation, or does each fix reset the
  error? This decides whether it is a drift-rate modifier or a periodic reset.

## 3. Inertial drift, honestly

- Drift rates for the MEMS-class inertial units affordable on an expendable
  airframe, in metres per minute of flight. Spec-sheet figures are published for
  this class; get real ones rather than a textbook grade.
- How much does visual-inertial odometry actually recover, and what does it cost
  in compute and in mass?
- What is the practical accuracy at the end of a long satellite-denied run - tens
  of metres, hundreds, or kilometres? The game needs the shape of the error
  growth, not just an endpoint.

## 4. Weather, which the game models not at all

The most under-researched area in the project, and reportedly a larger effect
than any of the masking and camouflage measures already studied.

- **Cloud and overcast**: blocks celestial navigation and satellite optical
  reconnaissance. Base heights and frequencies for that theatre by season.
- **Rain**: already partly covered - `thermal-optical.md` section 6.5 has
  extinction coefficients per band, and `radar-rf.md` has ITU-R rain attenuation.
  What is missing is rain on **flight**: which airframes stop flying, at what
  rate, and whether this is a hard stop or a degradation.
- **Cold**: battery capacity collapse in small electric airframes is the obvious
  one and needs a number. Also icing, which is a hard stop rather than a penalty.
- **Heat**: thermal crossover is already researched; what is missing is density
  altitude and payload limits.
- **Mud**: the seasonal one. Ground movement and resupply, which is a logistics
  mechanic rather than a sensor one, and interacts directly with the netted-
  corridor material already in `ground-logistics.md`.
- **Wind**: partly covered - `acoustic.md` gives a ladder for acoustic detection
  and recommends acoustic off entirely above 12 m/s. Missing: wind limits on
  small airframes by class, and what it does to loiter endurance.

## What the answer should let a designer do

Pick two or three weather states the interface can actually draw, and know which
drone classes each one grounds, blinds or merely inconveniences. If the honest
answer is that weather deserves a single axis rather than several, say so - the
game's own design rules cut any mechanic that needs a submenu.

## Rules

Same as `BRIEF.md`: prefer measured figures and vendor specifications over
commentary and say which is which; mark inference as inference; do not invent
citations; where you extrapolate, say so.
