using System;
using UnityEngine;

namespace LDY.Script {
    // strength: 반동 세기, frequency: 초당 진동 횟수(Hz), rampTime: 최대 세기까지 걸리는 시간
    [Serializable]
    public struct ClimbSwayTuning {
        public float strength;
        public float frequency;
        public float rampTime;
    }

    // 오르내리는 동안 진자에 더할 작은 좌우 반동(sin 파형)을 만든다. 진자 코드에 연출이 섞이지 않게 분리.
    public sealed class ClimbSway {
        // 이 값보다 세로 입력이 커야 오르는 중으로 본다
        private const float ClimbThreshold = 0.1f;
        private const float MinRampTime = 0.01f;

        private readonly ClimbSwayTuning _tuning;
        // 0~1로 서서히 커지는 세기 비율
        private float _amount;
        // sin 위상(오르는 동안만 진행)
        private float _phase;

        public ClimbSway(ClimbSwayTuning tuning) {
            _tuning = tuning;
        }

        // 오르는 중이면 세기를 서서히 키우고 위상을 진행해 반동 값을 돌려준다.
        public float Step(float climb, float deltaTime) {
            var climbing = Mathf.Abs(climb) > ClimbThreshold;
            var rate = deltaTime / Mathf.Max(_tuning.rampTime, MinRampTime);
            _amount = Mathf.MoveTowards(_amount, climbing ? 1f : 0f, rate);

            if (climbing)
                _phase += _tuning.frequency * Mathf.PI * 2f * deltaTime;

            return _tuning.strength * _amount * Mathf.Sin(_phase);
        }

        // 잡을 때마다 처음 상태로
        public void Reset() {
            _amount = 0f;
            _phase = 0f;
        }
    }
}
