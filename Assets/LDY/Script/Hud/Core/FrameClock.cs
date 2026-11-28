namespace LDY.Hud.Core {
    // 프레임 애니메이션 시계. 긴 프레임에도 건너뛴 만큼 정확히 넘어간다.
    public sealed class FrameClock {
        private readonly int _frameCount;
        private readonly float _frameSeconds;
        private float _elapsed;

        public FrameClock(int frameCount, float frameSeconds) {
            _frameCount = frameCount < 1 ? 1 : frameCount;
            _frameSeconds = frameSeconds;
        }

        public int Frame { get; private set; }

        public bool Tick(float deltaTime) {
            if (_frameCount == 1 || _frameSeconds <= 0f || deltaTime <= 0f)
                return false;

            _elapsed += deltaTime;
            if (_elapsed < _frameSeconds)
                return false;

            var steps = (int)(_elapsed / _frameSeconds);
            _elapsed -= steps * _frameSeconds;
            var next = (Frame + steps) % _frameCount;
            if (next == Frame)
                return false;

            Frame = next;
            return true;
        }

        public void Reset() {
            _elapsed = 0f;
            Frame = 0;
        }
    }
}
