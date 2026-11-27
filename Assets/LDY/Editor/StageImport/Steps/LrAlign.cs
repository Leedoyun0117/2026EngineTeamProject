using LDY.FlowEditor;
using LDY.StageImport.Core;
using UnityEngine;

namespace LDY.StageImportEditor {
    // L/R 특수 규칙: LRDoor 인스턴스의 자식 L/R 버튼을 맵의 "L"·"R" 칸으로 옮긴다. 인스턴스 위치만 바꾸고 프리팹 원본은 건드리지 않는다.
    internal static class LrAlign {
        private const string LButtonType = "LButton";
        private const string RButtonType = "RButton";

        public static LrPairing Judge(ImportPlan plan, CustomBlockAssets customs) {
            var lr = LrPairing.Judge(plan, CustomBlockSeed.LrDoorName, CustomBlockSeed.LName, CustomBlockSeed.RName);
            if (lr.Status != LrStatus.Aligned) return lr;
            var door = customs.Map != null ? customs.Map.Find(CustomBlockSeed.LrDoorName) : null;
            return door != null && door.prefab != null ? lr : LrPairing.Mismatch("LRDoor 대응표 프리팹 비어 있음");
        }

        public static void Apply(LrPairing lr, CustomPlacer customs, FlowLog log) {
            if (lr.Status == LrStatus.NotUsed) return;
            if (lr.Status == LrStatus.Mismatch) {
                log.Warn($"L/R 정렬 안 함: {lr.Reason} (L·R 칸은 분홍 임시 블록)");
                return;
            }

            if (!customs.Spawned.TryGetValue(CustomBlockSeed.LrDoorName, out var doors) || doors.Count != 1) {
                log.Warn("L/R 정렬 안 함: 놓은 LRDoor 인스턴스가 1개가 아님");
                return;
            }

            var door = doors[0];
            var lButton = FindChild(door, LButtonType);
            var rButton = FindChild(door, RButtonType);
            if (lButton == null || rButton == null) {
                log.Warn($"L/R 정렬 안 함: {door.name}에 LButton/RButton 자식이 없음");
                return;
            }

            // 문 몸통 기준으로 다시 맞춘다(처음 정렬은 버튼까지 포함한 범위라 어긋난다). 자식은 함께 움직이므로 문 먼저.
            var body = door.GetComponent<SpriteRenderer>();
            if (body != null) door.transform.position += SceneContext.RectCenter(lr.Door) - body.bounds.center;
            PrefabAligner.Center(lButton, SceneContext.RectCenter(lr.L));
            PrefabAligner.Center(rButton, SceneContext.RectCenter(lr.R));
            log.Info($"L/R 정렬: {door.name}의 L 버튼 → {lr.L}, R 버튼 → {lr.R} (인스턴스만 변경)");
        }

        private static GameObject FindChild(GameObject root, string typeName) {
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && component.gameObject != root && component.GetType().Name == typeName)
                    return component.gameObject;
            return null;
        }
    }
}
