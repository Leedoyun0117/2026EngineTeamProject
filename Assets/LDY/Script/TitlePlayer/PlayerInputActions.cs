using LDY.TitlePlayer.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.TitlePlayer {
    // PlayerInput의 액션을 이름으로 찾는 공통 진입점. FindAction(…, true)의 예외 대신, 없으면 같은 액션당 에러를 한 번만 남기고
    // null을 돌려준다(그 기능만 꺼지고 나머지는 동작). PlayerInput이 OnEnable에서 에셋을 복제할 수 있어 처음 쓸 때 찾는다.
    public static class PlayerInputActions {
        private static MissingReport _reported = new MissingReport();

        public static OptionalLookup<InputAction> Lookup(PlayerInput input, string actionName, Object context = null) =>
            new OptionalLookup<InputAction>(actionName, name => Find(input, name), name => Report(input, name, context));

        private static InputAction Find(PlayerInput input, string name) {
            var actions = input != null ? input.actions : null;
            return actions != null ? actions.FindAction(name, false) : null;
        }

        private static void Report(PlayerInput input, string name, Object context) {
            var actions = input != null ? input.actions : null;
            var asset = actions != null ? actions.name.Replace("(Clone)", "").Trim() : "PlayerInput(입력 에셋 없음)";
            if (_reported.TryMark(asset + "/" + name))
                Debug.LogError(InputActionNames.MissingMessage(asset, name), context);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _reported = new MissingReport();
    }
}
