using LDY.Death;
using UnityEngine;

namespace LDY.Spider {
    public sealed class NeedleKiller : MonoBehaviour {
        private const string PlayerTag = "Player";

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(PlayerTag))
                PlayerKill.Request(other, DeathCause.Enemy);
        }
    }
}
