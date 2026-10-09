using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 창 하나의 입력 처리. Alt 단계면 커서(마우스), Normal 단계면 플레이어(발판 + E)로 위젯을 조작한다.
    // 조작 상태(잡은 위젯, 드래그 중인 위젯)는 모두 여기서만 갖는다.
    public sealed class WindowInteraction
    {
        private readonly IReadOnlyList<WindowWidget> _widgets;
        private readonly WindowServices _services;
        private readonly Transform _window;
        private readonly FocusFrame _focus;
        private readonly PlatformDropper _dropper;
        private readonly IButtonInput _dropInput;

        private AltPhase _phase;
        private WindowWidget _captured;
        private WindowWidget _dragging;

        public WindowInteraction(IReadOnlyList<WindowWidget> widgets, WindowServices services, Transform window,
            FocusFrame focus, PlatformDropper dropper, IButtonInput dropInput)
        {
            _dropper = dropper;
            _dropInput = dropInput;
            _widgets = widgets;
            _services = services;
            _window = window;
            _focus = focus;
        }

        public void Tick(float dt)
        {
            AltPhase phase = _services.Alt.Phase;
            if (phase != _phase)
            {
                HandlePhaseChanged(phase);
                _phase = phase;
            }

            switch (phase)
            {
                case AltPhase.Alt:
                    TickPointer();
                    break;
                case AltPhase.Normal:
                    TickPlayer(dt);
                    break;
                default:
                    _focus.Hide();
                    break;
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            _dropper.FixedTick(fixedDeltaTime);
        }

        // ESC를 소비했으면 true. 잡고 있는 조작이나 열린 드롭다운이 있으면 그것만 취소하고 창은 닫지 않는다.
        public bool HandleCancel()
        {
            if (_captured != null)
            {
                WindowWidget widget = _captured;
                EndCapture();
                widget.PlayerCancel();
                return true;
            }

            foreach (WindowWidget widget in _widgets)
            {
                if (widget.PointerEngaged)
                {
                    widget.PointerCancel();
                    return true;
                }
            }

            return false;
        }

        // 창이 열리거나 닫힐 때 모든 조작 상태를 정리한다.
        public void Reset()
        {
            if (_captured != null)
            {
                WindowWidget widget = _captured;
                EndCapture();
                widget.PlayerCancel();
            }

            ReleaseDrag();
            foreach (WindowWidget widget in _widgets)
                widget.PointerCancel();

            _dropper.Clear();
            _focus.Hide();
            _phase = _services.Alt.Phase;
        }

        private void HandlePhaseChanged(AltPhase phase)
        {
            // Alt가 시작되면 키보드 조작은 풀린다. 플레이어 정지/복귀는 Alt 상태가 맡으므로 건드리지 않는다.
            if (_captured != null && phase != AltPhase.Normal)
            {
                WindowWidget widget = _captured;
                _captured = null;
                widget.PlayerCancel();
            }

            if (phase != AltPhase.Normal)
                _dropper.ReleaseAll();

            if (phase != AltPhase.Alt)
            {
                ReleaseDrag();
                foreach (WindowWidget widget in _widgets)
                {
                    if (widget.PointerEngaged)
                        widget.PointerCancel();
                }
            }
        }

        private void TickPointer()
        {
            IWindowInput input = _services.Input;
            bool inside = _services.Monitor.PointerInside;
            Vector2 local = ToLocal(_services.Alt.CursorWorldPosition);

            WindowWidget engaged = FindEngaged();
            WindowWidget hover = engaged ?? (inside ? FindPointed(local) : null);
            hover?.PointerMove(local);
            ShowFocus(_dragging ?? hover);

            if (_dragging != null)
            {
                if (input.PointerHeld)
                    _dragging.PointerDrag(local);
                else
                    ReleaseDrag();
                return;
            }

            if (!input.PointerPressed || !inside)
                return;

            // 열린 드롭다운이 있으면 어디를 눌러도 그 위젯이 먼저 받는다.
            if (hover != null && hover.PointerPress(local))
                _dragging = hover;
        }

        private void TickPlayer(float dt)
        {
            IWindowInput input = _services.Input;

            if (_captured != null)
            {
                ShowFocus(_captured);
                if (!_captured.PlayerTick(input, dt))
                {
                    EndCapture();
                    return;
                }

                FollowAnchor();
                return;
            }

            // 슬라이더를 잡거나 드롭다운이 열린 동안(위 분기)에는 S가 UI 조작이라 내려가기가 동작하지 않는다.
            if (_dropInput.Pressed)
                _dropper.TryDrop();

            WindowWidget near = FindNear();
            ShowFocus(near);
            if (near != null && input.InteractPressed && near.PlayerActivate())
                BeginCapture(near);
        }

        private void BeginCapture(WindowWidget widget)
        {
            _captured = widget;
            _services.PlayerControl.SetSuspended(true);
            Vector2 velocity = _services.PlayerBody.Velocity;
            _services.PlayerBody.Velocity = new Vector2(0f, velocity.y);
            FollowAnchor();
        }

        // Normal 단계에서만 조작을 되돌려 준다. Alt/Return 중에는 그쪽 상태가 정지와 복귀를 맡는다.
        private void EndCapture()
        {
            _captured = null;
            if (_services.Alt.Phase == AltPhase.Normal)
                _services.PlayerControl.SetSuspended(false);
        }

        // 잡는 동안 플레이어(커서)가 슬라이더 핸들에 붙어 따라간다. 높이는 물리가 그대로 정한다.
        private void FollowAnchor()
        {
            if (_captured == null || !_captured.TryGetPlayerAnchor(out Vector2 anchor))
                return;

            Vector2 world = _window.TransformPoint(anchor);
            Vector2 position = _services.PlayerBody.Position;
            _services.PlayerBody.Position = new Vector2(world.x, position.y);
            _services.PlayerBody.Velocity = new Vector2(0f, _services.PlayerBody.Velocity.y);
        }

        private void ReleaseDrag()
        {
            if (_dragging == null)
                return;

            _dragging.PointerRelease();
            _dragging = null;
        }

        private WindowWidget FindEngaged()
        {
            foreach (WindowWidget widget in _widgets)
            {
                if (widget.PointerEngaged)
                    return widget;
            }

            return null;
        }

        private WindowWidget FindPointed(Vector2 local)
        {
            foreach (WindowWidget widget in _widgets)
            {
                if (widget.PointerRect.Contains(local))
                    return widget;
            }

            return null;
        }

        // 플레이어 발바닥 점이 발판 구역 안에 있는 위젯.
        private WindowWidget FindNear()
        {
            Bounds bounds = _services.PlayerCollider.bounds;
            Vector2 feet = _window.InverseTransformPoint(new Vector3(bounds.center.x, bounds.min.y, 0f));
            WindowWidget best = null;
            float bestDistance = float.MaxValue;
            foreach (WindowWidget widget in _widgets)
            {
                if (!widget.ProximityRect.Contains(feet))
                    continue;

                float distance = Mathf.Abs(feet.y - widget.ProximityRect.center.y);
                if (distance < bestDistance)
                {
                    best = widget;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private void ShowFocus(WindowWidget widget)
        {
            if (widget == null)
                _focus.Hide();
            else
                _focus.Show(widget.FocusRect);
        }

        private Vector2 ToLocal(Vector2 world)
        {
            return _window.InverseTransformPoint(new Vector3(world.x, world.y, _window.position.z));
        }
    }

    // 하이라이트용 focus_frame. 위젯 범위에 맞춰 크기와 위치를 바꾼다.
    public sealed class FocusFrame
    {
        private readonly WindowKit _kit;
        private readonly SpriteRenderer _renderer;

        public FocusFrame(WindowKit kit, Transform parent)
        {
            _kit = kit;
            _renderer = kit.Sliced("FocusFrame", parent, kit.Art.focusFrame, new Rect(0f, 0f, 1f, 1f), 15);
            _renderer.gameObject.SetActive(false);
        }

        public void Show(Rect rect)
        {
            _renderer.gameObject.SetActive(true);
            _kit.Resize(_renderer, rect);
        }

        public void Hide()
        {
            _renderer.gameObject.SetActive(false);
        }
    }
}
