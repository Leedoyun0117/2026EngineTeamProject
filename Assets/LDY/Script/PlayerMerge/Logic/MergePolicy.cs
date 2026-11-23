namespace LDY.PlayerMerge {
    public enum MergeAction {
        KeepOurs,
        CopyBaseValues,
        AddFromBase
    }

    public enum FieldOwner {
        Base,
        Ours
    }

    public static class MergePolicy {
        public const string VisualChild = "Visual";

        public static MergeAction ForComponent(bool inBase, bool inOurs) {
            if (!inBase)
                return MergeAction.KeepOurs;

            return inOurs ? MergeAction.CopyBaseValues : MergeAction.AddFromBase;
        }

        public static FieldOwner ForChild(string childName) {
            return childName == VisualChild ? FieldOwner.Ours : FieldOwner.Base;
        }

        public static bool IsBrokenMask(int oursBits, int baseBits) {
            return oursBits == 0 && baseBits != 0;
        }
    }
}
