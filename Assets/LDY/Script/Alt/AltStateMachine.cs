using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    public interface IAltState
    {
        AltPhase Phase { get; } // Phase 상태를 가지고 왔음.
        void Enter(); // FSM에서 쓰는 3개 가지고 옴
        void Tick(float unscaledDeltaTime);
        void Exit();
    }

    public interface IAltStateSwitcher
    {
        void Switch<T>() where T : IAltState; // 상태를 바꿔 달라고 요청하는 창구
    }

    public class AltStateMachine : IAltStateSwitcher
    {
        private readonly Dictionary<Type, IAltState> _states = new Dictionary<Type, IAltState>(); //State 타입을 열쇠로 해서 상태 객체를 찾는 사전

        public IAltState Current { get; private set; } // 프로퍼티로 값 변경 못하게 만듬
        public event Action<AltPhase> PhaseChanged;// PhaseChanged 라는 이벤트 만듬. 상태가 바뀔 때 다른 곳에 알려 주는 방송

        public void Register(IAltState state)
        {
            _states[state.GetType()] = state; // 상태를 만들어서 사전에 넣어 두는 일. 등록 안 된 상태로 Switch하면 에러 발생
        }

        public void Switch<T>() where T : IAltState
        {
            // Exit가 실패해도 Current 갱신은 끝나야 상태가 어긋나지 않는다.
            try
            {
                Current?.Exit(); //현재 값이 널이 아니라면 Exit()을 실행
            }
            catch (Exception e) 
            {
                Debug.LogException(e); //예외 발생시 위험로그로 띄우고
            }

            Current = _states[typeof(T)]; //Current에 현재 타입을 넣어주고
            Current.Enter(); // Current의 Enter()를 실행
            PhaseChanged?.Invoke(Current.Phase); //PhaseChanged 이벤트도 현재 타입에 맞게 해주고
        }

        public void Tick(float unscaledDeltaTime)
        {
            Current?.Tick(unscaledDeltaTime); // 매 프레임 현재 상태에게 일 시키기
        }
    }
}
