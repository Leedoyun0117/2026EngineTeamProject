using System.Collections.Generic;
using LDY.StageSelect;
using LDY.StageSelect.Core;
using UnityEngine;

namespace LDY.Script {
    // BoardPlanner(Core)가 정한 발판·벽을 월드 콜라이더로 만든다. 다시 만들 때는 전부 지우고 새로 만든다.
    // Targets 목록 객체는 그대로 두고 내용만 바꿔서, 이 목록을 들고 있는 StandingMenuFinder가 바로 따라온다.
    public sealed class FileBoard {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly int _layer;
        private readonly Transform _root;
        private readonly List<FilePlatformTarget> _targets = new List<FilePlatformTarget>();

        // 콜라이더는 원점에 둔 전용 루트 아래에 월드 좌표 그대로 만든다(조립 오브젝트 위치와 무관).
        public FileBoard(int layer) {
            _root = new GameObject("FileBoardColliders").transform;
            _layer = layer;
        }

        public IReadOnlyList<FilePlatformTarget> Targets => _targets;

        public void Rebuild(BoardPlan plan, PixelSpace space, IFileBoardHost host) {
            Clear();
            for (var i = 0; i < plan.Files.Count; i++) {
                var span = plan.Files[i];
                var left = space.ToWorld(span.Left, span.Top);
                var right = space.ToWorld(span.Right, span.Top);
                var platform = OneWayPlatform.Create("File_" + span.Id, _root, left.X, right.X, left.Y, _layer);
                _objects.Add(platform.gameObject);
                _targets.Add(new FilePlatformTarget(span.Id, platform, new Vector2((left.X + right.X) * 0.5f, left.Y),
                    host));
            }

            Solid("Floor", space.ToWorld(plan.Floor));
            Solid("Ceiling", space.ToWorld(plan.Ceiling));
            Solid("LeftWall", space.ToWorld(plan.LeftWall));
            Solid("RightWall", space.ToWorld(plan.RightWall));
        }

        public FilePlatformTarget Find(string id) {
            for (var i = 0; i < _targets.Count; i++)
                if (_targets[i].Id == id)
                    return _targets[i];
            return null;
        }

        public void Clear() {
            _targets.Clear();
            // Destroy는 프레임 끝에 일어나므로 먼저 꺼서 이번 프레임 물리에서 빠지게 한다.
            foreach (var go in _objects) {
                if (go == null) continue;
                go.SetActive(false);
                Object.Destroy(go);
            }

            _objects.Clear();
        }

        private void Solid(string name, WorldBox box) {
            var go = new GameObject(name) { layer = _layer };
            go.transform.SetParent(_root, false);
            go.transform.localPosition = new Vector3(box.CenterX, box.CenterY, 0f);
            go.AddComponent<BoxCollider2D>().size = new Vector2(box.Width, box.Height);
            _objects.Add(go);
        }
    }
}
