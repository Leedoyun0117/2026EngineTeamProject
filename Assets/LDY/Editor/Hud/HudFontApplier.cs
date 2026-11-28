using TMPro;
using UnityEditor;
using UnityEngine;

namespace LDY.Hud.HudEditor {
    // HudSkin의 글꼴·크기를 프리팹의 모든 글자에 다시 적용한다(생각.txt 본문만 thoughtFont). 명시적으로 고를 때만 덮어쓴다.
    internal static class HudFontApplier {
        public static void Run(HudLog log) {
            var skin = AssetDatabase.LoadAssetAtPath<HudSkin>(HudPaths.Skin);
            if (skin == null || AssetDatabase.LoadAssetAtPath<GameObject>(HudPaths.Prefab) == null) {
                log.Warn("HudSkin 또는 HUD.prefab 없음");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(HudPaths.Prefab);
            try {
                var thoughtBody = ThoughtBody(root);
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true)) {
                    var isThought = text == thoughtBody;
                    text.font = isThought ? skin.thoughtFont : skin.font;
                    text.fontSize = isThought ? skin.thoughtFontSize : skin.fontSize;
                    log.Info($"{text.name}: {(text.font != null ? text.font.name : "없음")} {text.fontSize}");
                }

                PrefabUtility.SaveAsPrefabAsset(root, HudPaths.Prefab);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Object ThoughtBody(GameObject root) {
            var window = root.GetComponentInChildren<ThoughtWindow>(true);
            return window != null ? new SerializedObject(window).FindProperty("body").objectReferenceValue : null;
        }
    }
}
