using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script
{
    // 화면 영역 안으로 클램프한 마우스의 월드 좌표.
    public class MouseWorldPointer : IPointerSource
    {
        private readonly Camera _camera;
        private readonly Transform _depthReference;
        private Vector2 _last;

        public MouseWorldPointer(Camera camera, Transform depthReference)
        {
            _camera = camera;
            _depthReference = depthReference;
            _last = depthReference.position;
        }

        public Vector2 WorldPosition
        {
            get
            {
                Mouse mouse = Mouse.current;
                if (mouse == null)
                    return _last;

                Rect rect = _camera.pixelRect;
                Vector2 screen = mouse.position.ReadValue();
                screen.x = Mathf.Clamp(screen.x, rect.xMin, rect.xMax);
                screen.y = Mathf.Clamp(screen.y, rect.yMin, rect.yMax);

                float depth = _camera.WorldToScreenPoint(_depthReference.position).z;
                _last = _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
                return _last;
            }
        }
    }
}
