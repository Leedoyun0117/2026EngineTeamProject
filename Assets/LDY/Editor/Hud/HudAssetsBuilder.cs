using System.Linq;
using LDY.Hud.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LDY.Hud.HudEditor {
    // HudSkin·HudKeyMap을 만들고 비어 있는 칸만 채운다. 사람이 고친 값은 덮어쓰지 않는다.
    internal static class HudAssetsBuilder {
        private static readonly (AbilityId id, string icon, string name, string color)[] Abilities = {
            (AbilityId.Pause, "hud_icon_pause", "일시정지", "#1FA3BD"),
            (AbilityId.Cut, "hud_icon_cut", "잘라내기", "#B5552E"),
            (AbilityId.Drag, "hud_icon_drag", "드래그", "#D9831A"),
            (AbilityId.Alpha, "hud_icon_alpha", "투명도", "#7F8B97"),
            (AbilityId.Light, "hud_icon_light", "밝기", "#C9A227"),
            (AbilityId.Tab, "hud_icon_tab", "탭 전환", "#8657D4"),
            (AbilityId.Copy, "hud_icon_copy", "복사 붙여넣기", "#24A06F"),
            (AbilityId.Zoom, "hud_icon_zoom", "확대·축소", "#D14B6B")
        };

        // 태영 스킬 키는 InputAction 에셋이 아니라 코드(Keyboard.current)에 있어 경로를 데이터로 둔다.
        private static readonly (HudKey key, string action, string path)[] Keys = {
            (HudKey.Absorb, "Absorb", null),
            (HudKey.Interact, "Interact", null),
            (HudKey.UseSkill, null, "<Keyboard>/y"),
            (HudKey.SelectTarget, null, "<Keyboard>/r"),
            (HudKey.DropSkill, null, "<Keyboard>/q"),
            (HudKey.SwitchControl, null, "<Keyboard>/y")
        };

        public static void Run(HudLog log) {
            var sprites = HudSprites.Load(log);
            if (sprites == null)
                return;

            HudPaths.EnsureFolder(HudPaths.Data);
            BuildSkin(sprites, log);
            BuildKeyMap(log);
            AssetDatabase.SaveAssets();
        }

        private static void BuildSkin(HudSprites sprites, HudLog log) {
            var skin = LoadOrCreate<HudSkin>(HudPaths.Skin, log);
            skin.font = Keep(skin.font, () => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HudPaths.Font), "font", log);
            skin.thoughtFont = Keep(skin.thoughtFont, () => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HudPaths.ThoughtFont), "thoughtFont", log);
            skin.emptyIcon = Keep(skin.emptyIcon, () => sprites.Single("hud_icon_empty"), "emptyIcon", log);
            skin.toastInfo = Keep(skin.toastInfo, () => sprites.Single("hud_icon_info"), "toastInfo", log);
            skin.toastWarning = Keep(skin.toastWarning, () => sprites.Single("hud_icon_warn"), "toastWarning", log);
            skin.toastError = Keep(skin.toastError, () => sprites.Single("hud_icon_error"), "toastError", log);

            var entries = skin.abilities.Where(e => e != null).ToList();
            foreach (var (id, icon, name, color) in Abilities) {
                var entry = entries.FirstOrDefault(e => e.id == id);
                if (entry == null) {
                    ColorUtility.TryParseHtmlString(color, out var parsed);
                    entry = new HudAbilityEntry { id = id, displayName = name, color = parsed };
                    entries.Add(entry);
                    log.Info($"능력 추가 {id} {name} {color}");
                }

                if (entry.icon == null && AssetDatabase.LoadAssetAtPath<Sprite>($"{HudPaths.Sprites}/{icon}.png") is Sprite sprite)
                    entry.icon = sprite;
                if (entry.icon == null)
                    log.Warn($"{id} 아이콘 없음({icon}.png). 실행 중에는 빈 아이콘으로 보인다");
            }

            skin.abilities = entries.ToArray();
            EditorUtility.SetDirty(skin);
        }

        private static void BuildKeyMap(HudLog log) {
            var map = LoadOrCreate<HudKeyMap>(HudPaths.KeyMap, log);
            var references = AssetDatabase.LoadAllAssetsAtPath(HudPaths.PlayerActions).OfType<InputActionReference>().ToArray();
            var bindings = map.bindings.Where(b => b != null).ToList();
            foreach (var (key, action, path) in Keys) {
                var binding = bindings.FirstOrDefault(b => b.key == key);
                if (binding == null) {
                    binding = new HudKeyBinding { key = key, fallbackPath = path };
                    bindings.Add(binding);
                    log.Info($"키 추가 {key}");
                }

                if (binding.action == null && action != null)
                    binding.action = references.FirstOrDefault(r => r.action != null && r.action.name == action);
                if (action != null && binding.action == null)
                    log.Warn($"{HudPaths.PlayerActions}에서 {action} 액션을 못 찾음");
                log.Info($"{key} -> [{binding.Glyph()}]");
            }

            map.bindings = bindings.ToArray();
            EditorUtility.SetDirty(map);
        }

        private static T LoadOrCreate<T>(string path, HudLog log) where T : ScriptableObject {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            log.Info("만듦 " + path);
            return asset;
        }

        private static T Keep<T>(T current, System.Func<T> load, string label, HudLog log) where T : Object {
            if (current != null)
                return current;

            var loaded = load();
            if (loaded == null)
                log.Warn($"{label} 비어 있음");
            return loaded;
        }
    }
}
