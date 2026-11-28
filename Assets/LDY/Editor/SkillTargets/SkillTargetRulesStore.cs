using UnityEditor;
using UnityEngine;

namespace LDY.SkillTargetsEditor {
    internal static class SkillTargetRulesStore {
        private const string Folder = "Assets/LDY/Data/SkillTargets";
        public const string Path = Folder + "/SkillTargetRules.asset";

        public static SkillTargetRules Ensure() {
            var rules = AssetDatabase.LoadAssetAtPath<SkillTargetRules>(Path);
            if (rules != null) return rules;

            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/LDY/Data", "SkillTargets");
            rules = ScriptableObject.CreateInstance<SkillTargetRules>();
            rules.Seed(SkillTargetSeed.Abilities(), SkillTargetSeed.Targets());
            AssetDatabase.CreateAsset(rules, Path);
            AssetDatabase.SaveAssets();
            Debug.Log("[SkillTargets] 규칙 표 만듦 " + Path);
            return rules;
        }
    }
}
