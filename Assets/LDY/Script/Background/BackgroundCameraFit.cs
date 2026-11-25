using LDY.Background.Core;
using UnityEngine;

namespace LDY.Background {
    public sealed class BackgroundCameraFit : MonoBehaviour {
        [SerializeField] private Camera target;

        private void LateUpdate() {
            if (target == null) target = Camera.main;
            if (target == null || !target.orthographic) return;

            var cameraPosition = target.transform.position;
            transform.position = new Vector3(cameraPosition.x, cameraPosition.y, transform.position.z);
            var scale = (float)BackgroundFit.CoverScale(target.orthographicSize, target.aspect);
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
