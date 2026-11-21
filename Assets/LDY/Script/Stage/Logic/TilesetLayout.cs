using System.Collections.Generic;

namespace LDY.Stage {
    public readonly struct PixelRect {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public PixelRect(int x, int y, int width, int height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public static class TilesetLayout {
        public const int Columns = 4;
        public const int Rows = 6;
        private const int PlatformRow = 4;

        public static string NameAt(int column, int row) {
            if (row < PlatformRow) return GroundTileNamer.Name(row, column);
            if (row == PlatformRow) return PlatformTileNamer.Names[column];
            return SpikeTileNamer.Name((SpikeDirection)column);
        }

        public static IEnumerable<string> AllNames() {
            for (var row = 0; row < Rows; row++)
            for (var column = 0; column < Columns; column++)
                yield return NameAt(column, row);
        }

        public static PixelRect RectAt(int column, int row, int tileSize) {
            return new PixelRect(column * tileSize, (Rows - 1 - row) * tileSize, tileSize, tileSize);
        }
    }
}
