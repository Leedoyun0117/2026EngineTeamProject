using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    // unscaled 시간이라 Alt(timeScale 0) 중에도 진행됨
    public class MonitorZoomRig : MonoBehaviour, ICameraRewind
    {
        public enum FitMode
        {
            Fit,  // 목표 영역 전체가 보임(여백 생김)
            Fill  // 화면을 가득 채움(넘치면 잘림)
        }

        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform screenQuad;
        [SerializeField, Min(0.05f)] private float duration = 1.2f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private FitMode fitMode = FitMode.Fit;
        [Tooltip("켜면 CRT 곡률 때문에 검게 가려지는 모서리를 자동으로 제외한다.")]
        [SerializeField] private bool excludeCurvatureCorners = true;
        [Tooltip("CRT 곡률 값을 읽을 드라이버. 비어 있으면 screenQuad에서 찾는다.")]
        [SerializeField] private CrtMaterialDriver crt;
        [Tooltip("유리 테두리 등 추가 여유. 쿼드 전체 크기에 대한 한 변당 비율.")]
        [SerializeField, Range(0f, 0.2f)] private float extraInset = 0.02f;
        [SerializeField] private bool logEndMargins = true;

        private DeskCameraFit _fit;
        private Vector3 _startPosition;
        private float _startSize;
        private bool _zoomed;
        private bool _holding;

        public IEnumerator Play()
        {
            // 줌 중에 DeskCameraFit이 카메라를 되돌리지 않게 끔
            if (viewCamera.TryGetComponent(out DeskCameraFit fit))
                fit.enabled = false;

            Transform cam = viewCamera.transform;
            Vector3 startPosition = cam.position;
            float startSize = viewCamera.orthographicSize;
            _fit = fit;
            _startPosition = startPosition;
            _startSize = startSize;
            _zoomed = true;
            _holding = false;

            yield return UnscaledTween.Run(duration, t =>
            {
                // 창 크기가 바뀌어도 맞게 목표를 매 프레임 다시 계산
                ComputeEnd(startPosition.z, out Vector3 endPosition, out float endSize);
                cam.position = Vector3.LerpUnclamped(startPosition, endPosition, t);
                viewCamera.orthographicSize = Mathf.LerpUnclamped(startSize, endSize, t);
            }, ease);

            ApplyEnd();
            _holding = true;
            LogEndMargins();
        }

        private void LateUpdate()
        {
            if (_holding)
                ApplyEnd();
        }

        // 줌 전 상태로 되돌리고 DeskCameraFit을 다시 켬(줌한 적 없으면 아무것도 안 함)
        public IEnumerator Rewind(float duration)
        {
            if (!_zoomed)
                yield break;

            _holding = false;
            try
            {
                Transform cam = viewCamera.transform;
                Vector3 fromPosition = cam.position;
                float fromSize = viewCamera.orthographicSize;
                yield return UnscaledTween.Run(duration, t =>
                {
                    cam.position = Vector3.LerpUnclamped(fromPosition, _startPosition, t);
                    viewCamera.orthographicSize = Mathf.LerpUnclamped(fromSize, _startSize, t);
                }, ease);

                cam.position = _startPosition;
                viewCamera.orthographicSize = _startSize;
            }
            finally
            {
                if (_fit != null)
                    _fit.enabled = true;
                _zoomed = false;
            }
        }

        private void ApplyEnd()
        {
            ComputeEnd(viewCamera.transform.position.z, out Vector3 position, out float size);
            viewCamera.transform.position = position;
            viewCamera.orthographicSize = size;
        }

        private void ComputeEnd(float cameraZ, out Vector3 position, out float size)
        {
            GetTargetRect(out Vector2 center, out Vector2 half);
            float aspect = viewCamera.aspect;
            float byHeight = half.y;
            float byWidth = half.x / aspect;
            size = fitMode == FitMode.Fit ? Mathf.Max(byHeight, byWidth) : Mathf.Min(byHeight, byWidth);
            position = new Vector3(center.x, center.y, cameraZ);
        }

        private void GetTargetRect(out Vector2 center, out Vector2 half)
        {
            Vector3 scale = screenQuad.lossyScale;
            center = screenQuad.position;
            float normHalf = SafeNormHalf();
            half = new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)) * (0.5f * normHalf);
        }

        private float SafeNormHalf()
        {
            float s = 1f;
            if (excludeCurvatureCorners)
            {
                CrtMaterialDriver driver = crt != null ? crt : screenQuad.GetComponent<CrtMaterialDriver>();
                if (driver != null)
                    s = CornerSafeHalf(driver.CurvatureValue);
            }

            return Mathf.Max(0.05f, s - extraInset * 2f);
        }

        // CrtWarp가 p += (p.x*p.y^2, p.y*p.x^2)*k 라서 모서리가 [-1,1] 안에 들어오려면
        // s*(1 + k*s^2) <= 1. 뉴턴법으로 풂
        private static float CornerSafeHalf(float k)
        {
            if (k <= 0f)
                return 1f;

            float s = 1f;
            for (int i = 0; i < 8; i++)
            {
                float f = s * (1f + k * s * s) - 1f;
                float df = 1f + 3f * k * s * s;
                s -= f / df;
            }

            return Mathf.Clamp(s, 0.05f, 1f);
        }

        private void LogEndMargins()
        {
#if UNITY_EDITOR
            if (!logEndMargins)
                return;

            GetTargetRect(out Vector2 center, out Vector2 half);
            float camH = viewCamera.orthographicSize;
            float camW = camH * viewCamera.aspect;
            Vector3 cp = viewCamera.transform.position;
            // 양수는 여백, 음수는 잘린 양(월드 단위, 한쪽 기준)
            float sideX = camW - half.x - Mathf.Abs(cp.x - center.x);
            float sideY = camH - half.y - Mathf.Abs(cp.y - center.y);
            Debug.Log($"[MonitorZoomRig] mode={fitMode} aspect={viewCamera.aspect:F4} size={camH:F4} " +
                      $"target half=({half.x:F3},{half.y:F3}) " +
                      $"좌우 여백(+)/잘림(-)={sideX:F4} ({sideX / (camW * 2f) * 100f:F2}% of view) " +
                      $"상하 여백(+)/잘림(-)={sideY:F4} ({sideY / (camH * 2f) * 100f:F2}% of view)");
#endif
        }

#if UNITY_EDITOR
        // 목표 영역(초록), 카메라 영역(노랑), 쿼드 전체(회색)
        private void OnDrawGizmos()
        {
            if (screenQuad == null || viewCamera == null)
                return;

            Vector3 scale = screenQuad.lossyScale;
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(screenQuad.position, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 0f));

            GetTargetRect(out Vector2 center, out Vector2 half);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, new Vector3(half.x * 2f, half.y * 2f, 0f));

            Vector3 cp = viewCamera.transform.position;
            float camH = viewCamera.orthographicSize;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(cp.x, cp.y, 0f), new Vector3(camH * 2f * viewCamera.aspect, camH * 2f, 0f));
        }
#endif
    }
}
