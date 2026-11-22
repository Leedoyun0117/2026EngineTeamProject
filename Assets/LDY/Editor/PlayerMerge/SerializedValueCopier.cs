using UnityEditor;
using UnityEngine;
using LDY.PlayerMerge;

namespace LDY.Script {
    internal static class SerializedValueCopier {
        private const string ScriptField = "m_Script";

        public static void CopyValues(Component source, Component target, string label, MergeLog log) {
            var from = new SerializedObject(source);
            var to = new SerializedObject(target);
            var iterator = from.GetIterator();
            for (var enter = true; iterator.NextVisible(enter); enter = false) {
                if (iterator.name == ScriptField || iterator.propertyType == SerializedPropertyType.ObjectReference)
                    continue;

                var destination = to.FindProperty(iterator.propertyPath);
                if (destination == null || SerializedProperty.DataEquals(iterator, destination))
                    continue;

                Record(iterator, destination, label, log);
                destination.boxedValue = iterator.boxedValue;
            }

            to.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Record(SerializedProperty from, SerializedProperty to, string label, MergeLog log) {
            var line = $"{label}.{from.propertyPath}: {Describe(to)} -> {Describe(from)}";
            var broken = from.propertyType == SerializedPropertyType.LayerMask
                         && MergePolicy.IsBrokenMask(to.intValue, from.intValue);
            (broken ? log.Fixed : log.Changed).Add(line);
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
