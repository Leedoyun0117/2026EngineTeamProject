using LDY.Flow;
using LDY.FlowEditor;
using UnityEditor;

namespace LDY.StageImportEditor {
    // 연결 자체는 템플릿 생성 경로(StageSceneBuilder)가 한다. 여기서는 연결 상태만 읽어 알려 준다.
    internal static class CatalogStep {
        public static string Describe(string stageId, string scenePath) {
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog);
            if (catalog == null) return "카탈로그 없음";
            var entry = catalog.Find(stageId);
            if (entry == null) return $"카탈로그에 {stageId} 없음";
            if (string.IsNullOrEmpty(entry.scenePath)) return "비어 있음 → 연결 예정";
            if (entry.scenePath == scenePath) return "이 씬에 연결됨";
            var sub = entry.UsesSubRoot ? $" (루트 {entry.subRootName})" : "";
            return $"이미 {entry.scenePath}{sub}에 연결됨 → 바꾸지 않음";
        }

        public static bool IsLinkedTo(string stageId, string scenePath) {
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalog>(FlowPaths.Catalog);
            if (catalog == null) return false;
            var entry = catalog.Find(stageId);
            return entry != null && entry.scenePath == scenePath;
        }
    }
}
