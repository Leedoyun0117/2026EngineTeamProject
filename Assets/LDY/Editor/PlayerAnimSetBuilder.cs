using LDY.PlayerMerge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // aseprite 임포터가 만든 스프라이트(Frame_N)를 태그 표대로 나눠 PlayerAnimSet에 담는다.
    // 예전 Pink/V2 세트 메뉴는 2026-11-25에 삭제했다(지금 프리팹·씬은 Cursor 세트만 쓴다).
    public static class PlayerAnimSetBuilder {
        public const string CursorSetPath = "Assets/LDY/Data/PlayerAnimSet_Cursor.asset";

        // idle의 110ms 눈 깜빡임 프레임(전체 인덱스 2)
        private const int CursorAltFrame = 2;

        // 기존 aseprite 임포트 설정(Pixels Per Unit 100)과 같다.
        private const float DefaultPpu = 100f;

        private static readonly Regex FrameName = new(@"^Frame_(\d+)$");

        public static bool BuildCursor() {
            var report = new List<string>();
            if (!CursorAssetLocator.TryFind(CursorAssetLocator.CursorFile, out var path, report)) {
                Debug.LogError($"[PlayerAnimSetBuilder] {string.Join("\n", report)}");
                return false;
            }

            return Build(new Source(path, CursorSetPath, 0, Array.Empty<Tag>(), CursorAltFrame, CursorTagSpecs.All,
                PlayerArt.PixelsPerUnit));
        }

        private static bool Build(Source source) {
            AsepriteTagInfo info = null;
            if (source.Specs != null) {
                if (!TryReadInfo(source, out info))
                    return false;

                source = source.WithFile(info);
            }

            if (!TryCollectFrames(source, out var frames))
                return false;

            if (info != null && !TryValidateTags(source, info, frames))
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
            // 인스펙터에서 바꾼 값을 덮어쓰지 않도록 비어 있을 때만 idle의 눈 깜빡임 프레임으로 채운다.
            if (set.AltSprite == null)
                set.AltSprite = frames[source.AltFrame];
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PlayerAnimSetBuilder] {source.SetPath} 갱신 완료 ({entries.Count}개 클립)");
            return true;
        }

        private static bool TryReadInfo(Source source, out AsepriteTagInfo info) {
            if (AsepriteTagParser.TryParse(File.ReadAllBytes(source.AsepritePath), out info, out var error))
                return true;

            Debug.LogError($"[PlayerAnimSetBuilder] 중단: {source.AsepritePath} 태그를 읽지 못함 ({error})");
            return false;
        }

        // 파일의 태그 범위와 임포터가 만든 스프라이트 번호가 어긋나면 어떤 태그가 어긋났는지 출력하고 중단한다.
        private static bool TryValidateTags(Source source, AsepriteTagInfo info, Sprite[] frames) {
            var present = new bool[frames.Length];
            for (var i = 0; i < frames.Length; i++)
                present[i] = frames[i] != null;

            var names = new List<string>();
            foreach (var spec in source.Specs)
                names.Add(spec.Name);

            var errors = TagRangeValidator.Validate(info, present, names);
            if (errors.Count == 0)
                return true;

            Debug.LogError($"[PlayerAnimSetBuilder] 중단: {source.AsepritePath} 태그 범위가 스프라이트와 어긋남\n- {string.Join("\n- ", errors)}");
            return false;
        }

        // 프레임 이름/개수/순서 또는 임포트 설정이 어긋나면 에러를 내고 중단한다.
        private static bool TryCollectFrames(Source source, out Sprite[] frames) {
            frames = new Sprite[source.FrameCount];
            var errors = new List<string>();
            var names = new List<string>();

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(source.AsepritePath)) {
                if (!(asset is Sprite sprite))
                    continue;

                names.Add(sprite.name);
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
                CheckImportSettings(frames[0], source.ExpectedPpu, errors);

            if (errors.Count == 0)
                return true;

            Debug.LogError($"[PlayerAnimSetBuilder] 중단: {source.AsepritePath}\n- {string.Join("\n- ", errors)}"
                           + $"\n실제 스프라이트 {names.Count}개: {string.Join(", ", names)}");
            return false;
        }

        private static void CheckImportSettings(Sprite sample, float expectedPpu, List<string> errors) {
            if (!Mathf.Approximately(sample.pixelsPerUnit, expectedPpu))
                errors.Add($"PPU가 {sample.pixelsPerUnit} (기대값 {expectedPpu}). 임포터 Inspector에서 Pixels Per Unit을 맞추세요");
            if (sample.texture.filterMode != FilterMode.Point)
                errors.Add($"필터 모드가 {sample.texture.filterMode} (기대값 Point)");

            var format = sample.texture.format;
            if (format != TextureFormat.RGBA32 && format != TextureFormat.ARGB32 && format != TextureFormat.RGB24)
                errors.Add($"텍스처가 압축됨({format}). 임포터 Inspector에서 Compression을 None으로 맞추세요");
        }

        private readonly struct Source {
            public readonly string AsepritePath;
            public readonly string SetPath;
            public readonly int FrameCount;
            public readonly Tag[] Tags;
            public readonly int AltFrame;
            public readonly TagSpec[] Specs;
            public readonly float ExpectedPpu;

            public Source(string asepritePath, string setPath, int frameCount, Tag[] tags, int altFrame,
                TagSpec[] specs = null, float expectedPpu = DefaultPpu) {
                ExpectedPpu = expectedPpu;
                AsepritePath = asepritePath;
                SetPath = setPath;
                FrameCount = frameCount;
                Tags = tags;
                AltFrame = altFrame;
                Specs = specs;
            }

            // 태그 범위와 프레임 시간을 파일에서 읽은 값으로 채운 복사본. 범위 검증은 따로 한다.
            public Source WithFile(AsepriteTagInfo info) {
                var tags = new List<Tag>();
                foreach (var spec in Specs)
                    foreach (var tag in info.Tags)
                        if (tag.Name == spec.Name && tag.From >= 0 && tag.To >= tag.From && tag.To < info.FrameCount)
                            tags.Add(Tag.FromFile(spec, tag, info));

                return new Source(AsepritePath, SetPath, info.FrameCount, tags.ToArray(), AltFrame, Specs, ExpectedPpu);
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

            public static Tag FromFile(TagSpec spec, AsepriteTag tag, AsepriteTagInfo info) {
                var ms = new float[tag.Length];
                for (var i = 0; i < ms.Length; i++)
                    ms[i] = info.DurationsMs[tag.From + i];

                return new Tag(spec.Id, tag.From, spec.Loop, ms);
            }
        }
    }
}
