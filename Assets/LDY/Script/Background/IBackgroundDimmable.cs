using LDY.Background.Core;
using UnityEngine;

namespace LDY.Background {
    public interface IBackgroundDimmable {
        void ApplyDim(float dim);
    }

    public static class DimColor {
        public static Color Of(Color color, float dim) {
            var f = BackgroundDim.Factor(dim);
            return new Color(color.r * f, color.g * f, color.b * f, color.a);
        }
    }
}
