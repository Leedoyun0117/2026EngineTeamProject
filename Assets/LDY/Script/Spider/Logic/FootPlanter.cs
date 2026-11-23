using System;
using System.Collections.Generic;

namespace LDY.Spider {
    public enum LegPhase {
        Planted,
        Stepping,
        Busy,
        Cut
    }

    // 서 있는 발은 월드에 고정되고, 기준 위치에서 멀어진 다리만 호를 그리며 옮겨 간다.
    // 다리는 짝수/홀수 두 묶음으로 나뉘며, 한 묶음에서는 한 번에 한 다리만 들린다.
    public sealed class FootPlanter {
        private readonly float _threshold;
        private readonly float _stepTime;
        private readonly float _stepHeight;
        private readonly Vec2[] _planted;
        private readonly Vec2[] _current;
        private readonly Vec2[] _from;
        private readonly Vec2[] _to;
        private readonly float[] _timer;
        private readonly LegPhase[] _phase;
        private readonly float[] _overshoot;

        public FootPlanter(int legCount, float threshold, float stepTime, float stepHeight) {
            _threshold = threshold;
            _stepTime = stepTime;
            _stepHeight = stepHeight;
            _planted = new Vec2[legCount];
            _current = new Vec2[legCount];
            _from = new Vec2[legCount];
            _to = new Vec2[legCount];
            _timer = new float[legCount];
            _phase = new LegPhase[legCount];
            _overshoot = new float[legCount];
        }

        public IReadOnlyList<LegPhase> Phases => _phase;

        public LegPhase PhaseOf(int leg) => _phase[leg];
        public Vec2 Planted(int leg) => _planted[leg];
        public Vec2 Current(int leg) => _current[leg];

        public void Initialize(IReadOnlyList<Vec2> feet) {
            for (var i = 0; i < _planted.Length; i++) {
                _planted[i] = feet[i];
                _current[i] = feet[i];
                _phase[i] = LegPhase.Planted;
            }
        }

        public void SetBusy(int leg) {
            for (var i = 0; i < _phase.Length; i++) {
                if (i == leg && _phase[i] != LegPhase.Cut && _phase[i] != LegPhase.Busy) {
                    _planted[i] = _current[i];
                    _phase[i] = LegPhase.Busy;
                }
                else if (i != leg && _phase[i] == LegPhase.Busy) {
                    _phase[i] = LegPhase.Planted;
                }
            }
        }

        public void Cut(int leg) => _phase[leg] = LegPhase.Cut;

        public void Step(float deltaTime, IReadOnlyList<Vec2> desired) {
            for (var i = 0; i < _phase.Length; i++) {
                if (_phase[i] == LegPhase.Stepping)
                    Advance(i, deltaTime);
                else if (_phase[i] == LegPhase.Planted)
                    _current[i] = _planted[i];

                _overshoot[i] = (_planted[i] - desired[i]).Length;
            }

            var next = PickStepper(_overshoot, _phase, _threshold);
            if (next < 0)
                return;

            _from[next] = _planted[next];
            _to[next] = desired[next];
            _timer[next] = 0f;
            _phase[next] = LegPhase.Stepping;
        }

        public static int PickStepper(IReadOnlyList<float> overshoot, IReadOnlyList<LegPhase> phases, float threshold) {
            var best = -1;
            var bestOvershoot = threshold;
            for (var i = 0; i < phases.Count; i++) {
                if (phases[i] != LegPhase.Planted || overshoot[i] <= bestOvershoot || GroupIsLifted(phases, i))
                    continue;

                best = i;
                bestOvershoot = overshoot[i];
            }

            return best;
        }

        private static bool GroupIsLifted(IReadOnlyList<LegPhase> phases, int leg) {
            for (var i = leg % 2; i < phases.Count; i += 2)
                if (phases[i] == LegPhase.Stepping || phases[i] == LegPhase.Busy)
                    return true;

            return false;
        }

        private void Advance(int leg, float deltaTime) {
            _timer[leg] += deltaTime;
            var t = _stepTime <= 0f ? 1f : _timer[leg] / _stepTime;
            if (t >= 1f) {
                _planted[leg] = _to[leg];
                _current[leg] = _to[leg];
                _phase[leg] = LegPhase.Planted;
                return;
            }

            _current[leg] = LegMotion.Arc(_from[leg], _to[leg], t, _stepHeight);
        }
    }
}
