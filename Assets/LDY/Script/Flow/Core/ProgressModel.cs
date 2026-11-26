using System.Collections.Generic;

namespace LDY.Flow.Core {
    public readonly struct StageRecord {
        public StageRecord(bool cleared, double? bestSeconds) {
            Cleared = cleared;
            BestSeconds = bestSeconds;
        }

        public bool Cleared { get; }
        public double? BestSeconds { get; }
    }

    public sealed class ProgressModel {
        private readonly Dictionary<string, StageRecord> _records = new Dictionary<string, StageRecord>();
        private readonly HashSet<string> _forcedUnlocks = new HashSet<string>();

        public IReadOnlyDictionary<string, StageRecord> Records => _records;
        public IEnumerable<string> ForcedUnlocks => _forcedUnlocks;

        public bool IsCleared(string id) => _records.TryGetValue(id, out var record) && record.Cleared;

        public bool IsForcedUnlocked(string id) => _forcedUnlocks.Contains(id);

        public bool TryGetBest(string id, out double seconds) {
            seconds = 0d;
            if (!_records.TryGetValue(id, out var record) || !record.BestSeconds.HasValue) return false;
            seconds = record.BestSeconds.Value;
            return true;
        }

        // 클리어로 표시하고, 기록이 없거나 더 빠를 때만 최고 기록을 바꾼다. 기록이 바뀌면 true.
        public bool RecordClear(string id, double seconds) {
            var hasBest = TryGetBest(id, out var best);
            var improved = !hasBest || seconds < best;
            _records[id] = new StageRecord(true, improved ? seconds : best);
            return improved;
        }

        public void Restore(string id, StageRecord record) => _records[id] = record;

        public void ForceUnlock(string id) => _forcedUnlocks.Add(id);

        public void Reset() {
            _records.Clear();
            _forcedUnlocks.Clear();
        }
    }
}
