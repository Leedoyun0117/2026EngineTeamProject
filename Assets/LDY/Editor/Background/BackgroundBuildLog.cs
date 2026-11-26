using System.Collections.Generic;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal sealed class BackgroundBuildLog {
        private readonly List<string> _lines = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        public void Info(string line) => _lines.Add(line);
        public void Warn(string line) => _warnings.Add(line);

        public void Print(string title) {
            Debug.Log(title + "\n  " + string.Join("\n  ", _lines));
            foreach (var warning in _warnings) Debug.LogWarning(title + ": " + warning);
        }
    }
}
