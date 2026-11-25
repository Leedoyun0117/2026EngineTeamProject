using System;
using System.Collections.Generic;
using LDY.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.StageSelect {
    // 내 컴퓨터 > 로컬 디스크(C:) > GAME > STAGES > 월드들 + 설정.ini
    public sealed class TreeView : MonoBehaviour {
        private const float RowHeight = 16f;
        private const float LabelHeight = 12f;
        private const float LabelTop = (RowHeight - LabelHeight) / 2f;
        private static readonly Vector2 DefaultIconSize = new Vector2(16f, 16f);
        private const float Indent = 8f;
        private const float Top = 3f;
        private const int WorldDepth = 4;

        private readonly Dictionary<int, Row> _worlds = new Dictionary<int, Row>();
        private UiFactory _ui;
        private int _rowCount;

        public event Action<int> WorldClicked;
        public event Action SettingsClicked;

        public void Build(UiFactory ui, IEnumerable<WorldNodeData> worlds) {
            _ui = ui;
            var skin = ui.Skin;
            AddRow(StageSelectText.MyComputer, skin.computer, 0, null);
            AddRow(StageSelectText.LocalDisk, skin.disk, 1, null);
            AddRow(StageSelectText.Game, skin.folderOpen, 2, null);
            AddRow(StageSelectText.Stages, skin.folderOpen, 3, null);
            foreach (var world in worlds) {
                var number = world.World;
                _worlds[number] = AddRow(world.Label, skin.folder, WorldDepth, () => WorldClicked?.Invoke(number));
            }

            AddRow(StageSelectText.SettingsFile, skin.ini, WorldDepth, () => SettingsClicked?.Invoke());
        }

        public void Show(IEnumerable<WorldNodeData> worlds) {
            var skin = _ui.Skin;
            foreach (var world in worlds) {
                if (!_worlds.TryGetValue(world.World, out var row)) continue;
                row.Icon.sprite = world.Locked ? skin.folderLocked : world.Selected ? skin.folderOpen : skin.folder;
                row.Highlight.enabled = world.Selected;
                row.Label.color = world.Selected ? skin.selectionText : world.Locked ? skin.disabled : skin.text;
            }
        }

        private Row AddRow(string text, Sprite icon, int depth, Action onClick) {
            var y = Top + _rowCount++ * RowHeight;
            var x = 3f + depth * Indent;
            var hit = _ui.Solid("Node_" + text, transform, Color.clear);
            UiFactory.TopStrip(hit.rectTransform, y, RowHeight, 0f, 0f);

            var iconImage = _ui.NewImage("Icon", hit.transform, icon);
            PlaceIcon(iconImage, x);

            var highlight = _ui.Solid("Highlight", hit.transform, _ui.Skin.selection);
            var label = _ui.NewText("Label", hit.transform, text);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            var width = Mathf.Ceil(label.GetPreferredValues(text).x) + 4f;
            UiFactory.Place(highlight.rectTransform, x + 18f, LabelTop, width, LabelHeight);
            UiFactory.Place(label.rectTransform, x + 20f, LabelTop, width, LabelHeight);
            highlight.enabled = false;

            if (onClick != null) {
                hit.raycastTarget = true;
                var button = hit.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(() => onClick());
            }

            return new Row(iconImage, highlight, label);
        }

        // 아이콘은 스프라이트 원래 크기(폴더 16x14, 그 외 16x16) 그대로, 행 가운데에 정수 좌표로 둔다.
        private static void PlaceIcon(Image image, float x) {
            var size = image.sprite != null ? image.sprite.rect.size : DefaultIconSize;
            UiFactory.Place(image.rectTransform, x, Mathf.Floor((RowHeight - size.y) * 0.5f), size.x, size.y);
        }

        private sealed class Row {
            public Row(Image icon, Image highlight, TextMeshProUGUI label) {
                Icon = icon;
                Highlight = highlight;
                Label = label;
            }

            public Image Icon { get; }
            public Image Highlight { get; }
            public TextMeshProUGUI Label { get; }
        }
    }
}
