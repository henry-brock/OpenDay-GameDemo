# Open Day Game Demo — Design Document

*Expands on the one-line brief (`../Planning.txt`): a 2D Unity game for a
university open day, where each Computer Science module becomes a level and a
"key part" of that module becomes a gameplay feature.*

---

## 1. Vision & context

A short, approachable 2D game that lets a prospective student **play through the
CS degree**. Each level is a *taster* of a module, and the signature idea of that
module is turned into a mechanic (Databases → physical keys, Data Structures →
stacks/queues you stand on, etc.). The goal is to make the breadth of the course
tangible and fun in a few minutes at a booth.

### Design drivers (because it's an open-day booth)
- **Short sessions.** A full run should be a few minutes; individual levels
  1–3 minutes. People play in passing.
- **Pick-up-and-play on a controller.** Input is a **Microsoft/Xbox pad**
  (keyboard/WASD kept only as a dev fallback). No manual required — teach through
  play and on-screen prompts.
- **Low failure friction.** Dying should cost seconds, not progress. This is a
  showcase, not a challenge (the deliberately-hard capstone level was cut —
  see §3 scope decision).
- **Legible theming.** A player who has *never* studied CS should still have fun;
  a player who *has* should get the joke. Each mechanic maps to a real concept.

### Design pillars
1. **Every mechanic teaches a concept.**
2. **Readable at a glance** (clear art, clear goals, controller prompts).
3. **Self-contained levels** that share a core movement/collectible engine.

### Scope rule (short-term project)
- **One module = one short level.** Not a tour of the whole syllabus.
- **One signature mechanic per level** — pick the *single* concept from that
  module that makes the best game, and cut the rest. A module's other concepts
  are only worth including if they genuinely improve the level as a game, not for
  completeness. When a concept doesn't translate to fun platforming, drop it.

---

## 2. Core systems (shared across all levels)

These are built once and reused:

- **Player movement** — **2D platformer** (run + jump), controller-first.
  *(scaffolded: `PlayerController.cs`)*
- **Module "keys" / collectibles** — the recurring motif. Each level yields one
  themed collectible per paired module (two per level, except Databases which
  has one) that represents "you learned this module." *(scaffolded:
  `Collectible.cs`, `PlayerInventory.cs`, `CSModule.cs`)*
- **HUD** — keys collected, current objective, optional timer.
- **Level manager** — loads levels, tracks win/lose, handles transitions.
- **Interaction system** — reuse the existing `Interact` input action for
  doors, switches, terminals, NPCs.

> **Movement model — DECIDED: 2D platformer** (run + jump). Levels are built as
> side-scrolling platformers; puzzles use gravity, jumping, and standing on
> structures (a natural fit for OOP object-instancing and DS&A stacks/queues).
> The `Jump` action is wired to the Xbox **A** button.

---

## 3. Level plan

> **SCOPE DECISION (locked):** the demo ships with exactly **three playable
> levels**, each pairing two related modules so all five surviving modules fit:
> - **Level 1 — Object-Oriented Programming & Data Structures and Algorithms**
> - **Level 2 — Software Engineering & Software Project Management**
> - **Level 3 — Databases**
>
> **Team Project is cut as a level and repurposed into the Title Screen**
> instead of being built out as the capstone/hard level. **AI: Vision and
> Reality** is also out of scope and was never built. Each level now yields
> **two** collectible keys instead of one (one per paired module) —
> `PlayerInventory` already supports collecting more than one key per level
> with no changes needed.

Difficulty rises across the three built levels, ending with Databases; there is
no capstone "hard level" in the current scope.

> **Detailed per-level notes** (derived from the actual module lecture slides)
> live in `docs/levels/`. The DS&A and SPM detail docs
> ([06](levels/06-data-structures-and-algorithms.md),
> [07](levels/07-software-project-management.md)) describe mechanics now folded
> into Levels 1 and 2 below rather than standalone levels.

> **Open question — ordering:** the list below is your requested order. Note DS&A
> and OOP are usually *early* in a real degree. Current order is fine as a
> *difficulty* curve for the three built levels.

