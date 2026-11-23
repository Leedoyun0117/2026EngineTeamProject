using UnityEngine;

namespace LDY.Spider {
    public sealed class NeedleKiller : MonoBehaviour {
        private const string PlayerTag = "Player";

        private readonly PresentedDeath _death = new PresentedDeath(new DestroyPlayerKiller());

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(PlayerTag))
                _death.Request(other.gameObject, DeathCause.Needle, transform.position);
        }
    }
}
