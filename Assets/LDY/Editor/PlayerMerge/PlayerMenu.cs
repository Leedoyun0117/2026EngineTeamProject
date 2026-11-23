using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    internal static class PlayerMenu {
        private const string Root = "LDY/Player/";

        [MenuItem(Root + "Compare Prefabs")]
        private static void Compare() {
            PlayerCompareReport.Run();
        }

        [MenuItem(Root + "Merge Into One")]
        private static void Merge() {
            PlayerMergeRunner.Run();
        }

        [MenuItem(Root + "Replace Scene Instances")]
        private static void Replace() {
            var report = new List<string>();
            SceneInstanceReplacer.Run(report);
            AssetDatabase.SaveAssets();
            Debug.Log("[PlayerMerge] 씬 인스턴스 교체\n- " + string.Join("\n- ", report));
        }
    }
}
