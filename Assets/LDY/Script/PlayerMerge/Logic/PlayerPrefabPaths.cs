namespace LDY.PlayerMerge {
    public static class PlayerPrefabPaths {
        // 플레이어 프리팹 경로는 Final 한 곳에서만 정한다. LegacyShared는 병합 출처와 교체 대상 판별에만 쓴다.
        public const string Final = "Assets/prefab/PlayerMerged.prefab";
        public const string LegacyShared = "Assets/prefab/Player.prefab";
        public const string Copy = "Assets/prefab/PlayerCopy.prefab";
        public const string Jibin = "Assets/JJB/Prefab/Player.prefab";
        public const string Hty = "Assets/Main/02.Prefabs/Player/Player.prefab";
        public const string HtyCopy = "Assets/Main/02.Prefabs/Player/PlayerCopy.prefab";
        public const string DiffReport = "Assets/LDY/Debug/player_prefab_diff.md";
    }

    public static class PlayerArt {
        public const float PixelsPerUnit = 40f;
        public const int StageSortingOrder = 6;
    }
}
