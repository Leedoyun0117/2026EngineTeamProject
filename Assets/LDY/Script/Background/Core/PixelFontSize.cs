using System;

namespace LDY.Background.Core {
    public static class PixelFontSize {
        public static int PixelSize(float rectHeightPixels, int basePixels) {
            if (basePixels < 1) throw new ArgumentException("basePixels는 1 이상이어야 한다");
            var multiple = Math.Max(1, (int)Math.Floor(rectHeightPixels / basePixels));
            return basePixels * multiple;
        }

        public static bool OverflowsHeight(float rectHeightPixels, int basePixels) =>
            PixelSize(rectHeightPixels, basePixels) > rectHeightPixels;
    }
}
