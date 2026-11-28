using System.Linq;
using LDY.Background;
using LDY.Background.Core;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class VisualizerPartBuilder : IBackgroundPartBuilder {
        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.Visualizers) BuildVisualizer(context, spec);
        }

        private static void BuildVisualizer(BackgroundBuildContext context, VisualizerSpec spec) {
            if (!context.TryCreate(context.Widgets, spec.Id, out var root)) return;
            var sprite = context.LoadSprite(spec.SegmentFile);
            var spriteSize = sprite != null ? sprite.rect.size : new Vector2(spec.BarWidth, spec.SegmentHeight);
            var scale = new Vector3(spec.BarWidth / spriteSize.x, spec.SegmentHeight / spriteSize.y, 1f);

            var gridWidth = spec.Bars * spec.BarWidth + (spec.Bars - 1) * spec.BarGap;
            var gridHeight = (spec.Segments - 1) * spec.SegmentStep + spec.SegmentHeight;
            var left = spec.Rect.X + (spec.Rect.Width - gridWidth) * 0.5f;
            var bottom = spec.Rect.Y + spec.Rect.Height - (spec.Rect.Height - gridHeight) * 0.5f;

            var grid = new SpriteRenderer[spec.Bars * spec.Segments];
            for (var bar = 0; bar < spec.Bars; bar++)
            for (var s = 0; s < spec.Segments; s++) {
                var segment = context.NewSprite(root.transform, $"bar{bar:00}_seg{s:00}", sprite, BackgroundSorting.Widget);
                var x = left + bar * (spec.BarWidth + spec.BarGap) + spec.BarWidth * 0.5f;
                var y = bottom - s * spec.SegmentStep - spec.SegmentHeight * 0.5f;
                segment.transform.localPosition = context.Local(x, y);
                segment.transform.localScale = scale;
                grid[bar * spec.Segments + s] = segment;
            }

            root.AddComponent<VisualizerBars>().Configure(grid, spec.Bars, spec.Segments,
                spec.Ramp.Select(c => c.ToColor()).ToArray(), spec.Off.ToColor(), spec.FrameMs);
        }
    }
}
