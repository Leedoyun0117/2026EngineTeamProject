using System;
using System.Collections.Generic;

namespace LDY.Spider {
    [Serializable]
    public struct LegReach {
        public float Min;
        public float Max;

        public LegReach(float min, float max) {
            Min = min;
            Max = max;
        }
    }

    public readonly struct CombatZone {
        public readonly bool HasBounds;
        public readonly float Min;
        public readonly float Max;

        public CombatZone(float min, float max) {
            HasBounds = true;
            Min = Math.Min(min, max);
            Max = Math.Max(min, max);
        }

        public float Clamp(float x) => HasBounds ? Math.Clamp(x, Min, Max) : x;
    }

    // 다리는 자기 쪽(몸 중심 기준 같은 방향)의 목표만, 다리별 최소/최대 반경 안에서만 노린다.
    public static class AttackRangeRule {
        public static int Pick(float targetX, float bodyX, IReadOnlyList<float> legX, IReadOnlyList<bool> alive,
            IReadOnlyList<LegReach> reach) {
            var best = -1;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < legX.Count; i++) {
                if (!alive[i] || !Reaches(targetX, bodyX, SideOf(legX[i], bodyX), reach[i]))
                    continue;

                var distance = MathF.Abs(legX[i] - targetX);
                if (distance >= bestDistance)
                    continue;

                best = i;
                bestDistance = distance;
            }

            return best;
        }

        public static bool Reaches(float targetX, float bodyX, int side, LegReach reach) {
            var distance = (targetX - bodyX) * side;
            return distance >= reach.Min && distance <= reach.Max;
        }

        public static int SideOf(float legX, float bodyX) => legX < bodyX ? -1 : 1;

        // 목표가 한쪽 앞다리 사정거리 가운데에 오도록 서야 할 몸 x. 살아 있는 다리가 없는 쪽이면 반대쪽에서 접근한다.
        public static float StandoffBodyX(float targetX, float bodyX, IReadOnlyList<float> legX,
            IReadOnlyList<bool> alive, float standoff, CombatZone zone) {
            var side = targetX >= bodyX ? 1 : -1;
            if (!HasLiveLeg(side, bodyX, legX, alive))
                side = -side;

            return zone.Clamp(targetX - side * standoff);
        }

        private static bool HasLiveLeg(int side, float bodyX, IReadOnlyList<float> legX, IReadOnlyList<bool> alive) {
            for (var i = 0; i < legX.Count; i++)
                if (alive[i] && SideOf(legX[i], bodyX) == side)
                    return true;

            return false;
        }
    }
}
