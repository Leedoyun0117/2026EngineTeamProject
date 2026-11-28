using System.Collections.Generic;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    // 계획을 한 오브젝트에 실행한다. 지우는 단계는 없다.
    internal static class PlanApplier {
        public static int Apply(GameObject go, TargetPlan plan, List<string> errors) {
            var done = 0;
            foreach (var step in plan.Steps) {
                if (!step.IsChange) continue;
                if (Run(go, step)) done++;
                else errors.Add($"{go.name}: 실패 {step.Describe()}");
            }

            return done;
        }

        private static bool Run(GameObject go, PlanStep step) {
            switch (step.Kind) {
                case StepKind.AddRigidbody:
                    go.AddComponent<Rigidbody2D>().bodyType = ToBodyType(step.To);
                    return true;
                case StepKind.AddComponent:
                    var type = HtyTypes.Find(step.TypeName);
                    return type != null && go.AddComponent(type) != null;
                case StepKind.SetField:
                    return SetField(go, step);
                case StepKind.SetMaterial:
                    var renderer = go.GetComponent<SpriteRenderer>();
                    var material = AssetDatabase.LoadAssetAtPath<Material>(step.To);
                    if (renderer == null || material == null) return false;
                    renderer.sharedMaterial = material;
                    return true;
                case StepKind.SetLayer:
                    go.layer = int.Parse(step.To);
                    return true;
                default:
                    return false;
            }
        }

        private static bool SetField(GameObject go, PlanStep step) {
            var type = HtyTypes.Find(step.TypeName);
            var component = type != null ? go.GetComponent(type) : null;
            if (component == null) return false;

            var serialized = new SerializedObject(component);
            var property = serialized.FindProperty(step.Field);
            if (property == null || !SerializedValue.Write(property, step.To, go)) return false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static RigidbodyType2D ToBodyType(string body) => body switch {
            nameof(BodyPolicy.Static) => RigidbodyType2D.Static,
            nameof(BodyPolicy.Dynamic) => RigidbodyType2D.Dynamic,
            _ => RigidbodyType2D.Kinematic
        };
    }
}
