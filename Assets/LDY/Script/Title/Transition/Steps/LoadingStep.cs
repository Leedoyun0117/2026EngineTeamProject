using System.Collections;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 최소 표시 시간과 로딩 완료가 둘 다 충족돼야 끝남
    public class LoadingStep : ITransitionStep
    {
        private readonly IGameSceneLoader _loader;
        private readonly LoadingOverlay _overlay;
        private readonly string _text;
        private readonly Sprite _hourglass;
        private readonly Sprite[] _frames;
        private readonly float _frameDuration;
        private readonly float _flipDuration;
        private readonly AnimationCurve _flipCurve;
        private readonly TMP_FontAsset _font;
        private readonly float _minDisplay;
        private readonly float _timeout;

        public ISceneLoadHandle Handle { get; private set; }

        public LoadingStep(IGameSceneLoader loader, LoadingOverlay overlay, string text, Sprite hourglass,
            Sprite[] frames, float frameDuration, float flipDuration, AnimationCurve flipCurve, TMP_FontAsset font,
            float minDisplay, float timeout)
        {
            _frameDuration = frameDuration;
            _flipDuration = flipDuration;
            _flipCurve = flipCurve;
            _loader = loader;
            _overlay = overlay;
            _text = text;
            _hourglass = hourglass;
            _frames = frames;
            _font = font;
            _minDisplay = minDisplay;
            _timeout = timeout;
        }

        public IEnumerator Run(TransitionContext context)
        {
            if (_overlay != null)
                _overlay.Show(_text, _hourglass, _frames, _frameDuration, _flipDuration, _flipCurve, _font);

            if (_loader == null)
            {
                context.Fail("씬 로더가 IGameSceneLoader를 구현 안 함");
                yield break;
            }

            Handle = _loader.Begin();
            float elapsed = 0f;
            while (true)
            {
                if (Handle.Failed)
                {
                    context.Fail("씬 로딩 실패");
                    yield break;
                }

                if (Handle.Ready && elapsed >= _minDisplay)
                    yield break;

                if (!Handle.Ready && elapsed >= _timeout)
                {
                    context.Fail($"씬 로딩 타임아웃({_timeout:0.#}초)");
                    yield break;
                }

                elapsed += UnscaledTween.Delta;
                yield return null;
            }
        }
    }
}
