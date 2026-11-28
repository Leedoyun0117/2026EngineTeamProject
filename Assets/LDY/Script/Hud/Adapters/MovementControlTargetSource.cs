using JJB.Script.Player;
using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud.Adapters {
    // 복제체(CopyPlayer_HTY가 만든 JJBPlayer)를 찾고, 본체의 JJBPlayerMovement.IsPlayObj로 지금 조작 대상을 판단한다.
    internal sealed class MovementControlTargetSource : IControlTargetSource, IHudRefreshable {
        private const float ScanInterval = 0.2f;

        private readonly JJBPlayerMovement _primary;
        private JJBPlayerMovement _clone;
        private float _nextScan;

        private MovementControlTargetSource(JJBPlayerMovement primary) {
            _primary = primary;
        }

        public static MovementControlTargetSource TryCreate(GameObject player, out string reason) {
            reason = player.TryGetComponent<JJBPlayerMovement>(out var movement) ? null : $"{player.name}에 JJBPlayerMovement 없음";
            return movement != null ? new MovementControlTargetSource(movement) : null;
        }

        public bool HasClone => _primary != null && _clone != null && _clone.isActiveAndEnabled;
        public bool CloneControlled => HasClone && !_primary.IsPlayObj;

        public HudRect IdleBounds =>
            HasClone ? WorldBounds.Of(CloneControlled ? _primary.gameObject : _clone.gameObject) : default;

        public void Refresh() {
            if (_primary == null || Time.unscaledTime < _nextScan && (_clone == null || _clone.isActiveAndEnabled))
                return;

            _nextScan = Time.unscaledTime + ScanInterval;
            _clone = FindClone();
        }

        private JJBPlayerMovement FindClone() {
            foreach (var movement in Object.FindObjectsByType<JJBPlayerMovement>(FindObjectsSortMode.None)) {
                if (movement != _primary && movement.isActiveAndEnabled && movement.TryGetComponent<JJBPlayer>(out _))
                    return movement;
            }

            return null;
        }
    }
}
