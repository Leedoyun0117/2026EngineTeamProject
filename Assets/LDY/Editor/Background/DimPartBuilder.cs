using LDY.Background;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class DimPartBuilder : IBackgroundPartBuilder {
        private const string ReferenceName = "Reference";

        public void Build(BackgroundBuildContext context) {
            var added = 0;
            foreach (var group in new[] { context.Static, context.Widgets, context.Anim })
            foreach (var renderer in group.GetComponentsInChildren<SpriteRenderer>(true)) {
                if (renderer.name == ReferenceName || renderer.GetComponent<DimmedSprite>() != null) continue;
                if (renderer.GetComponentInParent<VisualizerBars>(true) != null ||
                    renderer.GetComponentInParent<NoteSpawner>(true) != null) continue;
                renderer.gameObject.AddComponent<DimmedSprite>().Configure(renderer.color);
                added++;
            }

            if (added > 0) context.Log.Info($"어둡게 대상 추가 {added}개 (기준 색 = 현재 색)");
        }
    }
}
