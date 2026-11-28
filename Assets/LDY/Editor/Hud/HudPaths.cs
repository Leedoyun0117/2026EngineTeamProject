using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LDY.Hud.HudEditor {
    internal static class HudPaths {
        public const string Sprites = "Assets/LDY/Sprites/ingame_hud/ingame_hud";
        public const string Manifest = Sprites + "/manifest.json";
        public const string Data = "Assets/LDY/Data/Hud";
        public const string Skin = Data + "/HudSkin.asset";
        public const string KeyMap = Data + "/HudKeyMap.asset";
        public const string PrefabFolder = "Assets/LDY/Prefab/UI";
        public const string Prefab = PrefabFolder + "/HUD.prefab";
        public const string Font = "Assets/LDY/Fonts/Galmuri9 Pixel.asset";
        public const string ThoughtFont = "Assets/LDY/Fonts/neodgm Pixel.asset";
        public const string PlayerActions = "Assets/JJB/JJBInput.inputactions";
        public const string UnlitSpriteMaterial =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        public static void EnsureFolder(string path) {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }

    internal sealed class HudLog {
        private readonly List<string> _lines = new List<string>();
        private int _warnings;

        public void Info(string line) => _lines.Add("  " + line);

        public void Warn(string line) {
            _warnings++;
            _lines.Add("  [!] " + line);
        }

        public void Print(string title) {
            var text = $"[HUD] {title}\n{string.Join("\n", _lines)}";
            if (_warnings > 0)
                Debug.LogWarning(text);
            else
                Debug.Log(text);
        }
    }
}
