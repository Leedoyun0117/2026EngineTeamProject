using System;

namespace LDY.Background.Core {
    public sealed class FrameClock {
        private readonly int _frames;
        private readonly double _frameSeconds;
        private double _remainder;

        public FrameClock(int frames, double frameSeconds) {
            if (frames < 1) throw new ArgumentException("frames는 1 이상이어야 한다");
            if (frameSeconds <= 0d) throw new ArgumentException("frameSeconds는 0보다 커야 한다");
            _frames = frames;
            _frameSeconds = frameSeconds;
        }

        public long Ticks { get; private set; }
        public int Index => (int)(Ticks % _frames);

        public bool Advance(double deltaSeconds) {
            if (deltaSeconds <= 0d) return false;
            _remainder += deltaSeconds;
            var steps = (long)(_remainder / _frameSeconds);
            if (steps == 0) return false;
            _remainder -= steps * _frameSeconds;
            Ticks += steps;
            return true;
        }
    }
}
