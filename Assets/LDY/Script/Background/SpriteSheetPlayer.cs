using LDY.Background.Core;
using UnityEngine;

namespace LDY.Background {
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteSheetPlayer : MonoBehaviour {
        [SerializeField] private Sprite[] frames = new Sprite[0];
        [SerializeField, Min(1f)] private float frameMs = 120f;
        [SerializeField] private BackgroundTime time;

        private SpriteRenderer _renderer;
        private FrameClock _clock;

        public void Configure(Sprite[] sheetFrames, float milliseconds) {
            frames = sheetFrames;
            frameMs = milliseconds;
            GetComponent<SpriteRenderer>().sprite = frames.Length > 0 ? frames[0] : null;
        }

        private void OnEnable() {
            _renderer = GetComponent<SpriteRenderer>();
            _clock = frames.Length > 0 ? new FrameClock(frames.Length, frameMs / 1000d) : null;
            if (_clock != null) _renderer.sprite = frames[0];
        }

        private void Update() {
            if (_clock != null && _clock.Advance(time.Delta)) _renderer.sprite = frames[_clock.Index];
        }
    }
}
