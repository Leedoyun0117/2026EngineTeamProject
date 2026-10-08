using UnityEngine;

namespace LDY.Script
{
    public class AltState : IAltState
    {
        private readonly IAltInput _input;
        private readonly IPointerSource _pointer;
        private readonly IPlayerBody _body;
        private readonly IFacing _facing;
        private readonly IPlayerControl _control;
        private readonly IAfterimageSpawner _afterimage;
        private readonly ITimeFreezer _freezer;
        private readonly ICameraLock _camera;
        private readonly AltSession _session;
        private readonly IAltStateSwitcher _switcher;

        public AltState(IAltInput input, IPointerSource pointer, IPlayerBody body, IFacing facing,
            IPlayerControl control, IAfterimageSpawner afterimage, ITimeFreezer freezer,
            ICameraLock camera, AltSession session, IAltStateSwitcher switcher)
        {
            _input = input;
            _pointer = pointer;
            _body = body;
            _facing = facing;
            _control = control;
            _afterimage = afterimage;
            _freezer = freezer;
            _camera = camera;
            _session = session;
            _switcher = switcher;
        }

        public AltPhase Phase => AltPhase.Alt;

        public void Enter()
        {
            _session.Snapshot = new AltSnapshot(_body.Position, _body.Velocity, _facing.Capture());
            _session.Afterimage = _afterimage.Spawn();

            _control.SetSuspended(true);
            _body.Velocity = Vector2.zero;
            _body.PhysicsEnabled = false;
            _camera.Freeze();
            _freezer.Freeze();
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (!_input.Held)
            {
                _switcher.Switch<ReturnState>();
                return;
            }

            _body.Position = _pointer.WorldPosition;
        }

        public void Exit() { }
    }
}
