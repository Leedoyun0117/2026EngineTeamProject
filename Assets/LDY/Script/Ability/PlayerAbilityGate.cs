using System;
using UnityEngine;

namespace LDY.Script
{
    public interface IAbilityGate
    {
        //기믹관련 필요한 기능 인터페이스로 분리
        bool CanUseAbilities { get; }
        bool AbilitiesEnabled { get; }
        event Action<bool> AbilitiesEnabledChanged;
        void SetAbilitiesEnabled(bool enabled);
    }

    [RequireComponent(typeof(PlayerInputLock))]
    public class PlayerAbilityGate : MonoBehaviour, IAbilityGate
    {
        //능력 사용을 허용하게 하는 스위치
        [SerializeField] private bool abilitiesEnabled = true;

        private IInputLock _lock;

        public bool AbilitiesEnabled => abilitiesEnabled; //허용 스위치가 켜져 있으면 참.
        public bool CanUseAbilities => abilitiesEnabled && !Lock.IsLocked; //스위치가 켜져 있고(abilitiesEnabled), 입력 잠금이 걸려 있지 않을 때(!Lock.IsLocked) 참
        public event Action<bool> AbilitiesEnabledChanged; //허용 스위치가 바뀌었을 때 구독한 쪽에 알려 주는 방송

        private IInputLock Lock => _lock ??= GetComponent<PlayerInputLock>().Lock; //처음 쓸 때 같은 오브젝트의 PlayerInputLock에서 잠금을 꺼내 와서 저장, 이후 재사용

        public void SetAbilitiesEnabled(bool enabled)
        {
            if (abilitiesEnabled == enabled) //이미 같은 값이면 아무것도 안 하고 끝내기
                return;

            abilitiesEnabled = enabled; //현재 능력사용 여부를 전달하고
            AbilitiesEnabledChanged?.Invoke(enabled); //능력사용 여부가 바뀌었다고 Invoke해주기
        }
    }
}
