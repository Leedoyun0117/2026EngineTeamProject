using System;
using UnityEngine;

namespace LDY.Script
{
    [Serializable]
    public class AltReturnSettings
    {
        [Min(0.01f)] public float returnSpeed = 40f;
        [Min(0f)] public float minDuration = 0.05f;
        [Min(0f)] public float maxDuration = 0.3f;

        [Tooltip("복귀 종료 직전 이 시간 동안 잔상이 사라진다")]
        [Min(0.01f)] public float afterimageFadeTime = 0.1f;

        public float CalculateDuration(float distance)
        {
            float max = Mathf.Max(minDuration, maxDuration);
            return Mathf.Clamp(distance / returnSpeed, minDuration, max);
        }
    }
}
