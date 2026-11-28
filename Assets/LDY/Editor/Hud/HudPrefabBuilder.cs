using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud.HudEditor {
    // HUD.prefab이 없으면 만들고, 있으면 열어서 이름으로 없는 요소만 추가한다. 이미 있는 요소의 위치·크기·글꼴·색은 건드리지 않는다.
    internal static class HudPrefabBuilder {
        private const int CanvasSortingOrder = 100;
        private static readonly Vector2 ReferenceResolution = new Vector2(640f, 360f);

        public static void Run(HudLog log) {
            var skin = AssetDatabase.LoadAssetAtPath<HudSkin>(HudPaths.Skin);
            var keys = AssetDatabase.LoadAssetAtPath<HudKeyMap>(HudPaths.KeyMap);
            var sprites = HudSprites.Load(log);
            if (skin == null || keys == null || sprites == null) {
                log.Warn("HudSkin/HudKeyMap/manifest 없음. 2. Build Skin And Key Map 먼저 실행");
                return;
            }

            HudPaths.EnsureFolder(HudPaths.PrefabFolder);
            var exists = AssetDatabase.LoadAssetAtPath<GameObject>(HudPaths.Prefab) != null;
            var root = exists ? PrefabUtility.LoadPrefabContents(HudPaths.Prefab) : new GameObject("HUD");
            log.Info(exists ? "기존 프리팹 보강 " + HudPaths.Prefab : "새 프리팹 " + HudPaths.Prefab);
            try {
                Build(new HudKit(skin, keys, sprites, log), root);
                PrefabUtility.SaveAsPrefabAsset(root, HudPaths.Prefab);
            }
            finally {
                if (exists)
                    PrefabUtility.UnloadPrefabContents(root);
                else
                    Object.DestroyImmediate(root);
            }
        }

        private static void Build(HudKit kit, GameObject root) {
            var hud = HudKit.Component<HudRoot>(root);
            var created = Canvas(kit, root.transform, out var canvas);
            var toasts = HudOverlaySection.Toasts(kit, canvas);
            var thought = HudOverlaySection.Thought(kit, canvas);
            var switchMarker = HudWorldSection.Build(kit, root.transform, canvas, toasts);
            HudTaskbarSection.Build(kit, canvas, switchMarker);
            if (created)
                DrawOrder(canvas, "WorldLayer", "Taskbar", "Thought", "Toasts");

            var feeder = HudKit.Component<HudDebugFeeder>(root);
            HudKit.Wire(feeder, "root", hud);
            HudKit.Wire(feeder, "toasts", toasts);
            HudKit.Wire(feeder, "thought", thought);
        }

        private static bool Canvas(HudKit kit, Transform root, out RectTransform rect) {
            if (!kit.Ui(root, "Canvas", out rect))
                return false;

            var canvas = rect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = CanvasSortingOrder;

            var scaler = rect.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            return true;
        }

        // 처음 만들 때만 그리는 순서를 정한다(뒤에 있을수록 위). 이후에는 사람이 바꾼 순서를 둔다.
        private static void DrawOrder(RectTransform canvas, params string[] backToFront) {
            foreach (var name in backToFront) {
                var child = canvas.Find(name);
                if (child != null)
                    child.SetAsLastSibling();
            }
        }
    }
}
