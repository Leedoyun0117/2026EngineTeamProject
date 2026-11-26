using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Stage {
    // 흡수 UI 목록의 유일한 출처. Setup Art와 검증 메뉴가 이 에셋만 읽는다.
    [CreateAssetMenu(menuName = "LDY/Stage/Stage1PickupCatalog", fileName = "Stage1PickupCatalog")]
    public sealed class Stage1PickupCatalog : ScriptableObject {
        [Serializable]
        public struct Entry {
            public string id;
            public string asepritePath;
            public string prefabName;
            public Color abilityColor;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool ContainsPrefab(string prefabName) {
            foreach (var entry in entries)
                if (entry.prefabName == prefabName) return true;
            return false;
        }

        public bool ContainsId(string id) {
            foreach (var entry in entries)
                if (entry.id == id) return true;
            return false;
        }

        // 같은 id가 있으면 값만 바꾸고 없으면 끝에 붙인다. 바뀐 게 있으면 true.
        public bool Upsert(Entry entry) {
            for (var i = 0; i < entries.Count; i++) {
                if (entries[i].id != entry.id) continue;
                if (entries[i].Equals(entry)) return false;
                entries[i] = entry;
                return true;
            }

            entries.Add(entry);
            return true;
        }
    }
}
