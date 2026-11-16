using UnityEngine;

namespace LDY.Script {
    public readonly struct FootPin {
        public readonly Vector2 Target;
        public readonly float Distance;

        public FootPin(Vector2 target, float distance) {
            Target = target;
            Distance = distance;
        }
    }
}
