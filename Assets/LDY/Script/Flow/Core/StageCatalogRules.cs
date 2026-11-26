using System.Collections.Generic;
using System.Linq;

namespace LDY.Flow.Core {
    // 카탈로그 순서가 곧 해금 순서라서 개수·중복·순서를 함께 본다. 빈 목록이면 문제가 없는 것.
    public static class StageCatalogRules {
        public static List<string> Validate(IReadOnlyList<StageSlot> slots) {
            var problems = new List<string>();
            var expected = StagePlan.WorldCount * StagePlan.StagesPerWorld;
            if (slots.Count != expected) problems.Add($"항목 수 {slots.Count}개, {expected}개여야 한다");

            foreach (var dup in slots.GroupBy(s => s.Id).Where(g => g.Count() > 1))
                problems.Add($"id 중복 {dup.Key} ({dup.Count()}개)");

            for (var world = 1; world <= StagePlan.WorldCount; world++) {
                var count = slots.Count(s => s.World == world);
                if (count != StagePlan.StagesPerWorld)
                    problems.Add($"월드 {world} 항목 {count}개, {StagePlan.StagesPerWorld}개여야 한다");
            }

            for (var i = 0; i < slots.Count; i++) {
                var slot = slots[i];
                if (slot.Id != StageNaming.Id(slot.World, slot.Number))
                    problems.Add($"{i}번 id {slot.Id}가 월드 {slot.World}, 번호 {slot.Number}와 맞지 않는다");
                if (i > 0 && Order(slots[i - 1]) >= Order(slot))
                    problems.Add($"{i}번 {slot.Id}는 {slots[i - 1].Id}보다 뒤 순서여야 한다");
            }

            return problems;
        }

        private static int Order(StageSlot slot) => slot.World * 100 + slot.Number;
    }
}
