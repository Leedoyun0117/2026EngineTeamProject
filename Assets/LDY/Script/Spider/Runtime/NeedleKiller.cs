using LDY.Death;
using LDY.PlayerLocate;
using UnityEngine;

namespace LDY.Spider {
    public sealed class NeedleKiller : MonoBehaviour {
        private const string PlayerTag = PrimaryPlayerLocator.Tag;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(PlayerTag))
                PlayerKill.Request(other, DeathCause.Enemy);
        }
    }
}
