# First gameplay mechanic — setup

**2D platformer** movement (Xbox controller + keyboard) and a themed collectible.
Input comes from a **Microsoft/Xbox controller** — left stick / D-pad to move,
**A button** to jump; WASD + Space still work for testing without the pad.

## Scripts
- `CSModule.cs` — enum of the CS modules the demo is themed around.
- `PlayerController.cs` — 2D platformer movement (run + jump), driven by the
  `Move` and `Jump` actions.
- `PlayerInventory.cs` — tracks collected module features; raises `ModuleCollected`.
- `Collectible.cs` — a pickup (first level: a Databases "key") collected on touch.

## Wiring it up in the Unity Editor

### Player
1. Create a GameObject named **Player**, add a `SpriteRenderer` (any sprite).
2. Add `Rigidbody2D` → **Gravity Scale = 3**, **Freeze Rotation Z**,
   **Interpolate = Interpolate**, **Collision Detection = Continuous**.
3. Add a `Collider2D` (e.g. `CapsuleCollider2D`).
4. Create an empty **child** GameObject named **GroundCheck**, position it at the
   player's feet.
5. Add `PlayerController` and `PlayerInventory`. On `PlayerController`:
   - Assign **Ground Check** = the GroundCheck child.
   - Set **Ground Layer** to the layer your ground/tilemap uses.
6. Add a **PlayerInput** component:
   - Actions = `InputSystem_Actions`
   - Default Map = `Player`
   - **Behavior = Send Messages**  (so `OnMove` / `OnJump` get called)
7. Set the Player GameObject's **Tag** to `Player`.

### Ground
- Put your floor/platforms on a dedicated layer (e.g. "Ground") and set the
  player's **Ground Layer** mask to match, or the jump won't work.

### Collectible (database key)
1. Create a GameObject with a `SpriteRenderer` (a key sprite).
2. Add a `Collider2D` with **Is Trigger** enabled.
3. Add `Collectible`; leave **Module = Databases** for the first level.

## Verify
Press Play. Left stick / WASD moves the player, **A / Space** jumps (only when
grounded — select the Player to see the yellow ground-check gizmo). Walk into the
collectible — it disappears and the Console logs `Collected Databases key (1 total).`

## Controller notes
The default `InputSystem_Actions` asset already binds `Move` to
`<Gamepad>/leftStick` and the D-pad, with a dedicated **Gamepad** control scheme,
so an Xbox pad works with no extra setup. If you later want rumble or button
prompts, that reads from the same asset.
