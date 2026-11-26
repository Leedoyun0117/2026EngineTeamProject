using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDY.Flow {
    // 루트에 "Bounds"/"CameraBounds" 콜라이더가 있으면 그걸, 없으면 루트 타일맵 범위로 만든 사각형을 Confiner에 건다.
    public static class StageCameraBinder {
        private static readonly string[] BoundsNames = { "CameraBounds", "Bounds" };
        private const string GeneratedName = "[Flow] CameraBounds";

        public static string Bind(GameObject root) {
            var camera = Object.FindFirstObjectByType<CinemachineCamera>();
            if (camera == null) return "CinemachineCamera 없음, 건너뜀";

            var shape = FindBounds(root);
            var source = shape != null ? shape.name : "타일맵 범위";
            if (shape == null) shape = FromTilemaps(root);
            if (shape == null) return "범위를 정할 타일맵이 없어 건너뜀";

            var confiner = camera.GetComponent<CinemachineConfiner2D>();
            if (confiner == null) confiner = camera.gameObject.AddComponent<CinemachineConfiner2D>();
            confiner.BoundingShape2D = shape;
            confiner.InvalidateBoundingShapeCache();
            return $"{camera.name} Confiner <- {source}";
        }

        private static Collider2D FindBounds(GameObject root) {
            foreach (var name in BoundsNames)
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name && t.TryGetComponent<Collider2D>(out var collider))
                    return collider;
            return null;
        }

        private static Collider2D FromTilemaps(GameObject root) {
            var hasAny = false;
            var bounds = new Bounds();
            foreach (var map in root.GetComponentsInChildren<Tilemap>(true)) {
                map.CompressBounds();
                if (map.cellBounds.size.x == 0 || map.cellBounds.size.y == 0) continue;
                var local = map.localBounds;
                var min = map.transform.TransformPoint(local.min);
                var max = map.transform.TransformPoint(local.max);
                if (!hasAny) bounds = new Bounds(min, Vector3.zero);
                bounds.Encapsulate(min);
                bounds.Encapsulate(max);
                hasAny = true;
            }

            if (!hasAny) return null;
            var go = new GameObject(GeneratedName) { layer = 2 };
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, root.scene);
            var polygon = go.AddComponent<PolygonCollider2D>();
            polygon.isTrigger = true;
            polygon.excludeLayers = ~0;
            polygon.points = new[] {
                new Vector2(bounds.min.x, bounds.min.y), new Vector2(bounds.max.x, bounds.min.y),
                new Vector2(bounds.max.x, bounds.max.y), new Vector2(bounds.min.x, bounds.max.y)
            };
            return polygon;
        }
    }
}
