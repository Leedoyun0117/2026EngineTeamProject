using UnityEngine;

namespace JJB.Script.MovingBlock
{
    [RequireComponent(typeof(MovingBlockMotor))]
    public class MovingBlock : MonoBehaviour, IPushSource
    {
        private MovingBlockMotor _motor;
        private PushableBlock _pushBlock;

        private void Awake()
        {
            _motor = GetComponent<MovingBlockMotor>();
        }

        private void OnEnable()
        {
            _motor.OnObstacleDetected += HandleObstacle;
            _motor.StartMoving();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Reverse"))
            {
                Reverse();
            }
        }

        private void HandleObstacle(Collider2D obstacle)
        {
            PushableBlock block = obstacle != null
                ? obstacle.GetComponentInParent<PushableBlock>()
                : null;

            if (block == null || !block.CompareTag("PushBlock"))
            {
                ReleaseCurrentBlock();
                return;
            }

            if (_pushBlock != block)
            {
                ReleaseCurrentBlock();
                _pushBlock = block;
            }

            block.ReceivePush(
                this,
                _motor.Dir,
                _motor.MoveSpeed
            );
        }

        public void OnPushBlocked()
        {
            Reverse();
        }

        private void Reverse()
        {
            ReleaseCurrentBlock();
            _motor.ReverseDirection();
        }

        private void ReleaseCurrentBlock()
        {
            if (_pushBlock == null)
                return;

            _pushBlock.ReleasePush(this);
            _pushBlock = null;
        }

        private void OnDisable()
        {
            _motor.OnObstacleDetected -= HandleObstacle;

            ReleaseCurrentBlock();
            _motor.StopMoving();
        }
    }
}