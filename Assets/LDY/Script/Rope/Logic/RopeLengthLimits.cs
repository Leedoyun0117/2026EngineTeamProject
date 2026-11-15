using UnityEngine;

namespace LDY.Script {
    public static class RopeLengthLimits {
        private const float MaxLengthRatio = 0.8f;

        public static PendulumTuning Apply(PendulumTuning tuning, float totalLength) {
            tuning.maxLength = Mathf.Min(tuning.maxLength, totalLength * MaxLengthRatio);
            tuning.minLength = Mathf.Min(tuning.minLength, tuning.maxLength);
            return tuning;
        }
    }
}
