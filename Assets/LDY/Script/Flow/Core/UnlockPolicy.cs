using System.Collections.Generic;

namespace LDY.Flow.Core {
    // 슬롯은 1-T, 1-1 … 1-6, 2-T … 순서. 앞 칸을 클리어해야 다음 칸이 열리고, 월드 끝(6)을 깨면 다음 월드 튜토리얼이 열린다.
    // 준비 안 된 칸은 클리어할 수 없으니 뒤 칸은 막힌 채로 남는다.
    public static class UnlockPolicy {
        public static StageStatus StatusOf(IReadOnlyList<StageSlot> slots, int index, ProgressModel progress) {
            var slot = slots[index];
            if (!slot.IsReady) return StageStatus.NotReady;
            if (progress.IsCleared(slot.Id)) return StageStatus.Cleared;
            return IsUnlocked(slots, index, progress) ? StageStatus.Open : StageStatus.Locked;
        }

        public static StageStatus[] Evaluate(IReadOnlyList<StageSlot> slots, ProgressModel progress) {
            var result = new StageStatus[slots.Count];
            for (var i = 0; i < slots.Count; i++) result[i] = StatusOf(slots, i, progress);
            return result;
        }

        public static bool IsUnlocked(IReadOnlyList<StageSlot> slots, int index, ProgressModel progress) {
            if (index == 0) return true;
            return progress.IsForcedUnlocked(slots[index].Id) || progress.IsCleared(slots[index - 1].Id);
        }

        // 막힌 칸을 열려면 먼저 끝내야 하는 바로 앞 칸. 첫 칸이면 null.
        public static string PrerequisiteOf(IReadOnlyList<StageSlot> slots, int index) =>
            index <= 0 ? null : slots[index - 1].Id;
    }
}
