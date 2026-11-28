using System.Linq;
using LDY.Background;
using LDY.Background.Core;

namespace LDY.BackgroundEditor {
    internal sealed class SelectionBarPartBuilder : IBackgroundPartBuilder {
        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.SelectionBars) BuildBar(context, spec);
        }

        private static void BuildBar(BackgroundBuildContext context, SelectionBarSpec spec) {
            if (!context.TryCreate(context.Widgets, spec.Id, out var go)) return;
            var bar = context.AddSprite(go, context.WhitePixel(), BackgroundSorting.Widget);
            bar.color = spec.Color.ToColor();
            go.transform.localScale = context.Size(spec.Rect);

            var rows = spec.RowTextIds
                .Select(id => context.Texts.Find(id))
                .Where(t => t != null)
                .Select(t => t.GetComponent<StageBackgroundText>())
                .ToArray();
            go.AddComponent<SelectionBar>()
                .Configure(rows, context.Center(spec.Rect), context.Space.ToUnits(spec.RowStep), spec.DefaultRow);
        }
    }
}
