using System;
using UnityEngine;

namespace LDY.Script
{
    public class ReturnState : IAltState
    {
        private readonly IPlayerBody _body;
        private readonly IFacing _facing;
        private readonly IPlayerControl _control;
        private readonly ITimeFreezer _freezer;
        private readonly ICameraLock _camera;
        private readonly IInputLock _inputLock;
        private readonly AltSession _session;
        private readonly AltReturnSettings _settings;
        private readonly IAltStateSwitcher _switcher;

        private IDisposable _lockToken;
        private Vector2 _from;
        private float _duration;
        private float _elapsed;

        public ReturnState(IPlayerBody body, IFacing facing, IPlayerControl control, ITimeFreezer freezer,
            ICameraLock camera, IInputLock inputLock, AltSession session, AltReturnSettings settings,
            IAltStateSwitcher switcher)
        {
            _body = body;
            _facing = facing;
            _control = control;
            _freezer = freezer;
            _camera = camera;
            _inputLock = inputLock;
            _session = session;
            _settings = settings;
            _switcher = switcher;
        }

        public AltPhase Phase => AltPhase.Return;

        public void Enter()
        {
            _lockToken = _inputLock.Acquire();
            _from = _body.Position;
            _duration = _settings.CalculateDuration(Vector2.Distance(_from, _session.Snapshot.Position));
            _elapsed = 0f;
        }

        public void Tick(float unscaledDeltaTime)
        {
            _elapsed += unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            float eased = 1f - (1f - t) * (1f - t);
            _body.Position = Vector2.Lerp(_from, _session.Snapshot.Position, eased);

            float remaining = _duration - _elapsed;
            _session.Afterimage?.SetFade(Mathf.Clamp01(remaining / _settings.afterimageFadeTime));

            if (t >= 1f)
                _switcher.Switch<NormalState>();
        }

        // timeScale 0인 상태에서 한 번의 Update 안에서 끝난다. 순서 변경 금지.
        // 앞 단계가 실패해도 시간, 잠금은 반드시 복구되도록 finally에 둔다.
        public void Exit()
        {
            try
            {
                _session.Afterimage?.Dispose();
                _session.Afterimage = null;

                _body.PhysicsEnabled = true;
                _body.Position = _session.Snapshot.Position;
                _body.Velocity = _session.Snapshot.Velocity;
                _facing.Restore(_session.Snapshot.Facing);
                _body.SyncTransforms();
            }
            finally
            {
                _camera.Release();
                _freezer.Release();
                _control.SetSuspended(false);

                _lockToken?.Dispose();
                _lockToken = null;
            }
        }
    }
}
