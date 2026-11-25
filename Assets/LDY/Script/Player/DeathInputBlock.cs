using System;
using JJB.Script.Player;
using UnityEngine;

namespace LDY.Script {
    // 사망 연출 동안 이동과 Alt를 막는 입력 잠금을 한 번만 건다. 잠금/게이트가 없는 플레이어에는 LDY 쪽 어댑터를 붙인다.
    internal sealed class DeathInputBlock : IDisposable {
        private IDisposable _token;

        public bool Engage(GameObject player) {
            if (_token != null)
                return false;

            if (!player.TryGetComponent<PlayerInputLock>(out var inputLock))
                inputLock = player.AddComponent<PlayerInputLock>();

            _token = inputLock.Lock.Acquire();
            BlockJump(player);
            return true;
        }

        public void Dispose() {
            _token?.Dispose();
            _token = null;
        }

        // 게이트는 JJBPlayer가 있어야 붙는다. 타이틀 플레이어 등에 JJB 컴포넌트를 끌어오지 않게 확인한다.
        private static void BlockJump(GameObject player) {
            if (!player.TryGetComponent<JJBControlGate>(out var gate)) {
                if (!player.TryGetComponent<JJBPlayer>(out _))
                    return;

                gate = player.AddComponent<JJBControlGate>();
            }

            gate.SetJumpBlocked(true);
        }
    }
}
