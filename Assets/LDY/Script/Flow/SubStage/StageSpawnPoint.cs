using UnityEngine;

namespace LDY.Flow {
    // 다중 스테이지 씬에서 이 루트의 시작 지점. 이름 규칙(Start/Spawn/StartMarker)보다 우선한다.
    public sealed class StageSpawnPoint : MonoBehaviour {
        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
}
