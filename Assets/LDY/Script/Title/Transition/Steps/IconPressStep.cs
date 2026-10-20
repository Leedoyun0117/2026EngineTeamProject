using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    // 색은 MenuIcon 강조와 겹쳐서 위치만 옮김
    public class IconPressStep : ITransitionStep
    {
        private readonly Transform _target;
        private readonly Vector3 _offset;
        private readonly float _duration;
        private Vector3 _original;
        private bool _pressed;

        public IconPressStep(Transform target, Vector2 offset, float duration)
        {
            _target = target;
            _offset = offset;
            _duration = duration;
        }

        public IEnumerator Run(TransitionContext context)
        {
            _original = _target.localPosition;
            _pressed = true;
            Vector3 pressed = _original + _offset;
            yield return UnscaledTween.Run(_duration, t => _target.localPosition = Vector3.Lerp(_original, pressed, t));
        }

        public void Restore()
        {
            if (_pressed && _target != null)
                _target.localPosition = _original;
            _pressed = false;
        }
    }
}
