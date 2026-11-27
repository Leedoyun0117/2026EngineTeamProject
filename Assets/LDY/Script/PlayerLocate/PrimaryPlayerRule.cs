using System;
using System.Collections.Generic;

namespace LDY.PlayerLocate {
    // "Player" 태그 후보 하나. 판별에 필요한 값만 담아 Unity 없이 테스트한다.
    public readonly struct PlayerCandidate {
        public PlayerCandidate(string name, bool hasActiveInput) {
            Name = name;
            HasActiveInput = hasActiveInput;
        }

        public string Name { get; }

        // PlayerInput이 있고 켜져 있다(isActiveAndEnabled)
        public bool HasActiveInput { get; }
    }

    public enum PrimaryReason {
        None,
        ActiveInput,
        Name,
        Fallback
    }

    public readonly struct PrimaryPick {
        public static readonly PrimaryPick None = new PrimaryPick(-1, PrimaryReason.None);

        public PrimaryPick(int index, PrimaryReason reason) {
            Index = index;
            Reason = reason;
        }

        public int Index { get; }
        public PrimaryReason Reason { get; }
        public bool Found => Index >= 0;

        // 규칙으로 하나를 가리지 못해 남은 후보 중 첫 번째를 골랐다(호출 쪽이 경고를 남긴다).
        public bool IsAmbiguous => Reason == PrimaryReason.Fallback;
    }

    // "Player" 태그가 여럿일 때(복사 플레이어 등) 본체 하나를 고르는 규칙.
    // 1) 활성 PlayerInput이 있는 것 → 2) 이름에 Copy/Clone이 없는 것 순서로 좁히고,
    // 하나로 좁혀지지 않으면 남은 후보 중 첫 번째(Fallback)를 고른다.
    public static class PrimaryPlayerRule {
        private static readonly string[] CopyMarkers = { "Copy", "Clone" };

        public static PrimaryPick Pick(IReadOnlyList<PlayerCandidate> candidates) {
            if (candidates == null || candidates.Count == 0) return PrimaryPick.None;

            var pool = new List<int>(candidates.Count);
            for (var i = 0; i < candidates.Count; i++) pool.Add(i);

            var byInput = pool.FindAll(i => candidates[i].HasActiveInput);
            if (byInput.Count == 1) return new PrimaryPick(byInput[0], PrimaryReason.ActiveInput);
            if (byInput.Count > 1) pool = byInput;

            var byName = pool.FindAll(i => !LooksLikeCopy(candidates[i].Name));
            if (byName.Count == 1) return new PrimaryPick(byName[0], PrimaryReason.Name);
            if (byName.Count > 1) pool = byName;

            return new PrimaryPick(pool[0], PrimaryReason.Fallback);
        }

        // 골처럼 본체만 반응해야 하는 곳에서 쓴다. 태그 후보가 없으면 가릴 수 없으므로 통과시킨다.
        public static bool IsPrimary(IReadOnlyList<PlayerCandidate> candidates, int index) {
            if (candidates == null || candidates.Count == 0) return true;
            return index >= 0 && Pick(candidates).Index == index;
        }

        public static bool LooksLikeCopy(string name) {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (var marker in CopyMarkers)
                if (name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }
    }
}
