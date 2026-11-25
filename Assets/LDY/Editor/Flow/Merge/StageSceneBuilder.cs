using System.IO;
using LDY.Flow;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace LDY.FlowEditor {
    internal sealed class StageSceneRequest {
        public string SceneName;
        public string StageId;
        public string Folder = FlowPaths.StageSceneFolder;
        public string PlayerPrefabPath;
        public bool LinkCatalog = true;

        public string ScenePath => $"{Folder}/{SceneName}.unity";
    }

    // 없는 부분만 이름으로 찾아 채운다. 이미 있는 씬을 다시 돌려도 기존 오브젝트는 건드리지 않는다.
    internal static class StageSceneBuilder {
        public const float OrthoSize = 5f;
        private static readonly Vector2 PlayerStart = new Vector2(-6f, -2f);
        private static readonly Vector2 GoalPosition = new Vector2(6f, -2f);
        private static readonly Vector2 BoundsHalf = new Vector2(16f, 9f);

        public static string Build(StageSceneRequest request, FlowLog log) {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return null;
            FlowPaths.EnsureFolder(request.Folder);
            var path = request.ScenePath;
            var existed = File.Exists(path);
            var scene = existed
                ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            log.Info((existed ? "기존 씬 보강 " : "새 씬 ") + path);

            var parts = new SceneParts(scene, log);
            parts.Root(request.StageId);
            var start = parts.Marker("PlayerStart", PlayerStart);
            var prefab = string.IsNullOrEmpty(request.PlayerPrefabPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(request.PlayerPrefabPath);
            var player = parts.Player(prefab, start.position);
            parts.Tilemaps();
            parts.Goal(GoalPosition);
            var bounds = parts.Bounds(BoundsHalf);
            parts.DeathZone(BoundsHalf);
            parts.Camera(player != null ? player.transform : null, bounds);
            parts.Light();
            log.Info("HUD 프리팹 없음, 건너뜀");

            EditorSceneManager.SaveScene(scene, path);
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            BuildSettingsSync.Add(path, log);
            if (request.LinkCatalog) LinkCatalog(request.StageId, asset, log);
            return path;
        }

        private static void LinkCatalog(string id, SceneAsset scene, FlowLog log) {
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog);
            if (catalog == null) {
                log.Warn("카탈로그가 없어 연결하지 않았다. LDY/Flow/Build Flow Assets 먼저 실행");
                return;
            }

            if (StageCatalogBuilder.LinkScene(catalog, id, scene, log)) AssetDatabase.SaveAssets();
        }

        private sealed class SceneParts {
            private readonly Scene _scene;
            private readonly FlowLog _log;

            public SceneParts(Scene scene, FlowLog log) {
                _scene = scene;
                _log = log;
            }

            public void Root(string id) {
                var root = Ensure("StageRoot", out var created);
                var component = root.GetComponent<StageRoot>();
                if (component == null) component = root.AddComponent<StageRoot>();
                if (created || string.IsNullOrEmpty(component.StageId)) {
                    var so = new SerializedObject(component);
                    so.FindProperty("stageId").stringValue = id;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                else if (component.StageId != id) {
                    _log.Warn($"StageRoot id가 이미 {component.StageId}라서 {id}로 바꾸지 않았다");
                }
            }

            public Transform Marker(string name, Vector2 position) {
                var go = Ensure(name, out var created);
                if (created) go.transform.position = position;
                return go.transform;
            }

            public GameObject Player(GameObject prefab, Vector3 position) {
                var existing = FindTagged(PlayerMatch.DefaultTag);
                if (existing != null) {
                    _log.Info("Player 유지 " + existing.name);
                    return existing;
                }

                if (prefab == null) {
                    _log.Warn("Player 프리팹이 없어 배치하지 않았다");
                    return null;
                }

                var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _scene);
                player.transform.position = position;
                _log.Info("Player 배치 " + AssetDatabase.GetAssetPath(prefab));
                return player;
            }

            public void Tilemaps() {
                var grid = Ensure("Grid", out _);
                if (grid.GetComponent<Grid>() == null) grid.AddComponent<Grid>().cellSize = Vector3.one;
                Layer(grid.transform, "Ground", "Ground", 0, false, false);
                Layer(grid.transform, "Platform", "Ground", 1, true, false);
                var hazard = Layer(grid.transform, "Hazard", "Spike", 2, false, true);
                if (hazard.GetComponent<StageDeathZone>() == null) hazard.AddComponent<StageDeathZone>();
            }

            public void Goal(Vector2 position) {
                var goal = Ensure("StageGoal", out var created);
                if (!created) return;
                goal.transform.position = position;
                var box = goal.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                goal.AddComponent<StageGoal>();
                var visual = AssetDatabase.LoadAssetAtPath<GameObject>(FlowPaths.GoalVisual);
                if (visual == null) return;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(visual, _scene);
                instance.transform.SetParent(goal.transform, false);
            }

            public PolygonCollider2D Bounds(Vector2 half) {
                var go = Ensure("Bounds", out var created);
                var shape = go.GetComponent<PolygonCollider2D>();
                if (!created && shape != null) return shape;
                go.layer = 2;
                shape = go.AddComponent<PolygonCollider2D>();
                shape.isTrigger = true;
                shape.excludeLayers = ~0;
                shape.points = new[] {
                    new Vector2(-half.x, -half.y), new Vector2(half.x, -half.y),
                    new Vector2(half.x, half.y), new Vector2(-half.x, half.y)
                };
                return shape;
            }

            public void DeathZone(Vector2 half) {
                var go = Ensure("DeathZone", out var created);
                if (!created) return;
                go.transform.position = new Vector3(0f, -half.y - 1f, 0f);
                var box = go.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size = new Vector2(half.x * 2f + 8f, 2f);
                go.AddComponent<StageDeathZone>();
            }

            public void Camera(Transform target, Collider2D bounds) {
                var main = Ensure("Main Camera", out var created);
                if (created) {
                    main.tag = "MainCamera";
                    main.transform.position = new Vector3(0f, 0f, -10f);
                    var camera = main.AddComponent<Camera>();
                    camera.orthographic = true;
                    camera.orthographicSize = OrthoSize;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color32(0x1E, 0x1E, 0x28, 0xFF);
                    main.AddComponent<AudioListener>();
                    main.AddComponent<CinemachineBrain>();
                }

                var rig = Ensure("StageCamera", out var rigCreated);
                if (!rigCreated) return;
                var cm = rig.AddComponent<CinemachineCamera>();
                var lens = cm.Lens;
                lens.OrthographicSize = OrthoSize;
                cm.Lens = lens;
                cm.Follow = target;
                var follow = rig.AddComponent<CinemachineFollow>();
                follow.FollowOffset = new Vector3(0f, 0f, -10f);
                var confiner = rig.AddComponent<CinemachineConfiner2D>();
                confiner.BoundingShape2D = bounds;
            }

            public void Light() {
                var go = Ensure("Global Light 2D", out var created);
                if (!created) return;
                var light = go.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Global;
            }

            private GameObject Layer(Transform grid, string name, string layerName, int order, bool platform,
                bool trigger) {
                var child = grid.Find(name);
                if (child != null) return child.gameObject;
                var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
                go.transform.SetParent(grid, false);
                var layer = LayerMask.NameToLayer(layerName);
                if (layer >= 0) go.layer = layer;
                go.GetComponent<TilemapRenderer>().sortingOrder = order;
                var collider = go.GetComponent<TilemapCollider2D>();
                collider.isTrigger = trigger;
                if (platform) {
                    collider.usedByEffector = true;
                    go.AddComponent<PlatformEffector2D>();
                }

                return go;
            }

            private GameObject Ensure(string name, out bool created) {
                foreach (var root in _scene.GetRootGameObjects())
                    if (root.name == name) {
                        created = false;
                        return root;
                    }

                var go = new GameObject(name);
                SceneManager.MoveGameObjectToScene(go, _scene);
                created = true;
                _log.Info("만듦 " + name);
                return go;
            }

            private GameObject FindTagged(string tag) {
                foreach (var root in _scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    if (t.CompareTag(tag))
                        return t.gameObject;
                return null;
            }
        }
    }
}
