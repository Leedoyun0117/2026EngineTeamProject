using System.Collections.Generic;
using System.Linq;
using LDY.FlowEditor;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    internal static class StageImportMenu {
        private const string Root = "LDY/Stage Import/";
        private const string OwnSceneRoot = "Assets/LDY/";

        [MenuItem(Root + "Preview", priority = 0)]
        private static void Preview() {
            var log = new FlowLog("[Stage Import] Preview");
            var source = ImportSource.Load(log);
            if (source != null) Debug.Log("[Stage Import] Preview\n" + PreviewReport.Build(source, log));
            log.Print();
        }

        [MenuItem(Root + "Refresh Temp Absorb Prefabs", priority = 10)]
        private static void RefreshTempAbsorbPrefabs() {
            var log = new FlowLog("[Stage Import] Refresh Temp Absorb Prefabs");
            TempPrefabFactory.RefreshAll(log);
            log.Print();
        }

        [MenuItem(Root + "Disable Unlinked (Open Scene)", priority = 11)]
        private static void DisableUnlinked() {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.path.StartsWith(OwnSceneRoot) && !EditorUtility.DisplayDialog("Disable Unlinked",
                    $"{scene.path}은(는) {OwnSceneRoot} 밖의 씬이다. 이 씬의 주인이 맞나?", "진행", "취소"))
                return;
            var log = new FlowLog("[Stage Import] Disable Unlinked " + scene.path);
            var map = CustomBlockMapStore.Ensure(null, log);
            var count = LinkSweep.Run(scene, map, LinkSweep.FindInScene(scene, map).ToList(), log);
            if (count > 0) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            log.Info("저장은 직접 (Ctrl+S)");
            log.Print();
        }

        [MenuItem(Root + "Import Selected", priority = 1)]
        private static void ImportSelected() => ImportSelectedWindow.Open();

        [MenuItem(Root + "Import All From Slots", priority = 2)]
        private static void ImportAll() {
            var log = new FlowLog("[Stage Import] Load");
            var source = ImportSource.Load(log);
            log.Print();
            if (source == null) return;

            var existing = source.Jobs.Count(j => j.SceneExists);
            var jobs = source.Jobs.ToList();
            if (existing == 0) {
                if (!EditorUtility.DisplayDialog("Import All From Slots",
                        $"슬롯의 맵 {jobs.Count}개로 새 씬을 만든다.", "만들기", "취소"))
                    return;
            }
            else {
                // 0: 건너뛰기, 1: 취소, 2: 새 이름으로
                var choice = EditorUtility.DisplayDialogComplex("Import All From Slots",
                    $"슬롯의 맵 {jobs.Count}개 중 {existing}개는 같은 이름 씬이 이미 있다. 기존 씬은 어느 쪽이든 그대로 둔다.\n" +
                    "· 건너뛰기: 없는 것만 만든다\n· 새 이름으로: 있는 것은 Stage{id}_{이름}_v2, _v3 … 로 따로 만든다",
                    "건너뛰기", "취소", "새 이름으로");
                if (choice == 1) return;
                if (choice == 2) jobs = jobs.Select(j => j.SceneExists ? j.AsNewVersion() : j).ToList();
            }

            Run(jobs);
        }

        public static void Run(IReadOnlyList<ImportJob> jobs) {
            try {
                for (var i = 0; i < jobs.Count; i++) {
                    var job = jobs[i];
                    var log = new FlowLog("[Stage Import] " + job.Label);
                    var made = StageImporter.Import(job, log, (float)i / jobs.Count, (i + 1f) / jobs.Count);
                    log.Print();
                    if (made) StageImporter.ValidateOpenScene();
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
        }
    }
}
