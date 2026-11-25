using UnityEngine;

namespace LDY.Flow {
    [RequireComponent(typeof(Collider2D))]
    public sealed class StageDeathZone : MonoBehaviour {
        [SerializeField] private PlayerMatch player = new PlayerMatch();

        private void Reset() => GetComponent<Collider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other) {
            if (player.Matches(other)) StageSignals.RaisePlayerLost();
        }
    }
}
