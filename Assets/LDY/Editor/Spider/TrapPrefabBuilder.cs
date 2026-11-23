using LDY.Spider;
using UnityEngine;

namespace LDY.SpiderEditor {
    internal static class TrapPrefabBuilder {
        public const string Path = SpiderPaths.Prefabs + "/MouseTrap.prefab";

        public static GameObject Build(Stage2Sprites sprites, TrapSettings settings) {
            var root = new GameObject("MouseTrap");
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites.Trap[0];
            renderer.sortingOrder = 2;

            var body = root.AddComponent<Rigidbody2D>();
            body.mass = settings.RestMass;
            body.gravityScale = settings.GravityScale;
            body.freezeRotation = true;

            var size = (Vector2)sprites.Trap[0].bounds.size;
            var solid = root.AddComponent<BoxCollider2D>();
            solid.size = size;
            var hazard = root.AddComponent<BoxCollider2D>();
            hazard.isTrigger = true;
            hazard.size = size + Vector2.one * (settings.HazardPadding * 2f);

            var view = root.AddComponent<MouseTrapView>();
            Wire.Ref(view, "trapRenderer", renderer);
            Wire.Ref(view, "openSprite", sprites.Trap[0]);
            Wire.Ref(view, "snapSprite", sprites.Trap[1]);

            var trap = root.AddComponent<MouseTrap>();
            Wire.Ref(trap, "settings", settings);
            Wire.Ref(trap, "view", view);

            ExternalComponents.TryAdd(root, ExternalComponents.DragObj);
            return SpiderPaths.SavePrefab(root, Path);
        }
    }
}
