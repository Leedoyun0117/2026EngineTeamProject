using System.IO;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    internal static class PlayerDeathSettingsBuilder {
        private const string AssetPath = "Assets/LDY/Resources/" + PlayerDeathSettings.ResourcePath + ".asset";
        private const string AnimSetPath = "Assets/LDY/Data/PlayerAnimSet_Cursor.asset";

        public static void Run() {
            var animSet = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(AnimSetPath);
            if (animSet == null) {
                Debug.LogError("[PlayerDeath] PlayerAnimSet을 찾지 못했다: " + AnimSetPath);
                return;
            }

            var settings = AssetDatabase.LoadAssetAtPath<PlayerDeathSettings>(AssetPath);
            if (settings == null) {
                Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
                settings = ScriptableObject.CreateInstance<PlayerDeathSettings>();
                AssetDatabase.CreateAsset(settings, AssetPath);
            }

            var so = new SerializedObject(settings);
            so.FindProperty("animSet").objectReferenceValue = animSet;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            Debug.Log($"[PlayerDeath] {AssetPath} ← {AnimSetPath} " +
                      $"(trapped={animSet.Has(PlayerAnimId.Trapped)}, death={animSet.Has(PlayerAnimId.Death)})", settings);
        }
    }
}
