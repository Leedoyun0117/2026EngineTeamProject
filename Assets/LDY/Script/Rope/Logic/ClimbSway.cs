using System;
using UnityEngine;

namespace LDY.Script {
    [Serializable]
    public struct ClimbSwayTuning {
        public float strength;
        public float frequency;
        public float rampTime;
    }

    public sealed class ClimbSway {
        private const float ClimbThreshold = 0.1f;
        private const float MinRampTime = 0.01f;

        private readonly ClimbSwayTuning _tuning;
        private float _amount;
        private float _phase;

        public ClimbSway(ClimbSwayTuning tuning) {
            _tuning = tuning;
        }

        public float Step(float climb, float deltaTime) {
            var climbing = Mathf.Abs(climb) > ClimbThreshold;
            var rate = deltaTime / Mathf.Max(_tuning.rampTime, MinRampTime);
            _amount = Mathf.MoveTowards(_amount, climbing ? 1f : 0f, rate);

            if (climbing)
                _phase += _tuning.frequency * Mathf.PI * 2f * deltaTime;

            return _tuning.strength * _amount * Mathf.Sin(_phase);
        }

        public void Reset() {
            _amount = 0f;
            _phase = 0f;
        }
    }
}
