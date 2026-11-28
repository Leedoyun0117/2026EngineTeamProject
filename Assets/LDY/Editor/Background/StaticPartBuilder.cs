using LDY.Background;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class StaticPartBuilder : IBackgroundPartBuilder {
        private const string WindowName = "window_base";
        private const string ReferenceName = "Reference";

        public void Build(BackgroundBuildContext context) {
            var space = context.Space;
            var center = context.Local(space.CanvasWidth * 0.5f, space.CanvasHeight * 0.5f);
            if (context.TryCreate(context.Static, WindowName, out var window)) {
                var sprite = context.LoadSprite(context.Layout.StaticFile);
                var size = sprite != null ? sprite.rect.size : new Vector2(space.CanvasWidth, space.CanvasHeight);
                window.transform.localPosition =
                    context.Local(context.Layout.StaticX + size.x * 0.5f, context.Layout.StaticY + size.y * 0.5f);
                context.AddSprite(window, sprite, BackgroundSorting.Static);
            }

            if (context.Layout.ReferenceFile == null) {
                context.Log.Warn("검증용 레퍼런스 이미지 없음");
                return;
            }

            if (!context.TryCreate(context.Static, ReferenceName, out var reference)) return;
            reference.transform.localPosition = center;
            var renderer = context.AddSprite(reference, context.LoadSprite(context.Layout.ReferenceFile),
                BackgroundSorting.Text + 1);
            renderer.color = new Color(1f, 1f, 1f, 0.5f);
            reference.SetActive(false);
            context.Log.Info("검증 기준 " + context.Layout.ReferenceFile + " (Static/Reference, 기본 꺼짐)");
        }
    }
}
