using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// Tracks which CS-module features (e.g. database keys) the player has collected.
    /// Lives on the Player GameObject alongside <see cref="PlayerController"/>.
    /// UI and level logic can subscribe to <see cref="ModuleCollected"/>.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        /// <summary>Raised whenever a new module feature is collected.</summary>
        public event Action<CSModule> ModuleCollected;

        private readonly HashSet<CSModule> _collected = new();

        public bool Has(CSModule module) => _collected.Contains(module);

        public int Count => _collected.Count;

        /// <summary>Every module collected so far, for saving or display.</summary>
        public IReadOnlyCollection<CSModule> Collected => _collected;

        private void Awake()
        {
            // Resuming from a save: the title screen queues the modules the
            // player already had before this level loaded.
            if (SaveSystem.TryConsumePendingResume(out var modules))
            {
                foreach (var module in modules)
                {
                    _collected.Add(module);
                }
            }
        }

        /// <summary>
        /// Records a module as collected. Returns false if it was already held
        /// (so callers can ignore duplicate pickups).
        /// </summary>
        public bool Collect(CSModule module)
        {
            if (!_collected.Add(module))
            {
                return false;
            }

            Debug.Log($"Collected {module} key ({_collected.Count} total).");
            ModuleCollected?.Invoke(module);
            return true;
        }
    }
}
