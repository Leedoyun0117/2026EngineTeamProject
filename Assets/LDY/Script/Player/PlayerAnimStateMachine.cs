using UnityEngine;

namespace LDY.Script
{
    public readonly struct PlayerAnimInput
    {
        public readonly Vector2 Velocity;
        public readonly bool Grounded;
        public readonly bool Locked;
        public readonly float DeltaTime;
        // 지금 재생 중인 클립이 끝났는지, 그리고 현재 프레임 번호
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
        // 이번 Tick에서 상태가 바뀌었는지와, 새 클립을 시작할 프레임
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

            // 땅에 있고 잠금이 없을 때만 센다. 잠금 중에는 멈춘다.
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

        // 공중이면 Jump/Fall, 땅이면 속도로 Idle/Walk/Run을 고른다. Run은 진입/이탈 속도가 다르다.
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
