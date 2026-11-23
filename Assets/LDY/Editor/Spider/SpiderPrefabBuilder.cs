using LDY.Spider;
using UnityEditor;
using UnityEngine;

namespace LDY.SpiderEditor {
    internal sealed class SpiderPrefabBuilder {
        public const string Path = SpiderPaths.Prefabs + "/Spider.prefab";

        private const float ThreadLength = 40f;
        private const int ThreadOrder = 4;
        private const int BodyOrder = 5;
        private const int UpperOrder = 2;
        private const int LowerOrder = 3;
        private const int MarkerOrder = 6;
        private static readonly Vector2 MarkerScale = new Vector2(0.9f, 0.25f);
        private static readonly Color MarkerColor = new Color(1f, 0.15f, 0.15f, 0.85f);

        private readonly Stage2Sprites _sprites;
        private readonly SpiderSettings _settings;

        public SpiderPrefabBuilder(Stage2Sprites sprites, SpiderSettings settings) {
            _sprites = sprites;
            _settings = settings;
        }

        // 서 있는 자세에서 몸 중심으로부터 가장 낮은 발까지의 거리(유닛). 씬에서 바닥 위에 놓을 높이다.
        public float RestHeight() {
            var upper = _sprites.Upper.bounds.size.x;
            var lower = _sprites.Lower.bounds.size.x;
            var lowest = 0f;
            for (var i = 0; i < SpiderArtSpec.SocketPixels.Length; i++) {
                var socket = SpiderObjects.Socket(i, _sprites.Body[0]);
                var angles = SpiderArtSpec.StandingScreenDegrees[i] * -Mathf.Deg2Rad;
                var foot = TwoBoneIK.Forward(socket.ToVec2(), upper, lower, angles.x, angles.y);
                lowest = Mathf.Min(lowest, foot.Y);
            }

            return -lowest;
        }

        public GameObject Build() {
            var root = new GameObject("Spider");
            var body = BuildBody(root.transform, out var bodyView);
            AddThread(body.transform);

            var legs = new SpiderLeg[SpiderArtSpec.SocketPixels.Length];
            for (var i = 0; i < legs.Length; i++)
                legs[i] = BuildLeg(body.transform, i);

            var marker = BuildMarker(root.transform);
            var controller = root.AddComponent<SpiderController>();
            Wire.Ref(controller, "settings", _settings);
            Wire.Refs(controller, "legs", legs);
            Wire.Ref(controller, "bodyView", bodyView);
            Wire.Ref(controller, "marker", marker);
            Wire.Mask(controller, "groundMask", LayerMask.GetMask("Ground"));
            return SpiderPaths.SavePrefab(root, Path);
        }

        private GameObject BuildBody(Transform parent, out SpiderBodyView view) {
            var renderer = SpriteObjects.Add(parent, "Body", _sprites.Body[0], BodyOrder);
            view = renderer.gameObject.AddComponent<SpiderBodyView>();
            Wire.Ref(view, "bodyRenderer", renderer);
            Wire.Ref(view, "idleSprite", _sprites.Body[0]);
            Wire.Ref(view, "glitchSprite", _sprites.Body[1]);
            Wire.Ref(view, "hurtSpriteA", _sprites.Body[2]);
            Wire.Ref(view, "hurtSpriteB", _sprites.Body[3]);
            return renderer.gameObject;
        }

        private void AddThread(Transform body) {
            var renderer = SpriteObjects.Add(body, "Thread", _sprites.Thread, ThreadOrder);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(_sprites.Thread.bounds.size.x, ThreadLength);
            renderer.transform.localPosition = new Vector3(0f, _sprites.Body[0].bounds.extents.y, 0f);
        }

        private SpiderLeg BuildLeg(Transform body, int index) {
            var leg = new GameObject("Leg_" + index);
            leg.transform.SetParent(body, false);
            var socket = SpiderObjects.Socket(index, _sprites.Body[0]);
            leg.transform.localPosition = new Vector3(socket.x, socket.y, 0f);

            var upper = SpriteObjects.Add(leg.transform, "Upper", _sprites.Upper, UpperOrder);
            var lower = SpriteObjects.Add(leg.transform, "Lower", _sprites.Lower, LowerOrder);
            lower.transform.localPosition = new Vector3(_sprites.Upper.bounds.size.x, 0f, 0f);
            var needle = BuildNeedle(lower.transform);

            var component = leg.AddComponent<SpiderLeg>();
            var angles = SpiderArtSpec.StandingScreenDegrees[index];
            Wire.Ref(component, "upper", upper.transform);
            Wire.Ref(component, "lower", lower.transform);
            Wire.Ref(component, "upperRenderer", upper);
            Wire.Ref(component, "lowerRenderer", lower);
            Wire.Ref(component, "needle", needle);
            Wire.Float(component, "restUpperDegrees", -angles.x);
            Wire.Float(component, "restLowerDegrees", -angles.y);
            Wire.Float(component, "bendSign", socket.x >= 0f ? 1f : -1f);
            Wire.Ref(component, "upperCutSprite", _sprites.UpperCut);
            Wire.Ref(component, "lowerPieceSprite", _sprites.LowerPiece);
            return component;
        }

        private Collider2D BuildNeedle(Transform lower) {
            var go = new GameObject("Needle");
            go.transform.SetParent(lower, false);
            go.transform.localPosition = new Vector3(_sprites.Lower.bounds.size.x, 0f, 0f);

            var circle = go.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = _settings.TipRadius;

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<NeedleKiller>();
            return circle;
        }

        private static TelegraphMarker BuildMarker(Transform parent) {
            var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpiderPaths.CircleSprite);
            var renderer = SpriteObjects.Add(parent, "Marker", circle, MarkerOrder);
            renderer.color = MarkerColor;
            renderer.transform.localScale = new Vector3(MarkerScale.x, MarkerScale.y, 1f);

            var marker = renderer.gameObject.AddComponent<TelegraphMarker>();
            Wire.Ref(marker, "markerRenderer", renderer);
            return marker;
        }
    }

    internal static class SpiderObjects {
        public static Vector2 Socket(int index, Sprite bodySprite) {
            return SpriteObjects.PixelsToLocal(SpiderArtSpec.SocketPixels[index], bodySprite);
        }
    }
}
