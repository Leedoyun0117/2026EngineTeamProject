using LDY.Flow.Core;
using UnityEngine;

namespace LDY.Flow {
    // 어떤 씬에서 플레이를 시작해도 첫 씬 로드 전에 흐름 서비스를 하나 만든다.
    internal static class FlowBootstrap {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create() {
            if (SceneFlow.Instance != null) return;
            var settings = Resources.Load<FlowSettings>(FlowSettings.ResourcePath);
            if (settings == null || settings.catalog == null || settings.skin == null) {
                Debug.LogWarning("[Flow] Resources/" + FlowSettings.ResourcePath +
                                 " 설정이 없거나 비어서 흐름 서비스를 만들지 않았다. LDY/Flow/Build Flow Assets 실행 필요");
                return;
            }

            var go = new GameObject("[LDY Flow]");
            Object.DontDestroyOnLoad(go);
            var window = go.AddComponent<LoadingWindow>();
            window.Build(settings.skin);
            var loader = go.AddComponent<SceneLoadRunner>();
            loader.Initialize(window, settings.loadingMinSeconds);
            var progress = new ProgressService(new JsonProgressStore(ProgressService.DefaultPath));
            go.AddComponent<SceneFlow>().Initialize(settings, progress, loader);
        }
    }
}
