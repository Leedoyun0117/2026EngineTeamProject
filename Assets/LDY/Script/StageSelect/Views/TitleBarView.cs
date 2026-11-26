using System;
using LDY.Flow;
using UnityEngine;

namespace LDY.StageSelect {
    public sealed class TitleBarView : MonoBehaviour {
        public event Action CloseClicked;

        public void Build(UiFactory ui, string title) {
            var skin = ui.Skin;
            var bar = ui.NewImage("Bar", transform, skin.titlebar, skin.selection);
            UiFactory.Stretch(bar.rectTransform);
            UiFactory.Stretch(ui.NewText("Title", bar.transform, title, skin.titleText).rectTransform,
                WindowChrome.TitleLeft, 0f, WindowChrome.TitleRightInset(true), 0f);

            // 배치 규칙(간격·여백·세로 가운데)은 WindowChrome 한 곳에서 정한다.
            ui.TitleButton(transform, WindowChrome.TitleButton.Minimize);
            ui.TitleButton(transform, WindowChrome.TitleButton.Maximize);
            ui.TitleButton(transform, WindowChrome.TitleButton.Close).onClick.AddListener(() => CloseClicked?.Invoke());
        }
    }
}
