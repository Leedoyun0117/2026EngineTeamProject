using System;

namespace LDY.Background.Core {
    public static class BackgroundDim {
        public const float Default = 0.35f;

        public static float Factor(float dim) => 1f - Math.Max(0f, Math.Min(1f, dim));

        public static Rgba Apply(Rgba color, float dim) {
            var f = Factor(dim);
            return new Rgba(color.R * f, color.G * f, color.B * f, color.A);
        }
    }
}
