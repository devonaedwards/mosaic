# KILL ZONE — UI / UX Specification

**Version 1.0 · UX Lead · September 2026**
Written against `brief.md` (platform revision: **iOS first, iPad reference, iPhone compact, PC second**), `spec-gameplay.md` v0.9, and §6 of `spec-futures-2027-2028.md`.

Where this document contradicts the gameplay spec it does so deliberately and labels itself **Amendment**. Everything else is an implementation of that spec, not a renegotiation of it.

**Units.** All layout figures are in **points (pt)**, the iOS logical unit, unless a section is explicitly about PC, where they are pixels. Reference devices:

| Device | Points (landscape) | Scale | Safe insets (landscape) |
|---|---|---|---|
| **iPad 11″ (reference)** | 1194 × 834 | @2x | bottom 20 (home indicator) |
| **iPad 13″** | 1366 × 1024 | @2x | bottom 20 |
| **iPhone compact** | 852 × 393 | @3x | leading 59, trailing 59, bottom 21 |
| **PC (secondary)** | 1920 × 1080 px | 100% | — |

**Minimum touch target is 44 × 44 pt with an 8 pt separation.** Primary, high-frequency targets (flight cards, ability buttons, the super) are **72 pt or larger**. Nothing that can lose you the match is smaller than 60 pt.

**Camera scale.** At default zoom, **1 map metre = 0.42 pt** on iPad, so a 1,000 m FPV radius is a 420 pt ring — about a third of the iPad's width. Pinch range is 0.24–0.90 pt/m (three stops of zoom). On PC, 1 m = 0.62 px.

---

## 1. UX principles

### 1.1 The one-second rule

A player glancing at the screen for **one second**, sound off, must be able to answer four questions. Nothing else may compete for that second.

1. **Can I fly?** — the crew strip. Green tokens = yes. All amber = no. The answer is a colour field, not a number.
2. **What is happening to me?** — the alert stack and the minimap. At most one P0 alert exists at a time.
3. **What am I holding?** — the selection card, and the shape of the selection ring on the map.
4. **Am I losing something?** — red. **Red is reserved.** Red means *enemy* or *loss* and nothing else in the entire HUD. Low materiel is amber. Blocked placement is amber. Errors are amber. Red is a body count.

Corollaries we enforce in review:

- **No live number is larger than 20 pt except Materiel and Tasking Points.** Size is scarce; we spend it on the two things that tick constantly.
- **Nothing on the HUD animates on a loop** except the day/night dial (one revolution per 6:00 phase cycle) and an active P0. A HUD that breathes is a HUD you stop reading.
- **Every state that matters carries three encodings**: hue, shape, and a letter or numeral. Any one of the three is sufficient alone. This is not only accessibility (§13); it is how the link ladder becomes legible at speed (§10.6).

### 1.2 There is no hover, so nothing may depend on one

Hover was the escape hatch where fidelity hid. Without it, every fact must be **always-on**, **revealed by the gesture that needs it**, or **cut**. The replacement policy, applied everywhere below:

| Fact class | Old (mouse) | Touch resolution |
|---|---|---|
| Exact HP, exact stat values | hover tooltip | **Tap-to-inspect.** Two-finger tap on any unit, or tap its chip in the selection card, opens a 320 pt Inspect sheet. Never changes selection. |
| Order consequences (path, exposure, ETA, link break) | hover a destination | **Drag-to-preview.** Press the unit or the flight card, drag; the preview draws live under the finger and commits on lift. This is strictly better than hover: the preview exists exactly as long as the decision does. |
| "Will this drone go blind here?" | hover | **Always-on while armed.** The instant a flight is armed, every jam dome on screen redraws its break radius *for that flight's Link Robustness* (§4.5). The map answers before the finger moves. |
| Costs, build times, prerequisites | tooltip | **Printed on the card.** An 88 pt build card has room for cost and time; it always shows them. |
| Long-form description, lore, full multiplier row | tooltip | **Long-press (450 ms)** opens the Inspect sheet. Never a floating tooltip — floating tooltips under a finger are occluded by the finger. |
| Anything that changes whether an order is legal | — | **Forbidden as a reveal.** Legality is always visible before the gesture: greyed card, red reticle, refused placement footprint. |

**PC keeps hover**, with a 260 ms delay, as an accelerator for the Inspect sheet only. No PC-exclusive information exists.

### 1.3 The no-spreadsheets rule

The gameplay spec contains a 7×5 damage multiplier table, a five-column link table, a nine-modifier signature model and an autonomy plausibility table. **None of them appear in the game.** They appear as consequences.

Enforceable form: *no panel may contain more than six live numbers, and no panel may present numbers in a grid.* Relationships — shaped charge is useless against a rotary drone and lethal against a tank's roof — become **two shape glyphs and an arrow**, with the number in the Inspect sheet. Budgets — crews, supply, uplink capacity — become **discrete tokens you can count**, never a bar with a fraction printed on it. Counting five amber tokens is a one-second operation at arm's length. Reading "11/16" at 11 pt is not. The one exception is the **post-match report** (§11.4), a spreadsheet on purpose, after the fact.

### 1.4 Thumbs, not cursors

In a landscape two-handed grip the thumbs sweep an arc of roughly **260 pt radius** on iPad, **150 pt** on iPhone. The top edge requires regripping. Three zones, and every control belongs to one:

- **Green (bottom 300 pt, both corners inward):** everything tapped more than once a minute. Flight cards, ability buttons, the super, the build tab spine, selection.
- **Amber (top 180 pt):** things tapped occasionally and deliberately. Minimap, alert cards, the tasking catalogue.
- **Read-only (top bar, 44 pt):** resources, clock, day/night, Grid Control banner. **Nothing in the top bar is ever a tap target**, so a mis-grab there costs nothing.

**The fat-finger rule:** any control whose mis-tap is irreversible (super ability, surrender, the Blackout/Arclight commit) requires a **two-stage confirm** — arm, then a distinct commit gesture on the map — never a double-tap on the same 72 pt square.

### 1.5 Teaching the link ladder without a tutorial wall

Four objects, no text walls.

**a) The pip teaches the state.** Every linked unit carries a three-state pip: green, amber, black. A player who has read nothing learns "amber is bad, black is dead" in the first bubble they fly into, because four seconds later the drone is gone. Tuition: one 200 MAT FPV.

**b) The red dome teaches the cause.** Jam bubbles draw as translucent red domes the moment the emitter is detected, permanently, as terrain. Nobody needs to know what J70/r450 means. They need to know the shape of the thing that ate their flight, drawn where it happened.

**c) The greyed card teaches the answer.** An unflyable flight card greys and shows *why* as one glyph: crossed headset (no crew), hollow hangar (no stock), padlock (source destroyed or unpowered), moon (daylight, Night Bomber). Reasons are never words on a card; always one glyph, always the same glyph.

**d) The ladder ribbon teaches the shape.** The TASKING drawer carries a permanent five-segment ribbon across its top: **RADIO · MESH · FIBER · SAT · AUTO**, with the columns you field lit and the enemy's *observed* columns ticked underneath. It is the only place the ladder is drawn as a ladder, it costs one glance, and it answers "what column are they on, and am I behind?"

**No modal tutorial popups exist in skirmish.** The Range arc (§12.3) is three real, optional missions.

---

## 2. Screen anatomy

### 2.1 The layout calls

**Call 1 — the sidebar survives as a spine plus a drawer. (Amendment to `spec-gameplay.md` §4.1.)** A permanent 300 pt panel is 25% of an iPad's width. So the C&C rail splits: an **always-visible 88 pt Command Spine** on the right edge carrying the six tab buttons and the live production queue, plus a **292 pt Build Drawer** that slides over the map on a tab tap and auto-collapses 2.5 s after a placement or on any map tap. The queue never hides, because what is building and how long is left is the thing a C&C player actually watches. Rejected: a bottom drawer (steals the flight line's home, fights the thumbs) and a radial (unreadable at 14 structures, no spatial memory between matches).

**Call 2 — the hangar moves to the bottom centre. (Amendment to §4.1.)** The gameplay spec puts flight cards "above the sidebar". A sortie is a *targeting* action, so the cards belong inside the thumb arc near the map, not stacked in a corner with the build list; and ten cards with a count, a link stripe and a crew rail will not read at 88 pt wide. The two-row block also maps onto an external keyboard's two rows (§13.5).

**Call 3 — the minimap stays persistent on iPad and becomes a hold-overlay on iPhone.** The minimap is where you check whether the middle of the map is currently lethal. On 1194 pt we can afford 200 pt for that. On 852 pt we cannot.

### 2.2 iPad 11″ — 1194 × 834 pt (reference layout)

```
 0                                                                              1194
 ┌────────────────────────────────────────────────────────────────────────────────┐ 0
 │ TOP BAR 1194×44   ◆MAT 4,120 ▴22   ◇TP 640   ⚡138/200   ◑NIGHT 1:12  14:22   │
 ├──────────────────┬───────────────────────────────────────┬─────────────────────┤ 44
 │ ┌──────────────┐ │                                       │ ┌─────────────────┐ │
 │ │ALERT 300×52  │ │                                       │ │   MINIMAP       │ │
 │ │▲ NODE CUT OFF│ │                                       │ │   200×168       │ │
 │ ├──────────────┤ │                                       │ │ ▓threat ●contact│ │
 │ │· KILL VERIF. │ │         M A P   V I E W P O R T       │ └─────────────────┘ │ 220
 │ └──────────────┘ │        (full screen, HUD floats)      │        ┌──────────┐ │
 │ ┌──┐             │                                       │        │ BUILD    │ │ 236
 │ │G1│ GROUP RAIL  │                                       │        │ DEFENCE  │ │
 │ ├──┤ 56×56 ×5    │                                       │        │ CREW     │ │
 │ │G2│ (auto-groups│                                       │        │ GROUND   │ │
 │ ├──┤  first 3)   │                                       │        │ AIR      │ │
 │ │G3│             │                                       │        │ TASKING  │ │
 │ ├──┤             │                                       │        ├──────────┤ │ 682
 │ │＋│             │                                       │        │ ▣ 0:14   │ │
 │ └──┘             │                                       │        │ +4 ▸     │ │
 │                  │                                       │        │ SPINE 88 │ │
 ├──────────────────┴────┬──────────────────────────┬───────┴───────┐│          │ │ 630
 │ SELECTION 329×184     │ FLIGHT LINE 472×184      │ OPS 241×184   ││          │ │
 │ ┌────┐ FIBER FPV  ×4  │ ┌───┬───┬───┬───┬───┐    │ ┌────┐ ⟦V⟧⟦⇥⟧ ││          │ │
 │ │ IMG│ ▰▰▰▰▰▰░ 70/90  │ │ 1 │ 2 │ 3 │ 4 │ 5 │    │ │SUPR│ ⟦G⟧⟦◎⟧ ││          │ │
 │ │    │ ⬢Ar ◆Shp ▮F    │ ├───┼───┼───┼───┼───┤    │ └────┘        ││          │ │
 │ └────┘ ▲▲⬡  ▼⬢        │ │ 6 │ 7 │ 8 │ 9 │10 │    │ ▸FPV×3 ●ETA6s ││          │ │
 │ [ABL1][ABL2][ABL3][×] │ └───┴───┴───┴───┴───┘    │ ▸REC×1 ●orbit ││          │ │
 └───────────────────────┴──────────────────────────┴───────────────┴┴──────────┘ 814
 16                   345 361                    833 849         1090          1186  (20pt safe)
```

