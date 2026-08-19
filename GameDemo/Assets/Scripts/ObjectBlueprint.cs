using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// A "class blueprint" the player can activate (Interact) to instantiate a
    /// physical crate — literally spawning an object instance, the OOP half of
    /// this puzzle. Crates fall and land on whatever's below them, including
    /// each other, so activating this a few times stacks them into a climbable
    /// tower — the DS&amp;A "stack" half of the puzzle.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to a GameObject marking the blueprint's spot.
    ///   2. Assign Crate Sprite (any sprite) and Ground Layer (the layer the
    ///      player's ground check looks for, so crates are standable).
    /// </summary>
    public class ObjectBlueprint : MonoBehaviour
    {
        [SerializeField] private Sprite crateSprite;
        [SerializeField] private Color crateColor = new Color(0.6f, 0.42f, 0.2f);
        [SerializeField] private Vector2 crateSize = new Vector2(1.2f, 1.2f);
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 3f, 0f);
        [SerializeField] private int maxCrates = 4;
        [SerializeField] private int groundLayer;

        private int _spawnedCount;

        /// <summary>Instantiates one crate above this blueprint, up to <see cref="maxCrates"/>.</summary>
        public void Spawn()
        {
            if (_spawnedCount >= maxCrates)
            {
                return;
            }

            var crate = new GameObject($"Crate_{_spawnedCount}");
            crate.transform.position = transform.position + spawnOffset;
            crate.transform.localScale = new Vector3(crateSize.x, crateSize.y, 1f);
            crate.layer = groundLayer;

            var renderer = crate.AddComponent<SpriteRenderer>();
            renderer.sprite = crateSprite;
            renderer.color = crateColor;

            var rb = crate.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;

            crate.AddComponent<BoxCollider2D>();

            _spawnedCount++;
        }
    }
}
