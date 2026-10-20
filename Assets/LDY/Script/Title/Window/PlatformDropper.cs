using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    // 시간이 아니라 몸이 발판에서 빠졌는지로 복구함. 시작은 Update, 복구 검사는 FixedUpdate
    public sealed class PlatformDropper
    {
        private const float RisingSpeed = 0.1f;
        // defaultContactOffset(0.01)보다 확실히 크게
        private const float SeparatedDistance = 0.05f;
        // 시작 직후엔 접촉 오프셋 때문에 분리된 것처럼 보여서, 이만큼 지난 뒤부터 판정
        private const int MinFixedTicks = 3;
        private const float MinFallDistance = 0.2f;
        private const float HardCapSeconds = 2f;
        // 하드 캡에서 위치 조건이 안 맞을 때 빠져나가게 주는 속도(downSpeed 비율)
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
        private Collider2D _undroppable;

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

        // 맨 아래 발판은 내려가면 창 밖이라 막음
        public void SetUndroppable(Collider2D platform)
        {
            _undroppable = platform;
        }

        // 서 있을 때만 시작(점프/상승 중이면 무시)
        public void TryDrop()
        {
            if (_body.Velocity.y > RisingSpeed)
                return;

            for (int i = 0; i < _platforms.Count; i++)
            {
                Collider2D platform = _platforms[i];
                if (platform == null || platform == _undroppable || !platform.enabled
                    || !platform.gameObject.activeInHierarchy || IsIgnored(platform))
                    continue;

                if (!_player.IsTouching(platform) || _player.bounds.min.y < platform.bounds.max.y - TitleTuning.StandTolerance)
                    continue;

                Physics2D.IgnoreCollision(_player, platform, true);
                _entries.Add(new Entry { Platform = platform, StartFootY = _player.bounds.min.y });
                Log($"시작 {platform.name} footY={_player.bounds.min.y:F2}");

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

                // 중심이 발판 아래면보다 아래여야 함. 아니면 복구할 때 발판 위로 튕김
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
                        Debug.LogWarning($"[Window] 내려가기: {entry.Platform.name}와 {HardCapSeconds}초 넘게 겹쳐서 강제 복구");
                        Restore(i, "하드 캡");
                    }
                    else
                    {
                        // 지금 복구하면 발판 위로 튕겨서 아래로 빼내며 계속 대기
                        if (!entry.CapWarned)
                        {
                            entry.CapWarned = true;
                            Debug.LogWarning($"[Window] 내려가기: {entry.Platform.name}와 {HardCapSeconds}초 지났지만 중심이 발판 아래면 위라 복구 보류, 아래로 밀어냄");
                        }

                        Vector2 velocity = _body.Velocity;
                        _body.Velocity = new Vector2(velocity.x, Mathf.Min(velocity.y, _downSpeed * EscapeSpeedRatio));
                        Log($"하드 캡 대기 {entry.Platform.name} centerY={_player.bounds.center.y:F2} platformBottom={entry.Platform.bounds.min.y:F2}");
                    }

                    continue;
                }

                // 위 행 발판에 머리가 걸친 채 아래 행에 착지하면 겹침이 안 풀릴 수 있음
                // 그래서 제한 시간 뒤 중심이 아래에서 멈춰 있으면 복구
                bool atRest = Mathf.Abs(_body.Velocity.y) < TitleTuning.RestSpeed;
                if (entry.Elapsed >= _maxIgnoreSeconds && centerBelow && atRest)
                    Restore(i, "착지 정지");
            }
        }

        // 겹침 풀린 발판은 바로 복구, 겹쳐 있으면 풀릴 때까지 그 발판만 무시 유지
        public void ReleaseAll()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].Platform == null || !IsOverlapped(_entries[i].Platform))
                    Restore(i);
            }
        }

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
