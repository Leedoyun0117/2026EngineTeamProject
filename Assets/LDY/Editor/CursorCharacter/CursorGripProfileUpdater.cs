using UnityEditor;

namespace LDY.Script {
    internal static class CursorGripProfileUpdater {
        public const string ProfilePath = "Assets/LDY/Data/RopeGripProfile.asset";

        public static void Apply(RopeGripProfile profile) {
            var so = new SerializedObject(profile);
            so.FindProperty("handPixel").vector2Value = CursorGripData.HandPixel;
            so.ApplyModifiedPropertiesWithoutUndo();

            profile.Feet = CursorGripData.Feet();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        public static bool Run(System.Collections.Generic.List<string> report) {
            var profile = AssetDatabase.LoadAssetAtPath<RopeGripProfile>(ProfilePath);
            if (profile == null) {
                report.Add($"중단: {ProfilePath} 가 없음. 'LDY/Player Visual/Setup Rope Grip'을 먼저 실행하세요");
                return false;
            }

            Apply(profile);
            report.Add($"RopeGripProfile: 손 {CursorGripData.HandPixel}, 발 {CursorGripData.FootPixel}, "
                       + $"프레임 {CursorGripData.FirstRopeFrame}~{CursorGripData.FirstRopeFrame + CursorGripData.RopeFrameCount - 1}");
            return true;
        }
    }
}
