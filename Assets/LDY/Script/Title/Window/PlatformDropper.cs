using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 창 발판 위에서 아래로 내려가기. 서 있는 발판과 플레이어 충돌을 무시하고, 시간이 아니라 몸이 발판에서 빠졌는지로 복구한다.
    // 시작은 Update(입력), 복구 검사는 FixedUpdate(물리)에서 한다.
    public sealed class PlatformDropper
    {
        // 상승 중으로 보지 않는 최대 위쪽 속도
        private const float RisingSpeed = 0.1f;
        // 정지로 보는 속도
        private const float RestSpeed = 0.1f;
        // 겹침이 풀렸다고 보는 최소 거리. Physics2D.defaultContactOffset(0.01)보다 확실히 크게 잡는다.
        private const float SeparatedDistance = 0.05f;
        // 시작 직후에는 접촉 오프셋 때문에 분리처럼 보일 수 있으므로, 이만큼 FixedUpdate가 지나고 이만큼 내려간 뒤부터 판정한다.
        private const int MinFixedTicks = 3;
        private const float MinFallDistance = 0.2f;
        private const float HardCapSeconds = 2f;
        // 하드 캡에서 위치 조건이 안 맞을 때 빠져나가도록 주는 아래 속도(비율은 downSpeed 기준)
        private const float EscapeSpeedRatio = 0.5f;

        private sealed class Entry
        {
            public Collider2D Platform;
            public float Elapsed;
            public int Ticks;
            public float StartFootY;
            public bool CapWarned;
        }

        private readonly IReadOnlyList<Collider2D> _platforms;
        private readonly Collider2D _player;
        private readonly IPlayerBody _body;
        private readonly float _maxIgnoreSeconds;
        private readonly float _downSpeed;
        private readonly bool _debug;
        private readonly List<Entry> _entries = new List<Entry>();

        public PlatformDropper(IReadOnlyList<Collider2D> platforms, Collider2D player, IPlayerBody body,
            float maxIgnoreSeconds, float downSpeed, bool debug = false)
        {
            _platforms = platforms;
            _player = player;
            _body = body;
            _maxIgnoreSeconds = maxIgnoreSeconds;
            _downSpeed = downSpeed;
            _debug = debug;
        }

        // 발판 위에 서 있을 때만 시작한다(점프/상승 중이면 무시). 같은 발판은 무시 중에 다시 시작하지 않는다.
        public void TryDrop()
        {
            if (_body.Velocity.y > RisingSpeed)
                return;

            for (int i = 0; i < _platforms.Count; i++)
            {
                Collider2D platform = _platforms[i];
                if (platform == null || !platform.enabled || !platform.gameObject.activeInHierarchy || IsIgnored(platform))
                    continue;

                if (!_player.IsTouching(platform) || _player.bounds.min.y < platform.bounds.max.y - TitleTuning.StandTolerance)
                    continue;

                Physics2D.IgnoreCollision(_player, platform, true);
                _entries.Add(new Entry { Platform = platform, StartFootY = _player.bounds.min.y });
                Log($"시작 {platform.name} footY={_player.bounds.min.y:F2}");

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
                entry.Ticks++;

                if (entry.Platform == null)
                {
                    Restore(i, "발판 사라짐");
                    continue;
                }

                // 몸 중심이 발판 아래면보다 아래여야 겹침 해소 방향이 아래쪽이다. 아니면 복구 시 발판 위로 튕긴다.
                bool centerBelow = _player.bounds.center.y < entry.Platform.bounds.min.y;
                bool settled = entry.Ticks >= MinFixedTicks && entry.StartFootY - _player.bounds.min.y >= MinFallDistance;

                if (settled && !IsOverlapped(entry.Platform))
                {
                    Restore(i, $"겹침 해소 centerBelow={centerBelow}");
                    continue;
                }

                if (entry.Elapsed >= HardCapSeconds)
                {
                    if (centerBelow)
                    {
                        Debug.LogWarning($"[Window] 내려가기: {entry.Platform.name}와 {HardCapSeconds}초가 지나도 겹쳐 있어 강제로 복구합니다.");
                        Restore(i, "하드 캡");
                    }
                    else
                    {
                        // 지금 복구하면 발판 위로 밀려 올라오므로, 아래로 빠져나가게 하고 계속 기다린다.
                        if (!entry.CapWarned)
                        {
                            entry.CapWarned = true;
                            Debug.LogWarning($"[Window] 내려가기: {entry.Platform.name}와 {HardCapSeconds}초가 지났지만 몸 중심이 발판 아래면 위라 복구하지 않고 아래로 밀어 냅니다.");
                        }

                        Vector2 velocity = _body.Velocity;
                        _body.Velocity = new Vector2(velocity.x, Mathf.Min(velocity.y, _downSpeed * EscapeSpeedRatio));
                        Log($"하드 캡 대기 {entry.Platform.name} centerY={_player.bounds.center.y:F2} platformBottom={entry.Platform.bounds.min.y:F2}");
                    }

                    continue;
                }

                // 위 행 발판에 머리가 걸친 채 아래 행에 착지하면 겹침이 풀리지 않을 수 있다.
                // 제한 시간이 지난 뒤 몸 중심이 발판 아래면보다 아래에서 멈춰 있으면 복구한다.
                bool atRest = Mathf.Abs(_body.Velocity.y) < RestSpeed;
                if (entry.Elapsed >= _maxIgnoreSeconds && centerBelow && atRest)
                    Restore(i, "착지 정지");
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
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Platform == platform)
                    return true;
            }

            return false;
        }

        private bool IsOverlapped(Collider2D platform)
        {
            ColliderDistance2D distance = _player.Distance(platform);
            return !distance.isValid || distance.distance < SeparatedDistance;
        }

        private void Restore(int index, string reason = "해제")
        {
            Collider2D platform = _entries[index].Platform;
            Log($"복구 {(platform != null ? platform.name : "null")} 사유={reason} t={_entries[index].Elapsed:F2} ticks={_entries[index].Ticks}");
            if (platform != null)
                Physics2D.IgnoreCollision(_player, platform, false);

            _entries.RemoveAt(index);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void Log(string message)
        {
            if (_debug)
                Debug.Log($"[PlatformDropper] {message}");
        }
    }
}
