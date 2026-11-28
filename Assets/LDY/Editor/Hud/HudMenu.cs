using UnityEditor;

namespace LDY.Hud.HudEditor {
    internal static class HudMenu {
        private const string Root = "LDY/HUD/";

        [MenuItem(Root + "1. Import Art", priority = 0)]
        private static void ImportArt() => Run("1. Import Art", HudArtImporter.Run);

        [MenuItem(Root + "2. Build Skin And Key Map", priority = 1)]
        private static void BuildAssets() => Run("2. Build Skin And Key Map", HudAssetsBuilder.Run);

        [MenuItem(Root + "3. Build Prefab (Add Missing Only)", priority = 2)]
        private static void BuildPrefab() => Run("3. Build Prefab", HudPrefabBuilder.Run);

        [MenuItem(Root + "4. Add HUD To Open Scene", priority = 3)]
        private static void AddToScene() => Run("4. Add HUD To Open Scene", HudSceneInstaller.AddToOpenScene);

        [MenuItem(Root + "Run 1-3", priority = 20)]
        private static void RunAll() {
            var log = new HudLog();
            HudArtImporter.Run(log);
            HudAssetsBuilder.Run(log);
            HudPrefabBuilder.Run(log);
            log.Print("Run 1-3");
        }

        [MenuItem(Root + "Reapply Skin Fonts To Prefab (Overwrites Fonts)", priority = 40)]
        private static void ApplyFonts() => Run("Reapply Skin Fonts", HudFontApplier.Run);

        private static void Run(string title, System.Action<HudLog> step) {
            var log = new HudLog();
            step(log);
            log.Print(title);
        }
    }
}
