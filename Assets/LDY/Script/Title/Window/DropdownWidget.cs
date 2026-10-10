using System;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 라벨 + 선택 상자 + 아래로 펼쳐지는 목록. 마우스는 클릭으로 열고 고르며, 플레이어는 E로 열고 W/S로 고르고 E로 확정한다.
    public class DropdownWidget : WindowWidget
    {
        private const float BoxHeight = 56f * WindowKit.Pixel;
        private const float Border = 8f * WindowKit.Pixel;
        private const float ItemHeight = 0.55f;

        private readonly string[] _options;
        private readonly Action<int> _changed;
        private readonly Rect _boxRect;
        private readonly Rect _listRect;
        private readonly GameObject _list;
        private readonly SpriteRenderer _highlight;
        private readonly TMP_Text _selectedText;
        private int _selected;
        private int _highlighted;
        private bool _open;
        private bool _byPointer;

        public DropdownWidget(WindowKit kit, Transform parent, string label, float centerY, Vector2 boxX,
            float platformTop, float zoneHeight, Vector2 rowX, string[] options, int initial, Action<int> changed)
        {
            _options = options;
            _changed = changed;

            Transform root = kit.Group("ScreenMode", parent, Vector2.zero);
            TitleArtSet art = kit.Art;

            kit.Text("Label", root, new Vector2(WindowStyle.LabelX, centerY), label, WindowStyle.RowFont,
                WindowStyle.Text, 12, TextAlignmentOptions.Left, 3f);

            _boxRect = new Rect(boxX.x, centerY - BoxHeight * 0.5f, boxX.y - boxX.x, BoxHeight);
            kit.Sliced("Box", root, art.dropdownBox, _boxRect, 12);

            float arrowSize = BoxHeight - Border * 2f;
            kit.Stretched("Arrow", root, art.dropdownArrowButton,
                new Rect(_boxRect.xMax - Border - arrowSize, _boxRect.center.y - arrowSize * 0.5f, arrowSize, arrowSize), 13);
            _selectedText = kit.Text("Selected", root, new Vector2(_boxRect.xMin + 0.25f, centerY), "",
                WindowStyle.RowFont, WindowStyle.Text, 13, TextAlignmentOptions.Left, _boxRect.width - 1f);

            float listHeight = _options.Length * ItemHeight + Border * 2f;
            _listRect = new Rect(_boxRect.xMin, _boxRect.yMin - 0.02f - listHeight, _boxRect.width, listHeight);

            _list = kit.Group("List", root, Vector2.zero).gameObject;
            Transform list = _list.transform;
            kit.Sliced("ListBg", list, art.dropdownListBg, _listRect, 16);
            _highlight = kit.Stretched("Highlight", list, art.dropdownHighlight, ItemRect(0, true), 17);
            for (int i = 0; i < _options.Length; i++)
            {
                kit.Text($"Item{i}", list, new Vector2(_boxRect.xMin + 0.25f, ItemRect(i, false).center.y), _options[i],
                    WindowStyle.RowFont, WindowStyle.Text, 18, TextAlignmentOptions.Left, _boxRect.width - 1f);
            }

            _kit = kit;
            _list.SetActive(false);

            PointerRect = _boxRect;
            FocusRect = new Rect(_boxRect.xMin - 0.1f, _boxRect.yMin - 0.1f, _boxRect.width + 0.2f, _boxRect.height + 0.2f);
            ProximityRect = new Rect(rowX.x, platformTop - WindowStyle.ZoneBelow, rowX.y - rowX.x, zoneHeight);

            SetIndex(initial);
        }

        private readonly WindowKit _kit;

        public override bool PointerEngaged => _open && _byPointer;

        // 외부(설정창 열기)에서 선택을 맞출 때는 콜백을 부르지 않는다.
        public void SetIndex(int index)
        {
            _selected = Mathf.Clamp(index, 0, _options.Length - 1);
            _selectedText.text = _options[_selected];
        }

        public override bool PointerPress(Vector2 local)
        {
            if (!_open)
            {
                if (_boxRect.Contains(local))
                    Open(true);
                return false;
            }

            int item = ItemAt(local);
            if (item >= 0)
                Select(item);
            Close();
            return false;
        }

        public override void PointerMove(Vector2 local)
        {
            if (!_open)
                return;

            int item = ItemAt(local);
            if (item >= 0)
                SetHighlight(item);
        }

        public override void PointerCancel()
        {
            Close();
        }

        public override bool PlayerActivate()
        {
            Open(false);
            return true;
        }

        public override bool PlayerTick(IWindowInput input, float dt)
        {
            if (input.InteractPressed)
            {
                Select(_highlighted);
                Close();
                return false;
            }

            // W가 위, S가 아래
            int step = -input.VerticalStep;
            if (step != 0)
                SetHighlight(Mathf.Clamp(_highlighted + step, 0, _options.Length - 1));

            return true;
        }

        public override void PlayerCancel()
        {
            Close();
        }

        private void Open(bool byPointer)
        {
            _open = true;
            _byPointer = byPointer;
            _list.SetActive(true);
            SetHighlight(_selected);
            PointerRect = Union(_boxRect, _listRect);
        }

        private void Close()
        {
            _open = false;
            _byPointer = false;
            _list.SetActive(false);
            PointerRect = _boxRect;
        }

        private void Select(int index)
        {
            if (index < 0 || index >= _options.Length || index == _selected)
                return;

            SetIndex(index);
            _changed(index);
        }

        private void SetHighlight(int index)
        {
            if (_highlighted == index)
                return;

            _highlighted = index;
            _kit.Resize(_highlight, ItemRect(index, true));
        }

        private int ItemAt(Vector2 local)
        {
            for (int i = 0; i < _options.Length; i++)
            {
                if (ItemRect(i, true).Contains(local))
                    return i;
            }

            return -1;
        }

        // inset이 true면 목록 배경 테두리 안쪽 폭(하이라이트/판정용), false면 글자 위치 계산용.
        private Rect ItemRect(int index, bool inset)
        {
            float x = inset ? _listRect.xMin + Border : _listRect.xMin;
            float width = inset ? _listRect.width - Border * 2f : _listRect.width;
            float top = _listRect.yMax - Border - index * ItemHeight;
            return new Rect(x, top - ItemHeight, width, ItemHeight);
        }

        private static Rect Union(Rect a, Rect b)
        {
            return Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin),
                Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));
        }
    }
}
