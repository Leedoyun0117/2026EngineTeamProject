using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LDY.Script {
    internal static class OverrideCleaner {
        private static readonly string[] KeptTransformPrefixes = { "m_LocalPosition", "m_LocalRotation", "m_LocalEulerAnglesHint" };
        private const string NameProperty = "m_Name";

        public static int Run(Scene scene, GameObject asset, List<string> report) {
            var reverted = 0;
            foreach (var root in scene.GetRootGameObjects().Where(r => PrefabUtility.GetCorrespondingObjectFromSource(r) == asset))
                reverted += Clean(root, report);

            return reverted;
        }

        private static int Clean(GameObject instance, List<string> report) {
            var lookup = BuildLookup(instance);
            var reverted = 0;
            foreach (var modification in PrefabUtility.GetPropertyModifications(instance)) {
                if (modification.target == null || !lookup.TryGetValue(modification.target, out var target)
                                                || IsKept(instance, target, modification.propertyPath))
                    continue;

                var property = new SerializedObject(target).FindProperty(modification.propertyPath);
                if (property == null)
                    continue;

                PrefabUtility.RevertPropertyOverride(property, InteractionMode.AutomatedAction);
                report.Add($"{instance.scene.path}: 오버라이드 제거 {target.GetType().Name}.{modification.propertyPath}");
                reverted++;
            }

            return reverted;
        }

        private static bool IsKept(GameObject instance, Object target, string propertyPath) {
            if (target is GameObject && propertyPath == NameProperty)
                return true;

            return target is Transform transform && transform == instance.transform
                                                 && KeptTransformPrefixes.Any(prefix => propertyPath.StartsWith(prefix));
        }

        private static Dictionary<Object, Object> BuildLookup(GameObject instance) {
            var lookup = new Dictionary<Object, Object>();
            foreach (var node in instance.GetComponentsInChildren<Transform>(true)) {
                Add(lookup, node.gameObject);
                foreach (var component in node.GetComponents<Component>())
                    if (component != null)
                        Add(lookup, component);
            }

            return lookup;
        }

        private static void Add(Dictionary<Object, Object> lookup, Object instanceObject) {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(instanceObject);
            if (source != null)
                lookup[source] = instanceObject;
        }
    }
}
