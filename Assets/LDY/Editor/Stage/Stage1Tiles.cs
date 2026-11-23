using System.Collections.Generic;
using System.Linq;
using LDY.Stage;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LDY.StageEditor {
    internal sealed class Stage1TileSet {
        public RuleTile Ground;
        public RuleTile Platform;
        public Tile[] Spikes;
        public Tile Invisible;

        public IEnumerable<TileBase> All() {
            yield return Ground;
            yield return Platform;
            foreach (var spike in Spikes) yield return spike;
            yield return Invisible;
        }
    }

    internal sealed class RuleTileFactory {
        private static readonly Vector3Int Up = new Vector3Int(0, 1, 0);
        private static readonly Vector3Int Down = new Vector3Int(0, -1, 0);
        private static readonly Vector3Int Left = new Vector3Int(-1, 0, 0);
        private static readonly Vector3Int Right = new Vector3Int(1, 0, 0);

        public RuleTile Ground(IReadOnlyDictionary<string, Sprite> sprites, out bool created) {
            var tile = AssetUpsert.LoadOrCreate<RuleTile>(StagePaths.Tile("Stage1_Ground"), out created);
            tile.m_DefaultSprite = sprites["ground_mid_C"];
            tile.m_DefaultColliderType = Tile.ColliderType.Grid;
            tile.m_TilingRules = new List<RuleTile.TilingRule>();
            for (var v = 0; v < GroundTileNamer.Vertical.Length; v++)
            for (var h = 0; h < GroundTileNamer.Horizontal.Length; h++) {
                var a = GroundTileNamer.AdjacencyOf(v, h);
                var rule = NewRule(sprites[GroundTileNamer.Name(v, h)], Tile.ColliderType.Grid);
                rule.ApplyNeighbors(new Dictionary<Vector3Int, int> {
                    { Up, Flag(a.Up) }, { Down, Flag(a.Down) }, { Left, Flag(a.Left) }, { Right, Flag(a.Right) }
                });
                tile.m_TilingRules.Add(rule);
            }

            EditorUtility.SetDirty(tile);
            return tile;
        }

        public RuleTile Platform(IReadOnlyDictionary<string, Sprite> sprites, out bool created) {
            var tile = AssetUpsert.LoadOrCreate<RuleTile>(StagePaths.Tile("Stage1_Platform"), out created);
            tile.m_DefaultSprite = sprites[PlatformTileNamer.Names[3]];
            tile.m_DefaultColliderType = Tile.ColliderType.Sprite;
            tile.m_TilingRules = new List<RuleTile.TilingRule>();
            for (var h = 0; h < PlatformTileNamer.Names.Length; h++) {
                var a = GroundTileNamer.AdjacencyOf(1, h);
                var rule = NewRule(sprites[PlatformTileNamer.Names[h]], Tile.ColliderType.Sprite);
                rule.ApplyNeighbors(new Dictionary<Vector3Int, int> {
                    { Left, Flag(a.Left) }, { Right, Flag(a.Right) }
                });
                tile.m_TilingRules.Add(rule);
            }

            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static RuleTile.TilingRule NewRule(Sprite sprite, Tile.ColliderType collider) {
            return new RuleTile.TilingRule {
                m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_ColliderType = collider,
                m_Sprites = new[] { sprite }
            };
        }

        private static int Flag(bool filled) {
            return filled ? RuleTile.TilingRuleOutput.Neighbor.This : RuleTile.TilingRuleOutput.Neighbor.NotThis;
        }
    }

    internal sealed class SpikeTileFactory {
        public Tile[] Build(IReadOnlyDictionary<string, Sprite> sprites, SetupReport report) {
            var directions = (SpikeDirection[])System.Enum.GetValues(typeof(SpikeDirection));
            return directions.Select(d => {
                var name = SpikeTileNamer.Name(d);
                var tile = AssetUpsert.LoadOrCreate<Tile>(StagePaths.Tile("Stage1_" + name), out var created);
                tile.sprite = sprites[name];
                tile.colliderType = Tile.ColliderType.Sprite;
                EditorUtility.SetDirty(tile);
                report.Record(created, "Tile " + name);
                return tile;
            }).ToArray();
        }
    }

    // 투명 블록 위치를 에디터에서 보기 위한 반투명 마커 타일. 런타임에는 렌더러가 꺼진다.
    internal sealed class InvisibleTileFactory {
        private const string TileName = "Stage1_InvisibleBlock";
        private const int Size = 16;

        public Tile Build(SetupReport report) {
            var sprite = EnsureSprite(StagePaths.Tiles + "/" + TileName + ".png");
            var tile = AssetUpsert.LoadOrCreate<Tile>(StagePaths.Tile(TileName), out var created);
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(tile);
            report.Record(created, "Tile " + TileName);
            return tile;
        }

        private static Sprite EnsureSprite(string path) {
            if (!System.IO.File.Exists(path)) {
                var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                var fill = new Color(0.6f, 0.2f, 1f, 0.35f);
                var edge = new Color(0.6f, 0.2f, 1f, 0.9f);
                for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                    texture.SetPixel(x, y, x == 0 || y == 0 || x == Size - 1 || y == Size - 1 ? edge : fill);
                System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != Size) {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = Size;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }

    internal sealed class Stage1TileBuilder {
        public Stage1TileSet Build(string tilesetPath, SetupReport report) {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(tilesetPath).OfType<Sprite>()
                .ToDictionary(s => s.name, s => s);
            var missing = TilesetLayout.AllNames().Where(n => !sprites.ContainsKey(n)).ToArray();
            if (missing.Length > 0) {
                report.Warn("타일셋 스프라이트 누락: " + string.Join(", ", missing));
                return null;
            }

            var factory = new RuleTileFactory();
            var set = new Stage1TileSet {
                Ground = factory.Ground(sprites, out var groundCreated),
                Platform = factory.Platform(sprites, out var platformCreated),
                Spikes = new SpikeTileFactory().Build(sprites, report),
                Invisible = new InvisibleTileFactory().Build(report)
            };
            report.Record(groundCreated, "RuleTile Stage1_Ground");
            report.Record(platformCreated, "RuleTile Stage1_Platform");
            return set;
        }
    }

    internal sealed class PaletteBuilder {
        private const string PaletteName = "Stage1_Palette";

        public void Build(Stage1TileSet tiles, SetupReport report) {
            var path = StagePaths.Tiles + "/" + PaletteName + ".prefab";
            var created = AssetDatabase.LoadAssetAtPath<GameObject>(path) == null;
            if (created)
                GridPaletteUtility.CreateNewPalette(StagePaths.Tiles, PaletteName, GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic, new Vector3(1, 1, 0), GridLayout.CellSwizzle.XYZ);

            var contents = PrefabUtility.LoadPrefabContents(path);
            try {
                var tilemap = contents.GetComponentInChildren<Tilemap>();
                tilemap.ClearAllTiles();
                var x = 0;
                foreach (var tile in tiles.All()) tilemap.SetTile(new Vector3Int(x++, 0, 0), tile);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            if (!AssetDatabase.LoadAllAssetsAtPath(path).OfType<GridPalette>().Any())
                report.Warn("Stage1_Palette에서 GridPalette 서브에셋이 사라졌다. 팔레트 창에서 확인 필요");
            report.Record(created, "Palette " + PaletteName);
        }
    }
}
