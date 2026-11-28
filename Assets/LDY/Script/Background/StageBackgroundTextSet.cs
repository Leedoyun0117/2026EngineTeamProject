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
            public bool overrideRect;
            [Tooltip("json 픽셀 좌표, 왼쪽 위 기준 x, y, w, h")]
            public Rect rect;
            public bool show;
            [HideInInspector] public bool showDecided;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool TryGet(string id, out Entry entry) => TryFind(entries, id, out entry);

        public bool AddMissing(string id, string text) {
            if (TryGet(id, out _)) return false;
            entries.Add(new Entry { id = id, text = text });
            return true;
        }

        public bool SetRectIfMissing(string id, Rect rect) {
            var index = entries.FindIndex(e => e.id == id);
            if (index < 0 || entries[index].overrideRect) return false;
            var entry = entries[index];
            entry.overrideRect = true;
            entry.rect = rect;
            entries[index] = entry;
            return true;
        }

        public bool DecideShowIfMissing(string id, bool show) {
            var index = entries.FindIndex(e => e.id == id);
            if (index < 0 || entries[index].showDecided) return false;
            var entry = entries[index];
            entry.show = show;
            entry.showDecided = true;
            entries[index] = entry;
            return true;
        }

        public static bool TryFind(IReadOnlyList<Entry> list, string id, out Entry entry) {
            foreach (var candidate in list) {
                if (candidate.id != id) continue;
                entry = candidate;
                return true;
            }

            entry = default;
            return false;
        }
    }
}
