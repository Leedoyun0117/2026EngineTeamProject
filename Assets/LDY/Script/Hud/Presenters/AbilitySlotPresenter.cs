using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 현재 능력 -> 슬롯 아이콘·이름, 실행/해제 키 활성.
    public sealed class AbilitySlotPresenter : MonoBehaviour, IHudPresenter, IHudTicker {
        [SerializeField] private HudSkin skin;
        [SerializeField] private AbilitySlotView slot;
        [SerializeField] private KeyButtonView useKey;
        [SerializeField] private KeyButtonView dropKey;

        private IAbilitySource _source;
        private AbilitySlotState? _shown;

        public void Bind(HudPorts ports) {
            _source = ports.Ability;
            Render(AbilitySlotState.Empty);
        }

        public void Tick(float deltaTime) {
            var state = _source?.Slot ?? AbilitySlotState.Empty;
            if (_shown != state)
                Render(state);
        }

        private void Render(AbilitySlotState state) {
            _shown = state;
            if (slot != null && skin != null)
                slot.Show(Icon(state), Label(state));
            if (useKey != null)
                useKey.SetInteractable(state.HasAbility);
            if (dropKey != null)
                dropKey.SetInteractable(state.HasAbility);
        }

        private Sprite Icon(AbilitySlotState state) {
            var entry = state.IsMapped ? skin.Find(state.Id) : null;
            if (entry != null && entry.icon != null)
                return entry.icon;

            if (state.HasAbility)
                HudWarnings.Once("icon:" + state, $"'{state.SourceName}' 아이콘 없음(HudSkin 대응 없음 또는 아트 없음). 빈 아이콘으로 표시", this);
            return skin.emptyIcon;
        }

        private string Label(AbilitySlotState state) {
            if (!state.HasAbility)
                return skin.emptyName;

            var entry = state.IsMapped ? skin.Find(state.Id) : null;
            if (entry == null || string.IsNullOrEmpty(entry.displayName))
                return state.SourceName;

            return $"<color=#{ColorUtility.ToHtmlStringRGB(entry.color)}>{entry.displayName}</color>";
        }
    }
}
