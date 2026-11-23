using UnityEditor;
using UnityEngine;

namespace LDY.PixelStyleEditor
{
    /// <summary>gear.aseprite 안의 원본 스프라이트와 컨트롤러를 찾는다.</summary>
    public static class OriginalGearReferences
    {
        public static bool TryResolve(out Sprite sprite, out RuntimeAnimatorController controller)
        {
            sprite = null;
            controller = null;

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(PixelGearPaths.OriginalAsepritePath))
            {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string _, out long fileId)) continue;
                if (fileId == PixelGearPaths.OriginalSpriteFileId) sprite = asset as Sprite;
                else if (fileId == PixelGearPaths.OriginalControllerFileId) controller = asset as RuntimeAnimatorController;
            }

            bool ok = sprite != null && controller != null;
            if (!ok) Debug.LogError("[PixelStyle] 원본 gear 참조를 찾지 못했다: " + PixelGearPaths.OriginalAsepritePath);
            return ok;
        }
    }
}
