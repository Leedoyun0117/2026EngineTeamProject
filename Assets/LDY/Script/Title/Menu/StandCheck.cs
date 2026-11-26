using UnityEngine;

namespace LDY.Script {
    // "발판 위에 서 있다" 판정. 타이틀 아이콘(MenuIcon)이 쓴다.
    public static class StandCheck {
        public static bool IsStandingOn(Collider2D body, Collider2D platform) {
            if (platform == null || !platform.IsTouching(body))
                return false;

            return body.bounds.min.y >= platform.bounds.max.y - TitleTuning.StandTolerance;
        }
    }
}
