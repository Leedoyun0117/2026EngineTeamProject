using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud {
    public sealed class ToastView : MonoBehaviour {
        [SerializeField] private GameObject body;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text message;

        public void Show(Sprite sprite, string text) {
            if (icon != null)
                icon.sprite = sprite;
            if (message != null)
                message.text = text;
            if (body != null && !body.activeSelf)
                body.SetActive(true);
        }

        public void Hide() {
            if (body != null && body.activeSelf)
                body.SetActive(false);
        }
    }
}
