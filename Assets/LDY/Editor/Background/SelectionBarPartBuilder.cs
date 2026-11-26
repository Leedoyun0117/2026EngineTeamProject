using System.Linq;
using LDY.Background;

namespace LDY.BackgroundEditor {
    internal sealed class SelectionBarPartBuilder : IBackgroundPartBuilder {
        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.SelectionBars) BuildBar(context, spec);
        }

        private static void BuildBar(BackgroundBuildContext context, SelectionBarSpec spec) {
            var bar = BackgroundObjects.SpriteChild(context.Widgets, spec.Id, context.WhitePixel(), BackgroundSorting.Widget);
            bar.color = spec.Color.ToColor();
            bar.transform.localScale = context.Size(spec.Rect);

            var rows = spec.RowTextIds
                .Select(id => context.Texts.Find(id))
                .Where(t => t != null)
                .Select(t => t.GetComponent<StageBackgroundText>())
                .ToArray();
            BackgroundObjects.Ensure<SelectionBar>(bar.gameObject)
                .Configure(rows, context.Center(spec.Rect), context.Space.ToUnits(spec.RowStep), spec.DefaultRow);
        }
    }
}
