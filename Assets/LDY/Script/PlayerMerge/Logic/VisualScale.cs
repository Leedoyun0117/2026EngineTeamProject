using System;

namespace LDY.PlayerMerge {
    public readonly struct VisualScaleChoice {
        public VisualScaleChoice(float scale, bool usedDefault) {
            Scale = scale;
            UsedDefault = usedDefault;
        }

        public float Scale { get; }
        public bool UsedDefault { get; }
    }

    // 비주얼 자식 배율로 다른 PPU의 그림을 기준 그림과 같은 월드 크기로 맞춘다.
    public static class VisualScale {
        public const float HeightTolerance = 0.0001f;

        public static float Matching(float sourcePixelsPerUnit, float sourceScale, float targetPixelsPerUnit) =>
            targetPixelsPerUnit * sourceScale / sourcePixelsPerUnit;

        public static float WorldHeight(int pixels, float pixelsPerUnit, float scale) =>
            pixels / pixelsPerUnit * scale;

        public static float EffectivePixelsPerUnit(float pixelsPerUnit, float scale) =>
            pixelsPerUnit / scale;

        public static VisualScaleChoice Choose(float defaultScale, float referenceHeight, int pixels, float pixelsPerUnit) {
            if (Math.Abs(WorldHeight(pixels, pixelsPerUnit, defaultScale) - referenceHeight) < HeightTolerance)
                return new VisualScaleChoice(defaultScale, true);

            return new VisualScaleChoice(referenceHeight * pixelsPerUnit / pixels, false);
        }
    }
}
