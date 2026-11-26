using System.Collections.Generic;
using LDY.FlowEditor;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    // 설정의 JSON을 읽어 슬롯 순서대로 작업 목록을 만든다.
    internal sealed class ImportSource {
        private ImportSource(StageImportSettings settings, CursorStageFile file, AssignmentResult assignments,
            List<ImportJob> jobs) {
            Settings = settings;
            File = file;
            Assignments = assignments;
            Jobs = jobs;
        }

        public StageImportSettings Settings { get; }
        public CursorStageFile File { get; }
        public AssignmentResult Assignments { get; }
        public IReadOnlyList<ImportJob> Jobs { get; }

        public static ImportSource Load(FlowLog log) {
            var settings = StageImportPaths.LoadSettings();
            if (!System.IO.File.Exists(settings.jsonPath)) {
                log.Warn($"툴 내보내기 파일이 없다: {settings.jsonPath} (경로는 {StageImportPaths.Settings}에서 바꾼다)");
                return null;
            }

            CursorStageFile file;
            try {
                file = CursorStageParser.Parse(System.IO.File.ReadAllText(settings.jsonPath));
            }
            catch (StageFileException e) {
                log.Warn($"{settings.jsonPath}: {e.Message}");
                return null;
            }

            var assignments = StageIdAssigner.Assign(file, settings.IdRule);
            var jobs = new List<ImportJob>();
            foreach (var assignment in assignments.Assigned) {
                var map = WorldMapStore.Ensure(assignment.World, log);
                var plan = ImportPlanner.Plan(file.FindStage(assignment.MapId), file, new MappingRules(map));
                jobs.Add(new ImportJob(assignment, plan, map, settings.sceneFolder.TrimEnd('/')));
            }

            return new ImportSource(settings, file, assignments, jobs);
        }
    }
}
