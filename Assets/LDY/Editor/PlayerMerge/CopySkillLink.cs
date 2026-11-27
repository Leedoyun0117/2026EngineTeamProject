using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // 최종본의 복사 스킬이 B'(PlayerCopy 원본) 대신 최종본의 Variant를 복제하게 한다. 태영 파일은 건드리지 않는다.
    internal static class CopySkillLink {
        public static bool Run(MergeLog log) {
            var copy = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPaths.Copy);
            using var final = PrefabContents.Open(PlayerPrefabPaths.Final, log);
            if (final == null || copy == null)
                return false;

            var skill = FindSkill(final.Root);
            if (skill == null) {
                log.Notes.Add($"최종본에 {PlayerParts.CopySkill}가 없어 복제 대상 연결을 건너뜀");
                return true;
            }

            var so = new SerializedObject(skill);
            var before = SerializedValueCopier.Leaves(skill);
            if (!Point(so, copy, log))
                return true;

            so.ApplyModifiedPropertiesWithoutUndo();
            SerializedValueCopier.Record(before, SerializedValueCopier.Leaves(skill), PlayerParts.CopySkill, log);
            final.Save();
            return true;
        }

        public static bool Point(SerializedObject skill, GameObject copy, MergeLog log) {
            var field = skill.FindProperty(PlayerParts.CopySkillPrefabField);
            if (field == null) {
                log.Notes.Add($"{PlayerParts.CopySkill}.{PlayerParts.CopySkillPrefabField} 필드가 없어 연결하지 않음");
                return false;
            }

            if (field.objectReferenceValue == copy)
                return false;

            field.objectReferenceValue = copy;
            return true;
        }

        private static Component FindSkill(GameObject root) {
            foreach (var component in root.GetComponentsInChildren<Component>(true))
                if (component != null && component.GetType().Name == PlayerParts.CopySkill)
                    return component;

            return null;
        }
    }
}
