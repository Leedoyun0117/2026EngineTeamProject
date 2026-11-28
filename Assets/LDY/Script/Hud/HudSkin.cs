using System;
using LDY.Hud.Core;
using TMPro;
using UnityEngine;

namespace LDY.Hud {
    [Serializable]
    public sealed class HudAbilityEntry {
        public AbilityId id;
        public Sprite icon;
        public string displayName;
        public Color color = Color.white;
    }

    // HUD 글꼴, 능력 아이콘·이름·색, 문구. 프리팹 생성 메뉴와 런타임 프레젠터가 같이 참조한다.
    [CreateAssetMenu(fileName = "HudSkin", menuName = "LDY/HUD/Skin")]
    public sealed class HudSkin : ScriptableObject {
        [Header("Font")]
        public TMP_FontAsset font;
        public int fontSize = 10;
        public TMP_FontAsset thoughtFont;
        public int thoughtFontSize = 16;

        [Header("Ability")]
        public HudAbilityEntry[] abilities = Array.Empty<HudAbilityEntry>();
        public Sprite emptyIcon;
        public string emptyName = "(빈 슬롯)";

        [Header("Control Target")]
        public string mainLabel = "Cursor";
        public string cloneLabel = "Cursor (복사본)";

        [Header("Toast")]
        public Sprite toastInfo;
        public Sprite toastWarning;
        public Sprite toastError;
        [Min(0.1f)] public float toastSeconds = 2.5f;
        [Min(0)] public int toastMaxWaiting = 3;
        public string alreadyHoldingMessage = "이미 능력을 보유중입니다.";
        public string rejectedMessage = "흡수할 수 없습니다.";

        public HudAbilityEntry Find(AbilityId id) {
            foreach (var entry in abilities)
                if (entry != null && entry.id == id)
                    return entry;
            return null;
        }

        public Sprite ToastIcon(ToastKind kind) {
            switch (kind) {
                case ToastKind.Error: return toastError;
                case ToastKind.Warning: return toastWarning;
                default: return toastInfo;
            }
        }
    }
}
