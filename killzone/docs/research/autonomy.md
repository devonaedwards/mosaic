# Autonomy in drone warfare, as of September 2026

Research note for KILL ZONE. Written for a designer who has to pick a number and
defend it. Nothing here is operational guidance.

## How this was researched, and how much to trust it

This note is built from open-source reporting retrieved on 14 September 2026.
Two limits are worth stating up front, because they change how much weight each
figure can carry:

1. **Full article text was unavailable.** The research environment blocked direct
   page fetches, so most figures below come from search-engine summaries of the
   named articles rather than from reading them end to end. URLs are given so
   they can be checked. Where a number is second-hand in this way it is marked
   *(snippet)*.
2. **Almost every performance figure in this domain originates with a
   belligerent or a vendor.** There is no independent measurement regime for
   drone hit rates. A "80% hit rate" is a manufacturer's or a brigade's claim,
   collected under conditions nobody outside can inspect, with no agreed
   denominator (does a drone lost to jamming on the way in count as a miss?).
   Treat single-digit precision as noise throughout.

Confidence markers used below:

- **[FIELDED]** — in serial production and in routine combat use; multiple
  independent reports.
- **[DEMONSTRATED]** — shown to work in combat or credible trials, but not at
  scale or not routinely.
- **[CLAIMED]** — asserted by a vendor, ministry or belligerent; no independent
  corroboration found.
- **[SPECULATIVE]** — plausible extrapolation, including mine. Not reported.

---

## Summary

**The single most important finding for the game: in September 2026, autonomy
has not removed the operator. It has removed the radio link.** The thing that is
genuinely, massively fielded is *last-mile terminal guidance* — a human picks
the target, locks it, and the drone flies the final few hundred metres to two
kilometres on its own camera, immune to jamming. That is a different mechanic
from "the machine chooses who to kill", and it has the opposite sign on error:
it makes drones **more** accurate, not less. **[FIELDED, high confidence]**

The machine-chooses-the-target case — full autonomous target selection — exists
in demonstrations, in a small number of contested combat reports, and in a very
large volume of marketing. It is not the backbone of either side's drone force.
**[DEMONSTRATED/CLAIMED, medium confidence]**

Autonomous *interception* is further along than autonomous *strike*, and for a
clean reason: an incoming Shahed over your own territory at night is an easy
classification problem with almost no negative class. Ukraine's MoD described a
system in June 2026 that automates "ninety-five percent of the entire
interception cycle". **[DEMONSTRATED, medium-high confidence]**

"Swarm" in 2026 almost always means one of two things, neither of which is
drones talking to each other: (a) *massing* — hundreds of Gerans on pre-planned
routes with no inter-drone coordination; or (b) *one operator, many drones* —
swarm-management software where a human is a mission commander rather than a
pilot. Genuine cooperative autonomy (drones re-allocating targets among
themselves in flight) is shipping as software but is thinly evidenced in combat.
**[mixed; see table]**

Decoys are the real and correct instinct in the game's model. Both sides field
inflatable vehicles with built-in heat sources and radar reflectors, and
Ukrainian Air Force reporting has put roughly half of a Russian strike package's
airframes in the decoy category. Loitering munitions are documented as having
been expended on decoys. **[FIELDED, high confidence]**

---

## 1. Last-mile / terminal guidance — the part that is real

### What it does

A module with a camera and a small inference chip sits on the airframe. The
operator flies to within sight of the target, boxes it on screen, and commits.
From that point the drone tracks the boxed object visually and completes the
attack with no radio link. The stated design purpose is surviving the last
stretch of flight, where electronic warfare is thickest and where, on a manual
FPV, the pilot's video goes to static and the drone flies into a field.

Reporting consistently describes this as covering roughly **the final 500 m**
of flight *(snippet — dronexl, Oct 2025)*, with longer-lock variants claimed
since. The human still selects the target and initiates.

### What is in service, and at what cost

| System | Origin | Cost | Status |
|---|---|---|---|
| The Fourth Law TFL-1 (terminal guidance module) | Ukraine | not found | Serial production; operational **[FIELDED]** |
| The Fourth Law TFL-2 (autonomous bombing module) | Ukraine | not found | In use across **30+ Ukrainian formations** *(snippet)* **[FIELDED]** |
| Vyriy-10-TFL-1 (FPV + TFL-1) | Ukraine | **Hr 18,500 ≈ $448** *(snippet, Kyiv Post)* | Mass production announced **[FIELDED]** |
| Vyriy-15 | Ukraine | **~$500** *(snippet, TechRadar)* | Claimed 110 km strike on Russian logistics **[DEMONSTRATED/CLAIMED]** |
| Baseline manual FPV (no AI) | Ukraine | **$300–400** *(defence-blog)* | **[FIELDED]** |
| Auterion Skynode (autonomy/strike computer) | Switzerland/US | not found | **33,000 shipped to Ukraine**, >50,000 globally claimed within six months *(snippet)* **[FIELDED]** |
| Lancet-3 / Izdeliye 51-53 | Russia | ~$35,000 *(low-quality source)* | **[FIELDED]**; autonomy claims contested |
| Geran-2 with onboard AI | Russia | not found | Claimed autonomous target recognition on Nvidia Jetson *(GUR claim)* **[CLAIMED]** |

