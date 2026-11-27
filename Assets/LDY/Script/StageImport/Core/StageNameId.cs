namespace LDY.StageImport.Core {
    // 맵 이름이 "{숫자}-{숫자|T}"이면 그 이름을 스테이지 id로 쓴다. 앞뒤 공백은 무시하고, 숫자 앞의 0은 뗀다("02-03" → "2-3").
    public static class StageNameId {
        public const string TutorialMark = "T";

        public static bool TryParse(string name, out int world, out string stageId) {
            world = 0;
            stageId = null;
            if (name == null) return false;

            var text = name.Trim();
            var dash = text.IndexOf('-');
            if (dash <= 0 || dash == text.Length - 1 || dash != text.LastIndexOf('-')) return false;

            var left = text.Substring(0, dash);
            var right = text.Substring(dash + 1);
            if (!TryNumber(left, out var w)) return false;

            string second;
            if (right == TutorialMark) second = TutorialMark;
            else if (TryNumber(right, out var number)) second = number.ToString();
            else return false;

            world = w;
            stageId = $"{w}-{second}";
            return true;
        }

        // char.IsDigit은 전각·다른 문자권 숫자도 받아서 '0'~'9'만 따로 본다.
        private static bool TryNumber(string text, out int value) {
            value = 0;
            if (text.Length == 0 || text.Length > 6) return false;
            foreach (var c in text)
                if (c < '0' || c > '9')
                    return false;
            return int.TryParse(text, out value);
        }
    }
}
