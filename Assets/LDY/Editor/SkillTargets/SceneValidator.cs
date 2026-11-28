using System.Collections.Generic;
using System.Linq;
using System.Text;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.SkillTargetsEditor {
    // 열린 씬을 규칙과 비교만 한다. 아무것도 고치지 않는다.
    internal static class SceneValidator {
        public static void AppendTo(StringBuilder sb, PlanSet set) {
            var byPrefab = set.Targets.GroupBy(t => t.PrefabPath).ToDictionary(g => g.Key, g => g.ToList());
            var ruled = new MarkdownTable("씬", "오브젝트", "프리팹", "대상", "규칙과 다른 것", "주의");
            var tagged = new MarkdownTable("씬", "오브젝트", "MyCan", "문제");

            foreach (var scene in LoadedScenes())
            foreach (var go in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                         .Select(t => t.gameObject)) {
                CheckRuled(scene, go, byPrefab, ruled);
                CheckTagged(scene, go, tagged);
            }

            sb.AppendLine("\n### 열린 씬: 규칙 대상 프리팹 인스턴스");
            if (ruled.Count == 0) sb.AppendLine("문제 없음");
            else ruled.AppendTo(sb);

            sb.AppendLine("\n### 열린 씬: Object_HTY가 붙은 오브젝트");
            if (tagged.Count == 0) sb.AppendLine("없음");
            else tagged.AppendTo(sb);
        }

        private static IEnumerable<Scene> LoadedScenes() {
            for (var i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded) yield return scene;
            }
        }

        private static void CheckRuled(Scene scene, GameObject go, IReadOnlyDictionary<string, List<MergedTarget>> byPrefab,
            MarkdownTable table) {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(go)) return;
            var targets = RuledTargets(go, byPrefab);
            if (targets == null) return;

            foreach (var target in targets) {
                var plan = PlanCalculator.Calculate(target, SnapshotReader.Read(TargetLocator.Find(go, target.Child), target));
                if (!plan.HasChanges && plan.Steps.All(s => s.Kind != StepKind.Conflict)) continue;
                table.Add(scene.name, go.name, DryRunReport.PrefabName(target.PrefabPath), target.Child,
                    DryRunReport.Changes(plan), DryRunReport.Issues(plan));
            }
        }

        private static List<MergedTarget> RuledTargets(GameObject go, IReadOnlyDictionary<string, List<MergedTarget>> byPrefab) {
            for (Object source = PrefabUtility.GetCorrespondingObjectFromSource(go);
                 source != null;
                 source = PrefabUtility.GetCorrespondingObjectFromSource(source)) {
                if (byPrefab.TryGetValue(AssetDatabase.GetAssetPath(source), out var targets)) return targets;
            }

            return null;
        }

        private static void CheckTagged(Scene scene, GameObject go, MarkdownTable table) {
            var type = HtyTypes.Find(HtyContract.ObjectType);
            var component = type != null ? go.GetComponent(type) : null;
            if (component == null) return;

            var property = new SerializedObject(component).FindProperty(HtyContract.MyCanField);
            var myCan = property != null ? SerializedValue.Read(property, go) : FieldValues.Missing;
            var problems = new List<string>();
            if (go.GetComponent<Collider2D>() == null) problems.Add("Collider2D 없음");
            if (go.GetComponent<SpriteRenderer>() == null) problems.Add("SpriteRenderer 없음");
            if (myCan == "CanStop" && go.GetComponent<Rigidbody2D>() == null) problems.Add("정지 대상인데 Rigidbody2D 없음");
            var rescale = HtyTypes.Find("ReScaleObject_HTY");
            if (myCan == "CanResize" && (rescale == null || go.GetComponent(rescale) == null))
                problems.Add("ReScaleObject_HTY 없음");
            table.Add(scene.name, go.name, myCan, problems.Count == 0 ? "OK" : string.Join("; ", problems));
        }
    }
}
