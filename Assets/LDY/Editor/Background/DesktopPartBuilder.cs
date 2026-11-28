using LDY.Background;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class DesktopPartBuilder : IBackgroundPartBuilder {
        private static readonly Color Teal = new Color32(0x00, 0x80, 0x80, 0xFF);

        public void Build(BackgroundBuildContext context) {
            if (!context.TryCreate(context.Root, BackgroundMapFit.DesktopName, out var go)) return;
            go.transform.SetAsFirstSibling();
            go.transform.localScale = new Vector3(context.Space.CanvasWidth / context.Space.PixelsPerUnit,
                context.Space.CanvasHeight / context.Space.PixelsPerUnit, 1f);
            var renderer = context.AddSprite(go, context.WhitePixel(), BackgroundSorting.Desktop);
            renderer.color = Teal;
            go.AddComponent<DimmedSprite>().Configure(Teal);
            context.Log.Info("Desktop 추가 (#008080, 맵에 맞출 때 크기 조정)");
        }
    }
}
