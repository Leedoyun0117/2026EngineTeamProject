using System.Collections.Generic;
using System.Linq;
using LDY.Flow.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Flow {
    // 씬에서 스테이지 루트를 이름으로 찾고 켜고 끈다(런타임 메모리에서만, 씬 파일은 그대로).
    public static class SubStageRoots {
        public static GameObject Find(Scene scene, string rootName) {
            foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == rootName)
                    return t.gameObject;
            return null;
        }

        public static GameObject Apply(Scene scene, string subRootName, IEnumerable<string> knownRootNames) {
            var target = Find(scene, subRootName);
            if (target == null) return null;
            var siblings = Siblings(target.transform).ToList();
            var plan = SubStagePlan.For(subRootName, siblings.Select(s => s.name), knownRootNames);
            foreach (var sibling in siblings.Where(s => plan.Deactivate.Contains(s.name)))
                sibling.gameObject.SetActive(false);
            target.SetActive(true);
            return target;
        }

        public static string ActiveRootName(Scene scene, IReadOnlyList<string> knownRootNamesInOrder) {
            var active = new HashSet<string>(knownRootNamesInOrder
                .Select(n => Find(scene, n))
                .Where(go => go != null && go.activeInHierarchy)
                .Select(go => go.name));
            return SubStagePlan.PickActive(knownRootNamesInOrder, active);
        }

        private static IEnumerable<Transform> Siblings(Transform target) {
            if (target.parent != null) {
                foreach (Transform child in target.parent) yield return child;
                yield break;
            }

            foreach (var root in target.gameObject.scene.GetRootGameObjects()) yield return root.transform;
        }
    }
}
