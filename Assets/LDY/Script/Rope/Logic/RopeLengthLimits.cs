using UnityEngine;

namespace LDY.Script {
    // 플레이어 거리 범위를 줄 전체 길이 안으로 제한해 꼬리가 항상 남게 한다.
    public static class RopeLengthLimits {
        // 최대 거리 = 총길이 x 0.8
        private const float MaxLengthRatio = 0.8f;

        // maxLength를 총길이의 80% 이하로, minLength는 maxLength 이하로 자른다.
        public static PendulumTuning Apply(PendulumTuning tuning, float totalLength) {
            tuning.maxLength = Mathf.Min(tuning.maxLength, totalLength * MaxLengthRatio);
            tuning.minLength = Mathf.Min(tuning.minLength, tuning.maxLength);
            return tuning;
        }
    }
}
