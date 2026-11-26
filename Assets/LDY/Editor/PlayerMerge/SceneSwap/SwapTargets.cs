using System.Collections.Generic;
using LDY.PlayerMerge;

namespace LDY.Script {
    internal readonly struct SwapScene {
        public SwapScene(string path, string owner) {
            Path = path;
            Owner = owner;
        }

        public string Path { get; }
        public string Owner { get; }
    }

    internal static class SwapTargets {
        public static readonly SwapScene[] Scenes = {
            new("Assets/JJB/JJBLevel.unity", "지빈"),
            new("Assets/JJB/JJBScene.unity", "지빈"),
            new("Assets/LDY/Scenes/Stage1_ArtTest.unity", "LDY"),
            new("Assets/HTY/00.Scenes/Main_HTY.unity", "태영"),
            new("Assets/LDY/Scenes/RopeTest.unity", "LDY"),
            new("Assets/LDY/Scenes/Stage2_SpiderTest.unity", "LDY"),
            new("Assets/HES/Scene/Test.unity", "HES")
        };

        public static readonly HashSet<string> OldPrefabs = new() {
            PlayerPrefabPaths.Jibin, PlayerPrefabPaths.Hty, PlayerPrefabPaths.LegacyShared
        };
    }
}
