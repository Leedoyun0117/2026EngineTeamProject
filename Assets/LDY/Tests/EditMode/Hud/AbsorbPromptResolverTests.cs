using LDY.Hud.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public class AbsorbPromptResolverTests {
        private static AbsorbSnapshot S(bool target, bool holding, bool locked = false) =>
            new AbsorbSnapshot(target, holding, locked);

        [Test]
        public void NoTargetHidesPrompt() {
            Assert.AreEqual(AbsorbPrompt.Hidden, AbsorbPromptResolver.Prompt(S(false, false)));
            Assert.AreEqual(AbsorbPrompt.Hidden, AbsorbPromptResolver.Prompt(S(false, true)));
            Assert.AreEqual(AbsorbPrompt.Hidden, AbsorbPromptResolver.Prompt(S(false, false), true));
        }

        [Test]
        public void LockedHidesPromptEvenWithTarget() {
            Assert.AreEqual(AbsorbPrompt.Hidden, AbsorbPromptResolver.Prompt(S(true, false, true)));
            Assert.AreEqual(AbsorbPrompt.Hidden, AbsorbPromptResolver.Prompt(S(true, true, true), true));
        }

        [Test]
        public void TargetWithoutAbilityIsAvailable() {
            Assert.AreEqual(AbsorbPrompt.Available, AbsorbPromptResolver.Prompt(S(true, false)));
        }

        [Test]
        public void TargetWhileHoldingIsBlocked() {
            Assert.AreEqual(AbsorbPrompt.Blocked, AbsorbPromptResolver.Prompt(S(true, true)));
        }

        [Test]
        public void RecentFailureShowsBlocked() {
            Assert.AreEqual(AbsorbPrompt.Blocked, AbsorbPromptResolver.Prompt(S(true, false), true));
        }

        [Test]
        public void PressWithoutTargetOrWhileLockedIsIgnored() {
            Assert.AreEqual(AbsorbOutcome.None, AbsorbPromptResolver.Judge(S(false, false), S(false, true)));
            Assert.AreEqual(AbsorbOutcome.None, AbsorbPromptResolver.Judge(S(true, false, true), S(true, false)));
            Assert.AreEqual(AbsorbOutcome.None, AbsorbPromptResolver.Judge(S(true, true, true), S(true, true)));
        }

        [Test]
        public void PressWhileHoldingIsAlreadyHolding() {
            Assert.AreEqual(AbsorbOutcome.AlreadyHolding, AbsorbPromptResolver.Judge(S(true, true), S(true, true)));
        }

        [Test]
        public void HoldingAfterPressIsSuccess() {
            Assert.AreEqual(AbsorbOutcome.Success, AbsorbPromptResolver.Judge(S(true, false), S(false, true)));
        }

        [Test]
        public void NotHoldingAfterPressIsRejected() {
            Assert.AreEqual(AbsorbOutcome.Rejected, AbsorbPromptResolver.Judge(S(true, false), S(true, false)));
        }

        [Test]
        public void OnlyAlreadyHoldingAndRejectedAreFailures() {
            Assert.IsFalse(AbsorbPromptResolver.IsFailure(AbsorbOutcome.None));
            Assert.IsFalse(AbsorbPromptResolver.IsFailure(AbsorbOutcome.Success));
            Assert.IsTrue(AbsorbPromptResolver.IsFailure(AbsorbOutcome.AlreadyHolding));
            Assert.IsTrue(AbsorbPromptResolver.IsFailure(AbsorbOutcome.Rejected));
        }
    }
}
