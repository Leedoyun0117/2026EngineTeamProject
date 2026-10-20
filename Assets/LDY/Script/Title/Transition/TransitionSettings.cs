using System;
using UnityEngine;

namespace LDY.Script
{
    // 시간은 모두 unscaled 초
    [Serializable]
    public class TransitionSettings
    {
        [Min(0f)] public float pressDuration = 0.1f;
        public Vector2 pressOffset = new Vector2(0.05f, -0.05f);
        [Min(0f)] public float frameDuration = 0.3f;
        [Min(0f)] public float powerOffDuration = 0.4f;
        [Min(0f)] public float fanFadeDuration = 0.4f;
        [Min(0f)] public float loadingMinDisplay = 0.8f;
        [Min(0.1f)] public float loadingTimeout = 10f;
        [Min(0f)] public float rewindDuration = 0.5f;
        [Min(0f)] public float powerOnDuration = 0.5f;
        [Min(0f)] public float powerOnFlickerDuration = 0.12f;
        [Range(0f, 0.5f)] public float powerOnFlickerAmount = 0.15f;
        [Min(0.01f)] public float hourglassFrameDuration = 0.3f;
        [Min(0.01f)] public float hourglassFlipDuration = 0.4f;
        public AnimationCurve hourglassFlipCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public string loadingText = "Cursor.exe 실행 중…";
        public string failureText = "Cursor.exe가 응답하지 않습니다";
    }
}
