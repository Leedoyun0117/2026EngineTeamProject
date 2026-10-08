using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script
{
    // AltModeController.pointerOverride에 연결한다. 마우스를 모니터 콘텐츠 월드 좌표로 바꿔 Alt 커서로 쓴다.
    public class MonitorPointerSource : MonoBehaviour, IPointerSource
    {
        [SerializeField] private MonitorSpace space;

        private Vector2 _last;
        private bool _hasLast;

        public Vector2 WorldPosition
        {
            get
            {
                Mouse mouse = Mouse.current;
                if (mouse != null)
                {
                    _last = space.ScreenToContentWorld(mouse.position.ReadValue(), out _);
                    _hasLast = true;
                }
                else if (!_hasLast)
                {
                    _last = space.ScreenToContentWorld(new Vector2(Screen.width, Screen.height) * 0.5f, out _);
                    _hasLast = true;
                }

                return _last;
            }
        }
    }
}
