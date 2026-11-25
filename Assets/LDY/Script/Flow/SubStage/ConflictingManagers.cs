using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Flow {
    // 같은 일(루트 생성·플레이어 배치)을 하는 팀원 매니저를 런타임에서만 끈다. 타입은 Assembly-CSharp라 이름으로 찾는다.
    public static class ConflictingManagers {
        private static readonly string[] TypeNames = { "JJB.Script.Level.JJBLevelManager" };

        public static List<string> Disable(Scene scene) {
            var disabled = new List<string>();
            foreach (var root in scene.GetRootGameObjects())
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true)) {
                if (behaviour == null || !behaviour.enabled || System.Array.IndexOf(TypeNames, behaviour.GetType().FullName) < 0)
                    continue;
                behaviour.enabled = false;
                disabled.Add($"{behaviour.GetType().Name}({behaviour.name})");
            }

            return disabled;
        }
    }
}
