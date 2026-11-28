using System.IO;
using LDY.FlowEditor;
using LDY.StageImport;
using LDY.StageImport.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 대응 프리팹이 없는 흡수 UI 자리. 픽업 그림(ui_{능력})이 있으면 그 그림, 없으면 분홍 사각형. 동작 스크립트는 없다(흡수 안 됨).
    // 이름표는 StageLabel(Scene 뷰 기즈모, EditorOnly)로만 붙인다. 이미 있는 임시 프리팹도 그림·이름표가 다르면 고친다.
    internal static class TempPrefabFactory {
        private const string KeyPrefix = "Temp_";
        private const string LabelName = "Label";
        private const string EditorOnlyTag = "EditorOnly";
        private static readonly Color Pink = new Color(1f, 0.4f, 0.75f, 1f);

        public static GameObject Ensure(string key, FlowLog log) {
            var path = StageImportPaths.TempPrefab(key);
            var look = Look.For(key);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing == null) return Create(path, look, log);

            if (!look.Matches(existing)) Update(path, look, log);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // 폴더에 이미 있는 Temp_abs_* 프리팹을 전부 다시 맞춘다(가져오기 없이 Imported 씬에 반영할 때).
        public static void RefreshAll(FlowLog log) {
            if (!AssetDatabase.IsValidFolder(StageImportPaths.TempPrefabs)) return;
            foreach (var file in Directory.GetFiles(StageImportPaths.TempPrefabs, KeyPrefix + ToolKeys.AbsorbPrefix + "*.prefab")) {
                var key = Path.GetFileNameWithoutExtension(file).Substring(KeyPrefix.Length);
                var path = StageImportPaths.TempPrefab(key);
                var look = Look.For(key);
                if (look.Matches(AssetDatabase.LoadAssetAtPath<GameObject>(path))) log.Info("그대로 " + path);
                else Update(path, look, log);
            }
        }

        private static GameObject Create(string path, Look look, FlowLog log) {
            FlowPaths.EnsureFolder(StageImportPaths.TempPrefabs);
            var go = new GameObject(Path.GetFileNameWithoutExtension(path));
            try {
                go.AddComponent<SpriteRenderer>();
                go.AddComponent<BoxCollider2D>().isTrigger = true;
                look.Apply(go);
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                log.Info($"임시 프리팹 만듦 {path} ({look.Describe})");
                return prefab;
            }
            finally {
                Object.DestroyImmediate(go);
            }
        }

        private static void Update(string path, Look look, FlowLog log) {
            var root = PrefabUtility.LoadPrefabContents(path);
            try {
                look.Apply(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                log.Info($"임시 프리팹 갱신 {path} ({look.Describe})");
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private sealed class Look {
            private readonly Sprite _art;
            private readonly string _label;

            private Look(Sprite art, string label) {
                _art = art;
                _label = label;
            }

            public static Look For(string key) {
                var ability = ToolKeys.AbilityOf(key) ?? key;
                return new Look(PickupArt.Find(ability), $"흡수 UI(임시): {ability}");
            }

            public string Describe => _art != null ? "그림 " + AssetDatabase.GetAssetPath(_art) : "그림 없음, 분홍";

            private Sprite Sprite => _art != null ? _art : AssetDatabase.LoadAssetAtPath<Sprite>(StageImportPaths.TempSprite);
            private Color Color => _art != null ? Color.white : Pink;

            public bool Matches(GameObject root) {
                if (root == null || !root.TryGetComponent<SpriteRenderer>(out var renderer)) return false;
                var label = root.transform.Find(LabelName);
                return renderer.sprite == Sprite && renderer.color == Color && label != null &&
                       label.TryGetComponent<StageLabel>(out var text) && text.Text == _label;
            }

            public void Apply(GameObject root) {
                var renderer = root.GetComponent<SpriteRenderer>();
                renderer.sprite = Sprite;
                renderer.color = Color;
                var bounds = renderer.sprite != null ? renderer.sprite.bounds : new Bounds(Vector3.zero, Vector3.one);
                if (root.TryGetComponent<BoxCollider2D>(out var box)) {
                    box.size = bounds.size;
                    box.offset = bounds.center;
                }

                var label = root.transform.Find(LabelName);
                if (label == null) {
                    label = new GameObject(LabelName).transform;
                    label.SetParent(root.transform, false);
                }

                label.gameObject.tag = EditorOnlyTag;
                label.localPosition = new Vector3(bounds.center.x, bounds.max.y, 0f);
                if (!label.TryGetComponent<StageLabel>(out var text)) text = label.gameObject.AddComponent<StageLabel>();
                text.SetText(_label);
            }
        }
    }
}
