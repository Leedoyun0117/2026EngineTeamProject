using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Background {
    [CreateAssetMenu(menuName = "LDY/Background/StageBackgroundTextSet", fileName = "StageBackgroundTextSet")]
    public sealed class StageBackgroundTextSet : ScriptableObject {
        [Serializable]
        public struct Entry {
            public string id;
            public string text;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool TryGet(string id, out string text) => TryFind(entries, id, out text);

        public bool AddMissing(string id, string text) {
            if (TryGet(id, out _)) return false;
            entries.Add(new Entry { id = id, text = text });
            return true;
        }

        public static bool TryFind(IReadOnlyList<Entry> list, string id, out string text) {
            foreach (var entry in list) {
                if (entry.id != id) continue;
                text = entry.text;
                return true;
            }

            text = null;
            return false;
        }
    }
}
