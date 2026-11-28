using LDY.Hud.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud.HudEditor {
    // 하단 작업표시줄: 능력 슬롯, 실행/해제 키, 조작 대상.
    internal static class HudTaskbarSection {
        private const int KeyGap = 4;

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
            var use = Key(kit, bar, "UseKey", HudKey.UseSkill, "실행");
            var drop = Key(kit, bar, "DropKey", HudKey.DropSkill, "해제");
            var target = ControlTarget(kit, bar);
            KeyRow(kit, bar, slot, target, use, drop);

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

        // 버튼 크기는 KeyCapFitter, 위치는 KeyRowFitter가 정한다(슬롯 오른쪽 4px, 버튼 사이 4px).
        private static KeyButtonView Key(HudKit kit, RectTransform bar, string name, HudKey key, string caption) {
            if (kit.Ui(bar, name, out var button))
                HudKit.Place(button, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);

            var on = Face(kit, button, "Face", "hud_key", key, caption, HudKeyHint.Ink, out var onParts);
            var off = Face(kit, button, "FaceDisabled", "hud_key_disabled", key, caption, HudKeyHint.DisabledInk, out var offParts);
            var view = HudKit.Component<KeyButtonView>(button.gameObject);
            HudKit.Wire(view, "enabledFace", on);
            HudKit.Wire(view, "disabledFace", off);
            HudKeyHint.Fit(kit, button, KeyCapSpec.Button, onParts, offParts);
            return view;
        }

        private static GameObject Face(HudKit kit, RectTransform button, string name, string sprite, HudKey key,
            string caption, Color ink, out KeyCapParts parts) {
            if (kit.Ui(button, name, out var face)) {
                HudKit.Stretch(face, 0f, 0f, 0f, 0f);
                kit.Image(face, sprite);
            }

            parts = HudKeyHint.Build(kit, face, key, caption, ink);
            return face.gameObject;
        }

        private static void KeyRow(HudKit kit, RectTransform bar, AbilitySlotView slot, TMP_Text target, params KeyButtonView[] keys) {
            var row = HudKit.Component<KeyRowFitter>(bar.gameObject, out var added);
            HudKit.Wire(row, "after", slot.transform);
            HudKit.Wire(row, "limit", target.transform.parent);

            var so = new SerializedObject(row);
            var array = so.FindProperty("keys");
            if (array.arraySize == 0) {
                array.arraySize = keys.Length;
                for (var i = 0; i < keys.Length; i++)
                    array.GetArrayElementAtIndex(i).objectReferenceValue = keys[i].GetComponent<KeyCapFitter>();
            }
            if (added)
                so.FindProperty("gap").intValue = KeyGap;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!added)
                return;

            row.Fit();
            foreach (var key in keys) {
                var rect = (RectTransform)key.transform;
                kit.Log.Info($"키 버튼 배치 {key.name}: x={rect.anchoredPosition.x} y={rect.anchoredPosition.y} {rect.rect.width}x{rect.rect.height}");
            }
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
