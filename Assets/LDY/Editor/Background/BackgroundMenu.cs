using LDY.Background;
using UnityEditor;

namespace LDY.BackgroundEditor {
    internal static class BackgroundMenu {
        [MenuItem("LDY/Background/Import Art")]
        private static void ImportArt() {
            var log = new BackgroundBuildLog();
            var importer = new BackgroundArtImporter();
            importer.EnsureWhitePixel(log);
            foreach (var source in BackgroundSources.All())
                if (BackgroundSources.TryRead(source, log, out var layout))
                    importer.Import(layout, source.PixelsPerUnit, log);
            AssetDatabase.SaveAssets();
            log.Print("LDY Background Import Art");
        }

        [MenuItem("LDY/Background/Build Stage Backgrounds")]
        private static void BuildStageBackgrounds() {
            var log = new BackgroundBuildLog();
            foreach (var source in BackgroundSources.All()) Build(source, log);
            AssetDatabase.SaveAssets();
            log.Print("LDY Background Build");
        }

        [MenuItem("LDY/Background/Add To Open Scene")]
        private static void AddToOpenScene() => BackgroundSceneWindow.Open();

        private static void Build(StageBackgroundSource source, BackgroundBuildLog log) {
            if (!BackgroundSources.TryRead(source, log, out var layout)) return;
            var textSet = new BackgroundTextSetBuilder().Build(source, layout, log);
            new BackgroundPrefabBuilder().Build(source, layout, textSet, log);
        }
    }
}
