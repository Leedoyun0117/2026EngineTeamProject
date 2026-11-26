using System;
using System.Collections.Generic;
using LDY.Flow.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LDY.Flow {
    public enum StageReadiness {
        NotReady,
        Ready
    }

    [Serializable]
    public sealed class WorldEntry {
        public int world;
        public string displayName;
        public string folderName;

        public string FolderPath => StageNaming.WorldPath(folderName);
    }

    [Serializable]
    public sealed class StageEntry {
        public string id;
        public int world;
        public int number;
        public string fileName;
        public string scenePath;
#if UNITY_EDITOR
        public Object sceneAsset;
#endif
        [Tooltip("비우면 씬 하나가 스테이지 하나. 값이 있으면 씬 안의 이 이름 루트만 켠다")]
        public string subRootName;
        public Sprite thumbnail;
        public List<string> abilities = new List<string>();
        public StageReadiness state;

        public bool IsReady => state == StageReadiness.Ready && !string.IsNullOrEmpty(scenePath);
        public bool UsesSubRoot => !string.IsNullOrEmpty(subRootName);
    }

    // 스테이지 목록의 유일한 출처. 목록 순서가 곧 해금 순서다.
    [CreateAssetMenu(menuName = "LDY/Flow/StageCatalog", fileName = "StageCatalog")]
    public sealed class StageCatalog : ScriptableObject {
        [SerializeField] private List<WorldEntry> worlds = new List<WorldEntry>();
        [SerializeField] private List<StageEntry> entries = new List<StageEntry>();

        public IReadOnlyList<WorldEntry> Worlds => worlds;
        public IReadOnlyList<StageEntry> Entries => entries;

        public List<WorldEntry> EditableWorlds => worlds;
        public List<StageEntry> EditableEntries => entries;

        public StageSlot[] Slots() {
            var slots = new StageSlot[entries.Count];
            for (var i = 0; i < entries.Count; i++)
                slots[i] = new StageSlot(entries[i].id, entries[i].world, entries[i].number, entries[i].IsReady);
            return slots;
        }

        public int IndexOf(string id) => entries.FindIndex(e => e.id == id);

        public StageEntry Find(string id) {
            var index = IndexOf(id);
            return index >= 0 ? entries[index] : null;
        }

        // 씬 하나 = 스테이지 하나인 항목만 찾는다. 다중 스테이지 씬은 SubStagesIn으로 본다.
        public StageEntry FindWholeScene(string scenePath) =>
            string.IsNullOrEmpty(scenePath) ? null : entries.Find(e => e.scenePath == scenePath && !e.UsesSubRoot);

        public List<StageEntry> SubStagesIn(string scenePath) =>
            entries.FindAll(e => !string.IsNullOrEmpty(scenePath) && e.scenePath == scenePath && e.UsesSubRoot);

        public WorldEntry World(int world) => worlds.Find(w => w.world == world);

#if UNITY_EDITOR
        private void OnValidate() {
            foreach (var entry in entries) {
                var path = entry.sceneAsset != null ? UnityEditor.AssetDatabase.GetAssetPath(entry.sceneAsset) : "";
                if (path.EndsWith(".unity")) entry.scenePath = path;
                else if (entry.sceneAsset == null) entry.scenePath = "";
            }
        }
#endif
    }
}
