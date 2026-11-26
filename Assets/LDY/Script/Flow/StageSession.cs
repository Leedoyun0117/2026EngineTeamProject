using UnityEngine;

namespace LDY.Flow {
    // 지금 플레이 중인 스테이지 id와 이번 시도의 경과 시간(일시정지 제외, scaled time).
    public sealed class StageSession {
        private float _startedAt;

        public string StageId { get; private set; }
        public bool IsActive => !string.IsNullOrEmpty(StageId);
        public float Elapsed => IsActive ? Time.time - _startedAt : 0f;

        public void Begin(string stageId) {
            StageId = stageId;
            _startedAt = Time.time;
        }

        public void End() => StageId = null;
    }
}
