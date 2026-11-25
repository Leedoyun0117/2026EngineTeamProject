using System;

namespace LDY.Death {
    // 덫 사망은 클립 전에 덫 중심 x로 끌려간다. SmoothStep 커브.
    public sealed class DeathPull {
        public const float DefaultDuration = 0.25f;

        private readonly float _startX;
        private readonly float _targetX;
        private readonly float _duration;
        private float _elapsed;

        public DeathPull(float startX, float targetX, float duration) {
            _startX = startX;
            _targetX = targetX;
            _duration = Math.Max(0f, duration);
        }

        public bool Finished => _elapsed >= _duration;

        public static bool Applies(DeathCause cause, float? anchorX) => cause == DeathCause.Trap && anchorX.HasValue;

        public float Tick(float deltaTime) {
            _elapsed += Math.Max(0f, deltaTime);
            return XAt(Progress);
        }

        private float Progress => _duration <= 0f ? 1f : Math.Min(1f, _elapsed / _duration);

        private float XAt(float t) {
            var eased = t * t * (3f - 2f * t);
            return _startX + (_targetX - _startX) * eased;
        }
    }
}
