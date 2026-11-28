using LDY.Background;
using LDY.Background.Core;
using TMPro;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class CaretPartBuilder : IBackgroundPartBuilder {
        public void Build(BackgroundBuildContext context) {
            foreach (var spec in context.Layout.Carets) BuildCaret(context, spec);
        }

        private static void BuildCaret(BackgroundBuildContext context, CaretSpec spec) {
            var caret = context.Anim.Find(spec.SheetId);
            var text = context.Texts.Find(spec.TextId);
            if (caret == null || text == null || caret.GetComponent<TextEndFollower>() != null) return;
            var renderer = caret.GetComponent<SpriteRenderer>();
            var width = renderer != null && renderer.sprite != null ? renderer.sprite.rect.width : 0f;
            var offset = context.Space.ToUnits(spec.GapPixels + width * 0.5f);
            caret.gameObject.AddComponent<TextEndFollower>().Configure(text.GetComponent<TMP_Text>(), offset);
            context.Log.Info($"{spec.SheetId}: {spec.TextId} 끝 + {spec.GapPixels}px를 따라감");
        }
    }
}
