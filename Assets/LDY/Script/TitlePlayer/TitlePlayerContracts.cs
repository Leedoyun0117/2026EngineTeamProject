using UnityEngine;

namespace LDY.TitlePlayer {
    // 입력 잠금·Alt 정지·점프 차단을 받는 쪽. 기존 LDY 시스템은 어댑터(TitlePlayerControl)를 거쳐 이걸 부른다.
    public interface IMovementLock {
        void SetLocked(bool locked);
        void SetSuspended(bool suspended);
        void SetJumpBlocked(bool blocked);
    }

    public interface IMotorReadout {
        Vector2 Velocity { get; }
        bool IsGrounded { get; }
    }
}
