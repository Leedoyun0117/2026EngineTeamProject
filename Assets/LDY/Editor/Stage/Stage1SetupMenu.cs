using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LDY.StageEditor {
    internal static class Stage1SetupMenu {
        private const string Title = "LDY Stage1 Setup";

        [MenuItem("LDY/Stage1/Setup Art")]
        private static void SetupArt() {
            var report = new SetupReport();
            try {
                Run(report);
            }
            finally {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                report.Print(Title);
            }
        }

        [MenuItem("LDY/Stage1/Build ArtTest Scene")]
        private static void BuildArtTestScene() {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var report = new SetupReport();
            new Stage1ArtTestSceneBuilder().Build(report);
            AssetDatabase.SaveAssets();
            report.Print("LDY Stage1 ArtTest");
        }

        private static void Run(SetupReport report) {
            StagePaths.EnsureFolder(StagePaths.Prefabs);
            StagePaths.EnsureFolder(StagePaths.Tiles);
            StagePaths.EnsureFolder(StagePaths.Data);

            var locator = new ArtLocator();
            var tilesetPath = locator.Find("tileset_stage1.png");
            if (tilesetPath == null) {
                report.Warn("tileset_stage1.png를 찾지 못했다");
                return;
            }

            if (!TilesetImporter.HasExpectedSize(tilesetPath)) {
                report.Warn("tileset_stage1.png 크기가 160x240이 아니라 중단했다");
                return;
            }

            new TilesetImporter().Import(tilesetPath);
            report.Updated("Tileset import " + tilesetPath);
            ConfigureAsepriteFiles(locator, report);
            AssetDatabase.Refresh();

            var tiles = new Stage1TileBuilder().Build(tilesetPath, report);
            if (tiles == null) return;
            AssetDatabase.SaveAssets();

            new PaletteBuilder().Build(tiles, report);
            new Stage1GridBuilder().Build(report);

            var art = new AsepriteArtLoader(locator);
            new VariantBuilder(new VariantContext(art)).Build(report);
            new PlaceholderBuilder(art).Build(report);
            new StageTileMapBuilder().Build(tiles, report);
        }

        private static void ConfigureAsepriteFiles(ArtLocator locator, SetupReport report) {
            var folder = locator.Folder();
            if (folder == null) {
                report.Warn("manifest.json 위치를 찾지 못해 aseprite 임포트 설정을 건너뜀");
                return;
            }

            var configurator = new AsepriteConfigurator();
            foreach (var file in System.IO.Directory.GetFiles(folder, "*.aseprite")) {
                var path = file.Replace('\\', '/');
                if (configurator.Configure(path)) report.Updated("Aseprite import " + path);
            }
        }
    }
}
