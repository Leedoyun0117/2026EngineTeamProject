using UnityEngine;

namespace LDY.StageImportEditor {
    // 프리팹 피벗이 제각각이라 렌더러 범위로 맞춘다. 범위가 없으면 피벗을 그대로 쓴다.
    internal static class PrefabAligner {
        public static void Center(GameObject go, Vector3 center) {
            go.transform.position = center;
            if (TryBounds(go, out var bounds)) go.transform.position += center - bounds.center;
        }

        public static void Bottom(GameObject go, float centerX, float bottomY) {
            go.transform.position = new Vector3(centerX, bottomY, 0f);
            if (!TryBounds(go, out var bounds)) return;
            go.transform.position += new Vector3(centerX - bounds.center.x, bottomY - bounds.min.y, 0f);
        }

        private static bool TryBounds(GameObject go, out Bounds bounds) {
            bounds = default;
            var found = false;
            foreach (var renderer in go.GetComponentsInChildren<Renderer>()) {
                if (!renderer.enabled || renderer.bounds.size == Vector3.zero) continue;
                if (!found) bounds = renderer.bounds;
                else bounds.Encapsulate(renderer.bounds);
                found = true;
            }

            return found;
        }
    }
}
