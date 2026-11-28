using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 입력 잠금(컷씬·사망 등) 동안 플레이어 머리 위 모래시계.
    public sealed class InputLockPresenter : MonoBehaviour, IHudPresenter, IHudTicker {
        [SerializeField] private UiWorldAnchor hourglass;

        private IInputLockSource _source;

        public void Bind(HudPorts ports) {
            _source = ports.InputLock;
            if (hourglass != null)
                hourglass.Hide();
        }

        public void Tick(float deltaTime) {
            if (hourglass == null)
                return;

            if (_source != null && _source.IsLocked)
                hourglass.ShowAbove(_source.PlayerBounds);
            else
                hourglass.Hide();
        }
    }
}
