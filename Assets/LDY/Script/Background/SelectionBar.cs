using UnityEngine;

namespace LDY.Background {
    public sealed class SelectionBar : MonoBehaviour {
        [SerializeField] private StageBackgroundText[] rows = new StageBackgroundText[0];
        [SerializeField] private Vector3 firstRowPosition;
        [SerializeField] private float rowStep;
        [SerializeField, Min(0)] private int selectedRow;

        public int SelectedRow => selectedRow;

        public void Configure(StageBackgroundText[] rowTexts, Vector3 firstRow, float step, int row) {
            rows = rowTexts;
            firstRowPosition = firstRow;
            rowStep = step;
            Select(row);
        }

        public void Select(int row) {
            selectedRow = rows.Length > 0 ? Mathf.Clamp(row, 0, rows.Length - 1) : Mathf.Max(0, row);
            transform.localPosition = firstRowPosition + Vector3.down * (rowStep * selectedRow);
            for (var i = 0; i < rows.Length; i++)
                if (rows[i] != null)
                    rows[i].SetSelected(i == selectedRow);
        }

        private void OnEnable() => Select(selectedRow);
    }
}
