using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 행은 창 발판만 아래에서 위로 셈(모니터 바닥은 행 아님). 이동 중엔 입력 잠금 + 물리 꺼서 발판을 통과
    // 어떤 경로로 끝나든(도착, 취소, 예외) 물리/속도/토큰은 Finish에서 복구
    public sealed class PlayerRowMover
    {
        private const float Duration = 0.12f;
        // 착지로 보는 속도, 행 높이 허용 오차
        private const float RestSpeed = 0.1f;
        private const float RowTolerance = 0.15f;
        // 겹침 없이 시작하려고 살짝 띄움(이후 중력이 내려앉힘)
        private const float StandGap = 0.02f;
        private const float MergeDistance = 0.05f;

        private readonly IPlayerBody _body;
        private readonly Collider2D _player;
        private readonly IInputLock _lock;
        private readonly IReadOnlyList<Collider2D> _platforms;
        private readonly bool _debug;
        private readonly List<float> _tops = new List<float>();
        private readonly List<Collider2D> _rowPlatforms = new List<Collider2D>();

        private IDisposable _token;
        private bool _moving;
        private float _elapsed;
        private Vector2 _from;
        private Vector2 _to;
        private int _targetRow;

        public PlayerRowMover(IPlayerBody body, Collider2D player, IInputLock inputLock,
            IReadOnlyList<Collider2D> platforms, bool debug)
        {
            _body = body;
            _player = player;
            _lock = inputLock;
            _platforms = platforms;
            _debug = debug;
        }

        public bool IsMoving => _moving;

        // 맨 아래 행으로 옮기는 건 WindowFloorGuard 담당
        public void Open()
        {
            Cancel();
            Physics2D.SyncTransforms();
            RebuildRows();
        }

        // dir: +1 위 행, -1 아래 행. 착지해 있고 이동 중 아닐 때만 시작
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
            float k = UnscaledTween.Progress(_elapsed, Duration);
            float eased = Mathf.SmoothStep(0f, 1f, k);
            _body.Position = Vector2.LerpUnclamped(_from, _to, eased);
            if (k >= 1f)
                Finish();
        }

        // 진행 중이면 목표 위치로 맞추고 정상 상태로 복구
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
                Log($"이동 시작 → {targetRow + 1}행 from={_from.y:F2} to={_to.y:F2}");
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

        // 바로 위 행은 한쪽 통과 발판이라 머리가 겹쳐도 정상이므로 제외
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
                    Debug.LogWarning($"[Window] 행 이동 도착 후 {platform.name}와 {-distance.distance:F3} 겹침, 행 배치 확인 필요");
                }
            }
        }

        // 공중이면 -1
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

        // x는 발판 안으로 맞추고, y는 충돌체 바닥이 윗면 + StandGap에 오게
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

            foreach (Collider2D platform in ordered)
            {
                float top = platform.bounds.max.y;
                if (_tops.Count > 0 && top - _tops[_tops.Count - 1] < MergeDistance)
                    continue;

                _tops.Add(top);
                _rowPlatforms.Add(platform);
            }

            Log($"행 {_tops.Count}개");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void Log(string message)
        {
            if (_debug)
                Debug.Log($"[PlayerRowMover] {message}");
        }
    }
}
