using System;
using System.Collections.Generic;
using System.Linq;

namespace LDY.TitlePlayer.Core {
    // 타이틀 플레이어가 PlayerInput(JJB 소유 JJBInput 에셋)에서 이름으로 찾는 맵·액션.
    // 팀원이 이름을 바꾸면 여기만 고친다. LDY/Merge/Validate Open Stage가 이 목록으로 에셋을 검사한다.
    public static class InputActionNames {
        public const string Map = "Player";
        public const string Move = "Move";
        public const string Jump = "Jump";
        public const string Interact = "Interact";

        public static readonly IReadOnlyList<string> Required = new[] { Move, Jump, Interact };

        public static string MissingMessage(string assetName, string action) =>
            $"{assetName}에 '{action}' 액션 없음 – 타이틀 입력 비활성";

        // 에셋에 있는 액션 이름 목록에서 빠진 필수 액션을 required 순서대로 돌려준다.
        public static List<string> Missing(IEnumerable<string> available, IEnumerable<string> required) {
            var present = new HashSet<string>(available ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            return (required ?? Enumerable.Empty<string>()).Where(name => !present.Contains(name)).ToList();
        }
    }

    // 이름으로 한 번만 찾는다. 못 찾으면 onMissing을 한 번 부르고, 그 뒤로는 다시 찾지 않고 null을 돌려준다(그 기능만 꺼짐).
    public sealed class OptionalLookup<T> where T : class {
        private readonly Func<string, T> _find;
        private readonly Action<string> _onMissing;
        private bool _resolved;
        private T _value;

        public OptionalLookup(string name, Func<string, T> find, Action<string> onMissing) {
            Name = name;
            _find = find;
            _onMissing = onMissing;
        }

        public string Name { get; }

        public T Get() {
            if (_resolved) return _value;
            _resolved = true;
            _value = _find?.Invoke(Name);
            if (_value == null) _onMissing?.Invoke(Name);
            return _value;
        }
    }

    // 같은 키(에셋/액션)는 한 번만 보고하게 거른다. 여러 입력 객체가 같은 액션을 찾아도 로그는 1회.
    public sealed class MissingReport {
        private readonly HashSet<string> _reported = new HashSet<string>(StringComparer.Ordinal);

        public bool TryMark(string key) => _reported.Add(key ?? "");
    }
}
