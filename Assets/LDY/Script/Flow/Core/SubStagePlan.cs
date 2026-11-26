using System;
using System.Collections.Generic;
using System.Linq;

namespace LDY.Flow.Core {
    // 한 씬에 스테이지 루트가 여럿일 때 무엇을 켜고 끌지. 카탈로그에 등록된 루트 이름만 끈다.
    public readonly struct SubStagePlan {
        private SubStagePlan(string activate, IReadOnlyList<string> deactivate) {
            Activate = activate;
            Deactivate = deactivate;
        }

        public static readonly SubStagePlan WholeScene = new SubStagePlan(null, Array.Empty<string>());

        public bool UsesSubRoot => Activate != null;
        public string Activate { get; }
        public IReadOnlyList<string> Deactivate { get; }

        public static SubStagePlan For(string subRootName, IEnumerable<string> siblingNames,
            IEnumerable<string> knownRootNames) {
            if (string.IsNullOrEmpty(subRootName)) return WholeScene;
            var known = new HashSet<string>(knownRootNames.Where(n => !string.IsNullOrEmpty(n)));
            var off = siblingNames.Where(n => n != subRootName && known.Contains(n)).Distinct().ToList();
            return new SubStagePlan(subRootName, off);
        }

        // 선택 화면을 거치지 않고 실행했을 때: 디자이너가 켜 둔 루트 중 카탈로그 순서상 첫 번째. 없으면 null.
        public static string PickActive(IEnumerable<string> knownRootNamesInOrder, ICollection<string> activeNames) =>
            knownRootNamesInOrder.FirstOrDefault(activeNames.Contains);
    }
}
