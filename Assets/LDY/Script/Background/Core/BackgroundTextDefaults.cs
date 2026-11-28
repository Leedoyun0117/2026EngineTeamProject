using System;

namespace LDY.Background.Core {
    public static class BackgroundTextDefaults {
        private static readonly string[] Shown = {
            "window_title", "tab_0", "tab_1", "tab_2", "tab_3", "address_path", "side_title"
        };

        public static bool ShownByDefault(string id) => Array.IndexOf(Shown, id) >= 0;
    }
}
