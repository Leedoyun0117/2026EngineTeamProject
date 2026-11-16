using UnityEngine;

namespace LDY.Script {
    [DisallowMultipleComponent]
    public class PlayerRenderer : MonoBehaviour {
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

        private IPlayerMotionSource _source;

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
            Play(_machine.Current, 0);
        }

        private void Update() {
            if (!_ready)
                return;

            if (_alt != null && _alt.Phase != AltPhase.Normal) {
                if (animSet.AltSprite != null)
                    target.sprite = animSet.AltSprite;
                return;
            }

            var dt = Time.deltaTime;
            var velocity = _source.Velocity;

            var input = new PlayerAnimInput(velocity, _source.IsGrounded, _source.IsInputLocked, dt,
                _player.Finished, _player.Frame, _pose.IsGrabbing, _pose.RowMove, _pose.IsRopeHanging);
            var id = _machine.Tick(input);
            if (_machine.Changed)
                Play(id, _machine.EntryFrame);

            _player.Tick(dt, PlaybackScale(id, velocity.x));
            ApplySprite();
            if (flipSprite)
                _flip.Apply(_source.FacingSign, id == PlayerAnimId.Grab || RopePose.IsRope(id));
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