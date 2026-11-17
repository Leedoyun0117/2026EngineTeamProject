using System;
using UnityEngine;

namespace LDY.Script {
    // 진자 수치: 길이 범위, 중력, 선형/제곱 감쇠, 흔들기 힘, 오르내림 속도, 소프트 한계 시작 각도·세기, 하드 최대 각도(0이면 제한 없음)
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

    // 진자 상태: 각도(수직 아래 0, 라디안), 각속도(rad/s), anchor로부터의 거리 s
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

    // 입력: Swing(좌우 흔들기), Climb(오르내림)
    public readonly struct PendulumInput {
        public readonly float Swing;
        public readonly float Climb;

        public PendulumInput(float swing, float climb) {
            Swing = swing;
            Climb = climb;
        }
    }

    // 매달린 플레이어를 진자로 움직이는 순수 계산. 입력/물리엔진/줄 모양을 모른다.
    public sealed class Pendulum {
        private const float MinSafeLength = 0.01f;
        private const float SoftRangeFallback = Mathf.PI / 3f;
        private readonly PendulumTuning _tuning;

        public Pendulum(PendulumTuning tuning) {
            _tuning = tuning;
        }

        // anchor와 상태로 플레이어 월드 위치 계산(각도 0이면 anchor 바로 아래)
        public static Vector2 PositionOf(Vector2 anchor, PendulumState state) {
            return anchor + state.Length * new Vector2(Mathf.Sin(state.Angle), -Mathf.Cos(state.Angle));
        }

        // 접선 속도 = 길이 x 각속도. 놓을 때 날아가는 속도가 된다.
        public static Vector2 TangentVelocity(PendulumState state) {
            return state.Length * state.AngularVelocity * new Vector2(Mathf.Cos(state.Angle), Mathf.Sin(state.Angle));
        }

        // 거리를 최소~최대 범위로 제한
        public float ClampLength(float length) {
            var min = Mathf.Max(_tuning.minLength, MinSafeLength);
            var max = Mathf.Max(_tuning.maxLength, min);
            return Mathf.Clamp(length, min, max);
        }

        // 잡은 위치와 현재 속도에서 시작 상태 생성(속도는 접선 성분만 반영)
        public PendulumState CreateState(Vector2 anchor, Vector2 position, Vector2 velocity) {
            var offset = position - anchor;
            var length = ClampLength(offset.magnitude);
            var angle = offset.sqrMagnitude < Mathf.Epsilon ? 0f : Mathf.Atan2(offset.x, -offset.y);
            var tangent = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            return new PendulumState(angle, Vector2.Dot(velocity, tangent) / length, length);
        }

        // 한 틱 진행: 길이 갱신 -> 중력/흔들기/감쇠/한계 힘을 합산해 각속도·각도 갱신.
        public PendulumState Step(PendulumState state, PendulumInput input, float deltaTime) {
            var length = ClampLength(state.Length - input.Climb * _tuning.climbSpeed * deltaTime);
            // 길이가 변해도 접선 속도가 유지되도록 각속도 보정
            var omega = state.AngularVelocity * state.Length / length;
            var reach = LimitReach(state.Angle);
            // 바깥쪽(각도가 커지는 쪽)으로 미는지
            var outward = state.Angle * input.Swing > 0f;

            // 중력이 아래로 되돌리는 힘
            var gravityPull = -(_tuning.gravity / length) * Mathf.Sin(state.Angle);
            // 입력으로 미는 힘(바깥쪽은 한계에 가까울수록 약해짐)
            var swingPush = input.Swing * _tuning.swingForce / length * Mathf.Cos(state.Angle);
            if (outward)
                swingPush *= 1f - reach;

            // 선형 + 제곱 감쇠(빠를수록 더 세게 깎음)
            var drag = -_tuning.damping * omega - _tuning.quadraticDamping * omega * Mathf.Abs(omega);
            // 소프트 한계 구간에서 안쪽으로 되돌리는 힘
            var limitPull = -Mathf.Sign(state.Angle) * _tuning.softLimitStrength * (_tuning.gravity / length) * reach * reach;
            // 소프트 한계 구간에서 바깥으로 향하는 속도를 깎는 힘
            var limitDrag = omega * state.Angle > 0f ? -omega * _tuning.softLimitStrength * reach : 0f;

            omega += (gravityPull + swingPush + drag + limitPull + limitDrag) * deltaTime;
            var angle = state.Angle + omega * deltaTime;
            return LimitAngle(angle, omega, length);
        }

        // 소프트 한계에 얼마나 들어갔는지 0~1(시작 각도 -> 하드 한계)
        private float LimitReach(float angle) {
            var start = _tuning.softLimitStartDegrees * Mathf.Deg2Rad;
            if (start <= 0f || _tuning.softLimitStrength <= 0f)
                return 0f;

            var hard = _tuning.maxAngleDegrees * Mathf.Deg2Rad;
            var end = hard > start ? hard : start + SoftRangeFallback;
            return Mathf.Clamp01((Mathf.Abs(angle) - start) / (end - start));
        }

        // 하드 한계: 각도를 자르고 바깥쪽 속도는 0으로
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
