using System.Collections.Generic;
using System.Linq;
using LDY.FlowEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.StageImportEditor {
    // "연결 필요" 항목의 인스턴스마다 LinkGuard를 돌리고 항목별 개수·필드를 로그에 남긴다.
    internal static class LinkSweep {
        public static int Run(Scene scene, CustomBlockMap map, IEnumerable<(string entryName, GameObject instance)> instances,
            FlowLog log) {
            var tally = new SortedDictionary<string, (int count, SortedSet<string> fields)>();
            foreach (var (entryName, instance) in instances) {
                var entry = map != null ? map.Find(entryName) : null;
                if (entry == null || !entry.needsLink || entry.linkFields == null || instance == null) continue;
                var missing = LinkGuard.Apply(instance, entry.linkFields, scene);
                if (missing.Count == 0) continue;
                if (!tally.TryGetValue(entryName, out var row)) row = (0, new SortedSet<string>());
                row.fields.UnionWith(missing);
                tally[entryName] = (row.count + 1, row.fields);
            }

            foreach (var pair in tally)
                log.Warn($"연결 필요 '{pair.Key}' {pair.Value.count}개: {string.Join(", ", pair.Value.fields)} " +
                         "(스크립트 꺼 둠. 연결한 뒤 켜기)");
            if (tally.Count == 0) log.Info("연결 필요 없음");
            return tally.Values.Sum(v => v.count);
        }

        public static IEnumerable<(string, GameObject)> FindInScene(Scene scene, CustomBlockMap map) {
            var byPrefab = map.Entries.Where(e => e.needsLink && e.prefab != null)
                .GroupBy(e => e.prefab).ToDictionary(g => g.Key, g => g.First().name);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) {
                var go = t.gameObject;
                if (!PrefabUtility.IsOutermostPrefabInstanceRoot(go)) continue;
                var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (source != null && byPrefab.TryGetValue(source, out var name)) yield return (name, go);
            }
        }
    }
}
