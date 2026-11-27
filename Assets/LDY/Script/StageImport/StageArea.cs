using UnityEngine;

namespace LDY.StageImport {
    public enum StageAreaKind {
        Dark,
        Tab,

        // custom 대응표에서 영역으로 지정한 이름(label에 이름).
        Custom
    }

    // 툴의 영역(어둠·탭 공간·이름 붙은 custom 영역) 표시. 콜라이더는 아무것과도 닿지 않게 두고, 인식하는 런타임 코드가 생기면 그쪽에서 읽는다.
    public sealed class StageArea : MonoBehaviour {
        [SerializeField] private StageAreaKind kind;
        [SerializeField] private int cellCount;
        [SerializeField] private string label;

        public StageAreaKind Kind => kind;
        public int CellCount => cellCount;
        public string Label => label;

        public void Init(StageAreaKind areaKind, int cells, string name = "") {
            kind = areaKind;
            cellCount = cells;
            label = name;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            var color = ColorOf(kind);
            foreach (var box in GetComponents<BoxCollider2D>()) {
                var center = transform.TransformPoint(box.offset);
                Gizmos.color = color;
                Gizmos.DrawCube(center, box.size);
                Gizmos.color = new Color(color.r, color.g, color.b, 0.9f);
                Gizmos.DrawWireCube(center, box.size);
            }
        }

        private static Color ColorOf(StageAreaKind areaKind) {
            switch (areaKind) {
                case StageAreaKind.Dark: return new Color(0.1f, 0.05f, 0.3f, 0.35f);
                case StageAreaKind.Tab: return new Color(0.2f, 0.9f, 0.4f, 0.25f);
                default: return new Color(1f, 0.6f, 0.1f, 0.25f);
            }
        }
#endif
    }
}
