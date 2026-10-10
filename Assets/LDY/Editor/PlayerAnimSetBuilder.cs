using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LDY.Script
{
    // aseprite 임포터가 만든 스프라이트(Frame_0~25)를 태그 표대로 나눠 PlayerAnimSet에 담는다. 임시 도구.
    public static class PlayerAnimSetBuilder
    {
        public const string AsepritePath = "Assets/LDY/Sprites/Character/player_anim (2).aseprite";
        public const string SetPath = "Assets/LDY/Data/PlayerAnimSet.asset";

        private const int FrameCount = 26;
        private const float ExpectedPpu = 40f;

        private static readonly Regex FrameName = new Regex(@"^Frame_(\d+)$");

        private readonly struct Tag
        {
            public readonly PlayerAnimId Id;
            public readonly int First;
            public readonly float[] Seconds;
            public readonly bool Loop;

            public Tag(PlayerAnimId id, int first, bool loop, params float[] ms)
            {
                Id = id;
                First = first;
                Loop = loop;
                Seconds = new float[ms.Length];
                for (int i = 0; i < ms.Length; i++)
                    Seconds[i] = ms[i] / 1000f;
            }
        }

        // aseprite 프레임 시간(ms)
        private static readonly Tag[] Tags =
        {
            new Tag(PlayerAnimId.Idle, 0, true, 900, 120, 900),
            new Tag(PlayerAnimId.Walk, 3, true, 90, 90, 90, 90, 90, 90, 90, 90),
            new Tag(PlayerAnimId.Run, 11, true, 60, 60, 60, 60, 60, 60, 60, 60),
            new Tag(PlayerAnimId.SitDown, 19, false, 100, 120, 160),
            new Tag(PlayerAnimId.SitLoop, 22, true, 1000),
            new Tag(PlayerAnimId.StandUp, 23, false, 100, 120, 100),
        };

        [MenuItem("LDY/Build Player Anim Set")]
        public static void Build()
        {
            if (!TryCollectFrames(out Sprite[] frames))
                return;

            var entries = new List<PlayerAnimSet.Entry>();
            foreach (Tag tag in Tags)
            {
                var clip = new PlayerAnimClip { loop = tag.Loop, durations = tag.Seconds };
                clip.frames = new Sprite[tag.Seconds.Length];
                System.Array.Copy(frames, tag.First, clip.frames, 0, tag.Seconds.Length);
                entries.Add(new PlayerAnimSet.Entry { id = tag.Id, clip = clip });
            }

            var set = AssetDatabase.LoadAssetAtPath<PlayerAnimSet>(SetPath);
            if (set == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SetPath));
                set = ScriptableObject.CreateInstance<PlayerAnimSet>();
                AssetDatabase.CreateAsset(set, SetPath);
            }

            set.Clips = entries.ToArray();
            // 인스펙터에서 바꾼 값을 덮어쓰지 않도록 비어 있을 때만 idle의 눈 깜빡임 프레임(전체 인덱스 1)으로 채운다.
            if (set.AltSprite == null)
                set.AltSprite = frames[1];
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PlayerAnimSetBuilder] {SetPath} 갱신 완료 ({entries.Count}개 클립)");
        }

        // 프레임 이름/개수/순서 또는 임포트 설정이 어긋나면 에러를 내고 중단한다.
        private static bool TryCollectFrames(out Sprite[] frames)
        {
            frames = new Sprite[FrameCount];
            var errors = new List<string>();

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(AsepritePath))
            {
                if (!(asset is Sprite sprite))
                    continue;

                Match match = FrameName.Match(sprite.name);
                if (!match.Success)
                {
                    errors.Add($"예상하지 못한 스프라이트 이름: {sprite.name}");
                    continue;
                }

                int index = int.Parse(match.Groups[1].Value);
                if (index >= FrameCount)
                    errors.Add($"프레임 번호가 범위를 벗어남: {sprite.name}");
                else if (frames[index] != null)
                    errors.Add($"프레임 번호 중복: {sprite.name}");
                else
                    frames[index] = sprite;
            }

            for (int i = 0; i < FrameCount; i++)
            {
                if (frames[i] == null)
                    errors.Add($"Frame_{i} 가 없음");
            }

            if (errors.Count == 0)
                CheckImportSettings(frames[0], errors);

            if (errors.Count == 0)
                return true;

            Debug.LogError($"[PlayerAnimSetBuilder] 중단: {AsepritePath}\n- {string.Join("\n- ", errors)}");
            return false;
        }

        private static void CheckImportSettings(Sprite sample, List<string> errors)
        {
            if (!Mathf.Approximately(sample.pixelsPerUnit, ExpectedPpu))
                errors.Add($"PPU가 {sample.pixelsPerUnit} (기대값 {ExpectedPpu}). 임포터 Inspector에서 Pixels Per Unit을 맞추세요");
            if (sample.texture.filterMode != FilterMode.Point)
                errors.Add($"필터 모드가 {sample.texture.filterMode} (기대값 Point)");

            TextureFormat format = sample.texture.format;
            if (format != TextureFormat.RGBA32 && format != TextureFormat.ARGB32 && format != TextureFormat.RGB24)
                errors.Add($"텍스처가 압축됨({format}). 임포터 Inspector에서 Compression을 None으로 맞추세요");
        }
    }
}
