using UnityEngine;

namespace LDY.Script {
    // 진자 각도를 매달림 스프라이트 회전각으로 바꾸는 유틸.
    public static class GripAngle {
        // 최대 각도로 자르고 step도 단위로 반올림(도트 스프라이트가 부드럽게 돌지 않게). 결과는 라디안.
        public static float Snap(float radians, float stepDegrees, float maxDegrees) {
            var limit = Mathf.Max(maxDegrees, 0f);
            var degrees = Mathf.Clamp(radians * Mathf.Rad2Deg, -limit, limit);
            if (stepDegrees > 0f)
                degrees = Mathf.Clamp(Mathf.Round(degrees / stepDegrees) * stepDegrees, -limit, limit);

            return degrees * Mathf.Deg2Rad;
        }
    }
}
