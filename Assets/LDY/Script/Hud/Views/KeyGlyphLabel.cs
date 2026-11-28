using TMPro;
using UnityEngine;

namespace LDY.Hud {
    // 키캡 글자를 HudKeyMap의 바인딩에서 채운다.
    public sealed class KeyGlyphLabel : MonoBehaviour {
        [SerializeField] private HudKeyMap keyMap;
        [SerializeField] private HudKey key;
        [SerializeField] private TMP_Text label;

        private void Start() {
            if (keyMap == null || label == null) {
                HudWarnings.Once("keymap:" + key, $"{name}: HudKeyMap 또는 글자 칸이 비어 있어 키 글자를 못 채움", this);
                return;
            }

            label.text = keyMap.Glyph(key);
        }
    }
}
