using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 흡수 대상 테두리·툴팁, E 입력 실패 토스트.
    public sealed class AbsorbPresenter : MonoBehaviour, IHudPresenter, IHudTicker {
        [SerializeField] private HudSkin skin;
        [SerializeField] private HudToasts toasts;
        [SerializeField] private WorldOutlineView availableOutline;
        [SerializeField] private WorldOutlineView blockedOutline;
        [SerializeField] private UiWorldAnchor tooltip;
        [SerializeField] [Min(0f)] private float failedHighlightSeconds = 0.8f;

        private IAbsorbTargetSource _source;
        private IInputLockSource _inputLock;
        private AbsorbSnapshot _last;
        private AbsorbSnapshot? _pressedFrom;
        private float _failedTimer;

        public void Bind(HudPorts ports) {
            if (_source != null)
                _source.AbsorbPressed -= OnPressed;

            _source = ports.Absorb;
            _inputLock = ports.InputLock;
            _pressedFrom = null;
            _failedTimer = 0f;
            _last = default;
            if (_source != null)
                _source.AbsorbPressed += OnPressed;
            Show(AbsorbPrompt.Hidden, default);
        }

        public void Tick(float deltaTime) {
            if (_source == null)
                return;

            var now = new AbsorbSnapshot(_source.HasTarget, _source.IsHolding, _inputLock != null && _inputLock.IsLocked);
            if (_pressedFrom.HasValue) {
                Report(AbsorbPromptResolver.Judge(_pressedFrom.Value, now));
                _pressedFrom = null;
            }

            _failedTimer = Mathf.Max(0f, _failedTimer - deltaTime);
            Show(AbsorbPromptResolver.Prompt(now, _failedTimer > 0f), _source.TargetBounds);
            _last = now;
        }

        private void OnDestroy() {
            if (_source != null)
                _source.AbsorbPressed -= OnPressed;
        }

        // 팀원 코드가 같은 입력으로 먼저 상태를 바꿀 수 있어서, 판정은 직전 프레임 상태를 기준으로 한다.
        private void OnPressed() {
            _pressedFrom ??= _last;
        }

        private void Report(AbsorbOutcome outcome) {
            if (!AbsorbPromptResolver.IsFailure(outcome))
                return;

            _failedTimer = failedHighlightSeconds;
            if (toasts == null || skin == null)
                return;

            var message = outcome == AbsorbOutcome.AlreadyHolding ? skin.alreadyHoldingMessage : skin.rejectedMessage;
            toasts.Push(ToastKind.Error, message);
        }

        private void Show(AbsorbPrompt prompt, HudRect target) {
            Toggle(availableOutline, prompt == AbsorbPrompt.Available, target);
            Toggle(blockedOutline, prompt == AbsorbPrompt.Blocked, target);
            if (tooltip == null)
                return;

            if (prompt == AbsorbPrompt.Available)
                tooltip.ShowAbove(target);
            else
                tooltip.Hide();
        }

        private static void Toggle(WorldOutlineView view, bool visible, HudRect target) {
            if (view == null)
                return;

            if (visible)
                view.Show(target);
            else
                view.Hide();
        }
    }
}
