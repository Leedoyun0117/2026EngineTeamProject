using System;
using UnityEngine;

namespace LDY.Script {
    // 사망 연출 동안 이동과 Alt를 막는 입력 잠금을 한 번만 건다. 잠금/게이트가 없는 플레이어에는 LDY 쪽 어댑터를 붙인다.
    internal sealed class DeathInputBlock : IDisposable {
        private IDisposable _token;

        public bool IsEngaged => _token != null;

        public bool Engage(GameObject player) {
            if (_token != null)
                return false;

            if (!player.TryGetComponent<PlayerInputLock>(out var inputLock))
                inputLock = player.AddComponent<PlayerInputLock>();

            if (!player.TryGetComponent<JJBControlGate>(out var gate))
                gate = player.AddComponent<JJBControlGate>();

            _token = inputLock.Lock.Acquire();
            gate.SetJumpBlocked(true);
            return true;
        }

        public void Dispose() {
            _token?.Dispose();
            _token = null;
        }
    }
}
