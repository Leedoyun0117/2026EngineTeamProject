using LDY.Death;
using UnityEngine;

namespace LDY.Flow {
    [RequireComponent(typeof(Collider2D))]
    public sealed class StageDeathZone : MonoBehaviour {
        [SerializeField] private PlayerMatch player = new PlayerMatch();
        [SerializeField] private DeathCause cause = DeathCause.Hazard;

        private void Reset() => GetComponent<Collider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other) {
            if (player.Matches(other)) PlayerKill.Request(other, cause);
        }
    }
}
