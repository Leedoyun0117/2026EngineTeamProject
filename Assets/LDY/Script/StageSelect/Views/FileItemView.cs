using System;
using LDY.Flow;
using LDY.Flow.Core;
using LDY.StageSelect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LDY.StageSelect {
    // 썸네일 + 파일명 + 상태 한 칸. 클릭은 선택, 더블클릭은 실행.
    public sealed class FileItemView : MonoBehaviour, IPointerClickHandler {
        private const float LineHeight = 12f;
        private static readonly PixelRect Frame = FileGridLayout.Thumbnail;
        private static readonly float NameTop = Frame.Bottom + 3f;
        private static readonly float StatusTop = NameTop + LineHeight + 1f;
        public static readonly Vector2 Size = new Vector2(FileGridLayout.ItemWidth, FileGridLayout.ItemHeight);
        private static readonly Color DimTint = new Color(0.55f, 0.55f, 0.55f, 1f);
        private static readonly Vector2 IconSize = new Vector2(16f, 16f);
        private static readonly Vector2 LockSize = new Vector2(16f, 20f);

        private UiSkin _skin;
        private Image _focus;
        private Image _thumbnail;
        private Image _fileIcon;
        private Image _badge;
        private Image _nameHighlight;
        private TextMeshProUGUI _name;
        private TextMeshProUGUI _status;
        private Action<string> _clicked;
        private Action<string> _doubleClicked;

        public string Id { get; private set; }

        public static FileItemView Create(UiFactory ui, Transform parent, Action<string> clicked,
            Action<string> doubleClicked) {
            var hit = ui.Solid("File", parent, Color.clear);
            hit.raycastTarget = true;
            var view = hit.gameObject.AddComponent<FileItemView>();
            view.Build(ui);
            view._clicked = clicked;
            view._doubleClicked = doubleClicked;
            return view;
        }

        private void Build(UiFactory ui) {
            _skin = ui.Skin;
            UiFactory.Place((RectTransform)transform, 0f, 0f, Size.x, Size.y);

            _focus = ui.NewImage("Focus", transform, _skin.focusDotted, _skin.text);
            _focus.type = Image.Type.Tiled;
            UiFactory.Stretch(_focus.rectTransform);

            var frame = ui.NewImage("Frame", transform, _skin.thumbFrame, _skin.face);
            UiFactory.Place(frame.rectTransform, Frame.X, Frame.Y, Frame.Width, Frame.Height);
            _thumbnail = ui.NewImage("Thumbnail", frame.transform, null, Color.black);
            UiFactory.Stretch(_thumbnail.rectTransform, 2f, 2f, 2f, 2f);
            _fileIcon = ui.NewImage("FileIcon", frame.transform, _skin.exe);
            UiFactory.Place(_fileIcon.rectTransform, 40f, 20f, IconSize.x, IconSize.y);
            _badge = ui.NewImage("Badge", frame.transform, null);
            UiFactory.Place(_badge.rectTransform, 2f, 42f, 12f, 12f);

            _nameHighlight = ui.Solid("NameHighlight", transform, _skin.selection);
            UiFactory.Place(_nameHighlight.rectTransform, 2f, NameTop, Size.x - 4f, LineHeight);
            _name = ui.NewText("Name", transform, "", null, TextAlignmentOptions.Center);
            UiFactory.Place(_name.rectTransform, 2f, NameTop, Size.x - 4f, LineHeight);
            _status = ui.NewText("Status", transform, "", null, TextAlignmentOptions.Center);
            UiFactory.Place(_status.rectTransform, 2f, StatusTop, Size.x - 4f, LineHeight);
        }

        public void Bind(FileItemData data, bool selected) {
            Id = data.Id;
            var tint = data.Dimmed ? DimTint : Color.white;
            var hasThumb = data.Thumbnail != null;
            _thumbnail.sprite = data.Thumbnail;
            _thumbnail.color = hasThumb ? tint : Color.Lerp(Color.black, _skin.desktop, 0.35f);

            _fileIcon.enabled = !hasThumb || data.Status == StageStatus.NotReady || data.Status == StageStatus.Locked;
            _fileIcon.sprite = Icon(data.Status);
            _fileIcon.color = data.Status == StageStatus.NotReady ? tint : Color.white;
            SetIconSize(data.Status == StageStatus.Locked ? LockSize : IconSize);

            _badge.enabled = data.Status == StageStatus.Cleared;
            _badge.sprite = _skin.checkBadge;

            _name.text = data.FileName;
            _status.text = data.StatusText;
            _focus.enabled = selected;
            _nameHighlight.enabled = selected;
            _name.color = selected ? _skin.selectionText : data.Dimmed ? _skin.disabled : _skin.text;
            _status.color = data.Dimmed ? _skin.disabled : _skin.text;
        }

        private Sprite Icon(StageStatus status) {
            switch (status) {
                case StageStatus.Locked: return _skin.lockBadge;
                case StageStatus.NotReady: return _skin.missing;
                default: return _skin.exe;
            }
        }

        private void SetIconSize(Vector2 size) {
            var rect = _fileIcon.rectTransform;
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(48f - size.x * 0.5f, -(28f - size.y * 0.5f));
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left || Id == null) return;
            if (eventData.clickCount >= 2) _doubleClicked?.Invoke(Id);
            else _clicked?.Invoke(Id);
        }
    }
}
