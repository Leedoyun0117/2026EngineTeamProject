using UnityEngine;

namespace LDY.Script
{
    public interface IPlayerMotionSource
    {
        Vector2 Velocity { get; }
        bool IsGrounded { get; }
        int FacingSign { get; }
        bool IsInputLocked { get; }
    }
}
