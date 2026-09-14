# spec-futures-2027-2028.md
## Escalation forecast for the drone war, and how it becomes tech tiers and campaign eras

Companion to `kill-zone-bestiary.html` (research base, compiled Sep 2026) and `brief.md`.
Written September 2026. Forecast horizon: 31 December 2028.

---

## 1. Executive summary

The 2024–2026 war settled one argument: the control link is the unit's real stat block, and every defeat of a link has been answered by moving one column right — radio, mesh, fiber, satellite, autonomy. By September 2026 all five columns are in the field at once and the ladder has run out of rungs. There is nowhere to go after autonomy. That is the single most important fact about 2027 and 2028: **the escalation stops being about links and becomes about judgement, magazine depth, and physical interception.** Four things follow.

**Autonomy arrives on defence first and on offence second.** An interceptor's target set is unambiguous — a Geran over your own territory is a lawful target with no discrimination problem — so autonomous interception normalises in 2027 while autonomous strike stays officially deniable. The line moves from "human in the loop" to "human on the loop" without anyone announcing it. The UN GGE process hit its 2026 deadline with no treaty and no negotiating mandate, removing the last external brake.

**The counter to autonomy is deception, not jamming.** Once a drone picks its own target from a camera, the cheap counter is a $40 decoy and a thermal blanket, not a $40,000 EW suite. Deception is the most under-built capability on the battlefield and the most likely growth area of 2027 — and the most under-used mechanic in strategy games.

**The defender wins the magazine war per shot and loses it per hour.** Interceptors at $1,400–3,500 killing Gerans at $40,000–80,000 inverted the cost-exchange ratio for the first time in this war. Ukraine claims up to 1,000 interceptors a day but a shortage of operators; against a Russian goal of 1,000 attack drones a day, Ukraine's own assessed need is roughly 3,000 interceptors a day. Money is no longer the binding constraint. **Crews are.** Which drives autonomy again, closing the loop.

**The kill zone keeps deepening and the front does not un-freeze by itself.** It ran 3–5 km in 2023 and 20–25 km in mid-2026, with a corps commander expecting 30 km by year end; extrapolation gives 35–45 km by end-2027 and 40–60 km by end-2028, at which point divisional rear areas sit inside it. Manoeuvre returns only where one side can buy a local, temporary counter-drone bubble — massed EW, turret vehicles, interceptor batteries, decoy screens — and push a robotic assault through it before it collapses. That is 2028 warfare, and it is an excellent shape for a real-time strategy game.

For the game this yields one new tech tier (**Tier 4: Release Authority**, gated by an irreversible doctrine choice rather than by money) and five campaign-era modifiers that re-tune the whole roster without rebuilding it.

---

## 2. Method and confidence levels

### 2.1 How this was built

Three inputs, in descending weight. **The bestiary's sourced 2026 baseline** — unit classes, costs, ranges, links, counters — taken as ground truth for "where we are". **Open-source reporting through September 2026**, gathered by search; where a defence publication is blocked to direct fetch I used the search summary and marked the claim, rather than fabricating a citation. Every URL is in section 8. **Move/counter-move reasoning** on the control-link ladder (radio → mesh → fiber → satellite → autonomy) and the physical-defeat ladder (EW → nets → guns → interceptors → directed energy), with an industrial check on each: can it be made at the rate the tactic needs, with the crews the tactic needs.

### 2.2 Rules held to

No invented citations; inferences are marked *[extrapolation]*. Numbers over adjectives, because a forecast without a number is not falsifiable. Every escalation carries an **observable signal** — if I cannot say what a reader would see in open sources when it arrives, it is not a forecast. And prefer the boring extrapolation: most of 2027 is 2026 at three times the rate with better software. Belligerent claims on kill counts, hit rates and production are directional, not audited; the Oklahoma Watch fact-check of the "70–80% of casualties are drone-caused" figure is the model — the direction is right, the decimal place is not. Ceasefire risk is not modelled: if the war stops in 2027, all of this still happens, in NATO, Chinese, Korean and Gulf procurement instead, on a slower clock.

### 2.3 Confidence scale

**High** — already demonstrated in combat or in serial production; only scale is in question (≥75%). **Medium** — prototype or limited fielding exists, the industrial path is visible, one hard blocker remains (40–75%). **Low** — physics and economics work but no fielded example; needs a policy change or a cost curve to break (≤40%).

### 2.4 A note on one figure in the brief

The brief cites "Ukraine at 100,000 interceptors a year". As of September 2026 that is **too conservative by roughly an order of magnitude**: Zelensky has publicly stated up to 1,000 interceptors a day with operators, not airframes, as the constraint; SkyFall states a Jetkiller capacity up to 50,000 a month. Treat 100,000/year as the 2025 figure. The 2027–2028 planning figure is **300,000–500,000 a year against a requirement near 1,000,000**, and the gap closes through autonomy, not factories. For the game economy this matters: interceptors should be cheap and abundant, and the scarce resource should be release authority and crews.

---

## 3. Fifteen escalations

Each entry: what it is, the driver, the counter it provokes, likelihood by end-2027 and end-2028, and the observable signal.

---

### E1. Closing the Loop — autonomous strike inside a designated box

**What.** Not roaming killer robots. A commander designates a box and a target class ("armoured vehicles, this grid, next four hours") and a group of drones runs find-fix-finish inside it, with one human supervising several missions rather than approving each release. Every technical piece is fielded: Swarmer's collaborative autonomy, the V2U's independent target selection, the autonomous Molniya with terrain-following and onboard computing seen in Zaporizhzhia in July 2026, the $100 last-mile vision module that lifts jammed hit rates from 10–20% to 70–80%. What changes is the *authority*, not the code.

**Driver.** EW makes the link unreliable, so the drone must finish alone anyway; operators are the scarce resource on both sides; and the legal brake failed — the UN GGE reached its November 2026 report with no negotiating mandate and the Secretary-General's 2026 treaty deadline expired without an instrument.

**Counter.** Deception (E3), specifically decoy-rich terrain placed where the box will be drawn. Second-order: hunt the supervising operator and the mission server, because killing him now removes ten strikes rather than one.

**Likelihood.** End-2027 **high** for autonomous terminal engagement inside an operator-drawn box — already effectively happening; reporting will simply stop hedging. End-2028 **medium-high** for autonomous target *selection* at group scale. The blocker is not capability; it is that neither side wants to be first to say so.

