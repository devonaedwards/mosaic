# Deep strike: the target set and the supply chain behind the front

Research note for KILL ZONE. Compiled 14 September 2026.

**Scope.** What there is to hit behind the front line, what hitting it achieves,
how long it stays hit, and how much of that survives being turned into a
real-time tactical game with a 30–60 minute match.

**Not in scope, because it is already written.** Netted road corridors, UGV
logistics, the 20–25 km kill zone, the last-kilometres problem and casualty
evacuation are in `ground-logistics.md`. This document starts where that one
stops — about 25 km behind the line — and runs to 2,500 km.

## How to read this document

- **[M] Measured** — satellite imagery, a published engineering figure, a spec.
- **[R] Reported** — a specific figure in press or institutional reporting,
  attributed to a named source.
- **[C] Claimed** — asserted by a belligerent or vendor. Often true, never audited.
- **[I] Inference** — my arithmetic on [M]/[R]/[C], assumptions stated.
- **[E] Estimate** — my number for the designer. Not research. Flag it in code.
- **[U] Unestablished** — looked for, not found. Do not fill in from memory.

**Sourcing caveat.** As in the other documents in this set, the egress proxy
blocked direct page fetches for most defence and journalism domains. Everything
below is **snippet-sourced** — search-result summaries, not read pages — unless
marked otherwise. Good enough to pick a number and defend it; not good enough to
quote in print.

---

## Summary: the ten things that matter

1. **Fuel is the only deep-strike category with a demonstrated effect on the
   front — and it took about a year to arrive.** Campaign restart August 2025;
   regional shortages within weeks; national rationing May–July 2026; front-line
   fuel rationing reported July–September 2026 [R].
2. **Rail matters more than road here, and the designer probably does not know
   it.** Russian ground forces cannot sustain themselves much beyond ~160 km from
   a railhead [R]; rail is the only mode that moves the tonnage. But rail is also
   the most repairable thing on the list — track is fixed in hours [R], while
   traction substations, transformers and locomotives are not (12–18 month lead
   times on high-voltage transformers [R]). **Hit the electricity, not the
   rails.**
3. **Roads cannot practically be destroyed. They are denied instead, and denial
   is cheap, fast and reversible.** The R-280 "Highway of Death" saw military
   traffic fall by up to 71% [R] without a metre of roadbed being removed.
4. **Bridges are the most over-rated target on the list.** A 90–120 kg warhead
   does not drop a span; repeated hits make a bridge unusable, not absent.
   Pontoons substitute in ~3 hours [R]; permanent repair took 57 days even for
   the Kerch road spans [C/R].
5. **Depots are the highest-yield single strike and the fastest adapted to.** One
   Toropets-scale hit removed a claimed two to four months of ammunition [C]; the
   counter — disperse, hold stocks 200+ km back [C] — costs the defender about a
   third of throughput at 145 km and two thirds at 290 km [R].
6. **Refinery damage is a squeeze, not a break**, and storage is not capacity.
   Distillation comes back in three to seven weeks [R]; the durable damage is to
   catalytic units needing Western parts nobody will sell [R]. A burned tank farm
   is lost inventory; a wrecked CDU is lost production.
7. **Concealment does almost nothing for a fixed site** — its coordinates are
   already known. What works is hardening, dispersal within the fence and point
   defence, all built at infrastructure scale by both sides [R].
8. **Penetration is roughly 20–30%** for Ukrainian long-range drones against
   Russian air defence [R, snippet]. That ratio governs deep-strike economics more
   than anything else.
9. **The offence/defence asymmetry in the supply chain is real but smaller than
   it looks, and it reverses in salients.** A defender holding a pocket has the
   longer, more exposed line — Myrnohrad is the case [R].
10. **Almost none of this belongs inside a match.** Section 9 names the three
    things that do.

---

## 1. The geography behind the front

`ground-logistics.md` gives three zones. The deep-strike picture needs five. All
distances are from the line of contact.

| Band | Depth | What lives there | What reaches it |
|---|---|---|---|
| Kill zone | 0–15 km (25 east, 30 projected) | Positions, 1–2 day caches | FPV, fibre FPV [R, ground-logistics] |
| Interdiction belt | 15–50 km | Transfer points, brigade trains, field depots | FPV, heavy multirotor, artillery, mid-range drones [R] |
| Operational ("middle strike") | 25–200 km | Railheads, army depots, repair shops, command posts, arteries | Mid-range strike drones, 50–300 km class [R] |
| Deep operational | 50–150 km — the Deep Strike Command Centre's declared reach from Mar 2026 [R, front-2026.md] | Airfields, army logistics, power nodes | Long-range one-way drones, cruise missiles |
| Strategic | 200–2,500 km | Refineries, arsenals, aircraft plants, bomber bases | FP-1 (1,600 km, 120 kg) and Liutyi class [R/C]; Omsk hit at 2,500+ km [R] |

Two geometry facts set everything else:

- **The railhead has retreated.** Kramatorsk, about 20 km from the front, was the
  iconic front-line rail stop; through-service was terminated indefinitely in
  November 2025 and the terminus moved west to Husarivka [R]. The rail-to-road
  transfer point is now a deep target, not a front-line one.
- **Depots have retreated further.** Ukrainian intelligence reported in 2026 that
  Russia was considering holding ammunition depots **more than 200 km** back,
  with forward field depots carrying **no more than two days** of stock [C] — a
  doubling or tripling of the truck leg.

**At twelve-to-one compression**, 200 km is about **16,700 map metres** against a
kill zone of 1,700–2,100. This is the most important design fact in the document:
**the deep-strike target set does not fit on a tactical map.** Section 9 deals
with what follows.

---

## 2. The target set, ranked by what striking it achieves

