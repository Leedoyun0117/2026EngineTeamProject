using System;

namespace LDY.Spider {
    public static class Easing {
        public static float Clamp01(float t) => Math.Clamp(t, 0f, 1f);

        public static float Smooth(float t) {
            t = Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        public static float EaseOut(float t) {
            var inverse = 1f - Clamp01(t);
            return 1f - inverse * inverse * inverse;
        }

        // 0에서 시작해 peak 지점에서 1까지 빠르게 올랐다가 부드럽게 0으로 돌아오는 한 번의 펄스.
        public static float Pulse(float t, float peak = 0.3f) {
            t = Clamp01(t);
            return t < peak ? EaseOut(t / peak) : 1f - Smooth((t - peak) / (1f - peak));
        }

        public static float EaseIn(float t) {
            t = Clamp01(t);
            return t * t * t;
        }
    }
}
