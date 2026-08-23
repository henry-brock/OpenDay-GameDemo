using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// A table's primary key, sitting on its pedestal until the player picks it
    /// up with Interact. The player can only carry one at a time (see
    /// <see cref="KeyHolder"/>), so choosing which key to carry to which
    /// foreign-key door is the puzzle.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to a GameObject with a SpriteRenderer.
    ///   2. Pick which table this key belongs to in the inspector.
    /// </summary>
    public class PrimaryKey : MonoBehaviour, IInteractable
    {
        [Tooltip("Which table this primary key belongs to.")]
        [SerializeField] private TableKey table;

        private SpriteRenderer _renderer;

        public TableKey Table => table;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void Interact()
        {
            var holder = FindFirstObjectByType<KeyHolder>();
            if (holder != null)
            {
                holder.Take(this);
            }
        }

        /// <summary>Hides the key on its pedestal while the player is carrying it.</summary>
        public void SetCarried(bool carried)
        {
            if (_renderer != null)
            {
                _renderer.enabled = !carried;
            }
        }
    }
}
