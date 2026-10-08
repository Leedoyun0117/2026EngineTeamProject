using UnityEngine;

namespace LDY.Script
{
    public class Rigidbody2DPlayerBody : IPlayerBody
    {
        private readonly Rigidbody2D _rb;

        public Rigidbody2DPlayerBody(Rigidbody2D rb)
        {
            _rb = rb;
        }

        public Vector2 Position
        {
            get => _rb.position;
            set
            {
                _rb.position = value;
                Vector3 p = _rb.transform.position;
                _rb.transform.position = new Vector3(value.x, value.y, p.z);
            }
        }

        public Vector2 Velocity
        {
            get => _rb.linearVelocity;
            set => _rb.linearVelocity = value;
        }

        public bool PhysicsEnabled
        {
            get => _rb.simulated;
            set => _rb.simulated = value;
        }

        public void SyncTransforms()
        {
            Physics2D.SyncTransforms();
        }
    }
}
