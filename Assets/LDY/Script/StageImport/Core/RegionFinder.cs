using System;
using System.Collections.Generic;

namespace LDY.StageImport.Core {
    public static class RegionFinder {
        // 같은 행에서 가로로 이어진 칸. 아래 행부터, 왼쪽부터.
        public static List<GridRect> HorizontalRuns(int width, int height, Func<int, int, bool> match) {
            var runs = new List<GridRect>();
            for (var y = 0; y < height; y++) {
                var x = 0;
                while (x < width) {
                    if (!match(x, y)) {
                        x++;
                        continue;
                    }

                    var start = x;
                    while (x < width && match(x, y)) x++;
                    runs.Add(new GridRect(start, y, x - start, 1));
                }
            }

            return runs;
        }

        // 상하좌우로 이어진 칸 묶음.
        public static List<List<GridPoint>> Connected4(int width, int height, Func<int, int, bool> match) {
            var seen = new bool[width * height];
            var regions = new List<List<GridPoint>>();
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++) {
                if (seen[y * width + x] || !match(x, y)) continue;
                regions.Add(Flood(width, height, match, seen, new GridPoint(x, y)));
            }

            return regions;
        }

        private static List<GridPoint> Flood(int width, int height, Func<int, int, bool> match, bool[] seen,
            GridPoint start) {
            var region = new List<GridPoint>();
            var stack = new Stack<GridPoint>();
            seen[start.Y * width + start.X] = true;
            stack.Push(start);
            while (stack.Count > 0) {
                var p = stack.Pop();
                region.Add(p);
                Visit(p.X + 1, p.Y);
                Visit(p.X - 1, p.Y);
                Visit(p.X, p.Y + 1);
                Visit(p.X, p.Y - 1);
            }

            region.Sort((a, b) => a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));
            return region;

            void Visit(int x, int y) {
                if (x < 0 || y < 0 || x >= width || y >= height) return;
                var i = y * width + x;
                if (seen[i] || !match(x, y)) return;
                seen[i] = true;
                stack.Push(new GridPoint(x, y));
            }
        }
    }

    // 칸 묶음을 겹치지 않는 사각형 몇 개로 덮는다. 가로 런을 만든 뒤 같은 폭의 위 행 런을 이어 붙인다.
    public static class RectCover {
        public static List<GridRect> Cover(IReadOnlyCollection<GridPoint> cells) {
            var set = new HashSet<GridPoint>(cells);
            if (set.Count == 0) return new List<GridRect>();
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var p in set) {
                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, p.Y);
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, p.Y);
            }

            var runs = RegionFinder.HorizontalRuns(maxX - minX + 1, maxY - minY + 1,
                (x, y) => set.Contains(new GridPoint(x + minX, y + minY)));
            var open = new List<GridRect>();
            var done = new List<GridRect>();
            var row = 0;
            foreach (var run in runs) {
                if (run.Y != row) {
                    CloseNotContinuing(open, done, run.Y);
                    row = run.Y;
                }

                var index = open.FindIndex(r => r.X == run.X && r.Width == run.Width && r.Y + r.Height == run.Y);
                if (index >= 0) open[index] = new GridRect(run.X, open[index].Y, run.Width, open[index].Height + 1);
                else open.Add(run);
            }

            done.AddRange(open);
            var result = done.ConvertAll(r => new GridRect(r.X + minX, r.Y + minY, r.Width, r.Height));
            result.Sort((a, b) => a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));
            return result;
        }

        private static void CloseNotContinuing(List<GridRect> open, List<GridRect> done, int nextRow) {
            for (var i = open.Count - 1; i >= 0; i--) {
                if (open[i].Y + open[i].Height == nextRow) continue;
                done.Add(open[i]);
                open.RemoveAt(i);
            }
        }
    }
}
