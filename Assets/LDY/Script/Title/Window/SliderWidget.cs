using System;
using TMPro;
using UnityEngine;

namespace LDY.Script {
    public class SliderWidget : WindowWidget {
        private const float KeyboardSpeed = 0.5f;
        private const float HandleWidth = 32f * WindowKit.Pixel;
        private const float HandleHeight = 56f * WindowKit.Pixel;
        private const float TrackHeight = 24f * WindowKit.Pixel;
        private const float TrackBorder = 8f * WindowKit.Pixel;

        private readonly Action<float> _changed;
        private readonly SpriteRenderer _fill;
        private readonly Transform _handle;
        private readonly Rect _inner;
        private readonly WindowKit _kit;
        private readonly TMP_Text _percent;

        private bool _dragging;

        // 키보드로 조금씩 움직일 때 반올림에 묻히지 않게 반올림 전 값을 따로 둠
        private float _exact;

        // 첫 SetValue에서 Refresh가 돌도록 불가능한 값으로 시작

        public SliderWidget(WindowKit kit, Transform parent, string label, string name, float centerY,
            Vector2 trackX, float platformTop, float zoneHeight, Vector2 rowX, float initial, Action<float> changed) {
            _kit = kit;
            _changed = changed;

            var root = kit.Group(name, parent, Vector2.zero);
            var art = kit.Art;
            var textColor = WindowStyle.Text;

            kit.Text("Label", root, new Vector2(WindowStyle.LabelX, centerY), label, WindowStyle.RowFont, textColor,
                WindowStyle.Order.Content, TextAlignmentOptions.Left, WindowStyle.LabelWidth);

            var trackWidth = trackX.y - trackX.x;
            kit.Sliced("Track", root, art.sliderTrack,
                new Rect(trackX.x, centerY - TrackHeight * 0.5f, trackWidth, TrackHeight), WindowStyle.Order.Content);

            _inner = new Rect(trackX.x + TrackBorder, centerY - (TrackHeight - TrackBorder * 2f) * 0.5f,
                trackWidth - TrackBorder * 2f, TrackHeight - TrackBorder * 2f);
            _fill = kit.Stretched("Fill", root, art.sliderFill, _inner, WindowStyle.Order.ContentRaised);
            _handle = kit.Stretched("Handle", root, art.sliderHandle,
                    new Rect(0f, centerY - HandleHeight * 0.5f, HandleWidth, HandleHeight),
                    WindowStyle.Order.SliderHandle)
                .transform;
            _percent = kit.Text("Percent", root, new Vector2(WindowStyle.PercentX, centerY), "", WindowStyle.RowFont,
                textColor, WindowStyle.Order.Content, TextAlignmentOptions.Right, WindowStyle.PercentWidth);

            PointerRect = new Rect(trackX.x - HandleWidth * 0.5f, centerY - HandleHeight * 0.5f,
                trackWidth + HandleWidth, HandleHeight);
            FocusRect = new Rect(rowX.x, centerY - WindowStyle.RowFocusHeight * 0.5f, rowX.y - rowX.x,
                WindowStyle.RowFocusHeight);
            ProximityRect = new Rect(rowX.x, platformTop - WindowStyle.ZoneBelow, rowX.y - rowX.x, zoneHeight);

            SetValue(initial, false);
        }

        public float Value { get; private set; } = -1f;

        // 외부에서 맞출 땐 콜백 안 부름
        public void SetValue(float value, bool notify) {
            _exact = Mathf.Clamp01(value);
            var rounded = Mathf.Round(_exact * WindowStyle.PercentScale) / WindowStyle.PercentScale;
            var changed = !Mathf.Approximately(rounded, Value);
            if (!changed)
                return;

            Value = rounded;
            Refresh();
            if (notify)
                _changed(Value);
        }

        public override bool PointerPress(Vector2 local) {
            _dragging = true;
            SetFromX(local.x);
            return true;
        }

        public override void PointerDrag(Vector2 local) {
            if (_dragging)
                SetFromX(local.x);
        }

        public override void PointerRelease() {
            _dragging = false;
        }

        public override void PointerCancel() {
            _dragging = false;
        }

        public override bool PlayerActivate() {
            return true;
        }

        public override bool PlayerTick(IWindowInput input, float dt) {
            if (input.JumpPressed || input.InteractPressed)
                return false;

            var axis = input.Horizontal;
            if (axis != 0f)
                SetValue(_exact + axis * KeyboardSpeed * dt, true);

            return true;
        }

        public override bool TryGetPlayerAnchor(out Vector2 local) {
            local = _handle.localPosition;
            return true;
        }

        private void SetFromX(float x) {
            SetValue((x - _inner.xMin) / _inner.width, true);
        }

        private void Refresh() {
            var fillRect = new Rect(_inner.xMin, _inner.yMin, Mathf.Max(_inner.width * Value, WindowStyle.MinFillWidth),
                _inner.height);
            _kit.Resize(_fill, fillRect);
            _handle.localPosition = new Vector3(_inner.xMin + _inner.width * Value, _inner.center.y, 0f);
            _percent.text = $"{Mathf.RoundToInt(Value * WindowStyle.PercentScale)}%";
        }
    }
}