using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script
{
    // 토큰은 이전 씬의 InputLockService에 걸려서 새 씬의 잠금은 SceneEntryDirector가 따로 잡음
    [DisallowMultipleComponent]
    public class TransitionCarrier : MonoBehaviour
    {
        private const float EntryClaimTimeout = 3f;

        private IDisposable _token;
        private float _sceneLoadedAt = -1f;

        public static TransitionCarrier Instance { get; private set; }
        public TransitionSession Session { get; private set; }
        public LoadingOverlay Overlay { get; private set; }

        // Domain Reload를 꺼도 이전 플레이의 참조가 안 남게
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        public static TransitionCarrier GetOrCreate(LoadingOverlay overlayPrefab)
        {
            if (Instance != null)
                return Instance;

            var go = new GameObject("TransitionCarrier");
            DontDestroyOnLoad(go);
            var carrier = go.AddComponent<TransitionCarrier>();
            if (overlayPrefab != null)
                carrier.Overlay = LoadingOverlay.Create(overlayPrefab, go.transform);
            else
                Debug.LogError("[Transition] TitleArtSet에 LoadingOverlay 프리팹이 연결 안 됨");
            return carrier;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            ReleaseToken();
        }

        private void OnDestroy()
        {
            ReleaseToken();
            if (Instance == this)
                Instance = null;
        }

        public TransitionSession Begin(IInputLock inputLock)
        {
            ReleaseToken();
            _token = inputLock.Acquire();
            _sceneLoadedAt = -1f;
            Session = new TransitionSession();
            return Session;
        }

        public void MarkSwitching()
        {
            if (Session != null)
                Session.Phase = TransitionPhase.Switching;
        }

        // 활성화가 시작된 뒤엔 새 씬이 이어받아서 건드리지 않음
        public void CancelIfLeaving()
        {
            if (Session == null || Session.Phase == TransitionPhase.Leaving)
                Cancel();
        }

        public void Cancel()
        {
            ReleaseToken();
            Session = null;
            if (Overlay != null)
                Overlay.Hide();
        }

        public bool TryClaimEntry(out TransitionSession session)
        {
            session = Session;
            if (session == null || session.Phase == TransitionPhase.Entering)
                return false;

            session.Phase = TransitionPhase.Entering;
            return true;
        }

        public void Complete()
        {
            Cancel();
            Destroy(gameObject);
        }

        public void ReleaseToken()
        {
            _token?.Dispose();
            _token = null;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Session != null && Session.Phase == TransitionPhase.Switching)
                _sceneLoadedAt = Time.unscaledTime;
        }

        // SceneEntryDirector가 없으면 입력이 영원히 잠기고 화면이 검게 남아서 일정 시간 뒤 강제로 품
        private void Update()
        {
            if (_sceneLoadedAt < 0f || Session == null || Session.Phase == TransitionPhase.Entering)
                return;

            if (Time.unscaledTime - _sceneLoadedAt < EntryClaimTimeout)
                return;

            Debug.LogError("[Transition] 새 씬에 SceneEntryDirector가 없어 전환을 강제 종료");
            Complete();
        }
    }
}
