using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    public sealed class RopeSimulation {
        private readonly RopeCurve _curve;
        private readonly RopeDampingProfile _damping;
        private int _grabIndex = -1;
        private bool _held;
        private float _sinceRelease = float.PositiveInfinity;

        public RopeSimulation(RopeCurveTuning curve, RopeDampingTuning damping) {
            _curve = new RopeCurve(curve);
            _damping = new RopeDampingProfile(damping);
        }

        public bool IsHeld => _held;
        public int PinIndex => _curve.PinIndex;
        public float TotalLength => _curve.TotalLength;
        public float CurrentLength => _curve.CurrentLength;
        public float SegmentLength => _curve.SegmentLength;
        public IReadOnlyList<Vector2> Points => _curve.Points;

        public Vector2 PointVelocity(int index) {
            return _curve.VelocityOf(index);
        }

        public void Initialize(Vector2 anchor) {
            _held = false;
            _grabIndex = -1;
            _sinceRelease = float.PositiveInfinity;
            _curve.Initialize(anchor);
        }

        public Vector2 PrepareGrab(Vector2 position) {
            var point = _curve.GetClosest(position, out var index);
            _grabIndex = index;
            return point;
        }

        public void Release() {
            if (!_held)
                return;

            _held = false;
            _grabIndex = -1;
            _sinceRelease = 0f;
            _curve.ClearPin();
        }

        public void Step(Vector2 anchor, float deltaTime) {
            _sinceRelease += deltaTime;
            var damping = _damping.AfterRelease(_sinceRelease);
            _curve.Step(anchor, damping, damping, deltaTime);
        }

        public void StepHeld(Vector2 anchor, float distance, Vector2 holdPosition, float deltaTime) {
            _held = true;
            var index = _curve.PinIndex >= 0 ? _curve.PinIndex : ResolveGrabIndex(distance);
            _curve.SetPin(index, holdPosition, distance);
            _curve.Step(anchor, _damping.Held, _damping.HeldTail, deltaTime);
        }

        private int ResolveGrabIndex(float distance) {
            return _grabIndex >= 0 ? _grabIndex : _curve.IndexAt(distance);
        }
    }
}
