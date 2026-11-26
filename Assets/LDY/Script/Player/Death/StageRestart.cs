using LDY.Death;
using LDY.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script {
    internal static class StageRestart {
        public static IStageRestarter Current =>
            SceneFlow.Instance != null ? (IStageRestarter)SceneFlow.Instance : ActiveSceneRestarter.Instance;
    }

    // 흐름 서비스가 없는 씬(설정 미생성)에서 쓰는 대체 경로.
    internal sealed class ActiveSceneRestarter : IStageRestarter {
        public static readonly ActiveSceneRestarter Instance = new ActiveSceneRestarter();

        public bool IsBusy => false;

        public void RequestRestart() {
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0) {
                Debug.LogWarning("[PlayerDeath] Build Settings에 없는 씬이라 다시 열 수 없다: " + scene.path);
                return;
            }

            SceneManager.LoadScene(scene.buildIndex);
        }
    }

    internal static class QuitWatch {
        public static bool IsQuitting { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register() {
            IsQuitting = false;
            Application.quitting -= Mark;
            Application.quitting += Mark;
        }

        private static void Mark() => IsQuitting = true;
    }
}
