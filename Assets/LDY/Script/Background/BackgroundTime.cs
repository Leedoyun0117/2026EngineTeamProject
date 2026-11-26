using System;
using UnityEngine;

namespace LDY.Background {
    [Serializable]
    public struct BackgroundTime {
        [SerializeField] private bool useUnscaledTime;

        public float Delta => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }
}
