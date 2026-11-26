using System;
using UnityEngine;

namespace LDY.Flow {
    // 골·사망을 흐름 서비스에 알리는 창구. 팀원 이벤트 어댑터도 여기로 보내면 된다.
    public static class StageSignals {
        public static event Action Cleared;
        public static event Action PlayerLost;

        public static void RaiseCleared() => Cleared?.Invoke();
        public static void RaisePlayerLost() => PlayerLost?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() {
            Cleared = null;
            PlayerLost = null;
        }
    }
}
