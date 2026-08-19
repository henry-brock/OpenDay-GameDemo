using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OpenDay.Editor
{
    /// <summary>
    /// Builds the three open-day demo levels from scratch, wiring up player,
    /// ground, camera and one themed collectible per paired CS module in each
    /// (two collectibles per level, except Databases which has one). A separate
    /// entry point builds the Title Screen (the demo's fourth, non-gameplay scene).
    /// Run via: Unity -batchmode -quit -projectPath &lt;path&gt; -executeMethod OpenDay.Editor.LevelBuilder.BuildLevels
    /// </summary>
    public static class LevelBuilder
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string ActionsAssetPath = "Assets/InputSystem_Actions.inputactions";
        private const string TitleScreenPath = "Assets/Scenes/TitleScreen.unity";

        private struct Platform
        {
            public Vector2 Position;
            public Vector2 Size;
            public Platform(float x, float y, float w, float h)
            {
                Position = new Vector2(x, y);
                Size = new Vector2(w, h);
            }
        }

        private struct ModuleCollectible
        {
            public CSModule Module;
            public Color Color;
            public Vector3 Position;
            public ModuleCollectible(CSModule module, Color color, Vector3 position)
            {
                Module = module;
                Color = color;
                Position = position;
            }
        }

        private struct LevelDef
        {
            public string FileName;
            public string DisplayName;
            public Vector3 PlayerStart;
            public ModuleCollectible[] Collectibles;
            public Platform[] Platforms;

            /// <summary>Where to place an OOP/DS&amp;A crate-stacking blueprint switch, if this level has one.</summary>
            public Vector3? BlueprintPosition;
        }

        [MenuItem("OpenDay/Build Levels")]
        public static void BuildLevels()
        {
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsAssetPath);
            if (actions == null)
            {
                Debug.LogError($"Could not load InputActionAsset at {ActionsAssetPath}");
                return;
            }

            var levels = new[]
            {
                new LevelDef
                {
                    FileName = "Level1_ObjectOrientedProgramming",
                    DisplayName = "Level 1 - Object-Oriented Programming & Data Structures and Algorithms",
                    PlayerStart = new Vector3(-6f, 1f, 0f),
                    Collectibles = new[]
                    {
                        // orange - OOP
                        new ModuleCollectible(CSModule.ObjectOrientedProgramming, new Color(0.95f, 0.55f, 0.1f), new Vector3(6f, 3.2f, 0f)),
                        // purple - Data Structures & Algorithms, up on the high ledge the crate stack reaches
                        new ModuleCollectible(CSModule.DataStructuresAndAlgorithms, new Color(0.55f, 0.35f, 0.85f), new Vector3(-3f, 7f, 0f)),
                    },
                    Platforms = new[]
                    {
                        new Platform(0f, -0.5f, 20f, 1f),      // main floor
                        new Platform(6f, 2f, 3f, 0.5f),        // platform under the OOP collectible
                        new Platform(-3f, 6.25f, 2.5f, 0.5f),  // high ledge under the DS&A collectible
                    },
                    // OOP "instantiate an object" blueprint, spawns stackable crates
                    // — a couple stacked crates give the extra height to reach the ledge above.
                    BlueprintPosition = new Vector3(-3f, 0f, 0f),
                },
                new LevelDef
                {
                    FileName = "Level2_SoftwareEngineering",
                    DisplayName = "Level 2 - Software Engineering & Software Project Management",
                    PlayerStart = new Vector3(-7f, 1f, 0f),
                    Collectibles = new[]
                    {
                        // teal - Software Engineering
                        new ModuleCollectible(CSModule.SoftwareEngineering, new Color(0.15f, 0.7f, 0.65f), new Vector3(7f, 4.7f, 0f)),
                        // green - Software Project Management
                        new ModuleCollectible(CSModule.SoftwareProjectManagement, new Color(0.25f, 0.75f, 0.35f), new Vector3(2f, 2.2f, 0f)),
                    },
                    Platforms = new[]
                    {
                        new Platform(-4f, -0.5f, 8f, 1f),
                        new Platform(2f, 1.5f, 4f, 0.5f),
                        new Platform(7f, 3.5f, 3f, 0.5f),
                    },
                },
                new LevelDef
                {
                    FileName = "Level3_Databases",
                    DisplayName = "Level 3 - Databases",
                    PlayerStart = new Vector3(-6f, 1f, 0f),
                    Collectibles = new[]
                    {
                        // yellow, matches the "key" theme
                        new ModuleCollectible(CSModule.Databases, new Color(0.95f, 0.85f, 0.1f), new Vector3(6f, 1f, 0f)),
                    },
                    Platforms = new[]
                    {
                        new Platform(0f, -0.5f, 20f, 1f),
                    },
                },
            };

            if (!AssetDatabase.IsValidFolder(ScenesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            var groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0)
            {
                Debug.LogError("Ground layer is not defined in TagManager.asset; aborting.");
                return;
            }

            var scenePaths = new List<string>();
            foreach (var level in levels)
            {
                var path = BuildLevel(level, actions, groundLayer);
                scenePaths.Add(path);
            }

            EditorBuildSettings.scenes = new[] { TitleScreenPath }.Concat(scenePaths)
                .Select(p => new EditorBuildSettingsScene(p, true))
                .ToArray();

            AssetDatabase.SaveAssets();
            Debug.Log($"Built {scenePaths.Count} levels and added them to Build Settings.");
        }

        [MenuItem("OpenDay/Build Title Screen")]
        public static void BuildTitleScreen()
        {
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsAssetPath);
            if (actions == null)
            {
                Debug.LogError($"Could not load InputActionAsset at {ActionsAssetPath}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(ScenesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem(actions);

            var canvas = CreateCanvas("Canvas");
            CreateTitleText(canvas.transform, "Open Day Demo", new Vector2(0f, 260f), 64);

            var startButton = CreateButton(canvas.transform, "Start New Game", new Vector2(0f, 60f));
            var resumeButton = CreateButton(canvas.transform, "Resume", new Vector2(0f, -40f));
            var quitButton = CreateButton(canvas.transform, "Quit", new Vector2(0f, -140f));

            var controllerGo = new GameObject("TitleScreenController");
            var controller = controllerGo.AddComponent<TitleScreenController>();
            var so = new SerializedObject(controller);
            so.FindProperty("startButton").objectReferenceValue = startButton;
            so.FindProperty("resumeButton").objectReferenceValue = resumeButton;
            so.FindProperty("quitButton").objectReferenceValue = quitButton;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, TitleScreenPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Saved Title Screen to {TitleScreenPath}");
        }

        /// <summary>An EventSystem is what lets Unity UI buttons respond to a
        /// gamepad, keyboard or mouse at all — every scene with UI needs one.</summary>
        private static void CreateEventSystem(InputActionAsset actions)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

            var uiMap = actions.FindActionMap("UI");
            var uiModule = go.AddComponent<InputSystemUIInputModule>();
            uiModule.actionsAsset = actions;
            uiModule.move = InputActionReference.Create(uiMap.FindAction("Navigate"));
            uiModule.submit = InputActionReference.Create(uiMap.FindAction("Submit"));
            uiModule.cancel = InputActionReference.Create(uiMap.FindAction("Cancel"));
            uiModule.point = InputActionReference.Create(uiMap.FindAction("Point"));
            uiModule.leftClick = InputActionReference.Create(uiMap.FindAction("Click"));
        }

        private static void CreatePauseMenu()
        {
            var canvas = CreateCanvas("PauseMenuCanvas");

            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(canvas.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            var panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.75f);

            CreateTitleText(panelGo.transform, "Paused", new Vector2(0f, 220f), 56);

            var resumeButton = CreateButton(panelGo.transform, "Resume", new Vector2(0f, 40f));
            var saveButton = CreateButton(panelGo.transform, "Save", new Vector2(0f, -60f));
            var returnButton = CreateButton(panelGo.transform, "Return to Title Screen", new Vector2(0f, -160f));

            panelGo.SetActive(false);

            var pauseMenuGo = new GameObject("PauseMenu");
            var pauseMenu = pauseMenuGo.AddComponent<PauseMenu>();
            var so = new SerializedObject(pauseMenu);
            so.FindProperty("menuRoot").objectReferenceValue = panelGo;
            so.FindProperty("resumeButton").objectReferenceValue = resumeButton;
            so.FindProperty("saveButton").objectReferenceValue = saveButton;
            so.FindProperty("returnToTitleButton").objectReferenceValue = returnButton;
            so.ApplyModifiedProperties();
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateTitleText(Transform parent, string text, Vector2 anchoredPosition, int fontSize)
        {
            var go = new GameObject(text);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1200f, 150f);
            rect.anchoredPosition = anchoredPosition;

            var label = go.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(440f, 80f);
            rect.anchoredPosition = anchoredPosition;

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.15f);

            var button = go.AddComponent<Button>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            return button;
        }

        private static string BuildLevel(LevelDef level, InputActionAsset actions, int groundLayer)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateGlobalLight();
            CreateEventSystem(actions);

            var groundParent = new GameObject("Ground").transform;
            foreach (var platform in level.Platforms)
            {
                CreateGroundPlatform(platform, groundLayer, groundParent);
            }

            CreatePlayer(level.PlayerStart, actions, groundLayer);
            foreach (var collectible in level.Collectibles)
            {
                CreateCollectible(collectible.Position, collectible.Module, collectible.Color);
            }

            if (level.BlueprintPosition.HasValue)
            {
                CreateBlueprint(level.BlueprintPosition.Value, groundLayer);
            }

            CreatePauseMenu();

            var path = $"{ScenesFolder}/{level.FileName}.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"Saved {level.DisplayName} to {path}");
            return path;
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.transform.position = new Vector3(0f, 2f, -10f);
            cameraGo.AddComponent<UniversalAdditionalCameraData>();
            cameraGo.AddComponent<AudioListener>();
        }

        private static void CreateGlobalLight()
        {
            var lightGo = new GameObject("Global Light 2D");
            var light2D = lightGo.AddComponent<Light2D>();
            light2D.lightType = Light2D.LightType.Global;
        }

        private const string SquareSpritePath = "Assets/Sprites/Square.png";
        private static Sprite _squareSprite;

        // GetBuiltinExtraResource<Sprite>("Sprites/Square") needs the Editor's GUI resources,
        // which aren't loaded under -nographics batch mode, so a real sprite asset is generated instead.
        private static Sprite SquareSprite
        {
            get
            {
                if (_squareSprite != null)
                {
                    return _squareSprite;
                }

                if (AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath) == null)
                {
                    var folder = System.IO.Path.GetDirectoryName(SquareSpritePath);
                    if (!AssetDatabase.IsValidFolder(folder))
                    {
                        AssetDatabase.CreateFolder("Assets", "Sprites");
                    }

                    var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                    var pixels = new Color32[8 * 8];
                    for (var i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = Color.white;
                    }
                    texture.SetPixels32(pixels);
                    texture.Apply();
                    System.IO.File.WriteAllBytes(SquareSpritePath, texture.EncodeToPNG());
                    Object.DestroyImmediate(texture);

                    AssetDatabase.ImportAsset(SquareSpritePath, ImportAssetOptions.ForceUpdate);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(SquareSpritePath);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.filterMode = FilterMode.Point;
                    importer.spritePixelsPerUnit = 8;
                    importer.SaveAndReimport();
                }

                _squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
                return _squareSprite;
            }
        }

        private static void CreateGroundPlatform(Platform platform, int groundLayer, Transform parent)
        {
            var go = new GameObject("Platform");
            go.transform.SetParent(parent);
            go.transform.position = platform.Position;
            go.layer = groundLayer;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = new Color(0.35f, 0.35f, 0.4f);
            go.transform.localScale = new Vector3(platform.Size.x, platform.Size.y, 1f);

            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one; // scaled by transform
        }

        private static void CreatePlayer(Vector3 startPos, InputActionAsset actions, int groundLayer)
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.transform.position = startPos;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = new Color(0.2f, 0.55f, 0.95f);
            go.transform.localScale = new Vector3(0.9f, 1.6f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = go.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.9f, 1.6f);

            var groundCheckGo = new GameObject("GroundCheck");
            groundCheckGo.transform.SetParent(go.transform);
            groundCheckGo.transform.localPosition = new Vector3(0f, -0.8f, 0f);

            var controller = go.AddComponent<PlayerController>();
            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("groundCheck").objectReferenceValue = groundCheckGo.transform;
            controllerSo.FindProperty("groundLayer").intValue = 1 << groundLayer;
            controllerSo.ApplyModifiedProperties();

            go.AddComponent<PlayerInventory>();
            go.AddComponent<PlayerInteractor>();

            var playerInput = go.AddComponent<PlayerInput>();
            playerInput.actions = actions;
            playerInput.defaultActionMap = "Player";
            playerInput.notificationBehavior = PlayerNotifications.SendMessages;
        }

        private static void CreateCollectible(Vector3 position, CSModule module, Color color)
        {
            var go = new GameObject($"Collectible_{module}");
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = color;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            var collectible = go.AddComponent<Collectible>();
            var so = new SerializedObject(collectible);
            so.FindProperty("module").enumValueIndex = (int)module;
            so.ApplyModifiedProperties();
        }

        private static void CreateBlueprint(Vector3 position, int groundLayer)
        {
            var go = new GameObject("Blueprint");
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = new Color(0.9f, 0.9f, 0.95f); // pale "blueprint" white

            var blueprint = go.AddComponent<ObjectBlueprint>();
            var so = new SerializedObject(blueprint);
            so.FindProperty("crateSprite").objectReferenceValue = SquareSprite;
            so.FindProperty("groundLayer").intValue = groundLayer;
            so.ApplyModifiedProperties();
        }
    }
}
