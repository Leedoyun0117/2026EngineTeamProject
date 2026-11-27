using System.Collections.Generic;
using System.Linq;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script {
    // 프리팹과 씬 인스턴스를 검사해 Console에 표로 낸다. 씬은 열기만 하고 저장하지 않는다.
    internal static class PlayerValidateRunner {
        public static readonly string[] FinalScenes = {
            "Assets/LDY/Scenes/RopeTest.unity",
            "Assets/LDY/Scenes/Stage2_SpiderTest.unity",
            "Assets/HES/Scene/Test.unity"
        };

        public static void ValidateOpen() {
            var validator = new PlayerValidator();
            CheckPrefabs(validator);
            for (var i = 0; i < SceneManager.sceneCount; i++)
                CheckScene(validator, SceneManager.GetSceneAt(i));

            Print("열린 씬 + 프리팹", validator.Rows);
        }

        public static void ValidateScenes() {
            var validator = new PlayerValidator();
            foreach (var path in FinalScenes) {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) {
                    validator.Add(path, "씬", false, "파일 없음");
                    continue;
                }

                using var session = SceneSession.Open(path);
                CheckScene(validator, session.Scene);
            }

            Print("C 사용 씬 3개", validator.Rows);
        }

        private static void CheckPrefabs(PlayerValidator validator) {
            var final = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Final);
            var copy = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Copy);
            if (final == null) {
                validator.Add(PlayerPrefabPaths.Final, "최종본", false, "없음. LDY/Player/Merge/1. Build Final Player를 먼저 실행");
                return;
            }

            validator.CheckName(final, "최종본");
            validator.Check(final, "최종본", false);
            if (copy == null) {
                validator.Add(PlayerPrefabPaths.Copy, "PlayerCopy", false, "없음");
                return;
            }

            validator.CheckVariant(copy, final);
            validator.Check(copy, "PlayerCopy (Variant)", true);
        }

        private static void CheckScene(PlayerValidator validator, Scene scene) {
            foreach (var instance in Instances(scene)) {
                var source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance);
                var target = $"{scene.name}/{instance.name}";
                if (source == PlayerPrefabPaths.Final || source == PlayerPrefabPaths.Copy) {
                    validator.Add(target, "출처", true, source);
                    validator.Check(instance, target, source == PlayerPrefabPaths.Copy);
                }
                else
                    validator.Add(target, "출처", false, $"{(string.IsNullOrEmpty(source) ? "프리팹 아님" : source)} (2단계 교체 대상)");
            }
        }

        private static IEnumerable<GameObject> Instances(Scene scene) =>
            scene.GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .Where(go => go.CompareTag(PlayerParts.PlayerTag)
                             && (PrefabUtility.IsOutermostPrefabInstanceRoot(go) || !PrefabUtility.IsPartOfPrefabInstance(go)));

        private static void Print(string scope, IReadOnlyList<CheckRow> rows) {
            var text = $"[PlayerValidate] {scope}\n{CheckTable.Render(rows)}";
            if (rows.All(r => r.Passed))
                Debug.Log(text);
            else
                Debug.LogWarning(text);
        }
    }
}
