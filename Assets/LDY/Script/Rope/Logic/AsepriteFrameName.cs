using System.Globalization;

namespace LDY.Script {
    // Aseprite가 만든 스프라이트 이름("Frame_12")에서 프레임 번호를 뽑는 유틸.
    public static class AsepriteFrameName {
        private const string Prefix = "Frame_";

        // 접두사가 "Frame_"이고 뒤가 숫자일 때만 true
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
