using LDY.TitlePlayer.Core;
using UnityEngine;

namespace LDY.TitlePlayer {
    [CreateAssetMenu(menuName = "LDY/Player/TitlePlayerTuning", fileName = "TitlePlayerTuning")]
    public sealed class TitlePlayerTuning : ScriptableObject {
        [Header("Move")] [Min(TuningMapping.MinMoveSpeed)] public float moveSpeed = 5f;

        [Header("Jump")] [Min(0f)] public float jumpSpeed = 8f;
        [Min(0f)] public float maxJumpHoldTime = 0.3f;

        [Header("Extra Gravity")] [Min(0f)] public float extraGravity = 30f;
        [Min(0f)] public float gravityDelay = 0.15f;

        [Header("Ground Check")] public Vector2 groundCheckSize = new Vector2(0.9f, 0.12f);
        public float groundCheckOffsetY = -0.5f;
        public LayerMask groundLayer;

        public MotorTuning Motor => new MotorTuning(moveSpeed, jumpSpeed, maxJumpHoldTime, extraGravity, gravityDelay);

        public void Apply(TitleTuningValues values) {
            moveSpeed = values.Motor.MoveSpeed;
            jumpSpeed = values.Motor.JumpSpeed;
            maxJumpHoldTime = values.Motor.MaxJumpHoldTime;
            extraGravity = values.Motor.ExtraGravity;
            gravityDelay = values.Motor.GravityDelay;
            groundCheckSize = new Vector2(values.GroundCheckWidth, values.GroundCheckHeight);
            groundCheckOffsetY = values.GroundCheckOffsetY;
            groundLayer = values.GroundLayerMask;
        }

        public string Describe() =>
            $"move {moveSpeed}, jump {jumpSpeed}, hold {maxJumpHoldTime}, extraGravity {extraGravity}, " +
            $"delay {gravityDelay}, check {groundCheckSize} @ y {groundCheckOffsetY}, layer {groundLayer.value}";
    }
}
