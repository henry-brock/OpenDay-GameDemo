using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace OpenDay.Tests.PlayMode
{
    /// <summary>
    /// Reflection helpers, duplicated from the EditMode suite because assembly
    /// definitions keep the two test assemblies separate.
    /// </summary>
    internal static class TestHelpers
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Static;

        internal static void SetField(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, Instance);
            Assert.NotNull(info, $"Field '{field}' not found on {target.GetType().Name}");
            info.SetValue(target, value);
        }

        internal static void SetStaticField<T>(string field, object value)
        {
            var info = typeof(T).GetField(field, Static);
            Assert.NotNull(info, $"Static field '{field}' not found on {typeof(T).Name}");
            info.SetValue(null, value);
        }

        internal static Sprite BlankSprite()
        {
            var texture = new Texture2D(4, 4);
            return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        }

        internal static int CrateCount()
        {
            return Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Count(go => go.name.StartsWith("Crate_"));
        }
    }

    /// <summary>
    /// KeyHolder.Consume destroys the carried key with Destroy(), which only behaves
    /// correctly at runtime — this is why the test lives here rather than in EditMode.
    /// </summary>
    public class KeyHolderConsumeTests
    {
        [UnityTest]
        public IEnumerator Consume_DestroysCarriedKeyAndClearsHeld()
        {
            var playerObject = new GameObject("TestPlayer");
            var holder = playerObject.AddComponent<KeyHolder>();

            var keyObject = new GameObject("Key_Customers");
            keyObject.AddComponent<SpriteRenderer>();
            var key = keyObject.AddComponent<PrimaryKey>();
            TestHelpers.SetField(key, "table", TableKey.Customers);

            holder.Take(key);
            Assert.AreEqual(TableKey.Customers, holder.Held);

            holder.Consume();
            yield return null; // Destroy takes effect at end of frame

            Assert.AreEqual(TableKey.None, holder.Held);
            Assert.IsTrue(key == null, "Consumed key should have been destroyed");

            Object.Destroy(playerObject);
        }
    }

    public class ForeignKeyDoorTests
    {
        private GameObject _playerObject;
        private GameObject _barrier;
        private GameObject _reader;
        private KeyHolder _holder;
        private ForeignKeyDoor _door;

        [SetUp]
        public void Setup()
        {
            _playerObject = new GameObject("TestPlayer");
            _holder = _playerObject.AddComponent<KeyHolder>();

            _barrier = new GameObject("Barrier");
            _barrier.AddComponent<SpriteRenderer>();
            _barrier.AddComponent<BoxCollider2D>();

            _reader = new GameObject("Reader");
            _door = _reader.AddComponent<ForeignKeyDoor>();
            TestHelpers.SetField(_door, "required", TableKey.Orders);
            TestHelpers.SetField(_door, "barrier", _barrier);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_reader);
            Object.DestroyImmediate(_barrier);
            Object.DestroyImmediate(_playerObject);
        }

        private PrimaryKey GiveKey(TableKey table)
        {
            var keyObject = new GameObject($"Key_{table}");
            keyObject.AddComponent<SpriteRenderer>();
            var key = keyObject.AddComponent<PrimaryKey>();
            TestHelpers.SetField(key, "table", table);
            _holder.Take(key);
            return key;
        }

        [Test]
        public void Interact_NoKeyHeld_BarrierStaysSolid()
        {
            _door.Interact();

            Assert.IsTrue(_barrier.GetComponent<BoxCollider2D>().enabled);
        }

        [Test]
        public void Interact_WrongKeyHeld_BarrierStaysSolidAndKeyNotConsumed()
        {
            GiveKey(TableKey.Customers);

            _door.Interact();

            Assert.IsTrue(_barrier.GetComponent<BoxCollider2D>().enabled,
                "A non-matching foreign key must not open the barrier");
            Assert.AreEqual(TableKey.Customers, _holder.Held,
                "Wrong key should not be spent on a door it does not match");
        }

        [Test]
        public void Interact_MatchingKeyHeld_OpensBarrierAndConsumesKey()
        {
            GiveKey(TableKey.Orders);

            _door.Interact();

            Assert.IsFalse(_barrier.GetComponent<BoxCollider2D>().enabled,
                "Matching key should disable the barrier collider");
            Assert.AreEqual(TableKey.None, _holder.Held, "Matching key should be consumed");
        }
    }

    public class ObjectBlueprintTests
    {
        private GameObject _blueprintObject;
        private ObjectBlueprint _blueprint;

        [SetUp]
        public void Setup()
        {
            _blueprintObject = new GameObject("Blueprint");
            _blueprint = _blueprintObject.AddComponent<ObjectBlueprint>();
            TestHelpers.SetField(_blueprint, "crateSprite", TestHelpers.BlankSprite());
            TestHelpers.SetField(_blueprint, "maxCrates", 3);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_blueprintObject);
            foreach (var crate in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                         .Where(go => go.name.StartsWith("Crate_")))
            {
                Object.DestroyImmediate(crate);
            }
        }

        [Test]
        public void Interact_SpawnsOneCratePerCall()
        {
            Assert.AreEqual(0, TestHelpers.CrateCount());

            _blueprint.Interact();
            Assert.AreEqual(1, TestHelpers.CrateCount());

            _blueprint.Interact();
            Assert.AreEqual(2, TestHelpers.CrateCount());
        }

        [Test]
        public void Interact_BeyondMaxCrates_StopsSpawning()
        {
            for (var i = 0; i < 6; i++)
            {
                _blueprint.Interact();
            }

            Assert.AreEqual(3, TestHelpers.CrateCount(),
                "Blueprint must not spawn more than maxCrates");
        }
    }

    /// <summary>
    /// Level 2's pipeline end to end: three stages that must run in order, where running
    /// one out of turn resets every stage. _nextExpectedOrder is static and shared across
    /// instances, so it is reset explicitly between tests.
    /// </summary>
    public class PipelineIntegrationTests
    {
        private GameObject _gateObject;
        private PipelineGate _gate;
        private GameObject[] _stageObjects;
        private PipelineStage[] _stages;

        [SetUp]
        public void Setup()
        {
            TestHelpers.SetStaticField<PipelineStage>("_nextExpectedOrder", 0);

            _gateObject = new GameObject("Gate");
            _gateObject.AddComponent<SpriteRenderer>();
            _gateObject.AddComponent<BoxCollider2D>();
            _gate = _gateObject.AddComponent<PipelineGate>();
            TestHelpers.SetField(_gate, "requiredStages", 3);

            _stageObjects = new GameObject[3];
            _stages = new PipelineStage[3];
            for (var i = 0; i < 3; i++)
            {
                _stageObjects[i] = new GameObject($"Stage_{i}");
                _stageObjects[i].AddComponent<SpriteRenderer>();
                _stages[i] = _stageObjects[i].AddComponent<PipelineStage>();
                TestHelpers.SetField(_stages[i], "order", i);
                TestHelpers.SetField(_stages[i], "gate", _gate);
            }
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var stageObject in _stageObjects)
            {
                Object.DestroyImmediate(stageObject);
            }

            Object.DestroyImmediate(_gateObject);
            TestHelpers.SetStaticField<PipelineStage>("_nextExpectedOrder", 0);
        }

        [Test]
        public void ActivateStagesInOrder_OpensGate()
        {
            _stages[0].Interact();
            Assert.IsFalse(_gate.IsOpen);

            _stages[1].Interact();
            Assert.IsFalse(_gate.IsOpen);

            _stages[2].Interact();
            Assert.IsTrue(_gate.IsOpen, "Gate should open once all three stages run in order");
        }

        [Test]
        public void ActivateStageOutOfOrder_ResetsProgress()
        {
            _stages[0].Interact();
            _stages[2].Interact(); // out of turn — should wipe the progress so far

            Assert.IsFalse(_gate.IsOpen);

            // Stage 0 must be the valid next step again, proving the reset really happened.
            _stages[0].Interact();
            _stages[1].Interact();
            _stages[2].Interact();

            Assert.IsTrue(_gate.IsOpen, "Pipeline should be completable again after a reset");
        }
    }

    public class CollectibleTests
    {
        private GameObject _collectibleObject;
        private GameObject _playerObject;
        private PlayerInventory _inventory;

        [SetUp]
        public void Setup()
        {
            _collectibleObject = new GameObject("Collectible");
            _collectibleObject.transform.position = Vector3.zero;
            var trigger = _collectibleObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            var collectible = _collectibleObject.AddComponent<Collectible>();
            TestHelpers.SetField(collectible, "module", CSModule.Databases);

            // Starts clear of the trigger so the move below produces a genuine
            // enter event; a body that is already overlapping and never moves
            // falls asleep and raises no callback at all.
            _playerObject = new GameObject("TestPlayer");
            _playerObject.transform.position = new Vector3(5f, 0f, 0f);
            _playerObject.AddComponent<BoxCollider2D>();
            var body = _playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
            _inventory = _playerObject.AddComponent<PlayerInventory>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_collectibleObject != null)
            {
                Object.DestroyImmediate(_collectibleObject);
            }

            Object.DestroyImmediate(_playerObject);
        }

        [UnityTest]
        public IEnumerator PlayerTouchingCollectible_CollectsModule()
        {
            _playerObject.tag = "Player";

            yield return new WaitForFixedUpdate();
            _playerObject.transform.position = Vector3.zero;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(_inventory.Has(CSModule.Databases),
                "Overlapping the trigger should collect the module");
        }

        [UnityTest]
        public IEnumerator UntaggedObjectTouchingCollectible_CollectsNothing()
        {
            _playerObject.tag = "Untagged";

            yield return new WaitForFixedUpdate();
            _playerObject.transform.position = Vector3.zero;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(_inventory.Has(CSModule.Databases),
                "Only objects with the player tag should trigger collection");
        }
    }
}
