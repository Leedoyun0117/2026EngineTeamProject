using UnityEngine;

namespace LDY.Spider {
    [CreateAssetMenu(menuName = "LDY/Stage2/Spider Settings", fileName = "SpiderSettings")]
    public sealed class SpiderSettings : ScriptableObject {
        [Header("Follow")]
        [SerializeField] [Min(0f)] private float followMaxSpeed = 2.5f;
        [SerializeField] [Min(0.1f)] private float followAcceleration = 4f;

        [Header("Attack Timing")]
        [SerializeField] [Min(0f)] private float aimDuration = 0.4f;
        [SerializeField] [Min(0.05f)] private float raiseTime = 0.7f;
        [SerializeField] [Min(0f)] private float tremorTime = 0.18f;
        [SerializeField] [Min(0.02f)] private float stabTime = 0.12f;
        [SerializeField] [Min(0f)] private float holdTime = 2f;
        [SerializeField] [Min(0.1f)] private float retractTime = 1f;
        [SerializeField] [Min(0f)] private float cooldown = 1.2f;
        [SerializeField] [Min(0f)] private float targetError = 0.6f;

        [Header("Attack Shape")]
        [SerializeField] [Min(0f)] private float raiseHeight = 6f;
        [SerializeField] [Min(0f)] private float tremorAmplitude = 0.08f;
        [SerializeField] [Min(1f)] private float tremorHertz = 40f;
        [SerializeField] [Min(0f)] private float holdShake = 0.04f;
        [SerializeField] [Min(1f)] private float holdShakeHertz = 25f;
        [SerializeField] [Range(0.05f, 0.95f)] private float retractLiftShare = 0.4f;
        [SerializeField] [Min(0f)] private float retractRise = 3f;
        [SerializeField] [Min(0f)] private float retractArcHeight = 1.2f;
        [SerializeField] [Min(0.01f)] private float tipRadius = 0.25f;

        [Header("Stepping")]
        [SerializeField] [Min(0.1f)] private float stepThreshold = 1.5f;
        [SerializeField] [Min(0.05f)] private float stepTime = 0.35f;
        [SerializeField] [Min(0f)] private float stepHeight = 0.8f;

        [Header("Body")]
        [SerializeField] [Min(0f)] private float bobAmplitude = 0.12f;
        [SerializeField] [Min(0.1f)] private float bobPeriod = 3f;
        [SerializeField] [Min(0f)] private float leanDegrees = 3f;
        [SerializeField] [Min(0.1f)] private float leanRate = 8f;
        [SerializeField] [Min(0f)] private float impactAmplitude = 0.35f;
        [SerializeField] [Min(0.05f)] private float impactDuration = 0.35f;
        [SerializeField] [Min(0f)] private float glitchInterval = 6f;
        [SerializeField] [Min(0f)] private float glitchDuration = 0.15f;

        [Header("Attack Range")]
        [SerializeField] private LegReach[] legReach = {
            new LegReach(3f, 8f), new LegReach(1.5f, 7f), new LegReach(1.5f, 7f), new LegReach(3f, 8f)
        };
        [SerializeField] [Min(0.5f)] private float standoff = 4.5f;
        [SerializeField] [Min(0.01f)] private float arriveTolerance = 0.3f;

        [Header("Hurt")]
        [SerializeField] [Min(0f)] private float hurtTime = 0.8f;
        [SerializeField] [Min(0f)] private float hurtCooldown = 2.5f;
        [SerializeField] [Min(0f)] private float hurtRecoil = 0.6f;
        [SerializeField] [Min(0.05f)] private float hurtRecoilTime = 0.35f;
        [SerializeField] [Min(0f)] private float flailDegrees = 14f;
        [SerializeField] [Min(0.1f)] private float flailHertz = 7f;
        [SerializeField] [Min(0.01f)] private float hurtFrameTime = 0.1f;

        [Header("Flee")]
        [SerializeField] [Range(0.05f, 1f)] private float fleeTuckShare = 0.35f;
        [SerializeField] [Min(0f)] private float fleeTuckHang = 1.5f;
        [SerializeField] [Min(0.02f)] private float fleeGlitchInterval = 0.12f;
        [SerializeField] [Min(0.01f)] private float fleeGlitchDuration = 0.06f;

        [Header("Defeat")]
        [SerializeField] [Min(1)] private int seversToDefeat = 2;
        [SerializeField] [Min(0f)] private float fleeTime = 2.5f;
        [SerializeField] [Min(0f)] private float fleeDistance = 30f;
        [SerializeField] [Min(0f)] private float pieceLifetime = 5f;

        [Header("Telegraph Marker")]
        [SerializeField] [Min(0.1f)] private float markerBlinkHz = 6f;
        [SerializeField] [Range(0.05f, 1f)] private float markerStartScale = 0.25f;

        public float FollowMaxSpeed => followMaxSpeed;
        public float FollowAcceleration => followAcceleration;
        public float TipRadius => tipRadius;
        public float StepThreshold => stepThreshold;
        public float StepTime => stepTime;
        public float StepHeight => stepHeight;
        public float BobAmplitude => bobAmplitude;
        public float BobPeriod => bobPeriod;
        public float LeanDegrees => leanDegrees;
        public float LeanRate => leanRate;
        public float ImpactAmplitude => impactAmplitude;
        public float ImpactDuration => impactDuration;
        public float GlitchInterval => glitchInterval;
        public float GlitchDuration => glitchDuration;
        public float FleeDistance => fleeDistance;
        public float PieceLifetime => pieceLifetime;
        public float Standoff => standoff;
        public float HurtRecoil => hurtRecoil;
        public float HurtRecoilTime => hurtRecoilTime;
        public float FlailDegrees => flailDegrees;
        public float FlailHertz => flailHertz;
        public float HurtFrameTime => hurtFrameTime;
        public float FleeTuckShare => fleeTuckShare;
        public float FleeTuckHang => fleeTuckHang;
        public float FleeGlitchInterval => fleeGlitchInterval;
        public float FleeGlitchDuration => fleeGlitchDuration;
        public float MarkerBlinkHz => markerBlinkHz;
        public float MarkerStartScale => markerStartScale;

        public SpiderBrainConfig ToBrainConfig(int legCount, CombatZone zone) {
            return new SpiderBrainConfig {
                LegCount = legCount,
                AimDuration = aimDuration,
                TelegraphTime = raiseTime + tremorTime,
                StuckTime = holdTime,
                RetractTime = retractTime,
                Cooldown = cooldown,
                TargetError = targetError,
                SeversToDefeat = seversToDefeat,
                FleeTime = fleeTime,
                HurtTime = hurtTime,
                HurtCooldown = hurtCooldown,
                Reach = legReach.Length >= legCount ? legReach : null,
                Standoff = standoff,
                ArriveTolerance = arriveTolerance,
                Zone = zone
            };
        }

        public AttackMotionConfig ToAttackMotion() {
            return new AttackMotionConfig {
                RaiseHeight = raiseHeight,
                RaiseTime = raiseTime,
                TremorAmplitude = tremorAmplitude,
                TremorHertz = tremorHertz,
                StabTime = stabTime,
                HoldShake = holdShake,
                HoldShakeHertz = holdShakeHertz,
                RetractLiftShare = retractLiftShare,
                RetractRise = retractRise,
                RetractArcHeight = retractArcHeight
            };
        }
    }
}