Ranked by demonstrated effect on the front, not by how satisfying the explosion
is.

| # | Category | Hard to damage? | Stays damaged | Propagates to the front? |
|---|---|---|---|---|
| 1 | **Refinery distillation and cracking units** | Medium — big and soft, but needs a real warhead on a specific vessel | 3–7 weeks (CDU) [R]; indefinite for sanctioned secondary units [R] | **Strongly — with a 9–12 month lag** |
| 2 | **Rail electrification and traction substations** | Low | Weeks to 12–18 months, depending on the transformer [R] | Yes, days to weeks |
| 3 | **Ammunition depots** | Low — sympathetic detonation does the work | Site gone; stock replaceable in months | Yes in weeks; absorbed by dispersal after |
| 4 | **Fuel storage and tank farms** | Very low — one hit burns the farm | Days to weeks; contents gone instantly | Locally; nationally absorbed |
| 5 | **Power substations serving industry and rail** | Low | Days (switchgear) to 12–18 months (autotransformers) [R] | Indirectly, via rail and production |
| 6 | **Roads — by denial, not destruction** | n/a, section 3 | Only while you keep paying | **Immediately** — and it stops when you stop |
| 7 | **Repair and assembly facilities** | Medium | Months; disperses into garages [R] | Slowly; absorbed well |
| 8 | **Bridges** | **High**, section 4 | Days (pontoon) to months (permanent) | Bounded; substitutes absorb most of it |
| 9 | **Command nodes** | Medium — main and reserve both known [R] | Hours to days | Contested; claims unverifiable |
| 10 | **Airfields** | High for sheltered aircraft; low for runways | Runway 4 h [M, USAF]. Airframes: forever | Yes for aircraft, no for the airfield |

Notes on the categories without their own section below.

**Repair and assembly.** Strikes on the 325th and 123rd Aircraft Repair Plants
are reported [R/C]; the armour equivalent is **[U]**. The instructive datapoint
runs the other way: after Russia struck Ukraine's tank plant in 2022, crews
repaired armour in junkyards and garages instead [R]. Heavy assembly is a real
target; *repair* disperses into the civilian economy. Model it as degradable, not
removable.

**Command nodes.** In 2026 Ukraine struck the **main and reserve** command posts
of a Russian formation in occupied Donetsk in one wave, SCALP debris found and a
JDAM-LR suspected on the second [R]. The design content is in that sentence:
serious formations run a main and an alternate, so killing one is a temporary
degradation. Model command kills as short tempo penalties, not capability
removal.

**Airfields.** Operation Spiderweb (1 June 2025) is the outlier that proves the
category: 41 aircraft claimed hit [C], 10–13 assessed destroyed [R], five bases
across five time zones, damage confirmed 4,300 km from Ukraine [R], from drones
smuggled in trucks; claimed value $7 billion [C]. Since 2024 Russia has built
reinforced concrete shelters at 14 airbases — Su-30/34/35 airframes do not fit
Soviet 2A13 shelters [R] — and by 2026 Ukrainian drones were reported hitting
through them [C/R]. The runway is the least durable damage in the document:
airfield damage repair restores three 750 lb-bomb craters in **four hours**
[M, USAF doctrine]. **Kill the aircraft, not the airfield.**

---

## 3. Roads: destroying versus denying

The designer's instinct — "most things can't take out roads, but some stuff can"
— is **half right, and the wrong half is the interesting one.**

**Destroying a road is a deliberate engineer task, not a strike mission.** A road
crater means drilling boreholes and placing tens of kilograms of cratering charge
per hole to a calculated pattern [R, US engineer doctrine] — something you do to
your own road as you retreat. Repair runs in hours: one engineer team filled two
large craters with more than 1,800 cubic feet of fill and concrete in "several
hours" [R]. Nothing in the 2022–26 reporting describes either side removing a
road by air attack. Roads are dirt and aggregate; they absorb explosions.

**Denying a road is cheap, fast, reversible and extremely common.** Four
mechanisms, cheapest first:

| Mechanism | Cost to impose | Duration | Reversed by | Conf. |
|---|---|---|---|---|
| **Wreck-blocking** — kill one vehicle in a defile, leave it | One FPV (~$400–$1,000, economics.md) | Hours to days | Recovery under observation | [R] — "a disabled truck can block a road, create congestion and leave stranded vehicles vulnerable" |
| **Scatterable mines, artillery-delivered** | One 155 mm RAAM round = 9 AT mines at 4–17.6 km | **Self-destruct ~4 h (RAAM-S) or >48 h (RAAM-L)** | Waiting | [R/M, US ordnance spec] |
| **Drone-delivered mining** | One mine-carrying quad sortie | Until cleared; no self-destruct | Clearance, hours to days | [R] — reported on the M-14 toward Chonhar; part of the highway closed, heavy trucks rerouted |
| **Persistent overwatch** | Standing commitment of crews and airframes | **Only while you pay** | Stopping | [R] |

Persistent overwatch dominates, and is the one the game should model. The
reference case is the **R-280 / M-14 "Highway of Death"** — the Rostov –
Mariupol – Melitopol – Crimea artery, attacked from mid-April 2026 under
Ukraine's declared **"logistics lockdown"**, a $113 million programme [R]:

- **483 Russian transport vehicles neutralised in a single day** (29 May) [C/R —
  an analyst tally; treat as an upper bound].
- **Military traffic down by up to 71%** [R]; the road **largely closed to
  civilian traffic since late May** [R].
- Flown by named units (412th "Nemesis") with Hornet and Morrigan-class
  fixed-wing drones — a **5 kg warhead**, 50 km range originally, extended to
  150–250 km with propulsion changes and Starlink terminals [R].

