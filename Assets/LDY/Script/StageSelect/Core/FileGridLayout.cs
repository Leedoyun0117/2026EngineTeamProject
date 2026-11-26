namespace LDY.StageSelect.Core {
    // 파일 목록 칸 배치(파일 목록 패널 왼쪽 위 기준 픽셀). 그리드 뷰와 칸 뷰가 같은 값을 쓴다.
    public static class FileGridLayout {
        public const int Columns = 4;
        public const float PaddingLeft = 8f;
        public const float PaddingTop = 8f;
        public const float SpacingX = 8f;
        public const float SpacingY = 6f;
        public const float ItemWidth = 104f;
        public const float ItemHeight = 88f;

        // 칸 안의 썸네일 틀.
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
