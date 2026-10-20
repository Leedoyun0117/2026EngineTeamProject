using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Script
{
    // 프레임을 순서대로 재생한 뒤 마지막 프레임을 180도 부드럽게 뒤집고 처음부터 반복
    // 씬 전환 중에도 돌게 unscaled 시간 사용
    public class HourglassAnimator
    {
        private readonly Image _target;
        private readonly List<Sprite> _frames = new List<Sprite>();
        private AnimationCurve _flipCurve;
        private float _frameDuration;
        private float _flipDuration;
        private float _startTime;
        private bool _playing;

        public HourglassAnimator(Image target)
        {
            _target = target;
        }

        // null 프레임은 건너뜀
        public void Play(IEnumerable<Sprite> frames, float frameDuration, float flipDuration, AnimationCurve flipCurve)
        {
            _frames.Clear();
            if (frames != null)
            {
                foreach (Sprite frame in frames)
                {
                    if (frame != null)
                        _frames.Add(frame);
                }
            }

            _frameDuration = Mathf.Max(0.01f, frameDuration);
            _flipDuration = Mathf.Max(0.01f, flipDuration);
            _flipCurve = flipCurve != null && flipCurve.length > 0 ? flipCurve : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            _startTime = Time.unscaledTime;
            _playing = _frames.Count > 0;
            Tick();
        }

        public void Stop()
        {
            _playing = false;
        }

        public void Tick()
        {
            if (!_playing || _target == null)
                return;

            float framesTime = _frames.Count * _frameDuration;
            float t = (Time.unscaledTime - _startTime) % (framesTime + _flipDuration);

            if (t < framesTime)
            {
                _target.sprite = _frames[Mathf.Min(_frames.Count - 1, (int)(t / _frameDuration))];
                _target.rectTransform.localRotation = Quaternion.identity;
                return;
            }

            // 180도 뒤집힌 마지막 프레임은 프레임 0과 같아서 주기가 넘어가는 순간 튀지 않음
            float progress = _flipCurve.Evaluate(UnscaledTween.Progress(t - framesTime, _flipDuration));
            _target.sprite = _frames[_frames.Count - 1];
            _target.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f * progress);
        }
    }
}
