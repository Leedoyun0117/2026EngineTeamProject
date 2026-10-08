using UnityEngine;

namespace LDY.Script
{
    public readonly struct FacingState
    {
        public readonly Vector3 LocalScale;
        public readonly bool FlipX;

        public FacingState(Vector3 localScale, bool flipX)
        {
            LocalScale = localScale;
            FlipX = flipX;
        }
    }

    public readonly struct AltSnapshot
    {
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly FacingState Facing;

        public AltSnapshot(Vector2 position, Vector2 velocity, FacingState facing)
        {
            Position = position;
            Velocity = velocity;
            Facing = facing;
        }
    }

    public class AltSession
    {
        public AltSnapshot Snapshot;
        public IAfterimage Afterimage;
    }
}
