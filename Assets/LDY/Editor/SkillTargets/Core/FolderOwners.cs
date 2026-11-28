namespace LDY.SkillTargets.Core {
    public enum FolderOwner { Other, Ldy, Main, Hes, Jjb }

    public enum ApplyAccess { ReadOnly, Direct, NeedsConfirmation }

    public static class FolderOwners {
        public static FolderOwner Resolve(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return FolderOwner.Other;
            var parts = assetPath.Replace('\\', '/').Split('/');
            if (parts.Length < 3 || parts[0] != "Assets") return FolderOwner.Other;
            return parts[1] switch {
                "LDY" => FolderOwner.Ldy,
                "Main" => FolderOwner.Main,
                "HES" => FolderOwner.Hes,
                "JJB" => FolderOwner.Jjb,
                _ => FolderOwner.Other
            };
        }

        public static ApplyAccess Access(FolderOwner owner) => owner switch {
            FolderOwner.Ldy => ApplyAccess.Direct,
            FolderOwner.Main => ApplyAccess.Direct,
            FolderOwner.Hes => ApplyAccess.NeedsConfirmation,
            FolderOwner.Jjb => ApplyAccess.NeedsConfirmation,
            _ => ApplyAccess.ReadOnly
        };
    }
}
