using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script {
    // 씬의 다른 오브젝트가 옛 인스턴스 안을 가리키던 참조를 새 인스턴스의 같은 자리로 옮긴다.
    internal sealed class ReferenceRemapper {
        private readonly HierarchyMap _map;
        private readonly GameObject _old;
        private readonly GameObject _fresh;

        public ReferenceRemapper(GameObject old, GameObject fresh) {
            _old = old;
            _fresh = fresh;
            _map = new HierarchyMap(old.transform, fresh.transform);
        }

        public int Apply(Scene scene) {
            var count = 0;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var component in root.GetComponentsInChildren<Component>(true))
                    if (component != null && !IsInside(component))
                        count += Remap(component);

            return count;
        }

        private bool IsInside(Component component) =>
            component.transform.IsChildOf(_old.transform) || component.transform.IsChildOf(_fresh.transform);

        private int Remap(Component component) {
            var so = new SerializedObject(component);
            var iterator = so.GetIterator();
            var changed = 0;
            while (iterator.Next(true)) {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                var value = iterator.objectReferenceValue;
                if (value == null || !_map.Owns(value))
                    continue;

                iterator.objectReferenceValue = _map.Resolve(value);
                changed++;
            }

            if (changed > 0)
                so.ApplyModifiedPropertiesWithoutUndo();

            return changed;
        }
    }
}
