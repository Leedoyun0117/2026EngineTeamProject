namespace LDY.StageSelect {
    public enum VolumeKind {
        Master,
        Bgm,
        Sfx
    }

    // 설정.ini 창이 읽고 쓰는 값. 실제 저장(PlayerPrefs, 믹서)은 타이틀과 같은 구현을 어댑터가 연결한다.
    public interface IStageSelectSettings {
        float GetVolume(VolumeKind kind);
        void SetVolume(VolumeKind kind, float value);
        bool IsFullScreen { get; }
        void SetFullScreen(bool fullScreen);
    }
}
