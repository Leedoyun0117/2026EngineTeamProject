namespace LDY.Spider {
    public interface ISeverable {
        void Sever();
    }

    public interface IPlayerLocator {
        bool TryGetX(out float x);
    }
}
