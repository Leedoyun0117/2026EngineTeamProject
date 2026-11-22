using System;

namespace LDY.Spider {
    public static class BodyBob {
        public static float Offset(float time, float amplitude, float period) {
            return period <= 0f ? 0f : amplitude * MathF.Sin(time / period * MathF.PI * 2f);
        }
    }

    public sealed class GlitchClock {
        private readonly float _interval;
        private readonly float _duration;
        private float _timer;

        public GlitchClock(float interval, float duration) {
            _interval = interval;
            _duration = duration;
        }

        public bool IsGlitching { get; private set; }

        public void Tick(float deltaTime) {
            if (_interval <= 0f)
                return;

            _timer += deltaTime;
            if (IsGlitching) {
                if (_timer < _duration)
                    return;

                IsGlitching = false;
                _timer = 0f;
            }
            else if (_timer >= _interval) {
                IsGlitching = true;
                _timer = 0f;
            }
        }
    }

    // 최대 속도와 가감속이 있는 x 추적. 목표 앞에서는 멈출 수 있는 속도까지만 낸다.
    public sealed class BodyTracker {
        private readonly float _maxSpeed;
        private readonly float _acceleration;

        public BodyTracker(float startX, float maxSpeed, float acceleration) {
            X = startX;
            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
        }

        public float X { get; private set; }
        public float Velocity { get; private set; }

        public void Step(float deltaTime, float targetX, bool hold) {
            var distance = hold ? 0f : targetX - X;
            var brakingSpeed = MathF.Sqrt(2f * _acceleration * MathF.Abs(distance));
            var desired = MathF.Sign(distance) * MathF.Min(_maxSpeed, brakingSpeed);
            Velocity = MoveToward(Velocity, desired, _acceleration * deltaTime);
            X += Velocity * deltaTime;
        }

        private static float MoveToward(float current, float target, float maxDelta) {
            return MathF.Abs(target - current) <= maxDelta ? target : current + MathF.Sign(target - current) * maxDelta;
        }
    }

    public sealed class SmoothedValue {
        public float Value { get; private set; }

        public float Step(float target, float ratePerSecond, float deltaTime) {
            var maxDelta = ratePerSecond * deltaTime;
            Value = MathF.Abs(target - Value) <= maxDelta ? target : Value + MathF.Sign(target - Value) * maxDelta;
            return Value;
        }
    }

    // 찍는 순간 몸이 아래로 튕겼다 돌아오는 오프셋.
    public sealed class ImpactBounce {
        private readonly float _amplitude;
        private readonly float _duration;
        private float _time;

        public ImpactBounce(float amplitude, float duration) {
            _amplitude = amplitude;
            _duration = duration;
            _time = duration;
        }

        public void Trigger() => _time = 0f;

        public float Step(float deltaTime) {
            if (_time >= _duration)
                return 0f;

            _time += deltaTime;
            var t = Easing.Clamp01(_time / _duration);
            return -_amplitude * MathF.Sin(MathF.PI * t);
        }
    }

    public static class BodyPose {
        // 공격 중인 다리 쪽(+는 오른쪽)으로 몸을 기울인다. 오른쪽으로 기울면 시계 방향(-)이다.
        public static float LeanTarget(SpiderState state, float legSide, float degrees) {
            var attacking = state == SpiderState.Telegraph || state == SpiderState.Stab || state == SpiderState.Stuck;
            return attacking ? -MathF.Sign(legSide) * degrees : 0f;
        }
    }
}
