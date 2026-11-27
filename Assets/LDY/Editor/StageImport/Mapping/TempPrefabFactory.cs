using LDY.FlowEditor;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 대응 프리팹이 없는 흡수 UI 자리에 둘 분홍 사각형. 기능은 없다.
    internal static class TempPrefabFactory {
        private static readonly Color Pink = new Color(1f, 0.4f, 0.75f, 1f);

        public static GameObject Ensure(string key, FlowLog log) {
            var path = StageImportPaths.TempPrefab(key);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            FlowPaths.EnsureFolder(StageImportPaths.TempPrefabs);
            var go = new GameObject("Temp_" + key);
            try {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(StageImportPaths.TempSprite);
                renderer.color = Pink;
                go.AddComponent<BoxCollider2D>().isTrigger = true;
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                log.Info("임시 프리팹 만듦 " + path);
                return prefab;
            }
            finally {
                Object.DestroyImmediate(go);
            }
        }
    }
}
