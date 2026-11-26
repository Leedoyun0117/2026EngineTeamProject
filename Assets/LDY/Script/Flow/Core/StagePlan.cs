using System.Collections.Generic;

namespace LDY.Flow.Core {
    public readonly struct WorldPlan {
        public WorldPlan(int world, string displayName, string folderName) {
            World = world;
            DisplayName = displayName;
            FolderName = folderName;
        }

        public int World { get; }
        public string DisplayName { get; }
        public string FolderName { get; }
    }

    public readonly struct StagePlanEntry {
        public StagePlanEntry(int world, int number, params string[] abilities) {
            World = world;
            Number = number;
            Abilities = abilities;
        }

        public int World { get; }
        public int Number { get; }
        public IReadOnlyList<string> Abilities { get; }
        public string Id => StageNaming.Id(World, Number);
        public string FileName => StageNaming.FileName(World, Number);
    }

    // 기획서의 월드·스테이지·사용 능력 표. 능력 id는 Stage1PickupCatalog id와 같다.
    public static class StagePlan {
        public const int WorldCount = 3;
        public const int StagesPerWorld = 7;

        public static readonly IReadOnlyList<WorldPlan> Worlds = new[] {
            new WorldPlan(1, "STAGE_1 뮤직", "STAGE_1_뮤직플레이어"),
            new WorldPlan(2, "STAGE_2 탐색기", "STAGE_2_파일탐색기"),
            new WorldPlan(3, "STAGE_3 휴지통", "STAGE_3_휴지통")
        };

        public static readonly IReadOnlyList<StagePlanEntry> Entries = new[] {
            new StagePlanEntry(1, 0, "pause"),
            new StagePlanEntry(1, 1, "pause"),
            new StagePlanEntry(1, 2, "pause"),
            new StagePlanEntry(1, 3, "pause", "cut"),
            new StagePlanEntry(1, 4, "pause", "drag"),
            new StagePlanEntry(1, 5, "pause", "drag", "zoom", "cut"),
            new StagePlanEntry(1, 6, "pause", "alpha"),
            new StagePlanEntry(2, 0, "zoom", "drag"),
            new StagePlanEntry(2, 1, "drag", "copy"),
            new StagePlanEntry(2, 2, "zoom", "alpha", "drag"),
            new StagePlanEntry(2, 3, "drag", "alpha", "zoom"),
            new StagePlanEntry(2, 4, "drag", "zoom"),
            new StagePlanEntry(2, 5, "drag", "alpha"),
            new StagePlanEntry(2, 6, "drag", "alpha", "cut"),
            new StagePlanEntry(3, 0, "light", "drag"),
            new StagePlanEntry(3, 1, "light", "cut", "drag"),
            new StagePlanEntry(3, 2, "light", "drag"),
            new StagePlanEntry(3, 3, "drag", "light", "tab"),
            new StagePlanEntry(3, 4, "light", "copy"),
            new StagePlanEntry(3, 5),
            new StagePlanEntry(3, 6)
        };
    }
}
