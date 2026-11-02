using System.Collections;
using UnityEngine;

namespace LDY.Script {
    public class FocusFrameExpandStep : ITransitionStep {
        private readonly Camera _contentCamera;
        private readonly float _duration;
        private readonly Collider2D _from;
        private readonly FocusFrame _view;

        public FocusFrameExpandStep(FocusFrame view, Collider2D from, Camera contentCamera, float duration) {
            _view = view;
            _from = from;
            _contentCamera = contentCamera;
            _duration = duration;
        }

        public IEnumerator Run(TransitionContext context) {
            var b = _from.bounds;
            var start = Rect.MinMaxRect(b.min.x, b.min.y, b.max.x, b.max.y);
            var end = ContentRect();

            yield return UnscaledTween.Run(_duration, t => _view.ShowWorld(new Rect(
                Vector2.Lerp(start.position, end.position, t), Vector2.Lerp(start.size, end.size, t))));
        }

        private Rect ContentRect() {
            var height = _contentCamera.orthographicSize * 2f;
            var texture = _contentCamera.targetTexture;
            var aspect = texture != null ? (float)texture.width / texture.height : _contentCamera.aspect;
            var size = new Vector2(height * aspect, height);
            return new Rect((Vector2)_contentCamera.transform.position - size * 0.5f, size);
        }
    }

    public class HideFocusFrameStep : ITransitionStep {
        private readonly FocusFrame _view;

        public HideFocusFrameStep(FocusFrame view) {
            _view = view;
        }

        public IEnumerator Run(TransitionContext context) {
            _view.Hide();
            yield break;
        }
    }
}