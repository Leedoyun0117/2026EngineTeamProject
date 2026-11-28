using System.Collections.Generic;
using System.Linq;
using LDY.Background;
using LDY.Background.Core;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace LDY.BackgroundEditor {
    public static class BackgroundMapFitter {
        private const string BoundsRootName = "Bounds";
        private const float DefaultOrtho = 5f;
        private const float DesignAspect = 16f / 9f;

        public static int FitAll(Scene scene) {
            var fitted = 0;
            foreach (var background in Find<StageBackground>(scene))
                if (Fit(background))
                    fitted++;
            return fitted;
        }

        public static bool Fit(StageBackground background) {
            var scene = background.gameObject.scene;
            if (!TryFindBounds(scene, out var bounds, out var source)) {
                Debug.LogWarning($"[Background] {scene.path}: 맵 bounds를 찾지 못해 맞추지 않았다 (Confiner, Bounds, 타일맵 모두 없음)");
                return false;
            }

            var fit = background.GetComponent<BackgroundMapFit>();
            if (fit == null) fit = Undo.AddComponent<BackgroundMapFit>(background.gameObject);
            var view = ViewHalfSize(scene);
            var map = new WorldRect(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax);
            var plan = MapFit.Plan(map, fit.Mode, fit.FixedScale, fit.Anchor, view.x, view.y);

            var t = background.transform;
            Undo.RecordObject(t, "Fit Background To Map");
            t.position = new Vector3(plan.Window.CenterX, plan.Window.CenterY, t.position.z);
            t.localScale = new Vector3(plan.Scale, plan.Scale, 1f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);
            var desktop = FitDesktop(t, plan);

            Undo.RecordObject(fit, "Fit Background To Map");
            fit.Record(bounds, plan.Scale, source);
            PrefabUtility.RecordPrefabInstancePropertyModifications(fit);
            Debug.Log($"[Background] {scene.path}: {source} {bounds.width:0.##}x{bounds.height:0.##}, {fit.Mode} {fit.Anchor} -> " +
                      $"배율 {plan.Scale}, 창 {Describe(plan.Window)}, " +
                      (desktop ? $"바탕화면 {Describe(plan.Desktop)}" : "Desktop 자식 없음 (Build Stage Backgrounds 먼저)"));
            return true;
        }

        private static bool FitDesktop(Transform root, BackgroundPlan plan) {
            var desktop = root.Find(BackgroundMapFit.DesktopName);
            if (desktop == null) return false;
            Undo.RecordObject(desktop, "Fit Background To Map");
            desktop.localPosition = new Vector3((plan.Desktop.CenterX - plan.Window.CenterX) / plan.Scale,
                (plan.Desktop.CenterY - plan.Window.CenterY) / plan.Scale, desktop.localPosition.z);
            desktop.localScale = new Vector3(plan.Desktop.Width / plan.Scale, plan.Desktop.Height / plan.Scale, 1f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(desktop);
            return true;
        }

        private static Vector2 ViewHalfSize(Scene scene) {
            var lens = Find<CinemachineCamera>(scene).Select(c => c.Lens.OrthographicSize).FirstOrDefault(s => s > 0f);
            var camera = Find<Camera>(scene).FirstOrDefault(c => c.CompareTag("MainCamera"));
            var ortho = lens > 0f ? lens : camera != null ? camera.orthographicSize : DefaultOrtho;
            var aspect = Mathf.Max(DesignAspect, camera != null ? camera.aspect : DesignAspect);
            return new Vector2(ortho * aspect, ortho);
        }

        private static string Describe(WorldRect r) =>
            $"({r.MinX:0.##}, {r.MinY:0.##})~({r.MaxX:0.##}, {r.MaxY:0.##}) {r.Width:0.##}x{r.Height:0.##}";

        public static bool TryFindBounds(Scene scene, out Rect bounds, out string source) {
            var confiner = Find<CinemachineConfiner2D>(scene).FirstOrDefault(c => c.BoundingShape2D != null);
            if (confiner != null && TryColliderRect(confiner.BoundingShape2D, out bounds)) {
                source = "Confiner " + confiner.BoundingShape2D.name;
                return true;
            }

            var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == BoundsRootName);
            var shape = root != null ? root.GetComponent<Collider2D>() : null;
            if (shape != null && TryColliderRect(shape, out bounds)) {
                source = BoundsRootName;
                return true;
            }

            source = "타일맵";
            return TryTilemapRect(Find<Tilemap>(scene), out bounds);
        }

        private static bool TryColliderRect(Collider2D collider, out Rect rect) {
            switch (collider) {
                case PolygonCollider2D polygon when polygon.pathCount > 0:
                    return TryPointsRect(Enumerable.Range(0, polygon.pathCount)
                        .SelectMany(polygon.GetPath)
                        .Select(p => (Vector2)polygon.transform.TransformPoint(p + polygon.offset)), out rect);
                case BoxCollider2D box:
                    var half = box.size * 0.5f;
                    return TryPointsRect(new[] { -half, half, new Vector2(-half.x, half.y), new Vector2(half.x, -half.y) }
                        .Select(p => (Vector2)box.transform.TransformPoint(p + box.offset)), out rect);
                default:
                    var b = collider.bounds;
                    rect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
                    return rect.width > 0f && rect.height > 0f;
            }
        }

        private static bool TryTilemapRect(IEnumerable<Tilemap> tilemaps, out Rect rect) {
            var corners = new List<Vector2>();
            foreach (var map in tilemaps) {
                var used = new List<Vector3Int>();
                foreach (var cell in map.cellBounds.allPositionsWithin)
                    if (map.HasTile(cell))
                        used.Add(cell);
                if (used.Count == 0) continue;
                var min = new Vector3Int(used.Min(p => p.x), used.Min(p => p.y), 0);
                var max = new Vector3Int(used.Max(p => p.x) + 1, used.Max(p => p.y) + 1, 0);
                corners.Add(map.CellToWorld(min));
                corners.Add(map.CellToWorld(max));
            }

            return TryPointsRect(corners, out rect);
        }

        private static bool TryPointsRect(IEnumerable<Vector2> points, out Rect rect) {
            var list = points.ToList();
            rect = default;
            if (list.Count == 0) return false;
            rect = Rect.MinMaxRect(list.Min(p => p.x), list.Min(p => p.y), list.Max(p => p.x), list.Max(p => p.y));
            return rect.width > 0f && rect.height > 0f;
        }

        private static IEnumerable<T> Find<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(true));
    }
}
