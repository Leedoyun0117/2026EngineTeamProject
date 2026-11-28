using System;

namespace LDY.Hud.Core {
    // 픽셀 격자에 맞춘다. step은 한 픽셀의 크기(캔버스 1, 월드 1/PPU).
    public static class PixelSnap {
        public static float Snap(float value, float step = 1f) {
            if (step <= 0f)
                return value;

            return (float)(Math.Round(value / step, MidpointRounding.AwayFromZero) * step);
        }
    }
}
