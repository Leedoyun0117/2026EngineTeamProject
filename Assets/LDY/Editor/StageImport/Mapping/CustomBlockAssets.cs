using LDY.StageImport.Core;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 계획 키(CustomKey)로 custom 대응표 항목·프리팹을 찾는다.
    internal sealed class CustomBlockAssets {
        public CustomBlockAssets(CustomBlockMap map) {
            Map = map;
            Index = map != null ? map.BuildIndex() : new CustomNameIndex();
        }

        public CustomBlockMap Map { get; }
        public CustomNameIndex Index { get; }

        public bool TryEntry(string key, out CustomBlockMap.Entry entry, out string col) {
            entry = null;
            if (Map == null || !CustomKey.TryParse(key, out var name, out col)) {
                col = null;
                return false;
            }

            entry = Map.Find(name);
            return entry != null;
        }

        public GameObject Prefab(string key) => TryEntry(key, out var entry, out _) ? entry.prefab : null;

        // 프리팹이나 장식 그림 중 하나라도 있으면 분홍 블록이 아니다.
        public bool HasContent(string key) =>
            TryEntry(key, out var entry, out _) && (entry.prefab != null || entry.decoration != null);

        public static string DisplayName(string key) =>
            CustomKey.TryParse(key, out var name, out _) ? name : key;
    }
}
