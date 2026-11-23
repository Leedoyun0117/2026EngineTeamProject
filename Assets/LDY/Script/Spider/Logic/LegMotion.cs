using System;

namespace LDY.Spider {
    public static class LegMotion {
        public static Vec2 Travel(Vec2 from, Vec2 to, float progress) => Vec2.Lerp(from, to, Easing.Smooth(progress));

        public static Vec2 Arc(Vec2 from, Vec2 to, float progress, float height) {
            var t = Easing.Clamp01(progress);
            var lift = height * MathF.Sin(MathF.PI * t);
            return Vec2.Lerp(from, to, Easing.Smooth(t)) + new Vec2(0f, lift);
        }

        public static Vec2 Tremor(float time, float amplitude, float hertz) {
            var phase = time * hertz * MathF.PI * 2f;
            return new Vec2(MathF.Sin(phase) * amplitude, MathF.Sin(phase * 1.7f + 1f) * amplitude * 0.5f);
        }
    }
}
