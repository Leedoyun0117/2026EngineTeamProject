using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script
{
    // 화면 픽셀 -> 모니터 쿼드 UV -> 곡률 왜곡 -> 콘텐츠 카메라 월드 좌표.
    // 쿼드는 -0.5~0.5 로컬 범위의 기본 Quad 메시여야 한다.
    public class MonitorSpace : MonoBehaviour, IMonitorSpace
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Camera contentCamera;
        [SerializeField] private Transform screenQuad;
        [SerializeField] private CrtMaterialDriver crt;

        public bool PointerInside
        {
            get
            {
                Mouse mouse = Mouse.current;
                if (mouse == null)
                    return false;

                ScreenToContentWorld(mouse.position.ReadValue(), out bool inside);
                return inside;
            }
        }

        public Vector2 ScreenToContentWorld(Vector2 screenPosition, out bool inside)
        {
            float depth = viewCamera.WorldToScreenPoint(screenQuad.position).z;
            Vector3 world = viewCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
            Vector3 local = screenQuad.InverseTransformPoint(world);

            Vector2 content = CrtCurvature.Warp(new Vector2(local.x + 0.5f, local.y + 0.5f), crt.CurvatureValue);
            inside = content.x >= 0f && content.x <= 1f && content.y >= 0f && content.y <= 1f;
            content.x = Mathf.Clamp01(content.x);
            content.y = Mathf.Clamp01(content.y);

            return contentCamera.ViewportToWorldPoint(new Vector3(content.x, content.y, contentCamera.nearClipPlane));
        }
    }
}
