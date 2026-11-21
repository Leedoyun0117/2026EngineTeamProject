using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static LDY.Script.PlayerVisualContext;

namespace LDY.Script {
    // 스테이지 Player.prefab과 시작 화면 AltPlayer.prefab의 PlayerRenderer가 PlayerAnimSet_Cursor를 쓰게 한다.
    internal static class CursorPlayerAssigner {
        public const string TitlePrefabPath = "Assets/LDY/Prefab/AltPlayer.prefab";

        public static bool Run(List<string> report) {
            var set = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(PlayerAnimSetBuilder.CursorSetPath);
            if (set == null) {
                report.Add($"중단: {PlayerAnimSetBuilder.CursorSetPath} 가 없음. 세트 만들기를 먼저 실행하세요");
                return false;
            }

            AssignToPrefab(PrefabPath, set, report);
            AssignToPrefab(TitlePrefabPath, set, report);
            RevertSceneOverride(report);
            AssetDatabase.SaveAssets();
            return true;
        }

        private static void AssignToPrefab(string path, PlayerAnimSet set, List<string> report) {
            var root = PrefabUtility.LoadPrefabContents(path);
            try {
                var renderers = root.GetComponentsInChildren<PlayerRenderer>(true);
                if (renderers.Length == 0) {
                    report.Add($"건너뜀: {path} 에 PlayerRenderer가 없음");
                    return;
                }

                foreach (var renderer in renderers)
                    Assign(renderer, set, path, report);

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void Assign(PlayerRenderer renderer, PlayerAnimSet set, string path, List<string> report) {
            var so = new SerializedObject(renderer);
            so.FindProperty("animSet").objectReferenceValue = set;
            var target = so.FindProperty("target").objectReferenceValue as SpriteRenderer
                         ?? renderer.GetComponentInChildren<SpriteRenderer>(true);
            so.ApplyModifiedPropertiesWithoutUndo();

            var idle = set.FirstFrame(PlayerAnimId.Idle);
            if (target != null && idle != null)
                target.sprite = idle;

            report.Add($"{path} {renderer.name}: animSet → {set.name}");
        }

        // 씬 인스턴스가 이전 세트를 오버라이드하고 있으면 프리팹 값으로 되돌린다.
        private static void RevertSceneOverride(List<string> report) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            using var session = SceneSession.Open(ScenePath);
            var instance = FindInstance(session.Scene, prefab);
            var visual = instance != null ? instance.transform.Find(PinkName) : null;
            var renderer = visual != null ? visual.GetComponent<PlayerRenderer>() : null;
            if (renderer == null) {
                report.Add("SampleScene: Player 인스턴스를 찾지 못해 오버라이드를 확인하지 못함");
                return;
            }

            var property = new SerializedObject(renderer).FindProperty("animSet");
            if (!property.prefabOverride) {
                report.Add("SampleScene: animSet 오버라이드 없음");
                return;
            }

            PrefabUtility.RevertPropertyOverride(property, InteractionMode.AutomatedAction);
            report.Add(session.SaveIfSafe()
                ? "SampleScene: animSet 오버라이드를 되돌리고 저장"
                : "SampleScene: animSet 오버라이드를 되돌렸지만 저장되지 않은 기존 변경이 있어 저장하지 않음. 직접 저장하세요");
        }
    }
}