This sits inside the broader **"middle strike" campaign** launched in early April
2026 against the 25–200 km band, in which mid-range strike missions rose
**28-fold over a year** [R], 5,000–8,000 Ukrainian operators working against the
supply lines of 250,000+ Russian troops [R].

**The design point.** A road is not a structure with hit points. It is a *state*
with an upkeep cost: **closed**, **contested** (night movement, singletons,
losses), or **open**. Denial is a standing expenditure of sorties and crews, and
the road reopens the moment attention moves. That is a better mechanic than a
destructible road and it matches the evidence.

---

## 4. Bridges: the most attractive and most disappointing target

**Why they attract.** A bridge is a unique, mapped, immovable choke point that a
whole axis depends on; over 400 have been destroyed in Ukraine since 2022 [R,
aggregator — weak].

**Why they disappoint: weapon weight.** The Antonivskyi road bridge at Kherson
was struck repeatedly with HIMARS from July 2022 and **never dropped by rocket
fire** — the explosive
in a GMLRS warhead is not enough to bring down a span of that construction, and
the bridge's design distributes the load [R]. What the strikes achieved was a
deck perforated with holes: vehicles stopped, and the Russians went onto pontoon
ferries and weight-limited crossings [R]. One span did collapse on 22 August
2022, possibly assisted by ten ammunition-laden trucks on it at the time;
attribution between HIMARS and airstrike is contested [R].

For the game: **a 90–120 kg warhead perforates; it does not drop.** What actually
drops spans: **a demolition charge placed by hand** — the Bryansk rail bridge,
destroyed by explosion on 31 May 2025 onto a passenger train [R], classified by
Russian investigators as a terrorist act [C]; **a very large charge on the deck**,
as at Kerch in October 2022, two two-lane road spans into the water [R]; or
**sustained repetition** — Chonhar, June 2026, FP-2 and Behemoth one-way drones,
closures on consecutive days, **repairs estimated at up to a month, with 15–20
days needed merely to assess the structure** [R].

**How fast substitutes arrive.**

| Substitute | Emplacement | Capacity | Survivability |
|---|---|---|---|
| Ferry / pontoon rafts | Minutes to an hour | One or two vehicles at a time | High — nothing fixed to hit |
| **PP-2005 pontoon bridge** | **~3 hours** [R] | 268 m at 60 t [R] | **Killed within ~24 h by artillery** [R]; three destroyed on the Siverskyi Donets by 8 May 2022 [R] |
| Bailey / modular truss | 24–48 h for ~30 m; 3–4 h small [R] | Sustained | As any fixed bridge |
| Permanent repair | **57 days** at Kerch, crash pace [C/R]; up to a month at Chonhar [R] | Full | — |

**The honest shape:** dropping a bridge buys **hours** before a ferry, **~3
hours** before a pontoon, **one to two days** before a Bailey, **one to two
months** before the real thing. What it costs the defender in between is
*throughput and predictability*, not access — a pontoon is a lower-capacity,
weather-dependent, artillery-magnetic crossing at a known point. Worth having;
not the axis-severing event games usually make it.

---

## 5. Refineries and fuel: the slow squeeze, and the timing a game gets wrong

This is the best-evidenced deep-strike category in the whole war and the one the
designer is most likely to mis-time.

### 5.1 Scale

**42.74%** of Russian refining capacity disabled as of 4 July 2026 [C, Ukrainian
General Staff], against **"more than 20%"** from the IEA [R]. Russian refining at
a **24-year low** [R, Bloomberg, Aug 2026]; fuel production **down 25%
year-on-year** in June and running **~20% below domestic demand** [R, Reuters];
**194 attacks** on refineries since the start of 2026, all 11 of the largest hit
[R/C]; **$13.5 billion** cumulative industry losses since August 2025 [R/C].

The spread between 42.7% and "more than 20%" is the honest measure of
uncertainty: **between a fifth and two fifths, both ends in circulation.** One
snippet gave "38% idle as of September 28" on an internally inconsistent date; I
have not used it.

### 5.2 Distillation versus storage — the distinction that matters

**Storage** (tank farms, terminals, depots) is trivially easy to destroy and
nearly meaningless strategically. Four drones at Labinsk in March 2026 set 18 of
20 tanks alight across ~3,000 m² [R]; a strike on two Smolensk depots destroyed a
claimed 26,000 m³ of fuel [C]. Spectacular, and *inventory* — replaceable from
production, in welded steel cylinders that rebuild in weeks.

**Distillation and conversion capacity** is the real target, and there are two
tiers inside it:

- **Crude distillation units (CDUs)** — big, repairable, and Russia is good at
  it. Ryazan's CDU-6 and CDU-4 were shut by a September 2026 strike, repairs "up
  to several weeks"; a July strike there suspended operations for **nearly three
  weeks** [R]. Kuibyshev began major repairs 1 July, resumed **21 August** (about
  seven weeks), and was hit again on 28 August [R]. Omsk's CDU-10 is about **38%
  of that refinery's throughput**, over 24,500 t of crude a day [R].
- **Secondary units — catalytic crackers, reformers, membranes, catalysts.**
  These make *gasoline*, and Russia cannot fix them. NORSI's cat cracker has been
  down since January with gasoline output cut 40%, because the only firm able to
  repair it was UOP, which left in 2022 [R]. Western licensors (UOP, ABB)
  modernised the whole Russian refining base and no longer fill orders [R];
  catalytic units and membranes have finite lives and no non-Western
  substitute [R].

**For the game:** two damage tracks per refinery. *Throughput damage* heals in
3–7 weeks. *Conversion damage* does not heal on any timescale a campaign cares
about. A player who keeps hitting CDUs is on a treadmill; a player who hits
crackers is banking permanent losses. That is a real decision, and it is true.

