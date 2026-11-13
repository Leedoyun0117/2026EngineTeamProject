namespace LDY.Script {
    public enum RowMoveDirection {
        None,
        Up,
        Down
    }

    public interface IRowMoveSource {
        RowMoveDirection RowMove { get; }
    }
}
