using LDY.Hud.Core;
using TMPro;
using UnityEngine;

namespace LDY.Hud {
    // 작업표시줄 오른쪽 조작 대상 이름, 복제체가 있을 때 조작 중이 아닌 쪽 위의 전환 마커.
    public sealed class ControlTargetPresenter : MonoBehaviour, IHudPresenter, IHudTicker {
        [SerializeField] private HudSkin skin;
        [SerializeField] private TMP_Text label;
        [SerializeField] private UiWorldAnchor switchMarker;

        private IControlTargetSource _source;
        private bool? _cloneShown;

        public void Bind(HudPorts ports) {
            _source = ports.Control;
            _cloneShown = null;
            SetLabel(false);
            if (switchMarker != null)
                switchMarker.Hide();
        }

        public void Tick(float deltaTime) {
            var hasClone = _source != null && _source.HasClone;
            SetLabel(hasClone && _source.CloneControlled);
            if (switchMarker == null)
                return;

            if (hasClone)
                switchMarker.ShowAbove(_source.IdleBounds);
            else
                switchMarker.Hide();
        }

        private void SetLabel(bool clone) {
            if (_cloneShown == clone || label == null || skin == null)
                return;

            _cloneShown = clone;
            label.text = clone ? skin.cloneLabel : skin.mainLabel;
        }
    }
}
