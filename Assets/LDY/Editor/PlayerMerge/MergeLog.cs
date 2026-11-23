using System.Collections.Generic;

namespace LDY.Script {
    internal sealed class MergeLog {
        public List<string> Changed { get; } = new();
        public List<string> Fixed { get; } = new();
        public List<string> Notes { get; } = new();

        public bool HasChanges => Changed.Count > 0 || Fixed.Count > 0;
    }
}
