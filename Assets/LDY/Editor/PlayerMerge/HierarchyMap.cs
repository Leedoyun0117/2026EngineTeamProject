using UnityEngine;

namespace LDY.Script {
    // 한 계층의 오브젝트를 다른 계층에서 같은 경로·같은 타입 순번의 오브젝트로 옮겨 찾는다.
    internal sealed class HierarchyMap {
        private readonly Transform _from;
        private readonly Transform _to;

        public HierarchyMap(Transform from, Transform to) {
            _from = from;
            _to = to;
        }

        public bool Owns(Object value) {
            var node = NodeOf(value);
            return node != null && node.IsChildOf(_from);
        }

        public Object Resolve(Object value) {
            if (!Owns(value))
                return value;

            var node = NodeOf(value);
            var counterpart = HierarchyPaths.Find(_to, HierarchyPaths.Relative(_from, node));
            if (counterpart == null)
                return null;

            return value switch {
                GameObject => counterpart.gameObject,
                Transform => counterpart,
                Component component => Nth(counterpart, component.GetType(), IndexOf(component)),
                _ => null
            };
        }

        private static Transform NodeOf(Object value) =>
            value switch {
                Component component => component.transform,
                GameObject go => go.transform,
                _ => null
            };

        private static int IndexOf(Component component) {
            var index = 0;
            foreach (var other in component.GetComponents(component.GetType())) {
                if (other == component)
                    return index;
                if (other.GetType() == component.GetType())
                    index++;
            }

            return 0;
        }

        private static Component Nth(Transform node, System.Type type, int index) {
            var found = 0;
            foreach (var component in node.GetComponents(type))
                if (component.GetType() == type && found++ == index)
                    return component;

            return null;
        }
    }
}
