using System.Collections.Generic;

namespace LDY.Flow.Core {
    public enum StageFilterMode {
        All,
        Incomplete,
        Complete
    }

    public readonly struct StageSummary {
        public StageSummary(int total, int cleared, int locked) {
            Total = total;
            Cleared = cleared;
            Locked = locked;
        }

        public int Total { get; }
        public int Cleared { get; }
        public int Locked { get; }
    }

    public static class StageFilter {
        public static bool Matches(StageFilterMode mode, StageStatus status) {
            switch (mode) {
                case StageFilterMode.Complete: return status == StageStatus.Cleared;
                case StageFilterMode.Incomplete: return status != StageStatus.Cleared;
                default: return true;
            }
        }

        public static List<int> Apply(StageFilterMode mode, IReadOnlyList<StageStatus> statuses,
            IEnumerable<int> candidates) {
            var result = new List<int>();
            foreach (var index in candidates)
                if (Matches(mode, statuses[index]))
                    result.Add(index);
            return result;
        }

        public static StageSummary Summarize(IReadOnlyList<StageStatus> statuses, IEnumerable<int> shown) {
            int total = 0, cleared = 0, locked = 0;
            foreach (var index in shown) {
                total++;
                if (statuses[index] == StageStatus.Cleared) cleared++;
                if (statuses[index] == StageStatus.Locked) locked++;
            }

            return new StageSummary(total, cleared, locked);
        }
    }
}
