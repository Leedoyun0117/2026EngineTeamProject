using LDY.Background.Core;
using UnityEngine;

namespace LDY.Background {
    public sealed class VisualizerBars : MonoBehaviour {
        [SerializeField] private SpriteRenderer[] segments = new SpriteRenderer[0];
        [SerializeField] private int bars;
        [SerializeField] private int segmentsPerBar;
        [SerializeField] private Color[] ramp = new Color[0];
        [SerializeField] private Color off = Color.white;
        [SerializeField, Min(1f)] private float frameMs = 120f;
        [SerializeField] private int seed = 1;
        [SerializeField] private BackgroundTime time;

        private VisualizerModel _model;
        private FrameClock _clock;

        public void Configure(SpriteRenderer[] grid, int barCount, int segmentCount, Color[] colors, Color offColor,
            float milliseconds) {
            segments = grid;
            bars = barCount;
            segmentsPerBar = segmentCount;
            ramp = colors;
            off = offColor;
            frameMs = milliseconds;
            foreach (var segment in segments) segment.color = off;
        }

        private void OnEnable() {
            if (bars < 1 || segmentsPerBar < 1 || segments.Length < bars * segmentsPerBar) return;
            _model = new VisualizerModel(seed, bars, segmentsPerBar);
            _clock = new FrameClock(1, frameMs / 1000d);
            Show(0);
        }

        private void Update() {
            if (_clock != null && _clock.Advance(time.Delta)) Show(_clock.Ticks);
        }

        private void Show(long step) {
            for (var bar = 0; bar < bars; bar++) {
                var height = _model.Height(step, bar);
                for (var s = 0; s < segmentsPerBar; s++)
                    segments[bar * segmentsPerBar + s].color = s < height ? ramp[s] : off;
            }
        }
    }
}
