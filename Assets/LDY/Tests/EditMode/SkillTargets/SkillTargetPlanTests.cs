using System.Collections.Generic;
using System.Linq;
using LDY.SkillTargets.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class SkillTargetPlanTests {
        private const string BoxPath = "Assets/Main/02.Prefabs/Obj/Box.prefab";

        private static readonly AbilityRequirement Stop = new AbilityRequirement("stop", "CanStop", true, true);

        private static readonly AbilityRequirement Drag = new AbilityRequirement("drag", "CanDrag", false, true,
            new[] { new ComponentRequirement("DragObj", Fields(("_rb", HtyContract.SelfReference))) });

        private static readonly AbilityRequirement Zoom = new AbilityRequirement("zoom", "CanResize", true, false,
            new[] { new ComponentRequirement("ReScaleObject_HTY", Fields(("_smallScale", "0.5"), ("_largeScale", "2"))) });

        private static readonly AbilityRequirement Cut = new AbilityRequirement("cut", "CanCut", true, false);

        private static Dictionary<string, string> Fields(params (string key, string value)[] pairs) =>
            pairs.ToDictionary(p => p.key, p => p.value);

        private static MergedTarget Single(params TargetRequest[] requests) {
            var merged = TargetMerger.Merge(requests);
            Assert.AreEqual(1, merged.Count);
            return merged[0];
        }

        private static TargetRequest Request(string label, params AbilityRequirement[] abilities) =>
            new TargetRequest(BoxPath, "", label, abilities, BodyPolicy.Dynamic);

        [Test]
        public void Merge_SamePrefabFromSeveralRows_UnionsAbilitiesAndLabels() {
            var target = Single(Request("상자 (정지)", Stop), Request("상자 (드래그)", Drag), Request("상자 (확대)", Zoom));

            CollectionAssert.AreEqual(new[] { "stop", "drag", "zoom" }, target.AbilityIds);
            CollectionAssert.AreEqual(new[] { "상자 (정지)", "상자 (드래그)", "상자 (확대)" }, target.Labels);
            CollectionAssert.AreEqual(new[] { "DragObj", "ReScaleObject_HTY" }, target.Components.Select(c => c.TypeName));
            Assert.IsTrue(target.NeedsRigidbody);
        }

        [Test]
        public void Merge_TwoSelectorAbilities_IsMyCanConflict() {
            var target = Single(Request("a", Stop), Request("b", Drag), Request("c", Zoom));

            Assert.IsTrue(target.HasMyCanConflict);
            Assert.IsNull(target.MyCan);
            CollectionAssert.AreEquivalent(new[] { "CanStop", "CanResize" }, target.MyCanConflicts);
        }

        [Test]
        public void Merge_DragDoesNotCompeteWithSelectorAbility() {
            var target = Single(Request("a", Drag), Request("b", Cut));

            Assert.IsFalse(target.HasMyCanConflict);
            Assert.AreEqual("CanCut", target.MyCan);
        }

        [Test]
        public void Merge_DragOnly_FallsBackToItsMyCan() {
            Assert.AreEqual("CanDrag", Single(Request("a", Drag)).MyCan);
        }

        [Test]
        public void Merge_SameAbilityTwice_CountsOnce() {
            var target = Single(Request("a", Stop), Request("b", Stop));

            CollectionAssert.AreEqual(new[] { "stop" }, target.AbilityIds);
            Assert.AreEqual("CanStop", target.MyCan);
        }

        [Test]
        public void Merge_DifferentChildren_StaySeparate() {
            var merged = TargetMerger.Merge(new[] {
                new TargetRequest(BoxPath, "", "a", new[] { Stop }),
                new TargetRequest(BoxPath, "Child", "b", new[] { Cut })
            });

            Assert.AreEqual(2, merged.Count);
        }

        [Test]
        public void Merge_DisagreeingBodyPolicy_FallsBackToSkipWithWarning() {
            var target = Single(new TargetRequest(BoxPath, "", "a", new[] { Stop }, BodyPolicy.Kinematic),
                new TargetRequest(BoxPath, "", "b", new[] { Stop }, BodyPolicy.Dynamic));

            Assert.AreEqual(BodyPolicy.Skip, target.Body);
            Assert.AreEqual(1, target.Warnings.Count);
        }

        [Test]
        public void Plan_ExistingComponentsWithSameValues_AreSkipped() {
            var target = Single(Request("a", Zoom));
            var snapshot = new TargetSnapshot { HasCollider = true, HasSpriteRenderer = true };
            snapshot.Components["ReScaleObject_HTY"] = Fields(("_smallScale", "0.5"), ("_largeScale", "2"));
            snapshot.Components[HtyContract.ObjectType] = Fields((HtyContract.MyCanField, "CanResize"));

            var plan = PlanCalculator.Calculate(target, snapshot);

            Assert.IsFalse(plan.HasChanges);
        }

        [Test]
        public void Plan_ExistingComponentWithOtherValue_OnlySetsThatField() {
            var target = Single(Request("a", Zoom));
            var snapshot = new TargetSnapshot { HasCollider = true, HasSpriteRenderer = true };
            snapshot.Components["ReScaleObject_HTY"] = Fields(("_smallScale", "0.5"), ("_largeScale", "1.5"));

            var steps = PlanCalculator.Calculate(target, snapshot).Steps.Where(s => s.IsChange).ToList();

            Assert.IsFalse(steps.Any(s => s.Kind == StepKind.AddComponent && s.TypeName == "ReScaleObject_HTY"));
            var set = steps.Single(s => s.TypeName == "ReScaleObject_HTY");
            Assert.AreEqual("_largeScale", set.Field);
            Assert.AreEqual("1.5", set.From);
            Assert.AreEqual("2", set.To);
        }

        [Test]
        public void Plan_TaeyoungBox_LinksRbAndLeavesObjectHtyEmptyOnConflict() {
            var target = Single(Request("a", Stop), Request("b", Drag), Request("c", Zoom));
            var snapshot = new TargetSnapshot { HasCollider = true, HasSpriteRenderer = true, HasRigidbody = true };
            snapshot.Components["DragObj"] = Fields(("_rb", "(없음)"));
            snapshot.Components["ReScaleObject_HTY"] = Fields(("_smallScale", "0.5"), ("_largeScale", "2"));

            var plan = PlanCalculator.Calculate(target, snapshot);
            var changes = plan.Steps.Where(s => s.IsChange).ToList();

            Assert.AreEqual(1, changes.Count);
            Assert.AreEqual("_rb", changes[0].Field);
            Assert.AreEqual(HtyContract.SelfReference, changes[0].To);
            Assert.IsFalse(plan.Steps.Any(s => s.TypeName == HtyContract.ObjectType));
            Assert.AreEqual(1, plan.Steps.Count(s => s.Kind == StepKind.Conflict));
        }

        [Test]
        public void Plan_MissingRigidbody_AddsWithPolicyOrWarnsOnSkip() {
            var snapshot = new TargetSnapshot { HasCollider = true, HasSpriteRenderer = true };
            var kinematic = Single(new TargetRequest(BoxPath, "", "a", new[] { Stop }, BodyPolicy.Kinematic));
            var skip = Single(new TargetRequest(BoxPath, "", "a", new[] { Stop }, BodyPolicy.Skip));

            var added = PlanCalculator.Calculate(kinematic, snapshot).Steps.First();
            var skipped = PlanCalculator.Calculate(skip, snapshot).Steps;

            Assert.AreEqual(StepKind.AddRigidbody, added.Kind);
            Assert.AreEqual("Kinematic", added.To);
            Assert.IsFalse(skipped.Any(s => s.Kind == StepKind.AddRigidbody));
            Assert.IsTrue(skipped.Any(s => s.Kind == StepKind.Warning));
        }

        [Test]
        public void Plan_NewTarget_AddsRigidbodyBeforeComponentsAndObjectHtyLast() {
            var target = Single(Request("a", Drag));
            var steps = PlanCalculator.Calculate(target, new TargetSnapshot { HasCollider = true, HasSpriteRenderer = true })
                .Steps.Where(s => s.IsChange).ToList();

            Assert.AreEqual(StepKind.AddRigidbody, steps[0].Kind);
            Assert.AreEqual("DragObj", steps[1].TypeName);
            Assert.AreEqual(HtyContract.ObjectType, steps.Last().TypeName);
            Assert.AreEqual("CanDrag", steps.Last().To);
        }

        [Test]
        public void Plan_IsIdempotent_AfterApplyingSnapshot() {
            var target = Single(Request("a", Zoom));
            var snapshot = new TargetSnapshot { HasCollider = true, HasSpriteRenderer = true };
            foreach (var step in PlanCalculator.Calculate(target, snapshot).Steps.Where(s => s.Kind == StepKind.SetField)) {
                if (!snapshot.Components.TryGetValue(step.TypeName, out var fields))
                    snapshot.Components[step.TypeName] = fields = new Dictionary<string, string>();
                fields[step.Field] = step.To;
            }

            Assert.IsFalse(PlanCalculator.Calculate(target, snapshot).HasChanges);
        }

        [Test]
        public void Plan_OldObjectHtyWithoutMyCanField_ShowsMissing() {
            var target = Single(Request("a", Cut));
            var snapshot = new TargetSnapshot { HasCollider = true, HasSpriteRenderer = true };
            snapshot.Components[HtyContract.ObjectType] = new Dictionary<string, string>();

            var step = PlanCalculator.Calculate(target, snapshot).Steps.Single(s => s.IsChange);

            Assert.AreEqual(FieldValues.Missing, step.From);
            Assert.AreEqual("CanCut", step.To);
        }

        [Test]
        public void Plan_SelectorNeedsColliderAndSpriteRenderer() {
            var target = Single(Request("a", Cut));
            var warnings = PlanCalculator.Calculate(target, new TargetSnapshot()).Steps.Count(s => s.Kind == StepKind.Warning);

            Assert.AreEqual(2, warnings);
        }

        [Test]
        public void Plan_TargetChildMissing_OnlyWarns() {
            var plan = PlanCalculator.Calculate(Single(Request("a", Cut)), TargetSnapshot.NotFound());

            Assert.IsFalse(plan.HasChanges);
            Assert.IsTrue(plan.HasIssues);
        }

        [TestCase("0.5", "0.50", true)]
        [TestCase("2", "2.0", true)]
        [TestCase("1.5", "2", false)]
        [TestCase("CanStop", "CanStop", true)]
        [TestCase("CanStop", "CanCut", false)]
        public void FieldValues_CompareNumbersByValue(string a, string b, bool same) {
            Assert.AreEqual(same, FieldValues.Same(a, b));
        }
    }
}
