using System.Collections.Generic;
using LDY.FlowEditor;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    internal static class StageImportMenu {
        private const string Root = "LDY/Stage Import/";

        [MenuItem(Root + "Preview", priority = 0)]
        private static void Preview() {
            var log = new FlowLog("[Stage Import] Preview");
            var source = ImportSource.Load(log);
            if (source != null) Debug.Log("[Stage Import] Preview\n" + PreviewReport.Build(source, log));
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
            if (!EditorUtility.DisplayDialog("Import All From Slots",
                    $"슬롯의 맵 {source.Jobs.Count}개로 새 씬을 만든다. 이미 있는 씬은 건너뛴다.", "만들기", "취소"))
                return;
            Run(source.Jobs);
        }

        public static void Run(IEnumerable<ImportJob> jobs) {
            foreach (var job in jobs) {
                var log = new FlowLog("[Stage Import] " + job.Label);
                var made = StageImporter.Import(job, log);
                log.Print();
                if (made) StageImporter.ValidateOpenScene();
            }

            AssetDatabase.SaveAssets();
        }
    }
}
