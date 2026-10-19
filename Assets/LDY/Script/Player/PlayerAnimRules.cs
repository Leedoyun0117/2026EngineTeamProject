using System;
using UnityEngine;

namespace LDY.Script
{
    [Serializable]
    public class PlayerAnimRules
    {
        [Min(0f)] public float idleSpeedThreshold = 0.1f;
        [Min(0f)] public float runEnterSpeed = 3.5f;
        [Min(0f)] public float runExitSpeed = 3f;
        [Min(0f)] public float sitDelay = 5f;

        [Header("Playback Speed")]
        [Min(0.01f)] public float speedScaleReference = 5f;
        [Min(0f)] public float speedScaleMin = 0.7f;
        [Min(0f)] public float speedScaleMax = 1.3f;

        [Header("Sit Interrupt")]
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
