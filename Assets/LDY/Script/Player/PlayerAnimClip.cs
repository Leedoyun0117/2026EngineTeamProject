using System;
using UnityEngine;

namespace LDY.Script
{
    [Serializable]
    public class PlayerAnimClip
    {
        public Sprite[] frames = Array.Empty<Sprite>();
        [Tooltip("프레임별 표시 시간(초). frames와 같은 길이")]
        public float[] durations = Array.Empty<float>();
        public bool loop;

        public bool IsValid => frames != null && durations != null && frames.Length > 0
                               && frames.Length == durations.Length;

        public static PlayerAnimClip Hold(Sprite sprite)
        {
            return new PlayerAnimClip { frames = new[] { sprite }, durations = new[] { 1f }, loop = true };
        }
    }
}
