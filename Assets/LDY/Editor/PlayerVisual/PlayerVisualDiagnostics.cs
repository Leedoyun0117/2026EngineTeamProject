using System.Linq;
using UnityEditor;
using UnityEngine;
using static LDY.Script.PlayerVisualContext;

namespace LDY.Script {
    internal static class PlayerVisualDiagnostics {
        public static void Run() {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) {
                Log($"프리팹 없음: {PrefabPath}");
                return;
            }

            DescribeVisuals("prefab", prefab, false);
            using (var session = SceneSession.Open(ScenePath)) {
                var instance = FindInstance(session.Scene, prefab);
                if (instance == null)
                    Log("SampleScene에서 Player 인스턴스를 찾지 못함");
                else {
                    DescribeVisuals("scene", instance, true);
                    DescribeOverrides(instance);
                    DescribeCamera(instance);
                }
            }

            DescribeAsepriteFiles();
            DescribeSets();
        }

        private static void DescribeVisuals(string label, GameObject root, bool scene) {
            foreach (var name in new[] { OldName, PinkName }) {
                var child = root.transform.Find(name);
                if (child == null) {
                    Log($"{label} {name}: 없음");
                    continue;
                }

                var sr = child.GetComponentInChildren<SpriteRenderer>(true);
                var hierarchy = scene ? $" inHierarchy={child.gameObject.activeInHierarchy}" : "";
                var sprites = " SpriteRenderer 없음";
                if (sr != null) {
                    var sprite = sr.sprite != null ? sr.sprite.name : "null";
                    sprites = $" sr.enabled={sr.enabled} sprite={sprite} sortingLayer='{sr.sortingLayerName}' order={sr.sortingOrder}";
                }

                Log($"{label} {name}: activeSelf={child.gameObject.activeSelf}{hierarchy}{sprites} " +
                    $"localPos={child.localPosition} localScale={child.localScale} worldZ={child.position.z}");

                var renderer = child.GetComponent<PlayerRenderer>();
                if (renderer != null)
                    DescribeRenderer(label, name, renderer);
            }
        }

        private static void DescribeRenderer(string label, string name, PlayerRenderer renderer) {
            var set = new SerializedObject(renderer).FindProperty("animSet").objectReferenceValue as PlayerAnimSet;
            Log($"{label} {name} PlayerRenderer: enabled={renderer.enabled} animSet={(set != null ? set.name : "null")}");
            if (set == null)
                return;

            var tags = set.Clips.Select(e => {
                var frames = e.clip != null ? e.clip.frames : null;
                var total = frames != null ? frames.Length : 0;
                var nulls = frames != null ? frames.Count(f => f == null) : 0;
                return $"{e.id}={total}(null {nulls})";
            });
            Log($"{label} {name} 세트 태그별 스프라이트: {string.Join(", ", tags)}");
        }

        private static void DescribeOverrides(GameObject instance) {
            var mods = PrefabUtility.GetPropertyModifications(instance);
            var count = 0;
            if (mods != null)
                foreach (var m in mods) {
                    count++;
                    var target = m.target != null ? $"{m.target.GetType().Name}({m.target.name})" : "null";
                    var reference = m.objectReference != null ? " ref=" + m.objectReference.name : "";
                    Log($"override {target} {m.propertyPath} = {m.value}{reference}");
                }

            Log($"override 속성 {count}개");
            foreach (var a in PrefabUtility.GetAddedComponents(instance))
                Log($"override 추가된 컴포넌트: {a.instanceComponent.GetType().Name}");
            foreach (var a in PrefabUtility.GetAddedGameObjects(instance))
                Log($"override 추가된 오브젝트: {a.instanceGameObject.name}");
            foreach (var r in PrefabUtility.GetRemovedComponents(instance))
                Log($"override 제거된 컴포넌트: {r.assetComponent.GetType().Name}");
            foreach (var r in PrefabUtility.GetRemovedGameObjects(instance))
                Log($"override 제거된 오브젝트: {r.assetGameObject.name}");
        }

        private static void DescribeAsepriteFiles() {
            var files = AsepriteCatalog.Scan();
            if (files.Count == 0)
                Log("player_anim_pink*.aseprite 파일 없음");
            foreach (var f in files)
                Log($"aseprite {f.Path} importer={f.ImporterType} 스프라이트={f.SpriteCount} 클립={f.ClipNames.Length}");
        }

        private static void DescribeSets() {
            foreach (var guid in AssetDatabase.FindAssets("t:PlayerAnimSet")) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var set = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(path);
                var sources = set.Clips.Where(e => e.clip != null && e.clip.frames != null)
                    .SelectMany(e => e.clip.frames).Where(f => f != null)
                    .Select(f => AssetDatabase.GetAssetPath(f)).Distinct().ToArray();
                Log($"세트 {path} → {(sources.Length > 0 ? string.Join(", ", sources) : "참조하는 스프라이트 없음")}");
            }
        }

        private static void DescribeCamera(GameObject player) {
            var camera = Camera.main != null ? Camera.main : Camera.allCameras.FirstOrDefault();
            if (camera == null) {
                Log("카메라 없음");
                return;
            }

            var inMask = (camera.cullingMask & (1 << player.layer)) != 0;
            var depth = player.transform.position.z - camera.transform.position.z;
            var inRange = depth >= camera.nearClipPlane && depth <= camera.farClipPlane;
            Log($"camera {camera.name}: pos={camera.transform.position} ortho={camera.orthographic} size={camera.orthographicSize} " +
                $"sortMode={camera.transparencySortMode} cullingMask에 Player 레이어({LayerMask.LayerToName(player.layer)}) 포함={inMask} " +
                $"깊이 {depth:0.##} (near {camera.nearClipPlane}, far {camera.farClipPlane}) 범위 안={inRange}");
        }
    }
}
