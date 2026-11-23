using System;
using UnityEngine;

namespace LDY.Spider {
    // 플레이어 쪽 어셈블리가 연출 컴포넌트를 만드는 방법을 등록하는 자리.
    public static class DeathPresenterProvider {
        public static Func<GameObject, IDeathPresenter> Create { get; set; }
    }

    // 플레이어에게 연출 통로가 있으면 그것을, 없으면 만들어 붙이고, 그것도 안 되면 기존 KillHitbox와 같은 즉시 파괴를 쓴다.
    public sealed class PresentedDeath {
        private readonly IPlayerKiller _fallback;

        public PresentedDeath(IPlayerKiller fallback) {
            _fallback = fallback;
        }

        public void Request(GameObject player, DeathCause cause, Vector2 anchor) {
            if (player == null)
                return;

            var presenter = player.GetComponentInParent<IDeathPresenter>() ?? DeathPresenterProvider.Create?.Invoke(player);
            if (presenter != null) {
                presenter.Begin(cause, anchor);
                return;
            }

            Debug.LogWarning("[Spider] 플레이어에 IDeathPresenter가 없어 연출 없이 바로 죽인다", player);
            _fallback.Kill(player);
        }
    }
}
