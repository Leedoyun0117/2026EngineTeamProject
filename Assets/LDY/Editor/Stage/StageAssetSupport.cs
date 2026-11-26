using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.StageEditor {
    internal static class StagePaths {
        public const string Prefabs = "Assets/LDY/Prefabs/Stage1";
        public const string Tiles = "Assets/LDY/Tiles/Stage1";
        public const string Data = "Assets/LDY/Data/Stage";
        public const string Scenes = "Assets/LDY/Scenes";
        public const string PickupCatalog = Data + "/Stage1PickupCatalog.asset";

        public static string Prefab(string name) => Prefabs + "/" + name + ".prefab";
        public static string Tile(string name) => Tiles + "/" + name + ".asset";

        public static void EnsureFolder(string path) {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }

    internal sealed class SetupReport {
        private readonly List<string> _made = new List<string>();
        private readonly List<string> _updated = new List<string>();
        private readonly List<string> _skipped = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        public void Made(string item) => _made.Add(item);
        public void Updated(string item) => _updated.Add(item);
        public void Skipped(string item) => _skipped.Add(item);
        public void Warn(string item) => _warnings.Add(item);

        public void Record(bool created, string item) {
            if (created) Made(item);
            else Updated(item);
        }

        public void Print(string title) {
            Debug.Log(title + "\n" + Section("만듦", _made) + Section("갱신", _updated) + Section("건너뜀", _skipped));
            foreach (var warning in _warnings) Debug.LogWarning(title + ": " + warning);
        }

        private static string Section(string label, List<string> items) {
            return items.Count == 0 ? "" : $"[{label} {items.Count}]\n  " + string.Join("\n  ", items) + "\n";
        }
    }

    internal static class AssetUpsert {
        public static T LoadOrCreate<T>(string path, out bool created) where T : ScriptableObject {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (!created) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }

    internal static class PrefabUpsert {
        public static GameObject Edit(string path, Func<GameObject> createRoot, Action<GameObject> configure,
            out bool created) {
            created = AssetDatabase.LoadAssetAtPath<GameObject>(path) == null;
            if (created) {
                var root = createRoot();
                configure(root);
                var asset = PrefabUtility.SaveAsPrefabAsset(root, path);
                Object.DestroyImmediate(root);
                return asset;
            }

            var contents = PrefabUtility.LoadPrefabContents(path);
            try {
                configure(contents);
                return PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        public static GameObject EditVariant(string path, GameObject source, Action<GameObject> configure,
            out bool created) {
            return Edit(path, () => (GameObject)PrefabUtility.InstantiatePrefab(source), configure, out created);
        }
    }

    internal static class ObjectTools {
        public static SpriteRenderer SetSprite(GameObject go, Sprite sprite, SpriteDrawMode mode = SpriteDrawMode.Simple) {
            var renderer = go.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = mode;
            return renderer;
        }

        public static void SetAnimator(GameObject go, RuntimeAnimatorController controller) {
            if (controller == null) return;
            var animator = go.GetComponent<Animator>();
            if (animator == null) animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
        }

        public static BoxCollider2D SetBox(GameObject go, Vector2 size, Vector2 offset, bool trigger) {
            var box = go.GetComponent<BoxCollider2D>();
            if (box == null) box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.offset = offset;
            box.isTrigger = trigger;
            return box;
        }

        public static void SetLayer(GameObject go, string layerName) {
            var layer = LayerMask.NameToLayer(layerName);
            if (layer >= 0) go.layer = layer;
        }

        public static Transform FindDeep(Transform root, string name) {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        public static Transform EnsureChild(Transform parent, string name) {
            var child = parent.Find(name);
            if (child != null) return child;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        public static T EnsureComponent<T>(GameObject go) where T : Component {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }
    }
}
