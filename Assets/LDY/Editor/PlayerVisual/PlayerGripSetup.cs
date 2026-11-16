using UnityEditor;
using UnityEngine;
using static LDY.Script.PlayerVisualContext;

namespace LDY.Script {
    internal static class PlayerGripSetup {
        private const string ProfilePath = "Assets/LDY/Data/RopeGripProfile.asset";
        private const string SettingsPath = "Assets/LDY/Data/RopeSettings.asset";
        private const string PivotName = "GripPivot";
        private const string SpriteName = "Sprite";

        [MenuItem("LDY/Player Visual/Setup Rope Grip")]
        private static void Run() {
            var profile = EnsureProfile();
            var settings = AssetDatabase.LoadAssetAtPath<RopeSettings>(SettingsPath);
            if (settings == null) {
                Log($"중단: {SettingsPath} 가 없음");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try {
                var visual = root.transform.Find(PinkName);
                if (visual == null) {
                    Log($"중단: 프리팹에 {PinkName} 가 없음");
                    return;
                }

                var sprite = EnsureHierarchy(visual);
                if (sprite == null)
                    return;

                Wire(visual, sprite, profile, settings);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Log("Rope Grip 구성 완료. 프리팹 저장됨");
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static RopeGripProfile EnsureProfile() {
            var profile = AssetDatabase.LoadAssetAtPath<RopeGripProfile>(ProfilePath);
            if (profile == null) {
                profile = ScriptableObject.CreateInstance<RopeGripProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            profile.Feet = new[] {
                Foot(33, 48, 54), Foot(34, 47, 54),
                Foot(35, 48, 54), Foot(36, 48, 54), Foot(37, 48, 53), Foot(38, 48, 54),
                Foot(39, 48, 54), Foot(40, 48, 53), Foot(41, 48, 54), Foot(42, 48, 54)
            };
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static RopeGripProfile.FootEntry Foot(int frame, float x, float y) {
            return new RopeGripProfile.FootEntry { frame = frame, pixel = new Vector2(x, y) };
        }

        private static SpriteRenderer EnsureHierarchy(Transform visual) {
            var pivot = visual.Find(PivotName);
            if (pivot != null) {
                var existing = pivot.Find(SpriteName);
                return existing != null ? existing.GetComponent<SpriteRenderer>() : null;
            }

            var original = visual.GetComponent<SpriteRenderer>();
            var renderer = visual.GetComponent<PlayerRenderer>();
            if (original == null || renderer == null) {
                Log($"중단: {PinkName} 에 SpriteRenderer 또는 PlayerRenderer 가 없음");
                return null;
            }

            var pivotObject = new GameObject(PivotName) { layer = visual.gameObject.layer };
            pivotObject.transform.SetParent(visual, false);
            var spriteObject = new GameObject(SpriteName) { layer = visual.gameObject.layer };
            spriteObject.transform.SetParent(pivotObject.transform, false);

            var moved = spriteObject.AddComponent<SpriteRenderer>();
            EditorUtility.CopySerialized(original, moved);
            Object.DestroyImmediate(original);

            var so = new SerializedObject(renderer);
            so.FindProperty("target").objectReferenceValue = moved;
            so.ApplyModifiedPropertiesWithoutUndo();
            return moved;
        }

        private static void Wire(Transform visual, SpriteRenderer sprite, RopeGripProfile profile, RopeSettings settings) {
            var rig = visual.GetComponent<PlayerGripRig>();
            if (rig == null)
                rig = visual.gameObject.AddComponent<PlayerGripRig>();

            var so = new SerializedObject(rig);
            so.FindProperty("sprite").objectReferenceValue = sprite;
            so.FindProperty("gripPivot").objectReferenceValue = sprite.transform.parent;
            so.FindProperty("profile").objectReferenceValue = profile;
            so.FindProperty("settings").objectReferenceValue = settings;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
