namespace LDY.Stage {
    public static class PlatformTileNamer {
        public static readonly string[] Names = { "platform_L", "platform_C", "platform_R", "platform_single" };

        public static string Name(bool left, bool right) {
            return Names[GroundTileNamer.HorizontalIndex(left, right)];
        }
    }
}
