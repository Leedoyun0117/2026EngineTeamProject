using LDY.Stage;
using LDY.StageImport.Core;

namespace LDY.StageImportEditor {
    // 대응표 항목을 코어의 묶는 방식으로 읽는다. 툴 기본 키가 아닌 항목(c_* 이름 등)에 쓰인다.
    internal sealed class MappingRules : IKeyRuleSource {
        private readonly StageTileMap _map;

        public MappingRules(StageTileMap map) {
            _map = map;
        }

        public bool TryGet(string key, out KeyRule rule) {
            rule = default;
            if (_map == null || !_map.TryGet(key, out var entry)) return false;
            rule = new KeyRule(PlacementOf(entry));
            return true;
        }

        private static Placement PlacementOf(StageTileMap.Entry entry) {
            if (entry.kind == EntryKind.Tile)
                return entry.tileLayer == TileLayerKind.Decoration ? Placement.Decoration : Placement.Tile;
            switch (entry.cluster) {
                case ClusterMode.StretchHorizontal: return Placement.HorizontalRun;
                case ClusterMode.Connected4: return Placement.Group4;
                default: return Placement.Single;
            }
        }
    }
}
