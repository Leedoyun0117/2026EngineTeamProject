using UnityEngine;

namespace LDY.Script {
    // 선분과 점 사이의 기하 계산.
    public static class RopeReach {
        // 점을 선분에 수직으로 투영한 가장 가까운 점(선분 밖이면 끝점)
        public static Vector2 ClosestPoint(Vector2 start, Vector2 end, Vector2 point) {
            var segment = end - start;
            var lengthSqr = segment.sqrMagnitude;
            if (lengthSqr < Mathf.Epsilon)
                return start;

            var t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSqr);
            return start + segment * t;
        }
    }
}
