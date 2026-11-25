namespace LDY.StageSelect.Core {
    // 선택 화면에서 걷는 플레이어의 월드 ↔ UI 대응.
    // 기준 해상도 640x360(UiFactory) 캔버스 1px = 1/36 유닛 → 16:9에서 카메라 orthographicSize 5.
    // 1920x1080이면 캔버스 1px = 화면 3px, 1유닛 = 화면 108px.
    // 플레이어 크기(약 1.05 x 1.275유닛 = 38 x 46px)가 타이틀 화면에서 화면 높이 대비 비율(약 12%)과 비슷하다.
    public static class BoardMetrics {
        public const float PixelsPerUnit = 36f;

        // 벽·바닥·천장 두께(px). 플레이어가 빠르게 부딪혀도 뚫지 않을 만큼.
        public const float WallThickness = 16f;
    }
}