**The cost delta is the load-bearing number: roughly $100–200 on top of a
$300–400 airframe, i.e. a 25–60% price premium for last-mile autonomy.** That
is the number a game economy should reflect. It is not a doubling and it is not
a rounding error.

### Measured improvement in hit rate

Three figures recur, all from interested parties:

- **20% → 80%** in one Ukrainian brigade's preliminary data, attributed to
  better tracking through shadow and tree cover *(snippet)*. A fourfold
  improvement.
- **40% → 80%** as a general manufacturers' claim for AI-enabled FPVs versus
  manual control *(snippet)*.
- **Tenfold increase in successful AI-guided strikes since the start of 2026**
  *(snippet, attributed to 2026 reporting)*. Note this measures *volume of
  adoption*, not per-drone accuracy, and is often misquoted as the latter.

**Design reading:** the honest band is *manual FPV in a contested EW
environment lands somewhere in the 20–50% range; terminal-guidance FPV lands
somewhere in the 60–80% range.* The improvement is real and large. The
denominator is undefined and the sources are partisan, so use the band, not the
point estimate. **[confidence: medium on the magnitude, low on the exact
numbers]**

Crucially, **most of this gain is not "better target recognition". It is
jam-resistance.** The AI is not outperforming a human at deciding what to hit;
it is outperforming a dead radio link at flying the last 500 m. This distinction
matters enormously for the game's model and is returned to in the verdict.

---

## 2. Full autonomous target selection — what evidence actually exists

This is the most over-claimed area in the entire subject. The evidentiary
history:

**Kargu-2, Libya, March 2020.** A UN Panel of Experts report suggested a Turkish
STM Kargu-2 may have engaged retreating forces in a mode requiring no operator —
widely reported as the first autonomous kill. **The panel provided no
corroborating evidence for the autonomous-mode claim**, and STM's position is
that the system uses computer vision to identify and track but requires an
operator to engage: "Unless an operator pushes the button, it is not possible
for the drone to select a target and attack" *(snippet)*. Six years on, this
remains contested and unresolved. **[CONTESTED]**
(ploughshares.ca; lieber.westpoint.edu; thebulletin.org)

**KUB-BLA, Ukraine, from March 2022.** Photographed wreckage confirms use. **No
imagery or reporting establishes whether it was in an autonomous mode**, and
Russian sources describe autonomous *navigation* to a human-selected target, not
autonomous selection. **[CONTESTED — leaning not autonomous]**
(automatedresearch.org)

**Lancet, Russia.** Rostec has promoted it as "highly autonomous", with an
"optical-electronic system that helps independently ferret out and destroy a
target". Reporting on later variants describes the AI taking over at a few
hundred metres to lock and track a *designated* vehicle type. CSIS's assessment
has been that Russia probably has *not* used AI-enabled weapons in Ukraine in
the fully autonomous sense. **[CLAIMED by vendor; terminal lock is
DEMONSTRATED; selection is not established]**
(csis.org; defencesecurityasia.com — the latter low quality)

**Ukraine, 2024–2026.** This is where the picture changed. Reporting in 2026
carries: an official Ukrainian statement that fully autonomous drones killed
Russian soldiers *during a test in 2024*; and separate reporting of an
operational autonomous system that patrols a sector, identifies a soldier, flies
to him and detonates without human input *(snippets)*. Headlines include
"Ukraine's Drones Can Now Kill Without a Human in the Loop" (National Interest,
June 2026), "Fully Autonomous Drones Reportedly Kill in Ukraine" (Small Wars
Journal, 17 Aug 2026), and Forbes reporting in May 2026 that Russian troops fear
"head-hunting" drones. **[DEMONSTRATED for the 2024 test; CLAIMED/thinly
sourced for routine operational use]**

**The official position contradicts the reporting.** Ukraine officially states
it does not field systems that autonomously select and engage targets without
human involvement. Both things are being said at once. The most defensible
reading is that autonomous selection exists as a **capability that is switched
on situationally** — in a defined geographic box, against a defined target class,
usually when the link is gone — rather than as a standing mode of operation.
**[my inference; SPECULATIVE but well supported by the pattern of evidence]**

**Bottom line:** there is no case, anywhere, in which a fully autonomous
target-selection engagement has been independently verified, with the engagement
chain documented, by a neutral party. Six years after Libya. That absence is
itself the finding.

---

## 3. Swarming — what the word means in 2026

Three distinct things get called "swarm". Separating them is the single most
useful thing this section can do.

