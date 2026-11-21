using System;
using System.Collections.Generic;

namespace LDY.Stage {
    public sealed class BlockKeyIndex {
        private readonly Dictionary<string, int> _indexByKey = new Dictionary<string, int>(StringComparer.Ordinal);

        public BlockKeyIndex(IReadOnlyList<string> keys) {
            for (var i = 0; i < keys.Count; i++) {
                if (string.IsNullOrEmpty(keys[i]) || _indexByKey.ContainsKey(keys[i])) continue;
                _indexByKey.Add(keys[i], i);
            }
        }

        public bool TryFind(string key, out int index) {
            index = -1;
            return key != null && _indexByKey.TryGetValue(key, out index);
        }
    }
}
