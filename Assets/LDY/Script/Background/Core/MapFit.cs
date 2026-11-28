using System;

namespace LDY.Background.Core {
    public enum BackgroundFitMode {
        FixedScale,
        CoverMap
    }

    public enum BackgroundAnchor {
        BottomLeft,
        Center
    }

    public readonly struct WorldRect {
        public readonly float MinX;
        public readonly float MinY;
        public readonly float MaxX;
        public readonly float MaxY;

        public WorldRect(float minX, float minY, float maxX, float maxY) {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public float Width => MaxX - MinX;
        public float Height => MaxY - MinY;
        public float CenterX => (MinX + MaxX) * 0.5f;
        public float CenterY => (MinY + MaxY) * 0.5f;

        public static WorldRect FromCenter(float centerX, float centerY, float width, float height) =>
            new WorldRect(centerX - width * 0.5f, centerY - height * 0.5f, centerX + width * 0.5f, centerY + height * 0.5f);
    }

    public readonly struct BackgroundPlan {
        public readonly int Scale;
        public readonly WorldRect Window;
        public readonly WorldRect Desktop;

        public BackgroundPlan(int scale, WorldRect window, WorldRect desktop) {
            Scale = scale;
            Window = window;
            Desktop = desktop;
        }
    }

    public static class MapFit {
        public const float DesignWidth = 16f;
        public const float DesignHeight = 9f;
        public const int DefaultFixedScale = 3;
        private const double Epsilon = 1e-4;

        public static int CoverScale(float mapWidth, float mapHeight) {
            if (mapWidth <= 0f || mapHeight <= 0f) throw new ArgumentException("맵 bounds의 가로·세로는 0보다 커야 한다");
            var needed = Math.Max(mapWidth / (double)DesignWidth, mapHeight / (double)DesignHeight);
            return Math.Max(1, (int)Math.Ceiling(needed - Epsilon));
        }

        public static BackgroundPlan Plan(WorldRect map, BackgroundFitMode mode, int fixedScale, BackgroundAnchor anchor,
            float viewHalfWidth, float viewHalfHeight) {
            var scale = mode == BackgroundFitMode.CoverMap ? CoverScale(map.Width, map.Height) : Math.Max(1, fixedScale);
            if (mode == BackgroundFitMode.FixedScale && (map.Width <= 0f || map.Height <= 0f))
                throw new ArgumentException("맵 bounds의 가로·세로는 0보다 커야 한다");
            var width = DesignWidth * scale;
            var height = DesignHeight * scale;
            var window = anchor == BackgroundAnchor.BottomLeft
                ? new WorldRect(map.MinX, map.MinY, map.MinX + width, map.MinY + height)
                : WorldRect.FromCenter(map.CenterX, map.CenterY, width, height);
            var marginX = Math.Max(0f, viewHalfWidth);
            var marginY = Math.Max(0f, viewHalfHeight);
            var desktop = new WorldRect(map.MinX - marginX, map.MinY - marginY, map.MaxX + marginX, map.MaxY + marginY);
            return new BackgroundPlan(scale, window, desktop);
        }
    }
}
