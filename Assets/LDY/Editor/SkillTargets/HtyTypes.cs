using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    // HTY 컴포넌트는 Assembly-CSharp에 있어 asmdef에서 직접 참조할 수 없다. 이름으로 찾는다.
    internal static class HtyTypes {
        private static Dictionary<string, Type> _byName;

        public static Type Find(string typeName) {
            _byName ??= TypeCache.GetTypesDerivedFrom<Component>()
                .GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.First());
            return _byName.TryGetValue(typeName, out var type) ? type : null;
        }

        public static bool HasMyCanField() {
            var type = Find(HtyContract.ObjectType);
            return type != null && type.GetField(HtyContract.MyCanField,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        // 적용 전에 막아야 하는 문제. MyCan 필드가 없는 옛 Object_HTY에 붙이면 나중에 기본값(CanCut)이 된다.
        public static List<string> BlockingProblems(IEnumerable<MergedTarget> targets) {
            var problems = new List<string>();
            if (!HasMyCanField())
                problems.Add("Object_HTY에 _myCan 필드가 없음. HTY 최신 코드(origin/HTY a2a7a98: CanIf enum)를 받은 뒤 실행.");

            var missing = targets.SelectMany(t => t.Components.Select(c => c.TypeName))
                .Distinct().Where(n => Find(n) == null);
            problems.AddRange(missing.Select(n => $"컴포넌트 타입 '{n}' 을 찾을 수 없음"));
            return problems;
        }
    }
}
