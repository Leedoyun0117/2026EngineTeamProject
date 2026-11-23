using System.Collections.Generic;
using System.Text;

namespace LDY.PlayerMerge {
    public enum DiffKind {
        Different,
        OnlyBase,
        OnlyOurs
    }

    public readonly struct DiffRow {
        public DiffRow(string key, string baseValue, string oursValue, DiffKind kind) {
            Key = key;
            BaseValue = baseValue;
            OursValue = oursValue;
            Kind = kind;
        }

        public string Key { get; }
        public string BaseValue { get; }
        public string OursValue { get; }
        public DiffKind Kind { get; }
    }

    public static class PropertyDiff {
        public static List<DiffRow> Compare(IReadOnlyDictionary<string, string> baseMap,
            IReadOnlyDictionary<string, string> oursMap) {
            var rows = new List<DiffRow>();
            foreach (var pair in baseMap) {
                if (!oursMap.TryGetValue(pair.Key, out var ours))
                    rows.Add(new DiffRow(pair.Key, pair.Value, "", DiffKind.OnlyBase));
                else if (ours != pair.Value)
                    rows.Add(new DiffRow(pair.Key, pair.Value, ours, DiffKind.Different));
            }

            foreach (var pair in oursMap)
                if (!baseMap.ContainsKey(pair.Key))
                    rows.Add(new DiffRow(pair.Key, "", pair.Value, DiffKind.OnlyOurs));

            rows.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return rows;
        }
    }

    public static class DiffMarkdown {
        public static string Render(IReadOnlyList<DiffRow> rows, string baseLabel, string oursLabel) {
            var text = new StringBuilder();
            text.AppendLine($"| 항목 | 종류 | {baseLabel} | {oursLabel} |");
            text.AppendLine("|---|---|---|---|");
            foreach (var row in rows)
                text.AppendLine($"| {Escape(row.Key)} | {row.Kind} | {Escape(row.BaseValue)} | {Escape(row.OursValue)} |");

            return text.ToString();
        }

        private static string Escape(string value) {
            return value.Replace("|", "\\|");
        }
    }
}
