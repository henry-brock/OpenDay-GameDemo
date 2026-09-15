# Sprite Assets

All sprites are loaded by the level builder from `Assets/Sprites/` using a
type-based folder convention. If a sprite for a type isn't found, it falls
back to a procedurally-generated white square.

## Folder structure

```
Assets/Sprites/
├── default/               (used by all visual elements if no type-specific sprite exists)
│   └── [sprite.png]       (single sprite file — name doesn't matter, first .png is loaded)
├── SPRITES.md             (this file)
└── Square.png             (procedural white square, auto-generated if none of the above exist)
```

## Current sprite types in use

The builder tries to load these types in this order; if a type folder doesn't
exist or has no .png files, it falls back to `default/`; if `default/` is
also empty, it uses the white square.

| Type | Used by | Notes |
|---|---|---|
| `default` | Everything (fallback) | Replace this single folder to restyle the whole game at once. Single .png file — name doesn't matter. |
| `player` | Player character | Recommended: ~90×160px (scaled to 0.9×1.6 in-game) |
| `platform` | Floors and walls | Can be any size — scaled to match the platform dimensions |
| `collectible` | CS module keys (Databases, SE, SPM, OOP, DS&A) | Recommended: ~80×80px. Colour-tinted per module — provide white/neutral and let the builder colour it. |
| `crate` | Crates spawned by Level 1 puzzle | Recommended: ~48×48px |
| `stage` | Pipeline terminals in Level 2 | Recommended: ~64×64px |
| `gate` | Deploy gate in Level 2 | Can be any size — scaled to match the gate dimensions |
| `key` | Primary keys in Level 3 (Customers, Orders) | Recommended: ~48×48px. Colour-tinted per table. |
| `door` | Foreign-key barriers in Level 3 | Can be any size — scaled to match the barrier dimensions |

## How to replace sprites

1. **Quick restyle (recommended):** Drop a single sprite file into
   `Assets/Sprites/default/` — this becomes the fallback for everything,
   so you can restyle the whole game without creating 9 separate folders.
   Filename doesn't matter; the first .png found in the folder is loaded.

2. **Type-specific:** Create subfolders like `Assets/Sprites/player/`,
   `Assets/Sprites/platform/`, etc., and place your sprites in each. The
   builder loads the first .png from each type's folder.

3. **No replacement:** Leave the folders empty, and the white square is
   used everywhere.

## Technical details

- Sprites are loaded at **build time** (when you run **OpenDay → Build
  Levels** from the Unity Editor menu), not at runtime.
- Changes to sprite files are picked up on the next rebuild — no need to
  restart the Editor.
- The builder looks for `.png` files specifically; other formats are
  ignored (use Unity's Import settings to convert `.jpg`, `.psd`, etc. to
  `.png` first if needed).
- **Caching:** once loaded, a sprite is cached in memory for the build
  session, so repeated uses of the same type don't re-load from disk.
  Clear the cache (e.g. by restarting the Editor) if you need to pick up a
  changed sprite file mid-session.

## Recommended workflow

1. Start with a single `default/` sprite to restyle the whole game at once.
2. Once you're happy with the overall look, add type-specific sprites to
   `player/`, `platform/`, etc. as needed for extra polish.
3. The builder will load the more specific one if it exists, falling back
   to `default/` otherwise.
