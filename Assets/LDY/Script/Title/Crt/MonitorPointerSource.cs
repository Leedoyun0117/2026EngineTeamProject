using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script {
    // AltModeController.pointerOverride에 연결해서 씀
    public class MonitorPointerSource : MonoBehaviour, IPointerSource {
        [SerializeField] private MonitorSpace space;
        private bool _hasLast;

        private Vector2 _last;

        public Vector2 WorldPosition {
            get {
                var mouse = Mouse.current;
                if (mouse != null) {
                    _last = space.ScreenToContentWorld(mouse.position.ReadValue(), out _);
                    _hasLast = true;
                }
                else if (!_hasLast) {
                    _last = space.ScreenToContentWorld(new Vector2(Screen.width, Screen.height) * 0.5f, out _);
                    _hasLast = true;
                }

                return _last;
            }
        }
    }
}