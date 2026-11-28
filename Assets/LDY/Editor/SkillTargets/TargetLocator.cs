using UnityEngine;

namespace LDY.SkillTargetsEditor {
    internal static class TargetLocator {
        public static GameObject Find(GameObject root, string child) {
            if (root == null || string.IsNullOrEmpty(child)) return root;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == child)
                    return t.gameObject;
            return null;
        }
    }
}
