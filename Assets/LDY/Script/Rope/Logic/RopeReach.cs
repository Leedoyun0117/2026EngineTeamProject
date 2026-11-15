using UnityEngine;

namespace LDY.Script {
    public static class RopeReach {
        public static Vector2 ClosestPoint(Vector2 start, Vector2 end, Vector2 point) {
            var segment = end - start;
            var lengthSqr = segment.sqrMagnitude;
            if (lengthSqr < Mathf.Epsilon)
                return start;

            var t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSqr);
            return start + segment * t;
        }

        public static bool TryGetGrabPoint(Vector2 start, Vector2 end, Vector2 point, float range, out Vector2 grabPoint) {
            grabPoint = ClosestPoint(start, end, point);
            return (grabPoint - point).sqrMagnitude <= range * range;
        }
    }
}
