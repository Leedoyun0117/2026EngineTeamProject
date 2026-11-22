using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDY.Stage {
    [CreateAssetMenu(menuName = "LDY/Stage/StageTileMap", fileName = "StageTileMap")]
    public sealed class StageTileMap : ScriptableObject {
        [Serializable]
        public struct Entry {
            public string key;
            public EntryKind kind;
            public TileBase tile;
            public TileLayerKind tileLayer;
            public GameObject prefab;
            public ClusterMode cluster;
            public Vector2Int fixedSize;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private BlockKeyIndex _index;

        public IReadOnlyList<Entry> Entries => entries;

        public void SetEntries(IEnumerable<Entry> value) {
            entries = new List<Entry>(value);
            _index = null;
        }

        public bool TryGet(string key, out Entry entry) {
            _index ??= new BlockKeyIndex(entries.ConvertAll(e => e.key));
            if (_index.TryFind(key, out var i)) {
                entry = entries[i];
                return true;
            }

            entry = default;
            return false;
        }

        private void OnValidate() {
            _index = null;
        }
    }
}
