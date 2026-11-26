using System.Collections;
using LDY.Title.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace LDY.Script {
    // unscaled 시간이라 Alt(timeScale 0) 중에도 진행됨
    // 끝 크기와 가림막 계산은 ZoomFraming(순수 함수)이 맡고, 여기서는 카메라와 가림막에 적용만 한다
    public class MonitorZoomRig : MonoBehaviour, ICameraRewind {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform screenQuad;
        [SerializeField] [Min(0.05f)] private float duration = 1.2f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Pillarbox: 화면 높이(또는 폭)에 맞춰 멈추고 쿼드 바깥을 가림막으로 가림. Cover: 쿼드가 화면을 꽉 채우게 더 확대(넘치는 쪽 잘림)")]
        [FormerlySerializedAs("fitMode")]
        [SerializeField]
        private ZoomEndFit endFit = ZoomEndFit.Pillarbox;

        [Tooltip("켜면 CRT 곡률 때문에 검게 가려지는 모서리를 자동으로 제외한다.")] [SerializeField]
        private bool excludeCurvatureCorners = true;

        [Tooltip("CRT 곡률 값을 읽을 드라이버. 비어 있으면 screenQuad에서 찾는다.")] [SerializeField]
        private CrtMaterialDriver crt;

        [Tooltip("유리 테두리 등 추가 여유. 쿼드 전체 크기에 대한 한 변당 비율.")] [SerializeField] [Range(0f, 0.2f)]
        private float extraInset = 0.02f;

        [Header("가림막 (Pillarbox)")] [Tooltip("확대 진행률(곡선 적용 후)이 이 값부터 1까지 가림막 알파 0→1")] [SerializeField] [Range(0f, 1f)]
        private float maskFadeStart = 0.7f;

        [SerializeField] private Color maskColor = Color.black;

        [Tooltip("쿼드와 같은 정렬 레이어 안의 순서. 모니터 프레임·책상 소품보다 커야 함(쿼드와는 겹치지 않음)")] [SerializeField]
        private int maskSortingOrder = 100;

        [SerializeField] private bool logEndMargins = true;

        private DeskCameraFit _fit;
        private bool _holding;
        private ZoomMasks _masks;
        private Vector3 _startPosition;
        private float _startSize;
        private bool _zoomed;

        private void LateUpdate() {
            if (_holding)
                ApplyEnd();
        }

        private void OnDestroy() {
            _masks?.Release();
        }

#if UNITY_EDITOR
        // 목표 영역(초록), 카메라 영역(노랑), 쿼드 전체(회색)
        private void OnDrawGizmos() {
            if (screenQuad == null || viewCamera == null)
                return;

            var quad = QuadRect();
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(quad.center, quad.size);

            GetTargetRect(out var center, out var half);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, new Vector3(half.x * 2f, half.y * 2f, 0f));

            var cp = viewCamera.transform.position;
            var camH = viewCamera.orthographicSize;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(cp.x, cp.y, 0f), new Vector3(camH * 2f * viewCamera.aspect, camH * 2f, 0f));
        }
