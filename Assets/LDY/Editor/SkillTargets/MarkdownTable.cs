using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LDY.SkillTargetsEditor {
    internal sealed class MarkdownTable {
        private readonly string[] _headers;
        private readonly List<string[]> _rows = new List<string[]>();

        public MarkdownTable(params string[] headers) {
            _headers = headers;
        }

        public int Count => _rows.Count;

        public void Add(params string[] cells) => _rows.Add(cells);

        public void AppendTo(StringBuilder sb) {
            sb.AppendLine(Line(_headers));
            sb.AppendLine(Line(_headers.Select(_ => "---")));
            foreach (var row in _rows) sb.AppendLine(Line(row));
        }

        private static string Line(IEnumerable<string> cells) =>
            "| " + string.Join(" | ", cells.Select(c => (c ?? "").Replace("|", "/").Replace("\n", " "))) + " |";
    }
}
