using UnityEngine;

namespace LDY.Script {
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

    public sealed class GripSolver {
        private readonly GripTuning _tuning;
        private Vector2 _offset;
        private bool _tracking;

        public GripSolver(GripTuning tuning) {
            _tuning = tuning;
        }

        public void Reset() {
            _tracking = false;
        }

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

        private Vector2 Smooth(Vector2 current, Vector2 target, float deltaTime) {
            if (_tuning.footSmoothTime <= 0f)
                return target;

            return Vector2.Lerp(current, target, 1f - Mathf.Exp(-deltaTime / _tuning.footSmoothTime));
        }
    }
}
