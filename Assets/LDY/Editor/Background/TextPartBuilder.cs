using LDY.Background;
using LDY.Background.Core;
using TMPro;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class TextPartBuilder : IBackgroundPartBuilder {
        public void Build(BackgroundBuildContext context) {
            foreach (var slot in context.Layout.Texts) BuildSlot(context, slot);
        }

        private static void BuildSlot(BackgroundBuildContext context, TextSlotSpec slot) {
            if (!context.TryCreate(context.Texts, slot.Id, out var go)) return;
            var rect = context.EffectiveRect(slot);
            var wraps = TextFit.Wraps(rect);
            var text = go.AddComponent<TextMeshPro>();
            var size = context.Size(rect);

            text.rectTransform.sizeDelta = new Vector2(size.x, size.y);
            text.rectTransform.localPosition = context.Center(rect);
            BackgroundTextStyle.Apply(text, context.Font, context.Space);
            text.text = context.TextOf(slot);
            text.alignment = Alignment(slot.Align);
            text.textWrappingMode = wraps ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            text.overflowMode = slot.Mask ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
            text.sortingLayerID = SortingLayer.NameToID(context.SortingLayerName);
            text.sortingOrder = BackgroundSorting.Text;

            go.AddComponent<StageBackgroundText>()
                .Configure(slot.Id, text, slot.Color.ToColor(), slot.SelectedColor?.ToColor());
            if (slot.Mask) context.Log.Info($"{slot.Id}: mask -> Truncate (rect 밖 글자 숨김)");
            ReportOverflow(context, slot, rect, wraps);
        }

        private static void ReportOverflow(BackgroundBuildContext context, TextSlotSpec slot, PixelRect rect, bool wraps) {
            var value = context.TextOf(slot);
            if (TextFit.PixelSize > rect.Height)
                context.Log.Warn($"텍스트 넘침(높이) {slot.Id}: 글자 {TextFit.PixelSize}px > rect {rect.Height}px");
            if (string.IsNullOrEmpty(value) || slot.Mask) return;
            if (wraps) {
                var height = context.Meter.WrappedHeightPixels(value, rect.Width);
                if (height > rect.Height)
                    context.Log.Warn($"텍스트 넘침(줄바꿈 높이) {slot.Id}: {height:0}px > rect {rect.Height}px");
                return;
            }

            var width = context.Meter.WidthPixels(value);
            if (width > rect.Width)
                context.Log.Warn($"텍스트 넘침(너비) {slot.Id}: {width:0}px > rect {rect.Width}px");
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
