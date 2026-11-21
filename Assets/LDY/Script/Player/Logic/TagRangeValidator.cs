using System.Collections.Generic;
using System.Linq;

namespace LDY.Script {
    // 파일의 태그 범위가 기대한 태그 이름과 임포트된 스프라이트 번호에 맞는지 검사한다.
    public static class TagRangeValidator {
        public static List<string> Validate(AsepriteTagInfo info, IReadOnlyList<bool> spritePresent,
            IReadOnlyCollection<string> expectedNames) {
            var errors = new List<string>();
            CheckSpriteCount(info, spritePresent, errors);
            CheckNames(info, expectedNames, errors);
            foreach (var tag in info.Tags)
                CheckRange(tag, info.FrameCount, spritePresent, errors);

            CheckOverlap(info.Tags, errors);
            return errors;
        }

        private static void CheckSpriteCount(AsepriteTagInfo info, IReadOnlyList<bool> present, List<string> errors) {
            var count = present.Count(p => p);
            if (present.Count != info.FrameCount || count != info.FrameCount)
                errors.Add($"스프라이트 {count}개 / 프레임 {info.FrameCount}개로 맞지 않음");
        }

        private static void CheckNames(AsepriteTagInfo info, IReadOnlyCollection<string> expected, List<string> errors) {
            var names = info.Tags.Select(t => t.Name).ToList();
            foreach (var duplicate in names.GroupBy(n => n).Where(g => g.Count() > 1))
                errors.Add($"태그 '{duplicate.Key}' 가 {duplicate.Count()}번 나옴");

            foreach (var missing in expected.Where(n => !names.Contains(n)))
                errors.Add($"태그 '{missing}' 가 파일에 없음");

            foreach (var unknown in names.Where(n => !expected.Contains(n)).Distinct())
                errors.Add($"태그 '{unknown}' 는 기대한 이름이 아님");
        }

        private static void CheckRange(AsepriteTag tag, int frameCount, IReadOnlyList<bool> present, List<string> errors) {
            if (tag.From < 0 || tag.To < tag.From || tag.To >= frameCount) {
                errors.Add($"태그 '{tag.Name}' 범위 {tag.From}-{tag.To} 가 프레임 0-{frameCount - 1} 밖임");
                return;
            }

            for (var i = tag.From; i <= tag.To; i++)
                if (i >= present.Count || !present[i])
                    errors.Add($"태그 '{tag.Name}' 의 Frame_{i} 스프라이트가 없음");
        }

        private static void CheckOverlap(IReadOnlyList<AsepriteTag> tags, List<string> errors) {
            for (var a = 0; a < tags.Count; a++)
            for (var b = a + 1; b < tags.Count; b++)
                if (tags[a].From <= tags[b].To && tags[b].From <= tags[a].To)
                    errors.Add($"태그 '{tags[a].Name}' 와 '{tags[b].Name}' 범위가 겹침");
        }
    }
}