**Signal.** A ministry or manufacturer describing a system in "missions supervised per operator" rather than "strikes approved". That metric only makes sense on the loop. Secondary: a publicised fratricide attributed to misclassification.

---

### E2. Defensive Autonomy First — the interceptor swarm

**What.** Autonomy normalised on air defence before strike, because a Geran inbound over your own rear presents no discrimination problem. Radar cues a magazine; interceptors self-assign across a salvo, deconflict, and re-task as some miss. One crew supervises a battery, not a drone. Fixed-wing interceptor batteries already fly to impact on onboard AI with radio and GPS contested, and vendors now market multi-interceptor coordination explicitly.

**Driver.** Arithmetic. Russia launched a record 741 drones in one night in July 2026 against a stated goal of 1,000 a day; interceptor drones already account for around half of Shahed-type kills on heavy nights. Ukraine says it has the airframes and not the crews.

**Counter.** Salvo composition — decoy-heavy raids (roughly 50,000 decoy airframes planned for 2026 alongside 60,000 strike airframes) designed to make an autonomous defence spend its magazine on Gerberas — plus terminal manoeuvre tuned to defeat a machine's engagement geometry.

**Likelihood.** End-2027 **high**: the least controversial autonomy in the world, with an overwhelming cost case. End-2028 **high**, and by then the default for point defence, with human approval retained only over populated areas.

**Signal.** Procurement written in rounds-per-battery rather than drones-per-crew; or a published intercept where simultaneous engagements exceed operators present.

---

### E3. The Decoy Economy — deception as an industrial line item

**What.** Mass-produced deception aimed at machine vision, not human eyes: inflatable and plywood silhouettes with correct thermal signatures, heated plates mimicking an engine deck, corner reflectors, disruptive patterns chosen to break a classifier's feature set. Today improvised and local; by 2028 a supply category with a per-kilometre issue rate. *[Extrapolation: no state has announced decoy procurement at scale; the bestiary flags this as open design space.]*

**Driver.** Direct consequence of E1. When the terminal decision is made by a small vision model on a $500 airframe, fooling it is cheap and repeatable in a way fooling a human pilot never was. A decoy that soaks one autonomous strike costs a few percent of the vehicle it protects.

**Counter.** Multi-sensor fusion and behavioural cues — motion, plume shimmer, radar return, RF emission — which add cost, weight and latency to the cheapest weapon on the battlefield and re-open the door to EW against the sensors they add. Plus retraining cycles, which make deception *perishable*: a pattern works until the enemy re-trains, and the arms race becomes a data race.

**Likelihood.** End-2027 **medium** — field improvisation certain, centralised procurement the question. End-2028 **high**, driven as much by NATO as by the Ukrainian front, because NATO armies own vehicles they cannot afford to lose.

**Signal.** A national procurement line for decoy targets in the tens of thousands, or a vendor advertising classifier robustness against adversarial camouflage.

---

### E4. The Jet Ladder — fast one-way attack versus fast interception

**What.** A pure speed race in the deep-strike layer. Geran-3 raised one-way attack speed from about 250 km/h to about 370 km/h; Ukrainian intelligence assesses the Geran-4 was designed specifically to beat interceptors and reports Geran-3 production halted in August 2026 in favour of the -4 and -5, at roughly 3,000 jet airframes a month alongside about 2,800 piston Geran-2s. The answer is a 450 km/h-plus interceptor generation — SkyFall's Jetkiller entered serial production in August 2026 — plus cheap SAMs where drones cannot close the geometry.

**Driver.** Interceptor drones broke the cost-exchange ratio, so the attacker's escape is to out-run the cheap answer and force the defender back onto expensive missiles.

**Counter.** The bottleneck moves from the interceptor to the **cue**: at 500 km/h you need radar track and a launch decision in tens of seconds, which drags autonomy (E2) forward. It also pushes interception geometry to the border, which means forward radar and forward launchers — both then targets.

**Likelihood.** End-2027 **high** that jet OWA is the majority of the Russian long-range fleet and 450 km/h interceptors are in serial use. End-2028 **medium** that the attacker regains the edge; more likely an expensive equilibrium around 85–92% interception with leakers aimed at a few high-value nodes. *[Percentages are extrapolation; the 2026 baseline is a claimed ~90% overall with Shahed hit rates near 9–14%.]*

**Signal.** Interceptor marketing leading with dash speed rather than cost; a rocket-boosted or turbine interceptor drone in volume production.

---

### E5. Fiber Goes Long and Goes Heavy

**What.** Fiber stops being an FPV trick and becomes a platform-agnostic link. Fifty-kilometre spools are demonstrated — a Ukrainian maker reported 50 km strikes in April 2026, a Russian ring-wing FPV is reported at up to 50 km, and a French tether spool is marketed at 50 km and as compatible with ground, air, underwater and amphibious vehicles. Three consequences: fiber on **UGVs**, removing the worst failure mode of ground robots — jammed into immobility in the open; fiber on **heavy bombers**, making the night mining and resupply platform EW-proof; and **fiber trunking**, where a carrier brings the spool forward and shorter-tethered drones work from it.

**Driver.** Fiber is the only genuinely unjammable link and the weight penalty falls yearly. A fiber UGV that costs 10% more and cannot be stopped by a trench jammer is an obvious buy.

**Counter.** A dedicated anti-fiber layer, already being stood up: the UK has launched an anti-fibre-optic drone programme and vendors sell passive optical detection precisely because fiber drones emit nothing. Practically — optical and acoustic detection, tether-cutting by wire traps and other drones, and the highest-value counter, **backtracking the fiber to the launch site**, since spent fiber is a line on the ground pointing at the crew.

**Likelihood.** End-2027 **high** for 40–50 km air fiber in routine use, **medium** for fiber UGVs at company scale. End-2028 **high** for both, **medium** for fiber heavy bombers — a 30 km spool plus 15 kg of payload is a real multirotor design problem.

**Signal.** A UGV advertised with a spool length in its spec sheet; a heavy bomber photographed with a spool canister; a fielded "fiber hunter" sold on finding the launch point.

---

### E6. The Robot Assault Company

