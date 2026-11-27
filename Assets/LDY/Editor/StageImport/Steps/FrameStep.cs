using Unity.Cinemachine;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 템플릿의 Bounds·DeathZone·카메라를 맵 크기에 맞추고, StageCamera가 Player를 따라가며 Bounds 안에 갇히는지 확인한다.
    internal sealed class FrameStep : ISceneStep {
        private const float MinWidth = 18f;
        private const float MinHeight = 10f;
        private const float DeathZoneDepth = 3f;
        private const float DeathZoneMargin = 8f;
        private const string RigName = "StageCamera";

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
            else {
                context.Log.Warn("템플릿 Bounds(PolygonCollider2D)가 없어 카메라 범위를 맞추지 못했다");
            }

            var death = context.FindRoot("DeathZone");
            if (death != null) {
                death.transform.position = new Vector3(center.x, center.y - half.y - DeathZoneDepth * 0.5f, 0f);
                var box = death.GetComponent<BoxCollider2D>();
                if (box != null) box.size = new Vector2(half.x * 2f + DeathZoneMargin, DeathZoneDepth);
            }

            var player = context.FindPlayer();
            var focus = player != null ? (Vector2)player.transform.position : center;
            foreach (var name in new[] { "Main Camera", RigName }) {
                var go = context.FindRoot(name);
                if (go != null) go.transform.position = new Vector3(focus.x, focus.y, -10f);
            }

            CheckRig(context, player, bounds);
            context.Log.Info($"카메라 범위 {half.x * 2f}x{half.y * 2f}, 맵 {width}x{height}");
        }

        // 템플릿 카메라: StageCamera(CinemachineCamera, Follow=Player) + CinemachineConfiner2D(Bounds).
        // 비어 있으면 채우고, 다른 대상을 가리키면 바꾸지 않고 알린다.
        private static void CheckRig(SceneContext context, GameObject player, PolygonCollider2D bounds) {
            var rig = context.FindRoot(RigName);
            var cm = rig != null ? rig.GetComponent<CinemachineCamera>() : null;
            if (cm == null) {
                context.Log.Warn($"{RigName}(CinemachineCamera)가 없어 카메라가 플레이어를 따라가지 않는다");
                return;
            }

            if (player != null && cm.Follow == null) {
                cm.Follow = player.transform;
                context.Log.Info($"{RigName} Follow가 비어 있어 {player.name}로 채움");
            }
            else if (player != null && cm.Follow != player.transform) {
                context.Log.Warn($"{RigName} Follow가 {cm.Follow.name}라서 Player를 따라가지 않는다 (바꾸지 않음)");
            }

            var confiner = rig.GetComponent<CinemachineConfiner2D>();
            if (confiner == null) {
                context.Log.Warn($"{RigName}에 CinemachineConfiner2D가 없어 카메라가 맵 밖을 비출 수 있다");
                return;
            }

            if (bounds != null && confiner.BoundingShape2D == null) confiner.BoundingShape2D = bounds;
            if (bounds != null && confiner.BoundingShape2D != bounds)
                context.Log.Warn($"{RigName} Confiner가 {confiner.BoundingShape2D.name}를 가리킨다 (Bounds 아님)");
            confiner.InvalidateBoundingShapeCache();

            var following = player != null && cm.Follow == player.transform;
            context.Log.Info($"카메라 확인: Follow={(cm.Follow != null ? cm.Follow.name : "없음")}" +
                             $"{(following ? " (Player)" : "")}, Confiner={(confiner.BoundingShape2D != null ? confiner.BoundingShape2D.name : "없음")}");
        }
    }
}
