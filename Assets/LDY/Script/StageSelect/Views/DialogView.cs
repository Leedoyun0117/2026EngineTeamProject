using System;
using LDY.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.StageSelect {
    // 모달 메시지 창. 열려 있는 동안 뒤쪽 클릭을 막는다.
    public sealed class DialogView : MonoBehaviour {
        private const float Width = 264f;
        private const float Height = 96f;

        private GameObject _root;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _message;
        private Image _icon;

        public bool IsOpen => _root != null && _root.activeSelf;
        public event Action Closed;

        public void Build(UiFactory ui) {
            var skin = ui.Skin;
            var blocker = ui.Solid("Blocker", transform, new Color(0f, 0f, 0f, 0.15f));
            blocker.raycastTarget = true;
            UiFactory.Stretch(blocker.rectTransform);
            _root = blocker.gameObject;

            var window = ui.Panel("Window", blocker.transform).rectTransform;
            window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
            window.sizeDelta = new Vector2(Width, Height);

            var bar = ui.WindowTitleBar(window, "", out _title);
            ui.TitleButton(bar.transform, WindowChrome.TitleButton.Close).onClick.AddListener(Close);

            _icon = ui.NewImage("Icon", window, skin.iconError);
            UiFactory.Place(_icon.rectTransform, 14f, 28f, 24f, 24f);
            _message = ui.NewText("Message", window, "");
            _message.textWrappingMode = TextWrappingModes.Normal;
            _message.overflowMode = TextOverflowModes.Overflow;
            _message.alignment = TextAlignmentOptions.TopLeft;
            UiFactory.Place(_message.rectTransform, 48f, 24f, Width - 60f, 40f);

            var ok = ui.NewButton("Ok", window, StageSelectText.Ok);
            var okRect = (RectTransform)ok.transform;
            okRect.anchorMin = okRect.anchorMax = okRect.pivot = new Vector2(0.5f, 0f);
            okRect.anchoredPosition = new Vector2(0f, 8f);
            okRect.sizeDelta = new Vector2(64f, 18f);
            ok.onClick.AddListener(Close);

            _root.SetActive(false);
        }

        public void Show(string title, string message, Sprite icon) {
            _title.text = title;
            _message.text = message;
            _icon.sprite = icon;
            _icon.enabled = icon != null;
            _root.SetActive(true);
        }

        public void Close() {
            if (!IsOpen) return;
            _root.SetActive(false);
            Closed?.Invoke();
        }
    }
}
