using System;
using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    // 입력을 잠그고 모니터로 카메라를 이동한 뒤 ISceneLoader 훅을 호출한다. 시작하면 되돌리지 않는다.
    public class GameStartTransition : MonoBehaviour, IMenuBlocker
    {
        [SerializeField] private MonitorZoomRig zoomRig;
        [SerializeField] private SceneLoaderBehaviour sceneLoader;

        private IInputLock _lock;
        private IDisposable _token;
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
            _token = _lock.Acquire();
            StartCoroutine(Run());
        }

        private void OnDisable()
        {
            _token?.Dispose();
            _token = null;
            _started = false;
        }

        private IEnumerator Run()
        {
            yield return zoomRig.Play();
            sceneLoader.LoadGameScene();
        }
    }
}
