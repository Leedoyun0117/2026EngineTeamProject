using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud.HudEditor {
    // 하단 작업표시줄: 능력 슬롯, 실행/해제 키, 조작 대상.
    internal static class HudTaskbarSection {
        public static void Build(HudKit kit, RectTransform canvas, UiWorldAnchor switchMarker) {
            if (kit.Ui(canvas, "Taskbar", out var bar)) {
                bar.anchorMin = Vector2.zero;
                bar.anchorMax = new Vector2(1f, 0f);
                bar.pivot = new Vector2(0.5f, 0f);
                bar.anchoredPosition = Vector2.zero;
                bar.sizeDelta = new Vector2(0f, 22f);
                kit.Image(bar, "hud_taskbar");
            }

            var slot = Slot(kit, bar);
            var use = Key(kit, bar, "UseKey", new Vector2(136f, 3f), HudKey.UseSkill, "실행");
            var drop = Key(kit, bar, "DropKey", new Vector2(184f, 3f), HudKey.DropSkill, "해제");
            var target = ControlTarget(kit, bar);

            var abilities = HudKit.Component<AbilitySlotPresenter>(bar.gameObject);
            HudKit.Wire(abilities, "skin", kit.Skin);
            HudKit.Wire(abilities, "slot", slot);
            HudKit.Wire(abilities, "useKey", use);
            HudKit.Wire(abilities, "dropKey", drop);

            var control = HudKit.Component<ControlTargetPresenter>(bar.gameObject);
            HudKit.Wire(control, "skin", kit.Skin);
            HudKit.Wire(control, "label", target);
            HudKit.Wire(control, "switchMarker", switchMarker);
        }

        private static AbilitySlotView Slot(HudKit kit, RectTransform bar) {
            if (kit.Ui(bar, "AbilitySlot", out var field)) {
                HudKit.Place(field, Vector2.zero, Vector2.zero, new Vector2(4f, 3f), new Vector2(128f, 16f));
                kit.Image(field, "hud_field_sunken");
            }

            if (kit.Ui(field, "Icon", out var icon)) {
                HudKit.Place(icon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(1f, 0f), new Vector2(16f, 16f));
                kit.Image(icon, "hud_icon_empty");
            }

            if (kit.Ui(field, "Label", out var label)) {
                HudKit.Stretch(label, 20f, 0f, 3f, 0f);
                kit.Text(label, kit.Skin.emptyName, HudKeyHint.Ink, TextAlignmentOptions.Left);
            }

            var view = HudKit.Component<AbilitySlotView>(field.gameObject);
            HudKit.Wire(view, "icon", icon.GetComponent<Image>());
            HudKit.Wire(view, "label", label.GetComponent<TMP_Text>());
            return view;
        }

        private static KeyButtonView Key(HudKit kit, RectTransform bar, string name, Vector2 position, HudKey key, string caption) {
            if (kit.Ui(bar, name, out var button))
                HudKit.Place(button, Vector2.zero, Vector2.zero, position, new Vector2(44f, 16f));

            var on = Face(kit, button, "Face", "hud_key", key, caption, HudKeyHint.Ink);
            var off = Face(kit, button, "FaceDisabled", "hud_key_disabled", key, caption, HudKeyHint.DisabledInk);
            var view = HudKit.Component<KeyButtonView>(button.gameObject);
            HudKit.Wire(view, "enabledFace", on);
            HudKit.Wire(view, "disabledFace", off);
            return view;
        }

        private static GameObject Face(HudKit kit, RectTransform button, string name, string sprite, HudKey key,
            string caption, Color ink) {
            if (kit.Ui(button, name, out var face)) {
                HudKit.Stretch(face, 0f, 0f, 0f, 0f);
                kit.Image(face, sprite);
                HudKit.Row(face, new RectOffset(3, 4, 3, 3), 3f, TextAnchor.MiddleCenter);
            }

            HudKeyHint.Build(kit, face, key, caption, ink);
            return face.gameObject;
        }

        private static TMP_Text ControlTarget(HudKit kit, RectTransform bar) {
            if (kit.Ui(bar, "ControlTarget", out var field)) {
                HudKit.Place(field, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-4f, 3f), new Vector2(88f, 16f));
                kit.Image(field, "hud_field_sunken");
            }

            if (kit.Ui(field, "Icon", out var icon)) {
                HudKit.Place(icon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(3f, 0f), new Vector2(12f, 14f));
                kit.Image(icon, "hud_icon_target_cursor");
            }

            if (kit.Ui(field, "Label", out var label)) {
                HudKit.Stretch(label, 18f, 0f, 3f, 0f);
                kit.Text(label, kit.Skin.mainLabel, HudKeyHint.Ink, TextAlignmentOptions.Left);
            }

            return label.GetComponent<TMP_Text>();
        }
    }
}
