using System.Collections.Generic;

namespace LDY.Script {
    // 실제로 플레이어 조작을 멈추고 푸는 쪽(구체 방법을 숨김).
    public interface ISuspendSink {
        void SetSuspended(bool suspended);
    }

    // 정지 요청 합산. 여러 곳(줄, Alt)이 각자 요청/해제하고, 하나라도 남아 있으면 정지 상태를 유지한다.
    public sealed class SuspendRequestHub {
        // 현재 정지를 요청 중인 소유자들
        private readonly HashSet<object> _owners = new();
        private readonly ISuspendSink _sink;

        public SuspendRequestHub(ISuspendSink sink) {
            _sink = sink;
        }

        public bool IsSuspended => _owners.Count > 0;

        // 첫 요청자(0->1)일 때만 실제 정지
        public void Request(object owner) {
            if (owner == null || !_owners.Add(owner))
                return;

            if (_owners.Count == 1)
                _sink.SetSuspended(true);
        }

        // 마지막 요청자(1->0)가 빠질 때만 실제 해제
        public void Release(object owner) {
            if (owner == null || !_owners.Remove(owner))
                return;

            if (_owners.Count == 0)
                _sink.SetSuspended(false);
        }
    }
}
