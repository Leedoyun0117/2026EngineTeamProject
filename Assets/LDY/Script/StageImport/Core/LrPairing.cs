using System.Collections.Generic;

namespace LDY.StageImport.Core {
    public enum LrStatus {
        // L·R 칸이 없다
        NotUsed,

        // LRDoor 1개 + L 1묶음 + R 1묶음: L/R 버튼을 그 칸으로 옮긴다
        Aligned,

        // 짝이 안 맞아 옮기지 않는다(Reason)
        Mismatch
    }

    // 같은 맵의 LRDoor(custom 묶음)와 "L"·"R" 칸을 짝짓는다. 이어진 칸은 한 묶음으로 본다.
    public sealed class LrPairing {
        private LrPairing(LrStatus status, string reason, GridRect door, GridRect l, GridRect r) {
            Status = status;
            Reason = reason;
            Door = door;
            L = l;
            R = r;
        }

        public LrStatus Status { get; }
        public string Reason { get; }
        public GridRect Door { get; }
        public GridRect L { get; }
        public GridRect R { get; }

        public static LrPairing Judge(ImportPlan plan, string doorEntry, string lEntry, string rEntry) {
            var doors = Groups(plan, doorEntry);
            var ls = Groups(plan, lEntry);
            var rs = Groups(plan, rEntry);
            if (ls.Count == 0 && rs.Count == 0) return Fail(LrStatus.NotUsed, "");

            var problems = new List<string>();
            if (doors.Count == 0) problems.Add("LRDoor 없음");
            else if (doors.Count > 1) problems.Add($"LRDoor {doors.Count}개");
            if (ls.Count == 0) problems.Add("L 없음");
            else if (ls.Count > 1) problems.Add($"L {ls.Count}곳");
            if (rs.Count == 0) problems.Add("R 없음");
            else if (rs.Count > 1) problems.Add($"R {rs.Count}곳");
            if (problems.Count > 0) return Fail(LrStatus.Mismatch, string.Join(", ", problems));

            return new LrPairing(LrStatus.Aligned, "", GroupSpot.Bounds(doors[0]), GroupSpot.Bounds(ls[0]),
                GroupSpot.Bounds(rs[0]));
        }

        public static LrPairing Mismatch(string reason) => Fail(LrStatus.Mismatch, reason);

        private static LrPairing Fail(LrStatus status, string reason) =>
            new LrPairing(status, reason, default, default, default);

        // 계획에서 대응표 항목 이름(col 무관)이 같은 칸을 모아 4방향으로 묶는다.
        private static List<List<GridPoint>> Groups(ImportPlan plan, string entryName) {
            var cells = new HashSet<GridPoint>();
            foreach (var item in plan.Prefabs)
                if (IsEntry(item.Key, entryName))
                    for (var x = item.Rect.X; x < item.Rect.X + item.Rect.Width; x++)
                    for (var y = item.Rect.Y; y < item.Rect.Y + item.Rect.Height; y++)
                        cells.Add(new GridPoint(x, y));
            foreach (var group in plan.Groups)
                if (IsEntry(group.Key, entryName))
                    foreach (var cell in group.Cells)
                        cells.Add(cell);
            if (cells.Count == 0) return new List<List<GridPoint>>();
            return RegionFinder.Connected4(plan.Width, plan.Height, (x, y) => cells.Contains(new GridPoint(x, y)));
        }

        public static bool IsEntry(string key, string entryName) =>
            CustomKey.TryParse(key, out var name, out _) && name == entryName;
    }
}
