using LDY.Background.Core;
using TMPro;
using UnityEngine;

namespace LDY.Background {
    public sealed class StageBackgroundText : MonoBehaviour, IBackgroundDimmable {
        [SerializeField] private string id;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color color = Color.black;
        [SerializeField] private bool hasSelectedColor;
        [SerializeField] private Color selectedColor = Color.white;

        private bool _selected;
        private float _dim;

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

        public void SetVisible(bool visible) {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }

        public void SetRect(PixelSpace space, PixelRect rect) {
            if (label == null) return;
            var center = space.CenterOf(rect);
            var size = space.SizeOf(rect);
            label.rectTransform.localPosition = new Vector3(center.X, center.Y, label.rectTransform.localPosition.z);
            label.rectTransform.sizeDelta = new Vector2(size.X, size.Y);
        }

        public void ApplyDim(float dim) {
            _dim = dim;
            SetSelected(_selected);
        }

        public void SetSelected(bool selected) {
            _selected = selected;
            if (label != null) label.color = DimColor.Of(selected && hasSelectedColor ? selectedColor : color, _dim);
        }
    }
}
