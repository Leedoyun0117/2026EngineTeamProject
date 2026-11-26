using System;
using System.Collections.Generic;
using LDY.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.StageSelect {
    // 설정.ini 창: 음량 3개 + 화면 모드. 값은 바로 적용·저장된다(타이틀 설정창과 같은 저장소).
    public sealed class SettingsDialogView : MonoBehaviour {
        private const float Width = 264f;
        private const float Height = 152f;
        private const float LabelWidth = 64f;
        private const float RowLeft = 12f;
        private const float RowTop = 26f;
        private const float RowStep = 22f;
        private const float SliderWidth = 140f;

        private readonly Dictionary<VolumeKind, (Slider slider, TextMeshProUGUI value)> _rows =
            new Dictionary<VolumeKind, (Slider, TextMeshProUGUI)>();

        private UiSkin _skin;
        private IStageSelectSettings _settings;
        private GameObject _root;
        private Image _fullScreen;
        private Image _windowed;

        public bool IsOpen => _root != null && _root.activeSelf;
        public event Action Closed;

        public void Build(UiFactory ui, IStageSelectSettings settings) {
            _skin = ui.Skin;
            _settings = settings;
            var blocker = ui.Solid("Blocker", transform, new Color(0f, 0f, 0f, 0.15f));
            blocker.raycastTarget = true;
            UiFactory.Stretch(blocker.rectTransform);
            _root = blocker.gameObject;

            var window = ui.Panel("Window", blocker.transform).rectTransform;
            window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
            window.sizeDelta = new Vector2(Width, Height);
            BuildTitle(ui, window);

            AddVolume(ui, window, VolumeKind.Master, StageSelectText.MasterVolume, 0);
            AddVolume(ui, window, VolumeKind.Bgm, StageSelectText.BgmVolume, 1);
            AddVolume(ui, window, VolumeKind.Sfx, StageSelectText.SfxVolume, 2);
            AddScreenMode(ui, window, RowTop + RowStep * 3);

            var ok = ui.NewButton("Ok", window, StageSelectText.Ok);
            var okRect = (RectTransform)ok.transform;
            okRect.anchorMin = okRect.anchorMax = okRect.pivot = new Vector2(0.5f, 0f);
            okRect.anchoredPosition = new Vector2(0f, 8f);
            okRect.sizeDelta = new Vector2(64f, 18f);
            ok.onClick.AddListener(Close);

            _root.SetActive(false);
        }

        public void Open() {
            foreach (var pair in _rows) pair.Value.slider.SetValueWithoutNotify(_settings.GetVolume(pair.Key));
            foreach (var pair in _rows) ShowValue(pair.Key);
            ShowScreenMode();
            _root.SetActive(true);
        }

        public void Close() {
            if (!IsOpen) return;
            _root.SetActive(false);
            Closed?.Invoke();
        }

        private void BuildTitle(UiFactory ui, RectTransform window) {
            var bar = ui.WindowTitleBar(window, StageSelectText.SettingsTitle, out _);
            ui.TitleButton(bar.transform, WindowChrome.TitleButton.Close).onClick.AddListener(Close);
        }

        private void AddVolume(UiFactory ui, RectTransform window, VolumeKind kind, string label, int row) {
            var y = RowTop + row * RowStep;
            UiFactory.Place(ui.NewText("Label_" + kind, window, label).rectTransform, RowLeft, y, LabelWidth, 14f);

            var track = ui.Sunken("Slider_" + kind, window);
            track.raycastTarget = true;
            UiFactory.Place(track.rectTransform, RowLeft + LabelWidth, y + 2f, SliderWidth, 10f);
            var slider = track.gameObject.AddComponent<Slider>();
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.transition = Selectable.Transition.None;

            var fillArea = UiFactory.Stretch(UiFactory.Rect("FillArea", track.transform), 2f, 2f, 2f, 2f);
            var fill = ui.Solid("Fill", fillArea, _skin.selection);
            UiFactory.Stretch(fill.rectTransform);
            slider.fillRect = fill.rectTransform;

            var handleArea = UiFactory.Stretch(UiFactory.Rect("HandleArea", track.transform), 3f, 0f, 3f, 0f);
            var handle = ui.NewImage("Handle", handleArea, _skin.button, _skin.face);
            handle.raycastTarget = true;
            handle.rectTransform.sizeDelta = new Vector2(6f, 4f);
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;

            var value = ui.NewText("Value_" + kind, window, "", null, TextAlignmentOptions.Right);
            UiFactory.Place(value.rectTransform, RowLeft + LabelWidth + SliderWidth + 4f, y, 32f, 14f);
            _rows[kind] = (slider, value);
            slider.onValueChanged.AddListener(v => {
                _settings.SetVolume(kind, v);
                ShowValue(kind);
            });
        }

        private void AddScreenMode(UiFactory ui, RectTransform window, float y) {
            UiFactory.Place(ui.NewText("Label_Screen", window, StageSelectText.ScreenMode).rectTransform, RowLeft, y + 2f,
                LabelWidth, 14f);
            _fullScreen = ModeButton(ui, window, StageSelectText.FullScreen, RowLeft + LabelWidth, y, true);
            _windowed = ModeButton(ui, window, StageSelectText.Windowed, RowLeft + LabelWidth + 72f, y, false);
        }

        private Image ModeButton(UiFactory ui, RectTransform window, string caption, float x, float y, bool fullScreen) {
            var button = ui.NewButton("Mode_" + caption, window, caption);
            UiFactory.Place((RectTransform)button.transform, x, y, 68f, 18f);
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => {
                _settings.SetFullScreen(fullScreen);
                ShowScreenMode(fullScreen);
            });
            return button.GetComponent<Image>();
        }

        private void ShowValue(VolumeKind kind) {
            var (slider, value) = _rows[kind];
            value.text = Mathf.RoundToInt(slider.value * 100f) + "%";
        }

        private void ShowScreenMode() => ShowScreenMode(_settings.IsFullScreen);

        // 화면 모드 변경은 다음 프레임에 반영돼서 누른 값으로 바로 표시한다.
        private void ShowScreenMode(bool fullScreen) {
            _fullScreen.sprite = fullScreen ? _skin.buttonPressed : _skin.button;
            _windowed.sprite = fullScreen ? _skin.button : _skin.buttonPressed;
        }
    }
}
