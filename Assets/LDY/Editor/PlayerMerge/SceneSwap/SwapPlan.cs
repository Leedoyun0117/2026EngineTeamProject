using System.Collections.Generic;
using System.Linq;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script {
    // 씬의 옛 플레이어 인스턴스 하나를 최종본으로 바꿀 때 옮길 것과 옮기지 않을 것.
    internal sealed class SwapPlan {
        public GameObject Instance { get; private set; }
        public string Source { get; private set; }
        public List<string> Carried { get; } = new();
        public List<string> NotCarried { get; } = new();
        public List<string> Structure { get; } = new();
        public List<string> References { get; } = new();

        public static List<SwapPlan> For(Scene scene) =>
            scene.GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .Where(go => PrefabUtility.IsOutermostPrefabInstanceRoot(go)
                             && SwapTargets.OldPrefabs.Contains(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go)))
                .Select(go => Build(scene, go))
                .ToList();

        private static SwapPlan Build(Scene scene, GameObject instance) {
            var plan = new SwapPlan { Instance = instance, Source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance) };
            plan.ReadModifications();
            plan.ReadStructure();
            plan.ReadReferences(scene);
            return plan;
        }

        public string Placement {
            get {
                var t = Instance.transform;
                var parent = t.parent != null ? t.parent.name : "(씬 루트)";
                return $"'{Instance.name}' 부모 {parent}, 위치 {t.position.ToString("0.###")}, 회전 {t.eulerAngles.ToString("0.#")}, "
                       + $"활성 {Instance.activeSelf}, 순서 {t.GetSiblingIndex()}";
            }
        }

        private void ReadModifications() {
            var sourceRoot = PrefabUtility.GetCorrespondingObjectFromSource(Instance);
            foreach (var modification in PrefabUtility.GetPropertyModifications(Instance) ?? new PropertyModification[0]) {
                var target = modification.target;
                var onRootGo = target == sourceRoot;
                var onRootTransform = sourceRoot != null && target == sourceRoot.transform;
                var line = $"{Describe(sourceRoot, target)}.{modification.propertyPath} = {Value(modification)}";
                (SwapOverrideRule.IsCarried(onRootTransform, onRootGo, modification.propertyPath) ? Carried : NotCarried).Add(line);
            }
        }

        private void ReadStructure() {
            foreach (var added in PrefabUtility.GetAddedComponents(Instance))
                Structure.Add($"추가된 컴포넌트 {HierarchyPaths.Relative(Instance.transform, added.instanceComponent.transform)}.{added.instanceComponent.GetType().Name}");
            foreach (var removed in PrefabUtility.GetRemovedComponents(Instance))
                Structure.Add($"제거된 컴포넌트 {(removed.assetComponent != null ? removed.assetComponent.GetType().Name : "(지금은 없는 컴포넌트)")}");
            foreach (var added in PrefabUtility.GetAddedGameObjects(Instance))
                Structure.Add($"추가된 오브젝트 {HierarchyPaths.Relative(Instance.transform, added.instanceGameObject.transform)}");
            foreach (var removed in PrefabUtility.GetRemovedGameObjects(Instance))
                Structure.Add($"제거된 오브젝트 {(removed.assetGameObject != null ? removed.assetGameObject.name : "(알 수 없음)")}");
        }

        private void ReadReferences(Scene scene) {
            var inside = Instance.transform;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var component in root.GetComponentsInChildren<Component>(true)) {
                    if (component == null || component.transform.IsChildOf(inside))
                        continue;

                    var iterator = new SerializedObject(component).GetIterator();
                    while (iterator.NextVisible(true)) {
                        if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                            continue;

                        var node = iterator.objectReferenceValue switch {
                            Component c => c.transform,
                            GameObject go => go.transform,
                            _ => null
                        };
                        if (node != null && node.IsChildOf(inside))
                            References.Add($"{HierarchyPaths.Relative(component.transform.root, component.transform)}"
                                           + $"({component.transform.root.name}).{component.GetType().Name}.{iterator.propertyPath} -> "
                                           + $"{HierarchyPaths.Relative(inside, node)}:{iterator.objectReferenceValue.GetType().Name}");
                    }
                }
        }

        private static string Describe(GameObject sourceRoot, Object target) {
            if (target == null)
                return "(없는 대상)";

            var node = target switch {
                Component c => c.transform,
                GameObject go => go.transform,
                _ => null
            };
            var path = node != null && sourceRoot != null ? HierarchyPaths.Relative(sourceRoot.transform, node) : "";
            return $"{(path.Length == 0 ? "(루트)" : path)}:{target.GetType().Name}";
        }

        private static string Value(PropertyModification modification) =>
            modification.objectReference != null ? modification.objectReference.name : modification.value;
    }
}
