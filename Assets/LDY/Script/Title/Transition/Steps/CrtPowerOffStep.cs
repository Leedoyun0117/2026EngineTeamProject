using System.Collections;

namespace LDY.Script
{
    public class CrtPowerOffStep : ITransitionStep
    {
        private readonly ICrtPower _crt;
        private readonly ITransitionAudio _audio;
        private readonly float _duration;
        private readonly float _fanFade;

        public CrtPowerOffStep(ICrtPower crt, ITransitionAudio audio, float duration, float fanFade)
        {
            _crt = crt;
            _audio = audio;
            _duration = duration;
            _fanFade = fanFade;
        }

        public IEnumerator Run(TransitionContext context)
        {
            _audio.FanFade(_fanFade);
            _audio.PowerOff();
            yield return _crt.AnimateTo(1f, _duration);
        }
    }
}
