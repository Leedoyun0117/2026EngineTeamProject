using System;
using UnityEngine;

namespace LDY.Script {
    // heldDamping/heldTailDamping: 잡은 동안 윗부분/꼬리 감쇠, releaseDamping: 놓은 직후 감쇠, settleTime: 가라앉는 데 걸리는 시간, releaseBlendTime: 잡은 값에서 놓은 값으로 넘어가는 시간
    [Serializable]
    public struct RopeDampingTuning {
        public float heldDamping;
        public float heldTailDamping;
        public float releaseDamping;
        public float settleTime;
        public float releaseBlendTime;
    }

    // 놓은 뒤 시간에 따라 감쇠가 커지는 곡선. 줄이 갑자기 멈추지 않고 천천히 가라앉게 한다.
    public sealed class RopeDampingProfile {
        // 진폭이 약 5%가 되는 시간을 대략 맞추려고 정한 값
        private const float DecayExponent = 6f;
        private const float MinTime = 0.01f;

        private readonly float _blendTime;
        private readonly float _held;
        private readonly float _heldTail;
        private readonly float _release;
        private readonly float _settle;
        private readonly float _settleTime;

        public RopeDampingProfile(RopeDampingTuning tuning) {
            _held = Mathf.Max(tuning.heldDamping, 0f);
            _heldTail = Mathf.Max(tuning.heldTailDamping, 0f);
            _release = Mathf.Max(tuning.releaseDamping, 0f);
            _settleTime = Mathf.Max(tuning.settleTime, MinTime);
            _blendTime = Mathf.Max(tuning.releaseBlendTime, MinTime);
            // 가라앉는 시점의 최종 감쇠값
            _settle = Mathf.Max(_release, 2f * DecayExponent / _settleTime - _release);
        }

        public float Held => _held;
        public float HeldTail => _heldTail;

        // 놓은 뒤 elapsed초의 감쇠값. 잡은 값에서 시작해 releaseDamping을 거쳐 settle로 부드럽게 증가.
        public float AfterRelease(float elapsed) {
            var settling = Mathf.Lerp(_release, _settle, Smooth(elapsed / _settleTime));
            return Mathf.Lerp(_held, settling, Smooth(elapsed / _blendTime));
        }

        // smoothstep 보간(0~1)
        private static float Smooth(float t) {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }
    }
}