**Region table — iPad 11″**

| Region | x | y | w | h | Zone | Notes |
|---|---|---|---|---|---|---|
| Top bar | 0 | 0 | 1194 | 44 | read-only | Never a tap target. |
| Alert stack | 16 | 56 | 300 | n×52 | amber | Max 5 cards; P0 pinned above, full-width. |
| Group rail | 16 | 300 | 56 | 5×60 | amber/green | 3 auto-groups + 2 user slots (§8.3). |
| Minimap | 986 | 52 | 200 | 168 | amber | Tap = jump, drag = pan, two-finger tap = ping (multiplayer). |
| Command spine | 1098 | 236 | 88 | 578 | green | 6 tabs 88×72 (y 236–682) + queue block 88×124 (y 690–814). |
| Build drawer | 806 | 236 | 292 | 578 | green | Slides left over the map, 160 ms. Auto-collapse 2.5 s. |
| Selection card | 16 | 630 | 329 | 184 | green | Ability row 4 × 64 pt along its bottom edge. |
| Flight Line | 361 | 630 | 472 | 184 | green | 2 rows × 5 cards, 88×88, 8 pt gutters. Block centre x=597 = screen centre. |
| Ops strip | 849 | 630 | 241 | 184 | green | Super 72×72, four overlay toggles 56×56, up to 4 sortie chips. |

**Occlusion:** 31.4% of the frame at rest, 45% with the Build Drawer open (transient). **Two-finger double-tap dims all HUD panels to 18% opacity for 2.5 s** so a player can read the ground under the flight line without changing anything.

### 2.3 iPad 13″ — 1366 × 1024 pt

Same anatomy, same anchors, more map. Controls grow by **1.12×** (flight cards 96×96, spine 100 pt, ability buttons 72 pt), and the extra width buys a **sixth column of flight cards (12 slots)** and a **240 × 202 minimap**. Occlusion drops to **26.8%**. The Build Drawer becomes 340 pt and shows 4 columns instead of 3. Nothing moves zone.

### 2.4 iPhone compact — 852 × 393 pt

The compact layout is not a scaled iPad. It is a different set of decisions about what is worth 393 pt of height.

```
 59 (safe)                                                                     793
 ┌────────────────────────────────────────────────────────────────────────────────┐ 0
 │ ◆4,120  ◇640  ◑1:12  14:22                      TOP BAR h32, read-only        │
 ├────────────────────────────────────────────────────────────────────────────────┤ 32
 │ ┌────────────┐                                                                 │
 │ │ALERT 260×44│              M A P   ( f u l l )                        ┌────┐  │
 │ └────────────┘                                                        │ ◫  │  │ 52
 │                                                                       │MAP │  │
 │                                                                       └────┘  │
 │                                                                       ┌────┐  │
 │                                                                       │SUPR│  │ 200
 │                                                                       └────┘  │
 ├──────────┬───────────────────────────────────────────────┬─────────────────────┤ 296
 │ ┌──────┐ │  ◂ ┌────┬────┬────┬────┬────┐ ▸                │            ┌─────┐ │
 │ │SEL ×4│ │    │ 1  │ 2  │ 3  │ 4  │ 5  │   FLIGHT 5×56    │            │ ▤   │ │
 │ │FIBER │ │    └────┴────┴────┴────┴────┘   paged          │            │BUILD│ │
 │ └──────┘ │                                                │            └─────┘ │
 └──────────┴───────────────────────────────────────────────┴─────────────────────┘ 372
                                                                        (21pt safe)
```

**What collapses, and the rule behind each:**

| Element | iPad | iPhone | Rule |
|---|---|---|---|
| Minimap | persistent 200×168 | **56 pt button; press-and-hold expands a 300×220 overlay**, orders may be issued inside the overlay, release dismisses | Anything you consult but do not watch becomes a hold. |
| Selection card | 329×184 full card | **88×56 chip** (type + count + link pip); tap expands a 300×200 sheet over the map | Anything you can summarise in three glyphs becomes a chip. |
| Ability buttons | 4 × 64 pt row | **radial on long-press of the unit**, 4 slices at 80 pt arc | Anything context-specific moves to the context. |
| Flight Line | 10 cards, 2 rows | **5 cards, 1 row of 56 pt, paged with ◂ ▸ and a swipe** | Frequency survives; breadth pages. |
| Command spine | 88 pt, always visible | **56 pt FAB bottom-right** opening a full-height modal drawer with the six tabs as a top segmented control | The queue moves into the top bar as a single 4 pt progress hairline under the resource row. |
| Power, Grid banner | top bar | **events in the alert stack** | A number you check twice a match is an event, not a readout. |
| Ops strip | 241 pt | **super button only, 56 pt**, right edge y 180 | |
| Group rail | 5 slots | **3 slots inside the selection sheet** | |

**iPhone also changes one default:** one-finger drag on the map **pans** rather than marquee-selects (§8.1), because on a 393 pt-tall screen panning dominates and a marquee is usually a mistake.

### 2.5 PC — 1920 × 1080 px (secondary platform)

The PC build is the same layout with the drawer pinned open, because a mouse has no thumb arc and a 320 px rail costs only 16.7% of a 1920 px frame.

```
 0                                                                    1580      1920
 ┌──────────────────────────────────────────────────────────────────────────────────┐ 0
 │ TOP BAR 1920×44  ◆MAT 4,120 ▴22  ◇TP 640 ▴84  ⚡138/200  ◑NIGHT 1:12  14:22      │
 ├──────────────────────────────────────────────────────────┬───────────────────────┤ 44
 │ ┌────────────────┐                                       │ MINIMAP    296×248    │
 │ │ ALERT 360×56   │                                       ├───────────────────────┤ 304
 │ └────────────────┘      M A P   1580 × 1036              │ CONTROL ticker 296×26 │ 334
 │                                                          ├─────┬─────┬─────┬─────┤ 338
 │                                                          │BUILD│ DEF │CREW │     │
 │                                                          │GRND │ AIR │TASK │F1-F6│ 414
 │                                                          ├─────┴─────┴─────┤     │
 │                                                          │ ▣ ▣ ▣  BUILD    │     │
 │  ┌──────────────────────┐                                │ ▣ ▣ ▣  PALETTE  │     │
 │  │ EVENT LOG 420×160    │                                │ ▣ ▣ ▣  296×400  │     │ 818
 │  └──────────────────────┘                                ├─────────────────┤     │
 ├───────────────────┬──────────────────┬───────────────────┤ ▣▶ ▣ ▣ ▣ ▣ QUEUE│     │ 894
 │ SELECTION 420×180 │ FLIGHT 412×180   │ OPS 560×180       ├─────────────────┤     │
 │                   │ Q W E R T        │ SUPER  V ⇥ G ◎    │ CREWS ▪▪▪▪▪▪▪▪  │     │
 │                   │ A S D F G        │ sortie chips ×6   │ (34×30 tiles ×32)│    │
 └───────────────────┴──────────────────┴───────────────────┴─────────────────┴─────┘ 1080
```

PC region deltas: sidebar `x1580 y44 320×1036`; minimap `1592,56 296×248`; tab strip 2×3 of 96×36 at `1592,338`; build palette `1592,418 296×400`; queue `1592,822 296×72`; **crew rack `1592,898 296×170`** (8 × 34×30 tiles, 32 slots ≥ the 30 crew ceiling); selection `12,888 420×180`; flight line `584,888 412×180`; ops `1008,888 560×180`. Opaque chrome **32.1%**.

On iPad the crew rack does not fit a 32-tile grid in the spine, so it becomes the **crew strip**: a 296 × 18 pt bar immediately under the Flight Line, one 10 pt token per crew, green / blue / amber / grey, with the count printed at its left (`CREWS 14 · 9 READY`). Same information, one twelfth of the area, and it sits directly above the cards it gates — which is arguably better than the PC version and will probably be back-ported.

### 2.6 Top bar contents

| Element | iPad w | Content |
|---|---|---|
| Materiel | 190 | `◆` + 20 pt mono tabular numerals + 12 pt delta `▴22` (net MAT/10 s). Delta amber below +15, red when negative. |
| Tasking Points | 160 | `◇` + 20 pt mono. Earned TP flies here as a ghost numeral (§6.4). |
| Power | 120 | `⚡ 138/200` with a fill behind. At ≥100% draw it turns amber and every build card's timer shows the +40% penalty in-line. |
| Day/night dial | 130 | 28 pt dial, one revolution per 6:00, four shaded arcs, phase name and remaining time. 20 s chime + 0.4 s flare before each transition. |
| Match clock | 80 | Count-up, mono. |
| Grid Control banner | 230 | Appears only when a side holds 3+ taps: `▌GRID 3/5 3:41▐` counting down from 4:00 in the holder's allegiance colour. Allowed to pulse at 0.5 Hz under 1:00 — the only pulsing element that is not an alert. |