**(a) Massing.** Hundreds of airframes launched at the same target set on
pre-planned routes. Russian Geran strike packages are the canonical case:
"hundreds of Geran drones with minimal operator involvement", following pre-set
routes to fixed targets, **not dynamically adjusting in flight** *(snippet)*.
Ukraine's Operation Spiderweb (1 June 2025) released roughly **100** small
drones from containers near Russian airbases across several time zones to strike
together — spectacular, and still coordination *by plan*, not in flight.
**[FIELDED]**

**(b) One operator, many drones.** Swarm-management software in which the human
becomes a mission commander rather than a pilot. Kyiv-based **Swarmer** claims
combat-proven collaborative autonomy letting a single operator command
*hundreds* of drones, and cites **82,000 operations** *(vendor claim)*.
**Auterion's Nemyx** makes multiple drones "act as a single, coordinated unit",
engaging multiple targets simultaneously via onboard vision, rolled out on the
Skynode hardware already shipping in tens of thousands to Ukraine; US deployment
was floated for end of 2026. Reporting is explicit that *the human operator is
still very much in charge*. **[FIELDED as software; scale in combat CLAIMED]**

**(c) Genuine cooperative autonomy** — drones sensing each other, re-allocating
targets among themselves mid-flight, degrading gracefully when members are lost.
This is what the word is supposed to mean. Evidence that it happens routinely in
combat is thin. The obstacle is stated plainly in the reporting: swarming
depends on reliable coordination between operator and drones *and among the
drones themselves*, and electronic warfare attacks exactly that. **[DEMONSTRATED
in trials; not established at scale]**

**Practical numbers for a designer:** a "swarm" in 2026 is **8–30 drones under
one operator** for a tactical strike package using type (b) software, or
**100–800 airframes** for a type (a) strategic massing raid. The coordination in
case (a) is a flight plan. The coordination in case (b) is target allocation and
deconfliction pushed down from one console. Almost nothing is genuinely
peer-to-peer.

