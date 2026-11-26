using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Flow {
    // 빌드 설정에 있는 씬만 비동기로 연다. 로딩 창은 unscaled time으로 최소 시간만큼 보여 준다.
    public sealed class SceneLoadRunner : MonoBehaviour {
        private const float ReadyProgress = 0.9f;

        private LoadingWindow _window;
        private float _minSeconds;

        public bool IsBusy { get; private set; }

        public void Initialize(LoadingWindow window, float minSeconds) {
            _window = window;
            _minSeconds = minSeconds;
        }

        public static bool InBuild(string scenePath) =>
            !string.IsNullOrEmpty(scenePath) && SceneUtility.GetBuildIndexByScenePath(scenePath) >= 0;

        // 에디터에서는 Build Settings에 없어도 경고만 하고 연다(단독 테스트용). 빌드에서는 실패.
        public static bool CanLoad(string scenePath) {
            if (InBuild(scenePath)) return true;
#if UNITY_EDITOR
            return !string.IsNullOrEmpty(scenePath) && System.IO.File.Exists(scenePath);
#else
            return false;
#endif
        }

        public bool Load(string scenePath, string caption, float delay = 0f) {
            if (IsBusy) return false;
            if (!CanLoad(scenePath)) {
                Debug.LogWarning($"[Flow] 열 수 없는 씬: {scenePath}");
                return false;
            }

            if (!InBuild(scenePath)) Debug.LogWarning($"[Flow] Build Settings에 없는 씬, 에디터에서만 연다: {scenePath}");
            StartCoroutine(Run(scenePath, caption, delay));
            return true;
        }

        private static AsyncOperation StartLoad(string scenePath) {
#if UNITY_EDITOR
            if (!InBuild(scenePath))
                return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath,
                    new LoadSceneParameters(LoadSceneMode.Single));
#endif
            return SceneManager.LoadSceneAsync(scenePath);
        }

        private IEnumerator Run(string scenePath, string caption, float delay) {
            IsBusy = true;
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

            Time.timeScale = 1f;
            _window.Show(caption);
            yield return null;

            var operation = StartLoad(scenePath);
            operation.allowSceneActivation = false;
            var elapsed = 0f;
            while (operation.progress < ReadyProgress || elapsed < _minSeconds) {
                elapsed += Time.unscaledDeltaTime;
                var loaded = operation.progress / ReadyProgress;
                var shown = _minSeconds > 0f ? Mathf.Min(loaded, elapsed / _minSeconds) : loaded;
                _window.SetProgress(shown);
                yield return null;
            }

            _window.SetProgress(1f);
            operation.allowSceneActivation = true;
            while (!operation.isDone) yield return null;
            yield return null;

            _window.Hide();
            IsBusy = false;
        }
    }
}
