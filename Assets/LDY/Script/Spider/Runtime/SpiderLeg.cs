using System;
using UnityEngine;

namespace LDY.Spider {
    public sealed class SpiderLeg : MonoBehaviour, ISeverable {
        [SerializeField] private Transform upper;
        [SerializeField] private Transform lower;
        [SerializeField] private SpriteRenderer upperRenderer;
        [SerializeField] private SpriteRenderer lowerRenderer;
        [SerializeField] private Collider2D needle;
        [SerializeField] private float restUpperDegrees;
        [SerializeField] private float restLowerDegrees;
        [SerializeField] private float bendSign = 1f;
        [SerializeField] private Sprite upperCutSprite;
        [SerializeField] private Sprite lowerPieceSprite;

        private float _upperLength;
        private float _lowerLength;
        private bool _needleActive;

        public bool IsCut { get; private set; }

        public event Action<SpiderLeg> SeverRequested;

        public Vec2 Socket => ((Vector2)upper.position).ToVec2();

        public void Initialize(float tipRadius) {
            _upperLength = upperRenderer.sprite.bounds.size.x * upper.lossyScale.x;
            _lowerLength = lowerRenderer.sprite.bounds.size.x * lower.lossyScale.x;
            if (needle is CircleCollider2D circle)
                circle.radius = tipRadius;

            needle.enabled = false;
        }

        public Vec2 RestFoot() {
            return TwoBoneIK.Forward(Socket, _upperLength, _lowerLength,
                restUpperDegrees * Mathf.Deg2Rad, restLowerDegrees * Mathf.Deg2Rad);
        }

        public void Apply(Vec2 tip) {
            var solution = TwoBoneIK.Solve(Socket, _upperLength, _lowerLength, tip, bendSign);
            upper.rotation = Quaternion.Euler(0f, 0f, solution.UpperAngle * Mathf.Rad2Deg);
            lower.rotation = Quaternion.Euler(0f, 0f, solution.LowerAngle * Mathf.Rad2Deg);
            lower.position = new Vector3(solution.Knee.X, solution.Knee.Y, lower.position.z);
        }

        public void SetNeedleActive(bool active) {
            if (IsCut || _needleActive == active)
                return;

            _needleActive = active;
            needle.enabled = active;
        }

        public void Sever() {
            if (!IsCut && _needleActive)
                SeverRequested?.Invoke(this);
        }

        public void Cut(float pieceLifetime) {
            IsCut = true;
            _needleActive = false;
            Destroy(needle.gameObject);
            if (upperCutSprite != null)
                upperRenderer.sprite = upperCutSprite;
            if (lowerPieceSprite != null)
                lowerRenderer.sprite = lowerPieceSprite;

            upper.rotation = Quaternion.Euler(0f, 0f, restUpperDegrees);
            DetachLower(pieceLifetime);
        }

        private void DetachLower(float lifetime) {
            lower.SetParent(null, true);
            var body = lower.gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 2f;

            var bounds = lowerRenderer.sprite.bounds;
            var box = lower.gameObject.AddComponent<BoxCollider2D>();
            box.size = bounds.size;
            box.offset = bounds.center;
            box.excludeLayers = LayerMask.GetMask("Player");
            Destroy(lower.gameObject, lifetime);
        }
    }
}
