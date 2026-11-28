using System;
using System.Globalization;
using LDY.SkillTargets.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    // SerializedProperty 값을 규칙 표의 문자열과 서로 바꾼다.
    internal static class SerializedValue {
        private const string None = "(없음)";

        public static string Read(SerializedProperty property, GameObject owner) {
            switch (property.propertyType) {
                case SerializedPropertyType.Float: return property.floatValue.ToString("R", CultureInfo.InvariantCulture);
                case SerializedPropertyType.Integer: return property.intValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Boolean: return property.boolValue ? "true" : "false";
                case SerializedPropertyType.String: return property.stringValue;
                case SerializedPropertyType.Enum:
                    var index = property.enumValueIndex;
                    return index >= 0 && index < property.enumNames.Length ? property.enumNames[index] : index.ToString();
                case SerializedPropertyType.ObjectReference:
                    var value = property.objectReferenceValue;
                    if (value == null) return None;
                    return value is Component c && c.gameObject == owner ? HtyContract.SelfReference : value.name;
                default: return property.propertyType.ToString();
            }
        }

        public static bool Write(SerializedProperty property, string text, GameObject owner) {
            switch (property.propertyType) {
                case SerializedPropertyType.Float:
                    if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return false;
                    property.floatValue = f;
                    return true;
                case SerializedPropertyType.Integer:
                    if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return false;
                    property.intValue = i;
                    return true;
                case SerializedPropertyType.Boolean:
                    if (!bool.TryParse(text, out var b)) return false;
                    property.boolValue = b;
                    return true;
                case SerializedPropertyType.String:
                    property.stringValue = text;
                    return true;
                case SerializedPropertyType.Enum:
                    var index = Array.IndexOf(property.enumNames, text);
                    if (index < 0) return false;
                    property.enumValueIndex = index;
                    return true;
                case SerializedPropertyType.ObjectReference:
                    return WriteSelfReference(property, text, owner);
                default: return false;
            }
        }

        private static bool WriteSelfReference(SerializedProperty property, string text, GameObject owner) {
            if (text != HtyContract.SelfReference) return false;
            var type = HtyTypes.Find(ReferencedTypeName(property.type));
            var component = type != null ? owner.GetComponent(type) : null;
            if (component == null) return false;
            property.objectReferenceValue = component;
            return true;
        }

        private static string ReferencedTypeName(string propertyType) {
            const string prefix = "PPtr<$";
            return propertyType.StartsWith(prefix) ? propertyType.Substring(prefix.Length).TrimEnd('>') : propertyType;
        }
    }
}
