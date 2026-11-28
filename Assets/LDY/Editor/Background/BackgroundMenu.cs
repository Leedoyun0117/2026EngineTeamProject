using LDY.Background;
using UnityEditor;

namespace LDY.BackgroundEditor {
    internal static class BackgroundMenu {
        [MenuItem("LDY/Background/Import Art")]
        private static void ImportArt() {
            var log = new BackgroundBuildLog();
            Import(log);
            AssetDatabase.SaveAssets();
            log.Print("LDY Background Import Art");
        }

        [MenuItem("LDY/Background/Build Stage Backgrounds")]
        private static void BuildStageBackgrounds() {
            var log = new BackgroundBuildLog();
            Import(log);
            foreach (var source in BackgroundSources.All()) Build(source, log);
            AssetDatabase.SaveAssets();
            log.Print("LDY Background Build");
        }

        [MenuItem("LDY/Background/Add To Open Scene")]
        private static void AddToOpenScene() => BackgroundSceneWindow.Open();

        [MenuItem("LDY/Background/Fit To Map (Open Scene)")]
        private static void FitToMap() {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var fitted = BackgroundMapFitter.FitAll(scene);
            if (fitted > 0) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            else UnityEngine.Debug.LogWarning($"[Background] {scene.path}: 맞춘 배경이 없다 (배경 없음 또는 bounds 없음)");
        }

        private static void Import(BackgroundBuildLog log) {
            var importer = new BackgroundArtImporter();
            importer.EnsureWhitePixel(log);
            foreach (var source in BackgroundSources.All())
                if (BackgroundSources.TryRead(source, log, out var layout))
                    importer.Import(layout, source.PixelsPerUnit, log);
        }

        private static void Build(StageBackgroundSource source, BackgroundBuildLog log) {
            if (!BackgroundSources.TryRead(source, log, out var layout)) return;
            var textSet = new BackgroundTextSetBuilder().Build(source, layout, log);
            new BackgroundPrefabBuilder().Build(source, layout, textSet, log);
        }
    }
}
