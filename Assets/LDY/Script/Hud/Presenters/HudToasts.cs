using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud {
    // 상단 중앙 토스트. 다른 프레젠터와 게임 코드는 Push만 쓴다.
    public sealed class HudToasts : MonoBehaviour, IHudTicker {
        [SerializeField] private HudSkin skin;
        [SerializeField] private ToastView view;

        private ToastQueue _queue;
        private int _shownVersion = -1;

        private ToastQueue Queue => _queue ??= new ToastQueue(skin != null ? skin.toastMaxWaiting : 3);

        public void Push(ToastKind kind, string message) {
            Queue.Push(kind, message, skin != null ? skin.toastSeconds : 2.5f);
        }

        public void Tick(float deltaTime) {
            Queue.Tick(deltaTime);
            if (Queue.Version == _shownVersion || view == null)
                return;

            _shownVersion = Queue.Version;
            var current = Queue.Current;
            if (current == null)
                view.Hide();
            else
                view.Show(skin != null ? skin.ToastIcon(current.Kind) : null, current.Message);
        }

        private void Awake() {
            if (view != null)
                view.Hide();
        }
    }
}
