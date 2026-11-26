using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.FontsEditor {
    public sealed class PixelFontCatalog : ScriptableObject {
        [Serializable]
        public struct Entry {
            public string sourcePath;
            public int samplingPointSize;
            public string outputPath;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        // outputPath가 같은 항목을 바꾸고 없으면 붙인다. 바뀐 게 있으면 true.
        public bool Upsert(Entry entry) {
            var index = entries.FindIndex(e => e.outputPath == entry.outputPath);
            if (index < 0) {
                entries.Add(entry);
                return true;
            }

            if (entries[index].Equals(entry)) return false;
            entries[index] = entry;
            return true;
        }
    }
}
