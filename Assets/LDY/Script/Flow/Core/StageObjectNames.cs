using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LDY.Flow.Core {
    public readonly struct SpawnCandidate {
        public SpawnCandidate(string name, bool hasSpawnComponent) {
            Name = name;
            HasSpawnComponent = hasSpawnComponent;
        }

        public string Name { get; }
        public bool HasSpawnComponent { get; }
    }

    // 런타임에는 프리팹 출처를 알 수 없어서 인스턴스 이름(복제 접미사 제외)으로 골·시작 지점을 가린다.
    public static class StageObjectNames {
        public const string GoalPrefab = "Stage1_GoalMusicFile";
        public const int NoSpawn = -1;

        private static readonly string[] SpawnNames = { "Start", "Spawn", "StartMarker" };
        private static readonly string[] SpawnPrefabs = { "Stage1_Start", "start_marker" };
        private static readonly Regex CloneSuffix = new Regex(@"(\s*\(\d+\)|\(Clone\))+$");

        public static string StripCloneSuffix(string name) =>
            string.IsNullOrEmpty(name) ? name : CloneSuffix.Replace(name, "");

        public static bool IsGoal(string name) =>
            string.Equals(StripCloneSuffix(name), GoalPrefab, StringComparison.Ordinal);

        // 0: StageSpawnPoint 컴포넌트, 1: 이름 Start/Spawn/StartMarker, 2: 시작 마커 프리팹 인스턴스
        public static int SpawnRank(SpawnCandidate candidate) {
            if (candidate.HasSpawnComponent) return 0;
            var name = StripCloneSuffix(candidate.Name);
            if (Matches(SpawnNames, name)) return 1;
            if (Matches(SpawnPrefabs, name)) return 2;
            return NoSpawn;
        }

        // 순위가 가장 높은 후보, 같으면 계층 순서상 앞의 것. 없으면 -1.
        public static int BestSpawn(IReadOnlyList<SpawnCandidate> candidates) {
            var best = -1;
            var bestRank = int.MaxValue;
            for (var i = 0; i < candidates.Count; i++) {
                var rank = SpawnRank(candidates[i]);
                if (rank == NoSpawn || rank >= bestRank) continue;
                best = i;
                bestRank = rank;
            }

            return best;
        }

        private static bool Matches(string[] names, string name) {
            foreach (var candidate in names)
                if (string.Equals(candidate, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
