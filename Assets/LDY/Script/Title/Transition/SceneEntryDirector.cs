using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 전환으로 들어왔을 때만 동작(씬을 직접 실행하면 아무것도 안 함)
    // Awake에서 입력 잠금을 먼저 잡음(잠금 서비스는 필요할 때 만들어져서 순서 문제 없음)
    // 전원 켜짐이 끝나면 반납하고 Carrier 정리
    [DefaultExecutionOrder(-1000)]
    public class SceneEntryDirector : MonoBehaviour
    {
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private CrtMaterialDriver crt;
        [SerializeField] private TransitionSettings settings = new TransitionSettings();
        [Tooltip("본게임 모니터가 화면을 꽉 채우지 않을 때 전원 켜짐 화면 크기(화면 대비 비율)를 줄인다")]
        [SerializeField] private Vector2 screenFill = Vector2.one;
        [Tooltip("ITransitionAudio를 구현한 컴포넌트 (비우면 빈 구현)")]
        [SerializeField] private MonoBehaviour audioHook;

        private TransitionCarrier _carrier;
        private IDisposable _token;

        private void Awake()
        {
            _carrier = TransitionCarrier.Instance;
            if (_carrier == null || !_carrier.TryClaimEntry(out _))
            {
                _carrier = null;
                enabled = false;
                return;
            }

            if (playerLock == null)
                playerLock = FindFirstObjectByType<PlayerInputLock>(FindObjectsInactive.Include);

            if (playerLock != null)
                _token = playerLock.Lock.Acquire();
            else
                Debug.LogError("[Transition] PlayerInputLock을 못 찾아서 입력을 잠그지 못함", this);

            if (crt != null)
            {
                crt.Fill = screenFill;
                crt.Level = 1f;
            }
        }

        private void Start()
        {
            StartCoroutine(Run());
        }

        private void OnDisable()
        {
            Release();
        }

        private void OnDestroy()
        {
            Release();
        }

        private IEnumerator Run()
        {
            try
            {
                // 첫 프레임이 검은 CRT로 그려진 뒤에 오버레이를 걷음
                yield return null;
                if (_carrier != null && _carrier.Overlay != null)
                    _carrier.Overlay.Hide();

                if (crt != null)
                {
                    var steps = new List<ITransitionStep>
                    {
                        new CrtPowerOnStep(crt, audioHook as ITransitionAudio ?? new NullTransitionAudio(),
                            settings.powerOnDuration, settings.powerOnFlickerDuration, settings.powerOnFlickerAmount)
                    };
                    var context = new TransitionContext();
                    yield return new TransitionSequence(steps).Run(context);
                    if (context.Failed)
                        crt.Level = 0f;
                }
            }
            finally
            {
                if (crt != null)
                    crt.Level = 0f;
                Release();
            }
        }

        private void Release()
        {
            _token?.Dispose();
            _token = null;

            if (_carrier != null)
            {
                TransitionCarrier carrier = _carrier;
                _carrier = null;
                carrier.Complete();
            }
        }
    }
}
