using UnityEngine;

namespace LDY.Script
{
    // 배경 연출(CRT, 팬, LED, 마우스)이 공유하는 시간. bool 하나로 Alt 중 동작 여부를 바꾼다.
    public class TitleClock : MonoBehaviour
    {
        [Tooltip("true면 Alt(timeScale 0) 중에도 배경 연출이 계속된다")]
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField, Min(0.001f)] private float maxDeltaTime = TitleTuning.MaxDeltaTime;

        public bool UseUnscaledTime => useUnscaledTime;
        public float Delta => Mathf.Min(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime, maxDeltaTime);
        public float Now => useUnscaledTime ? Time.unscaledTime : Time.time;
    }
}
