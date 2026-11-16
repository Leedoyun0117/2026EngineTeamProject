using UnityEngine;

namespace LDY.Script {
    public static class GripAngle {
        public static float Snap(float radians, float stepDegrees, float maxDegrees) {
            var limit = Mathf.Max(maxDegrees, 0f);
            var degrees = Mathf.Clamp(radians * Mathf.Rad2Deg, -limit, limit);
            if (stepDegrees > 0f)
                degrees = Mathf.Clamp(Mathf.Round(degrees / stepDegrees) * stepDegrees, -limit, limit);

            return degrees * Mathf.Deg2Rad;
        }
    }
}
