using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 열린 동안 맨 아래 발판 밑(모니터 바닥)으로 못 내려가게 함
    public sealed class WindowFloorGuard
    {
        private const float FallTolerance = 0.15f;

        private readonly IPlayerBody _body;
        private readonly Collider2D _player;
        private readonly IReadOnlyList<Collider2D> _platforms;
        private readonly bool _debug;
        private bool _active;

        public WindowFloorGuard(IPlayerBody body, Collider2D player, IReadOnlyList<Collider2D> platforms, bool debug)
        {
            _body = body;
            _player = player;
            _platforms = platforms;
            _debug = debug;
        }

        public Collider2D Lowest { get; private set; }

        public void Activate()
        {
            Physics2D.SyncTransforms();
            Lowest = FindLowest();
            _active = Lowest != null;
            if (!_active)
            {
                Debug.LogWarning("[Window] 쓸 수 있는 발판이 없어 바닥 보호 안 함");
                return;
            }

            PlaceOnLowest("열림");
        }

        public void Deactivate()
        {
            _active = false;
        }

        public void FixedTick()
        {
            if (!_active)
                return;

            if (Lowest == null || !Lowest.gameObject.activeInHierarchy)
            {
                _active = false;
                return;
            }

            if (_player.bounds.min.y >= Lowest.bounds.max.y - FallTolerance)
                return;

            Debug.LogWarning("[Window] 플레이어가 맨 아래 발판 밑으로 떨어져 위로 복구");
            PlaceOnLowest("복구");
        }

        // 물리는 켜 둔 채 속도만 0으로
        private void PlaceOnLowest(string reason)
        {
            Vector2 position = _body.Position;
            float pivotToFoot = position.y - _player.bounds.min.y;
            float half = _player.bounds.extents.x;
            float min = Lowest.bounds.min.x + half;
            float max = Lowest.bounds.max.x - half;
            float x = min <= max ? Mathf.Clamp(position.x, min, max) : position.x;

            _body.Velocity = Vector2.zero;
            _body.Position = new Vector2(x, Lowest.bounds.max.y + TitleTuning.StandGap + pivotToFoot);
            _body.SyncTransforms();
            Log($"{reason}: 맨 아래 발판({Lowest.name}) 위로 이동 x={x:F2}");
        }

        private Collider2D FindLowest()
        {
            Collider2D lowest = null;
            for (int i = 0; i < _platforms.Count; i++)
            {
                Collider2D platform = _platforms[i];
                if (platform == null || !platform.enabled || !platform.gameObject.activeInHierarchy)
                    continue;

                if (lowest == null || platform.bounds.max.y < lowest.bounds.max.y)
                    lowest = platform;
            }

            return lowest;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void Log(string message)
        {
            if (_debug)
                Debug.Log($"[WindowFloorGuard] {message}");
        }
    }
}
