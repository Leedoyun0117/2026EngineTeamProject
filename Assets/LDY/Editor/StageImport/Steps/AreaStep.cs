using System.Collections.Generic;
using LDY.StageImport;
using LDY.StageImport.Core;
using UnityEngine;

namespace LDY.StageImportEditor {
    // darkMask·tabzone·custom 영역 묶음마다 오브젝트 하나, 덮는 사각형마다 BoxCollider2D 트리거 하나.
    // 이 영역을 읽는 런타임 코드가 아직 없어서 아무것과도 닿지 않게 둔다(표시용). custom 영역은 이름표(EditorOnly)를 붙인다.
    internal sealed class AreaStep : ISceneStep {
        private const string RootName = "Imported Areas";
        private const int IgnoreRaycastLayer = 2;

        public void Apply(SceneContext context) {
            if (context.Plan.Areas.Count == 0) return;
            var root = context.EnsureRoot(RootName);
            int dark = 0, tab = 0;
            var named = new SortedDictionary<string, int>();
            foreach (var area in context.Plan.Areas) {
                switch (area.Kind) {
                    case AreaKind.Dark:
                        Build(root, area, StageAreaKind.Dark, $"DarkArea_{dark++}", "");
                        break;
                    case AreaKind.Tab:
                        Build(root, area, StageAreaKind.Tab, $"TabArea_{tab++}", "");
                        break;
                    default:
                        var name = CustomBlockAssets.DisplayName(area.Key);
                        named.TryGetValue(name, out var index);
                        named[name] = index + 1;
                        var go = Build(root, area, StageAreaKind.Custom, $"{name}_Area_{index}", name);
                        var first = area.Rects[0];
                        BlockLabel.Attach(go.transform, name, new Vector3(first.CenterX, first.CenterY, 0f));
                        break;
                }
            }

            if (dark > 0) context.Log.Warn($"어둠 영역 {dark}개: 인식하는 런타임 코드가 없어 표시용으로만 만들었다");
            if (tab > 0) context.Log.Warn($"탭 공간 {tab}개: 인식하는 런타임 코드가 없어 표시용으로만 만들었다");
            foreach (var pair in named)
                context.Log.Warn($"custom 영역 '{pair.Key}' {pair.Value}개: 이름표 붙은 트리거 사각형(표시용)으로 만들었다");
        }

        private static GameObject Build(Transform root, AreaRegion area, StageAreaKind kind, string objectName,
            string label) {
            var go = new GameObject(objectName);
            go.transform.SetParent(root, false);
            go.layer = IgnoreRaycastLayer;
            go.AddComponent<StageArea>().Init(kind, area.Cells.Count, label);
            foreach (var rect in area.Rects) {
                var box = go.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.excludeLayers = ~0;
                box.offset = new Vector2(rect.CenterX, rect.CenterY);
                box.size = new Vector2(rect.Width, rect.Height);
            }

            return go;
        }
    }
}
