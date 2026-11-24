using UnityEditor;
using UnityEngine;

namespace LDY.PixelStyleEditor
{
    public static class PixelStyleMenu
    {
        private const string Root = "LDY/Pixel Style/Gear/";

        [MenuItem(Root + "1. Build Animation (clip + controller)")]
        private static void BuildAnimation()
        {
            AssetDatabase.Refresh();
            if (!GearAnimationBuilder.TryLoadFrames(out Sprite[] frames)) return;
            GearAnimationBuilder.Build(frames);
            Debug.Log("[PixelStyle] 클립/컨트롤러 갱신: " + PixelGearPaths.ClipPath + ", " + PixelGearPaths.ControllerPath);
        }

        [MenuItem(Root + "2. Apply To Stage1_Gear Prefab")]
        private static void ApplyToPrefab()
        {
            if (!GearAnimationBuilder.TryLoadFrames(out Sprite[] frames)) return;
            RuntimeAnimatorController controller =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PixelGearPaths.ControllerPath);
            if (controller == null)
            {
                Debug.LogError("[PixelStyle] 컨트롤러가 없다. 먼저 '1. Build Animation'을 실행할 것.");
                return;
            }
            GearPrefabSwapper.Apply(frames[0], controller);
            AssetDatabase.SaveAssets();
        }

        [MenuItem(Root + "Revert Stage1_Gear Prefab To Original")]
        private static void Revert()
        {
            GearPrefabSwapper.Revert();
            AssetDatabase.SaveAssets();
        }
    }
}
