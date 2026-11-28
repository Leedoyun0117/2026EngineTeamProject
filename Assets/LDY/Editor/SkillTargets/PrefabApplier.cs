using System.Collections.Generic;
using System.Linq;
using LDY.SkillTargets.Core;
using UnityEditor;

namespace LDY.SkillTargetsEditor {
    // 프리팹을 열어 계획을 다시 계산하고 실행한 뒤 같은 경로에 저장한다(GUID 유지).
    internal static class PrefabApplier {
        public static List<string> Apply(IEnumerable<PrefabPlan> prefabs) {
            var log = new List<string>();
            foreach (var prefab in prefabs.Where(p => p.HasChanges)) {
                var errors = new List<string>();
                var done = ApplyOne(prefab, errors);
                log.Add($"{prefab.Path}: {done}건 적용");
                log.AddRange(errors.Select(e => "  실패 " + e));
            }

            return log;
        }

        private static int ApplyOne(PrefabPlan prefab, List<string> errors) {
            var root = PrefabUtility.LoadPrefabContents(prefab.Path);
            try {
                var done = 0;
                foreach (var target in prefab.Targets.Select(t => t.Target)) {
                    var go = TargetLocator.Find(root, target.Child);
                    if (go == null) continue;
                    var plan = PlanCalculator.Calculate(target, SnapshotReader.Read(go, target));
                    done += PlanApplier.Apply(go, plan, errors);
                }

                if (done > 0) PrefabUtility.SaveAsPrefabAsset(root, prefab.Path);
                return done;
            } finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
