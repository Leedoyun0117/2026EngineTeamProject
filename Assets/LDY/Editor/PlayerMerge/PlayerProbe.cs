using System.Collections.Generic;
using System.Linq;
using LDY.PlayerMerge;
using UnityEditor;
using UnityEngine;

namespace LDY.Script {
    // 플레이어 하나의 값을 루트 기준 월드 단위로 읽는다. 아무것도 바꾸지 않는다.
    internal sealed class PlayerProbe {
        private readonly GameObject _root;

        public PlayerProbe(GameObject root) {
            _root = root;
        }

        public GameObject Root => _root;
        public Float2 RootScale => _root.transform.lossyScale.ToFloat2();

        public bool TryCollider(out Float2 offset, out Float2 size) {
            offset = size = default;
            if (!_root.TryGetComponent<CapsuleCollider2D>(out var collider))
                return false;

            var scale = _root.transform.lossyScale;
            offset = new Float2(collider.offset.x * scale.x, collider.offset.y * scale.y);
            size = new Float2(Mathf.Abs(collider.size.x * scale.x), Mathf.Abs(collider.size.y * scale.y));
            return true;
        }

        public bool TryGroundCheck(out Float2 position, out Float2 size) {
            position = size = default;
            var jump = ComponentNames.Find(_root, "JJBPlayerJump");
            if (jump == null)
                return false;

            var so = new SerializedObject(jump);
            var point = so.FindProperty("groundCheck")?.objectReferenceValue as Transform;
            if (point == null)
                return false;

            position = Relative(point.position);
            size = so.FindProperty("groundCheckSize").vector2Value.ToFloat2();
            return true;
        }

        public int Mask(string componentName, string field) {
            var component = ComponentNames.Find(_root, componentName);
            var property = component != null ? new SerializedObject(component).FindProperty(field) : null;
            return property != null ? property.intValue : 0;
        }

        public int Constraints => _root.TryGetComponent<Rigidbody2D>(out var body) ? (int)body.constraints : 0;

        public int MissingScripts =>
            _root.GetComponentsInChildren<Transform>(true)
                .Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));

        public List<SpriteRenderer> ShownRenderers =>
            _root.GetComponentsInChildren<SpriteRenderer>(true).Where(r => r.enabled && ActiveInPrefab(r.transform)).ToList();

        public int Animators => _root.GetComponentsInChildren<Animator>(true).Length;

        public SpriteRenderer RendererTarget => _root.GetComponentInChildren<PlayerRenderer>(true)?.Target;

        public SpriteRenderer AltSprite {
            get {
                var alt = _root.GetComponent<AltModeController>();
                return alt != null ? new SerializedObject(alt).FindProperty("playerSprite").objectReferenceValue as SpriteRenderer : null;
            }
        }

        public bool ChildActive(string name) {
            var child = _root.transform.Find(name);
            return child != null && child.gameObject.activeSelf;
        }

        public bool HasChild(string name) => _root.transform.Find(name) != null;

        public float FeetY(SpriteRenderer renderer, Sprite sprite, OpaqueRows rows) =>
            Relative(renderer.transform.position).Y
            + (rows.Lowest - sprite.pivot.y) / sprite.pixelsPerUnit * renderer.transform.lossyScale.y;

        private Float2 Relative(Vector3 world) => (world - _root.transform.position).ToFloat2();

        private bool ActiveInPrefab(Transform node) {
            for (var t = node; t != null; t = t.parent)
                if (!t.gameObject.activeSelf)
                    return false;

            return true;
        }
    }
}
