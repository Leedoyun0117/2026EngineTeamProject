using System;

namespace LDY.StageImport.Core {
    // 툴 칸 배열을 Unity 좌표(y 위로 증가)로 읽는 창.
    public sealed class StageGrid<T> {
        private readonly T[] _cells;

        public StageGrid(int width, int height, Func<int, int, T> readToolCell) {
            Width = width;
            Height = height;
            _cells = new T[width * height];
            for (var ty = 0; ty < height; ty++)
            for (var tx = 0; tx < width; tx++) {
                var p = CellCoords.ToUnity(tx, ty, height);
                _cells[p.Y * width + p.X] = readToolCell(tx, ty);
            }
        }

        public int Width { get; }
        public int Height { get; }

        public T this[int x, int y] => _cells[y * Width + x];

        public bool Contains(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public static class StageGrids {
        public static StageGrid<string> Keys(ToolStage stage) =>
            new StageGrid<string>(stage.Width, stage.Height, (x, y) => stage.Cells[y * stage.Width + x] ?? "");

        public static StageGrid<bool> Dark(ToolStage stage) =>
            new StageGrid<bool>(stage.Width, stage.Height, (x, y) => stage.DarkMask[y * stage.Width + x] == 1);
    }
}
