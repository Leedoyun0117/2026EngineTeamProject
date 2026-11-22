using System;
using System.Collections.Generic;

namespace LDY.Spider {
    public static class LegPicker {
        public static int Pick(float targetX, IReadOnlyList<float> legX, IReadOnlyList<bool> alive) {
            var best = -1;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < legX.Count; i++) {
                if (!alive[i])
                    continue;

                var distance = MathF.Abs(legX[i] - targetX);
                if (distance >= bestDistance)
                    continue;

                best = i;
                bestDistance = distance;
            }

            return best;
        }
    }
}
