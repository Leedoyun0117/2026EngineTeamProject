using System.Collections.Generic;

namespace LDY.StageImport.Core {
    public sealed class SlotIdRule {
        public SlotIdRule(bool firstIsTutorial, string tutorialSuffix) {
            FirstIsTutorial = firstIsTutorial;
            TutorialSuffix = tutorialSuffix;
        }

        public bool FirstIsTutorial { get; }
        public string TutorialSuffix { get; }
    }

    public enum IdSource {
        Name,
        SlotOrder
    }

    public readonly struct StageAssignment {
        public StageAssignment(string mapId, string stageId, int world, IdSource source = IdSource.SlotOrder,
            int slotWorld = 0) {
            MapId = mapId;
            StageId = stageId;
            World = world;
            Source = source;
            SlotWorld = slotWorld == 0 ? world : slotWorld;
        }

        public string MapId { get; }
        public string StageId { get; }

        // 대응표·카탈로그에 쓰는 월드. 이름에서 id를 얻었으면 이름의 월드다.
        public int World { get; }
        public IdSource Source { get; }

        // 맵이 들어 있는 슬롯 번호(1부터).
        public int SlotWorld { get; }
    }

    public readonly struct WorldMismatch {
        public WorldMismatch(string mapId, string mapName, int slotWorld, int nameWorld) {
            MapId = mapId;
            MapName = mapName;
            SlotWorld = slotWorld;
            NameWorld = nameWorld;
        }

        public string MapId { get; }
        public string MapName { get; }
        public int SlotWorld { get; }
        public int NameWorld { get; }
    }

    public sealed class IdConflict {
        public IdConflict(string stageId, IReadOnlyList<string> mapIds) {
            StageId = stageId;
            MapIds = mapIds;
        }

        public string StageId { get; }
        public IReadOnlyList<string> MapIds { get; }
    }

    public sealed class AssignmentResult {
        public List<StageAssignment> Assigned { get; } = new List<StageAssignment>();
        public List<string> Unslotted { get; } = new List<string>();
        public List<string> MissingInStages { get; } = new List<string>();
        public List<string> Duplicates { get; } = new List<string>();
        public List<WorldMismatch> WorldMismatches { get; } = new List<WorldMismatch>();

        // 같은 스테이지 id가 두 맵 이상에 붙은 경우. 해당 맵은 모두 Assigned에서 빠진다.
        public List<IdConflict> IdConflicts { get; } = new List<IdConflict>();
    }

    // 맵 이름이 "{숫자}-{숫자|T}"면 그 이름을 id로 쓰고, 아니면 slots[i].ids 순서대로 "{i+1}-T", "{i+1}-1", ... 를 붙인다.
    // 순서 번호는 이름으로 id를 얻은 맵도 한 자리로 센다.
    public static class StageIdAssigner {
        public static AssignmentResult Assign(CursorStageFile file, SlotIdRule rule) {
            var result = new AssignmentResult();
            var used = new HashSet<string>();
            var candidates = new List<StageAssignment>();
            for (var s = 0; s < file.Slots.Count; s++) {
                var world = s + 1;
                var number = 0;
                foreach (var mapId in file.Slots[s].Ids) {
                    var stage = file.FindStage(mapId);
                    if (stage == null) {
                        result.MissingInStages.Add(mapId);
                        continue;
                    }

                    if (!used.Add(mapId)) {
                        result.Duplicates.Add(mapId);
                        continue;
                    }

                    candidates.Add(Candidate(stage, world, IdFor(world, number, rule), result));
                    number++;
                }
            }

            RemoveConflicts(candidates, result);
            foreach (var stage in file.Stages)
                if (!used.Contains(stage.Id))
                    result.Unslotted.Add(stage.Id);
            return result;
        }

        public static string IdFor(int world, int index, SlotIdRule rule) {
            if (!rule.FirstIsTutorial) return $"{world}-{index + 1}";
            return index == 0 ? $"{world}-{rule.TutorialSuffix}" : $"{world}-{index}";
        }

        private static StageAssignment Candidate(ToolStage stage, int slotWorld, string slotId, AssignmentResult result) {
            if (!StageNameId.TryParse(stage.Name, out var nameWorld, out var nameId))
                return new StageAssignment(stage.Id, slotId, slotWorld, IdSource.SlotOrder, slotWorld);
            if (nameWorld != slotWorld)
                result.WorldMismatches.Add(new WorldMismatch(stage.Id, stage.Name, slotWorld, nameWorld));
            return new StageAssignment(stage.Id, nameId, nameWorld, IdSource.Name, slotWorld);
        }

        private static void RemoveConflicts(List<StageAssignment> candidates, AssignmentResult result) {
            var byId = new Dictionary<string, List<string>>();
            var order = new List<string>();
            foreach (var c in candidates) {
                if (!byId.TryGetValue(c.StageId, out var maps)) {
                    maps = new List<string>();
                    byId.Add(c.StageId, maps);
                    order.Add(c.StageId);
                }

                maps.Add(c.MapId);
            }

            foreach (var id in order)
                if (byId[id].Count > 1)
                    result.IdConflicts.Add(new IdConflict(id, byId[id]));
            foreach (var c in candidates)
                if (byId[c.StageId].Count == 1)
                    result.Assigned.Add(c);
        }
    }
}
