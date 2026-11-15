using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script {
    public interface IRopeInput {
        Vector2 Axis { get; }
        bool ConsumeGrab();
        bool ConsumeJump();
        void Clear();
    }

    public class RopeInput : IRopeInput, IDisposable {
        private readonly InputAction _grab = new("RopeGrab", InputActionType.Button, "<Keyboard>/f");
        private readonly InputAction _jump = new("RopeJump", InputActionType.Button, "<Keyboard>/space");
        private readonly InputAction _move = new("RopeMove", InputActionType.Value);
        private bool _grabLatched;
        private bool _jumpLatched;

        public RopeInput() {
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _grab.performed += _ => _grabLatched = true;
            _jump.performed += _ => _jumpLatched = true;
        }

        public Vector2 Axis => _move.ReadValue<Vector2>();

        public bool ConsumeGrab() {
            var value = _grabLatched;
            _grabLatched = false;
            return value;
        }

        public bool ConsumeJump() {
            var value = _jumpLatched;
            _jumpLatched = false;
            return value;
        }

        public void Clear() {
            _grabLatched = false;
            _jumpLatched = false;
        }

        public void Dispose() {
            Disable();
            _grab.Dispose();
            _jump.Dispose();
            _move.Dispose();
        }

        public void Enable() {
            _grab.Enable();
            _jump.Enable();
            _move.Enable();
        }

        public void Disable() {
            _grab.Disable();
            _jump.Disable();
            _move.Disable();
            Clear();
        }
    }
}
