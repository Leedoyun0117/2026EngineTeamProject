using UnityEngine;

namespace LDY.Background {
    public sealed class ProgressBar : MonoBehaviour {
        [SerializeField] private Transform fill;
        [SerializeField] private Transform thumb;
        [SerializeField] private float trackLeft;
        [SerializeField] private float trackWidth;
        [SerializeField] private float trackCenterY;
        [SerializeField] private float trackHeight;
        [SerializeField] private float thumbMinX;
        [SerializeField] private float thumbMaxX;
        [SerializeField, Range(0f, 1f)] private float progress;
        [SerializeField, Min(0f)] private float loopSeconds = 60f;
        [SerializeField] private BackgroundTime time;

        public float Progress {
            get => progress;
            set {
                progress = Mathf.Clamp01(value);
                Apply();
            }
        }

        public void Configure(Transform fillBar, Transform thumbMark, Rect track, float minX, float maxX) {
            fill = fillBar;
            thumb = thumbMark;
            trackLeft = track.xMin;
            trackWidth = track.width;
            trackCenterY = track.center.y;
            trackHeight = track.height;
            thumbMinX = minX;
            thumbMaxX = maxX;
            Apply();
        }

        private void Update() {
            if (loopSeconds <= 0f) return;
            progress = Mathf.Repeat(progress + time.Delta / loopSeconds, 1f);
            Apply();
        }

        private void Apply() {
            if (fill != null) {
                var width = trackWidth * progress;
                fill.localPosition = new Vector3(trackLeft + width * 0.5f, trackCenterY, 0f);
                fill.localScale = new Vector3(width, trackHeight, 1f);
            }

            if (thumb != null) {
                var position = thumb.localPosition;
                position.x = Mathf.Lerp(thumbMinX, thumbMaxX, progress);
                thumb.localPosition = position;
            }
        }
    }
}
