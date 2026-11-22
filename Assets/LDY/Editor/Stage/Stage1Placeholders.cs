using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LDY.StageEditor {
    internal enum PlaceholderShape {
        None,
        Box,
        Circle
    }

    internal readonly struct PlaceholderSpec {
        public readonly string Name;
        public readonly string ArtName;
        public readonly PlaceholderShape Shape;
        public readonly Vector2 Size;
        public readonly Vector2 Offset;
        public readonly bool Trigger;
        public readonly bool OneWay;
        public readonly bool Dynamic;
        public readonly string Layer;

        public PlaceholderSpec(string name, string artName, PlaceholderShape shape, Vector2 size, Vector2 offset,
            bool trigger, bool oneWay, bool dynamic, string layer) {
            Name = name;
            ArtName = artName;
            Shape = shape;
            Size = size;
            Offset = offset;
            Trigger = trigger;
            OneWay = oneWay;
            Dynamic = dynamic;
            Layer = layer;
        }
    }

    internal static class PlaceholderCatalog {
        private const float Cell = 1f;
        private const float OneWayThickness = 10f / 40f;
        private const string Solid = "Ground";
        private const string Gimmick = "Gimmick";

        public static readonly IReadOnlyList<PlaceholderSpec> All = new[] {
            Plate("Button", "button"),
            Plate("LongPressButton", "long_press_button"),
            SolidBlock("SignalBlock", "signal_block"),
            OneWayPlate("SignalFloor", "signal_floor"),
            OneWayPlate("MovingPlatformH", "moving_platform_h"),
            OneWayPlate("MovingPlatformV", "moving_platform_v"),
            SolidBlock("RotatingBlock", "rotating_block"),
            Decoration("GuillotineFrame", "guillotine_frame"),
            Circle("Gear", "gear", 0.45f),
            Pushable("BoxCdCase", "box_cd_case"),
            SolidBlock("CutBlock", "cut_block"),
            SolidBlock("GhostBlock", "ghost_block"),
            SolidBlock("WallBarrier", "wall_barrier"),
            Trigger("GoalMusicFile", "goal_music_file", Vector2.one * Cell),
            Trigger("UiPause", "ui_pause", Vector2.one * 0.8f),
            Trigger("UiCut", "ui_cut", Vector2.one * 0.8f),
            Trigger("UiDrag", "ui_drag", Vector2.one * 0.8f),
            Trigger("UiZoom", "ui_zoom", Vector2.one * 0.8f),
            Trigger("UiAlpha", "ui_alpha", Vector2.one * 0.8f),
            Marker("Start")
        };

        private static PlaceholderSpec Plate(string name, string art) =>
            new PlaceholderSpec(name, art, PlaceholderShape.Box, new Vector2(Cell, 0.5f), new Vector2(0f, 0.25f), true,
                false, false, Gimmick);

        private static PlaceholderSpec OneWayPlate(string name, string art) =>
            new PlaceholderSpec(name, art, PlaceholderShape.Box, new Vector2(Cell, OneWayThickness),
                new Vector2(0f, OneWayThickness * 0.5f), false, true, false, Solid);

        private static PlaceholderSpec SolidBlock(string name, string art) =>
            new PlaceholderSpec(name, art, PlaceholderShape.Box, Vector2.one * Cell, Vector2.zero, false, false, false,
                Solid);

        private static PlaceholderSpec Pushable(string name, string art) =>
            new PlaceholderSpec(name, art, PlaceholderShape.Box, Vector2.one * Cell, Vector2.zero, false, false, true,
                Solid);

        private static PlaceholderSpec Decoration(string name, string art) =>
            new PlaceholderSpec(name, art, PlaceholderShape.None, Vector2.zero, Vector2.zero, false, false, false,
                "Default");

        private static PlaceholderSpec Circle(string name, string art, float radius) =>
            new PlaceholderSpec(name, art, PlaceholderShape.Circle, Vector2.one * radius, Vector2.zero, true, false,
                false, Gimmick);

        private static PlaceholderSpec Trigger(string name, string art, Vector2 size) =>
            new PlaceholderSpec(name, art, PlaceholderShape.Box, size, Vector2.zero, true, false, false, Gimmick);

        private static PlaceholderSpec Marker(string name) =>
            new PlaceholderSpec(name, null, PlaceholderShape.None, Vector2.zero, Vector2.zero, false, false, false,
                "Default");
    }

    internal sealed class PlaceholderBuilder {
        private readonly AsepriteArtLoader _art;

        public PlaceholderBuilder(AsepriteArtLoader art) {
            _art = art;
        }

        public IReadOnlyList<string> Build(SetupReport report) {
            var built = new List<string>();
            foreach (var spec in PlaceholderCatalog.All) {
                AsepriteArt art = null;
                if (spec.ArtName != null) {
                    art = _art.Load(spec.ArtName);
                    if (art?.First == null) {
                        report.Skipped($"Placeholder {spec.Name}: 아트 {spec.ArtName} 없음");
                        continue;
                    }
                }

                var path = StagePaths.Prefab("Stage1_" + spec.Name);
                PrefabUpsert.Edit(path, () => new GameObject("Stage1_" + spec.Name), root => Apply(root, spec, art),
                    out var created);
                report.Record(created, "Placeholder " + path);
                built.Add(path);
            }

            return built;
        }

        private static void Apply(GameObject root, PlaceholderSpec spec, AsepriteArt art) {
            if (art != null) {
                ObjectTools.SetSprite(root, art.First);
                ObjectTools.SetAnimator(root, art.Controller);
            }

            ObjectTools.SetLayer(root, spec.Layer);
            ApplyCollider(root, spec);
            if (spec.Dynamic) ApplyBody(root);
            if (spec.OneWay) ApplyOneWay(root);
        }

        private static void ApplyCollider(GameObject root, PlaceholderSpec spec) {
            if (spec.Shape == PlaceholderShape.Box) {
                ObjectTools.SetBox(root, spec.Size, spec.Offset, spec.Trigger);
            }
            else if (spec.Shape == PlaceholderShape.Circle) {
                var circle = ObjectTools.EnsureComponent<CircleCollider2D>(root);
                circle.radius = spec.Size.x;
                circle.offset = spec.Offset;
                circle.isTrigger = spec.Trigger;
            }
        }

        private static void ApplyBody(GameObject root) {
            var body = ObjectTools.EnsureComponent<Rigidbody2D>(root);
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
        }

        private static void ApplyOneWay(GameObject root) {
            var collider = root.GetComponent<Collider2D>();
            collider.usedByEffector = true;
            var effector = ObjectTools.EnsureComponent<PlatformEffector2D>(root);
            effector.useOneWay = true;
            effector.surfaceArc = 160f;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
        }
    }
}
