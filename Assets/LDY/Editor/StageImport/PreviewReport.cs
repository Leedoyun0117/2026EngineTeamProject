using System.Linq;
using System.Text;
using LDY.FlowEditor;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    // 씬을 만들지 않고 맵별 요약만 만든다.
    internal static class PreviewReport {
        public static string Build(ImportSource source, FlowLog log) {
            var sb = new StringBuilder();
            sb.Append($"파일 {source.Settings.jsonPath} (v{source.File.Version}), 맵 {source.File.Stages.Count}개, ");
            sb.Append($"슬롯 {source.File.Slots.Count}개, 튜토리얼 규칙 {(source.Settings.firstIsTutorial ? "있음" : "없음")}");
            foreach (var job in source.Jobs) AppendJob(sb, job, log);
            AppendList(sb, "슬롯에 없어 가져오지 않는 맵", source.Assignments.Unslotted);
            AppendList(sb, "슬롯에 있지만 stages에 없는 id", source.Assignments.MissingInStages);
            AppendList(sb, "슬롯에 두 번 이상 나온 맵", source.Assignments.Duplicates);
            return sb.ToString();
        }

        private static void AppendJob(StringBuilder sb, ImportJob job, FlowLog log) {
            var plan = job.Plan;
            var assets = new MapAssets(job.Map, job.Assignment.World, log);
            sb.Append($"\n\n[{job.Assignment.StageId}] {plan.Source.Name} ({job.Assignment.MapId}) " +
                      $"{plan.Width}x{plan.Height}, 블록 {plan.BlockCount}칸");
            sb.Append($"\n  씬 {job.ScenePath}{(job.SceneExists ? "  ← 이미 있음, 건너뜀" : "")}");
            sb.Append($"\n  카탈로그 {CatalogStep.Describe(job.Assignment.StageId, job.ScenePath)}");
            sb.Append($"\n  시작 {plan.Starts.Count}, 골 {plan.Goals.Count}, 메모 {plan.Notes.Count}, " +
                      $"어둠 {plan.Areas.Count(a => a.Kind == AreaKind.Dark)}, 탭 {plan.Areas.Count(a => a.Kind == AreaKind.Tab)}" +
                      (plan.Source.Dark ? ", 맵 전체 dark" : ""));
            foreach (var pair in plan.KeyCounts)
                sb.Append($"\n  {pair.Key,-12} {pair.Value,4}  {KeyStatus.Describe(pair.Key, plan, assets)}");
            if (plan.Unmapped.Count > 0)
                sb.Append("\n  매핑 못 함: " + string.Join(", ", plan.Unmapped.Select(p => $"{p.Key}×{p.Value}")));
        }

        private static void AppendList(StringBuilder sb, string title, System.Collections.Generic.List<string> ids) {
            if (ids.Count == 0) return;
            sb.Append($"\n\n{title}: {string.Join(", ", ids)}");
        }
    }

    internal static class KeyStatus {
        public static string Describe(string key, ImportPlan plan, MapAssets assets) {
            if (plan.Unmapped.ContainsKey(key)) return "불가 (대응 없음)";
            if (ToolKeys.IsCustom(key)) return DescribeCustom(key, plan, assets);
            DefaultKeyRules.TryGet(key, out var rule);
            return Describe(key, rule, assets);
        }

        private static string DescribeCustom(string key, ImportPlan plan, MapAssets assets) {
            var use = plan.Customs.Find(c => c.Id == key);
            var label = $"'{use.Name}' col={use.Col}";
            switch (use.Outcome) {
                case CustomOutcome.Skipped: return $"건너뜀 ({label})";
                case CustomOutcome.MappedByName: return $"대응표 '{use.Key}' ({label}) {State(assets.PrefabState(use.Key))}";
                default: return $"대체 → {use.Key} ({label}) {State(assets.TileState(use.Key))}";
            }
        }

        private static string Describe(string key, KeyRule rule, MapAssets assets) {
            switch (rule.Placement) {
                case Placement.Tile: return "타일 " + State(assets.TileState(key));
                case Placement.Decoration:
                    return assets.TileState(key) == AssetState.Missing ? "장식 타일 없음 → 건너뜀" : "장식 타일 " + State(assets.TileState(key));
                case Placement.Start: return "자동 (PlayerStart·Player 이동)";
                case Placement.Goal: return "자동 (StageGoal)";
                case Placement.Area: return "표시용 영역 (인식 코드 없음)";
                case Placement.HorizontalRun: return "가로 런 프리팹 " + State(assets.PrefabState(key));
                case Placement.Group4: return "4방향 묶음 프리팹 " + State(assets.PrefabState(key));
                default: return "프리팹 " + State(assets.PrefabState(key));
            }
        }

        private static string State(AssetState state) {
            switch (state) {
                case AssetState.Auto: return "자동";
                case AssetState.Temp: return "임시";
                default: return "불가 (에셋 없음)";
            }
        }
    }
}
