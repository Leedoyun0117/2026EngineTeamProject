namespace LDY.Script {
    public sealed class NoPoseSource : IGrabStateSource, IRowMoveSource, IRopeStateSource {
        public static readonly NoPoseSource Instance = new();

        private NoPoseSource() {
        }

        public bool IsGrabbing => false;
        public bool IsRopeHanging => false;
        public RowMoveDirection RowMove => RowMoveDirection.None;
    }

    public readonly struct PoseSourceSet {
        private readonly IGrabStateSource _grab;
        private readonly IRopeStateSource _rope;
        private readonly IRowMoveSource _rowMove;

        public PoseSourceSet(IGrabStateSource grab, IRowMoveSource rowMove, IRopeStateSource rope = null) {
            _grab = grab ?? NoPoseSource.Instance;
            _rowMove = rowMove ?? NoPoseSource.Instance;
            _rope = rope ?? NoPoseSource.Instance;
        }

        public bool IsGrabbing => (_grab ?? NoPoseSource.Instance).IsGrabbing;
        public bool IsRopeHanging => (_rope ?? NoPoseSource.Instance).IsRopeHanging;
        public RowMoveDirection RowMove => (_rowMove ?? NoPoseSource.Instance).RowMove;
    }
}
