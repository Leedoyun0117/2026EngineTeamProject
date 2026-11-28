using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LDY.Hud.HudEditor {
    // 월드를 따라가는 표시: 흡수 점선(월드 SpriteRenderer), 흡수 툴팁, 전환 마커, 모래시계, 일시정지 배지.
    internal static class HudWorldSection {
        private const int OutlineSortingOrder = 500;
        private const float OutlinePadding = 0.05f;

        public static UiWorldAnchor Build(HudKit kit, Transform root, RectTransform canvas, HudToasts toasts) {
            if (kit.Ui(canvas, "WorldLayer", out var layer)) {
                HudKit.Stretch(layer, 0f, 0f, 0f, 0f);
                layer.SetAsFirstSibling();
            }

            var available = Outline(kit, root, "SelectAvailable", "hud_select_ylw");
            var blocked = Outline(kit, root, "SelectBlocked", "hud_select_red");
            var tooltip = AbsorbTooltip(kit, layer);
            var marker = SwitchMarker(kit, layer);
            var hourglass = Hourglass(kit, layer);
            Badges(kit, layer);

            var absorb = HudKit.Component<AbsorbPresenter>(layer.gameObject);
            HudKit.Wire(absorb, "skin", kit.Skin);
            HudKit.Wire(absorb, "toasts", toasts);
            HudKit.Wire(absorb, "availableOutline", available);
            HudKit.Wire(absorb, "blockedOutline", blocked);
            HudKit.Wire(absorb, "tooltip", tooltip);

            var inputLock = HudKit.Component<InputLockPresenter>(layer.gameObject);
            HudKit.Wire(inputLock, "hourglass", hourglass);
            return marker;
        }

        private static WorldOutlineView Outline(HudKit kit, Transform root, string name, string key) {
            kit.Plain(root, "World", out var world);
            if (kit.Plain(world, name, out var node)) {
                var frames = kit.Sprites.Frames(key);
                var renderer = node.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = frames.Length > 0 ? frames[0] : null;
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.tileMode = SpriteTileMode.Continuous;
                renderer.size = Vector2.one;
                renderer.sortingOrder = OutlineSortingOrder;
                var unlit = AssetDatabase.LoadAssetAtPath<Material>(HudPaths.UnlitSpriteMaterial);
                if (unlit != null)
                    renderer.sharedMaterial = unlit;

                var animator = node.gameObject.AddComponent<RendererFrameAnimator>();
                HudKit.Frames(animator, frames, kit.Sprites.FrameMilliseconds(key));
                HudKit.Wire(animator, "target", renderer);

                var view = node.gameObject.AddComponent<WorldOutlineView>();
                HudKit.SetOnce(view, "padding", p => p.floatValue = OutlinePadding);
                node.gameObject.SetActive(false);
            }

            var outline = HudKit.Component<WorldOutlineView>(node.gameObject);
            HudKit.Wire(outline, "outline", node.GetComponent<SpriteRenderer>());
            return outline;
        }

        private static UiWorldAnchor AbsorbTooltip(HudKit kit, RectTransform layer) {
            var anchor = Anchor(kit, layer, "AbsorbTooltip");
            HudKeyHint.Bubble(kit, (RectTransform)anchor.transform, "Bubble", new Vector2(0.5f, 0f), new Vector2(0f, 3f),
                HudKey.Absorb, "흡수");
            return anchor;
        }

        private static UiWorldAnchor SwitchMarker(HudKit kit, RectTransform layer) {
            var anchor = Anchor(kit, layer, "SwitchMarker");
            var rect = (RectTransform)anchor.transform;
            Animated(kit, rect, "Arrow", "hud_marker_down", new Vector2(11f, 9f), new Vector2(0f, 2f));
            HudKeyHint.Bubble(kit, rect, "Hint", new Vector2(0f, 0f), new Vector2(7f, 8f), HudKey.SwitchControl, "전환");
            return anchor;
        }

        private static UiWorldAnchor Hourglass(HudKit kit, RectTransform layer) {
            var anchor = Anchor(kit, layer, "Hourglass");
            Animated(kit, (RectTransform)anchor.transform, "Glass", "hud_hourglass", new Vector2(12f, 16f), new Vector2(0f, 2f));
            return anchor;
        }

        private static void Badges(HudKit kit, RectTransform layer) {
            if (kit.Ui(layer, "PauseBadges", out var container))
                HudKit.Stretch(container, 0f, 0f, 0f, 0f);

            var template = Anchor(kit, container, "Badge");
            var rect = (RectTransform)template.transform;
            if (kit.Ui(rect, "Body", out var body)) {
                HudKit.Place(body, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 2f), new Vector2(18f, 18f));
                kit.Image(body, "hud_tooltip");
            }

            if (kit.Ui(body, "Icon", out var icon)) {
                HudKit.Place(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16f, 16f));
                kit.Image(icon, "hud_icon_pause");
            }

            var pool = HudKit.Component<UiPool>(container.gameObject);
            HudKit.Wire(pool, "template", template);
            var presenter = HudKit.Component<StoppedObjectsPresenter>(container.gameObject);
            HudKit.Wire(presenter, "badges", pool);
        }

        // 따라갈 점(대상 위 가운데)이 pivot. 실제 그림은 자식으로 두고 간격은 자식 위치로 정한다.
        private static UiWorldAnchor Anchor(HudKit kit, RectTransform parent, string name) {
            if (kit.Ui(parent, name, out var rect)) {
                HudKit.Place(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                rect.gameObject.SetActive(false);
            }

            return HudKit.Component<UiWorldAnchor>(rect.gameObject);
        }

        private static void Animated(HudKit kit, RectTransform parent, string name, string key, Vector2 size, Vector2 position) {
            if (!kit.Ui(parent, name, out var rect))
                return;

            var frames = kit.Sprites.Frames(key);
            HudKit.Place(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), position, size);
            var image = kit.Image(rect, frames.Length > 0 ? frames[0] : null);
            var animator = rect.gameObject.AddComponent<ImageFrameAnimator>();
            HudKit.Frames(animator, frames, kit.Sprites.FrameMilliseconds(key));
            HudKit.Wire(animator, "image", image);
        }
    }
}
