using System;

namespace LDY.Background.Core {
    public readonly struct PixelRect {
        public readonly float X;
        public readonly float Y;
        public readonly float Width;
        public readonly float Height;

        public PixelRect(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float CenterX => X + Width * 0.5f;
        public float CenterY => Y + Height * 0.5f;
    }

    public readonly struct LocalPoint {
        public readonly float X;
        public readonly float Y;

        public LocalPoint(float x, float y) {
            X = x;
            Y = y;
        }
    }

    public readonly struct PixelSpace {
        public readonly float CanvasWidth;
        public readonly float CanvasHeight;
        public readonly float PixelsPerUnit;

        public PixelSpace(float canvasWidth, float canvasHeight, float pixelsPerUnit) {
            if (canvasWidth <= 0f || canvasHeight <= 0f) throw new ArgumentException("canvas 크기는 0보다 커야 한다");
            if (pixelsPerUnit <= 0f) throw new ArgumentException("pixelsPerUnit은 0보다 커야 한다");
            CanvasWidth = canvasWidth;
            CanvasHeight = canvasHeight;
            PixelsPerUnit = pixelsPerUnit;
        }

        public LocalPoint ToLocal(float px, float py) =>
            new LocalPoint((px - CanvasWidth * 0.5f) / PixelsPerUnit, (CanvasHeight * 0.5f - py) / PixelsPerUnit);

        public LocalPoint CenterOf(PixelRect rect) => ToLocal(rect.CenterX, rect.CenterY);

        public LocalPoint SizeOf(PixelRect rect) => new LocalPoint(ToUnits(rect.Width), ToUnits(rect.Height));

        public float ToUnits(float pixels) => pixels / PixelsPerUnit;
    }
}
