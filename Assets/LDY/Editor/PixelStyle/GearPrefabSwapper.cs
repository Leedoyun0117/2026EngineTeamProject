using UnityEditor;
using UnityEngine;

namespace LDY.PixelStyleEditor
{
    /// <summary>Stage1_Gear 프리팹의 스프라이트/컨트롤러 참조만 교체한다. 콜라이더, 크기, pivot은 건드리지 않는다.</summary>
    public static class GearPrefabSwapper
    {
        public static void Apply(Sprite firstFrame, RuntimeAnimatorController controller)
        {
            Swap(firstFrame, controller);
        }

        public static void Revert()
        {
            if (!OriginalGearReferences.TryResolve(out Sprite sprite, out RuntimeAnimatorController controller)) return;
            Swap(sprite, controller);
        }

        private static void Swap(Sprite sprite, RuntimeAnimatorController controller)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PixelGearPaths.PrefabPath);
            try
            {
                SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                Animator animator = root.GetComponent<Animator>();
                if (renderer == null || animator == null)
                {
                    Debug.LogError("[PixelStyle] SpriteRenderer/Animator 없음: " + PixelGearPaths.PrefabPath);
                    return;
                }

                bool changed = false;
                changed |= Replace(PixelGearPaths.PrefabPath + " SpriteRenderer.sprite", renderer.sprite, sprite, () => renderer.sprite = sprite);
                changed |= Replace(PixelGearPaths.PrefabPath + " Animator.controller", animator.runtimeAnimatorController, controller, () => animator.runtimeAnimatorController = controller);

                if (changed) PrefabUtility.SaveAsPrefabAsset(root, PixelGearPaths.PrefabPath);
                else Debug.Log("[PixelStyle] 이미 적용됨, 변경 없음: " + PixelGearPaths.PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool Replace(string label, Object current, Object next, System.Action assign)
        {
            if (current == next) return false;
            assign();
            Debug.Log("[PixelStyle] 교체: " + label + "  " + Describe(current) + " -> " + Describe(next));
            return true;
        }

        private static string Describe(Object o)
        {
            return o == null ? "(none)" : o.name + " [" + AssetDatabase.GetAssetPath(o) + "]";
        }
    }
}
