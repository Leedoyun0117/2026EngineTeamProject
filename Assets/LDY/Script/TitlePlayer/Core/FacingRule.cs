namespace LDY.TitlePlayer.Core {
    public static class FacingRule {
        // 데드존 안이면 이전 방향을 유지한다. 1은 오른쪽, -1은 왼쪽.
        public static int Next(float velocityX, float deadZone, int previous) {
            if (velocityX > deadZone) return 1;
            if (velocityX < -deadZone) return -1;
            return previous;
        }
    }
}
