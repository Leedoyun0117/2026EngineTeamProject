using UnityEngine;

namespace LDY.Script {
    public sealed class GripBlend {
        private readonly float _duration;

        public GripBlend(float duration) {
            _duration = Mathf.Max(duration, Mathf.Epsilon);
        }

        public float Weight { get; private set; }

        public float Step(bool engaged, float deltaTime) {
            Weight = Mathf.MoveTowards(Weight, engaged ? 1f : 0f, deltaTime / _duration);
            return Weight;
        }
    }
}
