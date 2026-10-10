using System;
using TMPro;
using UnityEngine;

namespace LDY.Script
{
    // 라벨 + 트랙/채움/핸들 + 오른쪽 % 표시. 값은 0~1이고 1% 단위로 맞춘다.
    public class SliderWidget : WindowWidget
    {
        private const float KeyboardSpeed = 0.5f;
        private const float HandleWidth = 32f * WindowKit.Pixel;
        private const float HandleHeight = 56f * WindowKit.Pixel;
        private const float TrackHeight = 24f * WindowKit.Pixel;
        private const float TrackBorder = 8f * WindowKit.Pixel;

        private readonly Action<float> _changed;
        private readonly WindowKit _kit;
        private readonly Rect _inner;
        private readonly SpriteRenderer _fill;
        private readonly Transform _handle;
        private readonly TMP_Text _percent;
        // 키보드로 조금씩 움직일 때 반올림에 묻히지 않도록 반올림 전 값을 따로 둔다.
        private float _exact;
        // 첫 SetValue에서 반드시 Refresh가 돌도록 불가능한 값으로 시작한다.
        private float _value = -1f;
        private bool _dragging;

        // trackX: 트랙의 가로 범위, centerY: 행 중심, platformTop/rowLeft/rowRight: 플레이어 발판
        public SliderWidget(WindowKit kit, Transform parent, string label, string name, float centerY,
            Vector2 trackX, float platformTop, float zoneHeight, Vector2 rowX, float initial, Action<float> changed)
        {
            _kit = kit;
            _changed = changed;

            Transform root = kit.Group(name, parent, Vector2.zero);
            TitleArtSet art = kit.Art;
            Color textColor = WindowStyle.Text;

            kit.Text("Label", root, new Vector2(WindowStyle.LabelX, centerY), label, WindowStyle.RowFont, textColor, 12,
                TextAlignmentOptions.Left, 3f);

            float trackWidth = trackX.y - trackX.x;
            kit.Sliced("Track", root, art.sliderTrack,
                new Rect(trackX.x, centerY - TrackHeight * 0.5f, trackWidth, TrackHeight), 12);

            _inner = new Rect(trackX.x + TrackBorder, centerY - (TrackHeight - TrackBorder * 2f) * 0.5f,
                trackWidth - TrackBorder * 2f, TrackHeight - TrackBorder * 2f);
            _fill = kit.Stretched("Fill", root, art.sliderFill, _inner, 13);
            _handle = kit.Stretched("Handle", root, art.sliderHandle,
                new Rect(0f, centerY - HandleHeight * 0.5f, HandleWidth, HandleHeight), 14).transform;
            _percent = kit.Text("Percent", root, new Vector2(WindowStyle.PercentX, centerY), "", WindowStyle.RowFont,
                textColor, 12, TextAlignmentOptions.Right, 1.6f);

            PointerRect = new Rect(trackX.x - HandleWidth * 0.5f, centerY - HandleHeight * 0.5f,
                trackWidth + HandleWidth, HandleHeight);
            FocusRect = new Rect(rowX.x, centerY - 0.5f, rowX.y - rowX.x, 1f);
            ProximityRect = new Rect(rowX.x, platformTop - WindowStyle.ZoneBelow, rowX.y - rowX.x, zoneHeight);

            SetValue(initial, false);
        }

        public float Value => _value;

        // 외부(설정창 열기)에서 값을 맞출 때는 콜백을 부르지 않는다.
        public void SetValue(float value, bool notify)
        {
            _exact = Mathf.Clamp01(value);
            float rounded = Mathf.Round(_exact * 100f) / 100f;
            bool changed = !Mathf.Approximately(rounded, _value);
            if (!changed)
                return;

            _value = rounded;
            Refresh();
            if (notify)
                _changed(_value);
        }

        public override bool PointerPress(Vector2 local)
        {
            _dragging = true;
            SetFromX(local.x);
            return true;
        }

        public override void PointerDrag(Vector2 local)
        {
            if (_dragging)
                SetFromX(local.x);
        }

        public override void PointerRelease()
        {
            _dragging = false;
        }

        public override void PointerCancel()
        {
            _dragging = false;
        }

        public override bool PlayerActivate() => true;

        public override bool PlayerTick(IWindowInput input, float dt)
        {
            if (input.JumpPressed || input.InteractPressed)
                return false;

            float axis = input.Horizontal;
            if (axis != 0f)
                SetValue(_exact + axis * KeyboardSpeed * dt, true);

            return true;
        }

        public override bool TryGetPlayerAnchor(out Vector2 local)
        {
            local = _handle.localPosition;
            return true;
        }

        private void SetFromX(float x)
        {
            SetValue((x - _inner.xMin) / _inner.width, true);
        }

        private void Refresh()
        {
            var fillRect = new Rect(_inner.xMin, _inner.yMin, Mathf.Max(_inner.width * _value, 0.001f), _inner.height);
            _kit.Resize(_fill, fillRect);
            _handle.localPosition = new Vector3(_inner.xMin + _inner.width * _value, _inner.center.y, 0f);
            _percent.text = $"{Mathf.RoundToInt(_value * 100f)}%";
        }
    }
}
