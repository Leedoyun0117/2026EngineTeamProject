using LDY.Script;
using NUnit.Framework;

namespace LDY.Tests {
    public class RendererReadinessTests {
        [Test]
        public void AllPartsPresentIsReady() {
            Assert.AreEqual(RendererReadiness.Ready, RendererReadinessRules.Evaluate(true, true, true));
        }

        [Test]
        public void MissingAnimSetKeepsRendererAliveWithoutAnimation() {
            Assert.AreEqual(RendererReadiness.MissingAnimSet, RendererReadinessRules.Evaluate(true, true, false));
        }

        [Test]
        public void MissingSourceIsBroken() {
            Assert.AreEqual(RendererReadiness.Broken, RendererReadinessRules.Evaluate(false, true, true));
        }

        [Test]
        public void MissingTargetIsBroken() {
            Assert.AreEqual(RendererReadiness.Broken, RendererReadinessRules.Evaluate(true, false, true));
        }

        [Test]
        public void MissingEverythingIsBroken() {
            Assert.AreEqual(RendererReadiness.Broken, RendererReadinessRules.Evaluate(false, false, false));
        }
    }
}
