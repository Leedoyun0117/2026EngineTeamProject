using UnityEngine;

namespace LDY.StageImportEditor {
    // 템플릿의 Bounds·DeathZone·카메라를 맵 크기에 맞춘다.
    internal sealed class FrameStep : ISceneStep {
        private const float MinWidth = 18f;
        private const float MinHeight = 10f;
        private const float DeathZoneDepth = 3f;
        private const float DeathZoneMargin = 8f;

        public void Apply(SceneContext context) {
            var width = context.Plan.Width;
            var height = context.Plan.Height;
            var center = new Vector2(width * 0.5f, height * 0.5f);
            var half = new Vector2(Mathf.Max(width, MinWidth), Mathf.Max(height, MinHeight)) * 0.5f;

            var boundsRoot = context.FindRoot("Bounds");
            var bounds = boundsRoot != null ? boundsRoot.GetComponent<PolygonCollider2D>() : null;
            if (bounds != null) {
                bounds.transform.position = Vector3.zero;
                bounds.points = new[] {
                    center + new Vector2(-half.x, -half.y), center + new Vector2(half.x, -half.y),
                    center + new Vector2(half.x, half.y), center + new Vector2(-half.x, half.y)
                };
            }

            var death = context.FindRoot("DeathZone");
            if (death != null) {
                death.transform.position = new Vector3(center.x, center.y - half.y - DeathZoneDepth * 0.5f, 0f);
                var box = death.GetComponent<BoxCollider2D>();
                if (box != null) box.size = new Vector2(half.x * 2f + DeathZoneMargin, DeathZoneDepth);
            }

            var player = context.FindPlayer();
            var focus = player != null ? (Vector2)player.transform.position : center;
            foreach (var name in new[] { "Main Camera", "StageCamera" }) {
                var go = context.FindRoot(name);
                if (go != null) go.transform.position = new Vector3(focus.x, focus.y, -10f);
            }

            context.Log.Info($"카메라 범위 {half.x * 2f}x{half.y * 2f}, 맵 {width}x{height}");
        }
    }
}