### Level 1 — Object-Oriented Programming & Data Structures and Algorithms *(intro / tutorial level)*
Two collectible keys: an **OOP key** and a **DS&A key**. Because it's the first
level, it doubles as the movement tutorial.

**OOP thread**
- **Concept focus:** classes vs. objects, instantiation, inheritance,
  encapsulation, polymorphism.
- **Signature mechanic — "instantiate objects from a blueprint":** find a
  **class blueprint**, then spawn **object instances** (crates/platforms/helpers)
  to solve traversal puzzles.
- **Key-part features:**
  - *Encapsulation* → private rooms behind walls; the only way in is a public
    "method" switch (you can't touch the private field directly).
  - *Inheritance* → step on a parent "class" pad to inherit an ability (e.g. a
    child object gains the parent's ability to open certain doors).
  - *Polymorphism* → one button, context-dependent behaviour on different objects.

**DS&A thread**
- **Concept focus:** stacks, queues, trees, graphs, sorting, searching, recursion,
  Big-O.
- **Signature mechanic — "structures you stand on":** the environment *is* the
  data structure.
- **Key-part features:**
  - *Stack / queue* → LIFO/FIFO lifts and conveyor platforms.
  - *Sorting* → arrange weighted crates in order to open a gate.
  - *Search* → a binary-search door puzzle (halve the search space each guess).
  - *Graph/tree* → a maze that is literally a graph; find the path (bonus:
    shortest path rewarded).
  - *Recursion* → rooms nested within rooms.

- **Win:** collect both the OOP key and the DS&A key to reach the exit.

### Level 2 — Software Engineering & Software Project Management
Two collectible keys: an **SE key** and an **SPM key**.

**SE thread**
- **Concept focus:** the software lifecycle (requirements → design → build → test
  → deploy), debugging, version control.
- **Signature mechanic — "ship a build through the pipeline":** progress through
  rooms that mirror SDLC stages; each stage gates the next.
- **Key-part features:**
  - *Requirements* → gather spec fragments before a door will accept your build.
  - *Bugs* → literal **bug enemies**; squashing/avoiding them is the hazard.
  - *Version control* → **commit checkpoints** you can revert to on death
    (cheap, on-theme respawns).
  - *Testing* → a gate that only opens if your "build" passes (a small puzzle
    check) — ship a broken build and it bounces back.

**SPM thread**
- **Concept focus:** planning, scheduling (Gantt/critical path), resource &
  budget allocation, risk, Agile sprints.
- **Signature mechanic — "allocate limited resources against a deadline":** more
  strategy than action — sequence tasks along a **critical path**, spend limited
  resources, manage risk events.
- **Key-part features:**
  - *Critical path* → tasks with dependencies; wrong order wastes time.
  - *Resources/budget* → a limited pool to spend on completing tasks.
  - *Risk* → random risk events you mitigate with contingency.
  - *Sprints* → timed rounds of work.

- **Win:** collect both the SE key and the SPM key, pass tests, deploy.

### Level 3 — Databases *(the brief's headline example)*
- **Concept focus:** tables, primary/foreign keys, relationships, queries,
  normalization, indexing.
- **Signature mechanic — "keys unlock relationships":** rooms are **tables**;
  collect a **primary key** in one table to open the matching **foreign-key
  door** in a related table.
- **Key-part features:**
  - *Queries* → a `SELECT`/filter terminal reveals/hides which platforms exist
    (filter by a value to make a path appear).
  - *Relationships* → bridges only form between correctly related tables.
  - *Indexing* → an index acts as a fast-travel/shortcut once discovered.
- **Win:** chain keys across tables to reach the final record; collect the DB key.

### Cut modules — Team Project & AI: Vision and Reality
> Neither built, and neither planned. Team Project's slot became the **Title
> Screen** (see §7). Kept below only as reference in case scope ever expands.

**Team Project (would-be hard level):** collaboration, merge conflicts, scope
creep, deadlines — "coordinate multiple workers" combining mechanics from other
levels as a capstone.

**AI: Vision and Reality:** computer vision, perception, classification —
"seen vs. unseen" stealth against vision-based detectors, exploiting false
positives/negatives.

---

## 4. Screens & flow

**Required (your list):**
- **Title Screen** — game logo, Start, (Settings), (Quit). Controller-navigable.
- **Win Screen** — celebrates completion; shows keys/modules collected; option to
  replay or return to title.
- **Lose Screen** — encouraging, quick retry / back to level. (Given "low failure
  friction," most levels may *not* hit this — consider whether Lose is a
  full screen or just an in-level respawn. Probably reserve a full Lose screen for
  the Team Project / SPM levels, and use cheap checkpoint respawns elsewhere.)

**Flow:**
`Title → L1 → L2 → L3 → Win`
(Lose loops back to the current level's last checkpoint. No Hub/level-select
and no Levels 4–7 in current scope — see §3.)

---

## 5. "Anything else?" — recommended additions

Things worth planning now, roughly in priority order for a booth demo:

**High value:**
- **Hub / level select** — a small **campus map** you walk around, each building =
  a module. Ties the levels together thematically and is very on-brand for an open
  day. Also lets a visitor jump straight to a module they care about.
- **Tutorial / onboarding via prompts** — no manual; teach controls with on-screen
  **Xbox button prompts** in Level 1.
- **Pause menu** — resume / restart level / quit to title. Essential for a booth
  (staff can reset a session).
- **HUD** — keys collected + current objective (already listed in core systems).
- **Attract / idle mode** — after N seconds of no input, return to the Title (or
  play a short demo reel). Keeps the booth tidy between visitors. *Genuinely
  important for unattended kiosks.*

**Medium value:**
- **Settings screen** — master/SFX/music volume; the booth environment is loud.
- **Level transitions / loading screens** — a "loading next module…" beat, could
  show a one-line real fact about that module.
- **Credits screen** — it's a dissertation artefact; credit yourself and the tools.
- **Consistent "degree progress" framing** — the collected module keys visibly
  build toward "graduation" on the Win screen (narrative glue).

**Lower value for a booth (consider skipping):**
- **Save/continue** — sessions are short and public; probably unnecessary.
- **Accessibility** — colourblind-safe palettes and difficulty easing are nice,
  but at minimum keep detection cones/hazards distinguishable without colour.

---

## 6. Suggested build order (de-risking the scope)

Scope is locked at **three levels** (§3), so the build order is short:

1. **Core systems** — movement (done), collectibles (done), HUD, level manager,
   Title/Win/Lose screens. *This proves the whole loop end-to-end.*
2. **Level 3 (Databases)** — the headline concept, and the key/door mechanic is
   the simplest to prototype. Best first level to fully build. *(done)*
3. **Level 1 (OOP + DS&A)** — doubles as the tutorial once movement is
   finalised. *(scene + both collectibles scaffolded)*
4. **Level 2 (SE + SPM)** *(scene + both collectibles scaffolded)*
5. **Title Screen** — repurposed from the cut Team Project slot (§3). *(scene
   scaffolded; see `Assets/Editor/LevelBuilder.cs` → `BuildTitleScreen`.)*
6. **Pause, attract mode, settings** — layer in as time allows.

Team Project and AI: Vision and Reality are **not** part of the build — see the
scope decision in §3.

**Minimum viable demo:** Title → L1 → L2 → L3 → Win/Lose. Everything else is
expansion.

---

## 7. Decisions needed
- [x] Movement model: **2D platformer** (run + jump). *(§2)*
- [x] Scope: **locked at 3 levels**, each pairing two modules — L1: OOP + DS&A,
      L2: SE + SPM, L3: Databases. Team Project cut and repurposed as the Title
      Screen; AI: Vision and Reality out of scope. *(§3)*
- [ ] Level ordering: keep difficulty order, or mirror real course timeline? *(§3)*
- [ ] Lose = full screen everywhere, or cheap checkpoints + Lose only on hard
      levels? *(§4)*
- [x] Hub/level-select: **no** — flow is `Title → L1 → L2 → L3 → Win`. *(§4)*
- [ ] Art direction / who is the player character? (a student?)
