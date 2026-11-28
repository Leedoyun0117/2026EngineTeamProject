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
            StageBackgroundTextSet textSet, TextWidthMeter meter, BackgroundBuildLog log) {
            Root = root.transform;
            Layout = layout;
            Font = font;
            TextSet = textSet;
            Meter = meter;
            Log = log;
            SortingLayerName = BackgroundSorting.LayerName;
            Static = BackgroundObjects.Group(Root, "Static");
            Widgets = BackgroundObjects.Group(Root, "Widgets");
            Anim = BackgroundObjects.Group(Root, "Anim");
            Texts = BackgroundObjects.Group(Root, "Texts");
        }

        public Transform Root { get; }
        public Transform Static { get; }
        public Transform Widgets { get; }
        public Transform Anim { get; }
        public Transform Texts { get; }
        public StageBackgroundLayout Layout { get; }
        public TMP_FontAsset Font { get; }
        public StageBackgroundTextSet TextSet { get; }
        public TextWidthMeter Meter { get; }
        public BackgroundBuildLog Log { get; }
        public string SortingLayerName { get; }
        public int Created { get; private set; }
        public int Kept { get; private set; }
        public PixelSpace Space => Layout.Space;

        public PixelRect EffectiveRect(TextSlotSpec slot) {
            if (TextSet == null || !TextSet.TryGet(slot.Id, out var entry) || !entry.overrideRect) return slot.Rect;
            return new PixelRect(entry.rect.x, entry.rect.y, entry.rect.width, entry.rect.height);
        }

        public string TextOf(TextSlotSpec slot) =>
            TextSet != null && TextSet.TryGet(slot.Id, out var entry) ? entry.text : slot.DefaultText;

        public bool TryCreate(Transform parent, string name, out GameObject go) {
            var existing = parent.Find(name);
            if (existing != null) {
                go = existing.gameObject;
                Kept++;
                return false;
            }

            go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Created++;
            return true;
        }

        public SpriteRenderer AddSprite(GameObject go, Sprite sprite, int order) {
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = SortingLayerName;
            renderer.sortingOrder = order;
            return renderer;
        }

        public SpriteRenderer NewSprite(Transform parent, string name, Sprite sprite, int order) {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return AddSprite(go, sprite, order);
        }

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
            if (sprite == null) Log.Warn($"스프라이트 없음 {path}");
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
        public static Transform Group(Transform parent, string name) {
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

        public static Color ToColor(this Rgba c) => new Color(c.R, c.G, c.B, c.A);
    }
}
