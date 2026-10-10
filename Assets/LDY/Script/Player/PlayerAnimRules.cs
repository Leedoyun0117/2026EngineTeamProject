using System;
using UnityEngine;

namespace LDY.Script
{
    // 애니메이션 상태 전환에 쓰는 값. 규칙을 바꿀 때는 이 값만 조정한다.
    [Serializable]
    public class PlayerAnimRules
    {
        [Tooltip("이 속도 미만이면 정지(Idle)로 본다")]
        [Min(0f)] public float idleSpeedThreshold = 0.1f;
        [Tooltip("이 속도 이상이 되면 Run으로 들어간다")]
        [Min(0f)] public float runEnterSpeed = 3.5f;
        [Tooltip("Run 중에는 이 속도 미만이 되어야 Walk로 내려간다 (경계에서 깜빡임 방지)")]
        [Min(0f)] public float runExitSpeed = 3f;
        [Tooltip("Idle이 이 시간(초) 이어지면 앉는다")]
        [Min(0f)] public float sitDelay = 5f;

        [Header("Playback Speed")]
        [Tooltip("재생 배율 = |속도| / 기준속도. 현재 JJB 이동 속도와 맞춘다")]
        [Min(0.01f)] public float speedScaleReference = 5f;
        [Min(0f)] public float speedScaleMin = 0.7f;
        [Min(0f)] public float speedScaleMax = 1.3f;

        [Header("Sit Interrupt")]
        [Tooltip("SitDown의 n번째 프레임에서 중단될 때 StandUp을 시작할 프레임. -1이면 StandUp 없이 바로 이동 상태로")]
        public int[] standUpEntryBySitFrame = { -1, 1, 0 };

        public float SpeedScale(float speed)
        {
            return Mathf.Clamp(speed / speedScaleReference, speedScaleMin, speedScaleMax);
        }

        public int StandUpEntryFor(int sitFrame)
        {
            if (standUpEntryBySitFrame == null || standUpEntryBySitFrame.Length == 0)
                return 0;

            return standUpEntryBySitFrame[Mathf.Clamp(sitFrame, 0, standUpEntryBySitFrame.Length - 1)];
        }
    }
}
