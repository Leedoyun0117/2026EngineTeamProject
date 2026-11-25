using System;
using System.Collections.Generic;
using LDY.Flow;
using LDY.Flow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.StageSelect {
    public sealed class FilterTabsView : MonoBehaviour {
        private const float TabWidth = 56f;
        private const float TabHeight = 13f;
        private const float Top = 2f;

        // 탭 줄 전체 높이. 씬 빌더가 탭 영역 크기를 이 값으로 잡는다(선택 탭은 Top만큼 위로 솟는다).
        public const float Height = Top + TabHeight;

        private readonly Dictionary<StageFilterMode, Image> _tabs = new Dictionary<StageFilterMode, Image>();
        private UiSkin _skin;

        public event Action<StageFilterMode> Changed;

        public void Build(UiFactory ui) {
            _skin = ui.Skin;
            AddTab(ui, StageFilterMode.All, StageSelectText.All, 0);
            AddTab(ui, StageFilterMode.Incomplete, StageSelectText.IncompleteTab, 1);
            AddTab(ui, StageFilterMode.Complete, StageSelectText.CompleteTab, 2);
        }

        public void Show(StageFilterMode active) {
            foreach (var pair in _tabs) {
                var on = pair.Key == active;
                pair.Value.sprite = on ? _skin.tabActive : _skin.tab;
                var rect = pair.Value.rectTransform;
                rect.sizeDelta = new Vector2(TabWidth, on ? TabHeight + Top : TabHeight);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, on ? 0f : -Top);
            }
        }

        private void AddTab(UiFactory ui, StageFilterMode mode, string caption, int index) {
            var tab = ui.NewImage("Tab_" + mode, transform, _skin.tab);
            UiFactory.Place(tab.rectTransform, 2f + index * (TabWidth - 2f), Top, TabWidth, TabHeight);
            tab.raycastTarget = true;
            UiFactory.Stretch(ui.NewText("Caption", tab.transform, caption, null, TMPro.TextAlignmentOptions.Center)
                .rectTransform);
            var button = tab.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => Changed?.Invoke(mode));
            _tabs[mode] = tab;
        }
    }
}
