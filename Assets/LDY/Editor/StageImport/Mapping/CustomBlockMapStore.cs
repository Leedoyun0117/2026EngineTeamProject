using System.Collections.Generic;
using LDY.FlowEditor;
using LDY.StageImport.Core;
using UnityEditor;
using UnityEngine;

namespace LDY.StageImportEditor {
    // CustomBlockMap을 읽는다. 없으면 만들고, 초기값과 파일에 새로 나온 이름으로 빈 칸만 채운다.
    // 팀원 프리팹은 참조만 하고 열거나 고치지 않는다.
    internal static class CustomBlockMapStore {
        public static CustomBlockMap Ensure(CursorStageFile file, FlowLog log) {
            // 밧줄 초기값이 가리킬 LDY 프리팹을 먼저 만든다(없을 때만).
            RopePrefab.Ensure(log);

            var path = StageImportPaths.CustomMap;
            var map = AssetDatabase.LoadAssetAtPath<CustomBlockMap>(path);
            if (map == null) {
                FlowPaths.EnsureFolder(StageImportPaths.Data);
                map = ScriptableObject.CreateInstance<CustomBlockMap>();
                AssetDatabase.CreateAsset(map, path);
                log.Info("custom 대응표 만듦 " + path);
            }

            var changes = 0;
            foreach (var seed in CustomBlockSeed.All) changes += ApplySeed(map, seed);
            if (file != null) changes += AddNewNames(map, file);

            if (changes > 0) {
                EditorUtility.SetDirty(map);
                AssetDatabase.SaveAssets();
                log.Info($"custom 대응표 {path}: 빈 칸 {changes}개 채움");
            }

            return map;
        }

        private static int ApplySeed(CustomBlockMap map, CustomSeed seed) {
            var entry = FindByAnyName(map, seed.Name);
            if (entry == null) {
                map.Entries.Add(new CustomBlockMap.Entry {
                    name = seed.Name, aliases = ToArray(seed.Aliases), prefab = SurePrefab(seed),
                    binding = seed.Binding, sizing = seed.Sizing, note = Note(seed),
                    tile = seed.Tile, decoration = SeedSprite(seed), skipIfEmpty = seed.SkipIfEmpty
                });
                return 1;
            }

            if (entry.locked) return 0;
            var changes = 0;
            if (entry.decoration == null && entry.prefab == null) {
                var sprite = SeedSprite(seed);
                if (sprite != null) {
                    entry.decoration = sprite;
                    changes++;
                }
            }

            if (entry.prefab == null) {
                var prefab = SurePrefab(seed);
                if (prefab != null) {
                    entry.prefab = prefab;
                    changes++;
                }
            }

            if ((entry.aliases == null || entry.aliases.Length == 0) && seed.Aliases.Count > 0) {
                entry.aliases = ToArray(seed.Aliases);
                changes++;
            }

            if (string.IsNullOrWhiteSpace(entry.note)) {
                entry.note = Note(seed);
                changes++;
            }

            return changes;
        }

        private static int AddNewNames(CustomBlockMap map, CursorStageFile file) {
            var added = 0;
            foreach (var custom in file.Customs) {
                if (string.IsNullOrWhiteSpace(custom.Name) || FindByAnyName(map, custom.Name) != null) continue;
                map.Entries.Add(new CustomBlockMap.Entry {
                    name = custom.Name.Trim(), binding = CustomBinding.PerCell, sizing = CustomSizing.Stretch,
                    note = $"[없음] 파일에서 새로 나온 이름 ({custom.Id}, col={custom.Col})"
                });
                added++;
            }

            return added;
        }

        private static CustomBlockMap.Entry FindByAnyName(CustomBlockMap map, string name) {
            var key = CustomNames.Normalize(name);
            foreach (var entry in map.Entries) {
                if (CustomNames.Normalize(entry.name) == key) return entry;
                if (entry.aliases == null) continue;
                foreach (var alias in entry.aliases)
                    if (CustomNames.Normalize(alias) == key)
                        return entry;
            }

            return null;
        }

        // .aseprite 같은 파일은 스프라이트가 하위 에셋이라 파일 이름과 같은 것을 먼저 고른다.
        private static Sprite SeedSprite(CustomSeed seed) {
            if (string.IsNullOrEmpty(seed.Decoration)) return null;
            var main = AssetDatabase.LoadAssetAtPath<Sprite>(seed.Decoration);
            if (main != null) return main;
            var fileName = System.IO.Path.GetFileNameWithoutExtension(seed.Decoration);
            Sprite first = null;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(seed.Decoration)) {
                if (!(asset is Sprite sprite)) continue;
                if (sprite.name == fileName) return sprite;
                if (first == null) first = sprite;
            }

            return first;
        }

        private static GameObject SurePrefab(CustomSeed seed) {
            if (seed.Certainty != SeedCertainty.Sure) return null;
            foreach (var path in seed.Candidates) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) return prefab;
            }

            return null;
        }

        private static string Note(CustomSeed seed) {
            var candidates = seed.Candidates.Count == 0 ? "" : " 후보: " + string.Join(", ", seed.Candidates);
            return $"[{seed.CertaintyLabel}] {seed.Reason}.{candidates}";
        }

        private static string[] ToArray(IReadOnlyList<string> list) => new List<string>(list).ToArray();
    }
}
