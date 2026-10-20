using UnityEngine;

namespace LDY.Script
{
    public readonly struct PlayerAnimInput
    {
        public readonly Vector2 Velocity;
        public readonly bool Grounded;
        public readonly bool Locked;
        public readonly float DeltaTime;
        public readonly bool ClipFinished;
        public readonly int ClipFrame;

        public PlayerAnimInput(Vector2 velocity, bool grounded, bool locked, float deltaTime,
            bool clipFinished, int clipFrame)
        {
            Velocity = velocity;
            Grounded = grounded;
            Locked = locked;
            DeltaTime = deltaTime;
            ClipFinished = clipFinished;
            ClipFrame = clipFrame;
        }
    }

    public class PlayerAnimStateMachine
    {
        private readonly PlayerAnimRules _rules;
        private float _idleTimer;

        public PlayerAnimStateMachine(PlayerAnimRules rules)
        {
            _rules = rules;
        }

        public PlayerAnimId Current { get; private set; } = PlayerAnimId.Idle;
        public bool Changed { get; private set; }
        public int EntryFrame { get; private set; }

        public PlayerAnimId Tick(in PlayerAnimInput input)
        {
            Changed = false;
            EntryFrame = 0;

            float speed = Mathf.Abs(input.Velocity.x);
            bool moving = speed >= _rules.idleSpeedThreshold;

            switch (Current)
            {
                case PlayerAnimId.StandUp:
                    if (input.ClipFinished)
                        Enter(Locomotion(input, speed));
                    break;

                case PlayerAnimId.SitDown:
                    if (moving || !input.Grounded)
                        InterruptSit(input, speed);
                    else if (input.ClipFinished)
                        Enter(PlayerAnimId.SitLoop);
                    break;

                case PlayerAnimId.SitLoop:
                    if (moving || !input.Grounded)
                        Enter(PlayerAnimId.StandUp);
                    break;

                default:
                    TickFree(input, speed);
                    break;
            }

            return Current;
        }

        private void TickFree(in PlayerAnimInput input, float speed)
        {
            PlayerAnimId target = Locomotion(input, speed);
            if (target != PlayerAnimId.Idle)
            {
                if (target != Current)
                    Enter(target);
                return;
            }

            if (Current != PlayerAnimId.Idle)
            {
                Enter(PlayerAnimId.Idle);
                return;
            }

            // 잠금 중에는 앉기 타이머가 멈춤
            if (input.Locked)
                return;

            _idleTimer += input.DeltaTime;
            if (_idleTimer >= _rules.sitDelay)
                Enter(PlayerAnimId.SitDown);
        }

        private void InterruptSit(in PlayerAnimInput input, float speed)
        {
            int entry = _rules.StandUpEntryFor(input.ClipFrame);
            if (entry < 0)
            {
                Enter(Locomotion(input, speed));
                return;
            }

            Enter(PlayerAnimId.StandUp);
            EntryFrame = entry;
        }

        // Run은 진입/이탈 속도가 달라서 깜빡이지 않음
        private PlayerAnimId Locomotion(in PlayerAnimInput input, float speed)
        {
            if (!input.Grounded)
                return input.Velocity.y > 0f ? PlayerAnimId.Jump : PlayerAnimId.Fall;

            if (speed < _rules.idleSpeedThreshold)
                return PlayerAnimId.Idle;

            float runSpeed = Current == PlayerAnimId.Run ? _rules.runExitSpeed : _rules.runEnterSpeed;
            return speed >= runSpeed ? PlayerAnimId.Run : PlayerAnimId.Walk;
        }

        private void Enter(PlayerAnimId next)
        {
            if (next == Current)
                return;

            Current = next;
            Changed = true;
            _idleTimer = 0f;
        }
    }
}
