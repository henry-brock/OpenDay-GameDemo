using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// Tracks the single primary key the player is currently carrying. Picking
    /// up a new key puts the previous one back on its pedestal, so the player
    /// has to plan which key to bring to which door.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to the Player GameObject.
    /// </summary>
    public class KeyHolder : MonoBehaviour
    {
        private PrimaryKey _held;

        public TableKey Held => _held != null ? _held.Table : TableKey.None;

        public void Take(PrimaryKey key)
        {
            if (_held == key)
            {
                return;
            }

            // Only one key at a time — the old one goes back where it came from.
            if (_held != null)
            {
                _held.SetCarried(false);
            }

            _held = key;
            key.SetCarried(true);
            Debug.Log($"Carrying the {key.Table} primary key.");
        }

        /// <summary>Spends the carried key on a matching foreign-key door.</summary>
        public void Consume()
        {
            if (_held == null)
            {
                return;
            }

            Destroy(_held.gameObject);
            _held = null;
        }
    }
}
