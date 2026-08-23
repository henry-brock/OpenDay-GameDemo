using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// Minimal save/load for the booth demo's "Resume" flow, backed by
    /// PlayerPrefs. Remembers which level scene to return to and which
    /// module keys had been collected.
    /// </summary>
    public static class SaveSystem
    {
        private const string HasSaveKey = "OpenDay.HasSave";
        private const string LevelBuildIndexKey = "OpenDay.LevelBuildIndex";
        private const string CollectedModulesKey = "OpenDay.CollectedModules";
        private const string MusicVolumeKey = "OpenDay.MusicVolume";

        private static CSModule[] _pendingResumeModules;

        public static bool HasSave => PlayerPrefs.GetInt(HasSaveKey, 0) == 1;

        /// <summary>Music volume, 0–1. Defaults low-ish; an open-day booth is already loud.</summary>
        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.6f);
            set
            {
                PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        public static void Save(int levelBuildIndex, IEnumerable<CSModule> collectedModules)
        {
            var mask = 0;
            foreach (var module in collectedModules)
            {
                mask |= 1 << (int)module;
            }

            PlayerPrefs.SetInt(LevelBuildIndexKey, levelBuildIndex);
            PlayerPrefs.SetInt(CollectedModulesKey, mask);
            PlayerPrefs.SetInt(HasSaveKey, 1);
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out int levelBuildIndex, out CSModule[] collectedModules)
        {
            if (!HasSave)
            {
                levelBuildIndex = 0;
                collectedModules = Array.Empty<CSModule>();
                return false;
            }

            levelBuildIndex = PlayerPrefs.GetInt(LevelBuildIndexKey, 0);
            var mask = PlayerPrefs.GetInt(CollectedModulesKey, 0);
            collectedModules = Enum.GetValues(typeof(CSModule))
                .Cast<CSModule>()
                .Where(module => (mask & (1 << (int)module)) != 0)
                .ToArray();
            return true;
        }

        /// <summary>Queues collected modules for the next level's <see cref="PlayerInventory"/> to pick up on load.</summary>
        public static void SetPendingResume(CSModule[] modules)
        {
            _pendingResumeModules = modules;
        }

        public static bool TryConsumePendingResume(out CSModule[] modules)
        {
            modules = _pendingResumeModules ?? Array.Empty<CSModule>();
            var hadPending = _pendingResumeModules != null;
            _pendingResumeModules = null;
            return hadPending;
        }
    }
}
