using LDY.PlayerLocate;
using UnityEngine;

namespace LDY.Flow {
    [RequireComponent(typeof(Collider2D))]
    public sealed class StageGoal : MonoBehaviour {
        [SerializeField] private PlayerMatch player = new PlayerMatch();

        private bool _reached;

        private void Reset() => GetComponent<Collider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other) {
            if (_reached || !player.Matches(other) || !PrimaryPlayerLocator.IsPrimary(PlayerMatch.Body(other))) return;
            _reached = true;
            StageSignals.RaiseCleared();
        }
    }
}
