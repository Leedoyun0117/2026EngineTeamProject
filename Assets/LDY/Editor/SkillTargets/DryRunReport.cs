using System.Linq;
using System.Text;
using LDY.SkillTargets.Core;

namespace LDY.SkillTargetsEditor {
    internal static class DryRunReport {
        public static string Build(PlanSet set) {
            var sb = new StringBuilder("## Skill Targets Dry Run\n");
            if (!HtyTypes.HasMyCanField())
                sb.AppendLine("\n**Object_HTY에 _myCan 없음 → Apply 막힘. HTY 최신 코드(CanIf)를 먼저 받아야 함.**");

            foreach (var group in set.Prefabs.GroupBy(p => p.Owner).OrderBy(g => g.Key)) {
                sb.AppendLine($"\n### {OwnerName(group.Key)} ({AccessName(FolderOwners.Access(group.Key))})");
                var table = new MarkdownTable("프리팹", "대상", "기획 대상", "능력", "할 일", "주의");
                foreach (var prefab in group)
                foreach (var plan in prefab.Targets)
                    table.Add(PrefabName(prefab.Path), Child(plan.Target.Child), string.Join(", ", plan.Target.Labels),
                        string.Join("+", plan.Target.AbilityIds), Changes(plan), Issues(plan));
                table.AppendTo(sb);
            }

            if (set.RuleIssues.Count > 0) {
                sb.AppendLine("\n### 규칙 표에서 비어 있거나 잘못된 것");
                foreach (var issue in set.RuleIssues) sb.AppendLine("- " + issue);
            }

            var pending = set.Prefabs.Count(p => p.HasChanges);
            sb.AppendLine($"\n바뀔 프리팹 {pending}개 / 대상 프리팹 {set.Prefabs.Count}개");
            return sb.ToString();
        }

        public static string Changes(TargetPlan plan) {
            var changes = plan.Steps.Where(s => s.IsChange).Select(s => s.Describe()).ToList();
            return changes.Count == 0 ? "변경 없음" : string.Join("; ", changes);
        }

        public static string Issues(TargetPlan plan) =>
            string.Join("; ", plan.Steps.Where(s => !s.IsChange).Select(s => s.Describe()));

        public static string OwnerName(FolderOwner owner) => owner switch {
            FolderOwner.Ldy => "LDY",
            FolderOwner.Main => "Main",
            FolderOwner.Hes => "HES(의승)",
            FolderOwner.Jjb => "JJB(지빈)",
            _ => "기타"
        };

        private static string AccessName(ApplyAccess access) => access switch {
            ApplyAccess.Direct => "2. Apply (LDY, Main) 대상",
            ApplyAccess.NeedsConfirmation => "확인 후 3. Apply",
            _ => "적용 안 함"
        };

        public static string PrefabName(string path) => System.IO.Path.GetFileNameWithoutExtension(path);
        private static string Child(string child) => string.IsNullOrEmpty(child) ? "(루트)" : child;
    }
}
