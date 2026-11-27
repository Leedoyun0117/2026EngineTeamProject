using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // LDY/Player/Merge/1. Build Final Player. A·B·C는 읽기만 하고, 새 최종본과 PlayerCopy Variant만 저장한다.
    internal static class FinalPlayerBuilder {
        private const string Title = "[PlayerMerge] 1. Build Final Player";
        private const string SpritePath = PlayerParts.Visual + "/GripPivot/Sprite";

        public static void Run() {
            var log = new MergeLog();
            var ok = EnsureFinalFile(log) && BuildFinal(log) && CopyVariantBuilder.Run(log) && CopySkillLink.Run(log);
            AssetDatabase.SaveAssets();
            log.Print(Title, ok);
        }

        private static bool EnsureFinalFile(MergeLog log) {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Final) != null)
                return true;

            using var shared = PrefabContents.Open(PlayerPrefabPaths.LegacyShared, log);
            if (shared == null)
                return false;

            PrefabUtility.SaveAsPrefabAsset(shared.Root, PlayerPrefabPaths.Final);
            log.Changed.Add($"+{PlayerPrefabPaths.Final} (C {PlayerPrefabPaths.LegacyShared}의 복사본으로 시작, C는 바꾸지 않음)");
            return true;
        }

        private static bool BuildFinal(MergeLog log) {
            using var jibin = PrefabContents.Open(PlayerPrefabPaths.Jibin, log);
            using var hty = PrefabContents.Open(PlayerPrefabPaths.Hty, log);
            using var final = PrefabContents.Open(PlayerPrefabPaths.Final, log);
            if (jibin == null || hty == null || final == null)
                return false;

            var sprite = final.Root.transform.Find(SpritePath)?.GetComponent<SpriteRenderer>();
            if (sprite == null) {
                log.Notes.Add($"중단: 최종본에 {SpritePath}의 SpriteRenderer가 없음");
                return false;
            }

            var before = log.Changed.Count;
            if (!new JibinValuesStep(jibin.Root, final.Root, log).Run())
                return false;

            var copyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Copy);
            new HtyBundleStep(hty.Root, final.Root, copyPrefab, log).Run();
            new VisualStep(jibin.Root, final.Root, log).Run();
            new StageComponents(final.Root, sprite, log).Run();
            new VisualPlacer(final.Root, sprite, log).Run();

            if (log.Changed.Count > before)
                final.Save();

            return true;
        }
    }
}
