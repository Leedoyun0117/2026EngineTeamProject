using System;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.Script {
    // 최종본의 Variant로 PlayerCopy를 만든다. B'(PlayerCopy 원본)에 없던 스킬 묶음·Alt만 빼고 나머지는 본체를 그대로 물려받는다.
    internal static class CopyVariantBuilder {
        public static bool Run(MergeLog log) {
            var final = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Final);
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Copy);
            if (existing == null)
                Create(final, log);
            else if (!IsVariantOf(existing, final)) {
                log.Notes.Add($"중단: {PlayerPrefabPaths.Copy}가 이미 있지만 최종본의 Variant가 아님. 덮어쓰지 않음");
                return false;
            }

            using var copy = PrefabContents.Open(PlayerPrefabPaths.Copy, log);
            if (copy == null)
                return false;

            var before = log.Changed.Count;
            var ok = RemoveComponents(copy.Root, log);
            DeactivateChildren(copy.Root, log);
            if (log.Changed.Count > before)
                copy.Save();

            return ok;
        }

        public static bool IsVariantOf(GameObject candidate, GameObject final) =>
            PrefabUtility.GetPrefabAssetType(candidate) == PrefabAssetType.Variant
            && PrefabUtility.GetCorrespondingObjectFromSource(candidate) == final;

        private static void Create(GameObject final, MergeLog log) {
            var scene = EditorSceneManager.NewPreviewScene();
            try {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(final, scene);
                PrefabUtility.SaveAsPrefabAsset(instance, PlayerPrefabPaths.Copy);
                log.Changed.Add($"+{PlayerPrefabPaths.Copy} ({PlayerPrefabPaths.Final}의 Variant)");
            }
            finally {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static bool RemoveComponents(GameObject root, MergeLog log) {
            var ok = true;
            foreach (var typeName in PlayerParts.CopyRemovedComponents) {
                var component = ComponentNames.Find(root, typeName);
                if (component == null)
                    continue;

                try {
                    Object.DestroyImmediate(component);
                    log.Changed.Add($"Variant: -{typeName} (Removed Component 오버라이드)");
                }
                catch (InvalidOperationException e) {
                    ok &= typeName != PlayerParts.SkillManager;
                    log.Notes.Add($"Variant: {typeName} 제거 실패 ({e.Message})");
                }
            }

            return ok;
        }

        private static void DeactivateChildren(GameObject root, MergeLog log) {
            foreach (var name in PlayerParts.HtyChildren) {
                var child = root.transform.Find(name);
                if (child == null || !child.gameObject.activeSelf)
                    continue;

                child.gameObject.SetActive(false);
                log.Changed.Add($"Variant: {name}.activeSelf True -> False");
            }
        }
    }
}
