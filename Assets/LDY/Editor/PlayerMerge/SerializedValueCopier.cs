using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.Script {
    // 컴포넌트의 직렬화 값을 그대로 옮긴다. 원본 계층 안을 가리키는 참조는 대상 계층의 같은 자리로 바꾼다.
    internal static class SerializedValueCopier {
        private const string ScriptField = "m_Script";

        public static void Copy(Component source, Component target, HierarchyMap map, string label, MergeLog log,
            Action<SerializedObject> adjust = null) {
            var before = Leaves(target);
            var from = new SerializedObject(source);
            var to = new SerializedObject(target);
            var iterator = from.GetIterator();
            for (var enter = true; iterator.NextVisible(enter); enter = false) {
                if (iterator.name == ScriptField)
                    continue;

                var destination = to.FindProperty(iterator.propertyPath);
                if (destination == null)
                    continue;

                try {
                    destination.boxedValue = iterator.boxedValue;
                }
                catch (Exception e) when (e is NotSupportedException or InvalidOperationException) {
                    log.Notes.Add($"{label}.{iterator.propertyPath}: 값을 옮기지 못함 ({e.Message})");
                }
            }

            Remap(to, map, label, log);
            adjust?.Invoke(to);
            to.ApplyModifiedPropertiesWithoutUndo();
            Record(before, Leaves(target), label, log);
        }

        public static Dictionary<string, string> Leaves(Object target) {
            var map = new Dictionary<string, string>();
            var iterator = new SerializedObject(target).GetIterator();
            while (iterator.NextVisible(true))
                if (iterator.propertyType != SerializedPropertyType.Generic && iterator.name != ScriptField)
                    map[iterator.propertyPath] = Describe(iterator);

            return map;
        }

        public static void Record(Dictionary<string, string> before, Dictionary<string, string> after, string label, MergeLog log) {
            foreach (var pair in after)
                if (!before.TryGetValue(pair.Key, out var old) || old != pair.Value)
                    log.Changed.Add($"{label}.{pair.Key}: {old ?? "(없음)"} -> {pair.Value}");
        }

        private static void Remap(SerializedObject target, HierarchyMap map, string label, MergeLog log) {
            var iterator = target.GetIterator();
            while (iterator.NextVisible(true)) {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference || iterator.name == ScriptField)
                    continue;

                var value = iterator.objectReferenceValue;
                if (value == null || !map.Owns(value))
                    continue;

                var mapped = map.Resolve(value);
                if (mapped == null)
                    log.Notes.Add($"{label}.{iterator.propertyPath}: 대상 계층에 '{value.name}' 짝이 없어 비움");
                iterator.objectReferenceValue = mapped;
            }
        }

        public static string Describe(SerializedProperty property) {
            return property.propertyType switch {
                SerializedPropertyType.Integer or SerializedPropertyType.LayerMask => property.intValue.ToString(),
                SerializedPropertyType.Boolean => property.boolValue.ToString(),
                SerializedPropertyType.Float => property.floatValue.ToString("0.####"),
                SerializedPropertyType.String => property.stringValue,
                SerializedPropertyType.Enum => property.enumValueIndex.ToString(),
                SerializedPropertyType.Vector2 => property.vector2Value.ToString("0.####"),
                SerializedPropertyType.Vector3 => property.vector3Value.ToString("0.####"),
                SerializedPropertyType.Color => property.colorValue.ToString(),
                SerializedPropertyType.Quaternion => property.quaternionValue.ToString("0.####"),
                SerializedPropertyType.ObjectReference => DescribeReference(property.objectReferenceValue),
                SerializedPropertyType.ArraySize => property.intValue.ToString(),
                _ => property.propertyType.ToString()
            };
        }

        private static string DescribeReference(Object value) {
            if (value == null)
                return "null";

            var path = AssetDatabase.GetAssetPath(value);
            return path.Length > 0 ? $"{value.name} ({path})" : $"{value.name}:{value.GetType().Name}";
        }
    }
}
