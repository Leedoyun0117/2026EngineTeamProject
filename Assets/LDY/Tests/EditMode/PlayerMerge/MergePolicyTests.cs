using LDY.PlayerMerge;
using NUnit.Framework;

namespace LDY.Tests {
    public class MergePolicyTests {
        [TestCase(true, true, MergeAction.CopyBaseValues)]
        [TestCase(true, false, MergeAction.AddFromBase)]
        [TestCase(false, true, MergeAction.KeepOurs)]
        [TestCase(false, false, MergeAction.KeepOurs)]
        public void ForComponent_FollowsPresence(bool inBase, bool inOurs, MergeAction expected) {
            Assert.AreEqual(expected, MergePolicy.ForComponent(inBase, inOurs));
        }

        [TestCase("Visual", FieldOwner.Ours)]
        [TestCase("GroundCheck", FieldOwner.Base)]
        [TestCase("InteractPoint", FieldOwner.Base)]
        [TestCase("AbsorbPoint", FieldOwner.Base)]
        public void ForChild_OnlyVisualBelongsToOurs(string name, FieldOwner expected) {
            Assert.AreEqual(expected, MergePolicy.ForChild(name));
        }

        [TestCase(0, 512, true)]
        [TestCase(0, 0, false)]
        [TestCase(512, 512, false)]
        [TestCase(64, 512, false)]
        public void IsBrokenMask_OnlyEmptyOursWithFilledBase(int ours, int baseBits, bool expected) {
            Assert.AreEqual(expected, MergePolicy.IsBrokenMask(ours, baseBits));
        }
    }
}
