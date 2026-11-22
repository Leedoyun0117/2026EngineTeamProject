using UnityEngine;

namespace LDY.Script {
    // 계산 결과: 회전각, 발 고정 여부/위치, 손에서 발까지 거리.
    public readonly struct GripResult {
        public readonly float Angle;
        public readonly bool HasFoot;
        public readonly Vector2 Foot;
        public readonly float FootDistance;

        public GripResult(float angle, bool hasFoot, Vector2 foot, float footDistance) {
            Angle = angle;
            HasFoot = hasFoot;
            Foot = foot;
            FootDistance = footDistance;
        }
    }

    // 진자 각도와 스프라이트 샘플로 매달림 회전각과 발이 닿을 줄 위치를 계산한다.
    public sealed class GripSolver {
        private readonly GripTuning _tuning;
        // 손 기준 발 오프셋(부드럽게 따라가는 현재값)
        private Vector2 _offset;
        // 발을 추적 중인지(처음엔 스무딩 없이 바로 맞춤)
        private bool _tracking;

        public GripSolver(GripTuning tuning) {
            _tuning = tuning;
        }

        // 잡거나 놓을 때 추적 초기화
        public void Reset() {
            _tracking = false;
        }

        // 각도를 스냅하고, 발이 있으면 목표 위치를 구해 스무딩한다.
        public GripResult Solve(float pendulumAngle, Vector2 hand, in GripSample sample, float deltaTime) {
            var angle = GripAngle.Snap(pendulumAngle, _tuning.angleStepDegrees, _tuning.maxAngleDegrees);
            if (!sample.HasFoot) {
                _tracking = false;
                return new GripResult(angle, false, hand, 0f);
            }

            var target = sample.Layout.OffsetFromHand(sample.FootPixel, sample.Flipped, angle);
            _offset = _tracking ? Smooth(_offset, target, deltaTime) : target;
            _tracking = true;
            return new GripResult(angle, true, hand + _offset, _offset.magnitude);
        }

        // 지수 보간: 프레임률과 무관하게 footSmoothTime 정도로 따라감
        private Vector2 Smooth(Vector2 current, Vector2 target, float deltaTime) {
            if (_tuning.footSmoothTime <= 0f)
                return target;

            return Vector2.Lerp(current, target, 1f - Mathf.Exp(-deltaTime / _tuning.footSmoothTime));
        }
    }
}
