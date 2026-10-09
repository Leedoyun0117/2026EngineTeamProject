using UnityEngine;

namespace LDY.Script
{
    public enum ScreenModeOption
    {
        FullScreen,
        Windowed
    }

    public interface IScreenModeSetting
    {
        ScreenModeOption Current { get; }
        void Apply(ScreenModeOption option);
    }

    // 화면 모드는 Screen.fullScreenMode에 바로 적용하고 PlayerPrefs에 저장한다. 사운드와는 무관하다.
    public class ScreenModeSetting : IScreenModeSetting
    {
        private const string Key = "Settings.ScreenMode";

        public ScreenModeOption Current =>
            Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ||
            Screen.fullScreenMode == FullScreenMode.FullScreenWindow
                ? ScreenModeOption.FullScreen
                : ScreenModeOption.Windowed;

        // 저장된 값이 있으면 시작할 때 한 번 적용한다 (다시 저장하지 않는다).
        public void ApplySaved()
        {
            if (PlayerPrefs.HasKey(Key))
                Set((ScreenModeOption)PlayerPrefs.GetInt(Key));
        }

        public void Apply(ScreenModeOption option)
        {
            Set(option);
            PlayerPrefs.SetInt(Key, (int)option);
            PlayerPrefs.Save();
        }

        private static void Set(ScreenModeOption option)
        {
            Screen.fullScreenMode = option == ScreenModeOption.FullScreen
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;
        }
    }
}
