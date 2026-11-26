using System.Linq;
using LDY.Background;
using LDY.Background.Core;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LDY.BackgroundEditor {
    internal interface IBackgroundPartBuilder {
        void Build(BackgroundBuildContext context);
    }

    internal sealed class BackgroundBuildContext {
        public BackgroundBuildContext(GameObject root, StageBackgroundLayout layout, TMP_FontAsset font,
            int fontPixelSize, BackgroundBuildLog log) {
            Root = root.transform;
            Layout = layout;
            Font = font;
            FontPixelSize = fontPixelSize;
            Log = log;
            Static = BackgroundObjects.Child(Root, "Static");
            Widgets = BackgroundObjects.Child(Root, "Widgets");
            Anim = BackgroundObjects.Child(Root, "Anim");
            Texts = BackgroundObjects.Child(Root, "Texts");
        }

        public Transform Root { get; }
        public Transform Static { get; }
        public Transform Widgets { get; }
        public Transform Anim { get; }
        public Transform Texts { get; }
        public StageBackgroundLayout Layout { get; }
        public TMP_FontAsset Font { get; }
        public int FontPixelSize { get; }
        public BackgroundBuildLog Log { get; }
        public PixelSpace Space => Layout.Space;

        public Vector3 Local(float px, float py) {
            var p = Space.ToLocal(px, py);
            return new Vector3(p.X, p.Y, 0f);
        }

        public Vector3 Center(PixelRect rect) => Local(rect.CenterX, rect.CenterY);

        public Vector3 Size(PixelRect rect) {
            var s = Space.SizeOf(rect);
            return new Vector3(s.X, s.Y, 1f);
        }

        public Sprite LoadSprite(string path) {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) Log.Warn($"스프라이트 없음 {path} (Import Art를 먼저 실행)");
            return sprite;
        }

        public Sprite[] LoadFrames(string path) {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .OrderBy(s => FrameIndex(s.name))
                .ToArray();
        }

        public Sprite WhitePixel() => LoadSprite(BackgroundArtImporter.WhitePixelPath);

        private static int FrameIndex(string name) {
            var cut = name.LastIndexOf('_');
            return cut >= 0 && int.TryParse(name.Substring(cut + 1), out var n) ? n : int.MaxValue;
        }
    }

    internal static class BackgroundObjects {
        public static Transform Child(Transform parent, string name) {
            var child = parent.Find(name);
            if (child != null) return child;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        public static T Ensure<T>(GameObject go) where T : Component {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        public static SpriteRenderer SpriteChild(Transform parent, string name, Sprite sprite, int order) {
            var renderer = Ensure<SpriteRenderer>(Child(parent, name).gameObject);
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = order;
            return renderer;
        }

        public static Color ToColor(this Rgba c) => new Color(c.R, c.G, c.B, c.A);
    }
}
