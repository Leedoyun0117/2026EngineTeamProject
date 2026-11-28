using TMPro;
using UnityEngine;

namespace LDY.Background {
    public sealed class TextEndFollower : MonoBehaviour {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float offset;

        public void Configure(TMP_Text target, float localOffset) {
            text = target;
            offset = localOffset;
            if (text != null) text.ForceMeshUpdate();
            Place();
        }

        private void OnEnable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            if (text != null) text.ForceMeshUpdate();
            Place();
        }

        private void OnDisable() => TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

        private void OnTextChanged(Object changed) {
            if (changed == text) Place();
        }

        private void Place() {
            if (text == null || transform.parent == null) return;
            var rect = text.rectTransform.rect;
            var endX = string.IsNullOrEmpty(text.text) ? rect.xMin : text.textBounds.max.x;
            var end = transform.parent.InverseTransformPoint(text.transform.TransformPoint(new Vector3(endX, 0f, 0f)));
            var position = transform.localPosition;
            position.x = end.x + offset;
            transform.localPosition = position;
        }
    }
}
