using System.Collections.Generic;
using System.Linq;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LDY.Script {
    internal static class SceneInstanceReplacer {
        private static readonly string[] ReplaceScenes = {
            "Assets/LDY/Scenes/Stage1_ArtTest.unity",
            "Assets/LDY/Scenes/Stage2_SpiderTest.unity"
        };

        private const string CleanScene = "Assets/LDY/Scenes/RopeTest.unity";

        public static void Run(List<string> report) {
            var shared = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Shared);
            var jibin = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Jibin);
            if (shared == null || jibin == null) {
                report.Add("중단: 프리팹을 불러오지 못함");
                return;
            }

            foreach (var path in ReplaceScenes)
                Process(path, report, scene => Replace(scene, jibin, shared, report));

            Process(CleanScene, report, scene => OverrideCleaner.Run(scene, shared, report));
        }

        private static void Process(string path, List<string> report, System.Func<Scene, int> work) {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) {
                report.Add($"{path}: 씬 파일이 없어 건너뜀");
                return;
            }

            using var session = SceneSession.Open(path);
            var changed = work(session.Scene);
            if (changed == 0) {
                report.Add($"{path}: 변경 없음");
                return;
            }

            report.Add(session.SaveIfSafe()
                ? $"{path}: 저장됨"
                : $"{path}: 저장하지 않은 변경이 있어 저장하지 않음. 씬을 직접 저장하세요");
        }

        private static int Replace(Scene scene, GameObject oldAsset, GameObject newAsset, List<string> report) {
            var targets = scene.GetRootGameObjects()
                .Where(root => PrefabUtility.GetCorrespondingObjectFromSource(root) == oldAsset)
                .ToList();
            foreach (var old in targets)
                report.Add($"{scene.path}: '{old.name}' 교체 (참조 {Swap(scene, old, newAsset)}개 재연결)");

            return targets.Count;
        }

        private static int Swap(Scene scene, GameObject old, GameObject newAsset) {
            var fresh = (GameObject)PrefabUtility.InstantiatePrefab(newAsset, scene);
            var parent = old.transform.parent;
            if (parent != null)
                fresh.transform.SetParent(parent, false);

            fresh.transform.SetPositionAndRotation(old.transform.position, old.transform.rotation);
            fresh.name = old.name;
            fresh.SetActive(old.activeSelf);
            fresh.transform.SetSiblingIndex(old.transform.GetSiblingIndex());
            var remapped = new ReferenceRemapper(old, fresh).Apply(scene);
            Object.DestroyImmediate(old);
            return remapped;
        }
    }
}
