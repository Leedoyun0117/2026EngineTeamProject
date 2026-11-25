namespace LDY.Flow.Core {
    public enum StageStatus {
        Open,
        Cleared,
        Locked,
        NotReady
    }

    // 진행 순서대로 나열된 스테이지 한 칸. number 0은 튜토리얼.
    public readonly struct StageSlot {
        public StageSlot(string id, int world, int number, bool isReady) {
            Id = id;
            World = world;
            Number = number;
            IsReady = isReady;
        }

        public string Id { get; }
        public int World { get; }
        public int Number { get; }
        public bool IsReady { get; }
    }
}
