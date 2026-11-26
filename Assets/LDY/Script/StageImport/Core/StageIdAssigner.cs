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

    public readonly struct StageAssignment {
        public StageAssignment(string mapId, string stageId, int world) {
            MapId = mapId;
            StageId = stageId;
            World = world;
        }

        public string MapId { get; }
        public string StageId { get; }
        public int World { get; }
    }

    public sealed class AssignmentResult {
        public List<StageAssignment> Assigned { get; } = new List<StageAssignment>();
        public List<string> Unslotted { get; } = new List<string>();
        public List<string> MissingInStages { get; } = new List<string>();
        public List<string> Duplicates { get; } = new List<string>();
    }

    // slots[i].ids 순서대로 "{i+1}-T", "{i+1}-1", ... 를 붙인다.
    public static class StageIdAssigner {
        public static AssignmentResult Assign(CursorStageFile file, SlotIdRule rule) {
            var result = new AssignmentResult();
            var used = new HashSet<string>();
            for (var s = 0; s < file.Slots.Count; s++) {
                var world = s + 1;
                var number = 0;
                foreach (var mapId in file.Slots[s].Ids) {
                    if (file.FindStage(mapId) == null) {
                        result.MissingInStages.Add(mapId);
                        continue;
                    }

                    if (!used.Add(mapId)) {
                        result.Duplicates.Add(mapId);
                        continue;
                    }

                    result.Assigned.Add(new StageAssignment(mapId, IdFor(world, number, rule), world));
                    number++;
                }
            }

            foreach (var stage in file.Stages)
                if (!used.Contains(stage.Id))
                    result.Unslotted.Add(stage.Id);
            return result;
        }

        public static string IdFor(int world, int index, SlotIdRule rule) {
            if (!rule.FirstIsTutorial) return $"{world}-{index + 1}";
            return index == 0 ? $"{world}-{rule.TutorialSuffix}" : $"{world}-{index}";
        }
    }
}
