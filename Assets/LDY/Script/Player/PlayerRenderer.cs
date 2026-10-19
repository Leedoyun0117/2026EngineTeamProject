using UnityEngine;

namespace LDY.Script
{
    [DisallowMultipleComponent]
    public class PlayerRenderer : MonoBehaviour
    {
        [SerializeField] private PlayerAnimSet animSet;
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private PlayerAnimRules rules = new PlayerAnimRules();

        private IPlayerMotionSource _source;
        private IAltStatus _alt;
        private PlayerAnimStateMachine _machine;
        private SpriteFramePlayer _player;
        private SpriteFlipDriver _flip;

        private void Awake()
        {
            if (target == null)
                target = GetComponentInChildren<SpriteRenderer>();

            _source = GetComponent<IPlayerMotionSource>();
            _alt = GetComponent<IAltStatus>();
            _machine = new PlayerAnimStateMachine(rules);
            _player = new SpriteFramePlayer();
            _flip = new SpriteFlipDriver(target);

            if (animSet == null || _source == null)
            {
                Debug.LogError("[PlayerRenderer] PlayerAnimSet 또는 IPlayerMotionSource 없음, 비활성화", this);
                enabled = false;
                return;
            }

            Play(_machine.Current, 0);
        }

        private void Update()
        {
            if (_alt != null && _alt.Phase != AltPhase.Normal)
            {
                if (animSet.AltSprite != null)
                    target.sprite = animSet.AltSprite;
                return;
            }

            float dt = Time.deltaTime;
            Vector2 velocity = _source.Velocity;

            var input = new PlayerAnimInput(velocity, _source.IsGrounded, _source.IsInputLocked, dt,
                _player.Finished, _player.Frame);
            PlayerAnimId id = _machine.Tick(input);
            if (_machine.Changed)
                Play(id, _machine.EntryFrame);

            _player.Tick(dt, PlaybackScale(id, velocity.x));
            ApplySprite();
            _flip.Apply(_source.FacingSign);
        }

        private float PlaybackScale(PlayerAnimId id, float velocityX)
        {
            bool scaled = id == PlayerAnimId.Walk || id == PlayerAnimId.Run;
            return scaled ? rules.SpeedScale(Mathf.Abs(velocityX)) : 1f;
        }

        private void Play(PlayerAnimId id, int startFrame)
        {
            _player.Play(animSet.Resolve(id), startFrame);
            ApplySprite();
        }

        private void ApplySprite()
        {
            Sprite sprite = _player.Sprite;
            if (sprite != null)
                target.sprite = sprite;
        }
    }
}