Both currencies stay visible. **This settles gameplay-spec open question §18.4 in favour of keeping TP on screen.** A currency you cannot see is a currency you do not plan around, and TP is a quarter of a player's spend value. We pay the cognitive cost down by never animating both numerals in the same frame and by giving TP a hollow glyph (◇) against Materiel's solid (◆).


### 2.7 Minimap behaviour

North-up, never rotates. Layers bottom to top: terrain value mask (three greys at 55% saturation) · **threat stain at 40% of full strength, always on** · Grid Taps as 10 pt diamonds (contested ones blink at 1 Hz) · structures 6 pt squares, units 4 pt dots, flights 5 pt chevrons pointing along heading · jam domes at 12% red fill · viewport rectangle · alert pings (expanding ring, 3 pulses of 700 ms) · pins as dashed circles with the owner's initials.

The minimap is the one place the threat stain never turns off, because "is the middle lethal right now" is a question you ask between other actions.

**Orders from the minimap always take the safest legal interpretation.** A tap on the minimap issues a *move*, never an attack-move, because a mis-tap at 1:40 scale is a lost army. Long-press inside the minimap opens the same context radial as the map (§8.1), so a deliberate attack-move remains possible.

---

## 3. The hangar and sorties

### 3.1 Flight card anatomy

```
 iPad card, 88 × 88 pt                      states, same footprint
 ┌──────────────────────────┐   READY        NO CREW       COOLDOWN      IN FLIGHT
 │▏                     ×7  │  ┌────────┐   ┌────────┐   ┌────────┐   ┌────────┐
 │▏   ╱▔▔╲          ready   │  │  ▲ ×7  │   │ ⌀headset│  │ ◔ 4.2s │   │ ▸ 3 up │
 │▏  ◄ ▣▣ ►    ▮ workshop   │  │        │   │  ×7 grey│  │  ×7    │   │ ×4 rdy │
 │▏   ╲__╱                  │  │ FIBER  │   │ FIBER   │   │ FIBER  │   │ FIBER  │
 │▏                         │  └────────┘   └────────┘   └────────┘   └────────┘
 │▏ [1]  FIBER FPV          │   full sat     42% sat      full, dim    cyan edge
 │══════════════════════════│   gold stripe  grey stripe  gold stripe  gold + pulse
 └──────────────────────────┘
   ▲          ▲         ▲
   │          │         └── 6 pt LINK STRIPE, hue + pattern + letter (F)
   │          └── 12 pt hotkey / slot chip, bottom-left
   └── 4 pt CREW RAIL, left edge: green = crews spare, amber = last crew, grey = none
```

Fixed elements, always present, never on a reveal: unit silhouette (40 pt), **ready count** as 20 pt mono at top-right, **link stripe** along the bottom edge (6 pt, carrying hue + pattern + letter), **crew rail** down the left edge (4 pt), **source glyph** at top-right of the silhouette (workshop / airfield / launch rail / spool plant), slot chip bottom-left, and the type name in 11 pt condensed caps.

### 3.2 Card states

| State | Visual | Interactive |
|---|---|---|
| **READY** | Full saturation, crew rail green, count in white | Tap / drag arms |
| **LAST CREW** | Crew rail amber, count in amber | Arms, but the armed reticle shows `⚠ 1 crew` |
| **NO CREW** | 42% saturation, crossed-headset glyph replaces the count, stripe desaturates | Tap does nothing but **flashes the crew strip** for 600 ms and speaks nothing — the eye is sent where the answer is |
| **NO STOCK** | Hollow hangar outline, count `×0`, a 3 pt production hairline at the card's base filling toward the next airframe | Tap **jumps the Build Drawer to that unit's tab and highlights its row** |
| **COOLDOWN** | Radial sweep over the silhouette, seconds remaining in mono | Inert; queues the arm if tapped within 2.0 s of ready (input forgiveness) |
| **IN FLIGHT** | Cyan edge-light 2 pt, `▸ n up` above the ready count | Tap **selects the airborne flight** rather than arming a new one — unless the card is double-tapped, which always arms |
| **LOST** | Base line flashes `--exposure-exposed` for 320 ms, the `n up` count ticks down with a 2 pt downward nudge | — |
| **DISABLED** | Padlock (source destroyed / unpowered) or moon (Night Bomber in daylight, with a countdown to dusk) at 30% opacity | Inert; long-press explains in the Inspect sheet |

Cards are ordered by **source structure, then tier**, and their slots are **sticky for the whole match**: once Fiber FPV occupies slot 3, it stays in slot 3 even at zero stock. Muscle memory is worth more than tidiness.

### 3.3 The two-click (two-tap) sortie

**Primary flow — tap, tap.**

1. **Tap a card.** Card lifts 4 pt over 70 ms, its stripe brightens, a cancel chip appears centre-bottom (`✕ CANCEL · 88 pt`), and the map enters *armed* state: 8% dim over terrain, every jam dome redraws its break radius for this flight's R (§4.5), and every valid target type gains a 1 pt highlight ring.
2. **Tap a target or a point.** Commit. The card flashes white for 140 ms, a launch marker pulses at the source structure, a sortie chip appears in the Ops strip, and the crew strip turns *n* tokens blue.

Total: two taps, ~600 ms, no submenu, no confirmation. This is the atomic action of the game and nothing may be added to it.

**Alternative flow — drag to target.** Press the card and drag onto the map without lifting. A live ribbon follows the finger: flight path arc, ETA in seconds at the reticle, crews consumed, and the exposure/link preview of §5. **Lift to commit; drag back onto the card to cancel.** This is the flow that answers "will this drone go blind here" before committing, and it is the one we teach in the Range.

**Count.** A **vertical drag on a card** (before leaving the card) sets how many airframes launch: up increases, down decreases, with a haptic tick per step and the count numeral swapping from `×7 ready` to `→ 3`. Default is "all ready, capped by idle crews".

**Multi-card packages.** Press one card and **slide horizontally across the hangar bar**; each card crossed toggles into the package with a light haptic tick and a 2 pt joining bracket drawn under the selected cards. Lift on the last card to arm the package, then tap the target. Packages launch simultaneously from their several sources and are registered as **one flight** for selection and recall purposes.

**PC equivalents:** click to arm, click to commit; mouse-wheel over a card sets count; Shift+click builds a package; `Z X C V B` / `Shift+Z…B` are slots 1–10 (§13.5). Drag-to-target works identically with the button held.

### 3.4 A sortie in progress

**On the map.** Each airborne unit carries its link pip and, if selected or in Link View, its control line. A one-way flight draws a **1 pt dotted intent line** from its current position to its commit point; the line is the flight's colour (allegiance cyan) and shortens as it closes — a visual ETA you read without numbers. Fiber flights draw their thread (§4.3) instead, which is a physical object.

**In the panel.** The Ops strip holds up to four **sortie chips** (241 × 32 pt each on iPad):

```
 ┌──────────────────────────────────────────────┐
 │ ▸ FIBER FPV  ×3   ●●●   [▮F]   ETA 6s   ⟲ ⟳ │
 └──────────────────────────────────────────────┘
    type & count    per-unit  link   time to    recall
                    pips      type   target     / redirect
```

Tap the chip to select the flight and centre the camera on it. The chip's per-unit pips are the fastest read in the game for "is my strike still alive and still connected": three green dots means three drones flying with link; one amber means one is in a bubble and has four seconds.

**Recall and redirect.** With a flight selected (or its chip tapped): **`⟳` redirect** — arms a new target reticle, tap to retask; **`⟲` recall** — reusable units (Multirole Quad, Recon Wing, Night Bomber, Interceptor) fly home and return to hangar stock; one-way units (FPV Team, Fiber FPV, Loitering Munition) **cannot be recalled** and their recall button is replaced by a `HOLD` button which parks them in a 120 m orbit at the last waypoint. This distinction is drawn on the chip itself with a **one-way glyph (→●)** so nobody learns it by losing four drones. On PC, recall is `Backspace`; redirect is a normal right-click order.

### 3.5 How a lost drone is reported

Deliberately quiet, because you will lose hundreds.

- The sortie chip's pip goes black then vanishes; the count ticks down with a 2 pt nudge and a 120 ms `--exposure-exposed` tint.
- The flight card's base line flashes red for 320 ms.
- A **✕ marker** drops at the loss point on the map, persists 8 s at full opacity, fades over 4 s, and then seeds the threat stain (§5.4). The marker is 14 pt and carries the unit's link glyph, so a field of ✕-with-R tells you at a glance that your *radio* drones are the thing dying there.
- The crew token in the crew strip goes amber for the recovery duration with a 1 pt red top edge.
- The event log gets one line.
- **CONTROL speaks only when three or more are lost within 5 s** ("Flight lost."), or when the loss is a Recon Wing, Mothership, Night Bomber or a crewed unit worth ≥900 MAT. Every other death is silent. A game that announces every FPV becomes a game with the sound off.

Losing an entire *package* triggers a single P1 alert card with a **Go to loss** action that frames the camera on the loss cluster with the threat stain visible.

---

## 4. The control-link layer on the map

Five things get drawn: jamming fields, relay chains, fiber tethers, satellite coverage, autonomy boxes. Each has four states — **at rest**, **on inspect**, **on selection**, **contested** — and one shared overlay.

### 4.1 The overlay toggle: Link View

**Hold the `⇥` button in the Ops strip** (PC: hold `Tab`). Hold, not toggle: the graph is a question you ask, not a state you live in. While held:

- Terrain desaturates to 25% and dims 30% over 120 ms.
- Every friendly linked unit draws a 1.5 pt line to its controlling node, in its link hue with its link pattern.
- Relay chains draw as bright segments with a small hop numeral at each node (`1 2 3`), and the **hop budget** of each chain shows at its root (`3/4`).
- Satellite units draw a 12 pt up-chevron instead of a line, and the Uplink Terminal shows `4/6` capacity used.
- Autonomous units draw **no line at all** and instead show their dashed hunt box.
- Enemy links you have observed draw at 40% opacity in red-tinted versions of the same patterns. You only ever see enemy links for units currently detected, and the graph never infers.

Release and everything is back in 90 ms. **Selected units always draw their link line without the overlay**, which is how a new player discovers the system: they tap a drone and a thread appears connecting it to a building.

