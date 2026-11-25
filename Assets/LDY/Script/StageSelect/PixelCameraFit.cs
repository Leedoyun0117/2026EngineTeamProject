using System;
using LDY.StageSelect.Core;
using UnityEngine;

namespace LDY.StageSelect {
    // Screen Space - Camera 캔버스를 쓰는 선택 화면에서 캔버스 1px = 1/PPU 유닛이 되게 카메라 크기를 맞춘다.
    // 해상도·창 비율이 바뀌어 캔버스 크기가 달라지면 다시 맞추고 Changed로 알린다(발판 재생성).
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-50)]
    public sealed class PixelCameraFit : MonoBehaviour {
        [SerializeField] private RectTransform canvas;
        [SerializeField] [Min(1f)] private float pixelsPerUnit = BoardMetrics.PixelsPerUnit;

        private Camera _camera;
        private Vector2 _fitted;

        public event Action Changed;

        public PixelSpace Space {
            get {
                var size = canvas.rect.size;
                var center = transform.position;
                return new PixelSpace(size.x, size.y, pixelsPerUnit, center.x, center.y);
            }
        }

        public RectTransform Canvas => canvas;

        private void Awake() {
            _camera = GetComponent<Camera>();
            if (canvas == null) {
                Debug.LogError("[StageSelect] PixelCameraFit: canvas 연결 안 됨", this);
                enabled = false;
                return;
            }

            Apply();
        }

        private void LateUpdate() {
            if (canvas.rect.size != _fitted) {
                Apply();
                Changed?.Invoke();
            }
        }

        // 캔버스 크기가 막 바뀐 직후에도 맞게 레이아웃을 먼저 갱신한다.
        public void Apply() {
            UnityEngine.Canvas.ForceUpdateCanvases();
            _fitted = canvas.rect.size;
            _camera.orthographic = true;
            _camera.orthographicSize = Space.OrthographicSize;
        }
    }
}
