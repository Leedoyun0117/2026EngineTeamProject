using System.Collections.Generic;

namespace LDY.Hud.Core {
    // InputSystem 바인딩 경로("<Keyboard>/e")에서 키캡 글자("E")를 만든다.
    public static class KeyGlyph {
        public const string Unknown = "?";
        private const string Keyboard = "Keyboard";

        private static readonly Dictionary<string, string> Names = new Dictionary<string, string> {
            { "space", "Space" },
            { "tab", "Tab" },
            { "escape", "Esc" },
            { "enter", "Enter" },
            { "backspace", "Back" },
            { "leftShift", "Shift" },
            { "rightShift", "Shift" },
            { "shift", "Shift" },
            { "leftCtrl", "Ctrl" },
            { "rightCtrl", "Ctrl" },
            { "ctrl", "Ctrl" },
            { "leftAlt", "Alt" },
            { "rightAlt", "Alt" },
            { "alt", "Alt" },
            { "upArrow", "Up" },
            { "downArrow", "Down" },
            { "leftArrow", "Left" },
            { "rightArrow", "Right" },
            { "leftButton", "LMB" },
            { "rightButton", "RMB" },
            { "middleButton", "MMB" }
        };

        public static string FromPath(string path) {
            if (!TrySplit(path, out _, out var control))
                return null;

            if (Names.TryGetValue(control, out var name))
                return name;
            if (control.StartsWith("numpad") && control.Length == 7)
                return "Num" + control.Substring(6);
            if (control.Length == 1)
                return control.ToUpperInvariant();

            return char.ToUpperInvariant(control[0]) + control.Substring(1);
        }

        // 키보드 바인딩을 먼저 고르고, 없으면 처음 읽히는 바인딩.
        public static string Pick(IEnumerable<string> paths) {
            if (paths == null)
                return null;

            string fallback = null;
            foreach (var path in paths) {
                if (!TrySplit(path, out var device, out _))
                    continue;

                var glyph = FromPath(path);
                if (device == Keyboard)
                    return glyph;
                fallback ??= glyph;
            }

            return fallback;
        }

        private static bool TrySplit(string path, out string device, out string control) {
            device = null;
            control = null;
            if (string.IsNullOrEmpty(path) || path[0] != '<')
                return false;

            var close = path.IndexOf('>');
            var slash = path.LastIndexOf('/');
            if (close < 2 || slash <= close || slash == path.Length - 1)
                return false;

            device = path.Substring(1, close - 1);
            control = path.Substring(slash + 1);
            return true;
        }
    }
}
