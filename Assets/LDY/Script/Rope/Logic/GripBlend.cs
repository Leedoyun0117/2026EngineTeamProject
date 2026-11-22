using UnityEngine;

namespace LDY.Script {
    // 매달림 회전을 붙을 때/놓을 때 0~1로 부드럽게 섞는 가중치.
    public sealed class GripBlend {
        private readonly float _duration;

        public GripBlend(float duration) {
            _duration = Mathf.Max(duration, Mathf.Epsilon);
        }

        public float Weight { get; private set; }

        // engaged면 1로, 아니면 0으로 duration에 걸쳐 이동
        public float Step(bool engaged, float deltaTime) {
            Weight = Mathf.MoveTowards(Weight, engaged ? 1f : 0f, deltaTime / _duration);
            return Weight;
        }
    }
}