**What.** Ground robots graduate from logistics to manoeuvre. The markers are on the board: a single combat UGV held a contested intersection for 45 days in what Ukraine's 3rd Army Corps called its first fully robotic defensive operation; a Russian position was taken in April 2026 by UGVs and UAVs alone with no reported friendly casualties; the 3rd Corps stood up a UGV strike company; Ukraine ran a robotic amphibious assault in July 2026, began dropping robots from heavy drones in August, and put rifle-armed UGVs into combat testing the same month. Over 100,000 UGV missions in eight months, 25,000 systems contracted.

**Driver.** Infantry scarcity, and a measured effect: Ukraine's General Staff calculates roughly 35% fewer friendly casualties in UGV- and drone-dense units. With a 25 km kill zone, the irreplaceable thing is the man who walks through it.

**Counter.** Mines, which stop robots better than men and cannot be argued with; fiber FPVs, which kill the UGV with no link fight; EW, which strands radio UGVs in the open — hence E5. And the recognition that a robot cannot take a surrender, clear a dugout, or tell a wounded conscript from a fighter, so a small human element stays attached.

**Likelihood.** End-2027 **high** for company-scale robotic assault as routine on at least one axis. End-2028 **medium-high** for battalion-scale robotic-first assaults where humans arrive only to consolidate.

**Signal.** A table of organisation listing UGVs as manoeuvre elements rather than support; a brigade reporting assault strength in robots.

---

### E7. The Relay Grid — motherships, balloons, and a persistent airborne link layer

**What.** The mothership stops being a stunt and becomes infrastructure. Every ingredient is fielded: Gerberas and converted Geran-2s carrying FPVs plus a repeater deep into Ukraine; FP-1/2 airframes carrying Starlink-equipped FPVs on the wings; an A-32 ultralight carrying six interceptors; Maguras launching FPVs that hit S-400s off Crimea; balloons lofting Starlink Mini relays; Russia testing the Barrazh-1 stratospheric relay at roughly 20 km. Assembled, they make a **standing airborne relay grid** that extends short-range control 40–150 km and re-forms when nodes die.

**Driver.** Short-range airframes are absurdly cheap and long-range control is the only thing keeping them out of the enemy rear. A relay is the cheapest range multiplier ever invented.

**Counter.** Node hunting — long-endurance interceptors and gun trucks tasked purely at repeaters — and an altitude race: put the relay at 20 km and only a missile reaches it. It also revives something the drone war had killed, **airborne early warning as a tactical requirement**, because you cannot find a loitering repeater without looking up and out.

**Likelihood.** End-2027 **high** for routine mothership use on both sides. End-2028 **medium** for a persistent multi-node grid rather than mission-by-mission carriers, gated on endurance and on somebody paying for standing air cover.

**Signal.** A relay platform sold by hours-on-station and coverage footprint rather than by payload — infrastructure, not vehicle.

---

### E8. Sovereign Satellite Parity

**What.** The satellite column stops being one-sided. Russia is building Rassvet: 16 satellites in March 2026 and 16 more in July, a stated 156 by end-2026 and a 318-satellite constellation targeted around 2028, at roughly 800 km with claimed 1 Gbps terminals. Ukraine's fallback, Eutelsat OneWeb, remains under 1,000 terminals against roughly 42,000 Starlink terminals carrying over 70% of tactical data near the front.

**Driver.** Russia lost commercial satellite access in 2026 and answered with mesh. Mesh is a workaround; a sovereign constellation removes the dependency permanently.

**Counter.** Uplink jamming and terminal hunting rather than link denial — you attack the terminal, the antenna and the policy, not the beam. Above that sits a strategic counter with no tactical answer: anti-satellite activity, and the political fight over who may use which constellation for targeting.

**Likelihood.** End-2027 **medium** for militarily useful Russian sovereign coverage over the theatre; a partial constellation gives intermittent passes, which is enough for some missions. End-2028 **medium-high** for near-continuous coverage. Launch cadence is the blocker and it is a real one. *[The 2028 constellation size is a stated plan, not an achieved fact.]*

**Signal.** Russian drones controlled over satellite beyond mesh range with no relay chain in evidence; Russian UGVs working at 50-plus km without a repeater.

---

### E9. The Turret Belt

**What.** AI gun turrets multiply from point defence into a belt. Ukraine's Sky Sentinel mounts a .50-calibre on an AI tracking mount claimed to handle targets to 800 km/h; Bullfrog turns an ordinary machine gun into an autonomous mount and rides on Magura hulls; US forces are testing an AI hard-kill counter-UAS turret against swarms. A turret is an interceptor's vision stack pointed from the ground, with a magazine measured in belts rather than airframes.

**Driver.** Cost per shot. Once the turret exists, killing a small drone costs a few dollars of ammunition — better than a $1,400 interceptor — and needs no crew.

**Counter.** Stand-off and saturation. Turrets have a few hundred metres to about two kilometres of useful reach, so attackers hit them from outside that ring: fiber FPVs in terrain masking, loitering munitions from 30 km, mortars. And a turret's radar cue is a beacon.

**Likelihood.** End-2027 **high** for turrets on fixed high-value points — depots, bridges, airfields, command posts. End-2028 **medium** for a continuous belt; more likely a lattice of protected nodes with gaps everyone knows about.

**Signal.** Turrets procured per kilometre of road or per depot rather than per unit; the first published turret engagement of a friendly aircraft.

---

### E10. Directed Energy, Narrowly

**What.** Lasers and high-power microwave arrive, and arrive small. Ukraine's Tryzub laser entered final testing in early 2026 with claimed useful range around 5 km; Ukraine is trialling indigenous microwave weapons through Brave1. RAND's assessment, as reported, is that neither is yet reliable, robust or scalable for sustained use in a drone-saturated environment. That read will still be substantially correct at the end of 2028.

**Driver.** Infinite magazine. A laser that works is the only counter-drone weapon whose cost per engagement is measured in cents.

**Counter.** Weather and mass. Fog, smoke, rain and dust cut dwell effectiveness; every extra target adds seconds of dwell; thirty cheap airframes beat a single-aperture weapon by arithmetic. HPM is the mirror image: many targets at once in a cone, at very short range, and it will happily kill your own electronics and the turret beside it.

