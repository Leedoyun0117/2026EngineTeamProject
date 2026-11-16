using UnityEngine;

namespace LDY.Script {
    [DisallowMultipleComponent]
    public class PlayerGripRig : MonoBehaviour, IRopeGripSource {
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Transform gripPivot;
        [SerializeField] private RopeGripProfile profile;
        [SerializeField] private RopeSettings settings;

        private GripBlend _blend;
        private IRopeAngleSource _angle;
        private IRopeStateSource _rope;
        private Vector3 _restPosition;

        private void Awake() {
            _restPosition = transform.localPosition;
            _blend = new GripBlend(settings != null ? settings.Grip.blendTime : 0f);
            _angle = GetComponentInParent<IRopeAngleSource>();
            _rope = GetComponentInParent<IRopeStateSource>();
        }

        private void LateUpdate() {
            if (Time.timeScale <= 0f || sprite == null || sprite.sprite == null || profile == null)
                return;

            _angle ??= GetComponentInParent<IRopeAngleSource>();
            _rope ??= GetComponentInParent<IRopeStateSource>();

            var weight = _blend.Step(_rope != null && _rope.IsRopeHanging, Time.deltaTime);
            var layout = profile.LayoutFor(sprite.sprite.pixelsPerUnit);
            var handLocal = (Vector3)layout.HandLocal(sprite.flipX);
            var handInParent = (Vector3)layout.Scaled(transform.localScale.y).HandLocal(sprite.flipX);
            var angle = GripFacing.LocalAngle(_angle != null ? _angle.RopeAngle : 0f, RootMirrored);

            transform.localPosition = Vector3.Lerp(_restPosition, -handInParent, weight);
            gripPivot.localPosition = handLocal;
            gripPivot.localRotation = Quaternion.Euler(0f, 0f, angle * weight * Mathf.Rad2Deg);
            sprite.transform.localPosition = -handLocal;
        }

        public bool TryGetSample(out GripSample sample) {
            var current = sprite != null ? sprite.sprite : null;
            if (current == null || profile == null) {
                sample = default;
                return false;
            }

            var hasFoot = false;
            var foot = Vector2.zero;
            if (AsepriteFrameName.TryParse(current.name, out var frame))
                hasFoot = profile.TryGetFoot(frame, out foot);

            var flipped = GripFacing.IsWorldFlipped(sprite.flipX, RootMirrored);
            var layout = profile.LayoutFor(current.pixelsPerUnit).Scaled(transform.lossyScale.y);
            sample = new GripSample(layout, flipped, hasFoot, foot);
            return true;
        }

        private bool RootMirrored => GripFacing.IsRootMirrored(transform.right.x);
    }
}
