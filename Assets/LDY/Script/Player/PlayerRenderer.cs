using UnityEngine;

namespace LDY.Script
{
    // 부품 조립과 Update 위임만 담당한다. 애니메이션 시간은 scaled Time.deltaTime이라 Alt(timeScale 0) 중에는 멈춘다.
    [DisallowMultipleComponent]
    public class PlayerRenderer : MonoBehaviour
    {
        [SerializeField] private PlayerAnimSet animSet;
        [Tooltip("비우면 AltModeController와 같은 방식(GetComponentInChildren)으로 찾는다")]
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
                Debug.LogError("[PlayerRenderer] PlayerAnimSet 또는 IPlayerMotionSource가 없어 비활성화합니다.", this);
                enabled = false;
                return;
            }

            Play(_machine.Current, 0);
        }

        private void Update()
        {
            // Alt/복귀 중에는 틱을 멈추고 눈 감은 스프라이트를 보여준다. Normal로 돌아오면 아래 ApplySprite가 현재 프레임을 되돌린다.
            // 방향(flipX)은 건드리지 않아 그대로 유지된다.
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
