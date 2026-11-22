using LDY.PlayerMerge;
using UnityEngine;

namespace LDY.Script {
    internal sealed class LegacyVisualSwitch {
        private readonly Transform _root;
        private readonly MergeLog _log;

        public LegacyVisualSwitch(Transform root, MergeLog log) {
            _root = root;
            _log = log;
        }

        public void Run() {
            var visual = _root.Find(MergePolicy.VisualChild);
            if (visual == null) {
                _log.Notes.Add($"{MergePolicy.VisualChild} 오브젝트가 없어 끄기를 건너뜀");
                return;
            }

            if (visual.TryGetComponent<SpriteRenderer>(out var sprite) && sprite.enabled) {
                sprite.enabled = false;
                _log.Changed.Add($"{MergePolicy.VisualChild}.SpriteRenderer.enabled: True -> False");
            }

            if (visual.TryGetComponent<Animator>(out var animator) && animator.enabled) {
                animator.enabled = false;
                _log.Changed.Add($"{MergePolicy.VisualChild}.Animator.enabled: True -> False");
            }
        }
    }
}
