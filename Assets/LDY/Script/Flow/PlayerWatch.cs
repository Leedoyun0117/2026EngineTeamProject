using UnityEngine;

namespace LDY.Flow {
    // 플레이어가 Destroy되면(팀원 KillHitbox 등) 한 번만 알린다. StageRoot가 없는 씬에서도 동작한다.
    public sealed class PlayerWatch {
        private GameObject _player;
        private bool _watching;

        public void Track() {
            _player = GameObject.FindGameObjectWithTag(PlayerMatch.DefaultTag);
            _watching = _player != null;
        }

        public void Stop() {
            _player = null;
            _watching = false;
        }

        public bool PollLost() {
            if (!_watching || _player != null) return false;
            _watching = false;
            return true;
        }
    }
}
