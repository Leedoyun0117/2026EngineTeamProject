using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    // RopeCurve와 감쇠 곡선을 묶는 줄의 공개 창구. 컨트롤러가 곡선 내부(pin, blend)를 몰라도 되게 한다.
    public sealed class RopeSimulation {
        private readonly RopeCurve _curve;
        private readonly RopeDampingProfile _damping;
        // 잡을 때 정한 점 번호 / 발이 걸린 점 번호
        private int _grabIndex = -1;
        private int _footIndex = -1;
        // 잡힌 상태인지
        private bool _held;
        // 놓은 뒤 경과 시간(감쇠 곡선 입력)
        private float _sinceRelease = float.PositiveInfinity;

        public RopeSimulation(RopeCurveTuning curve, RopeDampingTuning damping) {
            _curve = new RopeCurve(curve);
            _damping = new RopeDampingProfile(damping);
        }

        public bool IsHeld => _held;
        public int PinIndex => _curve.PinIndex;
        public int FootIndex => _curve.FootIndex;
        public float TotalLength => _curve.TotalLength;
        public float CurrentLength => _curve.CurrentLength;
        public float SegmentLength => _curve.SegmentLength;
        public IReadOnlyList<Vector2> Points => _curve.Points;

        // 점 속도 조회
        public Vector2 PointVelocity(int index) {
            return _curve.VelocityOf(index);
        }

        public void Initialize(Vector2 anchor) {
            _held = false;
            _grabIndex = -1;
            _footIndex = -1;
            _sinceRelease = float.PositiveInfinity;
            _curve.Initialize(anchor);
        }

        // 잡기 전에 플레이어와 가장 가까운 줄 위 점을 찾고 그 점 번호를 기억
        public Vector2 PrepareGrab(Vector2 position) {
            var point = _curve.GetClosest(position, out var index);
            _grabIndex = index;
            return point;
        }

        // 고정만 풀고 경과 시간 측정을 시작(속도는 유지)
        public void Release() {
            if (!_held)
                return;

            _held = false;
            _grabIndex = -1;
            _footIndex = -1;
            _sinceRelease = 0f;
            _curve.ClearPin();
        }

        // 놓은 상태의 한 틱: 경과 시간에 맞는 감쇠로 줄만 시뮬레이션
        public void Step(Vector2 anchor, float deltaTime) {
            _sinceRelease += deltaTime;
            var damping = _damping.AfterRelease(_sinceRelease);
            _curve.Step(anchor, damping, damping, deltaTime);
        }

        // 잡힌 상태의 한 틱: 플레이어 위치에 점을 고정하고(필요하면 발도) 줄을 시뮬레이션
        public void StepHeld(Vector2 anchor, float distance, Vector2 holdPosition, float deltaTime, FootPin? foot = null) {
            _held = true;
            var index = _curve.PinIndex >= 0 ? _curve.PinIndex : ResolveGrabIndex(distance);
            _curve.SetPin(index, holdPosition, distance);
            ApplyFoot(foot);
            _curve.Step(anchor, _damping.Held, _damping.HeldTail, deltaTime);
        }

        // 발 고정점을 줄에 적용하거나 해제
        private void ApplyFoot(FootPin? foot) {
            if (!foot.HasValue) {
                _curve.ClearFootPin();
                _footIndex = -1;
                return;
            }

            if (_footIndex < 0)
                _footIndex = _curve.PinIndex + Mathf.Max(1, Mathf.RoundToInt(foot.Value.Distance / SegmentLength));

            _curve.SetFootPin(_footIndex, foot.Value.Target, foot.Value.Distance);
        }

        // 잡은 점 번호가 없으면 거리로 계산
        private int ResolveGrabIndex(float distance) {
            return _grabIndex >= 0 ? _grabIndex : _curve.IndexAt(distance);
        }
    }
}
