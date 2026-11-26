using System;
using System.Linq;
using LDY.Flow.Core;

namespace LDY.Tests.Flow {
    internal static class FlowTestSlots {
        public static StageSlot[] FromPlan(Func<StagePlanEntry, bool> isReady) =>
            StagePlan.Entries.Select(e => new StageSlot(e.Id, e.World, e.Number, isReady(e))).ToArray();

        // 3-5, 3-6은 기획 미정이라 비어 있다.
        public static StageSlot[] PlannedReadiness() => FromPlan(e => e.Abilities.Count > 0);

        public static int IndexOf(StageSlot[] slots, string id) => Array.FindIndex(slots, s => s.Id == id);

        public static void ClearUpTo(ProgressModel progress, StageSlot[] slots, string lastId) {
            foreach (var slot in slots) {
                progress.RecordClear(slot.Id, 10d);
                if (slot.Id == lastId) return;
            }
        }
    }
}
