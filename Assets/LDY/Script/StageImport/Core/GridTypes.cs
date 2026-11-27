using System;

namespace LDY.StageImport.Core {
    // Unity 셀 좌표. 맵 왼쪽 아래가 (0,0).
    public readonly struct GridPoint : IEquatable<GridPoint> {
        public GridPoint(int x, int y) {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public bool Equals(GridPoint other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPoint other && Equals(other);
        public override int GetHashCode() => (X * 397) ^ Y;
        public override string ToString() => $"({X},{Y})";
    }

    public readonly struct GridRect : IEquatable<GridRect> {
        public GridRect(int x, int y, int width, int height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public float CenterX => X + Width * 0.5f;
        public float CenterY => Y + Height * 0.5f;

        public static GridRect Cell(GridPoint p) => new GridRect(p.X, p.Y, 1, 1);

        public bool Equals(GridRect other) =>
            X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        public override bool Equals(object obj) => obj is GridRect other && Equals(other);
        public override int GetHashCode() => (((X * 397) ^ Y) * 397 ^ Width) * 397 ^ Height;
        public override string ToString() => $"[{X},{Y} {Width}x{Height}]";
    }

    public static class CellCoords {
        public static GridPoint ToUnity(int toolX, int toolY, int height) => new GridPoint(toolX, height - 1 - toolY);
    }
}
