using System.Collections.Generic;
using System.Linq;
using System.Text;
using LDY.PlayerLocate;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script {
    // 2단계: 씬의 A/B/C 인스턴스를 최종본으로 바꾼다. Dry Run은 목록만, Apply는 씬 하나씩 확인 후 실행한다.
    internal static class SceneSwapper {
        private const string Title = "[PlayerMerge] Swap Scenes";

        public static void DryRun() {
            var text = new StringBuilder($"{Title} (Dry Run) — 아무것도 바꾸지 않음\n");
            foreach (var target in SwapTargets.Scenes) {
                if (!Exists(target.Path, text))
                    continue;

                using var session = SceneSession.Open(target.Path);
                AppendScene(text, target, SwapPlan.For(session.Scene));
            }

            text.Append(CopyReferenceFinder.Report());
            Debug.Log(text.ToString());
        }

        public static void Apply(string scenePath) {
            var target = SwapTargets.Scenes.First(s => s.Path == scenePath);
            var final = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Final);
            if (final == null) {
                Debug.LogError($"{Title}: {PlayerPrefabPaths.Final}가 없음. 1단계를 먼저 실행");
                return;
            }

            if (!EditorUtility.DisplayDialog("Swap Scenes (Apply)",
                    $"{target.Path}\n담당: {target.Owner}\n\n이 씬의 주인에게 확인 받았습니까?", "예, 확인 받았습니다", "취소"))
                return;

            var text = new StringBuilder($"{Title} (Apply) {target.Path}\n");
            using var session = SceneSession.Open(target.Path);
            var plans = SwapPlan.For(session.Scene);
            AppendScene(text, target, plans);
            foreach (var plan in plans)
                text.AppendLine($"  교체: '{plan.Instance.name}' 참조 {Swap(session.Scene, plan.Instance, final)}개 재연결");

            if (plans.Count > 0)
                text.AppendLine(session.SaveIfSafe() ? "  저장됨" : "  저장하지 않은 변경이 있던 씬이라 저장하지 않음. 직접 저장하세요");

            Debug.Log(text.ToString());
        }

        private static bool Exists(string path, StringBuilder text) {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                return true;

            text.AppendLine($"\n## {path}: 씬 파일 없음");
            return false;
        }

        private static void AppendScene(StringBuilder text, SwapScene target, List<SwapPlan> plans) {
            text.AppendLine($"\n## {target.Path} (담당 {target.Owner}) — 교체 대상 {plans.Count}개");
            foreach (var plan in plans) {
                text.AppendLine($"- {plan.Source} → {PlayerPrefabPaths.Final}");
                text.AppendLine($"  {plan.Placement}");
                if (PrimaryPlayerRule.LooksLikeCopy(plan.Instance.name))
                    text.AppendLine("  경고: 이름에 Copy/Clone이 있어 본체로 인식되지 않음");
                Section(text, "옮기는 오버라이드 (위치·회전·이름·활성)", plan.Carried);
                Section(text, "옮기지 않는 오버라이드 (보고만)", plan.NotCarried);
                Section(text, "구조 오버라이드 (옮기지 않음)", plan.Structure);
                Section(text, "이 인스턴스를 참조하는 다른 오브젝트 (재연결)", plan.References);
            }
        }

        private static void Section(StringBuilder text, string title, List<string> lines) {
            text.AppendLine($"  [{title}] {(lines.Count == 0 ? "없음" : "")}");
            foreach (var line in lines)
                text.AppendLine("    - " + line);
        }

        private static int Swap(Scene scene, GameObject old, GameObject final) {
            var fresh = (GameObject)PrefabUtility.InstantiatePrefab(final, scene);
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
