using UnityEditor;
using UnityEngine;

namespace LDY.Script.Editor {
    // AltPlayer Rigidbody2D에 TitleSetupSettings의 물리 재질을 연결한다.
    // 참조가 비어 있으면 기존 재질을 덮어쓰지 않고 경고만 남긴다.
    public static class TitlePlayerPhysics {
        // 바뀌었으면 true
        public static bool Apply(Rigidbody2D body) {
            if (body == null) return false;
            var settings = TitleSetupSettings.LoadOrCreate();
            var material = settings.playerPhysicsMaterial;
            if (material == null) {
                var current = body.sharedMaterial != null ? body.sharedMaterial.name : "없음";
                Debug.LogWarning($"[Title] {TitleSetupSettings.AssetPath}의 playerPhysicsMaterial이 비어 있어 " +
                                 $"{body.name} 물리 재질을 바꾸지 않았다(현재: {current})", settings);
                return false;
            }

            if (body.sharedMaterial == material) return false;
            body.sharedMaterial = material;
            return true;
        }

        // 이미 있는 프리팹에도 적용한다. GUID는 유지된다(열어서 저장).
        public static void ApplyToPrefab(string prefabPath) {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try {
                if (!Apply(root.GetComponent<Rigidbody2D>())) return;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[Title] {prefabPath} 물리 재질 → {root.GetComponent<Rigidbody2D>().sharedMaterial.name}");
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
