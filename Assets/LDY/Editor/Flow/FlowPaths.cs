using System.IO;
using UnityEditor;

namespace LDY.FlowEditor {
    internal static class FlowPaths {
        public const string Data = "Assets/LDY/Data/Flow";
        public const string Catalog = Data + "/StageCatalog.asset";
        public const string Skin = Data + "/UiSkin.asset";
        public const string Settings = "Assets/LDY/Resources/LDY/FlowSettings.asset";
        public const string PickupCatalog = "Assets/LDY/Data/Stage/Stage1PickupCatalog.asset";
        public const string TitleScene = "Assets/Scenes/LDY_TitleScene.unity";
        public const string SelectScene = "Assets/LDY/Scenes/StageSelect.unity";
        public const string StageSceneFolder = "Assets/Scenes/Stages";
        public const string Thumbnails = "Assets/LDY/Art/Thumbnails";
        public const string SelectSprites = "Assets/LDY/Sprites/stage_select_ui/stage_select_ui";
        public const string HudSprites = "Assets/LDY/Sprites/ingame_hud/ingame_hud";
        public const string FontAsset = "Assets/LDY/Fonts/Galmuri9 Pixel.asset";
        public const string AudioMixer = "Assets/JJB/JJBAudioMixer.mixer";
        public const string GoalVisual = "Assets/LDY/Prefabs/Stage1/Stage1_GoalMusicFile.prefab";

        public const string AltPlayer = "Assets/LDY/Prefab/AltPlayer.prefab";
        public const string HudPrefab = "Assets/LDY/Prefab/UI/HUD.prefab";

        public static void EnsureFolder(string path) {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
