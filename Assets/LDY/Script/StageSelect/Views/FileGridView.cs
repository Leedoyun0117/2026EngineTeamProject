using System;
using System.Collections.Generic;
using LDY.Flow;
using UnityEngine;

namespace LDY.StageSelect {
    public sealed class FileGridView : MonoBehaviour {
        public const int Columns = 4;
        private const float Padding = 8f;
        private const float SpacingX = 8f;
        private const float SpacingY = 6f;

        private readonly List<FileItemView> _items = new List<FileItemView>();
        private UiFactory _ui;

        public event Action<string> Selected;
        public event Action<string> Executed;

        public void Build(UiFactory ui) => _ui = ui;

        public void Show(IEnumerable<FileItemData> items, string selectedId) {
            var count = 0;
            foreach (var data in items) {
                if (count == _items.Count)
                    _items.Add(FileItemView.Create(_ui, transform, id => Selected?.Invoke(id), id => Executed?.Invoke(id)));
                var view = _items[count];
                view.gameObject.SetActive(true);
                Position(view, count);
                view.Bind(data, data.Id == selectedId);
                count++;
            }

            for (var i = count; i < _items.Count; i++) _items[i].gameObject.SetActive(false);
        }

        private static void Position(FileItemView view, int index) {
            var x = Padding + index % Columns * (FileItemView.Size.x + SpacingX);
            var y = Padding + index / Columns * (FileItemView.Size.y + SpacingY);
            UiFactory.Place((RectTransform)view.transform, x, y, FileItemView.Size.x, FileItemView.Size.y);
        }
    }
}
