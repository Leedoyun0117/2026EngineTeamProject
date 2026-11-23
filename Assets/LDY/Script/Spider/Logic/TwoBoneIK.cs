using System;

namespace LDY.Spider {
    public readonly struct IKSolution {
        public readonly float UpperAngle;
        public readonly float LowerAngle;
        public readonly Vec2 Knee;
        public readonly Vec2 Tip;
        public readonly bool Clamped;

        public IKSolution(float upperAngle, float lowerAngle, Vec2 knee, Vec2 tip, bool clamped) {
            UpperAngle = upperAngle;
            LowerAngle = lowerAngle;
            Knee = knee;
            Tip = tip;
            Clamped = clamped;
        }
    }

    // 각도는 월드 기준 라디안(반시계가 +). bendSign +1은 무릎이 목표 방향의 반시계 쪽, -1은 시계 쪽.
    public static class TwoBoneIK {
        private const float MinReach = 1e-4f;
        private const float ClampTolerance = 1e-5f;

        public static IKSolution Solve(Vec2 root, float upperLength, float lowerLength, Vec2 target, float bendSign) {
            var toTarget = target - root;
            var distance = toTarget.Length;
            var direction = distance > MinReach ? toTarget * (1f / distance) : new Vec2(0f, -1f);

            var minReach = MathF.Max(MathF.Abs(upperLength - lowerLength), MinReach);
            var maxReach = upperLength + lowerLength;
            var reach = Math.Clamp(distance, minReach, maxReach);

            var cosKnee = (upperLength * upperLength + reach * reach - lowerLength * lowerLength) /
                          (2f * upperLength * reach);
            var offset = MathF.Acos(Math.Clamp(cosKnee, -1f, 1f));
            var bend = bendSign >= 0f ? 1f : -1f;

            var upperAngle = MathF.Atan2(direction.Y, direction.X) + bend * offset;
            var knee = root + Vec2.FromAngle(upperAngle) * upperLength;
            var tip = root + direction * reach;
            var lowerAngle = MathF.Atan2(tip.Y - knee.Y, tip.X - knee.X);
            return new IKSolution(upperAngle, lowerAngle, knee, tip, MathF.Abs(reach - distance) > ClampTolerance);
        }

        public static Vec2 Forward(Vec2 root, float upperLength, float lowerLength, float upperAngle, float lowerAngle) {
            return root + Vec2.FromAngle(upperAngle) * upperLength + Vec2.FromAngle(lowerAngle) * lowerLength;
        }
    }
}
