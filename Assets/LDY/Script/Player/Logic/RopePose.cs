namespace LDY.Script {
    public static class RopePose {
        public static PlayerAnimId For(RowMoveDirection rowMove) {
            switch (rowMove) {
                case RowMoveDirection.Up:
                    return PlayerAnimId.RopeClimb;
                case RowMoveDirection.Down:
                    return PlayerAnimId.RopeClimbDown;
                default:
                    return PlayerAnimId.RopeHang;
            }
        }

        public static bool IsRope(PlayerAnimId id) {
            return id == PlayerAnimId.RopeHang || id == PlayerAnimId.RopeClimb || id == PlayerAnimId.RopeClimbDown;
        }

        // 밧줄 클립이 없는 세트는 밧줄 구분이 생기기 전과 같이 Grab 포즈를 쓴다.
        public static PlayerAnimId FallbackFor(PlayerAnimId id) {
            return IsRope(id) ? PlayerAnimId.Grab : id;
        }
    }
}
