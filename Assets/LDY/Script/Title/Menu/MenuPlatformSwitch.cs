using System.Collections.Generic;

namespace LDY.Script
{
    // 창 뒤에 안 보이는 발판에 걸리지 않게 창이 열려 있는 동안 끔
    public class MenuPlatformSwitch
    {
        private readonly IReadOnlyList<MenuIcon> _icons;

        public MenuPlatformSwitch(IReadOnlyList<MenuIcon> icons)
        {
            _icons = icons;
        }

        public void SetEnabled(bool enabled)
        {
            for (int i = 0; i < _icons.Count; i++)
                _icons[i].SetPlatformEnabled(enabled);
        }
    }
}
