using System.Collections.Generic;
using System.IO;
using System.Linq;
using LDY.Flow;
using UnityEditor;

namespace LDY.FlowEditor {
    // 순서: 타이틀, 선택 화면, 카탈로그 Ready 씬(카탈로그 순서), 그 밖에 원래 있던 씬. 원래 씬의 체크 상태는 유지한다.
    internal static class BuildSettingsSync {
        public static void Run(StageCatalog catalog, FlowLog log) {
            var before = EditorBuildSettings.scenes.ToList();
            var wanted = new List<string> { FlowPaths.TitleScene, FlowPaths.SelectScene };
            if (catalog != null) wanted.AddRange(catalog.Entries.Where(e => e.IsReady).Select(e => e.scenePath));

            var result = new List<EditorBuildSettingsScene>();
            foreach (var path in wanted.Distinct()) {
                if (!File.Exists(path)) {
                    log.Warn("씬 파일 없음, 건너뜀: " + path);
                    continue;
                }

                result.Add(new EditorBuildSettingsScene(path, true));
            }

            foreach (var scene in before)
                if (result.All(r => r.path != scene.path))
                    result.Add(scene);

            var changed = !before.Select(s => s.path + s.enabled).SequenceEqual(result.Select(s => s.path + s.enabled));
            if (changed) EditorBuildSettings.scenes = result.ToArray();
            for (var i = 0; i < result.Count; i++)
                log.Info($"{i}: {result[i].path}{(result[i].enabled ? "" : " (꺼짐)")}");
            log.Info(changed ? "Build Settings 갱신" : "Build Settings 변경 없음");
        }

        public static bool Contains(string scenePath) =>
            EditorBuildSettings.scenes.Any(s => s.path == scenePath && s.enabled);

        public static void Add(string scenePath, FlowLog log) {
            if (EditorBuildSettings.scenes.Any(s => s.path == scenePath)) return;
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Append(new EditorBuildSettingsScene(scenePath, true)).ToArray();
            log.Info("Build Settings에 추가 " + scenePath);
        }
    }
}
