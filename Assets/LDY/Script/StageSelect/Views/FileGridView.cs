using System;
using System.Collections.Generic;
using LDY.Flow;
using LDY.StageSelect.Core;
using UnityEngine;

namespace LDY.StageSelect {
    // 칸 배치는 FileGridLayout(Core) 하나만 따른다. 발판 계산도 같은 값을 쓴다.
    public sealed class FileGridView : MonoBehaviour {
        public const int Columns = FileGridLayout.Columns;

        private readonly List<FileItemView> _items = new List<FileItemView>();
        private readonly List<string> _ids = new List<string>();
        private UiFactory _ui;

        public event Action<string> Selected;
        public event Action<string> Executed;

        // 보이는 파일 목록(순서 포함)이 바뀌었을 때만. 선택만 바뀐 경우는 부르지 않는다.
        public event Action LayoutChanged;

        public IReadOnlyList<string> VisibleIds => _ids;
        public RectTransform Area => (RectTransform)transform;

        public void Build(UiFactory ui) => _ui = ui;

        public void Show(IEnumerable<FileItemData> items, string selectedId) {
            var count = 0;
            var changed = false;
            foreach (var data in items) {
                if (count == _items.Count)
                    _items.Add(FileItemView.Create(_ui, transform, id => Selected?.Invoke(id), id => Executed?.Invoke(id)));
                var view = _items[count];
                view.gameObject.SetActive(true);
                Position(view, count);
                view.Bind(data, data.Id == selectedId);
                changed |= SetId(count, data.Id);
                count++;
            }

            for (var i = count; i < _items.Count; i++) _items[i].gameObject.SetActive(false);
            if (_ids.Count > count) {
                _ids.RemoveRange(count, _ids.Count - count);
                changed = true;
            }

            if (changed) LayoutChanged?.Invoke();
        }

        private bool SetId(int index, string id) {
            if (index == _ids.Count) {
                _ids.Add(id);
                return true;
            }

            if (_ids[index] == id) return false;
            _ids[index] = id;
            return true;
        }

        private static void Position(FileItemView view, int index) {
            var rect = FileGridLayout.Item(index);
            UiFactory.Place((RectTransform)view.transform, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
