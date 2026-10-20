using System;
using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    public static class UnscaledTween
    {
        public static float Delta => Mathf.Min(Time.unscaledDeltaTime, TitleTuning.MaxDeltaTime);

        // duration이 0 이하면 0으로 나누지 않고 완료(1)로 취급
        public static float Progress(float elapsed, float duration)
        {
            return duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
        }

        // 감속 곡선 1 - (1 - t)^2
        public static float EaseOutQuad(float t)
        {
            float inv = 1f - t;
            return 1f - inv * inv;
        }

        // apply는 마지막에 반드시 1로 한 번 호출됨
        public static IEnumerator Run(float duration, Action<float> apply, AnimationCurve ease = null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Delta;
                float t = Progress(elapsed, duration);
                apply(ease != null ? ease.Evaluate(t) : t);
                yield return null;
            }

            apply(ease != null ? ease.Evaluate(1f) : 1f);
        }
    }
}
