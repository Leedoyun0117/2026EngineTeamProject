using System;
using LDY.Flow;
using LDY.Flow.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.FlowEditor {
    // 플레이 중이면 실행 중인 흐름 서비스에, 아니면 progress.json 파일에 바로 반영한다.
    internal static class ProgressDebugActions {
        public static void UnlockAll() {
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog);
            if (catalog == null) {
                Debug.LogWarning("[Flow] 카탈로그가 없다. LDY/Flow/Build Flow Assets 먼저 실행");
                return;
            }

            Apply("Unlock All", model => {
                foreach (var entry in catalog.Entries) model.ForceUnlock(entry.id);
            });
        }

        public static void Reset() => Apply("Reset Progress", model => model.Reset());

        private static void Apply(string label, Action<ProgressModel> change) {
            if (Application.isPlaying && SceneFlow.Instance != null) {
                SceneFlow.Instance.Progress.Apply(change);
                Debug.Log($"[Flow] {label}: 실행 중인 진행도에 반영");
                return;
            }

            var store = new JsonProgressStore(ProgressService.DefaultPath);
            var model = store.Load();
            change(model);
            store.Save(model);
            Debug.Log($"[Flow] {label}: {store.Path}");
        }
    }
}
