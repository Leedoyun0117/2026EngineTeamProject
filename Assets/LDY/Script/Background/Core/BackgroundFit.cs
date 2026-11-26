using System;

namespace LDY.Background.Core {
    public static class BackgroundFit {
        public const double DesignWidth = 16d;
        public const double DesignHeight = 9d;

        public static double CoverScale(double orthographicSize, double aspect) {
            if (orthographicSize <= 0d) throw new ArgumentException("orthographicSize는 0보다 커야 한다");
            if (aspect <= 0d) throw new ArgumentException("aspect는 0보다 커야 한다");
            var viewHeight = orthographicSize * 2d;
            return Math.Max(viewHeight / DesignHeight, viewHeight * aspect / DesignWidth);
        }
    }
}
