namespace LDY.Script {
    // 잡은 직후의 놓기 입력을 무시하는 문지기. 같은 F 입력이 잡기와 놓기로 동시에 처리되는 것을 막는다.
    public sealed class ReleaseGate {
        private readonly float _minDelay;
        private int _grabFrame = int.MinValue;
        private float _grabTime = float.NegativeInfinity;

        public ReleaseGate(float minDelay) {
            _minDelay = minDelay < 0f ? 0f : minDelay;
        }

        // 잡은 프레임/시각 기록
        public void MarkGrab(int frame, float time) {
            _grabFrame = frame;
            _grabTime = time;
        }

        // 눌린 프레임이 잡은 프레임보다 뒤이고 최소 지연이 지났을 때만 허용
        public bool Accepts(int pressFrame, float now) {
            return pressFrame > _grabFrame && now - _grabTime >= _minDelay;
        }
    }
}
