using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// A themed pickup representing one CS module's feature — for the first level,
    /// a physical "key" for the Databases module. When the player walks into it,
    /// the module is added to their <see cref="PlayerInventory"/> and the pickup
    /// disappears.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to a GameObject with a SpriteRenderer.
    ///   2. Add a Collider2D with "Is Trigger" enabled.
    ///   3. Give the Player GameObject the tag set in <see cref="playerTag"/> ("Player").
    ///   4. Pick which CS module this collectible represents in the inspector.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        [Tooltip("Which CS module this pickup represents.")]
        [SerializeField] private CSModule module = CSModule.Databases;

        [Tooltip("Tag on the player GameObject that can collect this.")]
        [SerializeField] private string playerTag = "Player";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            // Collect through the player's inventory; ignore if we can't find one.
            if (other.TryGetComponent(out PlayerInventory inventory) && inventory.Collect(module))
            {
                Destroy(gameObject);
            }
        }
    }
}
