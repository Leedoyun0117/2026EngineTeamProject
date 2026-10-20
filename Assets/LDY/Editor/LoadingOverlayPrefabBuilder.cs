using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Script.Editor
{
    // 프리팹이 없을 때만 한 번 만든다. 이미 있으면 손으로 고친 값을 지키려고 절대 덮어쓰지 않는다.
    public static class LoadingOverlayPrefabBuilder
    {
        public const string PrefabPath = "Assets/LDY/Prefab/LoadingOverlay.prefab";

        // 원본이 68x68이라 2배 = 136
        private const float HourglassSize = 136f;
        private const float LabelHeight = 48f;
        private const float GroupSpacing = 24f;
        private const float LabelWidth = 1200f;

        [MenuItem("LDY/Title/Create Loading Overlay Prefab")]
        public static void CreateMenu()
        {
            LoadingOverlay prefab = EnsurePrefab(FindFirstFrame());
            var art = AssetDatabase.LoadAssetAtPath<TitleArtSet>(TitleArtImporter.Folder + "/TitleArtSet.asset");
            if (art != null && prefab != null && art.loadingOverlayPrefab == null)
            {
                art.loadingOverlayPrefab = prefab;
                EditorUtility.SetDirty(art);
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"LoadingOverlay 프리팹: {PrefabPath} ({(prefab != null ? "사용 가능" : "실패")})");
        }

        public static LoadingOverlay EnsurePrefab(Sprite previewSprite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<LoadingOverlay>(PrefabPath);
            if (existing != null)
                return existing;

            var root = new GameObject("LoadingOverlay", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            canvas.pixelPerfect = true;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Image background = NewChild<Image>("Background", root.transform);
            background.color = Color.black;
            background.raycastTarget = false;
            var backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            var group = new GameObject("Group", typeof(RectTransform));
            group.transform.SetParent(root.transform, false);
            var groupRect = (RectTransform)group.transform;
            groupRect.anchorMin = groupRect.anchorMax = groupRect.pivot = new Vector2(0.5f, 0.5f);
            groupRect.anchoredPosition = Vector2.zero;
            groupRect.sizeDelta = new Vector2(LabelWidth, HourglassSize + GroupSpacing + LabelHeight);

            var layout = group.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = GroupSpacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Image hourglass = NewChild<Image>("Hourglass", group.transform);
            hourglass.sprite = previewSprite;
            hourglass.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            hourglass.preserveAspect = true;
            hourglass.raycastTarget = false;
            var hourglassSize = hourglass.gameObject.AddComponent<LayoutElement>();
            hourglassSize.preferredWidth = HourglassSize;
            hourglassSize.preferredHeight = HourglassSize;

            var label = NewChild<TextMeshProUGUI>("Label", group.transform);
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 36f;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = "Loading";
            var labelSize = label.gameObject.AddComponent<LayoutElement>();
            labelSize.preferredWidth = LabelWidth;
            labelSize.preferredHeight = LabelHeight;

            var overlay = root.AddComponent<LoadingOverlay>();
            var so = new SerializedObject(overlay);
            so.FindProperty("canvas").objectReferenceValue = canvas;
            so.FindProperty("hourglass").objectReferenceValue = hourglass;
            so.FindProperty("label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            var saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return saved != null ? saved.GetComponent<LoadingOverlay>() : null;
        }

        private static T NewChild<T>(string name, Transform parent) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        private static Sprite FindFirstFrame()
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(TitleArtImporter.HourglassFolder + "/hourglass_0.png");
        }
    }
}
