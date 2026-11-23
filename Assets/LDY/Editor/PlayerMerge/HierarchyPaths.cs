using UnityEngine;

namespace LDY.Script {
    internal static class HierarchyPaths {
        public static string Relative(Transform root, Transform node) {
            return node == root ? "" : Join(Relative(root, node.parent), node.name);
        }

        public static Transform Find(Transform root, string relative) {
            return relative.Length == 0 ? root : root.Find(relative);
        }

        private static string Join(string parent, string name) {
            return parent.Length == 0 ? name : parent + "/" + name;
        }
    }
}
