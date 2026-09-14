# Research brief: grounding the sensor and signature model

KILL ZONE is a **video game** — a real-time strategy game in the Command &
Conquer tradition, set in a fictional near-future Eastern Europe, for iPad and
iPhone first. Nothing in this repository is operational guidance; it is
simulation code with hit points, build costs and cooldown timers.

What these research documents are for: the game now models detection across five
channels, and every number in that model was estimated rather than researched.
The numbers below need grounding in published, open-source reporting so that the
game's behaviour is recognisable to anyone who knows the subject, and so that
whoever tunes it later knows which figures are load-bearing and which were
guesses.

## Before you start: how research actually works in this environment

**Do not attempt WebFetch, curl, or any direct page retrieval. It cannot work,
and every attempt is wasted time.** This is not per-site blocking or flakiness
that a different domain or a retry might get past.

The environment's network policy denies outbound HTTPS to everything except a
package-registry allowlist. Verified directly: `pypi.org` and
`registry.npmjs.org` return 200, and every content domain tried - MDPI, arXiv,
ISW, Wikipedia, Euromaidan Press, Robin Radar, United24, War on the Rocks,
GlobeNewswire, Space.com - fails identically, with the proxy logging
`gateway answered 403 to CONNECT (policy denial)` for each. The failure is at the
gateway, before the site is ever contacted, so the identity of the site is
irrelevant.

Earlier research passes burned a lot of effort discovering this one domain at a
time, and several documents record it as though particular publishers were
blocking them. They were not.

**So: WebSearch is the only channel.** It runs server-side and works normally.
Everything you write therefore rests on search-result snippets rather than
article text. That is a real limit on how much weight a figure can carry and it
must be stated in your document, not buried:

- Mark snippet-derived figures as snippet-derived.
- Prefer a figure that appears in several independent snippets over one that
  appears in a single richer-looking one, since you cannot open either.
- When a snippet gives a number without its units, its date, or what it is
  measuring, say so rather than inferring the missing part.
- Do not cite a URL as though you read the page. You did not.

**Budget.** WebSearch is capped per session and shared across every agent running
at once. Your brief will name your allocation. Spend it on the load-bearing
questions; a document that answers three questions well is worth more than one
that touches nine and lands on none.

## What the game currently assumes

Reach is in **map metres**. The game compresses real distance roughly twelve to
one, so a figure of 100 map metres stands in for something on the order of a
kilometre of real ground. Treat that as elastic: what matters is the *ratio*
between sensors and between targets, not the absolute number.

### Sensor reach (map metres, against a target at full strength on that channel)

| Platform | Optical | Thermal | Acoustic | Radar | Passive RF |
|---|---|---|---|---|---|
| Gun Mount | 600 | — | 130 | — | — |
| Radar Mast | — | — | — | 1400 | 900 |
| Interceptor Battery | — | 500 | — | 800 | 600 |
| Recon Wing | 900 | — | — | — | — |
| Night Bomber | 300 | 400 | — | — | — |
| Main Tank | 360 | 300 | — | — | — |
| Command Post | 400 | — | — | — | 500 |

### Signatures (0–100, per channel)

| Unit | RF | Thermal | Acoustic | Visual | Radar |
|---|---|---|---|---|---|
| Small electric quad (FPV) | 70 | 8 | 70 | 15 | 22 |
| Fiber-optic quad | **0** | 8 | 70 | 15 | 22 |
| Electric heavy multirotor | 60 | 22 | 95 | 55 | 55 |
| Fixed-wing recon | 55 | 25 | 25 | 30 | 40 |
| Combustion loitering munition | 50 | 45 | 35 | 25 | 35 |
| Combustion heavy strike drone | 0 | 60 | 55 | 45 | 60 |
| Turbojet strike drone | 0 | 85 | 70 | 40 | 55 |
| Decoy drone (reflectors) | 0 | 25 | 35 | 30 | **80** |
| Main battle tank | 0 | 90 | 85 | 90 | — |
| Jammer, transmitting | 100 | 40 | 20 | 70 | — |

### Rules currently applied

- Reach scales with **√(signature/100)**, except radar which uses the **fourth
  root**, on the basis that radar range goes as RCS^(1/4).
- **Optical** loses 65% of its reach at night.
- **Thermal** is scaled 0.55 by day, 1.25 at night, 0.90 at twilight, on the
  basis that solar loading destroys thermal contrast.
- **Acoustic** is cut to 35% against high-altitude targets; **radar** gains 20%;
  **optical** loses 30%; ground weapons lose 40% of their reach shooting upward.
- Detection is **solid inside 60% of reach** and **intermittent beyond it**, with
  per-channel reliability: passive RF 95, optical 88, thermal 84, radar 78,
  acoustic 52.
- A track once acquired is **held for two seconds** after the sensor loses it.

## What each research document should deliver

1. **What is actually reported**, with sources and dates, for detection ranges,
   degradation factors and target signatures in your area. Prefer measured
   figures and vendor specifications over commentary; say which is which.
2. **Where the game's assumption above is wrong**, specifically, with the number
   it should be instead and the reasoning.
3. **The near-future trajectory** — what changes by 2027–28, and what that should
   do to the model.
4. **What is genuinely uncertain or contested**, so nobody later mistakes a guess
   for a finding.

Write for a designer who has to pick a number and defend it, not for a
specialist. Do not invent citations. Where you extrapolate, say so.
