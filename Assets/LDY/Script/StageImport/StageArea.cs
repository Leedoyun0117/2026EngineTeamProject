using UnityEngine;

namespace LDY.StageImport {
    public enum StageAreaKind {
        Dark,
        Tab
    }

    // 툴의 영역(어둠·탭 공간) 표시. 콜라이더는 아무것과도 닿지 않게 두고, 인식하는 런타임 코드가 생기면 그쪽에서 읽는다.
    public sealed class StageArea : MonoBehaviour {
        [SerializeField] private StageAreaKind kind;
        [SerializeField] private int cellCount;

        public StageAreaKind Kind => kind;
        public int CellCount => cellCount;

        public void Init(StageAreaKind areaKind, int cells) {
            kind = areaKind;
            cellCount = cells;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            var color = kind == StageAreaKind.Dark ? new Color(0.1f, 0.05f, 0.3f, 0.35f) : new Color(0.2f, 0.9f, 0.4f, 0.25f);
            foreach (var box in GetComponents<BoxCollider2D>()) {
                var center = transform.TransformPoint(box.offset);
                Gizmos.color = color;
                Gizmos.DrawCube(center, box.size);
                Gizmos.color = new Color(color.r, color.g, color.b, 0.9f);
                Gizmos.DrawWireCube(center, box.size);
            }
        }
#endif
    }
}