### 5.3 The lag — be concrete

This is the thing to get right. Reconstructing the timeline from the reporting:

| When | What | Marker |
|---|---|---|
| **Aug 2025** | Campaign restarts — a dozen-plus strikes 2–24 Aug, 14 refineries hit that month | [R] |
| **+2–4 weeks** | **First shortages at the periphery** — Crimea and the Far East, the ends of the network | [R] |
| **May 2026, +9 mo** | First national wave of shortages | [R] |
| **Jul 2026, +11 mo** | All regions; **two thirds of 83 federal subjects rationing** in some form. Export bans: gasoline (Apr), jet fuel (1 Jun), diesel (8 Jul) | [R] |
| **Jul–Sep 2026, +11–13 mo** | **Front-line effect reported**: unit deliveries cut up to 30%, some units on 20 litres a day, fuel issued only to military-plated vehicles while up to 90% of front-line vehicles are privately owned | [C/R — Ukrainian-sourced reports of Russian soldier complaints; the litre figure is [C]] |

**The rule, as plainly as I can put it:** the lag from strike to front-line fuel
effect is **not days and not one turn**. It is **weeks** for a local peripheral
civilian effect, **six to twelve months** of *sustained campaign* for a national
one, and **nine to thirteen months** before a rifleman notices.

The reason is not transit time. The system has **four buffers in series** —
commercial inventory, strategic reserve, export diversion, and rationing
civilians before soldiers. The military sits at the *end* of that queue, which is
why petrol-station queues precede military effects by the better part of a year.

A game where one refinery strike cuts enemy fuel next turn has modelled something
that has never happened. A game where fifty strikes across a long campaign slowly
raise movement costs and lower sortie rates has modelled it exactly.

### 5.4 Strike economics

**Penetration is 20–30%** of Ukrainian long-range drones against Russian air
defence [R, snippet — single source, and the most load-bearing soft number here].
Raids run large: 555 drones claimed downed in one night around the June 2026
Moscow refinery strike [C], an 822-drone raid in August [C], "up to 1,000 units
per attack" [R]. The FP-1 is about **60% of strikes inside Russia** as of August
2026 — ~100 built a day, 120 kg warhead, 1,600 km range, ~$55,000 each (see
`economics.md`) [R/C]; Omsk was hit at over 2,500 km [R].

**[I]** At a 25% leaker rate and $55,000 a drone, each *arriving* warhead costs
about **$220,000** — before the ones that arrive and miss the right vessel.
Against a repair bill in the tens of millions that is favourable but not free,
and it explains why this is fought in hundred-drone raids, not precision singles.

---

## 6. Depots and the dispersal transition

**The big-depot strike is the highest-yield single event in the document.**
Toropets, 18 September 2024: over 100 drones against a facility holding up to
**30,000 tonnes** of munitions [R/C], a detonation compared to 750,000 artillery
shells at once [C], with Ukrainian assessments claiming **two to four months of
warfighting stock** destroyed [C]. Satellite imagery confirmed the devastation
[M].

**The counter is dispersal, and it is well documented.** After the 2024 campaign
Russian forces "improved air defence coverage of depots, dispersed ammunition
stocks, and adjusted command arrangements" [R]; strikes "forced Russia to move
depots farther from the front, lengthen supply lines, and disperse stockpiles,
all of which increased friction and reduced the efficiency of resupply" [R],
leaving "smaller, less centrally coordinated distribution networks" [R]. By 2026
Ukrainian intelligence reported a **200 km minimum standoff** under
consideration, with forward field depots holding **no more than two days** [C].

**What dispersal costs the defender.** This is the good part, because it is
quantified:

- **Throughput falls with distance**: **−33% at ~145 km**, **−66% at ~290 km**
  [R, snippet]. Pushing depots from 100 to 200 km roughly **halves** what the same
  truck fleet delivers [I].
- **Trucks run out of reach.** The Russian army is assessed as lacking trucks to
  sustain itself beyond **~160 km from railhead dumps** [R]; a 200 km standoff
  puts the depot past that, forcing more trucks, more rail, or an intermediate
  transfer point — itself a new targetable node.
- **Reaction time collapses.** Two days forward makes a two-day interruption a
  crisis.
- **Scale.** Roughly **200 kg per soldier per day**; a combined-arms army of
  ~20,500 needs **~4,100 t/day** [R, RAND]. The Russian MoD claimed 10,000–15,000
  t of ammunition and fuel daily theatre-wide in 2023 [C].

**Why the transition is a game state, not a nerf.** Dispersal is not merely
harder to kill; it **changes the game the attacker is playing.** Before: a few
enormous, known targets — strikes rare, expensive, occasionally decisive,
reconnaissance cheap because nothing moves. After: many small, unhardened,
*relocating* targets holding two days each — no single strike matters,
reconnaissance becomes the binding constraint, tempo has to rise to match. That
is a phase change the player should be able to *cause* and then adapt to, and a
campaign layer where enemy depot posture flips after N successful strikes —
defender paying a permanent throughput tax, attacker losing the knockout — is one
of the best mechanics in this research set.

---

## 7. Camouflage, concealment and hardening of fixed sites

`ground-logistics.md` covers tactical netting over roads; `decoys-masking.md`
covers masking of *mobile* things. Fixed sites are a different problem.

**A fixed site cannot be concealed, because it is already on the map.** A
refinery, substation, bridge or arsenal has published coordinates, decades of
imagery and usually a Wikipedia article; netting and paint cannot make it
not-there. **Concealment at depth has value only against the terminal seeker, not
the mission planner** — it can make an autonomous drone's image match fail in the
last few hundred metres, and that is all.

What is actually built:

**Ukraine runs three formal protection levels for the power grid** [R]: level 1,
gabions, sandbags and anti-drone netting against shrapnel and debris — **90
facilities across 21 regions**; level 2, reinforced concrete around substations
and autotransformers — **22 substations, 63 autotransformers, 14 regions**; level
3, heavy "sarcophagi" and burial, including one substation in an underground
concrete bunker with a second under way. Ukrenergo reports **fewer than 20% of
its substations without anti-drone protection** [R], and concrete structures
taking hits with the equipment inside surviving [C].

**Russia — the same idea, applied to refineries and airbases** [R]:

**"Web"** modular metal screens (RT-Project Technologies with Standard-Elektrik)
claimed to stop a direct hit from a **200 kg drone at 250 km/h** [C, vendor];
anti-drone nets and metal frames around individual process units, and **walls of
shipping containers** at Novoshakhtinsk [R]; the **ZAK-30 "Citadel"**
anti-aircraft gun system explicitly for stationary infrastructure including
refineries [R]; hardened aircraft shelters at 14 airbases since 2024 (section 2).
The Russian government's own framing: protection is costly, idle plants cost more
[R, Meduza].

**What this buys.** Hardening changes the **weapon weight required**, not the
probability of being targeted. A 5 kg Hornet warhead that would wreck an exposed
transformer does nothing to one in a concrete box; a 120 kg FP-1 warhead
plausibly does. **Hardening sets a damage threshold, and everything below it is
wasted** — not a percentage reduction, and it should not be modelled as one.

**Decoys at depth: [U], and I think that is a real finding.** Decoy vehicles,
HIMARS, artillery and thermally-heated dummy soldiers are extensively documented
(see `decoys-masking.md`). I found **no reporting of decoy refineries,
substations or depots**. The reason is structural: a decoy works by being where a
real thing might plausibly be, and there is no plausible place to put a second
Ryazan refinery. **Decoys defend the relocatable; hardening and point defence
defend the fixed.** That rule falls out of the evidence rather than being imposed
on it.

---

## 8. Offensive versus defensive asymmetry in the supply chain

The designer asked for this specifically. Here is the honest answer: **the
asymmetry is real, it is roughly a factor of two to three in supply terms, and it
is much smaller than the casualty asymmetry it is usually confused with.**

### 8.1 The two sides of it

**The attacker pays** on five counts: the line lengthens behind him as he
advances; throughput falls with distance (−33% at ~145 km, −66% at ~290 km [R]);
the route is new and unimproved — no netting, no dug shelters, no repaired
bridges; he crosses both sides' minefields, including scatterable mines re-sown
behind him; and his new forward positions hold two days of stock with no
accumulated dumps [C].

**The defender gains** on four: the line shortens as he falls back, onto routes
he built; he falls back onto prepared positions, cached stock and repaired
bridges; interior lines let him shift reserves on shorter chords; and his rear is
his own rail network.

**The casualty exchange is the visible half, and a different quantity.**
Open-source estimates put Russian losses at **2:1 to 5:1** against prepared
defences in 2025–26 [R/I]; Estonian intelligence at ~**100 casualties per km²**
captured in January 2025 [C]; CSIS-derived figures for the first five months of
2026, when net gains nearly vanished, work out to **over 9,600 per square mile
(~3,700 per km²)** [R]. Net change **reversed** in spring 2026 — Russia down
~400 km² across April–May, Ukraine liberating 400–500 km² in February–March [R],
the Economist counting 31 km² of Russian gain in the 30 days to 30 June [R].

### 8.2 How big is it, really?

**[E] My number: a factor of about 2 to 2.5 on delivered supply per unit of
transport committed, at a 100 km differential in line length.** That is the
throughput-versus-distance curve (−33% at 145 km) applied symmetrically plus a
modest penalty for the unimproved route. It is *not* a factor of ten; a game that
makes attacking logistically impossible has over-corrected. The larger
asymmetry — the 2:1 to 5:1, the thousands of casualties per km² — is a **combat**
asymmetry, not a supply one. Keep them separate or the attacker is unplayable.

### 8.3 Where it reverses

Three places:

1. **Salients and pockets.** A defender holding a bulge has the *longer*, more
   exposed line while the attacker's guns sit on the chord. Pokrovsk–Myrnohrad is
   the case: Ukrainian logistics were pulled deeper to the rear because convoys
   could no longer run the old routes, backup routes had to be laid, and by
   October 2025 supply into Myrnohrad was "a stripped-down operation handled by
   drones and unmanned ground vehicles" [R]. "Complicated but functioning, no
   encirclement" [C] is what a reversed asymmetry sounds like from the inside.
2. **When the attacker captures infrastructure rather than ground.** Russia's
   rebuilt Rostov–Mariupol–Volnovakha–Donetsk railway, thirteen months building
   and opened August 2024, **cut its own logistics routes by 300 km** [R]. An
   attacker who takes a rail junction *shortens* his line by advancing.
3. **At the strategic level, entirely.** The deep striker pays almost nothing in
   logistics — a $55,000 drone launched from home. The defender pays for air
   defence, hardening, dispersal and repair across a continent. **In the deep
   fight the asymmetry runs the other way, and much larger than 2:1.**

---

## 9. What this should be in the game. Opinionated.

**Most of this does not belong in a match, and pretending otherwise will wreck
the game.** The arithmetic settles it. At twelve-to-one compression the kill
zone is 1,700–2,100 map metres — a map. The middle-strike band is 2,000–16,700 map
metres — many maps. A depot at 200 km standoff is 16,700; a refinery at 1,600 km
is **133,000 map metres**.

