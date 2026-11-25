using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.Script {
    // 팀원 프리팹을 고치지 않도록 씬이 열릴 때 씬에 놓인 플레이어 한 명에게만 붙인다.
    // 프리팹에 굽지 않으므로 런타임 복사 플레이어(HTY)는 파괴돼도 재시작하지 않는다.
    internal static class PlayerDeathInstaller {
        private const string PlayerTag = "Player";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register() {
            SceneManager.sceneLoaded -= Attach;
            SceneManager.sceneLoaded += Attach;
        }

        private static void Attach(Scene scene, LoadSceneMode mode) {
            var player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player != null && !player.TryGetComponent<PlayerDeath>(out _))
                player.AddComponent<PlayerDeath>();
        }
    }
}
