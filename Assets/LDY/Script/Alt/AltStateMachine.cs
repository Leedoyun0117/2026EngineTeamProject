using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    public interface IAltState
    {
        AltPhase Phase { get; }
        void Enter();
        void Tick(float unscaledDeltaTime);
        void Exit();
    }

    public interface IAltStateSwitcher
    {
        void Switch<T>() where T : IAltState;
    }

    public class AltStateMachine : IAltStateSwitcher
    {
        private readonly Dictionary<Type, IAltState> _states = new Dictionary<Type, IAltState>();

        public IAltState Current { get; private set; }
        public event Action<AltPhase> PhaseChanged;

        public void Register(IAltState state)
        {
            _states[state.GetType()] = state;
        }

        public void Switch<T>() where T : IAltState
        {
            // Exit에서 예외가 나도 Current는 넘어가야 상태가 안 꼬임
            try
            {
                Current?.Exit();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Current = _states[typeof(T)];
            Current.Enter();
            PhaseChanged?.Invoke(Current.Phase);
        }

        public void Tick(float unscaledDeltaTime)
        {
            Current?.Tick(unscaledDeltaTime);
        }
    }
}
