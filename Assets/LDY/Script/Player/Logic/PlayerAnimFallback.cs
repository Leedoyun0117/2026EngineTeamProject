using System.Collections.Generic;

namespace LDY.Script {
    // 세트에 없는 ID가 넘어갈 대체 ID와, ID 키로 클립을 찾는 규칙.
    public static class PlayerAnimFallback {
        private const int MaxDepth = 4;

        public static bool TryNext(PlayerAnimId id, out PlayerAnimId next) {
            switch (id) {
                case PlayerAnimId.Land:
                    next = PlayerAnimId.Idle;
                    return true;
                case PlayerAnimId.Push:
                    next = PlayerAnimId.Walk;
                    return true;
            }

            next = RopePose.FallbackFor(id);
            return next != id;
        }

        public static bool TryResolve<T>(IReadOnlyDictionary<PlayerAnimId, T> clips, PlayerAnimId id, out T clip) {
            var current = id;
            for (var i = 0; i < MaxDepth; i++) {
                if (clips.TryGetValue(current, out clip))
                    return true;

                if (!TryNext(current, out current))
                    break;
            }

            clip = default;
            return false;
        }
    }
}
