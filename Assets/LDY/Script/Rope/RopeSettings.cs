using UnityEngine;

namespace LDY.Script {
    [CreateAssetMenu(menuName = "LDY/Rope Settings", fileName = "RopeSettings")]
    public class RopeSettings : ScriptableObject {
        [Tooltip("min/maxLength: 플레이어가 anchor로부터 줄 위에서 위치할 수 있는 거리 범위")]
        [SerializeField] private PendulumTuning tuning = new() {
            minLength = 1f,
            maxLength = 4.5f,
            gravity = 25f,
            damping = 0.15f,
            quadraticDamping = 0.25f,
            swingForce = 9f,
            climbSpeed = 2.5f,
            softLimitStartDegrees = 40f,
            softLimitStrength = 4f,
            maxAngleDegrees = 90f
        };

        [Tooltip("올라갈 때 줄이 살짝 흔들리는 정도: strength 반동 세기, frequency Hz")]
        [SerializeField] private ClimbSwayTuning climbSway = new() {
            strength = 0.3f,
            frequency = 1.2f,
            rampTime = 0.25f
        };

        [SerializeField] [Min(0.1f)] private float totalLength = 6f;

        [Tooltip("segmentCount: 줄을 이루는 점의 개수")]
        [SerializeField] private RopeCurveTuning curve = new() {
            segmentCount = 20,
            totalLength = 6f,
            gravity = 100f,
            iterations = 100,
            holdBlendTime = 0.15f,
            sleepSpeed = 0.002f,
            tailStiffness = 0.5f
        };

        [SerializeField] private RopeDampingTuning ropeDamping = new() {
            heldDamping = 1.5f,
            heldTailDamping = 8f,
            releaseDamping = 0.3f,
            settleTime = 3f,
            releaseBlendTime = 0.25f
        };

        [SerializeField] [Min(0f)] private float jumpReleaseBonus = 6f;
        [SerializeField] [Min(0f)] private float regrabCooldown = 0.2f;

        public PendulumTuning Tuning => RopeLengthLimits.Apply(tuning, totalLength);

        public RopeCurveTuning Curve {
            get {
                var value = curve;
                value.totalLength = totalLength;
                return value;
            }
        }

        public RopeDampingTuning Damping => ropeDamping;
        public ClimbSwayTuning ClimbSway => climbSway;
        public float TotalLength => totalLength;
        public float JumpReleaseBonus => jumpReleaseBonus;
        public float RegrabCooldown => regrabCooldown;
    }
}
