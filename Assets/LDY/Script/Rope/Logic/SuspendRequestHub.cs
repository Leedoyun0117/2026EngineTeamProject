using System.Collections.Generic;

namespace LDY.Script {
    public interface ISuspendSink {
        void SetSuspended(bool suspended);
    }

    public sealed class SuspendRequestHub {
        private readonly HashSet<object> _owners = new();
        private readonly ISuspendSink _sink;

        public SuspendRequestHub(ISuspendSink sink) {
            _sink = sink;
        }

        public bool IsSuspended => _owners.Count > 0;

        public void Request(object owner) {
            if (owner == null || !_owners.Add(owner))
                return;

            if (_owners.Count == 1)
                _sink.SetSuspended(true);
        }

        public void Release(object owner) {
            if (owner == null || !_owners.Remove(owner))
                return;

            if (_owners.Count == 0)
                _sink.SetSuspended(false);
        }
    }
}
