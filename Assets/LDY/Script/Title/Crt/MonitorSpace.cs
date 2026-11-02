using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Script {
    // screenQuad는 -0.5~0.5 로컬 범위의 기본 Quad여야 함
    public class MonitorSpace : MonoBehaviour, IMonitorSpace {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Camera contentCamera;
        [SerializeField] private Transform screenQuad;
        [SerializeField] private CrtMaterialDriver crt;

        public bool PointerInside {
            get {
                var mouse = Mouse.current;
                if (mouse == null)
                    return false;

                ScreenToContentWorld(mouse.position.ReadValue(), out var inside);
                return inside;
            }
        }

        public Vector2 ScreenToContentWorld(Vector2 screenPosition, out bool inside) {
            var depth = viewCamera.WorldToScreenPoint(screenQuad.position).z;
            var world = viewCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
            var local = screenQuad.InverseTransformPoint(world);

            var content = CrtCurvature.Warp(new Vector2(local.x + 0.5f, local.y + 0.5f), crt.CurvatureValue);
            inside = content.x >= 0f && content.x <= 1f && content.y >= 0f && content.y <= 1f;
            content.x = Mathf.Clamp01(content.x);
            content.y = Mathf.Clamp01(content.y);

            return contentCamera.ViewportToWorldPoint(new Vector3(content.x, content.y, contentCamera.nearClipPlane));
        }
    }
}