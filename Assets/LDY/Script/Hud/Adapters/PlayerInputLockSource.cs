using LDY.Hud.Core;
using LDY.Script;
using UnityEngine;

namespace LDY.Hud.Adapters {
    // PlayerInputLock(컷씬·사망 잠금 토큰)과 PlayerDeath.IsDying. 둘 다 실행 중에 붙을 수 있어 없으면 매번 다시 찾는다.
    // SuspendRequestHub(줄 매달림·Alt)는 조작이 가능한 정지라 넣지 않는다. 허브를 꺼내면 JJBControlGate가 붙는 부작용도 있다.
    internal sealed class PlayerInputLockSource : IInputLockSource, IHudRefreshable {
        private readonly GameObject _player;
        private PlayerInputLock _lock;
        private PlayerDeath _death;

        public PlayerInputLockSource(GameObject player) {
            _player = player;
        }

        public bool IsLocked { get; private set; }
        public HudRect PlayerBounds => WorldBounds.Of(_player);

        public void Refresh() {
            if (_player == null) {
                IsLocked = false;
                return;
            }

            if (_lock == null)
                _player.TryGetComponent(out _lock);
            if (_death == null)
                _player.TryGetComponent(out _death);

            IsLocked = (_lock != null && _lock.Lock.IsLocked) || (_death != null && _death.IsDying);
        }
    }
}
