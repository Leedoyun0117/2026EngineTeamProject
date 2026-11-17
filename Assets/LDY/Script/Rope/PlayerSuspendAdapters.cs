using UnityEngine;

namespace LDY.Script {
    // ISuspendSink -> IPlayerControl.SetSuspended 연결 어댑터. 허브가 JJB 쪽 타입을 모르게 한다.
    public class PlayerControlSuspendSink : ISuspendSink {
        private readonly IPlayerControl _control;

        public PlayerControlSuspendSink(IPlayerControl control) {
            _control = control;
        }

        public void SetSuspended(bool suspended) {
            _control.SetSuspended(suspended);
        }
    }

    // Alt 코드가 쓰는 IPlayerControl 모양의 어댑터. SetSuspended 호출을 허브의 Request/Release(소유자별)로 바꾼다.
    public class SuspendRequestControl : IPlayerControl {
        private readonly SuspendRequestHub _hub;
        private readonly object _owner;

        public SuspendRequestControl(SuspendRequestHub hub, object owner) {
            _hub = hub;
            _owner = owner;
        }

        public void SetSuspended(bool suspended) {
            if (suspended)
                _hub.Request(_owner);
            else
                _hub.Release(_owner);
        }
    }

    // 플레이어에 붙어 SuspendRequestHub를 보관하는 컴포넌트. 줄과 Alt가 같은 허브를 공유해 정지 요청이 합산된다.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(JJBControlGate))]
    public class PlayerSuspendHub : MonoBehaviour {
        private SuspendRequestHub _hub;

        // 처음 접근할 때 JJBControlGate에 연결된 허브를 만든다.
        public SuspendRequestHub Hub =>
            _hub ??= new SuspendRequestHub(new PlayerControlSuspendSink(GetComponent<JJBControlGate>()));

        // 플레이어에 허브가 없으면 붙여서 돌려준다.
        public static PlayerSuspendHub GetOrAdd(GameObject player) {
            return player.TryGetComponent<PlayerSuspendHub>(out var hub) ? hub : player.AddComponent<PlayerSuspendHub>();
        }
    }

    // 플레이어에 붙어 HoldSlot(한 번에 줄 하나만 잡기)을 보관하는 컴포넌트.
    [DisallowMultipleComponent]
    public class PlayerRopeSlot : MonoBehaviour {
        public HoldSlot Slot { get; } = new();

        public static PlayerRopeSlot GetOrAdd(GameObject player) {
            return player.TryGetComponent<PlayerRopeSlot>(out var slot) ? slot : player.AddComponent<PlayerRopeSlot>();
        }
    }
}
