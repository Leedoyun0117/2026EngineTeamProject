using System.Collections.Generic;

namespace LDY.Hud.Core {
    // 태영 SkillState 이름 -> HUD 능력. 대응이 없으면 Unmapped로 돌려 빈 아이콘 + 경고로 처리한다.
    public static class SkillStateMap {
        public const string NoSkill = "None";

        private static readonly Dictionary<string, AbilityId> Table = new Dictionary<string, AbilityId> {
            { "TimeStop", AbilityId.Pause },
            { "Cutting", AbilityId.Cut },
            { "Drag", AbilityId.Drag },
            { "Opacity", AbilityId.Alpha },
            { "Luminosity", AbilityId.Light },
            { "Tp", AbilityId.Tab },
            { "Copy", AbilityId.Copy },
            { "ReSizeObj", AbilityId.Zoom }
        };

        public static IReadOnlyDictionary<string, AbilityId> Entries => Table;

        public static bool TryMap(string skillStateName, out AbilityId id) {
            id = AbilityId.None;
            return skillStateName != null && Table.TryGetValue(skillStateName, out id);
        }

        public static AbilitySlotState Resolve(string skillStateName) {
            if (string.IsNullOrEmpty(skillStateName) || skillStateName == NoSkill)
                return AbilitySlotState.Empty;

            return TryMap(skillStateName, out var id) ? AbilitySlotState.Of(id) : AbilitySlotState.Unmapped(skillStateName);
        }
    }
}
