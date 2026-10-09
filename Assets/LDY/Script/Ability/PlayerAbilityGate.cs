using System;
using UnityEngine;

namespace LDY.Script
{
    public interface IAbilityGate
    {
        bool CanUseAbilities { get; }
        bool AbilitiesEnabled { get; }
        event Action<bool> AbilitiesEnabledChanged;
        void SetAbilitiesEnabled(bool enabled);
    }

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
