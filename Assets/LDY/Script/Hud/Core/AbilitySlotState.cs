using System;

namespace LDY.Hud.Core {
    // 슬롯에 보일 능력. 능력은 있지만 HUD 목록에 대응이 없으면 Unmapped(원래 이름 보관).
    public readonly struct AbilitySlotState : IEquatable<AbilitySlotState> {
        public static readonly AbilitySlotState Empty = default;

        public AbilityId Id { get; }
        public string SourceName { get; }
        public bool HasAbility { get; }

        private AbilitySlotState(AbilityId id, string sourceName, bool hasAbility) {
            Id = id;
            SourceName = sourceName;
            HasAbility = hasAbility;
        }

        public bool IsMapped => HasAbility && Id != AbilityId.None;
        public bool IsUnmapped => HasAbility && Id == AbilityId.None;

        public static AbilitySlotState Of(AbilityId id) =>
            id == AbilityId.None ? Empty : new AbilitySlotState(id, id.ToString(), true);

        public static AbilitySlotState Unmapped(string sourceName) =>
            new AbilitySlotState(AbilityId.None, sourceName ?? string.Empty, true);

        public bool Equals(AbilitySlotState other) =>
            Id == other.Id && HasAbility == other.HasAbility && SourceName == other.SourceName;

        public override bool Equals(object obj) => obj is AbilitySlotState other && Equals(other);
        public override int GetHashCode() => ((int)Id * 397) ^ (HasAbility ? 1 : 0) ^ (SourceName?.GetHashCode() ?? 0);
        public static bool operator ==(AbilitySlotState a, AbilitySlotState b) => a.Equals(b);
        public static bool operator !=(AbilitySlotState a, AbilitySlotState b) => !a.Equals(b);
        public override string ToString() => HasAbility ? $"{Id}({SourceName})" : "Empty";
    }
}
