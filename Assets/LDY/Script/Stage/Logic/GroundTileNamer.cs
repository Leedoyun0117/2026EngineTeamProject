namespace LDY.Stage {
    public static class GroundTileNamer {
        public static readonly string[] Vertical = { "top", "mid", "bottom", "strip" };
        public static readonly string[] Horizontal = { "L", "C", "R", "LR" };

        public static string Name(Adjacency a) {
            return Name(VerticalIndex(a), HorizontalIndex(a.Left, a.Right));
        }

        public static string Name(int verticalIndex, int horizontalIndex) {
            return "ground_" + Vertical[verticalIndex] + "_" + Horizontal[horizontalIndex];
        }

        public static int VerticalIndex(Adjacency a) {
            if (a.Up) return a.Down ? 1 : 2;
            return a.Down ? 0 : 3;
        }

        public static int HorizontalIndex(bool left, bool right) {
            if (left) return right ? 1 : 2;
            return right ? 0 : 3;
        }

        public static Adjacency AdjacencyOf(int verticalIndex, int horizontalIndex) {
            var up = verticalIndex == 1 || verticalIndex == 2;
            var down = verticalIndex == 0 || verticalIndex == 1;
            var left = horizontalIndex == 1 || horizontalIndex == 2;
            var right = horizontalIndex == 0 || horizontalIndex == 1;
            return new Adjacency(up, down, left, right);
        }
    }
}
