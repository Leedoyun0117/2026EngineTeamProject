using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LDY.StageEditor {
    internal sealed class VariantContext {
        private readonly AsepriteArtLoader _art;

        public VariantContext(AsepriteArtLoader art) {
            _art = art;
        }

        public AsepriteArt Art(string name) => _art.Load(name);

        public static string VariantPath(string sourceName) => StagePaths.Prefab("Stage1_" + sourceName);
    }

    internal interface IVariantRecipe {
        string SourceName { get; }
        string[] RequiredArt { get; }
        void Apply(GameObject root, GameObject source, VariantContext context);
    }

    internal sealed class GlassVariant : IVariantRecipe {
        public string SourceName => "Glass";
        public string[] RequiredArt => new[] { "breakable_block" };

        public void Apply(GameObject root, GameObject source, VariantContext context) {
            var art = context.Art("breakable_block");
            root.transform.localScale = Vector3.one;
            ObjectTools.SetSprite(root, art.First);
            ObjectTools.SetAnimator(root, art.Controller);
            ObjectTools.SetBox(root, art.SizeUnits, Vector2.zero, false);
            ObjectTools.SetLayer(root, "Ground");
        }
    }

    internal sealed class CircleVariant : IVariantRecipe {
        public string SourceName => "Circle";
        public string[] RequiredArt => new[] { "cannonball_cd" };

        public void Apply(GameObject root, GameObject source, VariantContext context) {
            var art = context.Art("cannonball_cd");
            root.transform.localScale = Vector3.one;
            ObjectTools.SetSprite(root, art.First);
            ObjectTools.SetAnimator(root, art.Controller);
            var circle = root.GetComponent<CircleCollider2D>();
            if (circle != null) circle.radius = art.SizeUnits.x * 0.5f;
        }
    }

    internal sealed class CannonVariant : IVariantRecipe {
        public string SourceName => "cannon";
        public string[] RequiredArt => new[] { "cannon" };

        public void Apply(GameObject root, GameObject source, VariantContext context) {
            var art = context.Art("cannon");
            root.transform.localScale = Vector3.one;
            ObjectTools.SetSprite(root, art.First);
            ObjectTools.SetAnimator(root, art.Controller);
            RedirectBullet(root, VariantContext.VariantPath("Circle"));
        }

        private static void RedirectBullet(GameObject root, string bulletPath) {
            var bullet = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPath);
            var cannon = root.GetComponents<MonoBehaviour>().FirstOrDefault(c => c != null && c.GetType().Name == "Cannon");
            if (bullet == null || cannon == null) return;
            var so = new SerializedObject(cannon);
            var property = so.FindProperty("bulletPrefab");
            if (property == null) return;
            property.objectReferenceValue = bullet;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    internal sealed class GuillotineVariant : IVariantRecipe {
        private const float KillEdgeUnits = 16f / 40f;

        public string SourceName => "Guillotine";
        public string[] RequiredArt => new[] { "guillotine_blade" };

        public void Apply(GameObject root, GameObject source, VariantContext context) {
            var blade = ObjectTools.FindDeep(root.transform, "GameObject");
            if (blade == null) return;
            var art = context.Art("guillotine_blade");
            blade.localScale = Vector3.one;
            ObjectTools.SetSprite(blade.gameObject, art.First);

            var polygon = blade.GetComponent<PolygonCollider2D>();
            if (polygon == null) return;
            var half = art.SizeUnits * 0.5f;
            polygon.pathCount = 1;
            polygon.SetPath(0, new[] {
                new Vector2(-half.x, -half.y),
                new Vector2(half.x, -half.y),
                new Vector2(half.x, -half.y + KillEdgeUnits),
                new Vector2(-half.x, -half.y + KillEdgeUnits)
            });
        }
    }

    internal sealed class DoorVariant : IVariantRecipe {
        private static readonly string[] ButtonNames = { "Square", "Square (1)" };

        public string SourceName => "LRDoor";
        public string[] RequiredArt => new[] { "wall_barrier", "button" };

        public void Apply(GameObject root, GameObject source, VariantContext context) {
            var barrier = context.Art("wall_barrier");
            var button = context.Art("button");
            var height = source.transform.localScale.y;

            root.transform.localScale = Vector3.one;
            var renderer = ObjectTools.SetSprite(root, barrier.First, SpriteDrawMode.Tiled);
            renderer.size = new Vector2(1f, height);

            foreach (var name in ButtonNames) {
                var sourceChild = ObjectTools.FindDeep(source.transform, name);
                var child = ObjectTools.FindDeep(root.transform, name);
                if (sourceChild == null || child == null) continue;
                var p = sourceChild.localPosition;
                child.localPosition = new Vector3(p.x, p.y * height - button.SizeUnits.y * 0.5f, p.z);
                child.localScale = Vector3.one;
                ObjectTools.SetSprite(child.gameObject, button.First);
                var box = child.GetComponent<BoxCollider2D>();
                if (box != null) {
                    box.size = button.SizeUnits;
                    box.offset = new Vector2(0f, button.SizeUnits.y * 0.5f);
                }
            }
        }
    }

    internal sealed class RotateBlockVariant : IVariantRecipe {
        public string SourceName => "RotateBlock";
        public string[] RequiredArt => new[] { "rotating_platform" };

        public void Apply(GameObject root, GameObject source, VariantContext context) {
            var art = context.Art("rotating_platform");
            root.transform.localScale = Vector3.one;
            ObjectTools.SetSprite(root, art.First);
            ObjectTools.SetBox(root, art.SizeUnits, Vector2.zero, false);
            ObjectTools.SetLayer(root, "Ground");
        }
    }

    internal sealed class VariantBuilder {
        private static readonly IVariantRecipe[] Recipes = {
            new CircleVariant(), new CannonVariant(), new GlassVariant(), new GuillotineVariant(),
            new DoorVariant(), new RotateBlockVariant()
        };

        private readonly VariantContext _context;

        public VariantBuilder(VariantContext context) {
            _context = context;
        }

        public IReadOnlyList<string> Build(SetupReport report) {
            var built = new List<string>();
            foreach (var recipe in Recipes) {
                var source = FindTeamPrefab(recipe.SourceName);
                if (source == null) {
                    report.Skipped($"Variant {recipe.SourceName}: 팀원 프리팹을 찾지 못함");
                    continue;
                }

                var missingArt = recipe.RequiredArt.Where(a => _context.Art(a)?.First == null).ToArray();
                if (missingArt.Length > 0) {
                    report.Skipped($"Variant {recipe.SourceName}: 아트 누락 {string.Join(", ", missingArt)}");
                    continue;
                }

                var path = VariantContext.VariantPath(recipe.SourceName);
                PrefabUpsert.EditVariant(path, source, root => recipe.Apply(root, source, _context), out var created);
                report.Record(created, $"Variant {path} (원본 {AssetDatabase.GetAssetPath(source)})");
                built.Add(path);
            }

            return built;
        }

        private static GameObject FindTeamPrefab(string name) {
            var guids = AssetDatabase.FindAssets(name + " t:Prefab", new[] { "Assets/HES", "Assets/JJB" });
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(path), name, StringComparison.Ordinal))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            return null;
        }
    }
}