A second overlay, **Supply View**, is the sibling (§7) on the `G` button; **Threat** is on `V`; **Exposure-always** is on `◎`. Four toggles, four buttons, 56 pt each, in one 2×2 block in the Ops strip.

### 4.2 Jamming fields

| State | Drawing |
|---|---|
| **At rest** | A translucent dome: `--jam` at **10% fill**, a 2 pt border at 40% opacity, and a 1 pt inner ring at the 70%-strength radius. Domes are *terrain*: they persist once the emitter is detected, they are drawn under units and over ground, and they never animate. |
| **Contested (emitter pulsing)** | An EW Truck running *Silent Running* fades its dome out over 600 ms to a 1 pt dashed outline — "it was here, it is off now" — and back in over 400 ms when it restarts. The dashed ghost decays over 25 s if never seen again. |
| **On inspect** (two-finger tap / long-press) | The Inspect sheet gives `J 70 · r 450 m · breaks: radio, mesh · survives: fiber, satellite, autonomy`, in words and glyphs, never as a formula. |
| **On selection** (of a flight, or while armed) | **The break ring.** See §4.5. |
| **Overlapping domes** | Fills do not stack additively past 18%; instead the overlap region draws a 2 pt cross-hatch. Two jammers looks like *hatching*, not like a darker blob. |

### 4.3 Fiber tethers

The thread is a real object and is always drawn, at rest, for everyone who can see it — per `spec-futures-2027-2028.md` §6.4.7, invisible fiber is unseeable death.

- **At rest:** a 1.5 pt solid `--link-fiber` line from launch point to drone, drawn *on the ground plane* (not in the air), following the flown path, not a straight line.
- **Snag risk:** where the thread lies in a forest or under a power-line corridor, it draws with a 3 pt dotted overlay in `--warn` — visible risk, no percentage. Long-press the thread for `4%/s` in the Inspect sheet.
- **Cut:** the thread snaps with a 180 ms retraction toward both ends and the drone's pip goes black in the same frame.
- **Residual:** the thread stays 30 s after the drone dies, decaying to 40% opacity. An enemy ground unit that touches a residual thread gets a **1.5 s directional arrow** pointing to the launch site, drawn from the touching unit, plus a P2 alert `THREAD FOUND`.
- **On selection:** the leash is drawn as a hard circle from the launch point at 1,400 m (2,000 m with Spool Extension) in `--link-fiber` at 25%, and the drone's remaining slack shows as a second, shrinking ring. Fly to the leash and the ring flashes twice.

### 4.4 Relay chains, satellite coverage, autonomy boxes

**Relay chains.** At rest, a Relay Mast draws a 700 m radius at 6% `--link-mesh` fill only while you have units using it. Chains draw as bright 2 pt segments between hops, and **the whole chain flashes white for 250 ms when any node in it dies** before the children's pips drop — 250 ms of warning is what turns "everything went black at once" into "they killed my relay." Obsidian's air units act as hops; their chains redraw continuously, which is why Obsidian's Link View looks alive and Kestrel's looks like plumbing.

**Satellite coverage.** A 4% `--link-sat` wash with 40 pt grain over the whole map, so it reads as "everywhere" rather than as a region. What is scarce is capacity, so the real UI is the **capacity meter**: six hollow chevrons in the Ops strip, filling as satellite units launch; at 6/6 every satellite flight card greys with a chevron glyph. A Kestrel Uplink Jammer punches a 300 m red-hatched hole with a hard edge — the only boundary satellite coverage ever has, and unmistakable.

**Autonomy boxes.** Drawn at rest as a **dashed 2 pt rectangle in `--link-auto`** with a corner tick count showing munitions committed to it. They have no lines, ever — the absence of a line is the point, and it is the visual signature of "no crew is watching this." While an autonomous munition resolves its target (§5.4 of the gameplay spec), the box's border chases once around its perimeter over 600 ms. If it picks a decoy, the decoy flashes `--link-auto` white on impact and the post-match Losses tab counts it as a **misidentification** — a running counter in the Ops strip (`MISID 3`) appears the first time it happens and stays for the match. This is the honest, legible price of autonomy that `spec-futures-2027-2028.md` §7.2 demands.

### 4.5 "This drone will go blind here" — before committing

This is the single most important preview in the game and it is always-on rather than on-demand.

**The moment a flight card is armed**, every visible jam dome recomputes and draws its **break ring**: the radius at which `J_eff > R` *for the specific link of the armed flight*. The ring is 3 pt, `--exposure-exposed`, with 8 pt inward ticks, and the area inside it fills to 18%. Outside the break ring but inside the dome, the fill stays at 10% — that band is survivable and the drawing says so.

Consequences that fall out for free, and that teach the ladder:

- Arm a **radio** FPV: break rings are almost as large as the domes. The map looks closed.
- Arm a **fiber** FPV: *the break rings disappear entirely* and the domes go flat 6%. One tap, and the player sees the map open up. Nothing we could write would teach fiber as fast.
- Arm a **satellite** flight under a Kestrel Uplink Jammer: one small, hard, 300 m ring in an otherwise open map.
- Arm an **autonomous** munition: no rings, but every **decoy cluster** you have detected draws its own dashed white circle — the autonomy-specific threat, shown in the autonomy-specific moment.

**During the drag-to-target flow**, the path ribbon marks the exact crossing point where the link would break with a **✕ flag on a 12 pt stalk**, labelled `LINK LOST · 3.1 s in` (mono, 12 pt). If the player owns the AI Last-Mile Module, the flag changes to `AUTONOMY TAKES OVER` in `--link-auto` and the ribbon continues in a dashed line to the commit point. The upgrade is thereby explained by the map, at the moment of use, forever.

**On PC** the same preview attaches to hover with the card armed, with an identical break ring.

---

## 5. Exposure preview and threat stain

### 5.1 What the player is being told

Two different things, and they must never look alike:

- **Exposure** is a *prediction about my own order*: how long this unit will be detectable in the open on this route, against my *estimate* of enemy reaction time. It is drawn in the **exposure ramp** (green→amber→red), only while an order is being composed, and it disappears on commit.
- **Threat stain** is a *memory of what has already happened to me*. It is drawn in `--stain` red, blurred, decaying, only on the Threat overlay and the minimap.

Rule: **prediction is a ribbon, memory is a stain.** Never the same shape, never the same texture.

### 5.2 Exposure preview wireframe

```
      ┌ press a unit, drag ─────────────────────────────────────────────┐
      │                                                                 │
      │   ●unit ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬ ◎        │
      │          CLEAR      │   TIGHT   │     EXPOSED      │ finger     │
      │          green 6 pt │ amber 8pt │  red 10 pt +hatch│            │
      │                                                                 │
      │                     ┌──────────────────────────────┐            │
      │                     │ EXPOSED   41 s                │  ← 190×92 │
      │                     │ react     22 s  ●●○  EST      │    chip,  │
      │                     │ ───────────────────────────── │    offset │
      │                     │ ⚠ radio link lost at 3.1 s    │    +64,-108│
      │                     │ ⏱ arrive 0:38   ◆ 45 supply   │    from    │
      │                     └──────────────────────────────┘    finger  │
      └─────────────────────────────────────────────────────────────────┘
```

### 5.3 Exact visual language

**The ribbon.** Drawn on the ground plane along the actual pathfound route, not a straight line. Width encodes severity so that severity survives colourblindness and survives a finger sitting on top of it:

| Band | Condition | Colour | Width | Texture |
|---|---|---|---|---|
| CLEAR | Exposure < 0.6 × Reaction | `--exposure-clear` #4F8F4A | 6 pt | solid |
| TIGHT | 0.6 – 1.0 | `--exposure-tight` #C98A1E | 8 pt | solid |
| EXPOSED | > 1.0 | `--exposure-exposed` #C4463F | 10 pt | 45° hatch, 6 pt pitch |
| UNKNOWN | no threat data for this area in the last 90 s | `--ink-mute` #6E7A73 | 6 pt | 3 pt dashes |

**UNKNOWN is not optional.** It is the honesty mechanism: a route through a sector you have never observed and never died in is drawn grey, not green. Green is a claim, and we only make claims we can support.

**The chip.** 190 × 92 pt on iPad (150 × 78 on iPhone), anchored 64 pt right and 108 pt above the finger so the hand never covers it, flipping to the left when within 220 pt of the screen edge. Contents, in this order and no more than six live numbers:

1. **Verdict word** — `CLEAR` / `TIGHT` / `EXPOSED` / `UNKNOWN`, 17 pt condensed caps, in the band colour. The worst band on the route wins.
2. **Exposure seconds**, 20 pt mono.
3. **Reaction seconds**, 14 pt mono, with **confidence dots** — `●●●` data < 20 s old, `●●○` 20–60 s, `●○○` > 60 s — and the literal tag `EST`. The tag never disappears. We are always guessing about the enemy and we always say so.
4. **One warning line**, if any: link loss, leash limit, mine field, net tunnel, cut-off destination. Maximum one; the highest-priority warning only.
5. **Arrival clock** and, for carriers, the delivered supply.

**Timing.** Ribbon and chip appear after **120 ms of drag** (kills flicker on taps), fade in over 90 ms, recompute at **8 Hz** with the ribbon interpolating over 60 ms so it slides rather than snaps, and fade out over 60 ms on lift. The chip never moves faster than 900 pt/s even if the finger does — a jittering number is an unreadable number.

**On commit,** the ribbon flashes once at 140% brightness for 120 ms and is gone. It is never persistent. A player who wants it back re-drags, or turns on the `◎` **Exposure-always** toggle, which draws thin 2 pt exposure ribbons on every moving friendly unit's current path continuously — an advanced, opt-in, slightly noisy mode we expect logistics players to live in.

### 5.4 The threat stain

Toggle `V`. Drawn on the ground plane, under all units and markers, over terrain.

