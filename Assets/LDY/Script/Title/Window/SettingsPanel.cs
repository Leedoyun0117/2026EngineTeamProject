using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 설정창: 음량 슬라이더 3개와 화면 모드 드롭다운. 값은 바꾸는 즉시 적용되고 저장은 각 구현체가 맡는다.
    public class SettingsPanel : WindowPanel
    {
        private IAudioVolume _volume;
        private IScreenModeSetting _screenMode;
        private SliderWidget _master;
        private SliderWidget _bgm;
        private SliderWidget _sfx;
        private DropdownWidget _screen;

        protected override string TitleText => WindowText.SettingTitle;
        protected override string ControlHint => WindowText.SettingControlHint;

        public void Bind(WindowServices services, IAudioVolume volume, IScreenModeSetting screenMode)
        {
            Bind(services);
            _volume = volume;
            _screenMode = screenMode;
        }

        protected override void BuildBody(WindowKit kit, Transform root, List<WindowWidget> widgets)
        {
            Vector2 rowX = new Vector2(WindowStyle.InnerLeft, WindowStyle.InnerRight);
            float top = WindowStyle.SliderTop0;

            _master = AddSlider(kit, root, widgets, WindowText.MasterVolume, "Slider_Master", top, rowX, VolumeChannel.Master);
            _bgm = AddSlider(kit, root, widgets, WindowText.BgmVolume, "Slider_Bgm", top - WindowStyle.RowSpacing, rowX,
                VolumeChannel.Bgm);
            _sfx = AddSlider(kit, root, widgets, WindowText.SfxVolume, "Slider_Sfx", top - WindowStyle.RowSpacing * 2f, rowX,
                VolumeChannel.Sfx);

            float dropdownTop = top - WindowStyle.RowSpacing * 3f;
            kit.Platform("Ledge_ScreenMode", root, rowX.x, rowX.y, dropdownTop, true);
            _screen = new DropdownWidget(kit, root, WindowText.ScreenMode, dropdownTop + WindowStyle.RowCenterAbovePlatform,
                WindowStyle.TrackX, dropdownTop, rowX, new[] { WindowText.FullScreen, WindowText.Windowed },
                (int)_screenMode.Current, index => _screenMode.Apply((ScreenModeOption)index));
            widgets.Add(_screen);
        }

        // 열 때마다 현재 값을 읽어 표시한다. 다른 곳에서 바뀐 값도 따라간다.
        protected override void OnWindowOpened()
        {
            _master.SetValue(_volume.Get(VolumeChannel.Master), false);
            _bgm.SetValue(_volume.Get(VolumeChannel.Bgm), false);
            _sfx.SetValue(_volume.Get(VolumeChannel.Sfx), false);
            _screen.SetIndex((int)_screenMode.Current);
        }

        private SliderWidget AddSlider(WindowKit kit, Transform root, List<WindowWidget> widgets, string label,
            string name, float platformTop, Vector2 rowX, VolumeChannel channel)
        {
            kit.Platform("Ledge_" + name, root, rowX.x, rowX.y, platformTop, true);
            var slider = new SliderWidget(kit, root, label, name, platformTop + WindowStyle.RowCenterAbovePlatform,
                WindowStyle.TrackX, platformTop, rowX, _volume.Get(channel), value => _volume.Set(channel, value));
            widgets.Add(slider);
            return slider;
        }
    }
}
