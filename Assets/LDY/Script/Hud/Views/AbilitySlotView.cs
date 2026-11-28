using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud {
    public sealed class AbilitySlotView : MonoBehaviour {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;

        public void Show(Sprite sprite, string text) {
            if (icon != null)
                icon.sprite = sprite;
            if (label != null)
                label.text = text;
        }
    }
}
