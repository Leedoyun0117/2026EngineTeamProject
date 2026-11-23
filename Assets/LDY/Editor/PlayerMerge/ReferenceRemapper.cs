using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LDY.Script {
    internal sealed class ReferenceRemapper {
        private readonly Dictionary<Object, Object> _map = new();
        private readonly GameObject _old;
        private readonly GameObject _fresh;

        public ReferenceRemapper(GameObject old, GameObject fresh) {
            _old = old;
            _fresh = fresh;
            foreach (var node in old.GetComponentsInChildren<Transform>(true))
                MapNode(node, HierarchyPaths.Find(fresh.transform, HierarchyPaths.Relative(old.transform, node)));
        }

        public int Apply(Scene scene) {
            var count = 0;
            foreach (var root in scene.GetRootGameObjects()) {
                if (root == _old || root == _fresh)
                    continue;

                foreach (var component in root.GetComponentsInChildren<Component>(true))
                    count += component != null ? Remap(component) : 0;
            }

            return count;
        }

        private void MapNode(Transform oldNode, Transform freshNode) {
            if (freshNode == null)
                return;

            _map[oldNode.gameObject] = freshNode.gameObject;
            var used = new Dictionary<System.Type, int>();
            foreach (var component in oldNode.GetComponents<Component>()) {
                if (component == null)
                    continue;

                var type = component.GetType();
                var index = used.TryGetValue(type, out var n) ? n : 0;
                used[type] = index + 1;
                var counterpart = Nth(freshNode, type, index);
                if (counterpart != null)
                    _map[component] = counterpart;
            }
        }

        private static Component Nth(Transform node, System.Type type, int index) {
            var found = 0;
            foreach (var component in node.GetComponents(type)) {
                if (component.GetType() == type && found++ == index)
                    return component;
            }

            return null;
        }

        private int Remap(Component component) {
            var so = new SerializedObject(component);
            var iterator = so.GetIterator();
            var changed = 0;
            for (var enter = true; iterator.NextVisible(enter); enter = true) {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                var value = iterator.objectReferenceValue;
                if (value == null || !_map.TryGetValue(value, out var replacement))
                    continue;

                iterator.objectReferenceValue = replacement;
                changed++;
            }

            if (changed > 0)
                so.ApplyModifiedPropertiesWithoutUndo();

            return changed;
        }
    }
}
