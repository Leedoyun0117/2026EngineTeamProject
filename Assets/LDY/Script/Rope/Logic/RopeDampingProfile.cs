using System;
using UnityEngine;

namespace LDY.Script {
    [Serializable]
    public struct RopeDampingTuning {
        public float heldDamping;
        public float heldTailDamping;
        public float releaseDamping;
        public float settleTime;
        public float releaseBlendTime;
    }

    public sealed class RopeDampingProfile {
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
            _settle = Mathf.Max(_release, 2f * DecayExponent / _settleTime - _release);
        }

        public float Held => _held;
        public float HeldTail => _heldTail;

        public float AfterRelease(float elapsed) {
            var settling = Mathf.Lerp(_release, _settle, Smooth(elapsed / _settleTime));
            return Mathf.Lerp(_held, settling, Smooth(elapsed / _blendTime));
        }

        private static float Smooth(float t) {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }
    }
}