The *time* constants are worse than the distances. A match is 30–60 minutes; the
refinery lag is nine to thirteen months, the transformer lead time 12–18 months,
a CDU repair three to seven weeks. Even the fastest genuinely strategic effect
here — a depot strike changing what arrives at the front — takes weeks. **There
is no honest compression that puts a refinery campaign inside an hour of play.**
Try, and you get a button marked "bomb refinery" and a fuel bar that drops: false
about the subject and not even fun.

So: **deep strike is a campaign layer.** Between matches the player allocates
long-range sorties against a target list, pays the 20–30% penetration tax, and
banks effects that arrive as **modifiers to future matches** — enemy fuel state,
reinforcement rate, artillery allowance, repair speed, air support. The lag is
the mechanic, not a bug to tune out: a strike made in match 3 shows up in match
9. That is unusual, memorable, and what actually happened.

### The three things that can be in a match

**1. Road denial as a live, contested state — not a destructible object.** The
strongest in-match import in the document. A road has three states — open,
contested, closed — and the player changes state by *spending*, not destroying: a
standing overwatch commitment, a mining sortie with a duration (four hours for
self-destructing munitions, indefinite for drone-laid), or a wreck left in a
defile. The defender clears it, nets it, or reroutes. It costs upkeep, reverses
when attention moves, and produces exactly the 71%-traffic-drop feel of the R-280
without deleting a metre of road. **Build this one first.**

**2. Bridges and the pontoon substitute, as a timer rather than a wall.** A
crossing belongs on a tactical map because both sides can reach it inside a
match — but it must behave correctly: light warheads perforate and impose a
weight limit, only a heavy charge or sustained repetition drops a span, and the
defender's answer is a pontoon emplaced in a fixed number of minutes at a
*visible, artillery-magnetic* point of his choosing. The race between demolition
and bridging equipment, then the fight over the substitute, is match-sized.

**3. The forward field depot with two days of stock.** The one piece of the
dispersal transition that fits on a map: a small, concealed, *relocatable* cache
at 5–20 km holding a couple of days of ammunition. Finding it is a reconnaissance
problem, killing it a strike problem, losing it a tempo penalty that bites inside
the match.

### Keep out of a match, and one structural note

Refineries, substations, railheads, arsenals, aircraft plants, airfields and army
command posts — each has a distance or time constant that breaks the match frame.
Put them on the campaign map, where slowness is a feature, and give every fixed
target there **two damage tracks, repairable and permanent**. That distinction
(CDU versus cracker; track versus transformer; runway versus airframe) recurs in
every category in this document and is what makes deep strike a strategy rather
than a treadmill.

---

## 10. What is genuinely uncertain

- **The true share of Russian refining capacity down.** 20% (IEA) to 42.7%
  (Ukrainian General Staff). Both numbers are in current circulation; the gap is
  the finding.
- **The 20–30% penetration rate.** Single-source, snippet-only, and the most
  load-bearing number in the deep-strike economy. Everything about strike costs
  scales with it.
- **Front-line fuel effects.** The "20 litres a day" and "deliveries cut 30%"
  figures come from Ukrainian-sourced reporting of Russian soldier complaints
  [C]. The *direction* is well corroborated; the magnitude is not.
- **Whether the fuel campaign has changed anything the front actually does.**
  Rationing, export bans and complaint are documented; a demonstrated drop in
  Russian offensive tempo attributable to fuel is **[U]**. The Soufan Center's
  "Russia can still absorb the cost" is the counter-case [R].
- **Armour repair plants** — the aircraft equivalent is reported, this is **[U]**.
- **Depot dispersal in numbers.** The 200 km standoff and two-day forward stock
  are one Ukrainian intelligence claim [C]; scale and implementation are **[U]**.
- **The throughput-versus-distance curve** (−33% / −66%): a clean, quotable figure
  from a single snippet I could not trace to its original study.
- **Decoys at fixed sites.** Asked for, nothing found. I have argued this is a
  real absence rather than a search failure, but it is an argument, not evidence.
- **Bridge repair times outside the two famous cases.** Kerch (57 days) and
  Chonhar (up to a month) are both strategic crossings repaired at crash pace;
  what an ordinary road bridge takes is **[U]**.
- **Command-node strike effects.** Every consequence claim is a general staff
  describing its own operation. All [C].

---

## Sources

All URLs below appeared in search results during this research. **None could be
fetched directly** — the session's egress proxy blocked the domains attempted — so
everything above derives from search-engine summaries of these pages. Verify
before quoting.