**Likelihood.** End-2027 **medium** for a handful of laser sites defending fixed strategic targets; **low** for anything mobile and tactical. End-2028 **medium-high** for fixed-site and naval lasers inside a layered defence; **low** for laser as a front-line unit; **medium** for HPM as a short-range last-ditch swarm-breaker at airfields and ports.

**Signal.** A laser credited with a multi-target engagement in bad weather — or, the honest signal, an operator complaining publicly about availability rates.

---

### E11. The Magazine War

**What.** The industrial contest becomes the decisive one. Russia: roughly 404 Shahed-type drones a day in January 2026 against a stated goal of 1,000, on a 2026 plan of about 60,000 long-range airframes plus 50,000 decoys. Ukraine: up to 1,000 interceptors a day claimed, roughly 3,000 a day assessed as needed. Per shot the defender now wins — $1,400–3,500 against $40,000–80,000, versus $4.2M for a PAC-3 MSE. Per hour the defender loses, because interceptors need cues and crews and the attacker needs neither.

**Driver.** Both sides have learned that resupply rate beats stockpile size: a magazine of 500 with 325 arriving daily outlasts a magazine of 16,000 with 200 arriving daily.

**Counter.** Strikes on production — which is why drone factories, engine plants and optics suppliers become priority deep-strike targets and why both sides are burying them. And when interception outpaces production the attacker does not stop: he shifts to decoy-heavy salvos that burn the magazine, to jet speed (E4), and to **timing**, concentrating a week of output into one night to exceed the defender's simultaneous-engagement ceiling rather than his stockpile.

**Likelihood.** End-2027 **high** that routine-night interception exceeds 90% and leakage becomes concentrated and deliberate. End-2028 **high** that both sides accept an attrition equilibrium in the deep-strike layer and push effort back into the tactical band, where interception is hardest.

**Signal.** Hit rates per launched airframe falling below 8% while absolute damage stays flat — the signature of an attacker trading efficiency for volume and precision of aim-point.

---

### E12. The Operator Ceiling

**What.** The binding constraint of the war becomes human. Ukraine names operator shortage as the interceptor bottleneck; Russia's Rubicon grew from about 1,450 people in March 2025 to roughly 5,000 against an authorised 9,000 by spring 2026, with an Unmanned Systems Forces recruiting target reported near 78,800 by end-2026 — and operators are themselves a priority target. Three answers, all in progress: **remote pools** (Hornet Vision Ctrl reportedly flies an interceptor from 2,000 km away over the internet, so the crew is no longer a front-line target), **one-to-many supervision** (Swarmer-class software), and **full autonomy** (E1).

**Driver.** You can build a million airframes in a year. You cannot train a million goggle-qualified pilots in a year, and the ones you have are being hunted.

**Counter.** Attacks on the pool rather than the crew: cut the internet path, jam the uplink, strike the control node. A pilot in another country is safe from an FPV and wholly dependent on a link — which puts the war back on the control-link ladder, one level up.

**Likelihood.** End-2027 **high** that remote and distributed piloting is normal for air defence. End-2028 **high** that operator-to-drone ratios above 1:10 are standard in the interceptor and ISR roles.

**Signal.** Drone-crew recruitment that does not require deployment; a strike on a control node hundreds of kilometres from the front described as a counter-crew operation.

---

### E13. NATO's Eastern Flank Grid

**What.** The eastern flank builds a layered counter-drone system rather than buying more fighters. The European Drone Defence Initiative launched in Q1 2026, initial capacity targeted for end-2026 and full function by end-2027, as interoperable sensor, EW and interceptor networks aligned to NATO command rather than a literal wall; a large NATO counter-drone initiative has been reported at the $40bn scale *[reported figure, treat with caution]*. Concretely: Lithuania bought 48 Merops interceptors and is fast-tracking more; Poland and Romania field the class; five NATO states agreed joint development of affordable interceptors; Poland unveiled a long-range one-way attack line reportedly scalable to 1,000 a month; vendors are seeding distributed production in NATO countries. The US Army stood up a roughly 600-strong tactical unmanned systems battalion in January 2026.

**Driver.** Repeated airspace incursions, and the recognition that firing million-dollar missiles at $20,000 drones is a losing game an adversary can play deliberately.

**Counter.** Grey-zone probing below the threshold that triggers the grid, and attacks on its seams — the sensor gaps between national systems, which are a political problem, not a technical one.

**Likelihood.** End-2027 **high** for a functioning interceptor-and-sensor layer over the Baltics, Poland and Romania, unevenly covered. End-2028 **medium-high** for genuine cross-border interoperability; procurement politics is the blocker, not engineering.

**Signal.** A cross-border intercept — one nation's sensor cueing another's interceptor — reported as routine rather than as an achievement.

---

### E14. The Littoral Robot War

**What.** The sea catches up. The first USV-versus-USV duel was reported in September 2026, a Ukrainian Sargan-3000 sinking a Russian attack USV. Maguras carry Bullfrog turrets as Shahed interceptors, launch interceptor UAVs from deck containers, and put FPVs onto S-400s ashore. Ukraine showed the Triton multi-role drone mothership and the SIRENA high-speed sea drone at Eurosatory 2026; Russia markets USV and UUV lines for export. The USV becomes what it obviously is: a mothership with air defence, a gun, a rocket pod and a magazine of small drones, at a quarter of a million rather than a quarter of a billion.

**Driver.** A robot boat is the cheapest way to move a launcher 800 nautical miles, and the Black Sea proved a navy can be pushed out of its own water by them.

**Counter.** Anti-USV USVs and helicopters; coastal turrets and interceptors; and the underwater layer — mines, UUVs and harbour nets — because everything above the surface is now watched.

**Likelihood.** End-2027 **high** for USV-versus-USV engagement as normal, **medium** for USVs as routine air-defence pickets. End-2028 **high** for both, **medium** for the Baltic as a second theatre of the same pattern: infrastructure attack, cable cutting, harbour defence.

**Signal.** A navy writing doctrine for surface drone-on-drone engagement; a USV credited with an aircraft shootdown outside the Black Sea.

---

### E15. The Buried Front and Armour's Conditional Return

