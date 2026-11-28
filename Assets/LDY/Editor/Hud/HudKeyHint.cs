using TMPro;
using UnityEngine;

namespace LDY.Hud.HudEditor {
    // "[키캡] 설명" 묶음. 키캡 글자는 KeyGlyphLabel이 실행 중 HudKeyMap에서 채운다.
    internal static class HudKeyHint {
        public static readonly Color Ink = new Color32(0x10, 0x10, 0x18, 0xFF);
        public static readonly Color DisabledInk = new Color32(0x80, 0x80, 0x88, 0xFF);

        public static void Build(HudKit kit, RectTransform row, HudKey key, string caption, Color ink) {
            if (kit.Ui(row, "Keycap", out var keycap)) {
                keycap.sizeDelta = new Vector2(10f, 10f);
                kit.Image(keycap, "hud_keycap");
                HudKit.Row(keycap, new RectOffset(2, 2, 0, 0), 0f, TextAnchor.MiddleCenter);
            }

            if (kit.Ui(keycap, "Glyph", out var glyph)) {
                glyph.sizeDelta = new Vector2(6f, 10f);
                kit.Text(glyph, kit.Keys.Glyph(key), ink, TextAlignmentOptions.Center);
            }

            var label = HudKit.Component<KeyGlyphLabel>(glyph.gameObject, out var added);
            if (added)
                HudKit.SetOnce(label, "key", p => p.enumValueIndex = (int)key);
            HudKit.Wire(label, "keyMap", kit.Keys);
            HudKit.Wire(label, "label", glyph.GetComponent<TMP_Text>());

            if (kit.Ui(row, "Caption", out var text)) {
                text.sizeDelta = new Vector2(0f, 10f);
                kit.Text(text, caption, ink, TextAlignmentOptions.Left);
            }
        }

        // 내용 폭에 맞춰 늘어나는 말풍선(hud_tooltip) 안의 키 안내.
        public static RectTransform Bubble(HudKit kit, RectTransform parent, string name, Vector2 pivot, Vector2 position,
            HudKey key, string caption) {
            if (kit.Ui(parent, name, out var bubble)) {
                HudKit.Place(bubble, new Vector2(0.5f, 0f), pivot, position, new Vector2(0f, 14f));
                kit.Image(bubble, "hud_tooltip");
                HudKit.Row(bubble, new RectOffset(2, 3, 2, 2), 2f, TextAnchor.MiddleLeft);
                HudKit.FitWidth(bubble);
            }

            Build(kit, bubble, key, caption, Ink);
            return bubble;
        }
    }
}
