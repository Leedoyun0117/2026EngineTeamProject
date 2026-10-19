using UnityEngine;

namespace LDY.Script
{
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
