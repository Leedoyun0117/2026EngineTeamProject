using UnityEngine;

namespace LDY.Script {
    [DisallowMultipleComponent]
    public class PlayerRenderer : MonoBehaviour {
        [SerializeField] private PlayerAnimSet animSet;
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private PlayerAnimRules rules = new();
        private IAltStatus _alt;
        private SpriteFlipDriver _flip;
        private IGrabStateSource _grab;
        private IRowMoveSource _rowMove;
        private PlayerAnimStateMachine _machine;
        private SpriteFramePlayer _player;

        private IPlayerMotionSource _source;

        private void Awake() {
            if (target == null)
                target = GetComponentInChildren<SpriteRenderer>();

            _source = GetComponent<IPlayerMotionSource>();
            _alt = GetComponent<IAltStatus>();
            _grab = GetComponent<IGrabStateSource>();
            _rowMove = GetComponent<IRowMoveSource>();
            _machine = new PlayerAnimStateMachine(rules);
            _player = new SpriteFramePlayer();
            _flip = new SpriteFlipDriver(target);

            if (animSet == null || _source == null) {
                Debug.LogError("[PlayerRenderer] PlayerAnimSet 또는 IPlayerMotionSource 없음, 비활성화", this);
                enabled = false;
                return;
            }

            Play(_machine.Current, 0);
        }

        private void Update() {
            if (_alt != null && _alt.Phase != AltPhase.Normal) {
                if (animSet.AltSprite != null)
                    target.sprite = animSet.AltSprite;
                return;
            }

            var dt = Time.deltaTime;
            var velocity = _source.Velocity;

            var input = new PlayerAnimInput(velocity, _source.IsGrounded, _source.IsInputLocked, dt,
                _player.Finished, _player.Frame, _grab != null && _grab.IsGrabbing,
                _rowMove?.RowMove ?? RowMoveDirection.None);
            var id = _machine.Tick(input);
            if (_machine.Changed)
                Play(id, _machine.EntryFrame);

            _player.Tick(dt, PlaybackScale(id, velocity.x));
            ApplySprite();
            _flip.Apply(_source.FacingSign, id == PlayerAnimId.Grab);
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