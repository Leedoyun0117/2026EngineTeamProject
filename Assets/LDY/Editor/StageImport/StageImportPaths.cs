using LDY.FlowEditor;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    internal static class StageImportPaths {
        public const string DefaultJson = "Assets/LDY/StageData/stages.json";
        public const string DefaultSceneFolder = "Assets/LDY/Scenes/Imported";
        public const string Data = "Assets/LDY/Data/StageImport";
        public const string Settings = Data + "/StageImportSettings.asset";
        public const string LegacyTileMap = "Assets/LDY/Data/Stage/StageTileMap.asset";
        public const string TempPrefabs = "Assets/LDY/Prefabs/StageImport";
        public const string TempSprite = "Assets/LDY/Sprites/Square.png";

        public static string WorldMap(int world) => $"{Data}/StageTileMap_World{world}.asset";
        public static string TempPrefab(string key) => $"{TempPrefabs}/Temp_{key}.prefab";

        public static StageImportSettings LoadSettings() {
            var settings = AssetDatabase.LoadAssetAtPath<StageImportSettings>(Settings);
            if (settings != null) return settings;
            FlowPaths.EnsureFolder(Data);
            settings = ScriptableObject.CreateInstance<StageImportSettings>();
            AssetDatabase.CreateAsset(settings, Settings);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
