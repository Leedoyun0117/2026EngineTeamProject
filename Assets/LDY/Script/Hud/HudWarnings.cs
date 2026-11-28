using System.Collections.Generic;
using UnityEngine;

namespace LDY.Hud {
    // 같은 경고는 플레이 세션마다 한 번만 남긴다.
    public static class HudWarnings {
        private static readonly HashSet<string> Seen = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => Seen.Clear();

        public static void Once(string key, string message, Object context = null) {
            if (Seen.Add(key))
                Debug.LogWarning("[HUD] " + message, context);
        }
    }
}