**Refineries, fuel and the Russian fuel crisis**
- https://en.wikipedia.org/wiki/2025%E2%80%932026_Russian_fuel_crisis
- https://www.kyivpost.com/post/79574 (43% of refining capacity claim)
- https://www.themoscowtimes.com/2026/08/03/russian-oil-refining-falls-to-24-year-low-after-ukrainian-drone-strikes-bloomberg-a93404
- https://www.themoscowtimes.com/2026/05/20/drone-strikes-force-central-russian-refineries-to-halt-or-cut-output-reuters-a92805
- https://www.themoscowtimes.com/2026/09/10/rosnefts-ryazan-oil-refinery-shuts-down-after-drone-attack-industry-sources-say-a93678
- https://www.themoscowtimes.com/2026/07/07/russias-largest-oil-refinery-halts-production-after-drone-attack-sources-say-a93190
- https://www.themoscowtimes.com/2025/08/20/fuel-shortages-hit-russias-far-east-as-ukrainian-strikes-take-refineries-offline-a90300
- https://carnegieendowment.org/russia-eurasia/politika/2026/06/russia-new-refinery-strike
- https://www.defense-aerospace.com/russia-refining-losses-what-can-be-counted/
- https://militarnyi.com/en/news/drone-strike-damaged-primary-and-secondary-processing-units-at-kstovo-oil-refinery-satellite-imagery/
- https://united24media.com/war-in-ukraine/ukraine-hits-all-11-of-russias-largest-oil-refineries-in-long-range-drone-campaign-20523
- https://meduza.io/en/feature/2026/08/12/russia-s-fuel-crisis-is-back-with-queues-and-gasoline-rationing-returning-across-the-country
- https://www.fdd.org/analysis/2026/06/29/russian-gasoline-shortages-compound-economic-struggles/
- https://thesoufancenter.org/intelbrief-2026-august-5/
- https://www.kyivpost.com/post/80843 (fuel crisis hits the Russian army)
- https://united24media.com/war-in-ukraine/russian-troops-are-running-out-of-gas-and-sometimes-leaving-military-hardware-behind-20985
- https://mind.ua/en/news/20271809-russia-cannot-repair-equipment-at-damaged-refineries-due-to-sanctions
- https://oilprice.com/Latest-Energy-News/World-News/Russia-Is-Struggling-to-Repair-Refineries-Due-to-Sanctions
- https://euromaidanpress.com/2025/10/30/russian-refinery-shutdown-repair-crisis/
- https://meduza.io/en/news/2026/09/02/protecting-oil-refineries-from-drones-is-costly-but-idle-plants-mean-even-bigger-losses-russian-government-says
- https://www.kyivpost.com/post/72140 (Krasnodar military fuel depot)
- https://kyivindependent.com/oil-depot-reportedly-set-ablaze-in-russias-krasnodar-krai-following-ukrainian-strike/
- https://mod.gov.ua/en/news/ukraine-struck-11-russian-oil-refineries-and-8-defense-industry-facilities-results-of-june-deep-strike-operations

**Rail**
- https://www.irregularwarfare.org/ukraines-railway-wars-how-to-sabotage-russian-military-logistics/
- https://www.aberfoylesecurity.com/?p=4874 (rail war, logistical superiority)
- https://foreignpolicy.com/2022/04/21/russias-military-has-a-railroad-problem/
- https://www.cna.org/reports/2023/10/Russian-Military-Logistics-in-the-Ukraine-War.pdf
- https://www.rand.org/content/dam/rand/pubs/research_reports/RRA2000/RRA2033-1/RAND_RRA2033-1.pdf (200 kg/soldier/day)
- https://en.railmarket.com/news/rolling-stock/61963-russian-strikes-already-damaged-almost-500-ukrainian-locomotives
- https://www.railtarget.eu/technologies-and-infrastructure/ukraine-railway-damage-repair-effort-russian-war-ukrzaliznytsia-10976.html
- https://www.railfreight.com/railfreight/2026/06/29/ukraine-shares-rail-lessons-repair-and-restore-capacity-is-essential/
- https://www.kyivpost.com/post/76064 (753-drone strike on Ukrainian railways)
- https://kyivindependent.com/key-train-route-to-ukraines-east-has-been-reduced-for-an-indefinite-period-due-to-safety-concerns/
- https://euromaidanpress.com/2024/08/05/russia-opens-mariupol-railway-cutting-logistics-routes-by-300-km/
- https://en.cfts.org.ua/news/russian_occupiers_launch_railway_from_rostov_on_don_to_mariupol_volnovakha_and_donetsk
- https://en.wikipedia.org/wiki/2025_Russia_bridge_collapses
- https://www.kyivpost.com/post/53734 (Bryansk and Kursk rail bridges)

**Power and hardening of fixed sites**
- https://www.diis.dk/en/research/repairing-fire-recovery-chain-constraints-on-ukraines-energy-resilience
- https://www.intellinews.com/ukraine-starts-burying-power-substations-ahead-of-a-fourth-winter-of-strikes-461572/
- https://kyivindependent.com/ukraine-moves-its-power-grid-underground-to-shield-it-from-russian-attacks/
- https://newsukraine.rbc.ua/news/ukraine-s-energy-facilities-receive-three-1700483039.html
- https://euromaidanpress.com/2025/09/05/ukraine-completes-first-phase-power-grid-protection/
- https://babel.ua/en/news/100170-concrete-blocks-and-sandbags-the-financial-times-reported-how-ukraine-is-preparing-for-attacks-on-the-energy-sector
- https://www.kyivpost.com/post/79482 (Belgorod grid)
- https://www.themoscowtimes.com/2026/02/04/ukrainian-airstrikes-knock-out-power-in-belgorod-region-for-second-time-in-a-month-a91856
- https://kyivindependent.com/ukraine-belgorod-july-3/ (Energomash, the plant that makes the replacements)
- https://militarnyi.com/en/news/russia-develops-web-protective-structure-to-shield-oil-refineries/
- https://militarnyi.com/en/news/russian-oil-refineries-begin-using-shipping-containers-for-protection/
- https://thedefensepost.com/2026/07/16/russia-drone-threat-energy-sites/
- https://united24media.com/war-in-ukraine/ukrainian-drones-force-russia-into-costly-skynex-clone-refinery-defense-19180

**Depots**
- https://en.wikipedia.org/wiki/Toropets_depot_explosions
- https://www.twz.com/news-features/satellite-images-show-massive-devastation-at-russian-ammo-storage-sites-struck-by-ukrainian-drones
- https://www.forcesnews.com/ukraine/three-months-ammo-destroyed-one-ukraines-most-devastating-strike-war
- https://english.nv.ua/nation/key-russian-ammo-depot-destruction-impacting-long-term-war-plans-50452955.html
- https://beyondparallel.csis.org/ongoing-activity-at-russias-tikhoretsk-munitions-storage-facility/
- https://theins.press/en/politics/275240 (drones vs depots)
- https://www.ukrinform.net/rubric-ato/4133692-russian-troops-in-south-change-their-logistics-routes-and-ammunition-supply-chains-says-voloshyn
- https://www.realcleardefense.com/articles/2026/02/06/when_strategy_outruns_supply_1163267.html

