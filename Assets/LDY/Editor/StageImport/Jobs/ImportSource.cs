using System.Collections.Generic;
using LDY.FlowEditor;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    // 설정의 JSON을 읽어 슬롯 순서대로 작업 목록을 만든다.
    internal sealed class ImportSource {
        private ImportSource(StageImportSettings settings, string jsonPath, CursorStageFile file,
            AssignmentResult assignments, CustomBlockAssets customs, List<ColConflict> colConflicts,
            List<ImportJob> jobs) {
            Settings = settings;
            JsonPath = jsonPath;
            File = file;
            Assignments = assignments;
            Customs = customs;
            ColConflicts = colConflicts;
            Jobs = jobs;
        }

        public StageImportSettings Settings { get; }

        // 실제로 읽은 파일. 설정 경로에 없으면 StageImportPaths.JsonCandidates 중 있는 것.
        public string JsonPath { get; }
        public CursorStageFile File { get; }
        public AssignmentResult Assignments { get; }
        public CustomBlockAssets Customs { get; }
        public List<ColConflict> ColConflicts { get; }
        public IReadOnlyList<ImportJob> Jobs { get; }

        public static ImportSource Load(FlowLog log) {
            var settings = StageImportPaths.LoadSettings();
            var jsonPath = ResolveJson(settings, log);
            if (jsonPath == null) return null;

            CursorStageFile file;
            try {
                file = CursorStageParser.Parse(System.IO.File.ReadAllText(jsonPath));
            }
            catch (StageFileException e) {
                log.Warn($"{jsonPath}: {e.Message}");
                return null;
            }

            var customs = new CustomBlockAssets(CustomBlockMapStore.Ensure(file, log));
            foreach (var collision in customs.Index.Collisions) log.Warn("custom 대응표 이름 겹침 " + collision);

            var assignments = StageIdAssigner.Assign(file, settings.IdRule);
            Report(assignments, log);
            var conflicts = CustomColConflicts.Find(file.Customs, customs.Index);

            var jobs = new List<ImportJob>();
            foreach (var assignment in assignments.Assigned) {
                var map = WorldMapStore.Ensure(assignment.World, log);
                var plan = ImportPlanner.Plan(file.FindStage(assignment.MapId), file, new MappingRules(map),
                    customs.Index);
                jobs.Add(new ImportJob(assignment, plan, map, customs, settings.sceneFolder.TrimEnd('/')));
            }

            return new ImportSource(settings, jsonPath, file, assignments, customs, conflicts, jobs);
        }

        private static string ResolveJson(StageImportSettings settings, FlowLog log) {
            if (System.IO.File.Exists(settings.jsonPath)) return settings.jsonPath;
            var fallback = StageImportPaths.FirstExistingJson();
            if (fallback == null) {
                log.Warn($"툴 내보내기 파일이 없다: {settings.jsonPath} (경로는 {StageImportPaths.Settings}에서 바꾼다)");
                return null;
            }

            log.Warn($"설정 경로 {settings.jsonPath}에 파일이 없어 {fallback}를 읽었다 (설정은 바꾸지 않음)");
            return fallback;
        }

        private static void Report(AssignmentResult assignments, FlowLog log) {
            foreach (var m in assignments.WorldMismatches)
                log.Warn($"맵 '{m.MapName}'({m.MapId}): 이름은 {m.NameWorld}월드인데 {m.SlotWorld}번 슬롯에 있다 → 이름의 id·월드를 쓴다");
            foreach (var c in assignments.IdConflicts)
                log.Warn($"스테이지 id {c.StageId}가 맵 {string.Join(", ", c.MapIds)}에 겹쳐 모두 가져오지 않는다");
        }
    }
}
