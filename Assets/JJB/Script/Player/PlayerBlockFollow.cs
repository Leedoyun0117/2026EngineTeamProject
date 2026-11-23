using UnityEngine;

namespace JJB.Script.Player
{
    public class PlayerBlockFollow : MonoBehaviour
    {
        private Transform _movingPlatform;
        private Transform _originalParent;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryAttach(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryAttach(collision);
        }

        private void TryAttach(Collision2D collision)
        {
            if (collision.rigidbody == null)
                return;
            
            if (!collision.rigidbody.CompareTag("MovingBlock"))
                return;

            bool isStandingOnTop = false;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isStandingOnTop = true;
                    break;
                }
            }

            if (!isStandingOnTop)
                return;

            Transform platform = collision.rigidbody.transform;

            if (_movingPlatform == platform)
                return;

            _originalParent = transform.parent;
            _movingPlatform = platform;

            transform.SetParent(_movingPlatform, true);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (_movingPlatform == null || collision.rigidbody == null)
                return;

            if (collision.rigidbody.transform != _movingPlatform)
                return;

            Detach();
        }

        private void Detach()
        {
            transform.SetParent(_originalParent, true);

            _movingPlatform = null;
        }

        private void OnDisable()
        {
            if (_movingPlatform != null)
                Detach();
        }
    }
}