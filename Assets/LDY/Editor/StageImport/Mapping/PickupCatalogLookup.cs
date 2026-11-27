using LDY.FlowEditor;
using LDY.Stage;
using UnityEditor;

namespace LDY.StageImportEditor {
    // abs_{id}의 id로 Stage1PickupCatalog 항목을 찾는다.
    internal sealed class PickupCatalogLookup {
        private readonly Stage1PickupCatalog _catalog;

        public PickupCatalogLookup() {
            _catalog = AssetDatabase.LoadAssetAtPath<Stage1PickupCatalog>(FlowPaths.PickupCatalog);
        }

        public bool Loaded => _catalog != null;

        public string PrefabNameOf(string ability) {
            if (_catalog == null) return null;
            foreach (var entry in _catalog.Entries)
                if (entry.id == ability)
                    return entry.prefabName;
            return null;
        }
    }
}