- **Sources:** every friendly loss (200 m radius, intensity by MAT value, capped so a 1,600 MAT tank is 3× an FPV, not 8×); every observed enemy launch, staining its origin; every known enemy EW Post, Relay Mast and Workshop projecting its theoretical reach at half intensity.
- **Rendering:** not a smooth heat gradient. **Three discrete contour bands** at 33/66/100% intensity, filled at 8% / 14% / 22% `--stain`, with a 1 pt contour line at each boundary. Contours read as a *map*; a smooth blur reads as dirt on the screen. Bands also make the stain legible to a protanope, because the boundary lines carry it.
- **Decay:** 1.5%/s, so a quiet sector clears in about 60 s, and the whole overlay visibly *retreats* when you take ground. New stains bloom in over 400 ms. Decay is computed continuously but redrawn at 4 Hz to keep it calm.
- **Clutter control:** the stain never exceeds 22% alpha; it is suppressed entirely within 300 m of a selected unit while an order is being composed (the exposure ribbon owns that space); and it does not draw on the sidebar-adjacent 40 pt of the map, so the drawer never opens onto a red wall.

**How it stays honest.** (1) Built *only* from events you witnessed, never from simulation truth — if they moved their workshop and you have not seen it, your stain is wrong, and it staying wrong is the mechanic. (2) It decays whether or not the danger has gone, so it understates as often as it overstates. (3) Everything derived from it is labelled `EST` with confidence dots. We never present memory as intelligence.

**How it stays out of the way.** Off by default; permanently on at 40% on the minimap, where it is 200 pt across and cannot clutter anything; and, per gameplay-spec open question §18.2, drawn **automatically and locally** as a 400 m soft disc around any move-order drag whether or not the overlay is on. The stain then does its job at the moment it matters without the player having to remember it exists, which we believe answers the "does it become wallpaper" risk.


---

## 6. Recon, pins and verified kills

### 6.1 Placing a pin

**Touch:** long-press (450 ms) on a live enemy unit, or on any point inside a friendly recon footprint, then **drag outward to size the ring** (120 m default, 80–200 m range) and lift. A haptic tick fires at 450 ms when the pin becomes available, so the gesture teaches its own timing. If the point is not legally pinnable the ring draws red and refuses on lift with a sharp haptic and the reason as a 1.5 s toast: `NO OBSERVER`.

**PC:** `P` then click, or right-click-drag on a live enemy (as `spec-gameplay.md` §7.4 specifies).

Pin budget is 6. The Ops strip shows `PINS 3/6` as six hollow chevrons; a pin about to expire (< 10 s) flashes its chevron at 1 Hz.

### 6.2 How a pin looks

A **120 m white dashed ring**, 2 pt, dash 8/6, with a 60 s depletion arc drawn as the outer dash gradually converting to solid — so a nearly-expired pin is a nearly-solid circle, readable at a glance and at any zoom. At the centre, a 14 pt **white diamond bracket** marks Designated status for everything inside, and each designated enemy unit gains its own small bracket that snaps to it.

To an **ally** the same ring draws in **the owner's allegiance colour** with the owner's two-letter tag at the ring's top-right, so a 2v2 recon player's pins are visibly *theirs*. Ally pins are fully usable — indirect fire from either player resolves against them, which is the whole point of §7.4 of the gameplay spec.

Enemy pins are never shown. You learn you are pinned by being hit.

### 6.3 Verified kills

The distinction has to land in under 400 ms, without reading.

| | Verified | Unverified |
|---|---|---|
| Marker | White diamond bracket **snaps shut** over the corpse in 140 ms, then holds 900 ms | Hollow grey bracket, no snap, 600 ms |
| Payout | `+84` in 18 pt mono white, rising 24 pt over 900 ms with a 120 ms ease-out | `+42` in 13 pt `--ink-2`, rising 14 pt, with a small crossed-eye glyph |
| Audio | A short two-note confirm (verified only) | Nothing |
| Haptic | Light tick, rate-limited to 1 per 1.5 s | None |

The TP readout in the top bar receives a **ghost numeral** — the incoming value fades in beside the total, counts up over 400 ms, and the total lands. If three payouts land within 900 ms they merge into one ghost numeral; we never stack five `+12`s on top of each other.

