using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    [Serializable]
    public struct RopeCurveTuning {
        public int segmentCount;
        [HideInInspector] public float totalLength;
        public float gravity;
        public int iterations;
        public float holdBlendTime;
        public float sleepSpeed;
        public float tailStiffness;
    }

    public sealed class RopeCurve {
        private const int MinPointCount = 3;
        private const float MinLength = 0.01f;
        private const float MaxHeldRatio = 0.98f;

        private readonly float _blendTime;
        private readonly float _gravity;
        private readonly float _sleepSpeed;
        private readonly float _tailStiffness;
        private readonly int _iterations;
        private readonly Vector2[] _points;
        private readonly Vector2[] _previous;
        private readonly float[] _rest;
        private float _blend;
        private float _heldDistance;
        private int _pin = -1;
        private Vector2 _pinTarget;
        private int _split = -1;

        public RopeCurve(RopeCurveTuning tuning) {
            var count = Mathf.Max(tuning.segmentCount, MinPointCount);
            _points = new Vector2[count];
            _previous = new Vector2[count];
            _rest = new float[count - 1];
            _gravity = tuning.gravity;
            _iterations = Mathf.Max(tuning.iterations, 1);
            _blendTime = Mathf.Max(tuning.holdBlendTime, MinLength);
            _sleepSpeed = Mathf.Max(tuning.sleepSpeed, 0f);
            _tailStiffness = Mathf.Clamp01(tuning.tailStiffness);
            TotalLength = Mathf.Max(tuning.totalLength, MinLength);
            SegmentLength = TotalLength / (count - 1);
            FillRest();
        }

        private float Weight => _blend * _blend * (3f - 2f * _blend);

        public float TotalLength { get; }
        public float SegmentLength { get; }
        public int PinIndex => _pin;
        public IReadOnlyList<Vector2> Points => _points;

        public float CurrentLength {
            get {
                var sum = 0f;
                foreach (var length in _rest)
                    sum += length;
                return sum;
            }
        }

        public Vector2 VelocityOf(int index) {
            return _points[index] - _previous[index];
        }

        public int IndexAt(float distance) {
            return ClampPin(Mathf.RoundToInt(distance / SegmentLength));
        }

        public void Initialize(Vector2 anchor) {
            _pin = -1;
            _split = -1;
            _blend = 0f;
            for (var i = 0; i < _points.Length; i++) {
                _points[i] = anchor + Vector2.down * (SegmentLength * i);
                _previous[i] = _points[i];
            }

            FillRest();
        }

        public void SetPin(int index, Vector2 target, float distance) {
            index = ClampPin(index);
            if (index != _pin) {
                _pin = index;
                _split = index;
                _points[index] = target;
                _previous[index] = target;
            }

            _pinTarget = target;
            _heldDistance = Mathf.Clamp(distance, MinLength, TotalLength * MaxHeldRatio);
        }

        public void ClearPin() {
            _pin = -1;
        }

        public bool TryGetClosest(Vector2 position, float range, out Vector2 closest, out int index) {
            var best = float.PositiveInfinity;
            closest = _points[0];
            index = 1;
            for (var i = 0; i < _points.Length - 1; i++) {
                var candidate = RopeReach.ClosestPoint(_points[i], _points[i + 1], position);
                var distance = (candidate - position).sqrMagnitude;
                if (distance >= best)
                    continue;

                best = distance;
                closest = candidate;
                var along = Vector2.Distance(_points[i], candidate);
                var span = Vector2.Distance(_points[i], _points[i + 1]);
                index = ClampPin(span > Mathf.Epsilon && along / span >= 0.5f ? i + 1 : i);
            }

            return best <= range * range;
        }

        public void Step(Vector2 anchor, float damping, float tailDamping, float deltaTime) {
            _blend = Mathf.MoveTowards(_blend, _pin >= 0 ? 1f : 0f, deltaTime / _blendTime);
            FillRest();
            Integrate(damping, tailDamping, deltaTime);

            if (_pin >= 0) {
                _previous[_pin] = _points[_pin];
                _points[_pin] = _pinTarget;
            }

            _points[0] = anchor;
            _previous[0] = anchor;

            for (var i = 0; i < _iterations; i++)
                Relax(i % 2 == 0);

            Tighten();
        }

        private int ClampPin(int index) {
            return Mathf.Clamp(index, 1, _points.Length - 2);
        }

        private void FillRest() {
            var upperCount = _split >= 1 ? _split : 1;
            var tailCount = _rest.Length - upperCount;
            for (var i = 0; i < _rest.Length; i++) {
                if (_split < 1 || _blend <= 0f) {
                    _rest[i] = SegmentLength;
                    continue;
                }

                var held = i < upperCount
                    ? _heldDistance / upperCount
                    : (TotalLength - _heldDistance) / tailCount;
                _rest[i] = Mathf.Lerp(SegmentLength, held, Weight);
            }
        }

        private void Integrate(float damping, float tailDamping, float deltaTime) {
            var pull = new Vector2(0f, -_gravity * deltaTime * deltaTime);
            var sleepSqr = _sleepSpeed * _sleepSpeed;
            var tailKeep = Mathf.Max(0f, 1f - Mathf.Lerp(damping, tailDamping, Weight) * deltaTime);
            var keep = Mathf.Max(0f, 1f - damping * deltaTime);
            for (var i = 1; i < _points.Length; i++) {
                if (i == _pin)
                    continue;

                var current = _points[i];
                var velocity = (current - _previous[i]) * (_split >= 1 && i > _split ? tailKeep : keep);
                if (velocity.sqrMagnitude < sleepSqr)
                    velocity = Vector2.zero;

                _points[i] = current + velocity + pull;
                _previous[i] = current;
            }
        }

        private void Tighten() {
            if (_split < 1 || _blend <= 0f)
                return;

            Pull(1, _split, _points[0], _points[_split], Weight);
            Pull(_split + 1, _points.Length - 1, _points[_split], _points[_points.Length - 1], Weight * _tailStiffness);
        }

        private void Pull(int from, int to, Vector2 start, Vector2 end, float weight) {
            var count = to - from + 1;
            for (var i = from; i < to; i++) {
                var straight = Vector2.Lerp(start, end, (i - from + 1) / (float)count);
                var next = Vector2.Lerp(_points[i], straight, weight);
                _previous[i] += next - _points[i];
                _points[i] = next;
            }
        }

        private void Relax(bool forward) {
            var last = _points.Length - 2;
            for (var k = 0; k <= last; k++)
                Constrain(forward ? k : last - k);
        }

        private void Constrain(int a) {
            var b = a + 1;
            var delta = _points[b] - _points[a];
            var distance = delta.magnitude;
            if (distance < Mathf.Epsilon)
                return;

            var weightA = IsFixed(a) ? 0f : 1f;
            var weightB = IsFixed(b) ? 0f : 1f;
            var total = weightA + weightB;
            if (total <= 0f)
                return;

            var correction = delta * ((distance - _rest[a]) / distance / total);
            _points[a] += correction * weightA;
            _points[b] -= correction * weightB;
        }

        private bool IsFixed(int index) {
            return index == 0 || index == _pin;
        }
    }
}
