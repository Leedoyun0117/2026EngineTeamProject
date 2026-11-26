using LDY.PlayerLocate;
using UnityEngine;

namespace LDY.Spider {
    // 본체 판별은 PrimaryPlayerLocator에 맡긴다. 플레이어가 없을 때 매 물리 틱마다 씬을 뒤지지 않도록 다시 찾는 간격을 둔다.
    public sealed class TagPlayerLocator : IPlayerLocator {
        private const float RetrySeconds = 0.5f;

        private Transform _player;
        private float _nextSearch;

        public bool TryGetX(out float x) {
            if (_player == null && Time.time >= _nextSearch) {
                _nextSearch = Time.time + RetrySeconds;
                var found = PrimaryPlayerLocator.Find();
                _player = found != null ? found.transform : null;
            }

            x = _player != null ? _player.position.x : 0f;
            return _player != null;
        }
    }
}
