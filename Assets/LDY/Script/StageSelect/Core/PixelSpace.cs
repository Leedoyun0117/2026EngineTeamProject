using System;

namespace LDY.StageSelect.Core {
    // 캔버스 기준 픽셀 사각형. 원점은 캔버스 왼쪽 위, y는 아래로 늘어난다(UiFactory.Place와 같은 좌표).
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

    public readonly struct WorldPoint {
        public WorldPoint(float x, float y) {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
    }

    // 월드 사각형. y는 위로 늘어난다.
    public readonly struct WorldBox {
        public WorldBox(float left, float bottom, float right, float top) {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        public float Left { get; }
        public float Bottom { get; }
        public float Right { get; }
        public float Top { get; }
        public float Width => Right - Left;
        public float Height => Top - Bottom;
        public float CenterX => (Left + Right) * 0.5f;
        public float CenterY => (Bottom + Top) * 0.5f;
    }

    // 캔버스 픽셀 ↔ 월드. 카메라 가운데 = 캔버스 가운데, 캔버스 1px = 1/PixelsPerUnit 유닛.
    // Screen Space - Camera 캔버스는 카메라 화면을 꽉 채우므로 카메라 orthographicSize를 OrthographicSize로 맞추면 둘이 겹친다.
    public readonly struct PixelSpace {
        public PixelSpace(float canvasWidth, float canvasHeight, float pixelsPerUnit, float centerX, float centerY) {
            if (pixelsPerUnit <= 0f) throw new ArgumentOutOfRangeException(nameof(pixelsPerUnit));
            CanvasWidth = canvasWidth;
            CanvasHeight = canvasHeight;
            PixelsPerUnit = pixelsPerUnit;
            CenterX = centerX;
            CenterY = centerY;
        }

        public float CanvasWidth { get; }
        public float CanvasHeight { get; }
        public float PixelsPerUnit { get; }
        public float CenterX { get; }
        public float CenterY { get; }
        public float OrthographicSize => CanvasHeight * 0.5f / PixelsPerUnit;

        // 픽셀 좌표는 정수로 맞춘 뒤 바꾼다. 발판 모서리가 화면 픽셀 경계에 떨어진다.
        public WorldPoint ToWorld(float x, float y) =>
            new WorldPoint(CenterX + (Snap(x) - CanvasWidth * 0.5f) / PixelsPerUnit,
                CenterY + (CanvasHeight * 0.5f - Snap(y)) / PixelsPerUnit);

        public WorldBox ToWorld(PixelRect rect) {
            var topLeft = ToWorld(rect.X, rect.Y);
            var bottomRight = ToWorld(rect.Right, rect.Bottom);
            return new WorldBox(topLeft.X, bottomRight.Y, bottomRight.X, topLeft.Y);
        }

        public float ToUnits(float pixels) => pixels / PixelsPerUnit;

        // 0.5는 위로 올린다(음수 포함 같은 방향). Math.Round의 은행가 반올림을 피한다.
        public static float Snap(float value) => (float)Math.Floor(value + 0.5f);
    }
}
