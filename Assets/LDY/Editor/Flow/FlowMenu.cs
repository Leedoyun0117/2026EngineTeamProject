using LDY.Flow;
using UnityEditor;

namespace LDY.FlowEditor {
    internal static class FlowMenu {
        [MenuItem("LDY/Flow/Build Flow Assets", priority = 0)]
        private static void BuildFlowAssets() {
            var log = new FlowLog("[Flow] Build Flow Assets");
            try {
                var catalog = StageCatalogBuilder.Build(log);
                var skin = UiSkinBuilder.Build(log);
                FlowSettingsBuilder.Build(catalog, skin, log);
            }
            finally {
                AssetDatabase.SaveAssets();
                log.Print();
            }
        }

        [MenuItem("LDY/Flow/Build Stage Select Scene", priority = 1)]
        private static void BuildStageSelectScene() {
            var log = new FlowLog("[Flow] Build Stage Select Scene");
            StageSelectSceneBuilder.Build(log);
            log.Print();
        }

        [MenuItem("LDY/Flow/Sync Build Settings", priority = 1)]
        private static void SyncBuildSettings() {
            var log = new FlowLog("[Flow] Sync Build Settings");
            BuildSettingsSync.Run(AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog), log);
            log.Print();
        }

        [MenuItem("LDY/Flow/Connect Title To Stage Select", priority = 2)]
        private static void ConnectTitle() {
            var log = new FlowLog("[Flow] Connect Title");
            TitleConnector.Run(log);
            log.Print();
        }

        [MenuItem("LDY/Flow/Unlock All", priority = 20)]
        private static void UnlockAll() => ProgressDebugActions.UnlockAll();

        [MenuItem("LDY/Flow/Reset Progress", priority = 21)]
        private static void ResetProgress() => ProgressDebugActions.Reset();
    }
}
