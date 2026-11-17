using UnityEngine;

namespace LDY.Script {
    // 줄의 발 고정점 정보: 목표 월드 위치(Target)와 손에서 발까지 줄 위 거리(Distance).
    public readonly struct FootPin {
        public readonly Vector2 Target;
        public readonly float Distance;

        public FootPin(Vector2 target, float distance) {
            Target = target;
            Distance = distance;
        }
    }
}
