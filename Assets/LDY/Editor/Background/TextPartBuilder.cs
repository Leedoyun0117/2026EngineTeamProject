using LDY.Background;
using LDY.Background.Core;
using TMPro;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class TextPartBuilder : IBackgroundPartBuilder {
        private const float TmpWorldScale = 0.1f;

        public void Build(BackgroundBuildContext context) {
            foreach (var slot in context.Layout.Texts) BuildSlot(context, slot);
        }

        private static void BuildSlot(BackgroundBuildContext context, TextSlotSpec slot) {
            var go = BackgroundObjects.Child(context.Texts, slot.Id).gameObject;
            var text = BackgroundObjects.Ensure<TextMeshPro>(go);
            var size = context.Size(slot.Rect);
            var pixelSize = PixelFontSize.PixelSize(slot.Rect.Height, context.FontPixelSize);

            text.rectTransform.sizeDelta = new Vector2(size.x, size.y);
            text.rectTransform.localPosition = context.Center(slot.Rect);
            if (context.Font != null) text.font = context.Font;
            text.text = slot.DefaultText;
            text.enableAutoSizing = false;
            text.fontSize = context.Space.ToUnits(pixelSize) / TmpWorldScale;
            text.alignment = Alignment(slot.Align);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.sortingOrder = BackgroundSorting.Text;

            BackgroundObjects.Ensure<StageBackgroundText>(go)
                .Configure(slot.Id, text, slot.Color.ToColor(), slot.SelectedColor?.ToColor());
            ReportOverflow(context, slot, text, size, pixelSize);
        }

        private static void ReportOverflow(BackgroundBuildContext context, TextSlotSpec slot, TMP_Text text,
            Vector3 size, int pixelSize) {
            if (PixelFontSize.OverflowsHeight(slot.Rect.Height, context.FontPixelSize))
                context.Log.Warn($"텍스트 넘침(높이) {slot.Id}: 글자 {pixelSize}px > rect {slot.Rect.Height}px");
            if (string.IsNullOrEmpty(slot.DefaultText)) return;
            var width = text.GetPreferredValues(slot.DefaultText).x;
            if (width > size.x)
                context.Log.Warn($"텍스트 넘침(너비) {slot.Id}: {width * context.Space.PixelsPerUnit:0}px > rect {slot.Rect.Width}px");
        }

        private static TextAlignmentOptions Alignment(TextAlign align) {
            switch (align) {
                case TextAlign.Center: return TextAlignmentOptions.Center;
                case TextAlign.Right: return TextAlignmentOptions.Right;
                default: return TextAlignmentOptions.Left;
            }
        }
    }
}
