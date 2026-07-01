# Level 7 — Software Project Management

*One level, one signature mechanic. Distilled from the module decks (Weeks 1/2/4,
Estimation, Risk); raw slides live in `docs/reference/` (not committed).*

**Scope:** short project — a single platformer level. SPM is inherently a
*management* topic, so the challenge is finding the **one** idea that works as an
action game rather than a spreadsheet. We take the module's spine — **deliver
before the deadline, despite risks** — and nothing else.

## Signature mechanic: race the deadline (time pressure + risk hazards)

**A timed platformer level = the project deadline.** A countdown runs the whole
level. The player races to the exit ("ship the project") while **risk events**
periodically trigger hazards that cost time or block the path. It turns SPM's core
tension — *time vs. delivery* — into pure platforming urgency.

Why this one:
- **The deadline is the game** — a visible countdown is instantly understood and
  creates real tension at a booth.
- **Risk = level design, not a menu** — falling rocks, a closing gate, a delay
  trap. "Risk management" becomes dodging/mitigating hazards on the fly.
- **Avoids the trap** of turning the finale into a dry resource-allocation sim.

### Level flow (short)
1. Start the countdown; the exit is "delivery".
2. Along the way, **risk hazards** fire (e.g. a slippage trap that steals seconds,
   a blocked route forcing a detour = the "critical path" slipping).
3. Optional: a couple of **branching routes** — a risky shortcut vs. a safe longer
   path (the estimation/critical-path trade-off, expressed as level geometry).
4. Reach the exit before time runs out → collect the final **SPM key**
   (`CSModule.SoftwareProjectManagement`). Run out of time → Lose (fits this
   level having a real fail state, unlike the gentler early levels).

## Cut (didn't make the level)
- **QCTS four-meter balancing, bottom-up estimation, CPA, Waterfall-vs-Agile,
  monitoring/control** — all core to the *module*, but they're management-sim
  mechanics, not platformer mechanics. At this scope they'd make the level dry.
  We keep only their *feeling*: a deadline (Time) and risks.
- Keep this distinct from **Level 4 (Team Project)**: Team Project = people/
  coordination; SPM = beat-the-clock delivery under risk.