**What.** Two halves of one adaptation. **Burying:** ammunition, logistics hubs, command posts and aircraft go underground or into hardened shelters; trench systems become covered networks; movement happens at night, in ones and twos, on motorcycles, or by robot. **Armour:** the tank returns not as a breakthrough weapon but as a specialist — cage-and-net armoured, under an EW and turret bubble, in short dashes at night, increasingly carrying its own hard-kill counter-drone system. The 2026 consensus is that armour is not obsolete but is far easier to find and kill, and that armoured exploitation stays operationally necessary even when tactically expensive.

**Driver.** Nothing survives in the open that can be seen. So do not be seen — and if you must be seen, be seen for ninety seconds.

**Counter.** Persistent thermal stare and pattern-of-life analysis, which defeat night as a hiding place; fiber FPVs flown into dugout doorways and tunnel mouths; remote mining of the few routes that burrows force traffic onto; thermobaric and penetrating payloads on heavy bombers aimed at the covered position.

**Likelihood.** End-2027 **high** for burying as standard practice at every level. End-2028 **medium-high** for an armoured vehicle with integrated hard-kill counter-FPV in serial service on at least one side; **low** for a return to massed armoured assault.

**Signal.** A tank fielded with an APS advertised against small drones rather than ATGMs; a logistics tunnel long enough for its spoil heaps to show in satellite imagery.

---

## 4. The depth question: how deep is the kill zone in 2028, and does the front move?

The measured series is: 3–5 km in 2023; roughly 10–15 km as the Drone Line doctrinal target in 2025; 20–25 km in mid-2026 with a corps commander expecting 30 km by year end. Rubicon works the 10–40 km band behind the Ukrainian front; Ukraine's middle-strike campaign reaches 100 km.

Extrapolating the drivers rather than the curve — fiber at 50 km, relays adding 40–150 km, mid-range strike at 100–150 km, all of it cheaper each year — gives *[extrapolation]*:

| Band | End-2026 | End-2027 | End-2028 |
|---|---|---|---|
| Nothing moves in daylight | 20–30 km | 30–40 km | 40–60 km |
| Vehicles die reliably | 30–50 km | 50–80 km | 80–120 km |
| Fixed logistics nodes hunted | 100 km | 150 km | 200–300 km |

At 40–60 km, a divisional rear is inside the tactical kill zone. Everything an army does to prepare an offensive — assemble, stockpile, bring bridging forward — happens inside the enemy's cheapest weapons' reach. **The default answer is that the front stays static and the war is decided by industrial and infrastructure attrition.**

The exception, and it is the interesting one for a game, is the **bubble**. Manoeuvre returns where an attacker can generate a local, temporary, expensive counter-drone envelope — massed EW, turret vehicles, interceptor batteries, decoy screens, smoke — and push a robotic-first assault through it before the defender re-tasks his own drones. Bubbles are local, they cost more than the ground they take, and they collapse in hours. That is 2028 manoeuvre: not a breakthrough, a **raid with a lifespan**.

---

## 5. Three fronts in 2028

### 5.1 "Static Drone Siege" — the baseline, 60% likely

Two exhausted armies, a 50 km mutual kill zone, no manoeuvre. Both sides underground. Logistics moves at night by robot and motorcycle through covered routes; a truck in the band in daylight is a statistic. The tactical fight is a permanent air-to-air campaign for the ISR layer — whoever holds the recon picture for a sector owns its fires — so both sides spend most of their effort on interceptors hunting each other's eyes. Ground changes hands in tree lines, taken by robot platoons under bubbles that last an afternoon. The war is decided 300 km back, where deep strike burns refineries, drone plants and rail and the magazine war is fought. Infantry lives in two-to-four-man holes, rotated by robot. Tanks appear a dozen at a time, at night, for eight minutes.

**Game reading:** the default skirmish map. Slow front, hot air, deep economy. The fun is in the recon duel and the logistics duel, not the front line.

### 5.2 "Robotic Manoeuvre Returns" — 25% likely

One side wins the autonomy race by eighteen months. Its interceptor swarms clear the sector's air, its turret and EW vehicles move *with* the assault, and its robotic assault companies exploit at 15 km a day because it has stopped paying for casualties. Manoeuvre returns not because drones got weaker but because one side can generate a moving bubble and the other cannot re-task fast enough. What ends it is deception at scale (E3) plus mines, which are indifferent to how clever the robot is — typically as a 60 km advance that stalls when the attacker outruns his own relay grid and bubble logistics.

**Game reading:** the campaign's late-game payoff, and the answer to "does this RTS ever stop being a siege". It must be earnable through the autonomy tier and losable to a decoy-and-mine defence.

### 5.3 "NATO Flank Contingency" — 15% likely, and a different game

A short, violent, mostly-air fight in the Baltics. The differences are the point: real combat aircraft and cruise missiles; heavy peer EW and cyber; satellites on both sides; an eastern-flank grid (E13) that works but has seams at national borders; and neither side carrying Ukraine's or Russia's tolerance for attrition. Drone mass is smaller and quality higher, and the decisive events are the first 72 hours — whether the grid holds a saturation raid mixing decoys, jets and cruise missiles, and whether national sensor networks talk to each other under jamming. Ground fighting is short-range, urban, infrastructure-focused. The sea matters immediately: cables, ports, USVs in a confined basin.

**Game reading:** the second campaign, or an asymmetric multiplayer mode. Higher unit cost, smaller counts, shorter matches, and an explicit seam mechanic where allied sensor coverage has gaps the player must physically plug.

---

## 6. Game translation

### 6.1 Tier 4: Release Authority

Tiers 1–3 in the existing design are link tiers: radio, mesh/relay, fiber/satellite. **Tier 4 is not a link — it is permission.** It is unlocked at a structure called the **Mission Server** (a soft, expensive, hunt-worthy building) and it is gated by a one-time, irreversible **doctrine choice**, not by money:

- **On the Loop.** Every drone squadron gains an *Autonomous* stance. In that stance the squadron consumes **no pilot slot**, operates normally inside EW, and runs a standing order inside a player-drawn **Engagement Box** (target class + area + duration). Cost: a per-engagement **misidentification roll**. It will hit decoys. It will occasionally hit your own vehicles. It will not distinguish a supply truck from a captured one.
- **In the Loop.** Pilot slots remain mandatory, but the faction gains a permanent **+25% strike accuracy**, immunity to decoys, and access to Tier-4 *human* upgrades: remote pilot pools (pilot slots become an off-map resource that cannot be killed), a second stance on every multirole squadron, and faster re-tasking.

