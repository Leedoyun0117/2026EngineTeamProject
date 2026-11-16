using LDY.Script;
using NUnit.Framework;

namespace LDY.Tests {
    public class ReleaseGateTests {
        [Test]
        public void PressInSameFrameAsGrab_IsIgnored() {
            var gate = new ReleaseGate(0f);
            gate.MarkGrab(10, 1f);

            Assert.IsFalse(gate.Accepts(10, 1f));
        }

        [Test]
        public void PressBeforeGrabFrame_IsIgnored() {
            var gate = new ReleaseGate(0f);
            gate.MarkGrab(10, 1f);

            Assert.IsFalse(gate.Accepts(9, 1f));
        }

        [Test]
        public void PressFromNextFrame_IsAccepted() {
            var gate = new ReleaseGate(0f);
            gate.MarkGrab(10, 1f);

            Assert.IsTrue(gate.Accepts(11, 1.02f));
        }

        [Test]
        public void MinDelay_BlocksUntilElapsed() {
            var gate = new ReleaseGate(0.1f);
            gate.MarkGrab(10, 1f);

            Assert.IsFalse(gate.Accepts(11, 1.05f));
            Assert.IsTrue(gate.Accepts(12, 1.1f));
        }

        [Test]
        public void RegrabAfterRelease_RespectsCooldownSlot() {
            var slot = new HoldSlot();
            var owner = new object();
            Assert.IsTrue(slot.TryAcquire(owner, 0f));
            slot.Release(owner, 1f, 0.2f);

            Assert.IsFalse(slot.TryAcquire(owner, 1.1f));
            Assert.IsTrue(slot.TryAcquire(owner, 1.2f));
        }
    }
}
