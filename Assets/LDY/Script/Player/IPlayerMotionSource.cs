using UnityEngine;

namespace LDY.Script
{
    public interface IPlayerMotionSource
    {
        Vector2 Velocity { get; }
        bool IsGrounded { get; }
        // 데드존을 넘은 이동 방향. 오른쪽 +1, 왼쪽 -1, 방향이 없으면 0
        int FacingSign { get; }
        bool IsInputLocked { get; }
    }
}
