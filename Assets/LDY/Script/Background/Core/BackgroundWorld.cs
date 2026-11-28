namespace LDY.Background.Core {
    public static class BackgroundWorld {
        private const string StagePrefix = "Stage";

        public static bool TryFromStageId(string stageId, out int world) => TryLeadingNumber(stageId, 0, out world);

        public static bool TryFromAssetName(string assetName, out int world) {
            world = 0;
            return assetName != null && assetName.StartsWith(StagePrefix) &&
                   TryLeadingNumber(assetName, StagePrefix.Length, out world);
        }

        private static bool TryLeadingNumber(string text, int start, out int number) {
            number = 0;
            if (string.IsNullOrEmpty(text)) return false;
            var end = start;
            while (end < text.Length && char.IsDigit(text[end])) end++;
            return end > start && int.TryParse(text.Substring(start, end - start), out number) && number > 0;
        }
    }
}
