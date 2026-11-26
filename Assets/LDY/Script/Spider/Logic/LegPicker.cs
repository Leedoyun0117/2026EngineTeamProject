using System;
using System.Collections.Generic;

namespace LDY.Spider {
    public static class LegPicker {
        private const float TieEpsilon = 1e-4f;

        // 목표에 가장 가까운 살아 있는 다리. 거리가 같으면 몸 중심(다리 평균)에 더 가까운 안쪽 다리.
        public static int Pick(float targetX, IReadOnlyList<float> legX, IReadOnlyList<bool> alive) {
            var center = Center(legX);
            var best = -1;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < legX.Count; i++) {
                if (!alive[i])
                    continue;

                var distance = MathF.Abs(legX[i] - targetX);
                var tie = MathF.Abs(distance - bestDistance) <= TieEpsilon;
                if (tie ? MathF.Abs(legX[i] - center) >= MathF.Abs(legX[best] - center) : distance > bestDistance)
                    continue;

                best = i;
                bestDistance = distance;
            }

            return best;
        }

        private static float Center(IReadOnlyList<float> legX) {
            var sum = 0f;
            for (var i = 0; i < legX.Count; i++) sum += legX[i];
            return legX.Count == 0 ? 0f : sum / legX.Count;
        }
    }
}
