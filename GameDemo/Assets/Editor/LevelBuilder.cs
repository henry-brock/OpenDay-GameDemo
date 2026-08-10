using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace OpenDay.Editor
{
    /// <summary>
    /// Builds the four open-day demo levels (one scene per CS module) from scratch,
    /// wiring up player, ground, camera and a themed collectible in each.
    /// Run via: Unity -batchmode -quit -projectPath &lt;path&gt; -executeMethod OpenDay.Editor.LevelBuilder.BuildLevels
    /// </summary>
    public static class LevelBuilder
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string ActionsAssetPath = "Assets/InputSystem_Actions.inputactions";

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

        private struct LevelDef
        {
            public string FileName;
            public string DisplayName;
            public CSModule Module;
            public Color CollectibleColor;
            public Vector3 PlayerStart;
            public Vector3 CollectiblePos;
            public Platform[] Platforms;
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
                    DisplayName = "Level 1 - Object-Oriented Programming",
                    Module = CSModule.ObjectOrientedProgramming,
                    CollectibleColor = new Color(0.95f, 0.55f, 0.1f), // orange
                    PlayerStart = new Vector3(-6f, 1f, 0f),
                    CollectiblePos = new Vector3(6f, 3.2f, 0f),
                    Platforms = new[]
                    {
                        new Platform(0f, -0.5f, 20f, 1f),      // main floor
                        new Platform(6f, 2f, 3f, 0.5f),        // platform under collectible
                    },
                },
                new LevelDef
                {
                    FileName = "Level2_SoftwareEngineering",
                    DisplayName = "Level 2 - Software Engineering",
                    Module = CSModule.SoftwareEngineering,
                    CollectibleColor = new Color(0.15f, 0.7f, 0.65f), // teal
                    PlayerStart = new Vector3(-7f, 1f, 0f),
                    CollectiblePos = new Vector3(7f, 4.7f, 0f),
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
                    Module = CSModule.Databases,
                    CollectibleColor = new Color(0.95f, 0.85f, 0.1f), // yellow, matches the "key" theme
                    PlayerStart = new Vector3(-6f, 1f, 0f),
                    CollectiblePos = new Vector3(6f, 1f, 0f),
                    Platforms = new[]
                    {
                        new Platform(0f, -0.5f, 20f, 1f),
                    },
                },
                new LevelDef
                {
                    FileName = "Level4_TeamProject",
                    DisplayName = "Level 4 - Team Project",
                    Module = CSModule.TeamProject,
                    CollectibleColor = new Color(0.85f, 0.15f, 0.2f), // red - hardest level
                    PlayerStart = new Vector3(-8f, 1f, 0f),
                    CollectiblePos = new Vector3(8f, 6.2f, 0f),
                    Platforms = new[]
                    {
                        new Platform(-6f, -0.5f, 5f, 1f),
                        new Platform(-2f, 1f, 3f, 0.5f),
                        new Platform(2f, 2.5f, 3f, 0.5f),
                        new Platform(5f, 4f, 3f, 0.5f),
                        new Platform(8f, 5.5f, 3f, 0.5f),
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

            EditorBuildSettings.scenes = scenePaths
                .Select(p => new EditorBuildSettingsScene(p, true))
                .ToArray();

            AssetDatabase.SaveAssets();
            Debug.Log($"Built {scenePaths.Count} levels and added them to Build Settings.");
        }

        private static string BuildLevel(LevelDef level, InputActionAsset actions, int groundLayer)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateGlobalLight();

            var groundParent = new GameObject("Ground").transform;
            foreach (var platform in level.Platforms)
            {
                CreateGroundPlatform(platform, groundLayer, groundParent);
            }

            CreatePlayer(level.PlayerStart, actions, groundLayer);
            CreateCollectible(level.CollectiblePos, level.Module, level.CollectibleColor);

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
    }
}
