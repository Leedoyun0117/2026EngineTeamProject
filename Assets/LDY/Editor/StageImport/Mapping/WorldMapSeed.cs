using System.Collections.Generic;
using LDY.Stage;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    internal readonly struct SeedPrefab {
        public SeedPrefab(string key, ClusterMode cluster, params string[] candidates) {
            Key = key;
            Cluster = cluster;
            Candidates = candidates;
        }

        public string Key { get; }
        public ClusterMode Cluster { get; }
        public IReadOnlyList<string> Candidates { get; }
    }

    // 대응표를 처음 만들 때 넣을 기본값. 앞 후보부터 있는 프리팹을 쓴다.
    internal static class WorldMapSeed {
        private const string MainObj = "Assets/Main/02.Prefabs/Obj/";
        private const string MainSkill = "Assets/Main/02.Prefabs/SkillBlock/";
        private const string MainUi = "Assets/Main/02.Prefabs/UI/";
        private const string LdyStage1 = "Assets/LDY/Prefabs/Stage1/";

        public static readonly IReadOnlyList<(string key, TileLayerKind layer)> TileKeys = new[] {
            (ToolKeys.Ground, TileLayerKind.Ground),
            (ToolKeys.Platform, TileLayerKind.Platform),
            (ToolKeys.Spike, TileLayerKind.Hazard),
            ("invisible", TileLayerKind.Invisible)
        };

        public static IEnumerable<SeedPrefab> Prefabs(int world) {
            yield return world == 2
                ? new SeedPrefab("box", ClusterMode.PerCell, MainObj + "BoxStage2.prefab", MainObj + "Box.prefab")
                : new SeedPrefab("box", ClusterMode.PerCell, MainObj + "Box.prefab", LdyStage1 + "Stage1_BoxCdCase.prefab");
            yield return new SeedPrefab("cut", ClusterMode.PerCell, MainSkill + "CuttingBlock.prefab",
                LdyStage1 + "Stage1_CutBlock.prefab");
            yield return new SeedPrefab("ghost", ClusterMode.Connected4, MainSkill + "OpacityBlock.prefab",
                LdyStage1 + "Stage1_GhostBlock.prefab");
            yield return new SeedPrefab(ToolKeys.GhostHidden, ClusterMode.Connected4, MainSkill + "OpacityBlock.prefab",
                LdyStage1 + "Stage1_GhostBlock.prefab");
            yield return new SeedPrefab("mov_h", ClusterMode.StretchHorizontal, MainObj + "Stage1_MovingPlatformH.prefab");
            yield return new SeedPrefab("mov_v", ClusterMode.StretchHorizontal, MainObj + "Stage1_MovingPlatformV.prefab");
            yield return new SeedPrefab(ToolKeys.Goal, ClusterMode.PerCell, MainObj + "Stage1_GoalMusicFile.prefab",
                LdyStage1 + "Stage1_GoalMusicFile.prefab");
        }

        public static SeedPrefab Pickup(string ability, string catalogPrefabName) {
            var key = ToolKeys.AbsorbPrefix + ability;
            return string.IsNullOrEmpty(catalogPrefabName)
                ? new SeedPrefab(key, ClusterMode.PerCell)
                : new SeedPrefab(key, ClusterMode.PerCell, MainUi + catalogPrefabName + ".prefab",
                    LdyStage1 + catalogPrefabName + ".prefab");
        }
    }
}
