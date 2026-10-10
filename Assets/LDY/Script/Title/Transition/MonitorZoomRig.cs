using System.Collections;
using UnityEngine;

namespace LDY.Script
{
    // 책상을 비추는 카메라를 모니터 화면이 가득 차도록 이동/확대한다. Alt(timeScale 0) 중에도 진행된다.
    public class MonitorZoomRig : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform screenQuad;
        [SerializeField, Min(0.05f)] private float duration = 1.2f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public IEnumerator Play()
        {
            // 줌 중에 비율 보정이 카메라를 되돌리지 않게 한다. 시작 값은 이 시점의 카메라에서 읽는다.
            if (viewCamera.TryGetComponent(out DeskCameraFit fit))
                fit.enabled = false;

            Transform cam = viewCamera.transform;
            Vector3 startPosition = cam.position;
            float startSize = viewCamera.orthographicSize;

            Vector3 scale = screenQuad.lossyScale;
            Vector3 endPosition = new Vector3(screenQuad.position.x, screenQuad.position.y, startPosition.z);
            float endSize = Mathf.Max(scale.y * 0.5f, scale.x * 0.5f / viewCamera.aspect);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Mathf.Min(Time.unscaledDeltaTime, TitleTuning.MaxDeltaTime);
                float t = ease.Evaluate(Mathf.Clamp01(elapsed / duration));
                cam.position = Vector3.LerpUnclamped(startPosition, endPosition, t);
                viewCamera.orthographicSize = Mathf.LerpUnclamped(startSize, endSize, t);
                yield return null;
            }

            cam.position = endPosition;
            viewCamera.orthographicSize = endSize;
        }
    }
}
