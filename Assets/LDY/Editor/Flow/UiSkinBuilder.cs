using System.Collections.Generic;
using System.IO;
using LDY.Background.Core;
using LDY.Flow;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LDY.FlowEditor {
    // stage_select_ui / ingame_hud png를 Point·PPU100·9-slice로 임포트하고 UiSkin에 연결한다.
    internal static class UiSkinBuilder {
        private const float PixelsPerUnit = 100f;

        public static UiSkin Build(FlowLog log) {
            var borders = ReadBorders(FlowPaths.SelectSprites, log);
            foreach (var pair in ReadBorders(FlowPaths.HudSprites, log)) borders[pair.Key] = pair.Value;

            var skin = AssetDatabase.LoadAssetAtPath<UiSkin>(FlowPaths.Skin);
            var created = skin == null;
            if (created) {
                FlowPaths.EnsureFolder(FlowPaths.Data);
                skin = ScriptableObject.CreateInstance<UiSkin>();
                AssetDatabase.CreateAsset(skin, FlowPaths.Skin);
            }

            Sprite Select(string name) => Load(FlowPaths.SelectSprites, name, borders, log);
            Sprite Hud(string name) => Load(FlowPaths.HudSprites, name, borders, log);

            ApplyFont(skin, log);
            skin.panelRaised = Select("sel_panel_raised");
            skin.windowFrame = Hud("hud_window_frame");
            skin.titlebar = Hud("hud_window_titlebar");
            skin.fieldSunken = Hud("hud_field_sunken");
            skin.button = Hud("hud_key");
            skin.buttonPressed = Hud("hud_key_pressed");
            skin.titleMin = Select("sel_titlebtn_min");
            skin.titleMax = Select("sel_titlebtn_max");
            skin.titleClose = Select("sel_titlebtn_close");
            skin.progressSegment = Select("sel_progress_seg");
            skin.taskbar = Hud("hud_taskbar");
            skin.startIcon = Select("sel_start_icon");
            skin.tab = Select("sel_tab");
            skin.tabActive = Select("sel_tab_active");
            skin.thumbFrame = Select("sel_thumb_frame");
            skin.focusDotted = Select("sel_focus_dotted");
            skin.folder = Select("sel_folder");
            skin.folderOpen = Select("sel_folder_open");
            skin.folderLocked = Select("sel_folder_locked");
            skin.computer = Select("sel_computer");
            skin.disk = Select("sel_disk");
            skin.ini = Select("sel_ini");
            skin.exe = Select("sel_exe");
            skin.missing = Select("sel_missing");
            skin.lockBadge = Select("sel_lock");
            skin.checkBadge = Select("sel_check_badge");
            skin.iconError = Hud("hud_icon_error");
            skin.iconWarn = Hud("hud_icon_warn");
            skin.iconInfo = Hud("hud_icon_info");

            EditorUtility.SetDirty(skin);
            log.Info((created ? "만듦 " : "갱신 ") + FlowPaths.Skin);
            return skin;
        }

        // 탐색기 화면·로딩 창의 글자는 Galmuri9 하나. 크기는 폰트 픽셀 격자(10px)와 같아야 선명하다.
        public const int FontSize = 10;

        private static void ApplyFont(UiSkin skin, FlowLog log) {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FlowPaths.FontAsset);
            if (font == null) {
                log.Warn($"{FlowPaths.FontAsset} 없음. LDY/Fonts/Build Pixel Font Assets 후 다시 실행. 스킨 폰트는 그대로 둠");
                return;
            }

            if (skin.font != font || skin.fontSize != FontSize)
                log.Info($"폰트 {Describe(skin.font)}/{skin.fontSize} -> {font.name}/{FontSize}");
            skin.font = font;
            skin.fontSize = FontSize;
            if ((int)font.faceInfo.pointSize != FontSize)
                log.Warn($"{font.name} 샘플링 크기 {font.faceInfo.pointSize} != {FontSize}. 픽셀이 번진다");
        }

        private static string Describe(Object asset) => asset != null ? asset.name : "없음";

        private static Sprite Load(string folder, string name, Dictionary<string, Vector4> borders, FlowLog log) {
            var path = $"{folder}/{name}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) {
                log.Warn("스프라이트 없음 " + path);
                return null;
            }

            borders.TryGetValue(name, out var border);
            if (Configure(importer, border)) {
                importer.SaveAndReimport();
                log.Info("임포트 설정 " + path);
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static bool Configure(TextureImporter importer, Vector4 border) {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            var changed = settings.spriteMeshType != SpriteMeshType.FullRect
                          || importer.textureType != TextureImporterType.Sprite
                          || importer.spriteImportMode != SpriteImportMode.Single
                          || !Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit)
                          || importer.filterMode != FilterMode.Point
                          || importer.textureCompression != TextureImporterCompression.Uncompressed
                          || importer.mipmapEnabled
                          || importer.spriteBorder != border;
            if (!changed) return false;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.spriteBorder = border;
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            return true;
        }

        // border_LBRT [L,B,R,T] -> spriteBorder (x=L, y=B, z=R, w=T)
        private static Dictionary<string, Vector4> ReadBorders(string folder, FlowLog log) {
            var result = new Dictionary<string, Vector4>();
            var path = folder + "/manifest.json";
            if (!File.Exists(path)) {
                log.Warn("manifest 없음 " + path);
                return result;
            }

            if (!(JsonParser.Parse(File.ReadAllText(path)) is Dictionary<string, object> root)) return result;
            foreach (var pair in root) {
                if (!(pair.Value is Dictionary<string, object> spec)) continue;
                if (!spec.TryGetValue("border_LBRT", out var raw) || !(raw is List<object> values) || values.Count != 4)
                    continue;
                result[pair.Key] = new Vector4(Number(values[0]), Number(values[1]), Number(values[2]), Number(values[3]));
            }

            return result;
        }

        private static float Number(object value) => value is double d ? (float)d : 0f;
    }
}
