using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 창 발판 위에서 아래로 내려가기. 서 있는 발판과 플레이어 충돌을 무시하고, 시간이 아니라 몸이 발판에서 빠졌는지로 복구한다.
    // 시작은 Update(입력), 복구 검사는 FixedUpdate(물리)에서 한다.
    public sealed class PlatformDropper
    {
        private const float StandTolerance = 0.1f;
        // 상승 중으로 보지 않는 최대 위쪽 속도
        private const float RisingSpeed = 0.1f;
        // 정지로 보는 속도
        private const float RestSpeed = 0.1f;
        // 겹침이 풀렸다고 보는 최소 거리
        private const float SeparatedDistance = 0.01f;
        private const float HardCapSeconds = 2f;

        private sealed class Entry
        {
            public Collider2D Platform;
            public float Elapsed;
        }

        private readonly IReadOnlyList<Collider2D> _platforms;
        private readonly Collider2D _player;
        private readonly IPlayerBody _body;
        private readonly float _maxIgnoreSeconds;
        private readonly float _downSpeed;
        private readonly List<Entry> _entries = new List<Entry>();

        public PlatformDropper(IReadOnlyList<Collider2D> platforms, Collider2D player, IPlayerBody body,
            float maxIgnoreSeconds, float downSpeed)
        {
            _platforms = platforms;
            _player = player;
            _body = body;
            _maxIgnoreSeconds = maxIgnoreSeconds;
            _downSpeed = downSpeed;
        }

        // 발판 위에 서 있을 때만 시작한다(점프/상승 중이면 무시). 같은 발판은 무시 중에 다시 시작하지 않는다.
        public void TryDrop()
        {
            if (_body.Velocity.y > RisingSpeed)
                return;

            foreach (Collider2D platform in _platforms)
            {
                if (platform == null || !platform.enabled || !platform.gameObject.activeInHierarchy || IsIgnored(platform))
                    continue;

                if (!_player.IsTouching(platform) || _player.bounds.min.y < platform.bounds.max.y - StandTolerance)
                    continue;

                Physics2D.IgnoreCollision(_player, platform, true);
                _entries.Add(new Entry { Platform = platform });

                // 발판을 빨리 벗어나도록 아래로 밀어 준다.
                Vector2 velocity = _body.Velocity;
                _body.Velocity = new Vector2(velocity.x, Mathf.Min(velocity.y, _downSpeed));
                return;
            }
        }

        public void FixedTick(float dt)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                Entry entry = _entries[i];
                entry.Elapsed += dt;

                if (entry.Platform == null || !IsOverlapped(entry.Platform))
                {
                    Restore(i);
                    continue;
                }

                if (entry.Elapsed >= HardCapSeconds)
                {
                    Debug.LogWarning($"[Window] 내려가기: {entry.Platform.name}와 {HardCapSeconds}초가 지나도 겹쳐 있어 강제로 복구합니다.");
                    Restore(i);
                    continue;
                }

                // 위 행 발판에 머리가 걸친 채 아래 행에 착지하면 겹침이 영영 풀리지 않는다(행 간격이 키보다 좁다).
                // 제한 시간이 지난 뒤 발바닥이 발판 아래로 내려가 멈춰 있으면 평소 서 있는 상태와 같으므로 복구한다.
                bool landedBelow = _player.bounds.min.y < entry.Platform.bounds.min.y &&
                                   Mathf.Abs(_body.Velocity.y) < RestSpeed;
                if (entry.Elapsed >= _maxIgnoreSeconds && landedBelow)
                    Restore(i);
            }
        }

        // 창이 닫히거나 Alt로 넘어갈 때. 겹침이 이미 풀린 발판은 바로 복구하고, 겹쳐 있으면 풀릴 때까지 그 발판만 계속 무시한다.
        public void ReleaseAll()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                // 아직 겹친 항목은 남겨 두면 FixedTick이 겹침이 풀릴 때(또는 하드 캡) 복구한다.
                if (_entries[i].Platform == null || !IsOverlapped(_entries[i].Platform))
                    Restore(i);
            }
        }

        // 창이 닫혀 발판이 꺼질 때. 남은 무시를 모두 되돌린다.
        public void Clear()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
                Restore(i);
        }

        private bool IsIgnored(Collider2D platform)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.Platform == platform)
                    return true;
            }

            return false;
        }

        private bool IsOverlapped(Collider2D platform)
        {
            ColliderDistance2D distance = _player.Distance(platform);
            return !distance.isValid || distance.distance < SeparatedDistance;
        }

        private void Restore(int index)
        {
            Collider2D platform = _entries[index].Platform;
            if (platform != null)
                Physics2D.IgnoreCollision(_player, platform, false);

            _entries.RemoveAt(index);
        }
    }
}
