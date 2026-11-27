using System;
using LDY.FlowEditor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.StageImportEditor {
    // LDY 로프 프리팹(Assets/LDY/Prefab/Rope/Rope.prefab)을 RopeTest 씬의 "RopeTest" 오브젝트와 같은 구성으로 만든다.
    // 씬은 읽지도 고치지도 않고, 구성만 코드로 옮겼다. 로프 스크립트는 Assembly-CSharp라 이름으로 찾는다.
    internal static class RopePrefab {
        public const string SettingsPath = "Assets/LDY/Data/RopeSettings.asset";
        private const string Folder = "Assets/LDY/Prefab/Rope";
        private const string ObjectLayer = "Object";

        private const string HangType = "LDY.Script.RopeHangController";
        private const string LineType = "LDY.Script.RopeLineRenderer";
        private const string InteractType = "LDY.Script.RopeInteractable";
        private const string FollowerType = "LDY.Script.RopeColliderFollower";

        // 없을 때만 만든다. 있으면 사람이 고쳤을 수 있어 그대로 둔다.
        public static void Ensure(FlowLog log) {
            var path = CustomBlockSeed.RopePrefab;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var hang = FindType(HangType);
            var line = FindType(LineType);
            var interact = FindType(InteractType);
            var follower = FindType(FollowerType);
            if (hang == null || line == null || interact == null || follower == null) {
                log.Warn("로프 스크립트를 찾지 못해 Rope 프리팹을 만들지 않음 (밧줄은 분홍 블록)");
                return;
            }

            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(SettingsPath);
            FlowPaths.EnsureFolder(Folder);

            var root = EditorUtility.CreateGameObjectWithHideFlags("Rope", HideFlags.HideAndDontSave);
            try {
                var layer = LayerMask.NameToLayer(ObjectLayer);
                if (layer >= 0) root.layer = layer;
                var anchor = EditorUtility.CreateGameObjectWithHideFlags("Anchor", HideFlags.HideAndDontSave);
                anchor.transform.SetParent(root.transform, false);

                var renderer = root.AddComponent<LineRenderer>();
                renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
                renderer.sortingOrder = 5;
                renderer.widthMultiplier = 0.12f;
                renderer.positionCount = 2;
                renderer.SetPositions(new[] { Vector3.zero, Vector3.down });
                renderer.useWorldSpace = true;

                var edge = root.AddComponent<EdgeCollider2D>();
                edge.isTrigger = true;
                edge.points = new[] { Vector2.zero, Vector2.down };

                var controller = root.AddComponent(hang);
                root.AddComponent(line);
                root.AddComponent(interact);
                root.AddComponent(follower);

                var so = new SerializedObject(controller);
                so.FindProperty("anchor").objectReferenceValue = anchor.transform;
                so.FindProperty("settings").objectReferenceValue = settings;
                so.ApplyModifiedPropertiesWithoutUndo();

                root.hideFlags = HideFlags.None;
                anchor.hideFlags = HideFlags.None;
                PrefabUtility.SaveAsPrefabAsset(root, path);
                log.Info($"Rope 프리팹 만듦 {path} (settings {(settings != null ? SettingsPath : "없음")})");
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        private static Type FindType(string fullName) {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }
    }

    // 놓은 로프 인스턴스의 길이를 맞춘다. RopeSettings는 공유 에셋이라 길이별 사본을 만들어 인스턴스에만 연결한다.
    internal static class RopeLength {
        private const string Folder = "Assets/LDY/Data/StageImport/Rope";

        public static void Apply(GameObject go, int cells, FlowLog log) {
            var length = Mathf.Max(1, cells);
            Component controller = null;
            foreach (var component in go.GetComponents<MonoBehaviour>())
                if (component != null && component.GetType().Name == "RopeHangController")
                    controller = component;
            if (controller == null) return;

            var so = new SerializedObject(controller);
            var settings = so.FindProperty("settings");
            var source = settings.objectReferenceValue as ScriptableObject;
            if (source == null) {
                log.Warn($"{go.name}: RopeSettings가 비어 길이 {length}칸을 못 넣음");
                return;
            }

            var variant = Variant(source, length);
            if (variant == null) return;
            settings.objectReferenceValue = variant;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ScriptableObject Variant(ScriptableObject source, int length) {
            var path = $"{Folder}/RopeSettings_L{length}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (existing != null) return existing;

            FlowPaths.EnsureFolder(Folder);
            if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source), path)) return null;
            var copy = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            var so = new SerializedObject(copy);
            var total = so.FindProperty("totalLength");
            if (total != null) total.floatValue = length;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return copy;
        }
    }
}
