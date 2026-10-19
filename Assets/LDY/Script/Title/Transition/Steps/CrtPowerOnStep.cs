using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    // 새 씬의 SceneEntryDirector와 실패 복구가 같이 씀
    public class CrtPowerOnStep : ITransitionStep
    {
        private readonly ICrtPower _crt;
        private readonly ITransitionAudio _audio;
        private readonly float _duration;
        private readonly float _flickerDuration;
        private readonly float _flickerAmount;

        public CrtPowerOnStep(ICrtPower crt, ITransitionAudio audio, float duration, float flickerDuration,
            float flickerAmount)
        {
            _crt = crt;
            _audio = audio;
            _duration = duration;
            _flickerDuration = flickerDuration;
            _flickerAmount = flickerAmount;
        }

        public IEnumerator Run(TransitionContext context)
        {
            _audio.PowerOn();
            yield return _crt.AnimateTo(0f, _duration);

            // 깜빡임은 Level을 잠깐 흔들어서 표현
            yield return UnscaledTween.Run(_flickerDuration,
                t => _crt.Level = Mathf.FloorToInt(t * 6f) % 2 == 0 ? _flickerAmount * (1f - t) : 0f);
            _crt.Level = 0f;
        }
    }
}