One choice, whole-match consequences, readable in one sentence. This is the thesis-4 mechanic — "autonomy trades a pilot slot for target-selection error" — made into a tech tier.

### 6.2 Era modifiers

Campaign-scope rule sets that re-tune the same roster. Skirmish exposes them as lobby options.

| Era | Setting | Rule changes | What it does to the loop |
|---|---|---|---|
| **I. Pilot War** (2026) | Baseline | Pilot slots scarce, EW strong, kill zone 12 map-units deep, no autonomy | Teaches the core loop: recon gates fires, links gate drones |
| **II. The Loop Closes** (2027) | Tier 4 available | Autonomy unlocked; **Decoy Kit** becomes a core buildable and doubles in effect; misidentification enabled | Deception becomes a build order, not a gimmick |
| **III. The Magazine War** (2027–28) | Attrition | Drone costs −40%, build times −50%, but **interceptor and turret coverage doubles**; deep-strike damage now hits the opponent's *production rate* | Mass stops working by itself; you must break factories, not armies |
| **IV. The Relay Grid** (2028) | Deep | Motherships, balloon relays and remote pilot pools available to both sides; map-wide **Link Layer** overlay; kill zone 25 map-units deep | Range is a network you build and the enemy can shoot down |
| **V. Flank Contingency** (2028 NATO) | Alt-campaign | Unit costs ×3, counts ÷3, manned aircraft and cruise missiles enter, match length −30%, allied **sensor seams** as map objectives | Converts the siege game into a fast, brittle, high-stakes fight |

Eras III and IV stack. Era V replaces I–IV.

### 6.3 Escalation-to-content map

| # | Escalation | Game content | Effect on the loop |
|---|---|---|---|
| E1 | Closing the Loop | **Rule:** Autonomous stance + Engagement Box drawing tool; **Structure:** Mission Server | Removes the pilot-slot cap on your army in exchange for a friendly-fire and decoy tax |
| E2 | Defensive autonomy | **Upgrade:** *Battery Autonomy* on interceptor batteries — one crew supervises the whole battery, +2 simultaneous engagements | Air defence scales with money instead of crews; makes mass raids survivable |
| E3 | Decoy economy | **Unit:** Decoy Kit (cheap, deployable, wrecks autonomous strikes, useless vs piloted); **Upgrade:** *Classifier Retrain* (48s research that nullifies enemy decoys until they re-pattern) | A real rock-paper-scissors against the autonomy player, with a timer on it |
| E4 | Jet ladder | **Unit:** Jet OWA drone (fast, expensive, ignores first-gen interceptors); **Upgrade:** *Jet Interceptor* | Vertical escalation inside the existing air-defence duel; forces radar investment |
| E5 | Fiber long and heavy | **Upgrades:** *Long Spool* (+60% fiber range), *Fiber UGV* (robot immune to jam-stall), *Fiber Bomber*; **Counter-unit:** Fiber Hunter (finds the tether, reveals the launch site) | Makes fiber a faction-defining line rather than one unit, and gives the enemy a way to punish it |
| E6 | Robot assault company | **Unit:** Assault UGV (armed, slow, cheap, no casualty cost); **Rule:** robots cannot capture a structure alone — one infantry squad must arrive to hold it | Robotic push, human consolidation. Keeps infantry precious and relevant |
| E7 | Relay grid | **Structure/Unit:** Balloon Relay (static, high, cheap, visible), Mothership (mobile carrier+repeater); **Rule:** child drones drop to last-order autonomy when the parent dies | Range becomes a built object you can lose catastrophically |
| E8 | Satellite parity | **Upgrade (mesh faction):** *Sovereign Constellation* — late, expensive, grants uplink control and removes the relay dependency | Mirrors the asymmetry so the mesh faction has a win condition on the link ladder |
| E9 | Turret belt | **Structure:** AI Turret (cheap, autonomous, ~350 m, ammunition upkeep, radar emits) | Static defence that actually works, and gives loitering munitions a job |
| E10 | Directed energy | **Structure:** Laser Node — infinite magazine, one target at a time, **effectiveness halves in rain/fog/smoke**; **Structure:** HPM Emitter — cone, very short range, damages friendly drones in the cone | Anti-mass tool with two honest, legible weaknesses |
| E11 | Magazine war | **Rule (Era III):** deep strike damages the opponent's *build rate* for 90s per hit; production structures are buildable underground at 2× cost | Makes the strategic layer matter in a tactical match without a second map |
| E12 | Operator ceiling | **Resource:** Pilot Slots as a visible, capped, killable pool; **Upgrade:** *Remote Pool* (moves them off-map, un-killable, +30% cost) | The scarcity the whole design hangs on, made into a number in the sidebar |
| E13 | NATO flank grid | **Era V rules:** allied sensor coverage with **seams**; capturing a seam node stitches two coverage zones | Gives the alt-campaign a distinctive objective type |
| E14 | Littoral robots | **Unit:** USV Mothership (rockets, air defence, launches FPVs); **Rule:** coastal maps only | Showcase unit; one map family, not a whole tech tree |
| E15 | Buried front / armour | **Rule:** structures and positions can be **Dug In** (−60% damage, −50% build speed, immune to strafing, vulnerable to fiber FPV and bunker payloads); **Upgrade:** *Hard-Kill APS* on tanks (defeats the first 2 FPVs per minute) | Gives tanks a reason to exist and gives fiber FPVs their signature job |

### 6.4 Deliberately kept out

These are real, and they would make the game worse.

1. **Spectrum micro-management.** Frequency hopping, band selection, an EW minigame. The real thing is an invisible, laggy guessing game. Ship EW as a visible bubble with a radius and a counter.
2. **Fiber tether physics.** Simulated snagging on individual trees and wires is unreadable at RTS camera height and infuriating. Model it as a flat *Tether Risk* per second in forest and urban tiles, shown as an icon.
3. **Hundred-unit swarms with individual control.** A real swarm is six drones coordinating. A hundred selectable icons is unclickable. A squadron is 3–6 drones and one icon, always.
4. **Per-drone battery and fuel.** Endurance is already range. A second attrition clock is bookkeeping.
5. **A war-crimes or morality meter.** Autonomy's price here is *misidentification*, paid in lost material. A guilt slider attached to an ongoing war is in poor taste and is not a mechanic.
6. **Realistic altitude layering.** Four bands with separate envelopes is a simulation, not a C&C. Two layers: *low* (tactical) and *high* (recon, relays, deep strike), visually distinct.
7. **Invisible fiber threats.** Fiber FPVs emit nothing, which in a game means unseeable death. Always draw the tether on the ground so the player can trace it back.
8. **Named pilots with fatigue and morale.** Pilot Slots stay an abstract pool; a crew roster drags the game toward a management sim.
9. **Real insignia, formations, named people or casualty figures.** Per the brief: archetypes only.
10. **A separate strategic campaign map.** Deep strike hits the opponent's build rate inside the tactical match (Era III). One map, one screen, one loop.

