namespace LDY.StageSelect.Core {
    // 파일 목록 칸 배치(파일 목록 패널 왼쪽 위 기준 픽셀). 뷰와 발판 계산이 같은 값을 쓴다.
    // 세로 값은 플레이어 점프(TitlePlayerTuning 기준 약 3.17유닛 = 114px @ 36PPU)로 닿게 잡았다.
    //   줄 간격 RowPitch = 98px, 패널 바닥 → 아랫줄 썸네일 윗변 = 97px (패널 높이 241px 기준)
    //   윗줄에 서도 머리(36px)가 패널 천장에 닿지 않게 PaddingTop을 둔다.
    public static class FileGridLayout {
        public const int Columns = 4;
        public const float PaddingLeft = 8f;
        public const float PaddingTop = 44f;
        public const float SpacingX = 8f;
        public const float SpacingY = 10f;
        public const float ItemWidth = 104f;
        public const float ItemHeight = 88f;

        // 칸 안의 썸네일 틀. 윗변이 발판이 된다.
        public static readonly PixelRect Thumbnail = new PixelRect(4f, 2f, 96f, 56f);

        public static float RowPitch => ItemHeight + SpacingY;

        public static PixelRect Item(int index) =>
            new PixelRect(PaddingLeft + index % Columns * (ItemWidth + SpacingX),
                PaddingTop + index / Columns * RowPitch, ItemWidth, ItemHeight);

        public static PixelRect ThumbnailOf(int index) {
            var item = Item(index);
            return Thumbnail.Offset(item.X, item.Y);
        }
    }
}
