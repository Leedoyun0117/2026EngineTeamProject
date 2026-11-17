using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    // segmentCount: 점 개수, gravity: 줄 중력, iterations: 거리 제약 반복 횟수, holdBlendTime: 잡을 때 모양 전환 시간, sleepSpeed: 이보다 느리면 정지 처리, tailStiffness: 꼬리를 직선으로 펴는 정도(0~1)
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

    // 줄 모양: Verlet 점 N개 + 이웃 점 거리 제약. 시각용이라 플레이어 위치에는 영향이 없다.
    public sealed class RopeCurve {
        private const int MinPointCount = 3;
        private const float MinLength = 0.01f;
        // 잡은 거리 상한(총길이 대비). 꼬리가 항상 남게 한다.
        private const float MaxHeldRatio = 0.98f;

        private readonly float _blendTime;
        private readonly float _gravity;
        private readonly float _sleepSpeed;
        private readonly float _tailStiffness;
        private readonly int _iterations;
        // 현재 위치 / 이전 위치(Verlet은 둘의 차가 곧 속도) / 마디별 목표 길이
        private readonly Vector2[] _points;
        private readonly Vector2[] _previous;
        private readonly float[] _rest;
        // 0~1 잡힘 정도(잡을 때 올라가고 놓으면 내려감)
        private float _blend;
        // anchor~잡은 점 사이 줄 길이
        private float _heldDistance;
        // 플레이어가 잡은 점 번호(고정점, 없으면 -1)
        private int _pin = -1;
        // 발이 걸린 점 번호와 관련 값(없으면 -1)
        private int _foot = -1;
        private int _footSplit = -1;
        private float _footDistance;
        private Vector2 _footTarget;
        // 고정점이 가야 할 월드 위치
        private Vector2 _pinTarget;
        // 윗부분/꼬리를 나누는 점 번호
        private int _split = -1;

        // 점 배열과 수치를 준비한다.
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

        // _blend를 smoothstep으로 부드럽게 만든 가중치
        private float Weight => _blend * _blend * (3f - 2f * _blend);

        public float TotalLength { get; }
        public float SegmentLength { get; }
        public int PinIndex => _pin;
        public int FootIndex => _foot;
        public IReadOnlyList<Vector2> Points => _points;

        // 마디별 목표 길이의 합(총길이와 같아야 한다)
        public float CurrentLength {
            get {
                var sum = 0f;
                foreach (var length in _rest)
                    sum += length;
                return sum;
            }
        }

        // 점의 속도 = 현재 - 이전 위치
        public Vector2 VelocityOf(int index) {
            return _points[index] - _previous[index];
        }

        // 줄 위 거리에 가장 가까운 점 번호
        public int IndexAt(float distance) {
            return ClampPin(Mathf.RoundToInt(distance / SegmentLength));
        }

        // anchor에서 아래로 곧게 늘어진 상태로 초기화
        public void Initialize(Vector2 anchor) {
            _pin = -1;
            _foot = -1;
            _footSplit = -1;
            _split = -1;
            _blend = 0f;
            for (var i = 0; i < _points.Length; i++) {
                _points[i] = anchor + Vector2.down * (SegmentLength * i);
                _previous[i] = _points[i];
            }

            FillRest();
        }

        // 잡은 점을 플레이어 위치에 고정. 점 번호는 처음 한 번만 정해 오르내려도 안 튄다.
        public void SetPin(int index, Vector2 target, float distance) {
            index = ClampPin(index);
            if (index != _pin) {
                _pin = index;
                _split = index;
                _foot = -1;
                _footSplit = -1;
                _points[index] = target;
                _previous[index] = target;
            }

            _pinTarget = target;
            _heldDistance = Mathf.Clamp(distance, MinLength, TotalLength * MaxHeldRatio);
        }

        // 고정 해제. 점 위치/이전 위치는 그대로라 속도가 유지된다.
        public void ClearPin() {
            _pin = -1;
            _foot = -1;
        }

        // 잡은 점 아래의 점 하나를 발 위치에 고정
        public void SetFootPin(int index, Vector2 target, float distance) {
            if (_pin < 0 || _pin >= _points.Length - 2) {
                _foot = -1;
                return;
            }

            index = Mathf.Clamp(index, _pin + 1, _points.Length - 2);
            if (index != _foot) {
                _foot = index;
                _footSplit = index;
                _points[index] = target;
                _previous[index] = target;
            }

            _footTarget = target;
            _footDistance = Mathf.Max(distance, MinLength);
        }

        // 발 고정 해제
        public void ClearFootPin() {
            _foot = -1;
        }

        // 위치에서 가장 가까운 줄 위 점과 가까운 점 번호를 구한다.
        public Vector2 GetClosest(Vector2 position, out int index) {
            var best = float.PositiveInfinity;
            var closest = _points[0];
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

            return closest;
        }

        // 한 틱: 블렌드 갱신 -> 목표 길이 계산 -> 관성/중력 적분 -> 고정점 덮어쓰기 -> 거리 제약 반복 -> 직선화.
        public void Step(Vector2 anchor, float damping, float tailDamping, float deltaTime) {
            _blend = Mathf.MoveTowards(_blend, _pin >= 0 ? 1f : 0f, deltaTime / _blendTime);
            FillRest();
            Integrate(damping, tailDamping, deltaTime);

            if (_pin >= 0) {
                _previous[_pin] = _points[_pin];
                _points[_pin] = _pinTarget;
            }

            if (_foot >= 0) {
                _previous[_foot] = _points[_foot];
                _points[_foot] = _footTarget;
            }

            _points[0] = anchor;
            _previous[0] = anchor;

            for (var i = 0; i < _iterations; i++)
                Relax(i % 2 == 0);

            Tighten();
        }

        // anchor(0)와 끝점은 고정점이 될 수 없어 1~끝-1로 제한
        private int ClampPin(int index) {
            return Mathf.Clamp(index, 1, _points.Length - 2);
        }

        // 마디별 목표 길이 계산. 잡혔을 땐 윗부분/꼬리로 나눠 배분(합은 항상 총길이).
        private void FillRest() {
            for (var i = 0; i < _rest.Length; i++)
                _rest[i] = _split < 1 || _blend <= 0f ? SegmentLength : Mathf.Lerp(SegmentLength, HeldRest(i), Weight);
        }

        // 잡힌 상태의 마디 길이: 윗부분은 잡은 거리÷마디 수, 꼬리는 남은 길이÷마디 수
        private float HeldRest(int i) {
            if (i < _split)
                return _heldDistance / _split;

            if (_footSplit <= _split)
                return (TotalLength - _heldDistance) / (_rest.Length - _split);

            if (i < _footSplit)
                return _footDistance / (_footSplit - _split);

            return Mathf.Max((TotalLength - _heldDistance - _footDistance) / (_rest.Length - _footSplit), MinLength);
        }

        // Verlet 적분: 속도(현재-이전)에 감쇠를 곱하고 중력을 더해 이동. 너무 느리면 정지.
        private void Integrate(float damping, float tailDamping, float deltaTime) {
            var pull = new Vector2(0f, -_gravity * deltaTime * deltaTime);
            var sleepSqr = _sleepSpeed * _sleepSpeed;
            var tailKeep = Mathf.Max(0f, 1f - Mathf.Lerp(damping, tailDamping, Weight) * deltaTime);
            var keep = Mathf.Max(0f, 1f - damping * deltaTime);
            for (var i = 1; i < _points.Length; i++) {
                if (i == _pin || i == _foot)
                    continue;

                var current = _points[i];
                var velocity = (current - _previous[i]) * (_split >= 1 && i > _split ? tailKeep : keep);
                if (velocity.sqrMagnitude < sleepSqr)
                    velocity = Vector2.zero;

                _points[i] = current + velocity + pull;
                _previous[i] = current;
            }
        }

        // 윗부분/꼬리를 직선 쪽으로 당겨 줄이 덜 흔들리고 곧게 보이게 한다.
        private void Tighten() {
            if (_split < 1 || _blend <= 0f)
                return;

            Pull(1, _split, _points[0], _points[_split], Weight);
            var last = _points.Length - 1;
            if (_footSplit <= _split) {
                Pull(_split + 1, last, _points[_split], _points[last], Weight * _tailStiffness);
                return;
            }

            Pull(_split + 1, _footSplit, _points[_split], _points[_footSplit], Weight);
            Pull(_footSplit + 1, last, _points[_footSplit], _points[last], Weight * _tailStiffness);
        }

        // from~to 점을 start~end 직선 위 위치 쪽으로 weight만큼 당김(이전 위치도 같이 옮겨 속도 보존)
        private void Pull(int from, int to, Vector2 start, Vector2 end, float weight) {
            var count = to - from + 1;
            for (var i = from; i < to; i++) {
                var straight = Vector2.Lerp(start, end, (i - from + 1) / (float)count);
                var next = Vector2.Lerp(_points[i], straight, weight);
                _previous[i] += next - _points[i];
                _points[i] = next;
            }
        }

        // 거리 제약을 앞->뒤 / 뒤->앞 번갈아 적용해 한쪽으로 쏠리는 것을 줄임
        private void Relax(bool forward) {
            var last = _points.Length - 2;
            for (var k = 0; k <= last; k++)
                Constrain(forward ? k : last - k);
        }

        // 이웃한 두 점이 목표 길이를 유지하도록 서로 당기고 밀기(고정점은 안 움직임)
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

        // 움직이지 않는 점: anchor, 잡은 점, 발 점
        private bool IsFixed(int index) {
            return index == 0 || index == _pin || index == _foot;
        }
    }
}
