using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Flow {
    // Win95 복사 진행 창처럼 생긴 "실행 중..." 창. 씬을 넘어가도 남도록 흐름 서비스 아래에 붙는다.
    public sealed class LoadingWindow : MonoBehaviour {
        private const float Width = 232f;
        private const float Height = 74f;
        private const float BarWidth = 208f;
        private const float SegmentStep = 9f;

        private readonly List<Image> _segments = new List<Image>();
        private GameObject _root;
        private TextMeshProUGUI _caption;

        public void Build(UiSkin skin) {
            var ui = new UiFactory(skin);
            var canvas = UiFactory.CreateCanvas("LoadingCanvas", 1000, transform);
            _root = canvas.gameObject;

            var blocker = ui.Solid("Blocker", canvas.transform, new Color(0f, 0f, 0f, 0.25f));
            blocker.raycastTarget = true;
            UiFactory.Stretch(blocker.rectTransform);

            var window = ui.Panel("Window", canvas.transform).rectTransform;
            window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
            window.sizeDelta = new Vector2(Width, Height);

            // 로딩은 취소할 수 없어서 닫기 버튼은 모양만 둔다(대화상자와 같은 자리).
            var title = ui.WindowTitleBar(window, "실행 중...", out _);
            ui.TitleButtonImage(title.transform, WindowChrome.TitleButton.Close);

            _caption = ui.NewText("Caption", window, "");
            UiFactory.Place(_caption.rectTransform, 10f, 22f, Width - 20f, 16f);

            var bar = ui.Sunken("Bar", window).rectTransform;
            UiFactory.Place(bar, 10f, 44f, BarWidth + 4f, 16f);
            var count = Mathf.FloorToInt(BarWidth / SegmentStep);
            for (var i = 0; i < count; i++) {
                var segment = ui.NewImage("Seg" + i, bar, skin.progressSegment, skin.selection);
                UiFactory.Place(segment.rectTransform, 3f + i * SegmentStep, 3f, SegmentStep - 2f, 10f);
                _segments.Add(segment);
            }

            Hide();
        }

        public void Show(string caption) {
            _caption.text = caption;
            SetProgress(0f);
            _root.SetActive(true);
        }

        public void SetProgress(float value) {
            var lit = Mathf.RoundToInt(Mathf.Clamp01(value) * _segments.Count);
            for (var i = 0; i < _segments.Count; i++) _segments[i].enabled = i < lit;
        }

        public void Hide() => _root.SetActive(false);
    }
}
