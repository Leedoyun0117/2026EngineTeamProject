using LDY.Hud.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud.HudEditor {
    // "[키캡] 설명" 묶음. 키캡 글자는 KeyGlyphLabel이 실행 중 HudKeyMap에서 채우고,
    // 키 상자·묶음 크기는 KeyCapFitter가 글자 폭에서 계산한다(KeyCapSizer).
    internal static class HudKeyHint {
        public static readonly Color Ink = new Color32(0x10, 0x10, 0x18, 0xFF);
        public static readonly Color DisabledInk = new Color32(0x80, 0x80, 0x88, 0xFF);

        // 툴팁 말풍선의 기본값(이전 레이아웃 그룹 값과 같음). 키 상자 규칙은 버튼과 같다.
        private const int BubblePadLeft = 2, BubblePadRight = 3, BubblePadVertical = 2, BubbleGap = 2;

        public static KeyCapParts Build(HudKit kit, RectTransform row, HudKey key, string caption, Color ink) {
            if (kit.Ui(row, "Keycap", out var keycap))
                kit.Image(keycap, "hud_keycap");

            if (kit.Ui(keycap, "Glyph", out var glyph))
                kit.Text(glyph, kit.Keys.Glyph(key), ink, TextAlignmentOptions.Center);

            var label = HudKit.Component<KeyGlyphLabel>(glyph.gameObject, out var added);
            if (added)
                HudKit.SetOnce(label, "key", p => p.enumValueIndex = (int)key);
            HudKit.Wire(label, "keyMap", kit.Keys);
            HudKit.Wire(label, "label", glyph.GetComponent<TMP_Text>());

            if (kit.Ui(row, "Caption", out var text))
                kit.Text(text, caption, ink, TextAlignmentOptions.Left);

            return new KeyCapParts {
                keycap = keycap,
                glyph = glyph.GetComponent<TMP_Text>(),
                caption = text.GetComponent<TMP_Text>()
            };
        }

        // 내용 폭에 맞춰 늘어나는 말풍선(hud_tooltip) 안의 키 안내.
        public static RectTransform Bubble(HudKit kit, RectTransform parent, string name, Vector2 pivot, Vector2 position,
            HudKey key, string caption) {
            if (kit.Ui(parent, name, out var bubble)) {
                HudKit.Place(bubble, new Vector2(0.5f, 0f), pivot, position, Vector2.zero);
                kit.Image(bubble, "hud_tooltip");
            }

            var parts = Build(kit, bubble, key, caption, Ink);
            Fit(kit, bubble, BubbleSpec(bubble), parts);
            return bubble;
        }

        // KeyCapFitter를 처음 붙일 때만: 크기를 다투는 레이아웃 그룹·피터를 떼고, 규칙·연결을 넣고, 한 번 맞춘다.
        // 이미 붙어 있으면 빈 연결만 채운다(사람이 고친 규칙·크기는 그대로).
        public static KeyCapFitter Fit(HudKit kit, RectTransform target, KeyCapSpec spec, params KeyCapParts[] faces) {
            var fitter = HudKit.Component<KeyCapFitter>(target.gameObject, out var added);
            HudKit.Wire(fitter, "target", target);

            var so = new SerializedObject(fitter);
            var array = so.FindProperty("faces");
            if (array.arraySize == 0) {
                array.arraySize = faces.Length;
                for (var i = 0; i < faces.Length; i++) {
                    var element = array.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("keycap").objectReferenceValue = faces[i].keycap;
                    element.FindPropertyRelative("glyph").objectReferenceValue = faces[i].glyph;
                    element.FindPropertyRelative("caption").objectReferenceValue = faces[i].caption;
                }
            }

            if (added)
                WriteSpec(so.FindProperty("spec"), spec);
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!added)
                return fitter;

            StripLayout(kit, target);
            foreach (var face in faces) {
                StripLayout(kit, face.keycap);
                if (face.keycap != null && face.keycap.parent != target)
                    StripLayout(kit, face.keycap.parent);
            }

            fitter.Fit();
            kit.Log.Info($"키 크기 자동 맞춤 {target.name}: {target.rect.width}x{target.rect.height}");
            return fitter;
        }

        // 기존 말풍선은 레이아웃 그룹에 있던 여백·간격을 이어받는다.
        private static KeyCapSpec BubbleSpec(RectTransform bubble) {
            var spec = KeyCapSpec.Button;
            spec.gap = BubbleGap;
            spec.paddingLeft = BubblePadLeft;
            spec.paddingRight = BubblePadRight;
            var top = BubblePadVertical;
            var bottom = BubblePadVertical;
            if (bubble.TryGetComponent<HorizontalLayoutGroup>(out var row)) {
                spec.gap = Mathf.RoundToInt(row.spacing);
                spec.paddingLeft = row.padding.left;
                spec.paddingRight = row.padding.right;
                top = row.padding.top;
                bottom = row.padding.bottom;
            }

            spec.height = spec.capHeight + top + bottom;
            return spec;
        }

        private static void WriteSpec(SerializedProperty p, KeyCapSpec spec) {
            p.FindPropertyRelative("capPadding").intValue = spec.capPadding;
            p.FindPropertyRelative("capHeight").intValue = spec.capHeight;
            p.FindPropertyRelative("gap").intValue = spec.gap;
            p.FindPropertyRelative("paddingLeft").intValue = spec.paddingLeft;
            p.FindPropertyRelative("paddingRight").intValue = spec.paddingRight;
            p.FindPropertyRelative("height").intValue = spec.height;
        }

        private static void StripLayout(HudKit kit, Transform node) {
            if (node == null)
                return;
            if (node.TryGetComponent<ContentSizeFitter>(out var fitter)) {
                Object.DestroyImmediate(fitter);
                kit.Log.Info("ContentSizeFitter 제거(KeyCapFitter로 대체) " + node.name);
            }
            if (node.TryGetComponent<HorizontalLayoutGroup>(out var row)) {
                Object.DestroyImmediate(row);
                kit.Log.Info("HorizontalLayoutGroup 제거(KeyCapFitter로 대체) " + node.name);
            }
        }
    }
}
