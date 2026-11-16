namespace LDY.Script {
    public sealed class ReleaseGate {
        private readonly float _minDelay;
        private int _grabFrame = int.MinValue;
        private float _grabTime = float.NegativeInfinity;

        public ReleaseGate(float minDelay) {
            _minDelay = minDelay < 0f ? 0f : minDelay;
        }

        public void MarkGrab(int frame, float time) {
            _grabFrame = frame;
            _grabTime = time;
        }

        public bool Accepts(int pressFrame, float now) {
            return pressFrame > _grabFrame && now - _grabTime >= _minDelay;
        }
    }
}
