using System;
using LDY.Hud.Core;
using TMPro;
using UnityEngine;

namespace LDY.Hud {
    [Serializable]
    public sealed class KeyCapParts {
        public RectTransform keycap;
        public TMP_Text glyph;
        public TMP_Text caption;
    }

    // 키 글자·라벨 폭에 맞춰 키 상자와 묶음(target) 크기를 정한다. 크기·위치만 바꾸고 노드는 만들지 않는다.
    // 활성/비활성 모양처럼 같은 키를 그리는 면이 여럿이면 가장 넓은 면에 맞춰 전부 같은 치수로 둔다.
    public sealed class KeyCapFitter : MonoBehaviour {
        [SerializeField] private KeyCapSpec spec = KeyCapSpec.Button;
        [SerializeField] private RectTransform target;
        [SerializeField] private KeyCapParts[] faces = Array.Empty<KeyCapParts>();

        private string[] _seen = Array.Empty<string>();

        public RectTransform Target => target;

        private void OnEnable() => Fit();

        private void LateUpdate() {
            if (TextChanged())
                Fit();
        }

        [ContextMenu("Fit Now")]
        public void Fit() {
            if (target == null || faces == null || faces.Length == 0) {
                HudWarnings.Once("keycap-wiring:" + PathOf(transform), $"{PathOf(transform)}: KeyCapFitter 연결이 비어 있어 크기를 못 맞춤", this);
                return;
            }

            Remember();
            var metrics = Measure();
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, metrics.Width);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, metrics.Height);

            foreach (var face in faces) {
                if (face == null)
                    continue;
                Put(face.keycap, metrics.CapX, metrics.CapY, metrics.CapWidth, metrics.CapHeight);
                Put(face.glyph, metrics.GlyphX, metrics.GlyphY, metrics.GlyphWidth, metrics.GlyphHeight);
                Put(face.caption, metrics.LabelX, metrics.LabelY, metrics.LabelWidth, metrics.LabelHeight);
            }

            foreach (var face in faces) {
                if (face == null)
                    continue;
                if (metrics.GlyphOverflow)
                    Overflow(face.glyph, metrics.GlyphHeight, metrics.CapHeight);
                if (metrics.LabelOverflow)
                    Overflow(face.caption, metrics.LabelHeight, metrics.CapHeight);
                break;
            }
        }

        private KeyCapMetrics Measure() {
            Vector2 glyph = default, label = default;
            foreach (var face in faces) {
                if (face == null)
                    continue;
                glyph = Vector2.Max(glyph, Preferred(face.glyph));
                label = Vector2.Max(label, Preferred(face.caption));
            }

            return KeyCapSizer.Measure(glyph.x, glyph.y, label.x, label.y, spec);
        }

        private static Vector2 Preferred(TMP_Text text) =>
            text == null || string.IsNullOrEmpty(text.text) ? Vector2.zero : text.GetPreferredValues(text.text);

        private static void Put(Component node, int x, int y, int width, int height) {
            if (node == null)
                return;

            var rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static string TextOf(TMP_Text text) => text != null ? text.text : null;

        private void Overflow(TMP_Text text, int textHeight, int boxHeight) {
            if (text == null)
                return;

            var path = PathOf(text.transform);
            HudWarnings.Once("overflow:" + path, $"{path}: 글자 높이 {textHeight}px가 칸 높이 {boxHeight}px를 넘음(\"{text.text}\")", text);
        }

        private bool TextChanged() {
            var i = 0;
            foreach (var face in faces) {
                if (face == null)
                    continue;
                if (i + 1 >= _seen.Length || _seen[i] != TextOf(face.glyph) || _seen[i + 1] != TextOf(face.caption))
                    return true;
                i += 2;
            }

            return false;
        }

        private void Remember() {
            if (_seen.Length != faces.Length * 2)
                _seen = new string[faces.Length * 2];
            var i = 0;
            foreach (var face in faces) {
                if (face == null)
                    continue;
                _seen[i++] = TextOf(face.glyph);
                _seen[i++] = TextOf(face.caption);
            }
        }

        internal static string PathOf(Transform node) =>
            node.parent == null ? node.name : PathOf(node.parent) + "/" + node.name;
    }
}
