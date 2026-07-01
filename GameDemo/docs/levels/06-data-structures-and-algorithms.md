# Level 6 — Data Structures & Algorithms

*One level, one signature mechanic. Distilled from the module notes
(`dsaNotes1-14.pdf`); raw slides live in `docs/reference/` (not committed).*

**Scope:** short project — this is a single self-contained platformer level, not a
tour of all 14 units. We pick the **one** data structure that makes the best game
and build the level around it.

## Signature mechanic: the Stack (LIFO)

**Block-stacking platformer.** The player picks up and **pushes** blocks onto a
stack to build stairs/bridges and reach higher ground — but can only take the
block off the **top** (**pop**). Solving a room = pushing and popping blocks in
the right order to shape a path to the exit.

Why this one:
- **Tactile and visual** — LIFO is *shown*, not explained. You physically see the
  last-in-first-out rule when you can't grab a buried block.
- **Genuinely a game** — block-stacking is a proven puzzle-platformer mechanic.
- **Readable in seconds** — a booth player gets it without a tutorial.

### Level flow (short)
1. A gap too wide to jump → push 2 blocks to make steps.
2. A block you need is *under* another → you must pop the top one first (the
   "aha" that teaches LIFO).
3. Final climb built from the stack → reach the exit, collect the **DS&A key**
   (`CSModule.DataStructuresAndAlgorithms`).

## Cut (didn't make the level)
Deliberately **not** included — they're either not game-worthy at this scope or
would dilute the one clear idea:
- **Queue (FIFO)** — the closest alternative; could be a *single* moving-conveyor
  section if the stack level feels thin, but keep it minor.
- Trees, hash tables, sets, maps, lists, recursion, arrays, linked structures —
  interesting concepts, weak/duplicative as standalone platformer mechanics here.
- **Big-O / sorting / binary search** — puzzle-y but more "brain-teaser" than
  platformer; save for a different game.
