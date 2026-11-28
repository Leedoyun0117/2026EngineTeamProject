using System.Collections.Generic;
using System.Linq;
using LDY.StageImport;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LDY.StageImportEditor {
    // 대응표 "연결 필요" 프리팹 인스턴스를 검사한다. 비어 있는 필드의 스크립트(필요하면 감지용 트리거도)를 끄고 이름표에 기록한다.
    // 다시 돌렸을 때 연결돼 있으면 기록해 둔 것만 다시 켜고 이름표를 지운다.
    // 프리팹 안에 없는 스크립트(예: Receiver.receivers)는 씬의 그 스크립트가 이 오브젝트를 가리키는지만 보고, 끌 것이 없어 이름표만 단다.
    internal static class LinkGuard {
        private const string LabelName = "LinkLabel";
        private const float LabelLift = 0.9f;

        // 꺼진 스크립트에도 OnTrigger가 오므로, null 검사 없이 door를 쓰는 스크립트는 감지용 트리거 콜라이더도 끈다.
        private static readonly HashSet<string> TriggerOffTypes = new HashSet<string> { "NormalButton" };

        public static IReadOnlyList<string> Apply(GameObject instance, IEnumerable<string> linkFields, Scene scene) {
            var missing = new List<string>();
            var scripts = new List<Behaviour>();
            var triggers = new List<Collider2D>();
            foreach (var spec in linkFields) {
                if (!TrySplit(spec, out var typeName, out var field)) continue;
                var owners = Behaviours(instance).Where(b => b.GetType().Name == typeName).ToList();
                if (owners.Count == 0) {
                    if (!PointedFromScene(scene, typeName, field, instance)) missing.Add(spec);
                    continue;
                }

                foreach (var owner in owners.Where(o => IsEmpty(o, field))) {
                    scripts.Add(owner);
                    if (TriggerOffTypes.Contains(typeName))
                        triggers.AddRange(owner.GetComponents<Collider2D>().Where(c => c.isTrigger));
                    missing.Add(spec);
                }
            }

            var distinct = missing.Distinct().ToList();
            var hold = UpdateLabel(instance, distinct);
            if (hold != null) Hold(hold, scripts, triggers);
            return distinct;
        }

        private static void Hold(LinkHold hold, List<Behaviour> scripts, List<Collider2D> triggers) {
            Release(hold, hold.Scripts.Where(s => !scripts.Contains(s)), hold.Triggers.Where(t => !triggers.Contains(t)));
            Undo.RecordObject(hold, "Disable Unlinked");
            foreach (var script in scripts.Where(s => s.enabled)) {
                hold.HoldScript(script);
                SetEnabled(script, false);
            }

            foreach (var trigger in triggers.Where(t => t.enabled)) {
                hold.HoldTrigger(trigger);
                SetEnabled(trigger, false);
            }
        }

        private static void Release(LinkHold hold, IEnumerable<Behaviour> scripts, IEnumerable<Collider2D> triggers) {
            foreach (var script in scripts.ToList()) SetEnabled(script, true);
            foreach (var trigger in triggers.ToList()) SetEnabled(trigger, true);
        }

        private static void SetEnabled(Behaviour target, bool enabled) {
            if (target == null || target.enabled == enabled) return;
            Undo.RecordObject(target, enabled ? "Enable Linked" : "Disable Unlinked");
            target.enabled = enabled;
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }

        private static bool TrySplit(string spec, out string typeName, out string field) {
            var dot = spec?.LastIndexOf('.') ?? -1;
            typeName = dot > 0 ? spec.Substring(0, dot).Trim() : null;
            field = dot > 0 ? spec.Substring(dot + 1).Trim() : null;
            return dot > 0 && field.Length > 0;
        }

        private static IEnumerable<MonoBehaviour> Behaviours(GameObject go) =>
            go.GetComponentsInChildren<MonoBehaviour>(true).Where(b => b != null);

        private static bool IsEmpty(Object owner, string field) {
            var property = new SerializedObject(owner).FindProperty(field);
            if (property == null) return false;
            if (property.propertyType == SerializedPropertyType.ObjectReference) return property.objectReferenceValue == null;
            if (!property.isArray) return false;
            for (var i = 0; i < property.arraySize; i++)
                if (property.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    return false;
            return true;
        }

        private static bool PointedFromScene(Scene scene, string typeName, string field, GameObject target) {
            foreach (var root in scene.GetRootGameObjects())
            foreach (var behaviour in Behaviours(root).Where(b => b.GetType().Name == typeName)) {
                var property = new SerializedObject(behaviour).FindProperty(field);
                if (property == null) continue;
                if (property.propertyType == SerializedPropertyType.ObjectReference && Points(property.objectReferenceValue, target))
                    return true;
                if (!property.isArray) continue;
                for (var i = 0; i < property.arraySize; i++)
                    if (Points(property.GetArrayElementAtIndex(i).objectReferenceValue, target))
                        return true;
            }

            return false;
        }

        private static bool Points(Object value, GameObject target) {
            var go = value is Component c ? c.gameObject : value as GameObject;
            return go != null && (go == target || go.transform.IsChildOf(target.transform));
        }

        private static LinkHold UpdateLabel(GameObject instance, IReadOnlyList<string> missing) {
            var existing = instance.transform.Find(LabelName);
            if (missing.Count == 0) {
                if (existing == null) return null;
                var held = existing.GetComponent<LinkHold>();
                if (held != null) Release(held, held.Scripts, held.Triggers);
                Undo.DestroyObjectImmediate(existing.gameObject);
                return null;
            }

            var text = "연결 필요: " + string.Join(", ", missing);
            var label = existing != null
                ? existing.gameObject
                : BlockLabel.Attach(instance.transform, text, instance.transform.position + Vector3.up * LabelLift);
            if (existing == null) {
                label.name = LabelName;
                Undo.RegisterCreatedObjectUndo(label, "Disable Unlinked");
            }

            var stageLabel = label.GetComponent<StageLabel>();
            Undo.RecordObject(stageLabel, "Disable Unlinked");
            stageLabel.SetText(text);
            var hold = label.GetComponent<LinkHold>();
            return hold != null ? hold : Undo.AddComponent<LinkHold>(label);
        }
    }
}
