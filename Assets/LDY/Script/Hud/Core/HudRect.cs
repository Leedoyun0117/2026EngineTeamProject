namespace LDY.Hud.Core {
    // 월드 좌표 경계. HUD 표시가 따라갈 대상의 영역.
    public readonly struct HudRect {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }

        public HudRect(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float CenterX => X + Width * 0.5f;
        public float CenterY => Y + Height * 0.5f;
        public float Top => Y + Height;

        public static HudRect FromCenter(float centerX, float centerY, float width, float height) =>
            new HudRect(centerX - width * 0.5f, centerY - height * 0.5f, width, height);
    }
}
