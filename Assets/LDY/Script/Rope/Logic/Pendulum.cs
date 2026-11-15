using System;
using UnityEngine;

namespace LDY.Script {
    [Serializable]
    public struct PendulumTuning {
        public float minLength;
        public float maxLength;
        public float gravity;
        public float damping;
        public float quadraticDamping;
        public float swingForce;
        public float climbSpeed;
        public float softLimitStartDegrees;
        public float softLimitStrength;
        public float maxAngleDegrees;
    }

    public readonly struct PendulumState {
        public readonly float Angle;
        public readonly float AngularVelocity;
        public readonly float Length;

        public PendulumState(float angle, float angularVelocity, float length) {
            Angle = angle;
            AngularVelocity = angularVelocity;
            Length = length;
        }
    }

    public readonly struct PendulumInput {
        public readonly float Swing;
        public readonly float Climb;

        public PendulumInput(float swing, float climb) {
            Swing = swing;
            Climb = climb;
        }
    }

    public sealed class Pendulum {
        private const float MinSafeLength = 0.01f;
        private const float SoftRangeFallback = Mathf.PI / 3f;
        private readonly PendulumTuning _tuning;

        public Pendulum(PendulumTuning tuning) {
            _tuning = tuning;
        }

        public static Vector2 PositionOf(Vector2 anchor, PendulumState state) {
            return anchor + state.Length * new Vector2(Mathf.Sin(state.Angle), -Mathf.Cos(state.Angle));
        }

        public static Vector2 TangentVelocity(PendulumState state) {
            return state.Length * state.AngularVelocity * new Vector2(Mathf.Cos(state.Angle), Mathf.Sin(state.Angle));
        }

        public float ClampLength(float length) {
            var min = Mathf.Max(_tuning.minLength, MinSafeLength);
            var max = Mathf.Max(_tuning.maxLength, min);
            return Mathf.Clamp(length, min, max);
        }

        public PendulumState CreateState(Vector2 anchor, Vector2 position, Vector2 velocity) {
            var offset = position - anchor;
            var length = ClampLength(offset.magnitude);
            var angle = offset.sqrMagnitude < Mathf.Epsilon ? 0f : Mathf.Atan2(offset.x, -offset.y);
            var tangent = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            return new PendulumState(angle, Vector2.Dot(velocity, tangent) / length, length);
        }

        public PendulumState Step(PendulumState state, PendulumInput input, float deltaTime) {
            var length = ClampLength(state.Length - input.Climb * _tuning.climbSpeed * deltaTime);
            var omega = state.AngularVelocity * state.Length / length;
            var reach = LimitReach(state.Angle);
            var outward = state.Angle * input.Swing > 0f;

            var gravityPull = -(_tuning.gravity / length) * Mathf.Sin(state.Angle);
            var swingPush = input.Swing * _tuning.swingForce / length * Mathf.Cos(state.Angle);
            if (outward)
                swingPush *= 1f - reach;

            var drag = -_tuning.damping * omega - _tuning.quadraticDamping * omega * Mathf.Abs(omega);
            var limitPull = -Mathf.Sign(state.Angle) * _tuning.softLimitStrength * (_tuning.gravity / length) * reach * reach;
            var limitDrag = omega * state.Angle > 0f ? -omega * _tuning.softLimitStrength * reach : 0f;

            omega += (gravityPull + swingPush + drag + limitPull + limitDrag) * deltaTime;
            var angle = state.Angle + omega * deltaTime;
            return LimitAngle(angle, omega, length);
        }

        private float LimitReach(float angle) {
            var start = _tuning.softLimitStartDegrees * Mathf.Deg2Rad;
            if (start <= 0f || _tuning.softLimitStrength <= 0f)
                return 0f;

            var hard = _tuning.maxAngleDegrees * Mathf.Deg2Rad;
            var end = hard > start ? hard : start + SoftRangeFallback;
            return Mathf.Clamp01((Mathf.Abs(angle) - start) / (end - start));
        }

        private PendulumState LimitAngle(float angle, float omega, float length) {
            var limit = _tuning.maxAngleDegrees * Mathf.Deg2Rad;
            if (limit <= 0f || Mathf.Abs(angle) <= limit)
                return new PendulumState(angle, omega, length);

            angle = Mathf.Sign(angle) * limit;
            if (omega * angle > 0f)
                omega = 0f;

            return new PendulumState(angle, omega, length);
        }
    }
}
