using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LDY.FlowEditor {
    internal sealed class FlowLog {
        private readonly string _title;
        private readonly List<string> _lines = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        public FlowLog(string title) {
            _title = title;
        }

        public bool HasWarnings => _warnings.Count > 0;

        public void Info(string line) => _lines.Add(line);
        public void Warn(string line) => _warnings.Add(line);

        public void Print() {
            var sb = new StringBuilder(_title);
            foreach (var line in _lines) sb.Append("\n  ").Append(line);
            Debug.Log(sb.ToString());
            foreach (var warning in _warnings) Debug.LogWarning(_title + ": " + warning);
        }
    }
}
