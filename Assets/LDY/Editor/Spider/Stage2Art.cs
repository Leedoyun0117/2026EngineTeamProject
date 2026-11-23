using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace LDY.SpiderEditor {
    internal sealed class ArtSpec {
        public readonly string Name;
        public readonly SpriteAlignment Pivot;
        public readonly Vector2Int Size;

        public ArtSpec(string name, SpriteAlignment pivot, Vector2Int size) {
            Name = name;
            Pivot = pivot;
            Size = size;
        }
    }

    // manifest.json 기준값. 소켓은 왼쪽 위 원점 픽셀, 각도는 화면 시계 방향 도(degree)다.
    internal static class SpiderArtSpec {
        public const float PixelsPerUnit = 40f;
        public const string FolderMarker = "stage2_explorer_objects";

        public static readonly ArtSpec Body = new ArtSpec("spider_body", SpriteAlignment.Center, new Vector2Int(160, 96));
        public static readonly ArtSpec Upper = new ArtSpec("spider_leg_upper", SpriteAlignment.LeftCenter, new Vector2Int(150, 14));
        public static readonly ArtSpec Lower = new ArtSpec("spider_leg_lower", SpriteAlignment.LeftCenter, new Vector2Int(250, 12));
        public static readonly ArtSpec UpperCut = new ArtSpec("spider_leg_upper_cut", SpriteAlignment.LeftCenter, new Vector2Int(150, 14));
        public static readonly ArtSpec LowerPiece = new ArtSpec("spider_leg_lower_piece", SpriteAlignment.LeftCenter, new Vector2Int(250, 12));
        public static readonly ArtSpec Thread = new ArtSpec("spider_thread", SpriteAlignment.BottomCenter, new Vector2Int(4, 40));
        public static readonly ArtSpec Trap = new ArtSpec("mouse_trap", SpriteAlignment.Center, new Vector2Int(40, 20));

        public static readonly ArtSpec[] All = { Body, Upper, Lower, UpperCut, LowerPiece, Thread, Trap };

        public static readonly Vector2[] SocketPixels = {
            new Vector2(58f, 60f), new Vector2(72f, 64f), new Vector2(88f, 66f), new Vector2(104f, 66f)
        };

        public static readonly Vector2[] StandingScreenDegrees = {
            new Vector2(-165f, 119f), new Vector2(-148f, 90f), new Vector2(-32f, 89f), new Vector2(-15f, 58f)
        };
    }

    internal sealed class Stage2Sprites {
        public Sprite[] Body;
        public Sprite Upper;
        public Sprite Lower;
        public Sprite UpperCut;
        public Sprite LowerPiece;
        public Sprite Thread;
        public Sprite[] Trap;
    }

    internal sealed class Stage2ArtLoader {
        public bool TryLoad(out Stage2Sprites sprites, out string error) {
            sprites = null;
            var paths = new Dictionary<string, string>();
            foreach (var spec in SpiderArtSpec.All) {
                var path = Find(spec.Name + ".aseprite");
                if (path == null) {
                    error = $"아트를 찾지 못했다: {spec.Name}.aseprite (폴더 '{SpiderArtSpec.FolderMarker}')";
                    return false;
                }

                paths[spec.Name] = path;
            }

            foreach (var spec in SpiderArtSpec.All)
                Configure(paths[spec.Name], spec);

            sprites = new Stage2Sprites {
                Body = Frames(paths[SpiderArtSpec.Body.Name]),
                Upper = Frames(paths[SpiderArtSpec.Upper.Name]).FirstOrDefault(),
                Lower = Frames(paths[SpiderArtSpec.Lower.Name]).FirstOrDefault(),
                UpperCut = Frames(paths[SpiderArtSpec.UpperCut.Name]).FirstOrDefault(),
                LowerPiece = Frames(paths[SpiderArtSpec.LowerPiece.Name]).FirstOrDefault(),
                Thread = Frames(paths[SpiderArtSpec.Thread.Name]).FirstOrDefault(),
                Trap = Frames(paths[SpiderArtSpec.Trap.Name])
            };

            error = Validate(sprites);
            return error == null;
        }

        private static string Find(string fileName) {
            var term = Path.GetFileNameWithoutExtension(fileName);
            foreach (var guid in AssetDatabase.FindAssets(term)) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(SpiderArtSpec.FolderMarker) &&
                    path.EndsWith("/" + fileName, StringComparison.OrdinalIgnoreCase))
                    return path;
            }

            return null;
        }

        private static void Configure(string path, ArtSpec spec) {
            var importer = AssetImporter.GetAtPath(path) as AsepriteImporter;
            if (importer == null)
                return;

            var settings = importer.GetImporterPlatformSettings(BuildTarget.StandaloneWindows64);
            var dirty = !Mathf.Approximately(importer.spritePixelsPerUnit, SpiderArtSpec.PixelsPerUnit)
                        || importer.filterMode != FilterMode.Point
                        || importer.pivotAlignment != spec.Pivot
                        || settings.textureCompression != TextureImporterCompression.Uncompressed;
            if (!dirty)
                return;

            importer.spritePixelsPerUnit = SpiderArtSpec.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.pivotAlignment = spec.Pivot;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetImporterPlatformSettings(settings);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static Sprite[] Frames(string path) {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => FrameIndex(s.name)).ToArray();
        }

        private static int FrameIndex(string spriteName) {
            var cut = spriteName.LastIndexOf('_');
            return cut >= 0 && int.TryParse(spriteName.Substring(cut + 1), out var n) ? n : int.MaxValue;
        }

        private static string Validate(Stage2Sprites sprites) {
            if (sprites.Body.Length < 4) return "spider_body 프레임이 4개 미만이다 (idle, glitch, hurt 2프레임 필요)";
            if (sprites.UpperCut == null || sprites.LowerPiece == null) return "잘린 다리 스프라이트를 읽지 못했다";
            if (sprites.Trap.Length < 2) return "mouse_trap 프레임이 2개 미만이다 (open, snap 필요)";
            if (sprites.Upper == null || sprites.Lower == null || sprites.Thread == null)
                return "다리/실 스프라이트를 읽지 못했다";
            return null;
        }
    }
}
