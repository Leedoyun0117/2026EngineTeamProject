using System.Collections.Generic;
using LDY.PlayerMerge;
using NUnit.Framework;

namespace LDY.Tests {
    public class PlayerRulesTests {
        [TestCase(4, true)]
        [TestCase(7, true)]
        [TestCase(0, false)]
        [TestCase(3, false)]
        public void RotationFrozen_ChecksFreezeRotationBit(int constraints, bool expected) {
            Assert.AreEqual(expected, PlayerRules.RotationFrozen(constraints));
        }

        [TestCase(0, false)]
        [TestCase(512, true)]
        public void MaskFilled_ZeroIsBroken(int bits, bool expected) {
            Assert.AreEqual(expected, PlayerRules.MaskFilled(bits));
        }

        [Test]
        public void UnitScale_OnlyOne() {
            Assert.IsTrue(PlayerRules.UnitScale(new Float2(1f, 1f)));
            Assert.IsFalse(PlayerRules.UnitScale(new Float2(2f, 2f)));
        }

        [Test]
        public void GroundCheck_JJBLevelValues_ReachBottom() {
            Assert.IsTrue(PlayerRules.GroundCheckReachesBottom(-0.13f, 0.25f, -0.0085f));
        }

        [Test]
        public void GroundCheck_StalePrefabDefault_IsFarBelow() {
            Assert.IsFalse(PlayerRules.GroundCheckReachesBottom(-1.36f, 0.25f, -0.0085f));
        }

        [Test]
        public void GroundCheckWidth_WiderThanCollider_Fails() {
            Assert.IsTrue(PlayerRules.GroundCheckFitsWidth(0.5f, 0.872f));
            Assert.IsFalse(PlayerRules.GroundCheckFitsWidth(1f, 0.74f));
        }

        [Test]
        public void FeetAligned_WithinOnePixel() {
            Assert.IsTrue(PlayerRules.FeetAligned(-0.0085f, -0.0085f, 0.02f));
            Assert.IsTrue(PlayerRules.FeetAligned(0.0115f, -0.0085f, 0.02f));
            Assert.IsFalse(PlayerRules.FeetAligned(0.05f, -0.0085f, 0.02f));
        }

        [TestCase(1, 0, true)]
        [TestCase(2, 0, false)]
        [TestCase(1, 1, false)]
        [TestCase(0, 0, false)]
        public void SingleVisual_OneRendererNoAnimator(int renderers, int animators, bool expected) {
            Assert.AreEqual(expected, PlayerRules.SingleVisual(renderers, animators));
        }

        [Test]
        public void Missing_And_Forbidden_ListNames() {
            var present = new List<string> { "JJBPlayer", "SkillManager_HTY" };

            CollectionAssert.AreEqual(new[] { "JJBPlayerGimmick" }, PlayerRules.Missing(present, new[] { "JJBPlayer", "JJBPlayerGimmick" }));
            CollectionAssert.AreEqual(new[] { "SkillManager_HTY" }, PlayerRules.Forbidden(present, PlayerParts.CopyRemovedComponents));
        }

        [Test]
        public void CopyRemoves_SkillManager_BecauseItsAwakeDestroysDuplicates() {
            CollectionAssert.Contains(PlayerParts.CopyRemovedComponents, PlayerParts.SkillManager);
            CollectionAssert.DoesNotContain(PlayerParts.CommonRootComponents, PlayerParts.SkillManager);
        }

        [Test]
        public void Render_CountsFailures() {
            var rows = new List<CheckRow> { new("최종본", "a", true, "x"), new("최종본", "b", false, "y") };

            var text = CheckTable.Render(rows);

            StringAssert.Contains("| 최종본 | b | X | y |", text);
            StringAssert.Contains("실패 1개 / 전체 2개", text);
        }

        [TestCase(true, false, "m_LocalPosition.x", true)]
        [TestCase(true, false, "m_LocalRotation.w", true)]
        [TestCase(true, false, "m_LocalScale.x", false)]
        [TestCase(false, true, "m_Name", true)]
        [TestCase(false, true, "m_IsActive", true)]
        [TestCase(false, false, "m_LocalPosition.y", false)]
        [TestCase(false, false, "objectLayer.m_Bits", false)]
        public void SwapOverrideRule_CarriesOnlyRootPlacementAndName(bool rootTransform, bool rootGo, string path, bool expected) {
            Assert.AreEqual(expected, SwapOverrideRule.IsCarried(rootTransform, rootGo, path));
        }
    }
}
