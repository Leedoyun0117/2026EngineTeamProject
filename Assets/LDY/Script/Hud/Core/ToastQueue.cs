using System.Collections.Generic;

namespace LDY.Hud.Core {
    public enum ToastKind {
        Info,
        Warning,
        Error
    }

    public sealed class Toast {
        public ToastKind Kind { get; internal set; }
        public string Message { get; }
        public float Remaining { get; internal set; }

        internal Toast(ToastKind kind, string message, float seconds) {
            Kind = kind;
            Message = message;
            Remaining = seconds;
        }
    }

    // 표시 1개 + 대기열. 같은 문구는 새로 쌓지 않고 남은 시간만 갱신, 대기가 꽉 차면 가장 오래된 대기를 버린다.
    public sealed class ToastQueue {
        private readonly int _maxWaiting;
        private readonly List<Toast> _waiting = new List<Toast>();

        public ToastQueue(int maxWaiting) {
            _maxWaiting = maxWaiting < 0 ? 0 : maxWaiting;
        }

        public Toast Current { get; private set; }
        public IReadOnlyList<Toast> Waiting => _waiting;
        public int Version { get; private set; }

        public void Push(ToastKind kind, string message, float seconds) {
            if (string.IsNullOrEmpty(message))
                return;

            if (Current != null && Current.Message == message) {
                Current.Remaining = seconds;
                Current.Kind = kind;
                Version++;
                return;
            }

            var waiting = _waiting.Find(t => t.Message == message);
            if (waiting != null) {
                waiting.Remaining = seconds;
                waiting.Kind = kind;
                return;
            }

            var toast = new Toast(kind, message, seconds);
            if (Current == null) {
                Show(toast);
                return;
            }

            if (_maxWaiting == 0)
                return;
            if (_waiting.Count >= _maxWaiting)
                _waiting.RemoveAt(0);
            _waiting.Add(toast);
        }

        public void Tick(float deltaTime) {
            if (Current == null)
                return;

            Current.Remaining -= deltaTime;
            if (Current.Remaining > 0f)
                return;

            if (_waiting.Count == 0) {
                Show(null);
                return;
            }

            var next = _waiting[0];
            _waiting.RemoveAt(0);
            Show(next);
        }

        public void Clear() {
            _waiting.Clear();
            if (Current != null)
                Show(null);
        }

        private void Show(Toast toast) {
            Current = toast;
            Version++;
        }
    }
}