---

## 7. Open questions

1. **Does the autonomy doctrine choice split the factions or cut across them?** Recommendation: cut across, so the matchup matrix is four-by-four rather than two-by-two. Needs playtesting.
2. **Is misidentification fun, or just feel-bad?** It must be legible — distinct sound, red marker on the wrongly-killed unit, running counter in the sidebar. If testers read it as unfairness rather than as a price, replace it with a flat accuracy penalty against decoy-screened targets.
3. **How deep is the map?** A 50 km kill zone with 150 km relay reach does not fit one screen at C&C scale. Either compress hard (recommended: 1 map-unit = 250 m, map 40x40 units = 10 km) and scale all ranges down, or fire deep strike at off-map coordinates from a sidebar panel.
4. **Do Eras belong in multiplayer at all**, or do they fragment the ladder? Recommendation: Eras I+II ranked by default, the rest as lobby options.
5. **The Brave1 points economy** (kills buy drones) is specified in `spec-gameplay.md`; Era III's production-rate damage must not double-count with it.

---

## 8. Sources

All URLs below were returned by searches run September 2026, or appear in the source list of `kill-zone-bestiary.html`. Where a page could not be fetched directly, the claim is drawn from the search result summary and is marked in-text.

**Kill zone depth, front, and doctrine**
- https://euromaidanpress.com/2026/07/03/ukraines-eastern-kill-zone-is-25-km-deep-corps-commander-expects-30-by-years-end/
- https://militarnyi.com/en/articles/the-kill-zone-of-modern-warfare-size-and-structure-control-and-means-of-destruction-survival-and-shifting-the-lines/
- https://dronexl.co/2026/02/23/kill-zone-ukraine-frontline-drone/
- https://mod.gov.ua/en/news/drone-line-implementing-a-new-warfare-doctrine
- https://www.forbes.com/sites/davidkirichenko/2026/07/07/how-russia-learned-to-adapt-to-drone-warfare/
- https://kyivindependent.com/analysis-how-ukraines-new-middle-strike-drone-campaign-aims-to-strangle-russian-logistics/
- https://oklahomawatch.org/2026/06/09/have-70-of-casualties-in-the-russia-ukraine-war-been-caused-by-drones-as-rep-tom-cole-claimed/

**Autonomy, swarms, and the human-on-the-loop debate**
- https://smallwarsjournal.com/2026/06/12/line-crossed-fully-autonomous-drones-kill-russian-soldiers/
- https://smallwarsjournal.com/2026/08/17/fully-autonomous-drones-reportedly-kill-in-ukraine/
- https://nationalinterest.org/blog/buzz/ukraines-drones-can-now-kill-without-human-in-loop-sa-061226
- https://lieber.westpoint.edu/beyond-human-loop-battlefield-maps-drone-autonomy/
- https://lieber.westpoint.edu/whose-decision-was-it-drone-swarms-accountability-gap-ukraine/
- https://spectrum.ieee.org/amp/autonomous-drone-warfare-2676377272
- https://www.csis.org/analysis/how-russia-building-sovereign-drone-ecosystem-ai-driven-autonomy
- https://www.forbes.com/sites/vikrammittal/2026/04/08/drone-swarms-could-be-russias-answer-to-ukrainian-kill-zones/
- https://mezha.net/eng/bukvy/autonomous_drone_swarms_are/
- https://news.un.org/en/story/2026/08/1168196
- https://dig.watch/processes/gge-laws
- https://davisvanguard.org/2026/09/un-lethal-weapons-talks-conclude/

**Interceptors, jets, and the magazine war**
- https://thedefensepost.com/2026/07/27/ukraine-shahed-jet-killer/
- https://thedefender.media/en/2026/07/jet-powered-shaheds-intercept/
- https://militarnyi.com/en/news/jet-powered-shahed-drones-remain-a-challenge-for-interceptor-drones/
- https://militarnyi.com/en/blogs/interceptor-drones-jet-drones-arms-race/
- https://www.pravda.com.ua/eng/articles/2026/05/19/8035370/
- https://www.armyrecognition.com/news/aerospace-news/2026/ukraine-begins-mass-production-of-skyfall-600-km-strike-drone-as-jetkiller-shahed-interceptor-debuts
- https://news.liga.net/en/politics/news/ukraine-produces-up-to-1000-interceptors-per-day-but-there-are-not-enough-operators-zelensky
- https://www.kyivpost.com/post/83689
- https://www.cnn.com/2026/08/31/europe/ukraine-new-interceptors-russian-drones-intl
- https://isis-online.org/isis-reports/monthly-analysis-of-russian-shahed-136-deployment-against-ukraine
- https://dronexl.co/2026/01/19/russia-shahed-drone-production-surge/
- https://quwa.org/iran/military-news-iran/shahed-drone-irans-attrition-weapon-and-the-cost-exchange-crisis-of-2026/
- https://insideunmannedsystems.com/counter-uas-the-price-of-the-shot/
- https://www.twz.com/land/cheap-interceptor-drones-proven-in-ukraine-protected-u-s-troops-against-iranian-shaheds
- https://www.pravda.com.ua/eng/news/2026/04/30/8032528/

**Fiber optic links and their counters**
- https://militarnyi.com/en/news/ptashka-drones-achieves-50-km-range-with-successful-fiber-optic-drone-use/
- https://optics.org/product-announcements/a-french-optical-fiber-tether-spool-extends-drone-range-to-50-km
- https://oboronka.mezha.ua/en/rosiya-zapuskaye-fpv-dron-iz-kilcevim-krilom-309782/
- https://defence-blog.com/uk-launches-anti-fibre-optic-drone-program/
- https://vgi.com.ua/en/nets-fire-teams-and-lasers-inside-the-fight-against-fiber-optic-drones/
- https://www.army.mil/article/287737/fiber_optic_drones_posing_a_significant_c_uas_challenge

