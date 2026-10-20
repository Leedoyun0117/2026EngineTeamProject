using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script
{
    public class AsyncSceneLoader : SceneLoaderBehaviour, IGameSceneLoader
    {
        [Tooltip("Build Settings에 등록된 본게임 씬 이름. 비어 있으면 로딩 실패로 처리된다")]
        [SerializeField] private string sceneName;

        private Handle _pending;

        // 비동기 로드는 취소가 안 돼서 타임아웃 뒤 재시도해도 대기 중인 걸 재사용
        public ISceneLoadHandle Begin()
        {
            if (_pending != null && !_pending.Failed)
                return _pending;

            _pending = CreateHandle();
            return _pending;
        }

        public override void LoadGameScene()
        {
            Begin();
        }

        private Handle CreateHandle()
        {
            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[Transition] 씬을 불러올 수 없음: '{sceneName}' (Build Settings 확인)", this);
                return new Handle(null);
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
                return new Handle(null);

            operation.allowSceneActivation = false;
            return new Handle(operation);
        }

        private sealed class Handle : ISceneLoadHandle
        {
            private readonly AsyncOperation _operation;

            public Handle(AsyncOperation operation)
            {
                _operation = operation;
            }

            public bool Failed => _operation == null;
            // allowSceneActivation이 false면 progress가 0.9에서 멈춤
            public bool Ready => _operation != null && _operation.progress >= 0.9f;
            public bool IsDone => _operation != null && _operation.isDone;

            public void Activate()
            {
                if (_operation != null)
                    _operation.allowSceneActivation = true;
            }
        }
    }
}
