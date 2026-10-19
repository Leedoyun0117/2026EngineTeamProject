using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 연결이 비어 있는 단계는 건너뜀
    public class GameStartTransition : MonoBehaviour, IMenuBlocker
    {
        [SerializeField] private MonitorZoomRig zoomRig;
        [SerializeField] private SceneLoaderBehaviour sceneLoader;
        [SerializeField] private TransitionSettings settings = new TransitionSettings();

        [Header("Icon / Frame")]
        [Tooltip("눌림 연출로 움직일 GameStart 아이콘")]
        [SerializeField] private Transform startIcon;
        [Tooltip("점선 확대가 시작되는 영역 (GameStart 아이콘의 hitArea)")]
        [SerializeField] private Collider2D startIconBounds;
        [SerializeField] private Camera contentCamera;
        [SerializeField] private Transform frameParent;

        [Header("CRT / Loading")]
        [SerializeField] private CrtMaterialDriver crt;
        [SerializeField] private TitleArtSet art;
        [SerializeField] private TMP_FontAsset overlayFont;

        [Header("Hooks (비워 두면 빈 구현)")]
        [Tooltip("ITransitionAudio를 구현한 컴포넌트")]
        [SerializeField] private MonoBehaviour audioHook;
        [Tooltip("IGameNotifier를 구현한 컴포넌트")]
        [SerializeField] private MonoBehaviour notifierHook;

        private IInputLock _lock;
        private bool _started;

        public bool Blocking => _started;

        public void Initialize(IInputLock inputLock)
        {
            _lock = inputLock;
        }

        public void Begin()
        {
            if (_started)
                return;

            _started = true;
            TransitionCarrier carrier = TransitionCarrier.GetOrCreate(art != null ? art.loadingOverlayPrefab : null);
            carrier.Begin(_lock);
            StartCoroutine(Run(carrier));
        }

        // 활성화 전에 꺼지면 토큰 반납, 활성화가 시작됐으면 새 씬이 이어받음
        private void OnDisable()
        {
            if (_started && TransitionCarrier.Instance != null)
                TransitionCarrier.Instance.CancelIfLeaving();
            _started = false;
        }

        private IEnumerator Run(TransitionCarrier carrier)
        {
            var context = new TransitionContext();
            ITransitionAudio audio = audioHook as ITransitionAudio ?? new NullTransitionAudio();
            var restore = new List<Action>();

            yield return BuildSequence(carrier, audio, restore).Run(context);
            if (!context.Failed)
                yield break;

            var failure = new TransitionFailureHandler(carrier, crt, zoomRig, audio,
                notifierHook as IGameNotifier ?? new LogGameNotifier(), settings,
                () => restore.ForEach(action => action()));
            yield return failure.Handle(context);
            _started = false;
        }

        private TransitionSequence BuildSequence(TransitionCarrier carrier, ITransitionAudio audio, List<Action> restore)
        {
            var steps = new List<ITransitionStep>();

            if (startIcon != null)
            {
                var press = new IconPressStep(startIcon, settings.pressOffset, settings.pressDuration);
                steps.Add(press);
                restore.Add(press.Restore);
            }

            if (zoomRig != null)
                steps.Add(new ZoomStep(zoomRig));
            if (crt != null)
                steps.Add(new CrtPowerOffStep(crt, audio, settings.powerOffDuration, settings.fanFadeDuration));

            var loading = new LoadingStep(sceneLoader as IGameSceneLoader, carrier.Overlay, settings.loadingText,
                art != null ? art.iconHourglass : null, art != null ? art.hourglassFrames : null,
                settings.hourglassFrameDuration, settings.hourglassFlipDuration, settings.hourglassFlipCurve, overlayFont,
                settings.loadingMinDisplay, settings.loadingTimeout);
            steps.Add(loading);
            steps.Add(new ActivateSceneStep(loading, carrier));

            return new TransitionSequence(steps);
        }
    }
}
