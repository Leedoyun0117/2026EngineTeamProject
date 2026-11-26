using LDY.PlayerMerge;
using LDY.Spider;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.SpiderEditor {
    internal sealed class SpiderTestSceneBuilder {
        public const string ScenePath = SpiderPaths.Scenes + "/Stage2_SpiderTest.unity";

        private const string GroundLayer = "Ground";
        private const string UiSpritePath = "UI/Skin/UISprite.psd";
        private static readonly Vector2 GroundSize = new Vector2(80f, 2f);
        private static readonly Vector2 ZoneSize = new Vector2(36f, 20f);
        private static readonly Vector2 PlayerSpawn = new Vector2(-10f, 1.5f);
        private static readonly Vector2[] TrapSpawns = { new Vector2(-5f, 0.6f), new Vector2(4f, 0.6f) };

        public void Build(GameObject spiderPrefab, GameObject trapPrefab, float spiderRestHeight) {
            SpiderPaths.EnsureFolder(SpiderPaths.Scenes);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try {
                AddCamera(scene);
                AddGround(scene);
                var spider = Instantiate(spiderPrefab, scene, new Vector2(0f, spiderRestHeight));
                Wire.Ref(spider.GetComponent<SpiderController>(), "combatZone", AddCombatZone(scene));
                foreach (var spawn in TrapSpawns)
                    Instantiate(trapPrefab, scene, spawn);

                var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Final);
                if (player != null)
                    Instantiate(player, scene, PlayerSpawn);
                else
                    Debug.LogWarning("[Stage2] Player 프리팹을 찾지 못해 씬에 놓지 못했다: " + PlayerPrefabPaths.Final);

                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static GameObject Instantiate(GameObject prefab, Scene scene, Vector2 position) {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.position = position;
            return instance;
        }

        private static BoxCollider2D AddCombatZone(Scene scene) {
            var go = new GameObject("CombatZone");
            go.transform.position = new Vector3(0f, 6f, 0f);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = ZoneSize;
            SceneManager.MoveGameObjectToScene(go, scene);
            return box;
        }

        private static void AddCamera(Scene scene) {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.12f, 0.16f);
            go.transform.position = new Vector3(0f, 4f, -10f);
            SceneManager.MoveGameObjectToScene(go, scene);
        }

        private static void AddGround(Scene scene) {
            var go = new GameObject("Ground");
            var layer = LayerMask.NameToLayer(GroundLayer);
            if (layer >= 0)
                go.layer = layer;

            go.transform.position = new Vector3(0f, -GroundSize.y * 0.5f, 0f);
            go.AddComponent<BoxCollider2D>().size = GroundSize;

            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(UiSpritePath);
            if (sprite != null) {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = GroundSize;
                renderer.color = new Color(0.35f, 0.35f, 0.4f);
            }

            SceneManager.MoveGameObjectToScene(go, scene);
        }
    }
}
