using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LDY.PlayerMerge {
    public readonly struct CheckRow {
        public CheckRow(string target, string label, bool passed, string detail) {
            Target = target;
            Label = label;
            Passed = passed;
            Detail = detail;
        }

        public string Target { get; }
        public string Label { get; }
        public bool Passed { get; }
        public string Detail { get; }
    }

    // LDY/Player/Validate의 판정 규칙. 값만 받아 Unity 없이 테스트한다.
    public static class PlayerRules {
        public const int FreezeRotationBit = 4;
        public const float GroundCheckReach = 0.1f;
        private const float Epsilon = 0.0001f;

        public static bool RotationFrozen(int constraints) => (constraints & FreezeRotationBit) != 0;

        public static bool MaskFilled(int bits) => bits != 0;

        public static bool UnitScale(Float2 scale) =>
            Math.Abs(scale.X - 1f) < Epsilon && Math.Abs(scale.Y - 1f) < Epsilon;

        public static bool GroundCheckReachesBottom(float boxCenterY, float boxHeight, float colliderBottom) {
            var half = boxHeight * 0.5f;
            return colliderBottom >= boxCenterY - half - GroundCheckReach
                   && colliderBottom <= boxCenterY + half + GroundCheckReach;
        }

        public static bool GroundCheckFitsWidth(float boxWidth, float colliderWidth) => boxWidth <= colliderWidth + Epsilon;

        public static bool FeetAligned(float feetY, float colliderBottom, float pixelWorld) =>
            Math.Abs(feetY - colliderBottom) <= pixelWorld + Epsilon;

        public static bool SingleVisual(int enabledRenderers, int animators) => enabledRenderers == 1 && animators == 0;

        public static List<string> Missing(IEnumerable<string> present, IEnumerable<string> required) {
            var have = new HashSet<string>(present);
            return required.Where(name => !have.Contains(name)).ToList();
        }

        public static List<string> Forbidden(IEnumerable<string> present, IEnumerable<string> forbidden) {
            var have = new HashSet<string>(present);
            return forbidden.Where(have.Contains).ToList();
        }
    }

    public static class CheckTable {
        public static string Render(IReadOnlyList<CheckRow> rows) {
            var text = new StringBuilder();
            text.AppendLine("| 대상 | 검사 | 결과 | 내용 |");
            text.AppendLine("|---|---|---|---|");
            foreach (var row in rows)
                text.AppendLine($"| {row.Target} | {row.Label} | {(row.Passed ? "OK" : "X")} | {row.Detail} |");

            var failed = rows.Count(r => !r.Passed);
            text.Append(failed == 0 ? $"전체 {rows.Count}개 통과" : $"실패 {failed}개 / 전체 {rows.Count}개");
            return text.ToString();
        }
    }
}
