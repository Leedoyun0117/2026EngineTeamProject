using LDY.StageImport;
using LDY.StageImport.Core;
using UnityEngine;

namespace LDY.StageImportEditor {
    // darkMask·tabzone 묶음마다 오브젝트 하나, 덮는 사각형마다 BoxCollider2D 트리거 하나.
    // 이 영역을 읽는 런타임 코드가 아직 없어서 아무것과도 닿지 않게 둔다(표시용).
    internal sealed class AreaStep : ISceneStep {
        private const string RootName = "Imported Areas";
        private const int IgnoreRaycastLayer = 2;

        public void Apply(SceneContext context) {
            if (context.Plan.Areas.Count == 0) return;
            var root = context.EnsureRoot(RootName);
            int dark = 0, tab = 0;
            foreach (var area in context.Plan.Areas) {
                var kind = area.Kind == AreaKind.Dark ? StageAreaKind.Dark : StageAreaKind.Tab;
                var index = kind == StageAreaKind.Dark ? dark++ : tab++;
                Build(root, area, kind, index);
            }

            if (dark > 0) context.Log.Warn($"어둠 영역 {dark}개: 인식하는 런타임 코드가 없어 표시용으로만 만들었다");
            if (tab > 0) context.Log.Warn($"탭 공간 {tab}개: 인식하는 런타임 코드가 없어 표시용으로만 만들었다");
        }

        private static void Build(Transform root, AreaRegion area, StageAreaKind kind, int index) {
            var go = new GameObject($"{kind}Area_{index}");
            go.transform.SetParent(root, false);
            go.layer = IgnoreRaycastLayer;
            go.AddComponent<StageArea>().Init(kind, area.Cells.Count);
            foreach (var rect in area.Rects) {
                var box = go.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.excludeLayers = ~0;
                box.offset = new Vector2(rect.CenterX, rect.CenterY);
                box.size = new Vector2(rect.Width, rect.Height);
            }
        }
    }
}
