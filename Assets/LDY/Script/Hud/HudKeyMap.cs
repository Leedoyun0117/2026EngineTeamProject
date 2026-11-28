using System;
using System.Linq;
using LDY.Hud.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Hud {
    public enum HudKey {
        Absorb,
        Interact,
        UseSkill,
        SelectTarget,
        DropSkill,
        SwitchControl
    }

    [Serializable]
    public sealed class HudKeyBinding {
        public HudKey key;
        [Tooltip("InputAction 에셋에 있는 키. 있으면 이 바인딩이 우선")]
        public InputActionReference action;
        [Tooltip("InputAction 에셋에 없는 키(코드에서 Keyboard.current로 읽는 키)의 바인딩 경로")]
        public string fallbackPath;

        public string Glyph() {
            var bound = action != null && action.action != null
                ? KeyGlyph.Pick(action.action.bindings.Select(b => b.effectivePath))
                : null;
            return bound ?? KeyGlyph.FromPath(fallbackPath) ?? KeyGlyph.Unknown;
        }
    }

    // HUD 키캡 글자의 출처. 키 글자는 코드에 두지 않고 여기 바인딩에서 읽는다.
    [CreateAssetMenu(fileName = "HudKeyMap", menuName = "LDY/HUD/Key Map")]
    public sealed class HudKeyMap : ScriptableObject {
        public HudKeyBinding[] bindings = Array.Empty<HudKeyBinding>();

        public string Glyph(HudKey key) {
            foreach (var binding in bindings)
                if (binding != null && binding.key == key)
                    return binding.Glyph();
            return KeyGlyph.Unknown;
        }
    }
}
