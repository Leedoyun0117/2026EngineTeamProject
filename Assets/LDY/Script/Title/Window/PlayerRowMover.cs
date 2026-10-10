using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 창 안에서 플레이어를 발판 "한 칸"씩 오르내린다. 0행은 모니터 바닥, 1행~N행은 창 발판(아래에서 위로).
    // 이동은 짧은 보간이고, 그동안 입력 잠금 토큰을 잡고 물리를 꺼 발판을 그냥 통과한다.
    // 어떤 경로로 끝나든(도착, 취소, 예외) 물리, 속도, 토큰은 Finish 한 곳에서 되돌린다.
    public sealed class PlayerRowMover
    {
        private const float Duration = 0.12f;
        // 착지로 보는 속도와, 행 높이에서 이만큼 벗어나도 그 행에 서 있다고 본다.
        private const float RestSpeed = 0.1f;
        private const float RowTolerance = 0.15f;
        // 발판 윗면에서 살짝 띄워 겹침 없이 시작한다. 이후 중력이 내려앉힌다.
        private const float StandGap = 0.02f;
        private const float FloorProbeDistance = 6f;
        private const float MergeDistance = 0.05f;

        private readonly IPlayerBody _body;
        private readonly Collider2D _player;
        private readonly IInputLock _lock;
        private readonly Transform _window;
        private readonly IReadOnlyList<Collider2D> _platforms;
        private readonly float _windowHeight;
        private readonly bool _debug;
        // 각 행의 "서 있는 면" 높이(발판 윗면, 0행은 바닥 윗면)
        private readonly List<float> _tops = new List<float>();
        private readonly List<Collider2D> _rowPlatforms = new List<Collider2D>();

        private IDisposable _token;
        private bool _moving;
        private float _elapsed;
        private Vector2 _from;
        private Vector2 _to;
        private int _targetRow;

        public PlayerRowMover(IPlayerBody body, Collider2D player, IInputLock inputLock, Transform window,
            IReadOnlyList<Collider2D> platforms, float windowHeight, bool debug)
        {
            _body = body;
            _player = player;
            _lock = inputLock;
            _window = window;
            _platforms = platforms;
            _windowHeight = windowHeight;
            _debug = debug;
        }

        public bool IsMoving => _moving;

        // 창이 열릴 때. 행을 다시 계산하고, 0행(바닥)이 아닌 곳에 서 있으면 바닥으로 보낸다.
        public void Open()
        {
            Cancel();
            Physics2D.SyncTransforms();
            RebuildRows();
            if (_tops.Count == 0)
                return;

            if (CurrentRow() != 0)
                PlaceAt(StandPosition(0));
        }

        // dir: +1 위 행, -1 아래 행. 착지해 있고 이동 중이 아닐 때만 시작한다. 시작했으면 true.
        public bool TryStep(int dir)
        {
            if (_moving || _tops.Count == 0 || _lock.IsLocked)
                return false;

            if (Mathf.Abs(_body.Velocity.y) >= RestSpeed)
                return false;

            int row = CurrentRow();
            if (row < 0)
                return false;

            int target = row + dir;
            if (target < 0 || target >= _tops.Count)
                return false;

            Begin(StandPosition(target), target);
            return true;
        }

        public void Tick(float dt)
        {
            if (!_moving)
                return;

            _elapsed += dt;
            float k = Mathf.Clamp01(_elapsed / Duration);
            float eased = k * k * (3f - 2f * k);
            _body.Position = Vector2.LerpUnclamped(_from, _to, eased);
            if (k >= 1f)
                Finish();
        }

        // 창이 닫히거나, 꺼지거나, Alt로 넘어가거나, 씬이 바뀔 때. 진행 중이면 목표 위치로 맞추고 정상 상태로 되돌린다.
        public void Cancel()
        {
            if (_moving)
                Finish();
        }

        private void Begin(Vector2 to, int targetRow)
        {
            _token = _lock.Acquire();
            try
            {
                _from = _body.Position;
                _to = to;
                _targetRow = targetRow;
                _elapsed = 0f;
                _body.Velocity = Vector2.zero;
                _body.PhysicsEnabled = false;
                _moving = true;
                Log($"이동 시작 → {targetRow}행 from={_from.y:F2} to={_to.y:F2}");
            }
            catch
            {
                Finish();
                throw;
            }
        }

        private void Finish()
        {
            bool wasMoving = _moving;
            _moving = false;
            try
            {
                _body.Position = _to;
                _body.SyncTransforms();
            }
            finally
            {
                try
                {
                    _body.PhysicsEnabled = true;
                    _body.Velocity = Vector2.zero;
                }
                finally
                {
                    _token?.Dispose();
                    _token = null;
                }
            }

            if (wasMoving)
                ReportOverlap();
        }

        // 순간 이동한다(열 때 바닥으로 보내는 경우). 물리는 켜 둔 채로 위치와 속도만 맞춘다.
        private void PlaceAt(Vector2 position)
        {
            _body.Velocity = Vector2.zero;
            _body.Position = position;
            _body.SyncTransforms();
            Log($"열 때 바닥으로 이동 y={position.y:F2}");
        }

        // 도착 후 창 발판과 겹쳐 있으면 알린다. 바로 위 행 발판은 한쪽 통과 발판이라 머리가 겹쳐도 정상이므로 제외한다.
        private void ReportOverlap()
        {
            Collider2D rowAbove = _targetRow + 1 < _rowPlatforms.Count ? _rowPlatforms[_targetRow + 1] : null;
            for (int i = 0; i < _platforms.Count; i++)
            {
                Collider2D platform = _platforms[i];
                if (platform == null || platform == rowAbove || !platform.enabled || !platform.gameObject.activeInHierarchy)
                    continue;

                ColliderDistance2D distance = _player.Distance(platform);
                if (distance.isValid && distance.isOverlapped)
                {
                    Debug.LogWarning($"[Window] 행 이동 도착 위치에서 {platform.name}와 {-distance.distance:F3}만큼 겹칩니다. 행 배치를 확인하세요.");
                }
            }
        }

        // 발바닥 높이로 서 있는 행을 찾는다. 어느 행과도 맞지 않으면(공중) -1.
        private int CurrentRow()
        {
            float foot = _player.bounds.min.y;
            int best = -1;
            float bestDistance = RowTolerance;
            for (int i = 0; i < _tops.Count; i++)
            {
                float distance = Mathf.Abs(foot - (_tops[i] + StandGap));
                if (distance <= bestDistance)
                {
                    best = i;
                    bestDistance = distance;
                }
            }

            return best;
        }

        // 행에 서는 몸 위치. x는 유지하되 발판 안으로 들어오게 맞춘다. y는 충돌체 바닥이 윗면 + StandGap에 오도록.
        private Vector2 StandPosition(int row)
        {
            Vector2 position = _body.Position;
            float pivotToFoot = position.y - _player.bounds.min.y;
            float x = position.x;

            Collider2D platform = _rowPlatforms[row];
            if (platform != null)
            {
                float half = _player.bounds.extents.x;
                float min = platform.bounds.min.x + half;
                float max = platform.bounds.max.x - half;
                if (min <= max)
                    x = Mathf.Clamp(x, min, max);
            }

            return new Vector2(x, _tops[row] + StandGap + pivotToFoot);
        }

        private void RebuildRows()
        {
            _tops.Clear();
            _rowPlatforms.Clear();

            var ordered = new List<Collider2D>();
            for (int i = 0; i < _platforms.Count; i++)
            {
                Collider2D platform = _platforms[i];
                if (platform != null && platform.enabled && platform.gameObject.activeInHierarchy)
                    ordered.Add(platform);
            }

            ordered.Sort((a, b) => a.bounds.max.y.CompareTo(b.bounds.max.y));

            if (!TryFindFloor(out float floorTop))
            {
                Debug.LogWarning("[Window] 모니터 바닥을 찾지 못해 행 이동을 쓰지 않습니다(ContentBoundsWalls의 Floor가 필요).");
                return;
            }

            _tops.Add(floorTop);
            _rowPlatforms.Add(null);
            foreach (Collider2D platform in ordered)
            {
                float top = platform.bounds.max.y;
                if (top - _tops[_tops.Count - 1] < MergeDistance)
                    continue;

                _tops.Add(top);
                _rowPlatforms.Add(platform);
            }

            Log($"행 {_tops.Count - 1}개 (바닥 y={floorTop:F2})");
        }

        // 창 아래쪽에서 아래로 쏴서 창 발판과 플레이어가 아닌 가장 위의 바닥을 찾는다.
        private bool TryFindFloor(out float top)
        {
            top = 0f;
            Vector3 origin = _window.TransformPoint(new Vector3(0f, -_windowHeight * 0.5f, 0f));
            origin.x = _body.Position.x;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, FloorProbeDistance);
            float bestDistance = float.MaxValue;
            bool found = false;
            foreach (RaycastHit2D hit in hits)
            {
                Collider2D collider = hit.collider;
                if (collider == null || collider.isTrigger || collider == _player || IsWindowPlatform(collider))
                    continue;

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    top = hit.point.y;
                    found = true;
                }
            }

            return found;
        }

        private bool IsWindowPlatform(Collider2D collider)
        {
            for (int i = 0; i < _platforms.Count; i++)
            {
                if (_platforms[i] == collider)
                    return true;
            }

            return false;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void Log(string message)
        {
            if (_debug)
                Debug.Log($"[PlayerRowMover] {message}");
        }
    }
}
