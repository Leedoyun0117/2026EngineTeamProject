using System.Linq;
using System.Text;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    internal static class SkillTargetMenu {
        private const string Root = "LDY/Skill Targets/";
        private const string AskOwner = "이 폴더 주인에게 확인 받았습니까?";

        [MenuItem(Root + "1. Dry Run", priority = 0)]
        private static void DryRun() => Print(DryRunReport.Build(Plan()));

        [MenuItem(Root + "2. Apply (LDY, Main)", priority = 1)]
        private static void ApplyDirect() =>
            Apply(p => p.Access == ApplyAccess.Direct, "LDY, Main");

        [MenuItem(Root + "3. Apply (HES)", priority = 2)]
        private static void ApplyHes() => ApplyConfirmed(FolderOwner.Hes);

        [MenuItem(Root + "3. Apply (JJB)", priority = 3)]
        private static void ApplyJjb() => ApplyConfirmed(FolderOwner.Jjb);

        [MenuItem(Root + "Validate", priority = 20)]
        private static void Validate() {
            var set = Plan();
            var sb = new StringBuilder("## Skill Targets Validate (수정 안 함)\n");
            SceneValidator.AppendTo(sb, set);
            ImporterMapCheck.AppendTo(sb, set);
            Print(sb.ToString());
        }

        [MenuItem(Root + "Select Rules", priority = 40)]
        private static void SelectRules() {
            Selection.activeObject = SkillTargetRulesStore.Ensure();
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private static void ApplyConfirmed(FolderOwner owner) {
            var name = DryRunReport.OwnerName(owner);
            if (!EditorUtility.DisplayDialog($"Skill Targets: {name} 적용", $"{name} 폴더 프리팹을 수정합니다.\n{AskOwner}", "예, 확인 받음", "취소"))
                return;
            Apply(p => p.Owner == owner && p.Access == ApplyAccess.NeedsConfirmation, name);
        }

        private static void Apply(System.Func<PrefabPlan, bool> filter, string scope) {
            var set = Plan();
            var prefabs = set.Prefabs.Where(filter).ToList();
            var problems = HtyTypes.BlockingProblems(prefabs.SelectMany(p => p.Targets.Select(t => t.Target)));
            if (problems.Count > 0) {
                EditorUtility.DisplayDialog("Skill Targets: 적용 중단", string.Join("\n", problems), "확인");
                return;
            }

            var log = PrefabApplier.Apply(prefabs);
            AssetDatabase.SaveAssets();
            var after = Plan().Prefabs.Where(filter).Where(p => p.HasChanges).Select(p => p.Path).ToList();

            var sb = new StringBuilder($"## Skill Targets Apply ({scope})\n");
            foreach (var line in log.DefaultIfEmpty("바꿀 것 없음")) sb.AppendLine("- " + line);
            sb.AppendLine(after.Count == 0
                ? "\n다시 계산: 남은 변경 없음"
                : "\n다시 계산: 아직 남은 변경 " + string.Join(", ", after));
            Print(sb.ToString());
        }

        private static PlanSet Plan() => PrefabPlanner.Build(SkillTargetRulesStore.Ensure());

        private static void Print(string report) {
            EditorGUIUtility.systemCopyBuffer = report;
            Debug.Log(report + "\n(클립보드에 복사됨)");
        }
    }
}
