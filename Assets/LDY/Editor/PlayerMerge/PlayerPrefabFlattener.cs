using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    internal static class PlayerPrefabFlattener {
        private static readonly HashSet<string> Skipped = new() {
            "m_ObjectHideFlags", "m_CorrespondingSourceObject", "m_PrefabInstance", "m_PrefabAsset",
            "m_GameObject", "m_Script", "m_EditorHideFlags", "m_EditorClassIdentifier", "m_Children", "m_Father",
            "m_Component"
        };

        public static Dictionary<string, string> Flatten(GameObject root) {
            var map = new Dictionary<string, string>();
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                AddObject(map, transform.gameObject, PathOf(root.transform, transform));

            return map;
        }

        private static void AddObject(Dictionary<string, string> map, GameObject go, string path) {
            map[$"{path}#GameObject"] = "present";
            map[$"{path}#GameObject.layer"] = go.layer.ToString();
            map[$"{path}#GameObject.tag"] = go.tag;
            map[$"{path}#GameObject.active"] = go.activeSelf.ToString();

            var counts = new Dictionary<string, int>();
            foreach (var component in go.GetComponents<Component>()) {
                var name = component == null ? "MissingScript" : component.GetType().Name;
                counts[name] = counts.TryGetValue(name, out var n) ? n + 1 : 1;
                var key = counts[name] > 1 ? $"{path}#{name}[{counts[name] - 1}]" : $"{path}#{name}";
                map[key] = component == null ? "MISSING" : "present";
                if (component != null)
                    AddProperties(map, key, component);
            }
        }

        private static void AddProperties(Dictionary<string, string> map, string key, Component component) {
            var iterator = new SerializedObject(component).GetIterator();
            var enter = true;
            while (iterator.NextVisible(enter)) {
                var skipped = Skipped.Contains(iterator.name);
                var generic = iterator.propertyType == SerializedPropertyType.Generic;
                enter = generic && !skipped;
                if (skipped || generic)
                    continue;

                map[$"{key}.{iterator.propertyPath}"] = SerializedValueCopier.Describe(iterator);
            }
        }

        private static string PathOf(Transform root, Transform node) {
            if (node == root)
                return root.name;

            return PathOf(root, node.parent) + "/" + node.name;
        }
    }
}
