using System.Collections.Generic;
using System.Linq;
using LDY.Flow;
using LDY.Flow.Core;
using LDY.StageSelect.Core;
using UnityEngine;

namespace LDY.StageSelect {
    public readonly struct FileItemData {
        public FileItemData(StageEntry entry, StageStatus status, string statusText) {
            Id = entry.id;
            FileName = entry.fileName;
            Thumbnail = entry.thumbnail;
            Status = status;
            StatusText = statusText;
        }

        public string Id { get; }
        public string FileName { get; }
        public Sprite Thumbnail { get; }
        public StageStatus Status { get; }
        public string StatusText { get; }
        public bool Dimmed => Status == StageStatus.Locked || Status == StageStatus.NotReady;
    }

    public readonly struct WorldNodeData {
        public WorldNodeData(WorldEntry world, bool locked, bool selected) {
            World = world.world;
            Label = world.displayName;
            Locked = locked;
            Selected = selected;
        }

        public int World { get; }
        public string Label { get; }
        public bool Locked { get; }
        public bool Selected { get; }
    }

    // 화면 상태(현재 월드·필터·선택)만 들고, 해금·필터 판정은 Core에 맡긴다.
    public sealed class StageSelectModel {
        private readonly StageCatalog _catalog;
        private StageSlot[] _slots;
        private StageStatus[] _statuses;

        public StageSelectModel(StageCatalog catalog) {
            _catalog = catalog;
        }

        public int World { get; private set; } = 1;
        public StageFilterMode Filter { get; private set; } = StageFilterMode.All;
        public string SelectedId { get; private set; }
        public List<int> Visible { get; private set; } = new List<int>();

        public void Refresh(ProgressModel progress) {
            _slots = _catalog.Slots();
            _statuses = UnlockPolicy.Evaluate(_slots, progress);
            Visible = StageFilter.Apply(Filter, _statuses, IndicesOf(World));
            if (Visible.All(i => _slots[i].Id != SelectedId)) SelectedId = Visible.Count > 0 ? _slots[Visible[0]].Id : null;
        }

        public void SetWorld(int world, ProgressModel progress) {
            World = world;
            SelectedId = null;
            Refresh(progress);
        }

        public void SetFilter(StageFilterMode filter, ProgressModel progress) {
            Filter = filter;
            Refresh(progress);
        }

        public void Select(string id) => SelectedId = id;

        // 방금 클리어한 스테이지가 있으면 그것, 없으면 가장 앞의 미완료 칸에 포커스(StartSelectionRule).
        public void Focus(string justClearedId, ProgressModel progress) {
            Refresh(progress);
            if (_slots.Length == 0) return;
            var index = StartSelectionRule.Pick(_slots.Select(s => s.Id).ToArray(), _statuses, justClearedId);
            World = _slots[index].World;
            SelectedId = _slots[index].Id;
            Refresh(progress);
        }

        public string MoveSelection(int step) {
            if (Visible.Count == 0) return SelectedId;
            var position = Visible.FindIndex(i => _slots[i].Id == SelectedId);
            var next = Mathf.Clamp(position < 0 ? 0 : position + step, 0, Visible.Count - 1);
            SelectedId = _slots[Visible[next]].Id;
            return SelectedId;
        }

        // 보기 탭 순환(Tab = 다음, Shift+Tab = 이전). 끝에서 처음으로 돈다.
        public StageFilterMode NextFilter(int step) {
            var count = System.Enum.GetValues(typeof(StageFilterMode)).Length;
            var next = ((int)Filter + step) % count;
            return (StageFilterMode)(next < 0 ? next + count : next);
        }

        // 트리 순서에서 현재 월드의 앞(-1)·뒤(+1) 월드 번호. 끝이면 -1(돌지 않는다).
        public int NeighborWorld(int step) {
            var worlds = _catalog.Worlds;
            for (var i = 0; i < worlds.Count; i++) {
                if (worlds[i].world != World) continue;
                var next = i + step;
                return next >= 0 && next < worlds.Count ? worlds[next].world : -1;
            }

            return -1;
        }

        public StageStatus StatusOf(string id) {
            var index = _catalog.IndexOf(id);
            return index < 0 ? StageStatus.NotReady : _statuses[index];
        }

        public bool IsWorldLocked(int world) {
            var first = IndicesOf(world).DefaultIfEmpty(-1).First();
            return first < 0 || _statuses[first] == StageStatus.Locked;
        }

        public string PrerequisiteFileName(string id) {
            var prerequisite = UnlockPolicy.PrerequisiteOf(_slots, _catalog.IndexOf(id));
            return prerequisite == null ? null : _catalog.Find(prerequisite).fileName;
        }

        public string FirstIdOf(int world) {
            var first = IndicesOf(world).DefaultIfEmpty(-1).First();
            return first < 0 ? null : _slots[first].Id;
        }

        public IEnumerable<FileItemData> Items(ProgressModel progress) =>
            Visible.Select(i => new FileItemData(_catalog.Entries[i], _statuses[i], StatusText(i, progress)));

        public IEnumerable<WorldNodeData> Worlds() =>
            _catalog.Worlds.Select(w => new WorldNodeData(w, IsWorldLocked(w.world), w.world == World));

        public StageSummary Summary() => StageFilter.Summarize(_statuses, Visible);

        public string Address() {
            var world = _catalog.World(World);
            return world != null ? world.FolderPath : StageNaming.RootPath;
        }

        private string StatusText(int index, ProgressModel progress) {
            switch (_statuses[index]) {
                case StageStatus.Cleared:
                    return progress.TryGetBest(_slots[index].Id, out var best)
                        ? StageSelectText.Cleared + " " + StageNaming.FormatTime(best)
                        : StageSelectText.Cleared;
                case StageStatus.Locked: return StageSelectText.Locked;
                case StageStatus.NotReady: return StageSelectText.NotReady;
                default: return StageSelectText.Incomplete;
            }
        }

        private IEnumerable<int> IndicesOf(int world) {
            for (var i = 0; i < _slots.Length; i++)
                if (_slots[i].World == world)
                    yield return i;
        }
    }
}
