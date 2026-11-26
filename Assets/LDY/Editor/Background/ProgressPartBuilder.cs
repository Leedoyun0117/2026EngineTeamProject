using LDY.Background;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class ProgressPartBuilder : IBackgroundPartBuilder {
        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.ProgressBars) BuildProgress(context, spec);
        }

        private static void BuildProgress(BackgroundBuildContext context, ProgressSpec spec) {
            var root = BackgroundObjects.Child(context.Widgets, spec.Id);
            var fill = BackgroundObjects.SpriteChild(root, "fill", context.WhitePixel(), BackgroundSorting.Widget);
            fill.color = spec.FillColor.ToColor();

            var thumbSprite = context.LoadSprite(spec.ThumbFile);
            var thumb = BackgroundObjects.SpriteChild(root, "thumb", thumbSprite, BackgroundSorting.Widget + 1);
            var thumbSize = thumbSprite != null ? thumbSprite.rect.size : Vector2.zero;
            thumb.transform.localPosition = context.Local(spec.ThumbMinX + thumbSize.x * 0.5f, spec.ThumbTop + thumbSize.y * 0.5f);

            var center = context.Center(spec.Track);
            var size = context.Size(spec.Track);
            var track = new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);
            var minX = context.Local(spec.ThumbMinX + thumbSize.x * 0.5f, 0f).x;
            var maxX = context.Local(spec.ThumbMaxX + thumbSize.x * 0.5f, 0f).x;
            BackgroundObjects.Ensure<ProgressBar>(root.gameObject).Configure(fill.transform, thumb.transform, track, minX, maxX);
        }
    }
}
