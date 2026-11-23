namespace LDY.PlayerMerge {
    public readonly struct OpaqueRows {
        public OpaqueRows(int lowest, int highest) {
            Lowest = lowest;
            Highest = highest;
        }

        public int Lowest { get; }
        public int Highest { get; }
        public bool IsEmpty => Lowest < 0;
        public int Height => IsEmpty ? 0 : Highest - Lowest + 1;
    }

    public static class FeetAlignment {
        public static OpaqueRows FindOpaqueRows(byte[] alpha, int width, int height) {
            var lowest = -1;
            var highest = -1;
            for (var row = 0; row < height; row++)
                for (var column = 0; column < width; column++) {
                    if (alpha[row * width + column] == 0)
                        continue;

                    if (lowest < 0)
                        lowest = row;
                    highest = row;
                    break;
                }

            return new OpaqueRows(lowest, highest);
        }

        public static float ColliderBottom(float offsetY, float sizeY) {
            return offsetY - sizeY * 0.5f;
        }

        public static float PixelsToUnits(float pixels, float pixelsPerUnit) {
            return pixels / pixelsPerUnit;
        }

        public static float VisualLocalY(float colliderBottom, float pivotPixelY, int lowestRow, float pixelsPerUnit) {
            return colliderBottom - PixelsToUnits(lowestRow - pivotPixelY, pixelsPerUnit);
        }
    }
}
