using System;
using System.Collections.Generic;
using LDY.Flow;
using LDY.StageSelect.Core;
using UnityEngine;

namespace LDY.StageSelect {
    // 칸 배치는 FileGridLayout(Core) 하나만 따른다.
    public sealed class FileGridView : MonoBehaviour {
        public const int Columns = FileGridLayout.Columns;

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
            var rect = FileGridLayout.Item(index);
            UiFactory.Place((RectTransform)view.transform, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
