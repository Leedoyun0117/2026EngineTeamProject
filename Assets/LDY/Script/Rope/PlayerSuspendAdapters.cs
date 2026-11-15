using UnityEngine;

namespace LDY.Script {
    public class PlayerControlSuspendSink : ISuspendSink {
        private readonly IPlayerControl _control;

        public PlayerControlSuspendSink(IPlayerControl control) {
            _control = control;
        }

        public void SetSuspended(bool suspended) {
            _control.SetSuspended(suspended);
        }
    }

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

    [DisallowMultipleComponent]
    [RequireComponent(typeof(JJBControlGate))]
    public class PlayerSuspendHub : MonoBehaviour {
        private SuspendRequestHub _hub;

        public SuspendRequestHub Hub =>
            _hub ??= new SuspendRequestHub(new PlayerControlSuspendSink(GetComponent<JJBControlGate>()));

        public static PlayerSuspendHub GetOrAdd(GameObject player) {
            return player.TryGetComponent<PlayerSuspendHub>(out var hub) ? hub : player.AddComponent<PlayerSuspendHub>();
        }
    }

    [DisallowMultipleComponent]
    public class PlayerRopeSlot : MonoBehaviour {
        public HoldSlot Slot { get; } = new();

        public static PlayerRopeSlot GetOrAdd(GameObject player) {
            return player.TryGetComponent<PlayerRopeSlot>(out var slot) ? slot : player.AddComponent<PlayerRopeSlot>();
        }
    }
}
