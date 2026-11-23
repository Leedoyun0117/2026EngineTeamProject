using LDY.Spider;
using UnityEngine;
using UnityEngine.Events;

namespace LDY.Script {
    // 쥐덫/바늘에 죽을 때 입력을 잠그고 연출(trapped → 끝 두 프레임 → death)을 보여 준 뒤 기존 사망 처리를 호출한다.
    [DisallowMultipleComponent]
    public sealed class PlayerDeathPresenter : MonoBehaviour, IDeathPresenter, IDeathPose {
        [SerializeField] [Min(0f)] private float pullTime = 0.25f;
        [SerializeField] [Min(0f)] private float endHold = 0.4f;
        [SerializeField] private UnityEvent onDeathFinished;

        private readonly DeathSequence _sequence = new DeathSequence();
        private readonly DeathInputBlock _inputBlock = new DeathInputBlock();
        private readonly IPlayerKiller _killer = new DestroyPlayerKiller();
        private PlayerRenderer _renderer;
        private Rigidbody2D _body;
        private float _startX;
        private float _targetX;

        public bool IsTrapped => _sequence.IsTrapped;
        public bool IsDead => _sequence.IsDead;
        public DeathStage Stage => _sequence.Stage;

        private void Awake() {
            _renderer = GetComponentInChildren<PlayerRenderer>();
            _body = GetComponent<Rigidbody2D>();
        }

        public bool Begin(DeathCause cause, Vector2 anchor) {
            if (_sequence.Stage != DeathStage.None)
                return false;

            var timings = _renderer != null ? _renderer.DeathTimingsFor(pullTime, endHold) : default;
            _inputBlock.Engage(gameObject);
            Freeze();

            var pulls = cause == DeathCause.Trap;
            _startX = _body != null ? _body.position.x : transform.position.x;
            _targetX = DeathPull.TargetX(pulls, _startX, anchor.x);
            if (pulls)
                _sequence.BeginTrapped(timings);
            else
                _sequence.BeginInPlace(timings);

            if (_renderer != null)
                _renderer.Attach(this);

            Debug.Log($"[PlayerDeath] {cause} 시작 (renderer={_renderer != null}, lock={_inputBlock.IsEngaged}, " +
                      $"trapped={timings.TrappedTime:F2}s, death={timings.DeathTime:F2}s)", this);
            return true;
        }

        private void FixedUpdate() {
            if (!_sequence.IsActive)
                return;

            _sequence.Tick(Time.fixedDeltaTime);
            Hold();
            if (_sequence.Finished)
                Finish();
        }

        private void OnDestroy() {
            _inputBlock.Dispose();
        }

        private void Freeze() {
            if (_body == null)
                return;

            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Hold() {
            if (_body == null)
                return;

            _body.linearVelocity = Vector2.zero;
            if (_startX == _targetX)
                return;

            var x = Mathf.Lerp(_startX, _targetX, Mathf.SmoothStep(0f, 1f, _sequence.PullProgress));
            _body.position = new Vector2(x, _body.position.y);
        }

        private void Finish() {
            onDeathFinished?.Invoke();
            _killer.Kill(gameObject);
        }
    }
}
