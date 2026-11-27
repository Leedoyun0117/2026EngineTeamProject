using System.IO;
using LDY.Stage;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    internal sealed class ImportJob {
        public ImportJob(StageAssignment assignment, ImportPlan plan, StageTileMap map, CustomBlockAssets customs,
            string sceneFolder) : this(assignment, plan, map, customs, sceneFolder,
            SceneNaming.SceneName(assignment.StageId, plan.Source.Name), 1) { }

        private ImportJob(StageAssignment assignment, ImportPlan plan, StageTileMap map, CustomBlockAssets customs,
            string sceneFolder, string sceneName, int version) {
            Assignment = assignment;
            Plan = plan;
            Map = map;
            Customs = customs;
            SceneFolder = sceneFolder;
            SceneName = sceneName;
            Version = version;
        }

        public StageAssignment Assignment { get; }
        public ImportPlan Plan { get; }
        public StageTileMap Map { get; }
        public CustomBlockAssets Customs { get; }
        public string SceneFolder { get; }
        public string SceneName { get; }

        // 1이면 기본 이름, 2 이상이면 "_v{n}"을 붙인 새 씬. 새 이름 씬은 카탈로그를 건드리지 않는다.
        public int Version { get; }
        public bool IsVersioned => Version > 1;
        public string ScenePath => $"{SceneFolder}/{SceneName}.unity";
        public bool SceneExists => File.Exists(ScenePath);
        public string Label => $"{Assignment.StageId} {Plan.Source.Name} ({Assignment.MapId})";

        // 기존 씬은 그대로 두고 Stage{id}_{이름}_v2, _v3 … 로 새로 만드는 작업.
        public ImportJob AsNewVersion() {
            var baseName = SceneNaming.SceneName(Assignment.StageId, Plan.Source.Name);
            var name = SceneNaming.NextFreeVersion(SceneFolder, baseName, out var version);
            return new ImportJob(Assignment, Plan, Map, Customs, SceneFolder, name, version);
        }
    }
}
