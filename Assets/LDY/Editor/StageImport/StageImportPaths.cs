using LDY.FlowEditor;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    internal static class StageImportPaths {
        public const string DefaultJson = "Assets/LDY/StageData/stages.json";
        public const string DefaultSceneFolder = "Assets/LDY/Scenes/Imported";
        public const string Data = "Assets/LDY/Data/StageImport";
        public const string Settings = Data + "/StageImportSettings.asset";
        public const string CustomMap = Data + "/CustomBlockMap.asset";

        // 설정 경로에 파일이 없을 때 차례로 찾아보는 곳. 실제 내보내기 파일이 Data/stages1.json에 있다.
        public static readonly string[] JsonCandidates = { DefaultJson, "Assets/LDY/Data/stages1.json" };
        public const string LegacyTileMap = "Assets/LDY/Data/Stage/StageTileMap.asset";
        public const string TempPrefabs = "Assets/LDY/Prefabs/StageImport";
        public const string TempSprite = "Assets/LDY/Sprites/Square.png";

        // 흡수 픽업 그림 ui_{능력}.aseprite/.png를 차례로 찾는 폴더(읽기만 한다).
        public static readonly string[] PickupArtFolders = {
            "Assets/LDY/Sprites/stage1_music_objects/stage1_music_objects",
            "Assets/Main/04.Sprites"
        };

        public static string WorldMap(int world) => $"{Data}/StageTileMap_World{world}.asset";
        public static string TempPrefab(string key) => $"{TempPrefabs}/Temp_{key}.prefab";

        public static StageImportSettings LoadSettings() {
            var settings = AssetDatabase.LoadAssetAtPath<StageImportSettings>(Settings);
            if (settings != null) return settings;
            FlowPaths.EnsureFolder(Data);
            settings = ScriptableObject.CreateInstance<StageImportSettings>();
            settings.jsonPath = FirstExistingJson() ?? DefaultJson;
            AssetDatabase.CreateAsset(settings, Settings);
            AssetDatabase.SaveAssets();
            return settings;
        }

        public static string FirstExistingJson() {
            foreach (var path in JsonCandidates)
                if (System.IO.File.Exists(path))
                    return path;
            return null;
        }
    }
}
