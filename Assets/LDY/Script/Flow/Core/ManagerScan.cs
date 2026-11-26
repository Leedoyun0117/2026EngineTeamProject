using System;
using System.Collections.Generic;
using System.Linq;

namespace LDY.Flow.Core {
    // 다중 스테이지 씬에서 끌 팀원 매니저를 타입 전체 이름으로 찾은 결과.
    // 못 찾았을 때 원인(이름 변경 등)을 짐작할 수 있게, 이름 조각(hint)이 들어간 다른 타입도 함께 모은다.
    public sealed class ManagerScan {
        private ManagerScan(IReadOnlyList<string> matched, IReadOnlyList<string> similar) {
            Matched = matched;
            Similar = similar;
        }

        public IReadOnlyList<string> Matched { get; }
        public IReadOnlyList<string> Similar { get; }
        public bool FoundAny => Matched.Count > 0;

        public static ManagerScan Evaluate(IEnumerable<string> sceneTypeNames, IReadOnlyCollection<string> targets,
            string hint) {
            var names = (sceneTypeNames ?? Enumerable.Empty<string>()).Where(n => !string.IsNullOrEmpty(n))
                .Distinct(StringComparer.Ordinal).ToList();
            var wanted = new HashSet<string>(targets ?? Array.Empty<string>(), StringComparer.Ordinal);
            var matched = names.Where(wanted.Contains).ToList();
            var similar = string.IsNullOrEmpty(hint)
                ? new List<string>()
                : names.Where(n => !wanted.Contains(n) &&
                                   ShortName(n).IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            return new ManagerScan(matched, similar);
        }

        public static string ShortName(string fullName) {
            if (string.IsNullOrEmpty(fullName)) return fullName;
            var dot = fullName.LastIndexOf('.');
            return dot < 0 ? fullName : fullName.Substring(dot + 1);
        }
    }
}
