using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // aseprite 임포터가 만든 스프라이트(Frame_N)를 태그 표대로 나눠 PlayerAnimSet에 담는다. 임시 도구.
    public static class PlayerAnimSetBuilder {
        public const string AsepritePath = "Assets/LDY/Art/player_anim_pink (3).aseprite";
        public const string SetPath = "Assets/LDY/Data/PlayerAnimSet.asset";
        public const string AsepriteV2Path = "Assets/LDY/Sprites/Character/player_anim_pinkV2.aseprite";
        public const string SetV2Path = "Assets/LDY/Data/PlayerAnimSetV2.asset";

        // 기존 aseprite 임포트 설정(Pixels Per Unit 100)과 같다.
        private const float ExpectedPpu = 100f;

        private static readonly Regex FrameName = new(@"^Frame_(\d+)$");

        // aseprite 프레임 시간(ms)
        private static readonly Tag[] BaseTags = {
            new(PlayerAnimId.Idle, 0, true, 900, 120, 900),
            new(PlayerAnimId.Walk, 3, true, 90, 90, 90, 90, 90, 90, 90, 90),
            new(PlayerAnimId.Run, 11, true, 60, 60, 60, 60, 60, 60, 60, 60),
            new(PlayerAnimId.SitDown, 19, false, 100, 120, 160),
            new(PlayerAnimId.SitLoop, 22, true, 1000),
            new(PlayerAnimId.StandUp, 23, false, 100, 120, 100),
            new(PlayerAnimId.Jump, 26, false, 100, 140),
            new(PlayerAnimId.Fall, 28, true, 130, 130, 130, 130),
            new(PlayerAnimId.Grab, 32, true, 200)
        };

        private static readonly Tag[] RopeTags = {
            new(PlayerAnimId.RopeHang, 33, true, 450, 450),
            new(PlayerAnimId.RopeClimb, 35, true, 120, 110, 120, 110),
            new(PlayerAnimId.RopeClimbDown, 39, true, 110, 120, 110, 120)
        };

        private static readonly Source Original = new(AsepritePath, SetPath, 33, BaseTags);
        private static readonly Source V2 = new(AsepriteV2Path, SetV2Path, 43, Concat(BaseTags, RopeTags));

        [MenuItem("LDY/Build Player Anim Set")]
        public static void Build() {
            Build(Original);
        }

        [MenuItem("LDY/Build Player Anim Set V2")]
        public static void BuildV2() {
            Build(V2);
        }

        // 편집기 메뉴 밖에서 고른 aseprite 경로로 V2 세트를 만든다.
        public static bool BuildV2From(string asepritePath) {
            return Build(new Source(asepritePath, SetV2Path, V2.FrameCount, V2.Tags));
        }

        private static bool Build(Source source) {
            if (!TryCollectFrames(source, out var frames))
                return false;

            var entries = new List<PlayerAnimSet.Entry>();
            foreach (var tag in source.Tags) {
                var clip = new PlayerAnimClip { loop = tag.Loop, durations = tag.Seconds };
                clip.frames = new Sprite[tag.Seconds.Length];
                Array.Copy(frames, tag.First, clip.frames, 0, tag.Seconds.Length);
                entries.Add(new PlayerAnimSet.Entry { id = tag.Id, clip = clip });
            }

            var set = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(source.SetPath);
            if (set == null) {
                Directory.CreateDirectory(Path.GetDirectoryName(source.SetPath));
                set = ScriptableObject.CreateInstance<PlayerAnimSet>();
                AssetDatabase.CreateAsset(set, source.SetPath);
            }

            set.Clips = entries.ToArray();
            // 인스펙터에서 바꾼 값을 덮어쓰지 않도록 비어 있을 때만 idle의 눈 깜빡임 프레임(전체 인덱스 1)으로 채운다.
            if (set.AltSprite == null)
                set.AltSprite = frames[1];
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PlayerAnimSetBuilder] {source.SetPath} 갱신 완료 ({entries.Count}개 클립)");
            return true;
        }

        // 프레임 이름/개수/순서 또는 임포트 설정이 어긋나면 에러를 내고 중단한다.
        private static bool TryCollectFrames(Source source, out Sprite[] frames) {
            frames = new Sprite[source.FrameCount];
            var errors = new List<string>();

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(source.AsepritePath)) {
                if (!(asset is Sprite sprite))
                    continue;

                var match = FrameName.Match(sprite.name);
                if (!match.Success) {
                    errors.Add($"예상하지 못한 스프라이트 이름: {sprite.name}");
                    continue;
                }

                var index = int.Parse(match.Groups[1].Value);
                if (index >= source.FrameCount)
                    errors.Add($"프레임 번호가 범위를 벗어남: {sprite.name}");
                else if (frames[index] != null)
                    errors.Add($"프레임 번호 중복: {sprite.name}");
                else
                    frames[index] = sprite;
            }

            for (var i = 0; i < source.FrameCount; i++)
                if (frames[i] == null)
                    errors.Add($"Frame_{i} 가 없음");

            if (errors.Count == 0)
                CheckImportSettings(frames[0], errors);

            if (errors.Count == 0)
                return true;

            Debug.LogError($"[PlayerAnimSetBuilder] 중단: {source.AsepritePath}\n- {string.Join("\n- ", errors)}");
            return false;
        }

        private static void CheckImportSettings(Sprite sample, List<string> errors) {
            if (!Mathf.Approximately(sample.pixelsPerUnit, ExpectedPpu))
                errors.Add($"PPU가 {sample.pixelsPerUnit} (기대값 {ExpectedPpu}). 임포터 Inspector에서 Pixels Per Unit을 맞추세요");
            if (sample.texture.filterMode != FilterMode.Point)
                errors.Add($"필터 모드가 {sample.texture.filterMode} (기대값 Point)");

            var format = sample.texture.format;
            if (format != TextureFormat.RGBA32 && format != TextureFormat.ARGB32 && format != TextureFormat.RGB24)
                errors.Add($"텍스처가 압축됨({format}). 임포터 Inspector에서 Compression을 None으로 맞추세요");
        }

        private static Tag[] Concat(Tag[] first, Tag[] second) {
            var all = new Tag[first.Length + second.Length];
            first.CopyTo(all, 0);
            second.CopyTo(all, first.Length);
            return all;
        }

        private readonly struct Source {
            public readonly string AsepritePath;
            public readonly string SetPath;
            public readonly int FrameCount;
            public readonly Tag[] Tags;

            public Source(string asepritePath, string setPath, int frameCount, Tag[] tags) {
                AsepritePath = asepritePath;
                SetPath = setPath;
                FrameCount = frameCount;
                Tags = tags;
            }
        }

        private readonly struct Tag {
            public readonly PlayerAnimId Id;
            public readonly int First;
            public readonly float[] Seconds;
            public readonly bool Loop;

            public Tag(PlayerAnimId id, int first, bool loop, params float[] ms) {
                Id = id;
                First = first;
                Loop = loop;
                Seconds = new float[ms.Length];
                for (var i = 0; i < ms.Length; i++)
                    Seconds[i] = ms[i] / 1000f;
            }
        }
    }
}
