using System.IO;
using LDY.Stage;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    internal sealed class ImportJob {
        public ImportJob(StageAssignment assignment, ImportPlan plan, StageTileMap map, string sceneFolder) {
            Assignment = assignment;
            Plan = plan;
            Map = map;
            SceneFolder = sceneFolder;
            SceneName = SceneNaming.SceneName(assignment.StageId, plan.Source.Name);
        }

        public StageAssignment Assignment { get; }
        public ImportPlan Plan { get; }
        public StageTileMap Map { get; }
        public string SceneFolder { get; }
        public string SceneName { get; }
        public string ScenePath => $"{SceneFolder}/{SceneName}.unity";
        public bool SceneExists => File.Exists(ScenePath);
        public string Label => $"{Assignment.StageId} {Plan.Source.Name} ({Assignment.MapId})";
    }
}
