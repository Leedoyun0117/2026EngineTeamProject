using System.Globalization;

namespace LDY.Script {
    public static class AsepriteFrameName {
        private const string Prefix = "Frame_";

        public static bool TryParse(string name, out int frame) {
            frame = -1;
            if (name == null || !name.StartsWith(Prefix))
                return false;

            if (int.TryParse(name.Substring(Prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)) {
                frame = parsed;
                return true;
            }

            return false;
        }
    }
}