**Ground robots**
- https://www.defenseone.com/technology/2026/05/ukrainian-ground-robot-defended-position-russian-assault-six-weeks/413642/
- https://www.armyrecognition.com/news/army-news/2026/ukraine-uses-ground-robots-and-drones-to-capture-russian-positions-without-troop-losses
- https://www.forbes.com/sites/davidhambling/2026/07/14/ukraine--carries-out-first-ever-robotic-amphibious-assault/
- https://www.forbes.com/sites/davidhambling/2026/08/03/ukraine-carries-out-aerial-assaults-dropping-robots-from--drones/
- https://thedefensepost.com/2026/08/28/ukraine-kalashnikov-ground-robots/
- https://www.defensenews.com/unmanned/2026/04/24/ukraine-to-field-25000-ground-robots-in-push-to-replace-soldiers-for-frontline-logistics/
- https://thedefensepost.com/2026/08/25/ukraine-ground-robots-missions/
- https://foreignpolicy.com/2026/04/13/russia-ukraine-war-drones-ground-robots-ugvs/

**Motherships, relays, satellite**
- https://www.twz.com/air/fpv-drone-motherships-that-also-relay-their-signals-offer-huge-advantages
- https://www.armyrecognition.com/news/aerospace-news/2026/russia-converts-geran-2-into-fpv-drone-carrier-for-deep-strike-attacks-in-ukraine
- https://en.defence-ua.com/weapon_and_tech/what_first_sighting_of_ukrainian_fp_12_mothership_carrying_starlink_enabled_fpv_drones_reveals-18360.html
- https://aerospaceglobalnews.com/news/ukraine-a-32-ultralight-drone-mothership/
- https://www.forbes.com/sites/vikrammittal/2026/08/16/ukraine-uses-balloons-and-starlink-to-extend-the-range-of-fpv-drones/
- https://www.calibredefence.co.uk/mesh-networks-how-russia-is-increasing-the-range-of-its-drones/
- https://united24media.com/latest-news/russia-is-building-its-own-starlink-and-its-already-operating-over-ukraine-17975
- https://www.techradar.com/pro/russia-to-launch-rassvet-starlink-rival-within-days-with-318-strong-satellite-constellation-expected-by-2028
- https://militarnyi.com/en/news/russia-launches-second-batch-of-satellites-for-starlink-alternative/
- https://keeptrack.space/deep-dive/eutelsat-in-ukraine

**Turrets, directed energy, counter-UAS**
- https://nationalinterest.org/blog/buzz/ukraines-ai-driven-sky-sentinel-turret-rewriting-air-defense-as-we-know-it-bw-120325
- https://en.defence-ua.com/news/us_marines_buy_bullfrog_anti_drone_turret_after_video_shows_it_downing_shaheds_could_be_tested_in_ukraine-15994.html
- https://www.shephardmedia.com/news/air-warfare/us-military-tests-ai-powered-cuas-turret-against-drone-swarms/
- https://interestingengineering.com/innovation/ukrainian-anti-drone-laser-system
- https://rubryka.com/en/2026/01/20/mikrohvylovoyi-zbroyi-proty-droniv/
- https://en.defence-ua.com/weapon_and_tech/combat_lasers_and_microwaves_are_not_ready_for_ukraine_yet_why_these_weapons_remain_impractical-17096.html
- https://www.nationaldefensemagazine.org/articles/2026/1/20/counterdrone-mission-seen-as-killer-app-for-directed-energy

**NATO eastern flank**
- https://euro-sd.com/2026/03/articles/exclusive/49854/europes-drone-wall-ready-eddi-go/
- https://epthinktank.eu/2025/10/23/eastern-flank-watch-and-european-drone-wall/
- https://www.defensenews.com/global/europe/2026/05/05/nato-nations-size-up-an-interceptor-drone-bazaar-where-low-price-is-everything/
- https://defenceleaders.com/news/lithuania-fast-tracks-drone-interceptors-to-counter-baltic-airspace-threats/
- https://militarnyi.com/en/news/five-nato-countries-agree-on-joint-development-of-affordable-interceptor-drones/
- https://balticsentinel.eu/8515923/europe-s-answer-to-shahed-poland-unveils-production-line-for-long-range-hornet-attack-drone
- https://www.armyrecognition.com/news/aerospace-news/2026/nato-approves-40-billion-counter-drone-initiative-to-defeat-low-cost-uav-threats
- https://www.stripes.com/branches/army/2025-11-18/poland-merops-counter-drone-19809718.html
- https://mwi.westpoint.edu/want-to-maximize-drone-integration-in-close-combat-create-a-professional-drone-specialization-inside-the-infantry/

**Naval and littoral**
- https://www.navalnews.com/naval-news/2026/09/worlds-first-naval-drone-duel-results-in-ukrainian-success/
- https://united24media.com/latest-news/us-and-ukraine-transform-magura-naval-drones-into-ai-powered-shahed-interceptors-16506
- https://www.hisutton.com/Ukrainian-USVs-Russo-Ukraine-War.html
- https://www.edrmagazine.eu/fleet-2026-rosoboronexport-presents-a-line-of-russian-usvs-and-uuvs

**Hardening, armour, organisation**
- https://thedefensecircuit.com/2026/05/28/as-drones-fill-the-sky-armies-head-underground/
- https://contactairlandandsea.com/2026/06/11/the-future-of-tanks-have-they-been-made-redundant-by-drones/
- https://www.orfonline.org/expert-speak/the-tank-in-the-drone-age-manpower-technology-and-attrition
- https://www.fpri.org/article/2026/06/inside-rubicon-the-structure-of-russias-elite-drone-center/
- https://jamestown.org/rubicon-reveals-limits-of-russias-drone-centralization/
- https://carnegieendowment.org/research/2026/08/russias-long-drone-shadow
- https://www.hudson.org/security-alliances/nato-not-ready-war-assessing-military-balance-between-alliance-russia-can-kasapoglu

**Bestiary base (2026 research file)** — full source list in `kill-zone-bestiary.html`, section "Sources".
