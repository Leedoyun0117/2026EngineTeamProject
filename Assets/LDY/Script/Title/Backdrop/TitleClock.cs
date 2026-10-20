using UnityEngine;

namespace LDY.Script
{
    public class TitleClock : MonoBehaviour
    {
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField, Min(0.001f)] private float maxDeltaTime = TitleTuning.MaxDeltaTime;

        public bool UseUnscaledTime => useUnscaledTime;
        public float Delta => Mathf.Min(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime, maxDeltaTime);
        public float Now => useUnscaledTime ? Time.unscaledTime : Time.time;
    }
}
