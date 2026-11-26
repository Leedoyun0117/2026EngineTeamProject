namespace LDY.Flow.Core {
    public interface IProgressStore {
        ProgressModel Load();
        void Save(ProgressModel progress);
    }
}
