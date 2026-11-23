using System;
using System.Collections.Generic;

namespace LDY.Spider {
    public enum BodyFace {
        Idle,
        Glitch,
        HurtA,
        HurtB
    }

    public static class BodyFaceRule {
        public static BodyFace Pick(SpiderState state, float stateElapsed, bool idleGlitch, bool fleeGlitch,
            float hurtFrameTime) {
            switch (state) {
                case SpiderState.Hurt:
                    var frame = hurtFrameTime <= 0f ? 0 : (int)(stateElapsed / hurtFrameTime);
                    return frame % 2 == 0 ? BodyFace.HurtA : BodyFace.HurtB;
                case SpiderState.Flee:
                case SpiderState.Gone:
                    return fleeGlitch ? BodyFace.Glitch : BodyFace.Idle;
                default:
                    return idleGlitch ? BodyFace.Glitch : BodyFace.Idle;
            }
        }
    }

    // 남은 다리가 소켓을 축으로 흔들리는 각도. 시작과 끝에서 0이라 튀지 않는다.
    public static class LegFlail {
        public static float Degrees(float progress, float time, float amplitude, float hertz, float phase) {
            var envelope = MathF.Sin(MathF.PI * Easing.Clamp01(progress));
            return amplitude * envelope * MathF.Sin(time * hertz * MathF.PI * 2f + phase);
        }

        public static Vec2 Rotate(Vec2 tip, Vec2 socket, float degrees) {
            var radians = degrees * MathF.PI / 180f;
            var offset = tip - socket;
            var cos = MathF.Cos(radians);
            var sin = MathF.Sin(radians);
            return socket + new Vec2(offset.X * cos - offset.Y * sin, offset.X * sin + offset.Y * cos);
        }
    }

    public static class LegTuck {
        public static Vec2 Tip(Vec2 planted, Vec2 tuckPoint, float progress) {
            return Vec2.Lerp(planted, tuckPoint, Easing.Smooth(progress));
        }

        public static float Progress(float fleeProgress, float tuckShare) {
            return tuckShare <= 0f ? 1f : Easing.Clamp01(fleeProgress / tuckShare);
        }
    }

    // 다리가 잘린 뒤 남은 다리로 서도록, 살아 있는 다리의 기준 x를 원래 폭 안에 고르게 다시 펼친다.
    public static class StanceLayout {
        public static void Spread(IReadOnlyList<float> original, IReadOnlyList<bool> alive, float[] result) {
            var liveCount = 0;
            var min = float.MaxValue;
            var max = float.MinValue;
            for (var i = 0; i < original.Count; i++) {
                min = Math.Min(min, original[i]);
                max = Math.Max(max, original[i]);
                if (alive[i])
                    liveCount++;
            }

            if (liveCount == original.Count) {
                for (var i = 0; i < original.Count; i++)
                    result[i] = original[i];
                return;
            }

            var order = 0;
            for (var i = 0; i < original.Count; i++) {
                result[i] = original[i];
                if (!alive[i])
                    continue;

                result[i] = liveCount <= 1 ? (min + max) * 0.5f : min + (max - min) * order / (liveCount - 1);
                order++;
            }
        }
    }
}
