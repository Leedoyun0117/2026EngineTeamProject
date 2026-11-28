using LDY.Hud.Core;
using LDY.PlayerLocate;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

namespace LDY.Hud {
    // 에디터 플레이 중 숫자키로 HUD 상태를 강제로 띄운다. 0을 누르면 실제 플레이어 연결로 돌아간다.
    // 1 능력 순환 / 2 흡수 대상 / 3 흡수 실패 / 4 토스트 / 5 복제체·전환 / 6 입력 잠금 / 7 일시정지 배지 / 8 생각.txt
    public sealed class HudDebugFeeder : MonoBehaviour {
        [SerializeField] private HudRoot root;
        [SerializeField] private HudToasts toasts;
        [SerializeField] private ThoughtWindow thought;
        [SerializeField] private string[] toastSamples = { "정보 토스트 예시", "경고 토스트 예시", "이미 능력을 보유중입니다." };
        [SerializeField] private string thoughtSample = "…이게 정말 내가 고른 길일까?";
        [SerializeField] private Vector2 targetOffset = new Vector2(2f, 0f);
        [SerializeField] private Vector2 cloneOffset = new Vector2(-2.5f, 0f);
        [SerializeField] private Vector2 stoppedOffset = new Vector2(0f, 2.5f);
        [SerializeField] private Vector2 boxSize = new Vector2(1f, 1f);

#if UNITY_EDITOR
        private static readonly AbilityId[] AbilityCycle = {
            AbilityId.Pause, AbilityId.Cut, AbilityId.Drag, AbilityId.Alpha,
            AbilityId.Light, AbilityId.Tab, AbilityId.Copy, AbilityId.Zoom
        };

        private HudDebugState _state;
        private GameObject _player;
        private int _ability = -1;
        private int _toast;
        private bool _pressNextFrame;

        private void Update() {
            var keyboard = Keyboard.current;
            if (keyboard == null || root == null)
                return;

            if (_pressNextFrame) {
                _pressNextFrame = false;
                _state?.PressAbsorb();
            }

            if (keyboard.digit0Key.wasPressedThisFrame) {
                _state = null;
                root.Override(null);
                return;
            }

            for (var digit = 1; digit <= 8; digit++) {
                if (keyboard[DigitKey(digit)].wasPressedThisFrame)
                    Apply(digit);
            }

            if (_state != null)
                Follow();
        }

        private void Apply(int digit) {
            if (_state == null) {
                _state = new HudDebugState();
                root.Override(_state.ToPorts());
            }

            switch (digit) {
                case 1: NextAbility(); break;
                case 2: _state.HasTarget = !_state.HasTarget; _state.IsHolding = false; break;
                case 3: FailAbsorb(); break;
                case 4: NextToast(); break;
                case 5: NextCloneState(); break;
                case 6: _state.IsLocked = !_state.IsLocked; break;
                case 7: ToggleStopped(); break;
                case 8: if (thought != null) thought.Show(thoughtSample); break;
            }
        }

        private void NextAbility() {
            _ability = (_ability + 2) % (AbilityCycle.Length + 2) - 1;
            _state.Slot = _ability < 0 ? AbilitySlotState.Empty
                : _ability == AbilityCycle.Length ? AbilitySlotState.Unmapped("Paste")
                : AbilitySlotState.Of(AbilityCycle[_ability]);
        }

        private void FailAbsorb() {
            _state.HasTarget = true;
            _state.IsHolding = true;
            _pressNextFrame = true;
        }

        private void NextToast() {
            if (toasts == null || toastSamples.Length == 0)
                return;

            var index = _toast++ % toastSamples.Length;
            toasts.Push((ToastKind)(index % 3), toastSamples[index]);
        }

        private void NextCloneState() {
            if (!_state.HasClone)
                _state.HasClone = true;
            else if (!_state.CloneControlled)
                _state.CloneControlled = true;
            else
                _state.HasClone = _state.CloneControlled = false;
        }

        private void ToggleStopped() {
            if (_state.StoppedTargets.Count > 0)
                _state.StoppedTargets.Clear();
            else
                _state.StoppedTargets.Add(default);
        }

        private void Follow() {
            var center = Center();
            _state.PlayerBounds = Box(center);
            _state.TargetBounds = Box(center + targetOffset);
            _state.IdleBounds = Box(center + (_state.CloneControlled ? Vector2.zero : cloneOffset));
            for (var i = 0; i < _state.StoppedTargets.Count; i++)
                _state.StoppedTargets[i] = Box(center + stoppedOffset);
        }

        private Vector2 Center() {
            if (_player == null)
                _player = PrimaryPlayerLocator.Find(this);
            if (_player != null)
                return _player.transform.position;
            var camera = Camera.main;
            return camera != null ? (Vector2)camera.transform.position : Vector2.zero;
        }

        private HudRect Box(Vector2 center) => HudRect.FromCenter(center.x, center.y, boxSize.x, boxSize.y);

        private static Key DigitKey(int digit) => Key.Digit1 + (digit - 1);
#endif
    }
}
