using UnityEngine;

namespace LDY.Spider {
    // 기존 KillHitbox와 같은 방식(플레이어 오브젝트 파괴)으로 죽인다.
    public sealed class DestroyPlayerKiller : IPlayerKiller {
        public void Kill(GameObject player) => Object.Destroy(player);
    }
}
