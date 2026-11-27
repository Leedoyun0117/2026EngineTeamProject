using System;

namespace LDY.PlayerMerge {
    public readonly struct Float2 {
        public Float2(float x, float y) {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }

        public override string ToString() => $"({X:0.####}, {Y:0.####})";
    }

    // 루트 스케일이 다른 프리팹 사이에서 로컬 값을 옮길 때 월드 크기를 유지한다.
    public static class RootScaleConversion {
        public const float WorldTolerance = 0.01f;

        public static Float2 Convert(Float2 local, float fromRootScale, float toRootScale) =>
            new Float2(local.X * fromRootScale / toRootScale, local.Y * fromRootScale / toRootScale);

        public static Float2 ToWorld(Float2 local, float rootScale) =>
            new Float2(local.X * rootScale, local.Y * rootScale);

        public static bool SameWorld(Float2 a, Float2 b) =>
            Math.Abs(a.X - b.X) < WorldTolerance && Math.Abs(a.Y - b.Y) < WorldTolerance;
    }
}
