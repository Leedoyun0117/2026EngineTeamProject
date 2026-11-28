using System.Collections.Generic;
using System.Linq;

namespace LDY.StageImport.Core {
    // 이름표 자리. 칸 (x, y)는 월드 [x, x+1] x [y, y+1]을 차지한다.
    public readonly struct LabelSpot {
        public LabelSpot(float x, float y) {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
    }

    public static class LabelSpots {
        // 4방향 묶음마다 하나. 묶음 경계 사각형의 가로 가운데, 맨 윗변.
        public static List<LabelSpot> PerGroup(IEnumerable<GridPoint> cells) {
            var set = new HashSet<GridPoint>(cells);
            var spots = new List<LabelSpot>();
            if (set.Count == 0) return spots;

            var minX = set.Min(p => p.X);
            var minY = set.Min(p => p.Y);
            var width = set.Max(p => p.X) - minX + 1;
            var height = set.Max(p => p.Y) - minY + 1;
            var groups = RegionFinder.Connected4(width, height, (x, y) => set.Contains(new GridPoint(x + minX, y + minY)));
            foreach (var group in groups) {
                var left = group.Min(p => p.X) + minX;
                var right = group.Max(p => p.X) + minX + 1;
                var top = group.Max(p => p.Y) + minY + 1;
                spots.Add(new LabelSpot((left + right) * 0.5f, top));
            }

            return spots;
        }
    }
}
