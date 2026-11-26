using System.Globalization;

namespace LDY.Background.Core {
    public readonly struct Rgba {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public Rgba(float r, float g, float b, float a) {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    public static class HexColor {
        public static bool TryParse(string text, out Rgba color) {
            color = default;
            if (string.IsNullOrEmpty(text) || text[0] != '#') return false;
            var hex = text.Substring(1);
            if (hex.Length != 6 && hex.Length != 8) return false;
            if (!uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)) return false;
            if (hex.Length == 6) value = (value << 8) | 0xFFu;
            color = new Rgba(Channel(value, 24), Channel(value, 16), Channel(value, 8), Channel(value, 0));
            return true;
        }

        private static float Channel(uint value, int shift) => ((value >> shift) & 0xFFu) / 255f;
    }
}
