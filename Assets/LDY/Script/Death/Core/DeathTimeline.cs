using System;
using System.Collections.Generic;

namespace LDY.Death {
    // 클립을 순서대로 넘기고, 마지막 클립이 끝난 뒤 지연이 지나면 재시작 시점을 한 번만 알린다.
    public sealed class DeathTimeline {
        private readonly float[] _ends;
        private readonly float _restartAt;
        private float _elapsed;
        private bool _done;

        public DeathTimeline(IReadOnlyList<float> clipLengths, float restartDelay) {
            _ends = new float[clipLengths.Count];
            var total = 0f;
            for (var i = 0; i < _ends.Length; i++) {
                total += Math.Max(0f, clipLengths[i]);
                _ends[i] = total;
            }

            _restartAt = total + Math.Max(0f, restartDelay);
            ClipIndex = IndexAt(0f);
        }

        public int ClipIndex { get; private set; }
        public bool ClipChanged { get; private set; }
        public bool RestartDue { get; private set; }

        public void Tick(float deltaTime) {
            ClipChanged = false;
            RestartDue = false;
            if (_done)
                return;

            _elapsed += Math.Max(0f, deltaTime);
            var index = IndexAt(_elapsed);
            if (index != ClipIndex) {
                ClipIndex = index;
                ClipChanged = true;
            }

            if (_elapsed < _restartAt)
                return;

            RestartDue = true;
            _done = true;
        }

        // 끝난 클립은 건너뛰고, 모두 끝났으면 마지막 클립에 머문다.
        private int IndexAt(float time) {
            for (var i = 0; i < _ends.Length; i++)
                if (time < _ends[i])
                    return i;

            return _ends.Length - 1;
        }
    }
}
