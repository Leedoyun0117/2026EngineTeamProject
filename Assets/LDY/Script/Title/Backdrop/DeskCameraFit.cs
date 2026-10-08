using UnityEngine;

namespace LDY.Script
{
    // 화면 비율이 바뀌어도 방 배경 바깥이 보이지 않도록 메인 카메라 크기를 맞춘다.
    // 16:9는 배경이 화면에 딱 맞고, 그보다 좁으면 좌우가, 넓으면 위아래가 잘린다. 줌 연출이 시작되면 꺼진다.
    [RequireComponent(typeof(Camera))]
    public class DeskCameraFit : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer background;

        private Camera _camera;
        private float _lastAspect;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            Fit();
        }

        private void Update()
        {
            if (!Mathf.Approximately(_camera.aspect, _lastAspect))
                Fit();
        }

        private void Fit()
        {
            _lastAspect = _camera.aspect;
            Bounds bounds = background.bounds;
            _camera.orthographicSize = Mathf.Min(bounds.extents.y, bounds.extents.x / _camera.aspect);
            _camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, _camera.transform.position.z);
        }
    }
}
