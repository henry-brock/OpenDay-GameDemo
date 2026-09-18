using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace OpenDay.Tests.EditMode
{
    /// <summary>
    /// Shared helpers. Private serialised fields are set by reflection rather than
    /// SerializedObject so these tests carry no UnityEditor dependency, and Awake is
    /// invoked explicitly because Unity does not run it on AddComponent outside Play mode.
    /// </summary>
    internal static class TestHelpers
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        internal static void SetField(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, Instance);
            Assert.NotNull(info, $"Field '{field}' not found on {target.GetType().Name}");
            info.SetValue(target, value);
        }

        // Safe to call even if Unity already ran Awake: the methods under test are
        // idempotent (a second pending-resume consume simply returns false).
        internal static void InvokeAwake(MonoBehaviour target)
        {
            target.GetType().GetMethod("Awake", Instance)?.Invoke(target, null);
        }
    }

    public class PlayerInventoryTests
    {
        private GameObject _player;
        private PlayerInventory _inventory;

        [SetUp]
        public void Setup()
        {
            PlayerPrefs.DeleteAll();
            _player = new GameObject("TestPlayer");
            _inventory = _player.AddComponent<PlayerInventory>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_player);
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void Collect_NewModule_ReturnsTrueAndAddsToCollected()
        {
            Assert.IsTrue(_inventory.Collect(CSModule.ObjectOrientedProgramming));
            Assert.IsTrue(_inventory.Has(CSModule.ObjectOrientedProgramming));
        }

        [Test]
        public void Collect_DuplicateModule_ReturnsFalseAndDoesNotDuplicate()
        {
            _inventory.Collect(CSModule.SoftwareEngineering);

            Assert.IsFalse(_inventory.Collect(CSModule.SoftwareEngineering));
            Assert.AreEqual(1, _inventory.Count);
        }

        [Test]
        public void Has_ModuleNotCollected_ReturnsFalse()
        {
            Assert.IsFalse(_inventory.Has(CSModule.Databases));
        }

        [Test]
        public void Collected_ReflectsExactlyWhatWasCollected()
        {
            _inventory.Collect(CSModule.ObjectOrientedProgramming);
            _inventory.Collect(CSModule.Databases);

            Assert.AreEqual(2, _inventory.Collected.Count);
            Assert.IsTrue(_inventory.Has(CSModule.ObjectOrientedProgramming));
            Assert.IsTrue(_inventory.Has(CSModule.Databases));
            Assert.IsFalse(_inventory.Has(CSModule.SoftwareProjectManagement));
        }

        [Test]
        public void Awake_WithPendingResume_RestoresThoseModules()
        {
            SaveSystem.SetPendingResume(new[]
            {
                CSModule.DataStructuresAndAlgorithms,
                CSModule.SoftwareEngineering
            });

            var resumed = new GameObject("ResumedPlayer");
            var resumedInventory = resumed.AddComponent<PlayerInventory>();
            TestHelpers.InvokeAwake(resumedInventory);

            Assert.AreEqual(2, resumedInventory.Count);
            Assert.IsTrue(resumedInventory.Has(CSModule.DataStructuresAndAlgorithms));
            Assert.IsTrue(resumedInventory.Has(CSModule.SoftwareEngineering));

            Object.DestroyImmediate(resumed);
        }
    }

    public class SaveSystemTests
    {
        [SetUp]
        public void Setup()
        {
            // PlayerPrefs persists on disk between runs, not just between scenes,
            // so without this a previous run's state leaks into this one.
            PlayerPrefs.DeleteAll();
            SaveSystem.TryConsumePendingResume(out _);
        }

        [TearDown]
        public void Teardown()
        {
            PlayerPrefs.DeleteAll();
            SaveSystem.TryConsumePendingResume(out _);
        }

        [Test]
        public void Save_ThenTryLoad_RoundTripsLevelIndexAndModules()
        {
            SaveSystem.Save(1, new[] { CSModule.ObjectOrientedProgramming, CSModule.Databases });

            Assert.IsTrue(SaveSystem.TryLoad(out var levelIndex, out var modules));
            Assert.AreEqual(1, levelIndex);
            Assert.AreEqual(2, modules.Length);
            Assert.Contains(CSModule.ObjectOrientedProgramming, modules);
            Assert.Contains(CSModule.Databases, modules);
        }

        [Test]
        public void HasSave_FalseInitially_TrueAfterSave()
        {
            Assert.IsFalse(SaveSystem.HasSave);

            SaveSystem.Save(0, new CSModule[0]);

            Assert.IsTrue(SaveSystem.HasSave);
        }

        [Test]
        public void TryLoad_NoSaveExists_ReturnsFalse()
        {
            Assert.IsFalse(SaveSystem.TryLoad(out _, out _));
        }

        [Test]
        public void SetPendingResume_ThenConsume_ReturnsModulesOnce()
        {
            SaveSystem.SetPendingResume(new[] { CSModule.SoftwareEngineering });

            Assert.IsTrue(SaveSystem.TryConsumePendingResume(out var first));
            Assert.AreEqual(1, first.Length);

            // The clear-after-read is what stops a save leaking into a fresh game.
            Assert.IsFalse(SaveSystem.TryConsumePendingResume(out _));
        }

        [Test]
        public void MusicVolume_ClampsToZeroOneRange()
        {
            SaveSystem.MusicVolume = 1.5f;
            Assert.AreEqual(1f, SaveSystem.MusicVolume, 0.0001f);

            SaveSystem.MusicVolume = -0.5f;
            Assert.AreEqual(0f, SaveSystem.MusicVolume, 0.0001f);

            SaveSystem.MusicVolume = 0.6f;
            Assert.AreEqual(0.6f, SaveSystem.MusicVolume, 0.0001f);
        }
    }

    public class PipelineGateTests
    {
        private GameObject _gateObject;
        private PipelineGate _gate;

        [SetUp]
        public void Setup()
        {
            _gateObject = new GameObject("TestGate");
            _gateObject.AddComponent<SpriteRenderer>();
            _gateObject.AddComponent<BoxCollider2D>();
            _gate = _gateObject.AddComponent<PipelineGate>();
            TestHelpers.SetField(_gate, "requiredStages", 3);
            TestHelpers.InvokeAwake(_gate);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gateObject);
        }

        [Test]
        public void StageCompleted_FewerThanRequired_GateStaysClosed()
        {
            _gate.StageCompleted();
            Assert.IsFalse(_gate.IsOpen);

            _gate.StageCompleted();
            Assert.IsFalse(_gate.IsOpen);
        }

        [Test]
        public void StageCompleted_ExactlyRequired_GateOpens()
        {
            _gate.StageCompleted();
            _gate.StageCompleted();
            _gate.StageCompleted();

            Assert.IsTrue(_gate.IsOpen);
        }

        [Test]
        public void ResetProgress_AfterPartialProgress_RequiresFullSequenceAgain()
        {
            _gate.StageCompleted();
            _gate.StageCompleted();
            _gate.ResetProgress();

            _gate.StageCompleted();
            _gate.StageCompleted();
            Assert.IsFalse(_gate.IsOpen, "Reset should have discarded the earlier progress");

            _gate.StageCompleted();
            Assert.IsTrue(_gate.IsOpen);
        }
    }

    public class KeyHolderTests
    {
        private GameObject _playerObject;
        private KeyHolder _holder;

        [SetUp]
        public void Setup()
        {
            _playerObject = new GameObject("TestPlayer");
            _holder = _playerObject.AddComponent<KeyHolder>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_playerObject);
        }

        private static PrimaryKey CreateKey(TableKey table)
        {
            var go = new GameObject($"Key_{table}");
            go.AddComponent<SpriteRenderer>();
            var key = go.AddComponent<PrimaryKey>();
            TestHelpers.SetField(key, "table", table);
            TestHelpers.InvokeAwake(key);
            return key;
        }

        [Test]
        public void Take_FirstKey_SetsHeldToThatTable()
        {
            var key = CreateKey(TableKey.Customers);

            _holder.Take(key);

            Assert.AreEqual(TableKey.Customers, _holder.Held);
            Object.DestroyImmediate(key.gameObject);
        }

        [Test]
        public void Take_SecondKey_ReturnsFirstAndHeldReflectsSecond()
        {
            var customers = CreateKey(TableKey.Customers);
            var orders = CreateKey(TableKey.Orders);

            _holder.Take(customers);
            _holder.Take(orders);

            Assert.AreEqual(TableKey.Orders, _holder.Held);
            Assert.IsTrue(customers.GetComponent<SpriteRenderer>().enabled,
                "Swapped-out key should be visible on its pedestal again");

            Object.DestroyImmediate(customers.gameObject);
            Object.DestroyImmediate(orders.gameObject);
        }

        // Consume() is covered in the PlayMode suite instead: it calls Destroy(), which
        // only behaves correctly at runtime.
    }
}
