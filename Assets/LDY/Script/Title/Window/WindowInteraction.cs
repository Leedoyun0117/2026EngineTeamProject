using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script {
    // Alt면 커서(마우스), Normal이면 플레이어(발판 + E)로 위젯을 조작
    public sealed class WindowInteraction {
        private readonly IButtonInput _dropInput;
        private readonly PlatformDropper _dropper;
        private readonly WindowFloorGuard _floor;

        private readonly FocusFrame _focus;

        // 행 이동 창이면 _rows, 크레딧처럼 아니면 _dropper. 하나는 null
        private readonly PlayerRowMover _rows;
        private readonly WindowServices _services;
        private readonly IReadOnlyList<WindowWidget> _widgets;
        private readonly Transform _window;
        private WindowWidget _captured;
        private WindowWidget _dragging;

        private AltPhase _phase;

        public WindowInteraction(IReadOnlyList<WindowWidget> widgets, WindowServices services, Transform window,
            FocusFrame focus, PlayerRowMover rows, PlatformDropper dropper, IButtonInput dropInput,
            WindowFloorGuard floor) {
            _floor = floor;
            _rows = rows;
            _dropper = dropper;
            _dropInput = dropInput;
            _widgets = widgets;
            _services = services;
            _window = window;
            _focus = focus;
        }

        public void Tick(float dt) {
            var phase = _services.Alt.Phase;
            if (phase != _phase) {
                HandlePhaseChanged(phase);
                _phase = phase;
            }

            _rows?.Tick(dt);

            switch (phase) {
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

            PublishPose();
        }

        private void PublishPose() {
            _services.Pose?.Report(_captured is SliderWidget, _rows?.Direction ?? RowMoveDirection.None);
        }

        public void FixedTick(float fixedDeltaTime) {
            _dropper?.FixedTick(fixedDeltaTime);

            // Alt 중엔 플레이어가 창 밖에 있을 수 있고, 행 이동 중엔 위치를 직접 옮겨서 검사 안 함
            if (_services.Alt.Phase == AltPhase.Normal && (_rows == null || !_rows.IsMoving))
                _floor.FixedTick();
        }

        // 잡은 조작이나 열린 드롭다운이 있으면 그것만 취소하고 창은 안 닫음(소비하면 true)
        public bool HandleCancel() {
            if (_captured != null) {
                var widget = _captured;
                EndCapture();
                widget.PlayerCancel();
                return true;
            }

            for (var i = 0; i < _widgets.Count; i++)
                if (_widgets[i].PointerEngaged) {
                    _widgets[i].PointerCancel();
                    return true;
                }

            return false;
        }

        public void Reset() {
            if (_captured != null) {
                var widget = _captured;
                EndCapture();
                widget.PlayerCancel();
            }

            ReleaseDrag();
            for (var i = 0; i < _widgets.Count; i++)
                _widgets[i].PointerCancel();

            _dropper?.Clear();
            _focus.Hide();
            _phase = _services.Alt.Phase;
            _rows?.Cancel();
            _floor.Deactivate();
            PublishPose();
        }

        public void Opened() {
            _rows?.Open();
            _floor.Activate();
            _dropper?.SetUndroppable(_floor.Lowest);
        }

        // 꺼지거나 씬이 바뀔 때 호출. 오브젝트는 안 건드리고 상태만 복구
        public void Abort() {
            _rows?.Cancel();
            _floor.Deactivate();
            _services.Pose?.Report(false, RowMoveDirection.None);
        }

        private void HandlePhaseChanged(AltPhase phase) {
            // 플레이어 정지/복귀는 Alt 상태가 맡아서 여기선 SetSuspended 안 함
            if (_captured != null && phase != AltPhase.Normal) {
                var widget = _captured;
                _captured = null;
                widget.PlayerCancel();
            }

            if (phase != AltPhase.Normal) {
                _rows?.Cancel();
                _dropper?.ReleaseAll();
            }

            if (phase != AltPhase.Alt) {
                ReleaseDrag();
                for (var i = 0; i < _widgets.Count; i++)
                    if (_widgets[i].PointerEngaged)
                        _widgets[i].PointerCancel();
            }
        }

        private void TickPointer() {
            var input = _services.Input;
            var inside = _services.Monitor.PointerInside;
            var local = ToLocal(_services.Alt.CursorWorldPosition);

            var engaged = FindEngaged();
            var hover = engaged ?? (inside ? FindPointed(local) : null);
            hover?.PointerMove(local);
            ShowFocus(_dragging ?? hover);

            if (_dragging != null) {
                if (input.PointerHeld)
                    _dragging.PointerDrag(local);
                else
                    ReleaseDrag();
                return;
            }

            if (!input.PointerPressed || !inside)
                return;

            // 열린 드롭다운은 어디를 눌러도 먼저 받음
            if (hover != null && hover.PointerPress(local))
                _dragging = hover;
        }

        private void TickPlayer(float dt) {
            var input = _services.Input;

            if (_captured != null) {
                ShowFocus(_captured);
                if (!_captured.PlayerTick(input, dt)) {
                    EndCapture();
                    return;
                }

                FollowAnchor();
                return;
            }

            // 잡는 동안(위 분기)엔 Jump/S가 UI 조작이라 행 이동 안 됨
            if (_rows != null) {
                if (_rows.IsMoving) {
                    _focus.Hide();
                    return;
                }

                var down = DropPressed();
                if (input.JumpPressed)
                    _rows.TryStep(1);
                else if (down)
                    _rows.TryStep(-1);
            }
            else if (DropPressed()) {
                _dropper.TryDrop();
            }

            var near = FindNear();
            ShowFocus(near);
            if (near != null && input.InteractPressed && near.PlayerActivate())
                BeginCapture(near);
        }

        // 다른 입력처럼 잠금 중엔 S 내려가기도 막음
        private bool DropPressed() {
            return !_services.InputLock.IsLocked && _dropInput.Pressed;
        }

        private void BeginCapture(WindowWidget widget) {
            _captured = widget;
            _services.PlayerControl.SetSuspended(true);
            var velocity = _services.PlayerBody.Velocity;
            _services.PlayerBody.Velocity = new Vector2(0f, velocity.y);
            FollowAnchor();
        }

        // Alt/Return 중엔 그쪽 상태가 정지와 복귀를 맡아서 Normal일 때만 풀어줌
        private void EndCapture() {
            _captured = null;
            if (_services.Alt.Phase == AltPhase.Normal)
                _services.PlayerControl.SetSuspended(false);
        }

        // x만 핸들에 붙이고 높이는 물리에 맡김
        private void FollowAnchor() {
            if (_captured == null || !_captured.TryGetPlayerAnchor(out var anchor))
                return;

            Vector2 world = _window.TransformPoint(anchor);
            var position = _services.PlayerBody.Position;
            _services.PlayerBody.Position = new Vector2(world.x, position.y);
            _services.PlayerBody.Velocity = new Vector2(0f, _services.PlayerBody.Velocity.y);
        }

        private void ReleaseDrag() {
            if (_dragging == null)
                return;

            _dragging.PointerRelease();
            _dragging = null;
        }

        private WindowWidget FindEngaged() {
            for (var i = 0; i < _widgets.Count; i++)
                if (_widgets[i].PointerEngaged)
                    return _widgets[i];

            return null;
        }

        private WindowWidget FindPointed(Vector2 local) {
            for (var i = 0; i < _widgets.Count; i++)
                if (_widgets[i].PointerRect.Contains(local))
                    return _widgets[i];

            return null;
        }

        private WindowWidget FindNear() {
            var bounds = _services.PlayerCollider.bounds;
            Vector2 feet = _window.InverseTransformPoint(new Vector3(bounds.center.x, bounds.min.y, 0f));
            WindowWidget best = null;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < _widgets.Count; i++) {
                var widget = _widgets[i];
                if (!widget.ProximityRect.Contains(feet))
                    continue;

                var distance = Mathf.Abs(feet.y - widget.ProximityRect.center.y);
                if (distance < bestDistance) {
                    best = widget;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private void ShowFocus(WindowWidget widget) {
            if (widget == null)
                _focus.Hide();
            else
                _focus.Show(widget.FocusRect);
        }

        private Vector2 ToLocal(Vector2 world) {
            return _window.InverseTransformPoint(new Vector3(world.x, world.y, _window.position.z));
        }
    }
}