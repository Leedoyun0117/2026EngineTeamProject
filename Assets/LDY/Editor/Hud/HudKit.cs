using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud.HudEditor {
    // 프리팹 노드를 이름으로 찾고 없을 때만 만든다. 만들 때만 배치·모양을 정하고, 참조 연결은 빈 칸만 채운다.
    internal sealed class HudKit {
        private const int UiLayer = 5;

        public HudKit(HudSkin skin, HudKeyMap keys, HudSprites sprites, HudLog log) {
            Skin = skin;
            Keys = keys;
            Sprites = sprites;
            Log = log;
        }

        public HudSkin Skin { get; }
        public HudKeyMap Keys { get; }
        public HudSprites Sprites { get; }
        public HudLog Log { get; }

        public bool Ui(Transform parent, string name, out RectTransform rect) {
            var found = parent.Find(name);
            if (found != null) {
                rect = found as RectTransform;
                return false;
            }

            var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
            go.transform.SetParent(parent, false);
            rect = (RectTransform)go.transform;
            Log.Info("추가 " + PathOf(rect));
            return true;
        }

        public bool Plain(Transform parent, string name, out Transform node) {
            node = parent.Find(name);
            if (node != null)
                return false;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            node = go.transform;
            Log.Info("추가 " + PathOf(node));
            return true;
        }

        public static T Component<T>(GameObject go, out bool added) where T : Component {
            added = !go.TryGetComponent<T>(out var component);
            return added ? go.AddComponent<T>() : component;
        }

        public static T Component<T>(GameObject go) where T : Component => Component<T>(go, out _);

        public static void Wire(Object target, string field, Object value) {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property == null) {
                Debug.LogError($"[HUD] {target.GetType().Name}.{field} 필드 없음");
                return;
            }

            if (property.objectReferenceValue != null || value == null)
                return;

            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetOnce(Object target, string field, System.Action<SerializedProperty> set) {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property == null) {
                Debug.LogError($"[HUD] {target.GetType().Name}.{field} 필드 없음");
                return;
            }

            set(property);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Frames(Object animator, Sprite[] frames, int milliseconds) {
            var so = new SerializedObject(animator);
            var array = so.FindProperty("frames");
            if (array.arraySize > 0)
                return;

            array.arraySize = frames.Length;
            for (var i = 0; i < frames.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            so.FindProperty("frameMilliseconds").intValue = milliseconds;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public Image Image(RectTransform rect, string spriteKey) => Image(rect, Sprites.Single(spriteKey), Sprites.ImageType(spriteKey));

        public Image Image(RectTransform rect, Sprite sprite, Image.Type type = UnityEngine.UI.Image.Type.Simple) {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = type;
            image.raycastTarget = false;
            return image;
        }

        public TextMeshProUGUI Text(RectTransform rect, string text, Color color, TextAlignmentOptions alignment) {
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = Skin.font;
            label.fontSize = Skin.fontSize;
            label.color = color;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        public static HorizontalLayoutGroup Row(RectTransform rect, RectOffset padding, float spacing, TextAnchor alignment) {
            var row = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.padding = padding;
            row.spacing = spacing;
            row.childAlignment = alignment;
            row.childControlWidth = true;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            return row;
        }

        public static void FitWidth(RectTransform rect) {
            var fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size) {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static void Stretch(RectTransform rect, float left, float bottom, float right, float top) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static string PathOf(Transform node) =>
            node.parent == null ? node.name : PathOf(node.parent) + "/" + node.name;
    }
}
