using System;
using UnityEngine;

namespace LDY.Script
{
    public interface IAbilityGate
    {
        // 능력 입력 처리 전에 확인할 값: 외부 스위치 on이고 입력 잠금이 아닐 때만 true.
        bool CanUseAbilities { get; }
        bool AbilitiesEnabled { get; }
        event Action<bool> AbilitiesEnabledChanged;
        void SetAbilitiesEnabled(bool enabled);
    }

    // 타이틀/설정창 등이 SetAbilitiesEnabled로 능력을 끄고 켠다. 능력 자체는 구현하지 않는다.
    [RequireComponent(typeof(PlayerInputLock))]
    public class PlayerAbilityGate : MonoBehaviour, IAbilityGate
    {
        [SerializeField] private bool abilitiesEnabled = true;

        private IInputLock _lock;

        public bool AbilitiesEnabled => abilitiesEnabled;
        public bool CanUseAbilities => abilitiesEnabled && !Lock.IsLocked;
        public event Action<bool> AbilitiesEnabledChanged;

        private IInputLock Lock => _lock ??= GetComponent<PlayerInputLock>().Lock;

        public void SetAbilitiesEnabled(bool enabled)
        {
            if (abilitiesEnabled == enabled)
                return;

            abilitiesEnabled = enabled;
            AbilitiesEnabledChanged?.Invoke(enabled);
        }
    }
}
