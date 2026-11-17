using UnityEngine;

namespace LDY.Script {
    // 줄과 매달림에 쓰이는 모든 수치를 담는 데이터 에셋. 값은 코드가 아니라 이 에셋에서 조절한다.
    [CreateAssetMenu(menuName = "LDY/Rope Settings", fileName = "RopeSettings")]
    public class RopeSettings : ScriptableObject {
        // 진자 수치: 길이 범위, 중력, 감쇠, 흔들기 힘, 오르내림 속도, 각도 한계(소프트/하드)
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

        // 오르내릴 때 반동 수치
        [Tooltip("올라갈 때 줄이 살짝 흔들리는 정도: strength 반동 세기, frequency Hz")]
        [SerializeField] private ClimbSwayTuning climbSway = new() {
            strength = 0.3f,
            frequency = 1.2f,
            rampTime = 0.25f
        };

        // 줄 전체 길이(Curve.totalLength를 덮어씀)
        [SerializeField] [Min(0.1f)] private float totalLength = 6f;

        // 줄 모양(Verlet) 수치: 점 개수, 중력, 제약 반복 횟수 등
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

        // 잡은 동안/놓은 뒤 감쇠 수치
        [SerializeField] private RopeDampingTuning ropeDamping = new() {
            heldDamping = 1.5f,
            heldTailDamping = 8f,
            releaseDamping = 0.3f,
            settleTime = 3f,
            releaseBlendTime = 0.25f
        };

        // 매달린 플레이어 회전/발 고정 수치
        [Tooltip("매달린 플레이어 회전: 각도 단위, 최대 각도, 붙고 놓을 때 회전 블렌드 시간, 발 고정 위치 보간 시간(초)")]
        [SerializeField] private GripTuning grip = new() {
            angleStepDegrees = 7.5f,
            maxAngleDegrees = 60f,
            blendTime = 0.1f,
            footSmoothTime = 0.04f
        };

        // Space로 놓을 때 위로 더해지는 속도 / 놓은 뒤 재잡기 쿨다운 / 잡은 직후 놓기 입력 무시 시간
        [SerializeField] [Min(0f)] private float jumpReleaseBonus = 6f;
        [SerializeField] [Min(0f)] private float regrabCooldown = 0.2f;
        [SerializeField] [Min(0f)] private float releaseInputDelay = 0.05f;

        // 거리 범위를 총길이의 80% 안으로 제한해 내보낸다
        public PendulumTuning Tuning => RopeLengthLimits.Apply(tuning, totalLength);

        // 총길이를 채워서 내보낸다
        public RopeCurveTuning Curve {
            get {
                var value = curve;
                value.totalLength = totalLength;
                return value;
            }
        }

        public GripTuning Grip => grip;
        public RopeDampingTuning Damping => ropeDamping;
        public ClimbSwayTuning ClimbSway => climbSway;
        public float TotalLength => totalLength;
        public float JumpReleaseBonus => jumpReleaseBonus;
        public float RegrabCooldown => regrabCooldown;
        public float ReleaseInputDelay => releaseInputDelay;
    }
}
