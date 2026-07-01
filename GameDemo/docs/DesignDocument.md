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
  showcase, not a challenge — except the deliberately Hard level.
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
- **Module "keys" / collectibles** — the recurring motif. Each level yields a
  themed collectible that represents "you learned this module." *(scaffolded:
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

Difficulty rises across the run, peaking at the **Team Project** (Level 4) as the
deliberate "hard level," then easing into more strategic levels.

> **Detailed per-level notes** (derived from the actual module lecture slides)
> live in `docs/levels/`. So far: [Level 6 — DS&A](levels/06-data-structures-and-algorithms.md)
> and [Level 7 — SPM](levels/07-software-project-management.md). The summaries
> below are the overview; the `docs/levels/` files are the working detail.

> **Open question — ordering:** the list below is your requested order. Note DS&A
> and OOP are usually *early* in a real degree while Team Project is *late*.
> Current order is fine as a *difficulty* curve; flag if you want it to mirror the
> actual course timeline instead.

### Level 1 — Object-Oriented Programming *(intro / tutorial level)*
- **Concept focus:** classes vs. objects, instantiation, inheritance,
  encapsulation, polymorphism.
- **Signature mechanic — "instantiate objects from a blueprint":** find a
  **class blueprint**, then spawn **object instances** (crates/platforms/helpers)
  to solve traversal puzzles. Because it's the first level, it doubles as the
  movement tutorial.
- **Key-part features:**
  - *Encapsulation* → private rooms behind walls; the only way in is a public
    "method" switch (you can't touch the private field directly).
  - *Inheritance* → step on a parent "class" pad to inherit an ability (e.g. a
    child object gains the parent's ability to open certain doors).
  - *Polymorphism* → one button, context-dependent behaviour on different objects.
- **Win:** instantiate the right object to reach the exit; collect the OOP key.

### Level 2 — Software Engineering
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
- **Win:** pass tests, deploy, collect the SE key.

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

### Level 4 — Team Project **(Hard Level)**
- **Concept focus:** collaboration, coordination, communication, integration,
  merge conflicts, scope creep, deadlines.
- **Signature mechanic — "coordinate multiple workers":** control/juggle several
  characters or tasks at once (echoes OOP object-instancing but under pressure).
  This level **combines mechanics from Levels 1–3** — the "capstone."
- **Key-part features:**
  - *Merge conflicts* → hazards that appear when two work-streams collide; you
    must resolve them.
  - *Standups* → checkpoints/sync points.
  - *Scope creep* → an escalating hazard (rising water / spreading mess) forcing
    momentum.
  - *Roles* → different workers have different abilities; you must delegate.
- **Win:** integrate all components before the deadline; collect the Team key.
- **Note:** intentionally the difficulty spike. Keep failure cheap even here so
  booth players don't rage-quit — "hard" via complexity, not punishment.

### Level 5 — AI: Vision and Reality
- **Concept focus:** computer vision, perception, classification, training data,
  and the gap between AI hype ("vision") and messy "reality."
- **Signature mechanic — "seen vs. unseen":** stealth against **vision-based
  detectors** (guard/camera detection cones = object detection).
- **Key-part features:**
  - *Training data* → feed a classifier examples to change what it recognises
    (reprogram a guard to ignore you / target something else).
  - *Reality vs. vision twist* → the AI mispredicts; exploit **false
    positives/negatives** to slip past.
  - *Perception* → line-of-sight, occlusion, moving out of frame.
- **Win:** exploit the model's blind spots to reach the goal; collect the AI key.

### Level 6 — Data Structures & Algorithms
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
- **Win:** solve the structure puzzles to the exit; collect the DS&A key.

### Level 7 — Software Project Management *(strategic finale)*
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
- **Win:** deliver the project on time and on budget; collect the final key.

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
`Title → [Level Select / Hub?] → L1 → L2 → … → L7 → Win`
(Lose loops back to the current level's last checkpoint.)

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

Building all 7 levels is a lot. Recommend a **vertical slice** first:

1. **Core systems** — movement (done), collectibles (done), HUD, level manager,
   Title/Win/Lose screens. *This proves the whole loop end-to-end.*
2. **Level 3 (Databases)** — the headline concept, and the key/door mechanic is
   the simplest to prototype. Best first level to fully build.
3. **Level 1 (OOP)** — doubles as the tutorial once movement is finalised.
4. Then **6 (DS&A)**, **2 (SE)**, **5 (AI)**.
5. **7 (SPM)** and **4 (Team Project, Hard)** last — they reuse mechanics from the
   others, so build them once the parts exist.
6. **Hub, pause, attract mode, settings** — layer in once ≥2 levels are playable.

**Minimum viable demo:** Title → Hub → 1 fully-built level → Win/Lose. Everything
else is expansion.

---

## 7. Decisions needed
- [x] Movement model: **2D platformer** (run + jump). *(§2)*
- [ ] Level ordering: keep difficulty order, or mirror real course timeline? *(§3)*
- [ ] Lose = full screen everywhere, or cheap checkpoints + Lose only on hard
      levels? *(§4)*
- [ ] Hub/level-select **yes/no** (affects Title flow). *(§5)*
- [ ] Art direction / who is the player character? (a student?)
