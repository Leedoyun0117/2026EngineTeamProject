namespace LDY.Stage {
    public enum SpikeDirection {
        Up,
        Down,
        Left,
        Right
    }

    public static class SpikeTileNamer {
        public static string Name(SpikeDirection direction) {
            switch (direction) {
                case SpikeDirection.Up: return "spike_up";
                case SpikeDirection.Down: return "spike_down";
                case SpikeDirection.Left: return "spike_left";
                default: return "spike_right";
            }
        }
    }
}
