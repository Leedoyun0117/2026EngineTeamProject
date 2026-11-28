using System.Collections.Generic;
using System.Linq;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    internal sealed class PrefabPlan {
        public PrefabPlan(string path, IReadOnlyList<TargetPlan> targets) {
            Path = path;
            Targets = targets;
        }

        public string Path { get; }
        public IReadOnlyList<TargetPlan> Targets { get; }
        public FolderOwner Owner => FolderOwners.Resolve(Path);
        public ApplyAccess Access => FolderOwners.Access(Owner);
        public bool HasChanges => Targets.Any(t => t.HasChanges);
    }

    internal sealed class PlanSet {
        public List<PrefabPlan> Prefabs { get; } = new List<PrefabPlan>();
        public List<MergedTarget> Targets { get; } = new List<MergedTarget>();
        public List<string> RuleIssues { get; } = new List<string>();
    }

    // 규칙 표 → 프리팹별 계획. 프리팹 에셋을 읽기만 한다.
    internal static class PrefabPlanner {
        public static PlanSet Build(SkillTargetRules rules) {
            var set = new PlanSet();
            var requests = RequestBuilder.Build(rules, set.RuleIssues);
            set.Targets.AddRange(TargetMerger.Merge(requests));

            foreach (var group in set.Targets.GroupBy(t => t.PrefabPath).OrderBy(g => g.Key)) {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(group.Key);
                var plans = group.Select(t => PlanCalculator.Calculate(t, SnapshotReader.Read(TargetLocator.Find(root, t.Child), t)))
                    .ToList();
                set.Prefabs.Add(new PrefabPlan(group.Key, plans));
            }

            return set;
        }
    }
}