The accountability problem this creates is live in the legal literature (Lieber
Institute, "Whose Decision Was It? Drone Swarms and the Accountability Gap in
Ukraine").

---

## 4. Autonomous interception — ahead of autonomous strike, and why

**What happened.** On 8 June 2026, per Ukraine's MoD, an autonomous fixed-wing
interceptor launched, acquired an incoming drone *in darkness*, pursued and
destroyed it with **no operator flying it**. The ministry described a system
that automates **95% of the entire interception cycle**, from launch to kill
*(snippet, globalsecurity.org citing Ukrainian MoD)*. **[DEMONSTRATED; MoD
source]**

**Why this is easier than autonomous strike, and this is the important part:**

1. **The negative class is nearly empty.** A propeller-driven object inbound at
   150–200 km/h over your own territory at 2 a.m. is not a civilian, a friendly
   vehicle, or an inflatable decoy. Ground strike requires distinguishing a tank
   from a tractor from a decoy tank with a heater in it. Air defence requires
   distinguishing a Shahed from nothing.
2. **Geometry does the classification.** Heading, speed and altitude alone
   narrow the candidate set to near-unity. No fine-grained recognition needed.
3. **The failure cost is bounded.** A missed interception is a leaker. A wrongly
   selected ground target is a war crime.
4. **The economics force it.** Human interceptor pilots do not scale to
   hundreds of inbound drones per night.

**Numbers.** Interceptors reportedly destroyed **more than 70% of incoming
Shaheds over Kyiv in March 2026** *(snippet)*. Cost per Shahed killed has been
driven to roughly **$10,000**, against ~$4M for a Patriot round, with production
approaching **1,000 interceptors per day** *(snippet — treat the production
figure as aspirational)*. Named systems: **Wild Hornets "Sting"** at ~$2,500,
credited with 3,900 drones downed since May 2025 including the first confirmed
jet-powered Geran-3 kill; **SkyFall P1-SUN** fibre-optic Shahed hunter at
~$1,000 per unit, which the Pentagon reportedly sought to buy (Military Times,
11 Mar 2026). A ~$3,500 pilotless interceptor is separately reported.
**[FIELDED]**

**The pressure point** is the jet-powered **Geran-4**, cruising at close to
twice the speed of the propeller Shaheds interceptor crews learned on. Speed is
what forces autonomy: at those closure rates the human reaction loop is the
binding constraint, not the sensor.

---

## 5. Failure modes actually observed

This section is deliberately separated from the claims above because it is where
the game's design question lives.

**Fratricide.** CSIS analysis identifies fratricide as a **statistically
significant contributor to drone losses on both sides** *(snippet)*. No
percentage is published that I could verify. Documented incidents include a
Ukrainian TB2 shot down over Kyiv by friendly air defence, and a tank unit
downing returning friendly artillery drones with an anti-drone rifle. The
structural cause is stated well in the reporting: **because attritable drones
are built from the same commercial airframes as the adversary's, radar cross
section and visual profile offer no reliable friend/foe discriminator.** There
is no IFF on a $400 quadcopter.

**Friend/foe confusion by autonomous systems specifically.** Reported as a
known, unsolved problem: over sustained periods, fully autonomous systems have
trouble distinguishing friendly drones from enemy ones. Vendors argue their
software separates friendlies and civilians well; the counter-reporting is that
US and Ukrainian anecdotal experience says otherwise *(snippet, RFE/RL and
related)*.

**Decoy susceptibility — the best-evidenced failure mode.**
- Both sides deploy inflatable vehicles and false artillery positions. **Newer
  decoys carry built-in heat sources and IR/radar reflectors** to mimic both the
  thermal signature and the radar cross section of the real thing *(snippet)*.
- Russian Lancets have been expended **multiple munitions per decoy** against
  one designer's imitation launchers *(snippet)*. A $35,000 munition against a
  decoy costing a small fraction of a real system is a straightforward economic
  win for the defender.
- In the air-defence direction, Ukrainian Air Force reporting has characterised
  Russian strike packages as approximately **50% live / 50% decoy** airframes
  (Gerbera-type) *(snippet)*. Ukraine now runs the same play in reverse, using
  payload-free decoy drone swarms to draw out and map Russian air defences.
- ATR reporting names decoy discrimination as **"one of the most pressing
  challenges"** for automated target recognition.

**Mitigation in practice — and note that all of it is procedural, not
technical.** Brigades improvise: altitude bands reserved per unit, no-go bubbles
around friendly positions, and a hard rule that **a human must still approve
strikes in populated areas** *(snippet)*. That last one is the real
human-in-the-loop policy: not a doctrine document, a unit SOP.

**Published error rates: essentially none exist.** No side publishes a
misidentification rate, a fratricide rate, or a decoy-engagement rate for
autonomous systems. Anyone quoting one is either quoting a vendor or making it
up. This is the honest state of the evidence and the game should not pretend
otherwise in its documentation.

---

## 6. Human in the loop vs on the loop

**Stated policy.** Ukraine officially does not field systems that autonomously
select and engage targets without human involvement. Russia makes no
corresponding public commitment and its vendors actively market autonomy. US
policy (DoD Directive 3000.09, pre-dating this research and from general
knowledge rather than the searches below) requires appropriate levels of human
judgement and a senior review for systems that would select and engage targets
autonomously — a review process, not a prohibition.

**Apparent practice.** Human-*in*-the-loop for target selection; human
*off*-the-loop for terminal execution. The human boxes the target and commits;
after that there is no loop to be on, because the link is gone. That is the
fielded architecture on both sides, and it satisfies most stated policy while
making the policy nearly unfalsifiable from outside: **an engagement carried out
with the radio jammed is indistinguishable, from the wreckage, from an
engagement carried out with no human decision at all.** This is why the Libya
question is still open six years later.

**International process.** The CCW Group of Governmental Experts on LAWS ran a
three-year mandate through 2026. The UN Secretary-General and the ICRC president
called for a legally binding instrument concluded **by the end of 2026**. The
GGE **concluded in September 2026 without a binding agreement**, despite 76
states advocating for negotiations; its output is a basis for governments to
*consider* whether to negotiate. The decision point is the **Seventh CCW Review
Conference, 16–20 November 2026**, which will choose between opening a formal
negotiation, extending the discussion-only mandate, or letting the process
lapse. **[FACT, high confidence]** (unoda.org; article36.org; wilpf.org;
davisvanguard.org; dig.watch)

For the game: as of the fiction's present, **there is no treaty, and the most
likely near-term outcome is another extension of talks.**

---

## 7. Compute and cost — what fits on a cheap airframe

The constraint is real but smaller than people assume, and it is shrinking.

| Part | Weight | Throughput | Price |
|---|---|---|---|
| Nvidia Jetson Orin Nano | **140 g** | up to **40 TOPS** | dev kit from **$499**; module less at volume |
| Raspberry Pi class SBC | ~50 g | ~0 TOPS (CPU only) | **<$100** |
| Hailo-15 class accelerator | tens of grams | high TOPS/W | not found |
| Integrated autonomy module (e.g. TFL-1 on Vyriy) | not published | not published | implied **~$100–150** delta over base FPV |

Three consequences:

1. **The compute is no longer the expensive part.** A $448 Vyriy-10-TFL-1 versus
   a $300–400 baseline FPV says the whole autonomy kit — camera, compute,
   integration, margin — costs roughly $50–150. The Jetson dev-kit price is
   misleading; module pricing at scale is much lower, and Ukrainian modules are
   plainly not built on $500 boards.
2. **Weight is the tighter constraint than cost.** 140 g of compute, plus a
   camera, plus a separate flight controller (the Orin Nano cannot fly the
   aircraft), on a 7-inch FPV whose entire payload budget is 1–1.5 kg, is a
   meaningful fraction of the warhead. Every gram of inference is a gram of
   explosive not carried. **This is the right trade for the game to model.**
3. **The model has to be small.** 40 TOPS runs a compact detector-tracker at
   video rate. It does not run anything that reasons about context, intent, or
   whether the heat source in a tank-shaped object is an engine or a heater.

Notably, **Russian systems reportedly run the same silicon** — Nvidia Jetson
TX2 and successors found in Lancet and Geran-2 airframes, per Ukrainian military
intelligence (GUR) reporting in March 2026, alongside claimed Iranian
cooperation on Shahed upgrades. Both sides are flying export-controlled American
edge compute; neither has a compute advantage that matters.

---

## 8. What is genuinely hard and is not close to solved

1. **Non-cooperative target recognition in clutter.** Recognising a specific
   camouflaged vehicle, from an oblique angle, in a treeline, through dust, at
   video rate, on 40 TOPS. The 500 m terminal problem is tractable because the
   human already solved the hard half.
2. **Decoy discrimination.** Named as the pressing ATR problem. A decoy with a
   heater and a radar reflector defeats every cheap sensor channel at once, and
   the cost asymmetry — decoy vs munition — favours the defender permanently.
3. **Friend/foe for attritable airframes.** No IFF, identical commercial
   hardware, no radar or visual discriminator. Currently solved with altitude
   bands and geography, which is to say not solved.
4. **Rare-event validation.** You cannot measure a 2% misidentification rate
   without thousands of documented engagements with ground truth, and nobody has
   ground truth. This is why no error rates are published — not secrecy, mostly
   ignorance.
5. **Battle damage assessment.** An autonomous drone that strikes and detonates
   cannot report what it hit. The autonomy that removes the link also removes
   the evidence.
6. **GNSS-denied navigation at low cost.** Terrain-matching machine vision is
   the answer being pursued by 100+ Ukrainian companies, and it works, but it
   needs pre-mission terrain data and is not robust to seasonal change.
7. **Accountability.** Legally unresolved and getting worse with swarms; see the
   Lieber Institute's accountability-gap work. No treaty. No verification
   method.

---

## 9. Verdict on the game's model

The game's model: **autonomy removes the operator constraint and pays for it in
target-selection error, which cheap decoys make much worse.**

**Half right, and the half that is wrong is wrong in an interesting way.**

### What the game gets right

**Decoys.** Unambiguously correct and better-evidenced than most of this
subject. Inflatable vehicles with heaters and reflectors, munitions expended on
them, ~50% decoy fractions in air raids, ATR practitioners naming it as the
hardest problem. Keep this mechanic and lean into it.

**A cost for autonomy.** Correct in principle. The cost is just not primarily
selection error at the fielded level.

### What the game gets wrong

**1. "Autonomy removes the operator" overstates what is fielded.** In September
2026 autonomy removes *the link*, not the operator. The operator still flies to
the target and designates it. What autonomy buys is (a) immunity to terminal
jamming and (b) a better operator-to-drone ratio — one console commanding 8–30
airframes with swarm software instead of one pilot per drone.

*Suggested change:* make autonomy convert a **1:1 operator requirement into
1:N**, where N is a tech-tier number (start N=4, upgrade to 12, then 30), rather
than making it free. This also gives you a natural upgrade path and preserves
the operator as a targetable, jammable, killable resource — which is what the
real war looks like.

**2. Autonomy in its fielded form makes drones MORE accurate, not less.** The
20%→80% and 40%→80% claims are the fielded case. If the game applies a blanket
error penalty to autonomy it will contradict the most robust finding in the
subject. The error penalty belongs specifically to **machine target
*selection*** — the autonomy tier where no human boxed the target — not to
machine *guidance*.

*Suggested change:* split autonomy into two tiers with opposite signs.

| Tier | Operator cost | Hit/terminal accuracy | Wrong-target rate | Decoy multiplier |
|---|---|---|---|---|
| **Manual link** | 1 operator per drone | **35%** base, **15%** under jamming | ~1% | ×1 (operator sees it) |
| **Terminal guidance** (human designates, machine executes) | 1 operator per 4 drones | **70%**, **65%** under jamming | **3%** | ×1.5 |
| **Autonomous selection** (machine picks in a box) | 1 operator per 20 drones | **60%** | **20%** | **×2.5** |

Reading of the table: the fielded tier is a *huge* upgrade and the game should
let the player feel that. The speculative tier trades a further operator saving
for a real and visible selection error, and is the tier decoys punish.

**3. The error rate the game should use.** My recommendation, stated as an
estimate and not a finding, since **no published figures exist**:

- **Terminal guidance (human designated): 2–5% wrong-target rate.** This is
  essentially the human's own misidentification carried forward plus lock
  transfer errors. Use **3%**.
- **Autonomous selection, clean environment, defined target class: 10–15%.**
  Use **12%**.
- **Autonomous selection, prepared defence with decoys: 30–45% of engagements
  wasted or wrong.** Use **35%**, i.e. a decoy multiplier of roughly **×2.5–3**
  on the base selection error. This is directionally supported by the Lancet-vs-
  decoy reporting and the 50% decoy fraction figure, and nothing more precise is
  defensible.
- **Fratricide specifically: 1–3% of autonomous engagements**, dropping to near
  zero if the player has designated a no-go bubble. Make the bubble a buildable,
  because that is exactly what brigades improvise in reality.

**4. Interception should be the exception.** Give autonomous interception a
*low* error rate — **2–4%** — and no decoy penalty against high-speed inbound
tracks. This is both accurate and good design: it gives the player a clear,
teachable rule ("machines are good at shooting down machines, bad at deciding
which human to kill") that happens to be the actual finding.

---

## 10. What changes by 2028

**[all SPECULATIVE, graded by how safe the extrapolation is]**

- **Very likely.** Terminal guidance becomes standard rather than a premium
  option; the price delta collapses towards zero and every FPV has it. The game
  should treat the manual tier as obsolescent by the fiction's late game.
- **Very likely.** Autonomous interception goes from demonstration to the
  default, forced by jet-powered Shahed derivatives. The human moves from
  "flies the interceptor" to "authorises the engagement box for the night".
- **Likely.** One-to-many swarm control at 20–50 drones per operator becomes
  routine on both sides. Genuine peer-to-peer target reallocation stays rare
  because EW attacks the inter-drone link exactly as it attacks the operator
  link.
- **Likely.** The decoy arms race intensifies. Cheap multispectral decoys become
  standard issue, and the counter is *multi-sensor fusion plus behavioural cues*
  (does it move? does it emit? did it move last week?) rather than better
  single-frame vision. That is a good mechanic: make decoys detectable by
  *observation over time*, not by a better sensor.
- **Uncertain.** Treaty. The November 2026 Review Conference is the branch point.
  Base rate says another mandate extension rather than a binding instrument. A
  prohibition on autonomous selection against personnel specifically is the most
  plausible partial outcome.
- **Uncertain.** Whether autonomous *selection* ever becomes routine rather than
  situational. The technical obstacle (decoys, friend/foe, validation) is real
  and is not on a Moore's-law curve. My estimate is that by 2028 autonomous
  selection is normal against **fixed infrastructure and vehicles in a declared
  free-fire box**, and still exceptional against personnel.
- **Unlikely but high impact.** A publicly documented, independently verified
  autonomous-selection atrocity. This has not happened in six years of
  opportunity. If it does, policy moves fast.

---

## Sources

Retrieved 14 September 2026. Marked *(snippet)* figures come from search
summaries, not full text; several sources below are of variable quality and are
labelled.

**Terminal guidance and cost**
- Kyiv Post, "'Fire and Forget': Ukraine Rolls Out FPV Drones With Autonomous Terminal Guidance" — https://www.kyivpost.com/post/60152
- Ukraine's Arms Monitor, "The Fourth Law: Autonomy for Every Drone" — https://ukrainesarmsmonitor.substack.com/p/the-fourth-law-autonomy-for-every
- DroneXL, "Ukraine's AI Drone Revolution Hits Hardware Reality" (26 Oct 2025) — https://dronexl.co/2025/10/26/ukraine-ai-drone-revolution-hits-hardware-reality/
- TechRadar, on the $500 Vyriy AI FPV — https://www.techradar.com/pro/cheaper-than-an-iphone-price-of-record-breaking-ukraine-ai-fpv-drone-slashed-to-usd500-as-range-increases-sixfold-to-68-miles
- Defence Blog, "What Ukraine's drones really cost" — https://defence-blog.com/what-ukraines-drones-really-cost/
- MilitaryAI, "Ukraine Tests Whether AI Can Guide Drones to Moving Targets" — https://militaryai.ai/ukraine-ai-guided-drones/
- Wikipedia, "Terminal Autonomy AQ-400 Scythe" — https://en.wikipedia.org/wiki/Terminal_Autonomy_AQ-400_Scythe
- Wikipedia, "Fire Point FP-1" — https://en.wikipedia.org/wiki/Fire_Point_FP-1

**Autonomous selection: evidence and contestation**
- Project Ploughshares, "Kargu-2 debate raises awareness of autonomous weapons" — https://ploughshares.ca/kargu-2-debate-raises-awareness-of-autonomous-weapons/
- Lieber Institute, "The Kargu-2 Autonomous Attack Drone: Legal & Ethical Dimensions" — https://lieber.westpoint.edu/kargu-2-autonomous-attack-drone-legal-ethical/
- Bulletin of the Atomic Scientists, "Was a flying killer robot used in Libya? Quite possibly" — https://thebulletin.org/2021/05/was-a-flying-killer-robot-used-in-libya-quite-possibly/
- Automated Decision Research, "Weapons systems with autonomous functions used in Ukraine" — https://automatedresearch.org/news/weapons-systems-with-autonomous-functions-used-in-ukraine/
- CSIS, "Russia Probably Has Not Used AI-Enabled Weapons in Ukraine, but That Could Change" — https://www.csis.org/analysis/russia-probably-has-not-used-ai-enabled-weapons-ukraine-could-change
- CSIS, "Ukraine's Future Vision and Current Capabilities for Waging AI-Enabled Autonomous Warfare" — https://www.csis.org/analysis/ukraines-future-vision-and-current-capabilities-waging-ai-enabled-autonomous-warfare
- Small Wars Journal, "Fully Autonomous Drones Reportedly Kill in Ukraine" (17 Aug 2026) — https://smallwarsjournal.com/2026/08/17/fully-autonomous-drones-reportedly-kill-in-ukraine/
- The National Interest, "Ukraine's Drones Can Now Kill Without a Human in the Loop" (June 2026) — https://nationalinterest.org/blog/buzz/ukraines-drones-can-now-kill-without-human-in-loop-sa-061226
- Forbes (Kirichenko), "Inside Ukraine's Race To Build Autonomous Strike Capabilities" (9 Aug 2026) — https://www.forbes.com/sites/davidkirichenko/2026/08/09/inside-ukraines-race-to-build-autonomous-strike-capabilities/
- Forbes (Hambling), "Slaughterbots: Ukraine's Head-Hunting Drones" (19 May 2026) — https://www.forbes.com/sites/davidhambling/2026/05/19/slaughterbots-now-ukraines-head-hunting-drones-terrify-russians/
- Forbes (Smith), "Fully Autonomous Drone Warfare Is Coming To Ukraine — And Iran" (26 Mar 2026) — https://www.forbes.com/sites/craigsmith/2026/03/26/fully-autonomous-drone-warfare-is-coming-to-ukraineand-iran/
- CEPA, "Between Killer Robots and Flawless AI" — https://cepa.org/article/between-killer-robots-and-flawless-ai-reassessing-the-military-implications-of-autonomy/

**Russian systems**
- Euromaidan Press, "GUR: Russia is testing AI on its Lancet drones using Nvidia tech" (23 Mar 2026) — https://euromaidanpress.com/2026/03/23/gur-russia-is-testing-ai-on-its-lancet-drones-using-nvidia-tech-in-cooperation-with-iran/
- Autonomy Global, "Russia Mass Producing AI-Enabled Geran-2 Drones" — https://www.autonomyglobal.co/what-the-other-guys-are-doing-russia-mass-producing-ai-enabled-geran-2-drones/
- Frontliner, "Russia's AI drones: how tactics evolved over 4 years" — https://frontliner.ua/en/how-russias-approach-to-using-ai-in-the-war-against-ukraine-has-changed/
- ISIS, "Russian Lancet-3 Kamikaze Drone Filled with Foreign Parts" — https://isis-online.org/isis-reports/russian-lancet-3-kamikaze-drone-filled-with-foreign-parts
- *(lower quality, used only for the $35k Lancet figure)* ArtificialWeapons.com — https://artificialweapons.com/articles/russia-lancet-ai-drone-2026
- *(lower quality)* Defence Security Asia — https://defencesecurityasia.com/en/lancet-russia-targetting/

**Swarms**
- Swarmer — https://swarmer.com/
- Auterion, "Auterion Launches Nemyx, Enabling Fully Coordinated Drone Swarms" — https://auterion.com/auterion-launches-nemyx-enabling-fully-coordinated-drone-swarms/
- The Defense Post, "Auterion's 'Nemyx' Unifies Different Drone Fleets" — https://thedefensepost.com/2025/09/05/auterion-nemyx-drone-swarm/
- Lieber Institute, "Whose Decision Was It? Drone Swarms and the Accountability Gap in Ukraine" — https://lieber.westpoint.edu/whose-decision-was-it-drone-swarms-accountability-gap-ukraine/
- Forbes (Hambling), "Swarm Forge: Pentagon's Mass-Drone Test Signals Near-Term Deployment" (28 Jan 2026) — https://www.forbes.com/sites/davidhambling/2026/01/28/swarm-forge-pentagons-mass-drone-test-signals-near-term-deployment/
- Forbes (Mittal), "Drone Swarms Could Be Russia's Answer To Ukrainian Kill Zones" (8 Apr 2026) — https://www.forbes.com/sites/vikrammittal/2026/04/08/drone-swarms-could-be-russias-answer-to-ukrainian-kill-zones/
- NextBigFuture, "Are Drone Swarms Going to Enable Big Land Gains in the Ukraine War?" (Jul 2026) — https://www.nextbigfuture.com/2026/07/are-drone-swarms-going-to-enable-big-land-gains-in-the-ukraine-war-warfare-would-be-changed-forever.html
- Kyiv Post, "How Ukraine's Drone Swarms Decimated Russia's Shadow Fleet in the Sea of Azov" — https://www.kyivpost.com/videos/81001

**Interception**
- GlobalSecurity / Ukrainian MoD, "Next-generation interceptors: Ukrainian drones already autonomously take down Shahed-type UAVs" (8 Jun 2026) — https://www.globalsecurity.org/wmd/library/news/ukraine/2026/06/ukraine-260608-ukraine-mod02.htm
- Forbes (Kirichenko), "Ukraine Turns To Autonomous Drone Interceptors As Shahed Attacks Surge" (8 Mar 2026) — https://www.forbes.com/sites/davidkirichenko/2026/03/08/ukraine-turns-to-autonomous-drone-interceptors-as-shahed-attacks-surge/
- Military Times, "These are Ukraine's $1,000 interceptor drones the Pentagon wants to buy" (11 Mar 2026) — https://www.militarytimes.com/news/pentagon-congress/2026/03/11/these-are-ukraines-1000-interceptor-drones-the-pentagon-wants-to-buy/
- Defence Ukraine, "The Jet-Shahed Problem: Interceptors vs the Geran-4" — https://www.defenceukraine.com/en/insights/jet-shahed-interceptor-drones-2026/
- The Defender, "Terminal guidance on interceptors: how it works in Ukraine" (May 2026) — https://thedefender.media/en/2026/05/terminal-guidance-interceptor-drones/
- *(lower quality)* MigFlug, "The $3,500 Drone That Kills Shaheds Without a Pilot" — https://migflug.com/jetflights/autonomous-drone-on-drone-interception-ukraine-maxon/

**Failure modes, fratricide, decoys**
- RFE/RL, "Swarm Wars: The Shaky Rise Of AI Drones In Ukraine" — https://www.rferl.org/a/drone-ai-technology-russia-ukraine-war/33078798.html
- Breaking Defense, "Everyone is learning the wrong lessons from Ukraine's drone war" (Aug 2026) — https://breakingdefense.com/2026/08/everyone-is-learning-the-wrong-lessons-from-ukraines-drone-war/
- Defense Mirror, "Drone Fratricide Witnessed Over Ukraine" — https://defensemirror.com/news/33718/Drone_Fratricide_Witnessed_Over_Ukraine
- The Cove (Australian Army), "Doctrine VS Reality — FPV Drones, Targeting Authority, and the Doctrinal Gap" — https://cove.army.gov.au/article/doctrine-vs-reality-fpv-drones-targeting-authority-and-doctrinal-gap
- Forbes (Mittal), "Russia And Ukraine Are Deploying Increasingly Advanced Decoy Tanks" (3 Mar 2025) — https://www.forbes.com/sites/vikrammittal/2025/03/03/russia-and-ukraine-are-deploying-increasingly-advanced-decoy-tanks/
- Kyiv Post, "Ukraine Uses Decoy Drone Swarms, AI to Challenge Russian Air Defenses" — https://www.kyivpost.com/post/77275
- United24 Media, "How This 100-Year-Old War Trick Is Outsmarting Russian Drones in Ukraine" — https://united24media.com/latest-news/how-this-100-year-old-war-trick-is-outsmarting-russian-drones-in-ukraine-15236
- Ukrainian Air Force via Yahoo News, on 50% decoy composition of Russian raids — https://www.yahoo.com/news/ukrainian-air-force-reveals-tactics-103246675.html

**Compute**
- Nvidia Jetson Orin Nano specifications and pricing, via UAV Systems International — https://uavsystemsinternational.com/blogs/drone-guides/big-computing-performance-for-your-drone-jetson-nano
- ModalAI, "Top 5 Companion Computers for UAVs" — https://www.modalai.com/blogs/blog/top-5-companion-computers-for-uavs
- Hailo, "Physical AI Drones" — https://hailo.ai/applications/physical-ai/drones/
- Kingy AI, "Nvidia's Jetson Orin Nano 2 Brings More AI Muscle to Robots and Drones" — https://kingy.ai/news/nvidia-jetson-orin-nano-2-edge-ai-robots-drones/

**Policy and international process**
- UNODA, "Briefing by the Chair of the CCW GGE on LAWS" — https://disarmament.unoda.org/en/updates/briefing-chair-ccw-gge-laws-margins-first-committee
- Davis Vanguard, "UN Discussions Conclude without Binding Treaty on Autonomous Weapons" (Sep 2026) — https://davisvanguard.org/2026/09/un-lethal-weapons-talks-conclude/
- Article 36, "Opportunities after the UNGA Resolution on Autonomous Weapons" — https://article36.org/updates/opportunities-after-the-unga-resolution-on-autonomous-weapons-moving-toward-a-new-treaty/
- WILPF, "CCW Report, Vol. 14, No. 2: The Final Stretch Before the Finishing Line" — https://www.wilpf.org/ccw-report-vol-14-no-2-the-final-stretch-before-the-finishing-line/
- RSIS, "International Regulation of Lethal Autonomous Weapon Systems: A Long Road Ahead" — https://rsis.edu.sg/rsis-publication/idss/ip25095-international-regulation-of-lethal-autonomous-weapon-systems-a-long-road-ahead/
- Digital Watch Observatory, "GGE on lethal autonomous weapons systems" — https://dig.watch/processes/gge-laws
- Stop Killer Robots, "September 2025 GGE Joint statement" — https://www.stopkillerrobots.org/news/september-2025-gge-joint-statement/

**Not from these searches:** the description of US DoD Directive 3000.09 in
section 6 comes from general background knowledge, not from a source retrieved
here, and should be verified before being relied on.
