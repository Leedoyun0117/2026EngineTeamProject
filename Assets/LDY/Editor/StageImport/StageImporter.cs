using LDY.BackgroundEditor;
using LDY.PlayerMerge;
using LDY.FlowEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LDY.StageImportEditor {
    // 새 씬만 만든다. 같은 경로에 씬이 있으면 건너뛴다(새 이름으로 가져오기는 ImportJob.AsNewVersion).
    internal static class StageImporter {
        private const string ValidateMenu = "LDY/Merge/Validate Open Stage";
        private const string ProgressTitle = "Stage Import";

        private static readonly (string label, ISceneStep step)[] Steps = {
            ("타일", new TilemapStep()),
            ("프리팹", new PrefabStep()),
            ("시작·골", new StartGoalStep()),
            ("영역", new AreaStep()),
            ("메모", new NoteStep()),
            ("카메라·범위", new FrameStep())
        };

        // progress: 여러 맵을 연달아 만들 때 전체 진행(0~1) 중 이 맵의 시작·끝.
        public static bool Import(ImportJob job, FlowLog log, float progressFrom = 0f, float progressTo = 1f) {
            if (job.SceneExists) {
                log.Warn($"{job.Label}: {job.ScenePath}가 이미 있어 건너뜀");
                return false;
            }

            Progress(job, "템플릿 씬", 0, progressFrom, progressTo);
            var request = new StageSceneRequest {
                SceneName = job.SceneName, StageId = job.Assignment.StageId, Folder = job.SceneFolder,
                PlayerPrefabPath = PlayerPrefabPaths.Final, LinkCatalog = !job.IsVersioned
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
            for (var i = 0; i < Steps.Length; i++) {
                Progress(job, Steps[i].label, i + 1, progressFrom, progressTo);
                Steps[i].step.Apply(context);
            }

            BackgroundMapFitter.FitAll(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.Info(job.IsVersioned
                ? $"새 이름 v{job.Version}: 기존 씬과 카탈로그는 그대로 둠"
                : "카탈로그: " + CatalogStep.Describe(job.Assignment.StageId, path));
            return true;
        }

        public static void ValidateOpenScene() => EditorApplication.ExecuteMenuItem(ValidateMenu);

        private static void Progress(ImportJob job, string label, int index, float from, float to) {
            var t = from + (to - from) * index / (Steps.Length + 1f);
            EditorUtility.DisplayProgressBar(ProgressTitle,
                $"{job.Assignment.StageId} {job.Plan.Width}x{job.Plan.Height}: {label}", t);
        }
    }
}
