using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script {
    // 줄 전용 입력 인터페이스. 놓기/점프는 '눌린 프레임 번호'를 돌려준다.
    public interface IRopeInput {
        Vector2 Axis { get; }
        bool TryConsumeRelease(out int pressFrame);
        bool TryConsumeJump(out int pressFrame);
        void Clear();
    }

    // 줄 전용 자체 InputAction 묶음: F=놓기, Space=점프 놓기, WASD=이동. JJB 입력과 별개로 동작한다.
    public class RopeInput : IRopeInput, IDisposable {
        // 눌림 없음 표시값
        private const int NoPress = -1;

        // 입력 액션(키 바인딩)
        private readonly InputAction _jump = new("RopeJump", InputActionType.Button, "<Keyboard>/space");
        private readonly InputAction _move = new("RopeMove", InputActionType.Value);
        private readonly InputAction _release = new("RopeRelease", InputActionType.Button, "<Keyboard>/f");
        // 눌린 프레임 번호 저장(FixedUpdate가 나중에 읽을 때 ReleaseGate와 비교)
        private int _jumpFrame = NoPress;
        private int _releaseFrame = NoPress;

        // WASD 2D 벡터 바인딩 설정, 눌릴 때 현재 프레임 번호 기록
        public RopeInput() {
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _release.performed += _ => _releaseFrame = Time.frameCount;
            _jump.performed += _ => _jumpFrame = Time.frameCount;
        }

        // x: A/D 흔들기, y: W/S 오르내림
        public Vector2 Axis => _move.ReadValue<Vector2>();

        // 저장된 놓기 눌림을 한 번 꺼내고 비운다
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

        // 쌓인 눌림 기록 삭제
        public void Clear() {
            _releaseFrame = NoPress;
            _jumpFrame = NoPress;
        }

        // 액션 정리
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
