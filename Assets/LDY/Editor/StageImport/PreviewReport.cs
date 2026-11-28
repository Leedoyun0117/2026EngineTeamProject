using System.Collections.Generic;
using System.Linq;
using System.Text;
using LDY.FlowEditor;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    // 씬을 만들지 않고 맵별 요약만 만든다.
    internal static class PreviewReport {
        public static string Build(ImportSource source, FlowLog log) {
            var sb = new StringBuilder();
            sb.Append($"파일 {source.JsonPath} (v{source.File.Version}), 맵 {source.File.Stages.Count}개, ");
            sb.Append($"슬롯 {source.File.Slots.Count}개, 튜토리얼 규칙 {(source.Settings.firstIsTutorial ? "있음" : "없음")}");

            var details = new StringBuilder();
            sb.Append("\n\nid    | id 출처   | 맵 이름 | 크기    | 자동 | 임시 | 불가 | L/R 정렬 | 타일 대체 | 씬");
            foreach (var job in source.Jobs) {
                var assets = new MapAssets(job.Map, job.Assignment.World, log);
                var tally = BlockTally.Count(job.Plan, assets, job.Customs);
                var origin = job.Assignment.Source == IdSource.Name ? "이름" : "슬롯 순서";
                sb.Append($"\n{job.Assignment.StageId,-5} | {origin,-7} | {job.Plan.Source.Name,-6} | " +
                          $"{job.Plan.Width + "x" + job.Plan.Height,-7} | {tally.Auto,4} | {tally.Temp,4} | {tally.Fail,4} | " +
                          $"{tally.LrAligned,8} | {tally.TileReplaced,9} | " +
                          (job.SceneExists ? "있음 → 새 이름으로 가능" : "새로"));
                AppendJob(details, job, assets);
            }

            AppendWarnings(sb, source);
            sb.Append(details);
            return sb.ToString();
        }

        private static void AppendWarnings(StringBuilder sb, ImportSource source) {
            var a = source.Assignments;
            AppendList(sb, "슬롯에 없어 가져오지 않는 맵", a.Unslotted);
            AppendList(sb, "슬롯에 있지만 stages에 없는 id", a.MissingInStages);
            AppendList(sb, "슬롯에 두 번 이상 나온 맵", a.Duplicates);
            AppendList(sb, "이름의 월드와 슬롯 번호가 다른 맵",
                a.WorldMismatches.Select(m => $"{m.MapName}({m.MapId}) 슬롯 {m.SlotWorld}").ToList());
            AppendList(sb, "id가 겹쳐 가져오지 않는 맵",
                a.IdConflicts.Select(c => $"{c.StageId}: {string.Join("/", c.MapIds)}").ToList());
            AppendList(sb, "같은 이름인데 col이 다름 (같은 프리팹으로 감, 임시 블록은 col대로 충돌)",
                source.ColConflicts.Select(c => $"'{c.EntryName}' {string.Join("/", c.Cols)} ({string.Join(", ", c.CustomIds)})").ToList());
            AppendList(sb, "custom 대응표에서 프리팹이 빈 이름 (분홍 임시 블록) → " + StageImportPaths.CustomMap,
                UsedBlankEntries(source));
            AppendList(sb, "L/R 짝이 안 맞아 버튼을 옮기지 않는 맵", source.Jobs
                .Select(j => (j, lr: LrAlign.Judge(j.Plan, j.Customs)))
                .Where(x => x.lr.Status == LrStatus.Mismatch)
                .Select(x => $"{x.j.Assignment.StageId}({x.lr.Reason})").ToList());
        }

        private static List<string> UsedBlankEntries(ImportSource source) {
            var names = new SortedSet<string>();
            foreach (var job in source.Jobs) {
                var lr = LrAlign.Judge(job.Plan, job.Customs).Status == LrStatus.Aligned;
                foreach (var use in job.Plan.Customs)
                    if (use.Outcome == CustomOutcome.CustomPlaceholder && !(lr && KeyStatus.IsLrButton(use.Key)))
                        names.Add(CustomBlockAssets.DisplayName(use.Key));
            }
            return names.ToList();
        }

        private static void AppendJob(StringBuilder sb, ImportJob job, MapAssets assets) {
            var plan = job.Plan;
            sb.Append($"\n\n[{job.Assignment.StageId}] {plan.Source.Name} ({job.Assignment.MapId}) " +
                      $"{plan.Width}x{plan.Height}, 블록 {plan.BlockCount}칸, id 출처 " +
                      (job.Assignment.Source == IdSource.Name ? "이름" : "슬롯 순서"));
            sb.Append($"\n  씬 {job.ScenePath}{(job.SceneExists ? "  ← 이미 있음 (Import Selected에서 새 이름으로)" : "")}");
            sb.Append($"\n  카탈로그 {CatalogStep.Describe(job.Assignment.StageId, job.ScenePath)}");
            sb.Append($"\n  시작 {plan.Starts.Count}, 골 {plan.Goals.Count}, 메모 {plan.Notes.Count}, " +
                      $"어둠 {plan.Areas.Count(a => a.Kind == AreaKind.Dark)}, 탭 {plan.Areas.Count(a => a.Kind == AreaKind.Tab)}, " +
                      $"custom 영역 {plan.Areas.Count(a => a.Kind == AreaKind.Named)}" +
                      (plan.Source.Dark ? ", 맵 전체 dark" : ""));
            foreach (var pair in plan.KeyCounts)
                sb.Append($"\n  {pair.Key,-12} {pair.Value,4}  {KeyStatus.Describe(pair.Key, plan, assets, job.Customs)}");
            var links = LinkNeeds(job);
            if (links.Count > 0) sb.Append("\n  연결 필요 (스크립트 꺼 둠): " + string.Join(", ", links));
            if (plan.Unmapped.Count > 0)
                sb.Append("\n  매핑 못 함: " + string.Join(", ", plan.Unmapped.Select(p => $"{p.Key}×{p.Value}")));
        }

        private static List<string> LinkNeeds(ImportJob job) {
            var cells = new SortedDictionary<string, (int count, string fields)>();
            foreach (var pair in job.Plan.KeyCounts) {
                if (!CustomKey.Is(pair.Key) || !job.Customs.TryEntry(pair.Key, out var entry, out _)) continue;
                if (!entry.needsLink || entry.prefab == null || entry.linkFields == null) continue;
                cells.TryGetValue(entry.name, out var row);
                cells[entry.name] = (row.count + pair.Value, string.Join("/", entry.linkFields));
            }

            return cells.Select(p => $"{p.Key} {p.Value.count}칸 [{p.Value.fields}]").ToList();
        }

        private static void AppendList(StringBuilder sb, string title, List<string> items) {
            if (items.Count == 0) return;
            sb.Append($"\n\n{title}: {string.Join(", ", items)}");
        }
    }

    internal enum BlockKind {
        Auto,
        Temp,
        Fail
    }

    // 칸 수를 자동(실제 에셋)·임시(분홍·1월드 타일·표시용 영역)·불가(에셋·대응 없음, 건너뜀)로 나눈다.
    // L/R 정렬: 버튼을 옮기는 L·R 칸 수(짝이 맞을 때만), 타일 대체: 대응표에서 타일로 바꾼 칸 수.
    internal readonly struct BlockTally {
        private BlockTally(int auto, int temp, int fail, int lrAligned, int tileReplaced) {
            Auto = auto;
            Temp = temp;
            Fail = fail;
            LrAligned = lrAligned;
            TileReplaced = tileReplaced;
        }

        public int Auto { get; }
        public int Temp { get; }
        public int Fail { get; }
        public int LrAligned { get; }
        public int TileReplaced { get; }

        public static BlockTally Count(ImportPlan plan, MapAssets assets, CustomBlockAssets customs) {
            int auto = 0, temp = 0, fail = 0, lrCells = 0, tiles = 0;
            var lr = LrAlign.Judge(plan, customs).Status == LrStatus.Aligned;
            foreach (var pair in plan.KeyCounts) {
                if (ToolKeys.IsCustom(pair.Key)) {
                    var use = plan.Customs.Find(c => c.Id == pair.Key);
                    if (use.Outcome == CustomOutcome.CustomTile) tiles += pair.Value;
                    if (lr && KeyStatus.IsLrButton(use.Key)) {
                        lrCells += pair.Value;
                        auto += pair.Value;
                        continue;
                    }
                }

                switch (KeyStatus.Kind(pair.Key, plan, assets, customs)) {
                    case BlockKind.Auto:
                        auto += pair.Value;
                        break;
                    case BlockKind.Temp:
                        temp += pair.Value;
                        break;
                    default:
                        fail += pair.Value;
                        break;
                }
            }

            return new BlockTally(auto, temp, fail, lrCells, tiles);
        }
    }

    internal static class KeyStatus {
        public static bool IsLrButton(string key) =>
            LrPairing.IsEntry(key, CustomBlockSeed.LName) || LrPairing.IsEntry(key, CustomBlockSeed.RName);

        public static string Describe(string key, ImportPlan plan, MapAssets assets, CustomBlockAssets customs) {
            if (plan.Unmapped.ContainsKey(key)) return "불가 (대응 없음)";
            if (ToolKeys.IsCustom(key)) return DescribeCustom(key, plan, assets, customs);
            DefaultKeyRules.TryGet(key, out var rule);
            return Describe(key, rule, assets);
        }

        public static BlockKind Kind(string key, ImportPlan plan, MapAssets assets, CustomBlockAssets customs) {
            if (plan.Unmapped.ContainsKey(key)) return BlockKind.Fail;
            if (ToolKeys.IsCustom(key)) {
                var use = plan.Customs.Find(c => c.Id == key);
                switch (use.Outcome) {
                    case CustomOutcome.CustomPrefab:
                        return customs.HasContent(use.Key) ? BlockKind.Auto : BlockKind.Temp;
                    case CustomOutcome.CustomTile:
                        return FromState(assets.TileState(use.Key));
                    case CustomOutcome.CustomPlaceholder:
                    case CustomOutcome.CustomArea:
                        return BlockKind.Temp;
                    case CustomOutcome.MappedByName:
                        return FromState(assets.PrefabState(use.Key));
                    case CustomOutcome.Fallback:
                        return FromState(assets.TileState(use.Key));
                    default:
                        return BlockKind.Fail;
                }
            }

            DefaultKeyRules.TryGet(key, out var rule);
            switch (rule.Placement) {
                case Placement.Start:
                case Placement.Goal:
                    return BlockKind.Auto;
                case Placement.Area:
                    return BlockKind.Temp;
                case Placement.Tile:
                case Placement.Decoration:
                    return FromState(assets.TileState(key));
                default:
                    return FromState(assets.PrefabState(key));
            }
        }

        private static BlockKind FromState(AssetState state) =>
            state == AssetState.Auto ? BlockKind.Auto : state == AssetState.Temp ? BlockKind.Temp : BlockKind.Fail;

        private static string DescribeCustom(string key, ImportPlan plan, MapAssets assets, CustomBlockAssets customs) {
            var use = plan.Customs.Find(c => c.Id == key);
            var label = $"'{use.Name}' col={use.Col}";
            switch (use.Outcome) {
                case CustomOutcome.Skipped: return $"건너뜀 ({label})";
                case CustomOutcome.MappedByName: return $"대응표 '{use.Key}' ({label}) {State(assets.PrefabState(use.Key))}";
                case CustomOutcome.CustomPrefab: return DescribeEntry(use, customs, label);
                case CustomOutcome.CustomPlaceholder:
                    return $"임시 분홍 블록 ({label}, {Binding(use, customs)}) ← custom 대응표 프리팹 비어 있음";
                case CustomOutcome.CustomArea: return $"영역 트리거·이름표 ({label})";
                case CustomOutcome.CustomTile: return $"타일 대체 → {use.Key} ({label}) {State(assets.TileState(use.Key))}";
                default: return $"대체 → {use.Key} ({label}) {State(assets.TileState(use.Key))}";
            }
        }

        private static string DescribeEntry(CustomUse use, CustomBlockAssets customs, string label) {
            var prefab = customs.Prefab(use.Key);
            if (prefab != null) return $"자동 → {prefab.name} ({label}, {Binding(use, customs)})";
            return customs.TryEntry(use.Key, out var entry, out _) && entry.decoration != null
                ? $"장식 그림 → {entry.decoration.name} ({label}, 충돌 없음)"
                : $"임시 ({label}) 프리팹 없음";
        }

        private static string Binding(CustomUse use, CustomBlockAssets customs) {
            if (!customs.TryEntry(use.Key, out var entry, out _)) return "";
            var binding = entry.binding == CustomBinding.PerCell ? "칸마다"
                : entry.binding == CustomBinding.Group4 ? "4방향 묶음"
                : entry.binding == CustomBinding.HorizontalRun ? "가로 런" : "영역";
            if (entry.binding == CustomBinding.PerCell || entry.binding == CustomBinding.Area) return binding;
            var sizing = entry.sizing == CustomSizing.Stretch ? "늘리기"
                : entry.sizing == CustomSizing.RepeatPerCell ? "칸마다 복제"
                : entry.sizing == CustomSizing.OneAtLeft ? "하나(가장 왼쪽 칸)"
                : entry.sizing == CustomSizing.HangFromTop ? "천장 고정·칸 수 길이" : "하나(가운데)";
            return $"{binding}·{sizing}";
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
