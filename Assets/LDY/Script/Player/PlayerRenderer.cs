using UnityEngine;

namespace LDY.Script {
    [DisallowMultipleComponent]
    public class PlayerRenderer : MonoBehaviour {
        private const int TrappedTailFrames = 2;

        [SerializeField] private PlayerAnimSet animSet;
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private PlayerAnimRules rules = new();
        [SerializeField] private bool resolveSourcesOnAwake = true;
        [SerializeField] private bool flipSprite = true;
        private IAltStatus _alt;
        private SpriteFlipDriver _flip;
        private PlayerAnimStateMachine _machine;
        private PoseSourceSet _pose;
        private SpriteFramePlayer _player;
        private bool _ready;
        private bool _landAvailable;

        private IPlayerMotionSource _source;
        private IDeathPose _death;
        private DeathStage _lastDeathStage;

        public void Attach(IDeathPose death) {
            _death = death;
        }

        public DeathTimings DeathTimingsFor(float pullTime, float endHold) {
            var trapped = animSet != null ? animSet.Resolve(PlayerAnimId.Trapped) : null;
            var death = animSet != null ? animSet.Resolve(PlayerAnimId.Death) : null;
            return new DeathTimings {
                PullTime = pullTime,
                TrappedTime = trapped?.TotalDuration ?? 0f,
                TailTime = trapped?.TailDuration(TrappedTailFrames) ?? 0f,
                DeathTime = death?.TotalDuration ?? 0f,
                EndHold = endHold
            };
        }

        private void Awake() {
            if (resolveSourcesOnAwake)
                Initialize(GetComponent<IPlayerMotionSource>(), GetComponent<IAltStatus>(),
                    GetComponent<IGrabStateSource>(), GetComponent<IRowMoveSource>(), GetComponent<IRopeStateSource>());
        }

        public void Initialize(IPlayerMotionSource source, IAltStatus alt, IGrabStateSource grab,
            IRowMoveSource rowMove, IRopeStateSource rope = null) {
            if (target == null)
                target = GetComponentInChildren<SpriteRenderer>();

            _machine = new PlayerAnimStateMachine(rules);
            _player = new SpriteFramePlayer();
            _flip = new SpriteFlipDriver(target);

            switch (RendererReadinessRules.Evaluate(source != null, target != null, animSet != null)) {
                case RendererReadiness.Broken:
                    Debug.LogError("[PlayerRenderer] IPlayerMotionSource 또는 SpriteRenderer 없음, 비활성화", this);
                    enabled = false;
                    return;
                case RendererReadiness.MissingAnimSet:
                    Debug.LogWarning("[PlayerRenderer] PlayerAnimSet 없음, 기본 스프라이트를 유지합니다", this);
                    return;
            }

            _source = source;
            _alt = alt;
            _pose = new PoseSourceSet(grab, rowMove, rope);
            _ready = true;
            _landAvailable = animSet.Has(PlayerAnimId.Land);
            Play(_machine.Current, 0);
        }

        private void Update() {
            if (!_ready)
                return;

            var dead = _death != null && _death.IsDead;
            var trapped = _death != null && _death.IsTrapped;
            var altActive = _alt != null && _alt.Phase != AltPhase.Normal;
            if (PosePriority.Resolve(dead, trapped, altActive) == PoseLayer.Alt) {
                if (animSet.AltSprite != null)
                    target.sprite = animSet.AltSprite;
                return;
            }

            var dt = Time.deltaTime;
            var velocity = _source.Velocity;

            var input = new PlayerAnimInput(velocity, _source.IsGrounded, _source.IsInputLocked, dt,
                _player.Finished, _player.Frame, _pose.IsGrabbing, _pose.RowMove, _pose.IsRopeHanging,
                _landAvailable, trapped, dead);
            var id = _machine.Tick(input);
            if (_machine.Changed)
                Play(id, _machine.EntryFrame);

            ReplayTrappedTail();

            _player.Tick(dt, PlaybackScale(id, velocity.x));
            ApplySprite();
            if (flipSprite)
                _flip.Apply(_source.FacingSign, id == PlayerAnimId.Grab || RopePose.IsRope(id));
        }

        // trapped가 끝나면 마지막 두 프레임을 한 번 더 보여 준다.
        private void ReplayTrappedTail() {
            var stage = _death != null ? _death.Stage : DeathStage.None;
            var entered = stage != _lastDeathStage;
            _lastDeathStage = stage;
            if (!entered || stage != DeathStage.TrappedTail)
                return;

            var clip = animSet.Resolve(PlayerAnimId.Trapped);
            if (clip != null && clip.IsValid) {
                _player.Play(clip, clip.TailStartFrame(TrappedTailFrames));
                ApplySprite();
            }
        }

        private float PlaybackScale(PlayerAnimId id, float velocityX) {
            var scaled = id == PlayerAnimId.Walk || id == PlayerAnimId.Run;
            return scaled ? rules.SpeedScale(Mathf.Abs(velocityX)) : 1f;
        }

        private void Play(PlayerAnimId id, int startFrame) {
            _player.Play(animSet.Resolve(id), startFrame);
            ApplySprite();
        }

        private void ApplySprite() {
            var sprite = _player.Sprite;
            if (sprite != null)
                target.sprite = sprite;
        }
    }
}