namespace LDY.StageSelect.Core {
    // 캔버스 기준 픽셀 사각형. 원점은 부모 왼쪽 위, y는 아래로 늘어난다(UiFactory.Place와 같은 좌표).
    public readonly struct PixelRect {
        public PixelRect(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float Right => X + Width;
        public float Bottom => Y + Height;

        public PixelRect Offset(float dx, float dy) => new PixelRect(X + dx, Y + dy, Width, Height);
    }
}
