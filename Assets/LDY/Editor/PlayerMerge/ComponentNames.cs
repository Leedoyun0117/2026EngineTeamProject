using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LDY.Script {
    // 팀원 스크립트를 타입으로 직접 참조하지 않도록 이름으로 찾는다.
    internal static class ComponentNames {
        public static Component Find(GameObject go, string typeName) =>
            go.GetComponents<Component>().FirstOrDefault(c => c != null && c.GetType().Name == typeName);

        public static IEnumerable<string> On(GameObject go) =>
            go.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name);

        public static IEnumerable<string> InChildren(GameObject go) =>
            go.GetComponentsInChildren<Component>(true).Where(c => c != null).Select(c => c.GetType().Name);
    }
}
