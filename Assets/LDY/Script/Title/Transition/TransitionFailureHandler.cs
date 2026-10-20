using System;
using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    // 순서: 전원 켜기, 줌 복귀, 알림, 토큰 반납
    // 앞 단계가 예외를 내도 계속 진행하고, 토큰 반납은 finally로 반드시 실행
    public class TransitionFailureHandler
    {
        private readonly TransitionCarrier _carrier;
        private readonly ICrtPower _crt;
        private readonly ICameraRewind _camera;
        private readonly ITransitionAudio _audio;
        private readonly IGameNotifier _notifier;
        private readonly TransitionSettings _settings;
        private readonly Action _restoreVisuals;

        public TransitionFailureHandler(TransitionCarrier carrier, ICrtPower crt, ICameraRewind camera,
            ITransitionAudio audio, IGameNotifier notifier, TransitionSettings settings, Action restoreVisuals)
        {
            _carrier = carrier;
            _crt = crt;
            _camera = camera;
            _audio = audio;
            _notifier = notifier;
            _settings = settings;
            _restoreVisuals = restoreVisuals;
        }

        public IEnumerator Handle(TransitionContext context)
        {
            try
            {
                Try(() => _carrier.Overlay?.Hide());
                Try(_restoreVisuals);

                if (_crt != null)
                {
                    var powerOn = new CrtPowerOnStep(_crt, _audio, _settings.powerOnDuration,
                        _settings.powerOnFlickerDuration, _settings.powerOnFlickerAmount);
                    yield return SafeEnumerator.Guard(powerOn.Run(context), Debug.LogException);
                }

                if (_camera != null)
                    yield return SafeEnumerator.Guard(_camera.Rewind(_settings.rewindDuration), Debug.LogException);

                Try(() => _notifier.Notify(_settings.failureText));
            }
            finally
            {
                _carrier.Cancel();
            }
        }

        private static void Try(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
