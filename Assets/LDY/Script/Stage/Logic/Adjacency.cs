namespace LDY.Stage {
    public readonly struct Adjacency {
        public readonly bool Up;
        public readonly bool Down;
        public readonly bool Left;
        public readonly bool Right;

        public Adjacency(bool up, bool down, bool left, bool right) {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }
    }
}
