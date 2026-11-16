namespace LDY.Script {
    // 루트가 Y축 180도로 좌우를 처리하면 자식의 Z 회전과 로컬 좌표가 월드에서 거울상이 된다. 그 보정만 맡는다.
    public static class GripFacing {
        public static bool IsRootMirrored(float rightAxisWorldX) {
            return rightAxisWorldX < 0f;
        }

        // 월드 기준 발 위치 계산용. 스프라이트 flipX와 루트 거울상은 서로 상쇄된다.
        public static bool IsWorldFlipped(bool spriteFlipX, bool rootMirrored) {
            return spriteFlipX != rootMirrored;
        }

        // 월드에서 시계 반대 방향 radians 만큼 돌리려면 거울상 부모 아래에서는 부호를 뒤집어야 한다.
        public static float LocalAngle(float worldRadians, bool rootMirrored) {
            return rootMirrored ? -worldRadians : worldRadians;
        }
    }
}
