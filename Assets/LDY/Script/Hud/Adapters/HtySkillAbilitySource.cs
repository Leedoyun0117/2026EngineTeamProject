using System;
using LDY.Hud.Core;

namespace LDY.Hud.Adapters {
    // 태영 SkillManager_HTY.CurrentSkill/SkillChanged. 싱글턴이 바뀌면(씬 재시작 등) 새 인스턴스에 다시 구독한다.
    internal sealed class HtySkillAbilitySource : IAbilitySource, IHudRefreshable, IDisposable {
        private SkillManager_HTY _manager;

        public AbilitySlotState Slot { get; private set; } = AbilitySlotState.Empty;

        public void Refresh() {
            var manager = SkillManager_HTY.Instance;
            if (manager == null)
                manager = null;
            if (ReferenceEquals(manager, _manager))
                return;

            Unsubscribe();
            _manager = manager;
            if (_manager != null)
                _manager.SkillChanged += OnSkillChanged;
            Apply(_manager != null ? _manager.CurrentSkill : null);
        }

        public void Dispose() => Unsubscribe();

        private void Unsubscribe() {
            if (!ReferenceEquals(_manager, null))
                _manager.SkillChanged -= OnSkillChanged;
            _manager = null;
        }

        private void OnSkillChanged(SkillSo_HTY previous, SkillSo_HTY current) => Apply(current);

        private void Apply(SkillSo_HTY skill) {
            Slot = skill == null ? AbilitySlotState.Empty : SkillStateMap.Resolve(StateName(skill));
        }

        // SkillListSo_HTY의 상태 목록과 SO 목록은 같은 순서로 짝지어져 있다.
        private string StateName(SkillSo_HTY skill) {
            var list = _manager != null ? _manager._skillSoList : null;
            if (list != null && list._skillSoList != null && list._stateList != null) {
                var index = list._skillSoList.IndexOf(skill);
                if (index >= 0 && index < list._stateList.Count)
                    return list._stateList[index].ToString();
            }

            return skill.name;
        }
    }
}
