using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    internal sealed class StageComponents {
        private readonly GameObject _root;
        private readonly SpriteRenderer _sprite;
        private readonly MergeLog _log;

        public StageComponents(GameObject root, SpriteRenderer sprite, MergeLog log) {
            _root = root;
            _sprite = sprite;
            _log = log;
        }

        public void Run() {
            Ensure<PlayerInputLock>();
            Ensure<JJBControlGate>();
            Ensure<PlayerAbilityGate>();
            WireAlt(Ensure<AltModeController>());
        }

        private T Ensure<T>() where T : Component {
            if (_root.TryGetComponent<T>(out var existing))
                return existing;

            _log.Changed.Add($"+{typeof(T).Name}");
            return _root.AddComponent<T>();
        }

        private void WireAlt(AltModeController alt) {
            var so = new SerializedObject(alt);
            var property = so.FindProperty("playerSprite");
            if (property.objectReferenceValue == _sprite)
                return;

            property.objectReferenceValue = _sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
            _log.Changed.Add($"AltModeController.playerSprite -> {_sprite.name}");
        }
    }
}
