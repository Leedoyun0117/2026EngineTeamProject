using UnityEngine;

namespace LDY.Script
{
    public class Rigidbody2DPlayerBody : IPlayerBody
    {
        private readonly Rigidbody2D _rb;
        private readonly Collider2D _collider;
        private readonly IAltStatus _alt;

        public Rigidbody2DPlayerBody(Rigidbody2D rb)
        {
            _rb = rb;
            _collider = rb != null ? rb.GetComponent<Collider2D>() : null;
            _alt = rb != null ? rb.GetComponent<IAltStatus>() : null;
        }

        // 씬 언로드/파괴 중 복구 경로에서도 안전하도록 Rigidbody2D가 파괴됐으면 건너뛴다.
        // Position은 항상 transform(발) 기준이다. 다만 Alt 중에는 포인터가 몸통 중심에 오도록 충돌체 중심 높이만큼 내려 놓는다.
        public Vector2 Position
        {
            get => _rb != null ? _rb.position : Vector2.zero;
            set
            {
                if (_rb == null)
                    return;

                if (_alt != null && _alt.Phase == AltPhase.Alt)
                    value -= BodyCenterOffset();

                _rb.position = value;
                Vector3 p = _rb.transform.position;
                _rb.transform.position = new Vector3(value.x, value.y, p.z);
            }
        }

        // 충돌체 offset(인스펙터 값, TitleVisualSettings.playerBodyOffset)을 월드 단위로 환산한다.
        private Vector2 BodyCenterOffset()
        {
            if (_collider == null)
                return Vector2.zero;

            return new Vector2(0f, _collider.offset.y * _rb.transform.lossyScale.y);
        }

        public Vector2 Velocity
        {
            get => _rb != null ? _rb.linearVelocity : Vector2.zero;
            set
            {
                if (_rb != null)
                    _rb.linearVelocity = value;
            }
        }

        public bool PhysicsEnabled
        {
            get => _rb != null && _rb.simulated;
            set
            {
                if (_rb != null)
                    _rb.simulated = value;
            }
        }

        public void SyncTransforms()
        {
            Physics2D.SyncTransforms();
        }
    }
}
