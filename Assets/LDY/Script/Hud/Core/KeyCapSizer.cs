using System;

namespace LDY.Hud.Core {
    // 키 상자 + 라벨 묶음의 치수 규칙. 단위는 캔버스 픽셀.
    [Serializable]
    public struct KeyCapSpec {
        public int capPadding;   // 키 글자 좌우 여백
        public int capHeight;    // 키 상자 높이(고정)
        public int gap;          // 키 상자와 라벨 사이
        public int paddingLeft;  // 묶음 왼쪽 여백
        public int paddingRight; // 묶음 오른쪽 여백
        public int height;       // 묶음 높이(고정). 내용은 세로 가운데

        public KeyCapSpec(int capPadding, int capHeight, int gap, int paddingLeft, int paddingRight, int height) {
            this.capPadding = capPadding;
            this.capHeight = capHeight;
            this.gap = gap;
            this.paddingLeft = paddingLeft;
            this.paddingRight = paddingRight;
            this.height = height;
        }

        // 작업표시줄 키 버튼(hud_key).
        public static KeyCapSpec Button => new KeyCapSpec(3, 12, 3, 5, 5, 18);
    }

    // 모든 좌표는 묶음 왼쪽 아래 기준, 글자 좌표만 키 상자 왼쪽 아래 기준.
    public readonly struct KeyCapMetrics {
        public readonly int CapX, CapY, CapWidth, CapHeight;
        public readonly int GlyphX, GlyphY, GlyphWidth, GlyphHeight;
        public readonly int LabelX, LabelY, LabelWidth, LabelHeight;
        public readonly int Width, Height;
        public readonly bool GlyphOverflow, LabelOverflow;

        public KeyCapMetrics(int capX, int capY, int capWidth, int capHeight,
            int glyphX, int glyphY, int glyphWidth, int glyphHeight,
            int labelX, int labelY, int labelWidth, int labelHeight,
            int width, int height, bool glyphOverflow, bool labelOverflow) {
            CapX = capX;
            CapY = capY;
            CapWidth = capWidth;
            CapHeight = capHeight;
            GlyphX = glyphX;
            GlyphY = glyphY;
            GlyphWidth = glyphWidth;
            GlyphHeight = glyphHeight;
            LabelX = labelX;
            LabelY = labelY;
            LabelWidth = labelWidth;
            LabelHeight = labelHeight;
            Width = width;
            Height = height;
            GlyphOverflow = glyphOverflow;
            LabelOverflow = labelOverflow;
        }
    }

    public static class KeyCapSizer {
        // 측정값의 부동소수 잡음(5.0000001)으로 1px 늘어나지 않게 하는 허용치.
        private const double Tolerance = 0.01;

        // glyph/label 크기는 TMP preferredWidth/preferredHeight. 라벨 폭이 0이면 간격도 넣지 않는다.
        public static KeyCapMetrics Measure(float glyphWidth, float glyphHeight, float labelWidth, float labelHeight,
            KeyCapSpec spec) {
            var glyphW = Pixels(glyphWidth);
            var glyphH = Pixels(glyphHeight);
            var labelW = Pixels(labelWidth);
            var labelH = Pixels(labelHeight);

            var capW = glyphW + spec.capPadding * 2;
            var capH = spec.capHeight;
            var capX = spec.paddingLeft;
            var capY = Center(spec.height, capH);

            var gap = labelW > 0 ? spec.gap : 0;
            var labelX = capX + capW + gap;
            var width = labelX + labelW + spec.paddingRight;

            return new KeyCapMetrics(
                capX, capY, capW, capH,
                spec.capPadding, Center(capH, glyphH), glyphW, glyphH,
                labelX, capY + Center(capH, labelH), labelW, labelH,
                width, spec.height,
                glyphH > capH, labelH > capH);
        }

        // 가로로 나란히 둔다. 첫 요소는 start + gap, 이후는 앞 요소 오른쪽 + gap. 반환은 각 요소의 왼쪽 x.
        public static int[] Row(int start, int gap, int[] widths) {
            var xs = new int[widths?.Length ?? 0];
            var x = start;
            for (var i = 0; i < xs.Length; i++) {
                x += gap;
                xs[i] = x;
                x += widths[i];
            }

            return xs;
        }

        // 바깥 높이 안에서 가운데. 반 픽셀은 위로 올린다(PixelSnap과 같은 반올림).
        public static int Center(int outer, int inner) =>
            (int)Math.Round((outer - inner) / 2.0, MidpointRounding.AwayFromZero);

        public static int Pixels(float value) => value <= 0f ? 0 : (int)Math.Ceiling(value - Tolerance);
    }
}
