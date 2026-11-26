using System.Linq;
using System.Text;
using LDY.Flow;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.FlowEditor {
    internal static class MergeMenu {
        [MenuItem("LDY/Merge/Create Stage Scene", priority = 0)]
        private static void CreateStageScene() => CreateStageSceneWindow.Open();

        [MenuItem("LDY/Merge/Capture Thumbnails", priority = 2)]
        private static void CaptureThumbnails() {
            var log = new FlowLog("[Merge] Capture Thumbnails");
            ThumbnailCapture.Run(log);
            log.Print();
        }

        [MenuItem("LDY/Merge/Validate Open Stage", priority = 1)]
        private static void ValidateOpenStage() {
            var scene = SceneManager.GetActiveScene();
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog);
            var checks = StageSceneValidator.Run(scene, catalog);
            var failed = 0;
            var sb = new StringBuilder($"[Merge] Validate {scene.name} ({scene.path})");
            foreach (var check in checks) {
                if (!check.Passed) failed++;
                sb.Append($"\n  [{(check.Passed ? "OK" : "X ")}] {check.Label}");
                if (!string.IsNullOrEmpty(check.Detail)) sb.Append(" - ").Append(check.Detail);
            }

            if (StageSceneValidator.IsMultiStage(scene, catalog)) failed += AppendSubStages(sb, scene, catalog);
            sb.Append(failed == 0 ? "\n  통과" : $"\n  실패 {failed}개");
            if (failed == 0) Debug.Log(sb.ToString());
            else Debug.LogWarning(sb.ToString());
        }

        private static int AppendSubStages(StringBuilder sb, Scene scene, StageCatalog catalog) {
            var rows = StageSceneValidator.RunSubStages(scene, catalog);
            sb.Append("\n  다중 스테이지 모드 (StageRoot·StageGoal 대신 루트별 검사)");
            sb.Append("\n  |    | id  | 루트 | 골 | 시작 지점 | 카메라 범위 |");
            foreach (var row in rows)
                sb.Append($"\n  | {(row.Passed ? "OK" : "X ")} | {row.Id} | {row.Root}{(row.Found ? "" : " (없음)")} | " +
                          $"{row.Goals} | {row.Spawn ?? "없음"} | {row.Bounds} |");
            return rows.Count(r => !r.Passed);
        }
    }
}
