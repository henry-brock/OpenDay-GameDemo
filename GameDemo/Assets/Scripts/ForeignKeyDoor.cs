using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// A floor-level reader that opens its barrier only when the player is
    /// carrying the primary key it references — the foreign-key relationship
    /// that gives Level 3 its name. Bringing the wrong key does nothing.
    ///
    /// The reader sits at the player's height while the barrier it controls is
    /// a tall wall, so interaction stays within the player's reach.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to a floor-level GameObject beside the barrier.
    ///   2. Set Required to the table whose primary key opens it, and assign
    ///      Barrier to the wall GameObject.
    /// </summary>
    public class ForeignKeyDoor : MonoBehaviour, IInteractable
    {
        [Tooltip("The table whose primary key this foreign key references.")]
        [SerializeField] private TableKey required;

        [Tooltip("The wall this reader opens.")]
        [SerializeField] private GameObject barrier;

        [SerializeField] private Color openColor = new Color(0.25f, 0.75f, 0.35f, 0.25f);

        private bool _isOpen;

        public void Interact()
        {
            if (_isOpen)
            {
                return;
            }

            var holder = FindFirstObjectByType<KeyHolder>();
            if (holder == null || holder.Held != required)
            {
                Debug.Log($"This foreign key references {required} — bring that table's primary key.");
                return;
            }

            holder.Consume();
            Open();
        }

        private void Open()
        {
            _isOpen = true;
            Debug.Log($"{required} relationship satisfied — door open.");

            if (barrier == null)
            {
                return;
            }

            // Fades the wall out and stops it blocking, so the way through reads as open.
            if (barrier.TryGetComponent<Collider2D>(out var barrierCollider))
            {
                barrierCollider.enabled = false;
            }

            if (barrier.TryGetComponent<SpriteRenderer>(out var barrierRenderer))
            {
                barrierRenderer.color = openColor;
            }
        }
    }
}