#endif

        // 줌 전 상태로 되돌리고 DeskCameraFit을 다시 켬(줌한 적 없으면 아무것도 안 함)
        public IEnumerator Rewind(float duration) {
            if (!_zoomed)
                yield break;

            _holding = false;
            _masks?.Hide();
            try {
                var cam = viewCamera.transform;
                var fromPosition = cam.position;
                var fromSize = viewCamera.orthographicSize;
                yield return UnscaledTween.Run(duration, t => {
                    cam.position = Vector3.LerpUnclamped(fromPosition, _startPosition, t);
                    viewCamera.orthographicSize = Mathf.LerpUnclamped(fromSize, _startSize, t);
                }, ease);

                cam.position = _startPosition;
                viewCamera.orthographicSize = _startSize;
            }
            finally {
                if (_fit != null)
                    _fit.enabled = true;
                _zoomed = false;
            }
        }

        public IEnumerator Play() {
            // 줌 중에 DeskCameraFit이 카메라를 되돌리지 않게 끔
            if (viewCamera.TryGetComponent(out DeskCameraFit fit))
                fit.enabled = false;

            var cam = viewCamera.transform;
            var startPosition = cam.position;
            var startSize = viewCamera.orthographicSize;
            _fit = fit;
            _startPosition = startPosition;
            _startSize = startSize;
            _zoomed = true;
            _holding = false;

            yield return UnscaledTween.Run(duration, t => {
                // 창 크기가 바뀌어도 맞게 목표를 매 프레임 다시 계산
                var end = ComputeEnd();
                cam.position = Vector3.LerpUnclamped(startPosition, EndPosition(end, startPosition.z), t);
                viewCamera.orthographicSize = Mathf.LerpUnclamped(startSize, end.OrthoSize, t);
                UpdateMasks(t);
            }, ease);

            ApplyEnd();
            _holding = true;
            LogEndMargins();
        }

        private void ApplyEnd() {
            var end = ComputeEnd();
            viewCamera.transform.position = EndPosition(end, viewCamera.transform.position.z);
            viewCamera.orthographicSize = end.OrthoSize;
            UpdateMasks(1f);
        }

        private ZoomEndFrame ComputeEnd() {
            GetTargetRect(out var center, out var half);
            return ZoomFraming.End(center, half, QuadRect(), viewCamera.aspect, endFit);
        }

        private static Vector3 EndPosition(ZoomEndFrame end, float cameraZ) {
            var center = end.View.center;
            return new Vector3(center.x, center.y, cameraZ);
        }

        // 지금 카메라 시야 기준으로 가림막을 다시 잡아서 확대 도중에도 쿼드 테두리에 붙어 있게 함
        private void UpdateMasks(float progress) {
            if (endFit != ZoomEndFit.Pillarbox) {
                _masks?.Hide();
                return;
            }

            var alpha = ZoomFraming.MaskAlpha(progress, maskFadeStart);
            if (alpha <= 0f) {
                _masks?.Hide();
                return;
            }

            var position = viewCamera.transform.position;
            var view = ZoomFraming.View(position, viewCamera.orthographicSize, viewCamera.aspect);
            // 바깥쪽 가장자리만 조금 넓혀 화면 끝 반 픽셀 틈을 막음(쿼드 쪽 경계는 그대로)
            var bleed = view.height * 0.01f;
            view = Rect.MinMaxRect(view.xMin - bleed, view.yMin - bleed, view.xMax + bleed, view.yMax + bleed);
            Masks().Show(ZoomFraming.Masks(view, QuadRect()), alpha);
        }

        private ZoomMasks Masks() {
            if (_masks != null)
                return _masks;

            var quadRenderer = screenQuad.GetComponent<Renderer>();
            var sortingLayer = quadRenderer != null ? quadRenderer.sortingLayerID : 0;
            _masks = new ZoomMasks(screenQuad.gameObject.layer, sortingLayer, maskSortingOrder, maskColor);
            return _masks;
        }

        // screenQuad는 -0.5~0.5 로컬 범위의 기본 Quad
        private Rect QuadRect() {
            var scale = screenQuad.lossyScale;
            var size = new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            return new Rect((Vector2)screenQuad.position - size * 0.5f, size);
        }

        private void GetTargetRect(out Vector2 center, out Vector2 half) {
            var quad = QuadRect();
            center = quad.center;
            half = quad.size * (0.5f * SafeNormHalf());
        }

        private float SafeNormHalf() {
            var s = 1f;
            if (excludeCurvatureCorners) {
                var driver = crt != null ? crt : screenQuad.GetComponent<CrtMaterialDriver>();
                if (driver != null)
                    s = CornerSafeHalf(driver.CurvatureValue);
            }

            return Mathf.Max(0.05f, s - extraInset * 2f);
        }

        // CrtWarp가 p += (p.x*p.y^2, p.y*p.x^2)*k 라서 모서리가 [-1,1] 안에 들어오려면
        // s*(1 + k*s^2) <= 1. 뉴턴법으로 풂
        private static float CornerSafeHalf(float k) {
            if (k <= 0f)
                return 1f;

            var s = 1f;
            for (var i = 0; i < 8; i++) {
                var f = s * (1f + k * s * s) - 1f;
                var df = 1f + 3f * k * s * s;
                s -= f / df;
            }

            return Mathf.Clamp(s, 0.05f, 1f);
        }

        private void LogEndMargins() {
#if UNITY_EDITOR
            if (!logEndMargins)
                return;

            GetTargetRect(out var center, out var half);
            var camH = viewCamera.orthographicSize;
            var camW = camH * viewCamera.aspect;
            var cp = viewCamera.transform.position;
            // 양수는 여백, 음수는 잘린 양(월드 단위, 한쪽 기준)
            var sideX = camW - half.x - Mathf.Abs(cp.x - center.x);
            var sideY = camH - half.y - Mathf.Abs(cp.y - center.y);
            Debug.Log($"[MonitorZoomRig] mode={endFit} aspect={viewCamera.aspect:F4} size={camH:F4} " +
                      $"target half=({half.x:F3},{half.y:F3}) " +
                      $"좌우 여백(+)/잘림(-)={sideX:F4} ({sideX / (camW * 2f) * 100f:F2}% of view) " +
                      $"상하 여백(+)/잘림(-)={sideY:F4} ({sideY / (camH * 2f) * 100f:F2}% of view)");
#endif
        }
    }
}
