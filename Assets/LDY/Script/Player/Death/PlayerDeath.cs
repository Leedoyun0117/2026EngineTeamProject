using LDY.Death;
using UnityEngine;

namespace LDY.Script {
    // 플레이어 사망의 단일 진입점. 입력 잠금 → 정지 → (덫이면 끌어당김) → 원인별 클립 → 지연 → 재시작 요청.
    [DisallowMultipleComponent]
    public sealed class PlayerDeath : MonoBehaviour, IPlayerDeath {
        private readonly DeathGate _gate = new DeathGate();
        private readonly DeathInputBlock _inputBlock = new DeathInputBlock();
        private PlayerDeathSettings _settings;
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;
        private PlayerSpriteSnapshot _lastSeen;
        private DeathCause _cause;
        private DeathPull _pull;
        private DeathClipRunner _runner;

        public bool IsDying => _gate.IsDying;

        private void Awake() {
            _settings = PlayerDeathSettings.Load();
            _body = GetComponent<Rigidbody2D>();
        }

        public bool Die(DeathCause cause, float? pullToX = null) {
            if (!_gate.TryBegin())
                return false;

            _cause = cause;
            _inputBlock.Engage(gameObject);
            PlayerRestraint.Apply(gameObject);
            if (DeathPull.Applies(cause, pullToX))
                _pull = new DeathPull(CurrentX, pullToX.Value, PullTime);
            else
                StartClips();

            Debug.Log($"[PlayerDeath] {cause} (pull={_pull != null}, settings={_settings != null})", this);
            return true;
        }

        private float CurrentX => _body != null ? _body.position.x : transform.position.x;
        private float PullTime => _settings != null ? _settings.PullTime : DeathPull.DefaultDuration;

        private void FixedUpdate() {
            if (_pull == null)
                return;

            var x = _pull.Tick(Time.fixedDeltaTime);
            MoveTo(x);
            if (!_pull.Finished)
                return;

            _pull = null;
            StartClips();
        }

        private void Update() {
            if (_runner != null && _runner.Tick(Time.deltaTime) && _gate.TryRequestRestart())
                StageRestart.Current.RequestRestart();
        }

        private void LateUpdate() {
            if (_sprite == null)
                _sprite = PlayerSpriteLocator.Find(gameObject);

            _lastSeen = PlayerSpriteSnapshot.Capture(_sprite, transform);
        }

        private void OnDestroy() {
            _inputBlock.Dispose();
            var unloading = !gameObject.scene.isLoaded || StageRestart.Current.IsBusy;
            if (DestroyJudge.Judge(_gate.Phase, QuitWatch.IsQuitting, unloading) != DestroyVerdict.SpawnPuppet)
                return;

            var snapshot = _sprite != null ? PlayerSpriteSnapshot.Capture(_sprite, transform) : _lastSeen;
            Debug.Log("[PlayerDeath] 재시작 요청 전에 플레이어가 파괴되어 대역이 사망 연출을 이어받는다", this);
            DeathPuppet.Spawn(snapshot, _settings, StageRestart.Current);
        }

        private void StartClips() {
            PlayerRestraint.MuteSpriteDrivers(gameObject);
            _sprite = PlayerSpriteLocator.Find(gameObject);
            _runner = new DeathClipRunner(_sprite, _settings, DeathClipOrder.For(_cause));
        }

        private void MoveTo(float x) {
            if (_body == null) {
                transform.position = new Vector3(x, transform.position.y, transform.position.z);
                return;
            }

            _body.linearVelocity = Vector2.zero;
            _body.position = new Vector2(x, _body.position.y);
        }
    }
}
