namespace LDY.SkillTargets.Core {
    // 프리팹 이름 패턴. * 는 아무 글자 여러 개, ? 는 한 글자. 대소문자 무시.
    public static class NamePattern {
        public static bool Matches(string pattern, string name) {
            if (string.IsNullOrEmpty(pattern) || name == null) return false;
            return Match(pattern.ToLowerInvariant(), 0, name.ToLowerInvariant(), 0);
        }

        private static bool Match(string p, int pi, string s, int si) {
            while (pi < p.Length) {
                var c = p[pi];
                if (c == '*') {
                    for (var k = si; k <= s.Length; k++)
                        if (Match(p, pi + 1, s, k)) return true;
                    return false;
                }

                if (si >= s.Length || (c != '?' && c != s[si])) return false;
                pi++;
                si++;
            }

            return si == s.Length;
        }
    }
}
