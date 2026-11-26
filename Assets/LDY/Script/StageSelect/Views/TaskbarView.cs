using System;
using LDY.Flow;
using TMPro;
using UnityEngine;

namespace LDY.StageSelect {
    // 시작 버튼, 열린 창 버튼, 시계. 시계는 실제 시각이라 timeScale과 무관하다.
    public sealed class TaskbarView : MonoBehaviour {
        private TextMeshProUGUI _clock;

        // 마지막으로 표시한 하루 중 분(0~1439). 분이 바뀔 때만 문자열을 만들어 매 프레임 할당을 없앤다.
        private int _shownMinute = -1;

        public event Action StartClicked;

        public void Build(UiFactory ui, string taskTitle) {
            var skin = ui.Skin;
            var bar = ui.NewImage("Bar", transform, skin.taskbar, skin.face);
            UiFactory.Stretch(bar.rectTransform);

            var start = ui.NewButton("Start", transform, StageSelectText.Start, skin.startIcon);
            UiFactory.Place((RectTransform)start.transform, 2f, 4f, 50f, 16f);
            start.onClick.AddListener(() => StartClicked?.Invoke());

            var task = ui.NewImage("Task", transform, skin.buttonPressed, skin.face);
            UiFactory.Place(task.rectTransform, 56f, 4f, 148f, 16f);
            UiFactory.Place(ui.NewImage("Icon", task.transform, skin.folderOpen).rectTransform, 3f, 1f, 16f, 14f);
            UiFactory.Stretch(ui.NewText("Caption", task.transform, taskTitle).rectTransform, 22f, 0f, 2f, 0f);

            var tray = ui.Sunken("Tray", transform).rectTransform;
            tray.anchorMin = tray.anchorMax = tray.pivot = new Vector2(1f, 1f);
            tray.anchoredPosition = new Vector2(-2f, -4f);
            tray.sizeDelta = new Vector2(64f, 16f);
            _clock = ui.NewText("Clock", tray, "", null, TextAlignmentOptions.Center);
            UiFactory.Stretch(_clock.rectTransform);
            Tick();
        }

        private void Update() => Tick();

        private void Tick() {
            if (_clock == null) return;
            var now = DateTime.Now;
            var minute = now.Hour * 60 + now.Minute;
            if (minute == _shownMinute) return;
            _shownMinute = minute;
            _clock.text = Format(now);
        }

        private static string Format(DateTime now) {
            var hour = now.Hour % 12 == 0 ? 12 : now.Hour % 12;
            return $"{(now.Hour < 12 ? "오전" : "오후")} {hour}:{now.Minute:00}";
        }
    }
}
