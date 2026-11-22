using UnityEngine;

namespace LDY.Stage {
    // 회전하는 부모 아래에 있어도 월드 회전을 고정한다. 천장에 고정된 장식에 쓴다.
    public sealed class KeepWorldRotation : MonoBehaviour {
        [SerializeField] private Vector3 eulerAngles;

        private void OnEnable() {
            Apply();
        }

        private void LateUpdate() {
            Apply();
        }

        private void Apply() {
            transform.rotation = Quaternion.Euler(eulerAngles);
        }
    }
}
