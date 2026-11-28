using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 스프라이트 프레임을 unscaledDeltaTime으로 넘긴다(Alt 정지 중에도 움직임).
    public abstract class FrameAnimator : MonoBehaviour {
        [SerializeField] private Sprite[] frames = new Sprite[0];
        [SerializeField] [Min(1)] private int frameMilliseconds = 120;

        private FrameClock _clock;

        private void OnEnable() {
            _clock = new FrameClock(frames.Length, frameMilliseconds / 1000f);
            Apply();
        }

        private void Update() {
            if (_clock != null && _clock.Tick(Time.unscaledDeltaTime))
                Apply();
        }

        private void Apply() {
            if (frames.Length > 0)
                SetSprite(frames[_clock.Frame]);
        }

        protected abstract void SetSprite(Sprite sprite);
    }
}
