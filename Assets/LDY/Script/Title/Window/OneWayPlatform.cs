using UnityEngine;

namespace LDY.Script {
    // 아래에서 점프하면 통과하는 단방향 발판(PlatformEffector2D). 타이틀 창(WindowKit)이 쓴다.
    // 윗면이 top에 오도록 두께만큼 아래로 내려서 둔다. 플레이어 바닥 판정이 쓰는 Ground 레이어에 둬야 함
    public static class OneWayPlatform {
        public const float Thickness = 0.1f;

        public static BoxCollider2D Create(string name, Transform parent, float xMin, float xMax, float top, int layer) {
            var go = new GameObject(name) { layer = layer };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3((xMin + xMax) * 0.5f, top - Thickness * 0.5f, 0f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(xMax - xMin, Thickness);
            box.usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = WindowStyle.PlatformSurfaceArc;
            return box;
        }
    }
}
