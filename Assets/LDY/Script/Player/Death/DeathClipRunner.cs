using System.Collections.Generic;
using System.Linq;
using LDY.Death;
using UnityEngine;

namespace LDY.Script {
    // 사망 클립을 순서대로 스프라이트에 그리고, 끝난 뒤 지연이 지나면 Tick이 한 번 true를 돌려준다.
    internal sealed class DeathClipRunner {
        private readonly SpriteRenderer _target;
        private readonly PlayerAnimClip[] _clips;
        private readonly DeathTimeline _timeline;
        private readonly SpriteFramePlayer _frames = new SpriteFramePlayer();

        public DeathClipRunner(SpriteRenderer target, PlayerDeathSettings settings, IReadOnlyList<PlayerAnimId> order) {
            _target = target;
            _clips = Resolve(settings != null ? settings.AnimSet : null, order);
            var delay = settings != null ? settings.RestartDelay : PlayerDeathSettings.DefaultRestartDelay;
            _timeline = new DeathTimeline(_clips.Select(c => c.TotalDuration).ToArray(), delay);
            MatchPixelSize();
            PlayCurrent();
        }

        public bool Tick(float deltaTime) {
            _timeline.Tick(deltaTime);
            if (_timeline.ClipChanged)
                PlayCurrent();
            else
                _frames.Tick(deltaTime, 1f);

            Apply();
            return _timeline.RestartDue;
        }

        private static PlayerAnimClip[] Resolve(PlayerAnimSet set, IReadOnlyList<PlayerAnimId> order) {
            if (set == null)
                return new PlayerAnimClip[0];

            return order.Where(set.Has).Select(set.Resolve).Where(c => c != null && c.IsValid).ToArray();
        }

        private void MatchPixelSize() {
            if (_target == null || _clips.Length == 0)
                return;

            var shown = _target.sprite != null ? _target.sprite.pixelsPerUnit : 0f;
            var clip = _clips[0].frames[0] != null ? _clips[0].frames[0].pixelsPerUnit : 0f;
            _target.transform.localScale *= PixelScale.Ratio(shown, clip);
        }

        private void PlayCurrent() {
            var index = _timeline.ClipIndex;
            if (index < 0)
                return;

            _frames.Play(_clips[index]);
            Apply();
        }

        private void Apply() {
            var sprite = _frames.Sprite;
            if (_target != null && sprite != null)
                _target.sprite = sprite;
        }
    }
}
