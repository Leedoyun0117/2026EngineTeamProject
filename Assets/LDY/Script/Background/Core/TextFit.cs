using System;

namespace LDY.Background.Core {
    public static class TextFit {
        public const int PixelSize = 10;

        public static bool Wraps(PixelRect rect) => rect.Height >= PixelSize * 2;

        public static PixelRect Expand(PixelRect rect, TextAlign align, float neededWidth) {
            var height = Math.Max(rect.Height, PixelSize);
            var y = rect.CenterY - height * 0.5f;
            var width = Math.Max(rect.Width, (float)Math.Ceiling(neededWidth));
            var grow = width - rect.Width;
            var x = align == TextAlign.Left ? rect.X : align == TextAlign.Right ? rect.X - grow : rect.X - grow * 0.5f;
            return new PixelRect(x, y, width, height);
        }

        public static bool Same(PixelRect a, PixelRect b) =>
            a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
    }
}
