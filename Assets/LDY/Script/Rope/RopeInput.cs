using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script {
    public interface IRopeInput {
        Vector2 Axis { get; }
        bool TryConsumeRelease(out int pressFrame);
        bool TryConsumeJump(out int pressFrame);
        void Clear();
    }

    public class RopeInput : IRopeInput, IDisposable {
        private const int NoPress = -1;

        private readonly InputAction _jump = new("RopeJump", InputActionType.Button, "<Keyboard>/space");
        private readonly InputAction _move = new("RopeMove", InputActionType.Value);
        private readonly InputAction _release = new("RopeRelease", InputActionType.Button, "<Keyboard>/f");
        private int _jumpFrame = NoPress;
        private int _releaseFrame = NoPress;

        public RopeInput() {
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _release.performed += _ => _releaseFrame = Time.frameCount;
            _jump.performed += _ => _jumpFrame = Time.frameCount;
        }

        public Vector2 Axis => _move.ReadValue<Vector2>();

        public bool TryConsumeRelease(out int pressFrame) {
            pressFrame = _releaseFrame;
            _releaseFrame = NoPress;
            return pressFrame != NoPress;
        }

        public bool TryConsumeJump(out int pressFrame) {
            pressFrame = _jumpFrame;
            _jumpFrame = NoPress;
            return pressFrame != NoPress;
        }

        public void Clear() {
            _releaseFrame = NoPress;
            _jumpFrame = NoPress;
        }

        public void Dispose() {
            Disable();
            _release.Dispose();
            _jump.Dispose();
            _move.Dispose();
        }

        public void Enable() {
            _release.Enable();
            _jump.Enable();
            _move.Enable();
        }

        public void Disable() {
            _release.Disable();
            _jump.Disable();
            _move.Disable();
            Clear();
        }
    }
}