**Bridges and crossings**
- https://en.wikipedia.org/wiki/Bridges_in_the_Russo-Ukrainian_War
- https://planesandstuff.wordpress.com/2022/08/05/kherson-bridges-radar-analysis-and-imagery/
- https://www.cbsnews.com/news/ukraine-antonivskyi-bridge-essential-russian-supply-lines-occupied-kherson/
- https://en.wikipedia.org/wiki/2022_Crimean_Bridge_explosion
- https://www.newcivilengineer.com/latest/crimean-bridge-repaired-and-reopened-six-months-ahead-of-schedule-26-01-2023/
- https://militarnyi.com/en/news/chonhar-bridge-in-crimea-damaged-by-behemoth-and-fp-2-drones/
- https://euromaidanpress.com/2026/06/09/chonhar-bridge-linking-occupied-kherson-oblast-to-crimea-is-closed-again-after-the-second-attack-in-two-days/
- https://en.defence-ua.com/news/critical_internal_structural_damage_of_the_chonhar_bridges_raises_concerns_over_repairs_and_alternate_routes-7108.html
- https://www.armyrecognition.com/archives/archives-land-defense/land-defense-archives/russian-engineer-brigade-receives-new-pp-2005m-pontoon-bridge
- https://ukraine-war-analytics.com/regions/bridge-destruction-map.html

**Roads: denial, mining and the middle-strike campaign**
- https://en.wikipedia.org/wiki/Middle_strike_campaign
- https://en.wikipedia.org/wiki/Highway_of_Death_(Ukraine)
- https://www.scrippsnews.com/investigations/russia-ukraine-war-on-the-ground/ukraines-highway-of-death-how-drones-are-strangling-russias-southern-supply-line
- https://www.kyivpost.com/post/77980 (R-280)
- https://www.cesi-italia.org/en/articles/logistic-lockdown-inside-ukraines-medium-range-interdiction-campaign
- https://foreignpolicy.com/2026/06/03/ukraine-mid-range-drones-russia-war-success/
- https://www.cnn.com/2026/06/20/europe/ukraine-mid-range-drones-russia-logistics-intl-cmd
- https://www.globalsecurity.org/wmd/library/news/ukraine/2026/08/ukraine-260810-ukraine-mod03.htm
- https://www.forbes.com/sites/davidkirichenko/2026/05/30/why-ukraine-is-mining-russian-supply-routes-with-drones/
- https://www.forbes.com/sites/davidaxe/2025/02/24/ukrainian-road-cutter-drones-are-strangling-russian-supply-lines-and-saving-pokrovsk/
- https://euromaidanpress.com/2025/10/14/frontline-report-ukraine-replicates-roads-of-death-success-east-of-pokrovsk-crushing-russian-advance/
- https://en.wikipedia.org/wiki/Remote_Anti-Armor_Mine_System
- https://man.fas.org/dod-101/sys/land/fascam.htm
- https://apps.dtic.mil/sti/tr/pdf/ADA025248.pdf (deliberate road crater design test series)

**Airfields**
- https://en.wikipedia.org/wiki/Operation_Spiderweb
- https://militarnyi.com/en/news/spiderweb-operation-how-many-tu-95ms-tu-22m3-and-a-50s-destroyed-at-russian-airbases/
- https://www.armyrecognition.com/news/army-news/2025/russia-builds-blast-resistant-aircraft-shelters-at-airbases-near-ukraine-border-after-ukrainian-drone-strikes
- https://en.defence-ua.com/news/ukraines_new_drones_undermine_russian_aircraft_shelter_strategy_leaving_moscow_with_no_good_options-18997.html
- https://www.globalsecurity.org/military/library/policy/usmc/mcwp/3-21-1/ch6.pdf (rapid runway repair)
- https://apps.dtic.mil/sti/html/tr/ADA013517/index.html

**Strike economics, penetration and deep-strike airframes**
- https://en.wikipedia.org/wiki/Fire_Point_FP-1
- https://en.wikipedia.org/wiki/Liutyi
- https://www.adaptinstitute.org/ukraines-long-range-uav-campaign-against-russia-will-kyiv-change-the-course-of-war-by-attacks-on-russian-infrastructure/07/09/2026/ (20–30% penetration)
- https://www.forbes.com/sites/davidhambling/2026/06/19/moscow-refinery-attack-is-a-landmark-in-complex-drone-strikes/
- https://defence-blog.com/cheap-drones-are-beating-russias-billion-dollar-air-defenses/

**Asymmetry, advance rates and the Pokrovsk case**
- https://www.csis.org/analysis/russian-blood-and-treasure-ballooning-costs-putins-war
- https://ukraine-war-analytics.com/comparisons/attacker-defender-loss-ratio-2026.html
- https://united24media.com/war-in-ukraine/russias-failed-2026-spring-offensive-cost-it-nearly-100000-troops-as-ukraine-retakes-ground-18721
- https://www.russiamatters.org/news/russia-ukraine-war-report-card/russia-ukraine-war-report-card-july-1-2026
- https://united24media.com/latest-news/ukraine-builds-backup-logistics-network-to-sustain-pokrovsk-myrnohrad-defense-14279
- https://www.yahoo.com/news/articles/ukraines-general-staff-myrnohrad-pokrovsk-082800454.html
- https://mwi.westpoint.edu/logistics-determine-your-destiny-what-russias-invasion-is-reteaching-us-about-contested-logistics/
- https://warontherocks.com/feeding-the-bear-a-closer-look-at-russian-army-logistics/
