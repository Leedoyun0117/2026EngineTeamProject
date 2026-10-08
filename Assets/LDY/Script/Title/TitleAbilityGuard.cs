using System;

namespace LDY.Script
{
    // 타이틀/보조 화면 동안 능력을 막고(이동, 점프, Alt는 영향 없음), 끝나면 이전 값으로 되돌린다.
    public class TitleAbilityGuard : IDisposable
    {
        private readonly IAbilityGate _gate;
        private bool _applied;
        private bool _previous;

        public TitleAbilityGuard(IAbilityGate gate)
        {
            _gate = gate;
        }

        public void Apply()
        {
            if (_applied)
                return;

            _applied = true;
            _previous = _gate.AbilitiesEnabled;
            _gate.SetAbilitiesEnabled(false);
        }

        public void Dispose()
        {
            if (!_applied)
                return;

            _applied = false;
            _gate.SetAbilitiesEnabled(_previous);
        }
    }
}
