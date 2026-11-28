using System;
using System.Globalization;

namespace LDY.SkillTargets.Core {
    public static class FieldValues {
        public const string Missing = "(필드 없음)";

        public static bool Same(string a, string b) {
            if (TryNumber(a, out var x) && TryNumber(b, out var y)) return Math.Abs(x - y) < 1e-4;
            return string.Equals(a, b, StringComparison.Ordinal);
        }

        private static bool TryNumber(string text, out double value) =>
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