**Pin payouts** (your pin, ally's shot) draw the rising numeral **at the pin's ring**, not at the corpse, and pulse the ring white once. The spotter sees their contribution where their contribution is.

### 6.4 Tasking-point feedback loop

TP earned is a lagging signal, so we also show a leading one: the **TASKING tab badge** carries the number of catalogue rows that are *newly affordable* since you last opened the drawer (`TASKING ③`). It is the only badge in the HUD. It turns a slow trickle into a discrete, checkable event roughly every 90 s, which is the cadence at which a player should be asking "can I buy Thermal Optics yet".

---

## 7. Logistics UI

### 7.1 Supply state on units and positions

**No bars, no numbers on units.** A single 12 pt glyph under the unit:

| State | Glyph | Meaning |
|---|---|---|
| Sustained | *(nothing)* | Inside a node radius |
| Dry | hollow amber fuel drop | 45 s outside; −50% RoF, no abilities, no repair |
| Attrition | filled red fuel drop, slow 1 Hz pulse | 3:00 dry; −1% max HP/s |

**Nodes** show a **three-segment arc** around their footprint: full / half / low, in `--ok` / `--warn` / `--bad`. Exact stock is in the Inspect sheet. In Supply View the sustain radius draws as an 8% fill with a 1 pt boundary, and overlapping radii show their union outline only — a supply network reads as one shape, not as a Venn diagram.

### 7.2 Cut-off

The loudest non-P0 event in the game, because it is a 2:30 death sentence the player can still answer.

- A **red chevron** 20 pt above the node, plus the node's arc turning red and inverting (draining anticlockwise).
- A **P1 alert card** with two actions: `GO TO` and `RESUPPLY` — the second arms the Night Bomber resupply flow directly (§7.4), or, if no bomber is available, greys with the reason glyph.
- Minimap ping at 1 Hz for 10 s.
- CONTROL: *"Position cut off."*
- In Supply View, the broken route draws as a **dashed red line with a scissors glyph at the interdiction point** — the exact place the enemy is sitting. That glyph is where the counter-attack goes, and it turns an abstract state into a target.

### 7.3 Route planning for trucks, robots and bikes

**Ferry mode, two taps.** Select a carrier, tap `FERRY` in its ability row (PC: `F`), tap the source, tap the destination. The carrier now shuttles forever, drawing a **thin dotted loop with directional ticks every 120 m** in its allegiance colour. Tapping `FERRY` again clears it. No panel, no waypoint editor, no convoy UI (explicitly excluded by the gameplay spec's anti-pillars).

While the ferry is being set, the **exposure ribbon draws for the whole round trip**, banded per segment — this is the clearest statement the game makes of "trucks die in daylight", and it is the same object as every other order preview.

Manual waypoints exist for the one-off: drag an order, then **tap additional points before lifting** to add waypoints (PC: Shift-click). Up to 8. Each waypoint shows its own band.

Carriers get a **standing-route safety**: if a ferry's route's worst band has been `EXPOSED` for 20 continuous seconds, the carrier stops at the last safe node and raises a P2 alert `ROUTE INTERDICTED` rather than feeding trucks into a kill zone one at a time. This is an accessibility and attention-budget decision as much as a design one, and it can be turned off in options for players who want the honest carnage.

### 7.4 Night bomber resupply

The bomber is the only carrier that reaches a cut-off node, so its flow is privileged.

1. The Night Bomber card carries a **moon badge with a countdown to dusk** during daylight. It is grey and inert and it tells you exactly how long until it is not.
2. At dusk the card turns live. **Arming it opens a four-slot payload strip directly above the card** — HE / Mines / Thermite / Resupply, 64 pt each, with the currently chosen loadout for all four drops shown as four chips. Tap a payload then tap a drop slot, or tap once to fill all four. Payload choice is made **at arm time and is locked at launch**, exactly as the gameplay spec requires.
3. With Resupply selected, **every friendly node on the map draws its delivery ring** and cut-off nodes draw theirs in red with a `+80` preview. Tap the node to commit.
4. In flight, the bomber's sortie chip shows `▸ NIGHT BOMBER · 4 drops · ETA 22s` and its drops tick down as they land.

---

## 8. Command model

### 8.1 Selection and gestures

| Gesture | Action | Conflict avoidance |
|---|---|---|
| One-finger drag on empty ground | **Marquee select** (iPad default) / **camera pan** (iPhone default, swappable in options) | The dominant action on each device wins the dominant gesture |
| Two-finger drag | **Camera pan**, always, on every device | Two fingers are never a selection |
| Pinch | **Zoom**, 3 stops, 0.24–0.90 pt/m, snapping to stops when released within 8% | Rotation is ignored entirely; the map never rotates |
| One-finger drag starting **on a friendly unit** | **Order drag** with live preview, commit on lift | A drag that starts on a unit is never a marquee |
| One-finger drag starting **on a flight card** | **Sortie drag** (§3.3) | |
| Tap unit | Select | |
| Double-tap unit | Select all of that type **on screen** | |
| Triple-tap unit | Select all of that type **on the map** | 250 ms multi-tap window |
| Long-press on map with a selection | **Context radial**, 6 slices, 80 pt arc: Move · Attack-move · Hold · Sweep/Patrol · Pin · Recall | 450 ms, haptic at threshold |
| Long-press on a unit | Inspect sheet | |
| Two-finger tap | Inspect without selecting | |
| Two-finger double-tap | HUD dim 2.5 s | |
| Tap empty ground | Deselect | |

**System gesture safety.** We call `preferredScreenEdgesDeferringSystemGestures` on the bottom and right edges, keep all controls ≥ 21 pt above the home indicator, and never bind an edge swipe — the right edge belongs to iPadOS Slide Over, so the Build Drawer opens by tap only. Three- and four-finger gestures are unused.

### 8.2 Orders

**Attack-move differs by domain, and the cursor says so.**

- **Vehicles and infantry:** advance, engage anything in range, resume. Reticle: crossed swords.
- **Reusable drones (Multirole, Interceptor, Recon Wing, Night Bomber):** *Sweep* — fly the path, engage the first valid target matching the stance's class filter, then hold at the last waypoint. Reticle: a sweeping arc.
- **One-way drones (FPV, Fiber FPV, Loitering Munition):** attack-move **is a commitment** and the reticle changes to a **one-way glyph (→●)** with the flight's count. We never let a player discover by accident that their attack-move spent four airframes. On the first three occasions per profile, committing a one-way attack-move also shows a 1.2 s toast: `ONE-WAY · 4 AIRFRAMES`.

**Queueing.** Add waypoints by tapping before lift (PC: Shift). Abilities queue the same way. Queued orders draw as 1 pt white ticks with sequence numerals; a queued *ability* draws its own glyph at its waypoint.

**Formations.** Three, set per control group, cycled from the group chip's long-press menu (PC: `Ctrl+F`): **Column** (road movement, tight, fast), **Line** (assault), **Dispersed** (60 m spacing, default for anything moving inside a stained area). Dispersed materially reduces splash and cluster-munition losses; it is the doctrine lesson, and making it the automatic default inside the stain teaches it without a word.

**Rally points.** Drag from a production structure to set; ground units and crews walk there. Air units have no rally — they live in the hangar — but a structure *can* be given a **default sortie waypoint**, drawn as a hollow chevron, which pre-fills the launch corridor for flights from that building. Useful for routing around a known dome.

**Smart-cast** is **on by default** for zero-ambiguity single-target and self abilities (Terminal Commit, Dash, Dig In, Strip, stance switches, Silent Running) and **off** for Pins, Ferry and both supers. One options toggle flips the whole set; there is no per-ability matrix.

### 8.3 Control groups without a keyboard

The **group rail** on the left edge: five 56 pt chips.

- The **first three are automatic and permanent**: `◈ HARVEST` (all Recovery UGVs), `◉ EYES` (all recon-stance units and Designator Teams), `▲ AIRBORNE` (every flight currently up). These cover most of what manual grouping is used for in a C&C-lineage game and they require zero setup, which matters enormously for a touch player.
- The last two are user slots. **Long-press an empty slot assigns the current selection**; long-press a full slot offers Reassign / Clear.
- **Tap** = select. **Double-tap** = select and centre camera. The chip shows the group's dominant icon and a live count; it turns amber when any member is under fire and red when a member dies.

PC keeps `1`–`0` with `Ctrl+n` to set, and the rail mirrors slots 1–5.

### 8.4 The two super-abilities

One 72 pt button in the Ops strip with a ring timer (7:00 cooldown), a `U` hotkey on PC, and a mandatory two-stage commit.

**ARCLIGHT (Kestrel).** Arm → every friendly recon footprint lights as a **legal region** and everything else greys; the 250 m reticle turns red and refuses outside. The superweapon is gated by the recon layer and the UI makes that gate physical — the clearest possible statement of thesis 3. On commit: a 45 s inbound arc from the map edge, a countdown ring, and at T−5 s the circle's contents highlight so the victim can still run.

**BLACKOUT (Obsidian).** Arm → a 900 m ghost dome follows the finger and **pre-flashes consequences**: enemy pips that would sever flicker black at 50% alpha, while their fiber and autonomous units do not — showing exactly what Blackout will *not* solve. Friendly units inside gain a `--link-auto` tick. Commit gives the defender a 1.5 s dome expansion, drawn as a hard white edge, before the 60 s timer starts; a superweapon you can partly dodge produces play.

---

## 9. Unit and structure cards

### 9.1 Hierarchy

The selection card reads top to bottom in strict priority: **who → how hurt → whether it still works → what it is → what it does.**

```
 ┌───────────────────────────────────────────────────────────┐ 329×184
 │ ┌──────┐  FIBER FPV TEAM              ▮▮▮ ACE   ×4 in sel │
 │ │      │  ▰▰▰▰▰▰▰▰▰▰▰▰▰░░░  70 / 90                       │
 │ │ IMG  │  ● LINK ▮F fiber · leash 940/1400 m   [KZ] crew  │
 │ │ 64pt │  ⬢ Ar air-rotary        ◆ Shp shaped charge      │
 │ └──────┘  strong ▲▲ ⬡Top  ▲ ▭Lt      weak ▼▼ ⬢Ar  ▼ ▬St  │
 │ ┌────┐┌────┐┌────┐┌────┐                                  │
 │ │THRD││TERM││HOLD││ ✕  │   ability row, 64 pt, QWER on PC │
 │ └────┘└────┘└────┘└────┘                                  │
 └───────────────────────────────────────────────────────────┘
```

1. **Name + veterancy chevrons + selection count.**
2. **One HP bar**, segmented every 500 HP so size reads as toughness, with `current / max` in 13 pt mono.
3. **Link row**: the three-state pip, the link stripe glyph, and the link's one scarce quantity — fiber leash used, satellite capacity, mesh hops, or nothing for autonomy. Plus the crew callsign chip if a crew is consumed.
4. **Class row**: armour-class shape glyph + damage-type glyph.
5. **Matchup row** (§9.3).
6. **Ability row**: up to four 64 pt buttons.

Multi-selection replaces the portrait with a **chip grid** grouped by type with counts; the leftmost type is focused and owns the card. Tapping a chip focuses that type; long-pressing removes that type from the selection.

### 9.2 Icon language

| Concept | Encoding |
|---|---|
| **Link type** | 6 pt stripe: hue + pattern + letter. Radio `R` solid-dotted blue · Mesh `M` diamond-lattice teal · Fiber `F` unbroken gold · Satellite `S` chevron violet · Autonomy `A` bone hatch |
| **Armour class** | Silhouette shape: Soft `●` circle · Light `▭` chamfered rect · Heavy `⬢` hexagon · Structure `▬` slab · Air-rotary `◆` diamond · Air-fixed `▲` triangle. **Top facet** `⬡` hollow hexagon, used only in matchup rows |
| **Damage type** | Small mark inside the class shape: Shp cone · Frg starburst · AP wedge · Inc flame · Ram double-chevron |
| **Pilot slot** | A single 10 pt headset glyph on the card and on the unit's selection ring. **Absent means crew-free**, which is the important state and gets the quieter treatment on purpose |
| **Source building** | 10 pt corner glyph on every flight and build card |
| **Stance** | The ability button itself is the state: the active stance is filled, the others outlined |

### 9.3 Damage versus class, without a table

Two rows, four glyphs, no numbers:

```
strong  ▲▲ ⬡Top      ▲ ▭Lt          weak  ▼▼ ⬢Ar        ▼ ▬St
```

Only the **best two** and the **worst two** matchups are shown, as class shapes with arrow counts (▲▲ ≥ 1.8×, ▲ 1.2–1.8×, ▼ 0.5–0.8×, ▼▼ < 0.5×). Everything else is average and unmentioned. The full multiplier row lives in the Inspect sheet as a single horizontal strip of shapes with numerals — six numbers, one row, not a grid.

Structures use the same card with HP, power draw as a `⚡` count, and a **provides** line instead of a matchup row (`+4 CREWS`, `HANGAR 12`, `JAM J70 r450`, `SUPPLY 200 · 700 m`). Structures under construction show the radial timer and the queue position.

---

## 10. Alerts and audio-visual feedback

### 10.1 Priority tiers

| Tier | Examples | Presentation | Interrupts? |
|---|---|---|---|
| **P0** | Command Post under attack · Crew Quarters destroyed · enemy super inbound · Grid Control countdown begins | Full-width 64 pt banner below the top bar, 2.5 s, `--bad` edge, siren, heavy double haptic, camera-jump offered as a tap on the banner | **Yes.** One at a time; a second P0 queues. Never auto-moves the camera |
| **P1** | Node cut off · structure under attack · flight going black · Mothership / Uplink Terminal hit · package lost | Alert card in the stack, 6 s, minimap ping, one CONTROL line, medium haptic | No |
| **P2** | Kill verified · build complete · crew rank up · thread found · route interdicted | Alert card, 4 s, soft tick, no haptic | No |
| **P3** | Salvage cluster depleted · decoy expired · pin expired | Event log and CONTROL ticker only | No |

Rate limits, enforced globally: **maximum 3 alert cards per 4 s**; identical alerts within 8 s merge and show a count (`×3`); CONTROL never speaks twice inside 2.5 s and drops the lower-priority line rather than queueing it. A game that narrates every event teaches players to mute it, and a muted game loses the P0 siren, which is the one line that must land.

### 10.2 Unit voice lines

Three roles, and no others:

1. **CONTROL** (the operator voice, faction-neutral, calm, clipped) carries state changes: *"Link lost." "Crew recovering." "Position cut off." "Kill verified." "Night in twenty."* This is the EVA slot and it does the heavy lifting.
2. **Crew chatter** on selection and order, per link type — a radio crew is noisy and compressed, a fiber crew is clean and close-miked, a satellite crew has a 200 ms delay before answering. **The audio tells you the link before the pip does.** This is the single best idea in the audio design and it costs three variant sets.
3. **Autonomous units do not speak at all.** Selecting one gives a machine acknowledgement tone. The silence is the characterisation, and it makes an autonomy-heavy army feel genuinely different to command.

Barks are rate-limited to one per 1.2 s per player, never repeat the same line twice running, and drop entirely while a P0 is speaking.

### 10.3 Night visuals

Night is a 1:45 phase where optical detection falls to 35%, so the screen must read as *dark but legible*:

- Terrain luminance drops 45% and saturation 30%; **UI panel luminance does not change at all** (a HUD that dims at night is a HUD that fails at night).
- Friendly units keep full-strength allegiance outlines. Enemy units, once detected, draw at full strength too — we never make the *information* darker, only the ground.
- Undetected space gains a subtle 2% blue-grey grain to distinguish "dark" from "fogged".
- Thermal-equipped units draw a small 8 pt thermal glyph, and while Thermal Optics is owned their detection footprint keeps its daytime radius ring — the purchase is visible on the map the second it completes.
- Muzzle flashes, thermite and explosions are allowed to bloom at night (they are the only bloom in the game) and are capped at 30% screen luminance for photosensitivity.

### 10.4 Colourblind-safe encoding

**Four link types + autonomy**, each with three redundant encodings:

| Link | Hue | Pattern | Letter | Motion |
|---|---|---|---|---|
| Radio | `#3E86D6` | dotted, 2/3 | `R` | — |
| Mesh | `#2FA08C` | diamond lattice | `M` | hop numerals |
| Fiber | `#D79B2A` | unbroken solid | `F` | physical thread on the ground |
| Satellite | `#9B7BE0` | chevrons | `S` | up-arrow |
| Autonomy | `#D8DCD4` | 45° hatch | `A` | no line at all |

Hue alone is never load-bearing: the stripe pattern is visible at 6 pt and the letter at 11 pt. In protan and deutan simulation, fiber-gold and mesh-teal separate cleanly; radio-blue and satellite-violet do not, which is exactly why they carry *dots* and *chevrons*.

**Three armour classes** carry **shape** as the primary encoding (circle / chamfered rectangle / hexagon), hue as secondary, never hue alone. Shape survives every colour vision deficiency, greyscale capture, and a 10 pt icon at arm's length on an iPad.

**Allegiance** is the one place we allow a hue default, and we back it up: self `#41B5D9` solid 2 pt outline, ally `#4ED39A` solid outline + a 6 pt ally tick, enemy `#E2503F` **double outline**, neutral `#B9BEB5` dashed outline. The high-contrast allegiance option (§13.3) adds a hatch fill to every enemy unit.


---

## 11. Menus and meta

### 11.1 Main menu

A single full-bleed operational map still with a five-item vertical list, left-aligned, 72 pt rows on iPad (thumb-reachable in the left arc): **CAMPAIGN · SKIRMISH · MULTIPLAYER · THE RANGE · REPLAYS**, with `OPTIONS` and `PROFILE` as 56 pt icon buttons in the bottom-right. Continue-campaign is the default focus. No carousels, no news feed larger than a 300 pt strip, no animated backgrounds beyond a 0.08 Hz parallax on the map still (disabled by Reduced Motion).

### 11.2 Skirmish setup

One screen, three columns on iPad, a three-step pager on iPhone:

- **Map** — a thumbnail grid with, on each thumbnail, the two base pockets, the Grid Taps, and a **predicted contested band** drawn as a translucent stripe. That stripe is a map-design promise, and showing it in the lobby sets the right expectation about where the game happens.
- **Sides** — faction picker (two 160 pt cards with the one-sentence fantasy from §12 of the gameplay spec), colour, and AI opponent with a **doctrine picker**: `RAIDER` / `JAMMER` / `AUTONOMY` and a difficulty slider. Naming the AI's doctrine rather than only its difficulty is honest about the scripted-doctrine approach the gameplay spec flags in §18.6 and turns a limitation into a feature.
- **Rules** — victory conditions as three toggles (CP kill / Grid Control / Annihilation), match speed, and **Era modifiers** as a single dropdown drawn from `spec-futures-2027-2028.md` §6.2: *I Pilot War · II The Loop Closes · III The Magazine War · IV The Relay Grid · V Flank Contingency*, with III and IV stackable and V exclusive. Each era shows a one-line rule summary and a `T4` badge when it enables Release Authority. Default is **I + II**, matching the recommended ranked set.

### 11.3 Campaign map and briefing

The campaign is linear, so the map is an **operational strip**: nine nodes along a front, completed ones stamped, the next one lit. No strategic layer; no carry-over beyond the gameplay spec's crews-with-veterancy and 50% TP. Two panels hang off the strip: the **crew roster** (callsign, rank, kills, preferred airframe — the only place we let players get attached to crews, which pays off in Decapitation and No Hands) and the **catalogue** of carried-over purchases.

The **briefing screen** names the mission's mechanic in a 22 pt strip — `THIS MISSION: THE RECON GATE`. Every mission teaches exactly one thing; saying so out loud converts a campaign into a course without a single tutorial popup.

### 11.4 Post-match report

Five tabs. The **Summary** tab leads with four headline numbers, large, because they are the four numbers this game is about:

| Headline | Definition |
|---|---|
| **SORTIE RATE** | Sorties launched per minute, with your crew-utilisation percentage under it |
| **KILLS VERIFIED** | `n verified / m total`, with the verified share as a ring |
| **EXPOSURE TAKEN** | Unit-seconds your units spent inside an area where enemy reaction time was shorter than their exposure — the game's own measure of how recklessly you moved |
| **TP EARNED** | With a breakdown bar: kills / pins / grid |

Other tabs: **Sortie** (per-type launched / lost / hit rate, crew ranks gained, a link-column histogram showing which columns of the ladder you actually flew), **Economy** (MAT income curve vs opponent, salvage from debris vs fields, TP spend), **Losses** (every loss with cause — jammed, intercepted, snagged, shot, misidentified — this is where the autonomy misidentification count is reckoned), and **Timeline** (a horizontal band per player with tech gates, supers, and engagements as pips sized by value swing).

### 11.5 Replay viewer

Ships day one, per the gameplay spec's tuning philosophy. Scrubber with **event pips** colour-coded by tier, speeds 0.5 / 1 / 2 / 4 / 8, free camera, and a **fog selector** (`MINE / THEIRS / NONE`) — the feature that makes a replay a teaching tool rather than a highlight reel. Link, Supply and Threat overlays work in replay and can be switched to the *opponent's* memory, so you can watch your own stain being wrong. The **per-engagement value-swing readout** the gameplay spec asks for runs along the bottom: one block per engagement, height = MAT swing, colour = who won it, tap to jump. It is the tuning instrument and, accidentally, the best coaching tool we can ship.

---

## 12. Onboarding

### 12.1 The first ten minutes for a C&C or StarCraft veteran

The goal is: **no tutorial, no confusion, one surprise at minute six.**

| Time | What they do | What we do |
|---|---|---|
| 0:00–0:40 | Recognise the shape. Tap the spine, place a Generator and a Salvage Yard, send Recovery UGVs | Nothing. The genre does the teaching. Build cards look like build cards, the queue looks like a queue |
| 0:40–2:00 | Notice the Flight Line and tap a card, then the map | Their first sortie. The armed state, the reticle and the crews-consumed count all appear without explanation. One 1.4 s toast on the very first sortie only: `AIRFRAMES LAUNCH FROM THE HANGAR` |
| 2:00–3:30 | First contact over the middle. Kill something | The verified-kill bracket snaps and `+84 TP` flies to the top bar. They now know there is a second currency and that watching things die pays |
| 3:30–5:00 | Tap cards faster than crews recover | **Every flight card greys at once.** No text. The crew strip is the only green-to-amber thing on the screen and their eye goes to it. This is the pillar-2 lesson and it arrives as a consequence, not a lecture |
| 5:00–6:30 | Build a Radar Mast, see a red dome appear | The dome is drawn as terrain. They will fly into it anyway |
| 6:30–7:30 | Radio flight goes amber, then black, then falls | The one scripted-feeling moment: on a player's first black-link loss, CONTROL says *"Link lost — they are jamming."* and the TASKING badge appears. This is the only line of explicit instruction in a skirmish match |
| 7:30–10:00 | Arm a Fiber FPV and watch the break rings vanish (§4.5) | The ladder lands. They understand that the counter to a unit is a *column*, and they have understood it by tapping two cards |

### 12.2 The first ten minutes for a touch player who has never played C&C

Different problem: they do not know what a build queue is, and they will try to drag units around like chess pieces.

We add exactly three affordances, all of them silent:

1. **The map starts zoomed one stop in**, on the base, with two Recovery UGVs already harvesting. The economy is demonstrated before it is delegated.
2. **First-run gesture coaching on the map itself**, not in a dialogue: the first time the player touches the map, a 1.8 s ghost-hand animation shows two-finger pan and pinch, drawn on the terrain, dismissed by any input. It never appears again.
3. **The empty Flight Line is not empty.** Before any airframe exists, slot 1 shows a hollow card reading `BUILD A DRONE WORKSHOP` with the workshop's icon; tapping it opens the Build drawer at that structure. The hangar advertises its own prerequisite. Same device covers the empty group rail and the locked TASKING tab.

Beyond that, the new player is routed to **The Range** from the main menu, which is where the actual teaching happens.

### 12.3 The Range — a three-mission arc

Each is a real, winnable, ~10-minute mission with a score, not a click-here sequence.

**R1 — ROAD HEAD (7 min, no base).** Four vehicles and a Scout Quad must cross the band. The player has exactly one verb: drag a move order. Teaches **exposure, reaction and the drag-preview**, and it teaches them by killing the truck that takes the green-looking route through an `UNKNOWN` band. Success is 3 of 4 across. *(This is the gameplay spec's Kestrel mission 1, promoted to the tutorial arc.)*

**R2 — SORTIE (9 min).** A base, a workshop, one target list. Teaches the **hangar, crews, the two-tap sortie, packages and recovery**. The mission's difficulty curve is a crew curve: the target list demands more simultaneous sorties than six crews can sustain, so the player must build Crew Quarters to finish. Nobody is told this.

**R3 — RED DOME (12 min).** An EW Post kills their radio flights. Teaches **the pip, the dome, break rings, fiber and the Tasking catalogue**. Two winning routes exist — buy fiber, or buy AI Last-Mile and push through — and the debrief names which one the player took, which is how we introduce the idea that the ladder has more than one rung at each height.

Completing all three unlocks nothing mechanical. It awards a profile stamp, and it sets the default skirmish difficulty one notch up.

### 12.4 Contextual tips policy

- A tip is **one line, ≤ 9 words, on the CONTROL ticker or as a 1.4 s toast**. Never a dialogue, never a pause, never an OK button.
- **One tip per 45 s, eight per match, none while more than six units are selected or a P0 is live.** A player who is busy is a player who is not reading.
- Each tip fires **once per profile, ever**, and is afterwards browsable in the **Doctrine index** (Options → Doctrine), a list of every tip, glyph and overlay. Documentation lives there, not in the match.
- Tips stop firing in multiplayer after the first three matches.

---

## 13. Accessibility and options

### 13.1 Input and remapping

- **Full remap** of every keyboard binding and mouse button, with five presets: `KILL ZONE DEFAULT`, `C&C CLASSIC`, `STARCRAFT GRID`, `LEFT-HAND`, `CUSTOM`.
- **Touch options:** swap one-finger pan / marquee; long-press threshold 250–800 ms; double-tap window 200–400 ms; **handedness mirror**, flipping the whole HUD left-right — cheap, because every region in §2 is anchored rather than centred.
- **Dwell-free:** no gesture holds longer than 450 ms except Link View and the minimap overlay, and `STICKY HOLDS` converts every hold into tap-on / tap-off.
- **One-handed mode** on iPhone moves the flight line and selection chip to the grip side.

### 13.2 Scaling and layout

UI scale **80–130%** in 5% steps, independent of resolution, with the map viewport absorbing the difference. A separate **text size** control of ±2 steps affects text only, so a player can keep 88 pt cards and read 15 pt labels. **Compact HUD** drops the persistent minimap and collapses the crew strip, recovering 6.6% of the iPad frame. All three are live-previewed on a mock match still, never applied blind.

### 13.3 Colour and motion

- **Colourblind modes**: Protan / Deutan / Tritan, each a remap of the token table in §14.2 rather than a screen-space filter (filters ruin the threat stain). Every mode is validated against the §10.4 rule that hue is never the sole encoding.
- **High-contrast allegiance**: enemy units gain a hatch fill and a double outline; friendly units gain a solid 3 pt outline. Recommended for streaming as well as for low vision.
- **Reduced motion**: all panel transitions become ≤ 80 ms cross-fades; the day/night dial steps rather than sweeps; the pin ring stops rotating; alert banners do not slide; screen shake is zero; camera auto-jumps never happen (the P0 banner still offers a *tap to jump*).
- **Photosensitivity**: global cap of 3 Hz on any flashing element, explosion bloom capped at 30% screen luminance, and a `REDUCE FLASHES` option that converts every flash to a static outline change.

### 13.4 Screen reader and audio scope

Honest scoping: **VoiceOver is complete for all non-match UI** — main menu, skirmish setup, campaign map, briefing, post-match report, replay controls, options, Doctrine index — with a label, trait and hint on every control and the post-match report navigable as a table.

**In match, VoiceOver is out of scope**, and we say so rather than ship something unusable. What ships in match is a **status readout** on one bindable control (PC `Ctrl+Space`) speaking, in order: materiel and delta, TP, ready crews of total, highest-priority active alert, Grid Control state — plus **full subtitles for every CONTROL line and crew bark** with speaker labels in a 300 pt band above the flight line, and the event log as readable history.

### 13.5 External keyboard (iPad and PC)

An iPad with a Magic Keyboard and a PC use the same bindings.

| Binding | Action |
|---|---|
| `Z X C V B` / `Shift+Z…B` | Flight Line slots 1–10 (bottom keyboard row = bottom of the screen) |
| `Q W E R` | Ability slots 1–4 of the current selection |
| `A` / `S` / `H` / `F` | Attack-move / Stop / Hold / Ferry |
| `P` | Pin (then click), `U` Super (arm) |
| `1`–`0`, `Ctrl+n` | Control groups / assign |
| `F1`–`F6` | Sidebar tabs: Build, Defence, Crew, Ground, Air, Tasking |
| `Tab` (hold) / `Shift+Tab` (hold) | Link View / Supply View |
| `V` / `G` / `` ` `` | Threat overlay / supply toggle / exposure-always |
| `Backspace` | Recall selected flight |
| `Space` / `Space×2` | Jump to last alert / last loss |
| `,` / `.` | Cycle idle Recovery UGV / idle crew |
| Arrows, MMB-drag, edge scroll | Camera |

Every binding is shown on its on-screen control as an 11 pt chip whenever a keyboard is attached, and hidden when it is detached.

### 13.6 Controller stub

Not shipping at Early Access; designed so it can be added without a layout change. The model: **left stick = cursor with magnetism** toward units and legal targets; right stick = camera; `LB`/`RB` page the flight line, face buttons fire slots 1–4; `LT` held opens the context radial on the right stick; `RT` confirms. Control groups map to the d-pad, which is exactly five slots — the same five as the group rail, which is why the rail is five and not eight.

---

## 14. Visual style direction for UI

### 14.1 Typography roles

| Role | Face | Use | Sizes (iPad / PC) |
|---|---|---|---|
| **Display** | Barlow Condensed 600/700, uppercase, +0.01em | Panel headers, unit names, card labels, verdict words | 22 / 17 / 13 pt |
| **Body** | Barlow 400/500 | Briefings, options, catalogue descriptions, subtitles | 15 / 13 pt |
| **Numeric** | IBM Plex Mono 500, **tabular figures always** | Every live number without exception: MAT, TP, timers, HP, exposure seconds, counts | 20 / 14 / 11 pt |
| **Eyebrow** | Barlow Condensed 600, uppercase, +0.14em | Section labels, tab names, `EST`, `VERIFIED` | 11 pt |

**Rule: any number that changes is mono and tabular.** Proportional digits jitter, and a jittering resource counter is read as an animation rather than as a value. The 11 pt floor applies to mono numerals only; all other mid-match text has a 13 pt floor.

### 14.2 Colour tokens

```
 SURFACE                            SEMANTIC
 --hud-ground    #0E1512            --ok            #5FA45A
 --hud-surface   #16201C            --warn          #D7A22A
 --hud-raised    #1E2A25            --bad           #C4463F
 --hud-line      #2E3C36            --info          #4E8FD0
 --ink           #E6EAE4
 --ink-2         #A7B1A9            EXPOSURE / THREAT
 --ink-mute      #6E7A73            --exposure-clear   #4F8F4A
                                    --exposure-tight   #C98A1E
 LINKS                              --exposure-exposed #C4463F
 --link-radio    #3E86D6            --stain            #B23A3A
 --link-mesh     #2FA08C
 --link-fiber    #D79B2A            ALLEGIANCE
 --link-sat      #9B7BE0            --self    #41B5D9
 --link-auto     #D8DCD4            --ally    #4ED39A
                                    --enemy   #E2503F
 FACTION (menus and campaign only)  --neutral #B9BEB5
 --kestrel       #2C68A8  accent #9FC4E8
 --obsidian      #3B3F45  accent #E06A2C
```

Two rules govern the palette. **First, faction colours never appear in match.** Kestrel blue and Obsidian ember are branding — menus, briefings, loading screens, the campaign strip. In match, allegiance colour overrides everything, because "is that mine" must not depend on which faction you picked. **Second, `--bad` and `--enemy` are the only reds in the product.** Every other warning state is `--warn` amber. This is the rule that makes the threat stain and the exposure ribbon legible at a glance, and it will be violated by somebody in month four; it should be a lint rule in the UI toolkit.

### 14.3 Iconography rules

- One weight (2 pt at 24 pt icon size), one corner radius (2 pt), no gradients, no drop shadows on icons, no perspective. Everything is drawn as if stencilled onto equipment.
- Unit icons are **top-down silhouettes** at the same optical weight as the unit's map sprite, so the card icon and the thing on the map are recognisably the same object.
- Structure icons are **isometric outlines**, deliberately different from units, so a build card is never mistaken for a unit card.
- **A glyph means exactly one thing across the whole product.** The crossed headset means "no crew" on a flight card, in a tooltip, in the post-match report and in the Doctrine index. We maintain a glyph registry and adding a glyph requires deleting one or proving no existing glyph fits.
- Icons never carry text except the link letters (R/M/F/S/A) and the slot chips.

### 14.4 Motion rules

| Motion | Duration | Curve |
|---|---|---|
| Panel / drawer slide | 160 ms | `cubic-bezier(0.2, 0, 0, 1)` |
| Card state change, card lift | 70–90 ms | ease-out |
| Sortie commit flash | 140 ms | linear out |
| Alert card in / out | 160 / 220 ms | ease-out / ease-in |
| P0 banner in | 180 ms, hold 2.5 s, out 260 ms | ease-out |
| Exposure ribbon in / out | 90 / 60 ms | linear |
| TP ghost numeral | 400 ms rise + count | ease-out |
| Verified bracket snap | 140 ms | ease-in (it should feel like a shutter) |
| Overlay enter / exit | 120 / 90 ms | ease |
| Threat stain bloom / decay redraw | 400 ms / continuous at 4 Hz | linear |

**Nothing in the HUD exceeds 260 ms.** Nothing loops except the day/night dial and an active P0. The camera never moves without an explicit input — a game that steals the camera on an alert is a game that loses a base while apologising for it.

---

## 15. Open questions

1. **Does the 88 pt flight card survive a real thumb?** The middle cards of the bottom row sit at the far edge of both thumb arcs. If testers regrip to reach slots 3 and 8, the block splits into two five-card clusters anchored to the bottom corners and the selection card moves up.

2. **One-finger marquee versus one-finger pan on iPad.** We chose marquee because this is an RTS; every shipped touch strategy game chose pan. If new players fail the first minute, we flip the default and make marquee a hold-then-drag.

3. **Is the Build Drawer's 2.5 s auto-collapse right?** Too fast and a player laying four Net Traps fights the UI; too slow and the map stays covered. Candidate fix: collapse on the next map order, not on a timer.

4. **Does the threat stain survive a 200 pt minimap?** Three contour bands may be four pixels apart on iPhone. Fallback is two bands there.

5. **Can the reaction estimate be honest and fast?** It needs a per-area model of enemy readiness recomputed at 8 Hz along a dragged route. If that is too expensive on an A-series chip mid-battle, the fallback is 2 Hz with 250 ms interpolation, which will feel laggy under the finger.

6. **The iPhone flight ceiling.** Five paged cards means a Tier-3 Obsidian player with nine card types pages mid-fight. Either cap iPhone complexity, add a user-defined favourites page, or state plainly that iPhone is a campaign and 1v1 device.

7. **Does TP earn its top-bar slot at 393 pt?** It costs 160 of 734 usable points on iPhone. The alternative is a badge on the build FAB; §2.6 argues against, but iPhone may force it.

8. **Two-stage super commit versus one tap.** Competitive players will find arm-then-commit slow, and a 7:00 cooldown makes a mis-tap catastrophic. If tournament feedback demands it, add a `FAST SUPERS` option rather than change the default.

9. **Where do Era modifiers surface in match?** Era IV adds a map-wide Link Layer overlay that overlaps our Link View. The `◎` slot holds a fifth overlay, but if Eras III and IV both add HUD state we need an era strip, and the top bar is full.

10. **Haptics budget.** At 40 taps a minute the specified haptics may be a constant buzz that drains battery and attention. The 6-per-3-s limit is a guess and should be measured before it is defended.

11. **Does the crew roster make Decapitation cruel rather than dramatic?** Naming and ranking crews across nine missions is the campaign's emotional engine, and also why losing four permanently may read as punishment for playing well. Needs a playtest with real attachment, not an argument.

