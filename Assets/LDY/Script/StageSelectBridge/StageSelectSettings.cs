using LDY.StageSelect;
using UnityEngine;

namespace LDY.Script {
    // 선택 화면 설정.ini 창을 타이틀 설정과 같은 구현(MixerAudioVolume, ScreenModeSetting)에 잇는다. 저장 키도 같다.
    [RequireComponent(typeof(MixerAudioVolume))]
    public sealed class StageSelectSettings : MonoBehaviour, IStageSelectSettings {
        private readonly ScreenModeSetting _screen = new ScreenModeSetting();
        private IAudioVolume _volume;

        private void Awake() {
            _volume = GetComponent<MixerAudioVolume>();
            _screen.ApplySaved();
        }

        public float GetVolume(VolumeKind kind) => _volume.Get(Channel(kind));

        public void SetVolume(VolumeKind kind, float value) => _volume.Set(Channel(kind), value);

        public bool IsFullScreen => _screen.Current == ScreenModeOption.FullScreen;

        public void SetFullScreen(bool fullScreen) =>
            _screen.Apply(fullScreen ? ScreenModeOption.FullScreen : ScreenModeOption.Windowed);

        private static VolumeChannel Channel(VolumeKind kind) {
            switch (kind) {
                case VolumeKind.Bgm: return VolumeChannel.Bgm;
                case VolumeKind.Sfx: return VolumeChannel.Sfx;
                default: return VolumeChannel.Master;
            }
        }
    }
}
