namespace LDY.StageSelect {
    public static class StageSelectText {
        public const string WindowTitle = @"C:\GAME\STAGES - 스테이지 선택";
        public const string TaskTitle = "스테이지 선택";
        public const string MenuBar = "파일(F)   편집(E)   보기(V)   도움말(H)";
        public const string AddressLabel = "주소(D)";
        public const string Start = "시작";

        public const string MyComputer = "내 컴퓨터";
        public const string LocalDisk = "로컬 디스크 (C:)";
        public const string Game = "GAME";
        public const string Stages = "STAGES";
        public const string SettingsFile = "설정.ini";

        public const string All = "전체";
        public const string IncompleteTab = "미완료";
        public const string CompleteTab = "완료";

        public const string Cleared = "완료";
        public const string Incomplete = "미완료";
        public const string Locked = "잠김";
        public const string NotReady = "준비 중";

        public const string AccessDenied = "액세스가 거부되었습니다.";
        public const string RunFirstFormat = "{0}를 먼저 실행하십시오.";
        public const string FileNotFound = "파일을 찾을 수 없습니다.";
        public const string Ok = "확인";

        public const string SettingsTitle = "설정.ini - 설정";
        public const string MasterVolume = "전체 음량";
        public const string BgmVolume = "배경음";
        public const string SfxVolume = "효과음";
        public const string ScreenMode = "화면 모드";
        public const string FullScreen = "전체 화면";
        public const string Windowed = "창 모드";
        public const string SettingsMissing = "설정을 불러올 수 없습니다.";

        public static string Summary(int total, int cleared, int locked) =>
            $"{total}개 개체   {cleared}개 완료   {locked}개 잠김";
    }
}
