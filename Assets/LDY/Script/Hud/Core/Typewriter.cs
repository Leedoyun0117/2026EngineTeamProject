namespace LDY.Hud.Core {
    // 초당 글자 수만큼 앞에서부터 드러낸다. 프레임이 길어도 글자 수를 넘지 않는다.
    public sealed class Typewriter {
        private readonly float _charactersPerSecond;
        private string _text = string.Empty;
        private float _elapsed;

        public Typewriter(float charactersPerSecond) {
            _charactersPerSecond = charactersPerSecond;
        }

        public int VisibleCount { get; private set; }
        public string Text => _text;
        public string Visible => _text.Substring(0, VisibleCount);
        public bool IsDone => VisibleCount >= _text.Length;

        public void Start(string text) {
            _text = text ?? string.Empty;
            _elapsed = 0f;
            VisibleCount = _charactersPerSecond > 0f ? 0 : _text.Length;
        }

        public bool Tick(float deltaTime) {
            if (IsDone)
                return false;

            _elapsed += deltaTime < 0f ? 0f : deltaTime;
            var count = (int)(_elapsed * _charactersPerSecond);
            if (count > _text.Length)
                count = _text.Length;
            if (count == VisibleCount)
                return false;

            VisibleCount = count;
            return true;
        }

        public void Complete() {
            VisibleCount = _text.Length;
        }
    }
}
