using UnityEngine;

namespace LDY.Spider {
    public sealed class TagPlayerLocator : IPlayerLocator {
        private const string PlayerTag = "Player";
        private Transform _player;

        public bool TryGetX(out float x) {
            if (_player == null) {
                var found = GameObject.FindWithTag(PlayerTag);
                _player = found != null ? found.transform : null;
            }

            x = _player != null ? _player.position.x : 0f;
            return _player != null;
        }
    }
}
