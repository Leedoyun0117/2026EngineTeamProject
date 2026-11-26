using TMPro;
using UnityEngine;

namespace LDY.Background {
    public sealed class StageBackgroundText : MonoBehaviour {
        [SerializeField] private string id;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color color = Color.black;
        [SerializeField] private bool hasSelectedColor;
        [SerializeField] private Color selectedColor = Color.white;

        public string Id => id;

        public void Configure(string textId, TMP_Text target, Color normal, Color? selected) {
            id = textId;
            label = target;
            color = normal;
            hasSelectedColor = selected.HasValue;
            selectedColor = selected ?? normal;
            SetSelected(false);
        }

        public void SetText(string value) {
            if (label != null) label.text = value;
        }

        public void SetSelected(bool selected) {
            if (label != null) label.color = selected && hasSelectedColor ? selectedColor : color;
        }
    }
}
