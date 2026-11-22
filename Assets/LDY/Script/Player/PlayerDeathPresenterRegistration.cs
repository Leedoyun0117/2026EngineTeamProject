using LDY.Spider;
using UnityEngine;

namespace LDY.Script {
    // 프리팹에 컴포넌트가 없는 플레이어도 죽는 순간 연출 컴포넌트를 붙여 쓸 수 있게 한다.
    internal static class PlayerDeathPresenterRegistration {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register() {
            DeathPresenterProvider.Create = player => player.GetComponentInChildren<PlayerRenderer>() != null
                ? player.AddComponent<PlayerDeathPresenter>()
                : null;
        }
    }
}
