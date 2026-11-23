using UnityEngine;

namespace LDY.Spider {
    public interface ISeverable {
        void Sever();
    }

    public interface IPlayerLocator {
        bool TryGetX(out float x);
    }

    public interface IPlayerKiller {
        void Kill(GameObject player);
    }

    public enum DeathCause {
        Needle,
        Trap
    }

    // 연출을 보여 준 뒤 사망 처리를 하는 통로. 구현은 플레이어 쪽에 둔다.
    public interface IDeathPresenter {
        bool Begin(DeathCause cause, Vector2 anchor);
    }
}
