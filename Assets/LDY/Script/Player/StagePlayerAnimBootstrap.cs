using UnityEngine;

namespace LDY.Script {
    // 스테이지 플레이어용 조립. 루트 프리팹을 건드리지 않도록 필요한 어댑터를 런타임에 붙인다.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(PlayerRenderer))]
    public class StagePlayerAnimBootstrap : MonoBehaviour {
        private void Awake() {
            var root = transform.parent != null ? transform.parent.gameObject : null;
            if (root == null) {
                Debug.LogError("[StagePlayerAnimBootstrap] 부모(플레이어 루트)가 없음", this);
                enabled = false;
                return;
            }

            var source = GetOrAdd<JJBPlayerMotionSource>(root);
            var pose = GetOrAdd<PlayerPoseRelay>(root);
            GetComponent<PlayerRenderer>().Initialize(source, null, pose, pose, pose);
        }

        private static T GetOrAdd<T>(GameObject host) where T : Component {
            return host.TryGetComponent<T>(out var existing) ? existing : host.AddComponent<T>();
        }
    }
}
