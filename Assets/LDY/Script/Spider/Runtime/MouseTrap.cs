using UnityEngine;

namespace LDY.Spider {
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class MouseTrap : MonoBehaviour {
        private const string PlayerTag = "Player";

        [SerializeField] private TrapSettings settings;
        [SerializeField] private MouseTrapView view;

        private readonly TrapState _state = new TrapState();
        private readonly PresentedDeath _death = new PresentedDeath(new DestroyPlayerKiller());

        public bool IsArmed => _state.Phase == TrapPhase.Armed;

        private void Awake() {
            var body = GetComponent<Rigidbody2D>();
            body.mass = settings.RestMass;
            body.gravityScale = settings.GravityScale;
            body.freezeRotation = true;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(PlayerTag)) {
                Resolve(_state.PlayerTouched(), other.gameObject, null);
                return;
            }

            var severable = other.GetComponentInParent<ISeverable>();
            if (severable != null)
                Resolve(_state.NeedleTouched(), null, severable);
        }

        private void Resolve(TrapOutcome outcome, GameObject player, ISeverable target) {
            if (outcome == TrapOutcome.None)
                return;

            view.PlaySnap();
            if (outcome == TrapOutcome.KillPlayer)
                _death.Request(player, DeathCause.Trap, transform.position);
            else
                target.Sever();
        }
    }
}
