using System;
using System.Linq;
using LDY.Hud.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public class SkillStateMapTests {
        // 태영 SkillState 중 HUD 아이콘이 없는 값. None은 '능력 없음'.
        private static readonly string[] KnownUnmapped = { "None", "ReSize", "Paste" };

        [TestCase("TimeStop", AbilityId.Pause)]
        [TestCase("Cutting", AbilityId.Cut)]
        [TestCase("Drag", AbilityId.Drag)]
        [TestCase("Opacity", AbilityId.Alpha)]
        [TestCase("Luminosity", AbilityId.Light)]
        [TestCase("Tp", AbilityId.Tab)]
        [TestCase("Copy", AbilityId.Copy)]
        [TestCase("ReSizeObj", AbilityId.Zoom)]
        public void MapsSkillStateToHudAbility(string state, AbilityId expected) {
            Assert.IsTrue(SkillStateMap.TryMap(state, out var id));
            Assert.AreEqual(expected, id);
            Assert.AreEqual(AbilitySlotState.Of(expected), SkillStateMap.Resolve(state));
        }

        [Test]
        public void EveryHudAbilityHasExactlyOneSkill() {
            var mapped = SkillStateMap.Entries.Values.ToList();
            var abilities = Enum.GetValues(typeof(AbilityId)).Cast<AbilityId>().Where(a => a != AbilityId.None);

            CollectionAssert.AreEquivalent(abilities, mapped);
        }

        [Test]
        public void NoneOrEmptyIsEmptySlot() {
            Assert.AreEqual(AbilitySlotState.Empty, SkillStateMap.Resolve("None"));
            Assert.AreEqual(AbilitySlotState.Empty, SkillStateMap.Resolve(""));
            Assert.AreEqual(AbilitySlotState.Empty, SkillStateMap.Resolve(null));
            Assert.IsFalse(SkillStateMap.Resolve(null).HasAbility);
        }

        [TestCase("Paste")]
        [TestCase("ReSize")]
        [TestCase("SomethingNew")]
        public void UnknownSkillKeepsNameAndShowsAsUnmapped(string state) {
            var slot = SkillStateMap.Resolve(state);

            Assert.IsTrue(slot.HasAbility);
            Assert.IsTrue(slot.IsUnmapped);
            Assert.IsFalse(slot.IsMapped);
            Assert.AreEqual(state, slot.SourceName);
        }

        // 태영 enum에 값이 추가되면 여기서 알 수 있다(Assembly-CSharp가 없으면 판정 보류).
        [Test]
        public void EverySkillStateIsMappedOrKnownUnmapped() {
            var type = Type.GetType("SkillState, Assembly-CSharp");
            if (type == null)
                Assert.Inconclusive("Assembly-CSharp의 SkillState를 찾지 못함");

            var missing = Enum.GetNames(type)
                .Where(name => !SkillStateMap.TryMap(name, out _) && !KnownUnmapped.Contains(name))
                .ToList();
            CollectionAssert.IsEmpty(missing, "HUD 대응표에 없는 SkillState: " + string.Join(", ", missing));
        }

        [Test]
        public void OfNoneIsEmpty() {
            Assert.AreEqual(AbilitySlotState.Empty, AbilitySlotState.Of(AbilityId.None));
            Assert.IsTrue(AbilitySlotState.Of(AbilityId.Cut).IsMapped);
            Assert.AreNotEqual(AbilitySlotState.Of(AbilityId.Cut), AbilitySlotState.Of(AbilityId.Drag));
        }
    }
}
