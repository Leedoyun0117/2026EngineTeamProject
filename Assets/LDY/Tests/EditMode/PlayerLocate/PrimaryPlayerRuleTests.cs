using LDY.PlayerLocate;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class PrimaryPlayerRuleTests {
        private static PlayerCandidate C(string name, bool input) => new PlayerCandidate(name, input);

        [Test]
        public void NoCandidates_NotFound() {
            Assert.That(PrimaryPlayerRule.Pick(new PlayerCandidate[0]).Found, Is.False);
            Assert.That(PrimaryPlayerRule.Pick(null).Found, Is.False);
        }

        [Test]
        public void SingleWithInput_PickedByInput() {
            var pick = PrimaryPlayerRule.Pick(new[] { C("Player", true) });
            Assert.That(pick.Index, Is.EqualTo(0));
            Assert.That(pick.Reason, Is.EqualTo(PrimaryReason.ActiveInput));
        }

        [Test]
        public void OnlyOneHasActiveInput_WinsEvenIfListedLater() {
            var pick = PrimaryPlayerRule.Pick(new[] { C("PlayerCopy", false), C("Player", true) });
            Assert.That(pick.Index, Is.EqualTo(1));
            Assert.That(pick.IsAmbiguous, Is.False);
        }

        [Test]
        public void InputBeatsName() {
            // 입력이 있는 쪽이 이름보다 우선한다.
            var pick = PrimaryPlayerRule.Pick(new[] { C("Player", false), C("PlayerCopy", true) });
            Assert.That(pick.Index, Is.EqualTo(1));
            Assert.That(pick.Reason, Is.EqualTo(PrimaryReason.ActiveInput));
        }

        [Test]
        public void BothHaveInput_CopyNameExcluded() {
            var pick = PrimaryPlayerRule.Pick(new[] { C("PlayerCopy", true), C("Player", true) });
            Assert.That(pick.Index, Is.EqualTo(1));
            Assert.That(pick.Reason, Is.EqualTo(PrimaryReason.Name));
        }

        [Test]
        public void NoInput_CloneNameExcluded() {
            var pick = PrimaryPlayerRule.Pick(new[] { C("Player(Clone)", false), C("Player", false) });
            Assert.That(pick.Index, Is.EqualTo(1));
            Assert.That(pick.Reason, Is.EqualTo(PrimaryReason.Name));
        }

        [Test]
        public void NameFilterOnlyAmongInputHolders() {
            // 입력이 있는 두 후보가 모두 복사본 이름이면, 입력 없는 깨끗한 이름으로 넘어가지 않고 첫 번째(경고).
            var pick = PrimaryPlayerRule.Pick(new[] { C("Player", false), C("CopyA", true), C("CopyB", true) });
            Assert.That(pick.Index, Is.EqualTo(1));
            Assert.That(pick.IsAmbiguous, Is.True);
        }

        [Test]
        public void TwoIndistinguishable_FirstWithWarning() {
            var pick = PrimaryPlayerRule.Pick(new[] { C("A", true), C("B", true) });
            Assert.That(pick.Index, Is.EqualTo(0));
            Assert.That(pick.IsAmbiguous, Is.True);
        }

        [Test]
        public void SingleCopyWithoutInput_FallbackWithWarning() {
            var pick = PrimaryPlayerRule.Pick(new[] { C("PlayerCopy", false) });
            Assert.That(pick.Index, Is.EqualTo(0));
            Assert.That(pick.IsAmbiguous, Is.True);
        }

        [TestCase("PlayerCopy", true)]
        [TestCase("player_copy", true)]
        [TestCase("Player(Clone)", true)]
        [TestCase("Player", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void LooksLikeCopy(string name, bool expected) {
            Assert.That(PrimaryPlayerRule.LooksLikeCopy(name), Is.EqualTo(expected));
        }
    }
}
