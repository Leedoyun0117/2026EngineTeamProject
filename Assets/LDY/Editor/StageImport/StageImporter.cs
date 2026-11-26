using LDY.PlayerMerge;
using LDY.FlowEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LDY.StageImportEditor {
    // 새 씬만 만든다. 같은 경로에 씬이 있으면 건너뛴다.
    internal static class StageImporter {
        private const string ValidateMenu = "LDY/Merge/Validate Open Stage";

        private static readonly ISceneStep[] Steps = {
            new TilemapStep(),
            new PrefabStep(),
            new StartGoalStep(),
            new AreaStep(),
            new NoteStep(),
            new FrameStep()
        };

        public static bool Import(ImportJob job, FlowLog log) {
            if (job.SceneExists) {
                log.Warn($"{job.Label}: {job.ScenePath}가 이미 있어 건너뜀");
                return false;
            }

            var request = new StageSceneRequest {
                SceneName = job.SceneName, StageId = job.Assignment.StageId, Folder = job.SceneFolder,
                PlayerPrefabPath = PlayerPrefabPaths.Final, LinkCatalog = true
            };
            var path = StageSceneBuilder.Build(request, log);
            if (path == null) {
                log.Warn($"{job.Label}: 열린 씬 저장을 취소해서 중단");
                return false;
            }

            var scene = SceneManager.GetActiveScene();
            if (scene.path != path) {
                log.Warn($"{job.Label}: 만든 씬이 열려 있지 않다 ({scene.path})");
                return false;
            }

            var context = new SceneContext(scene, job, new MapAssets(job.Map, job.Assignment.World, log), log);
            foreach (var step in Steps) step.Apply(context);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.Info("카탈로그: " + CatalogStep.Describe(job.Assignment.StageId, path));
            return true;
        }

        public static void ValidateOpenScene() => EditorApplication.ExecuteMenuItem(ValidateMenu);
    }
}
