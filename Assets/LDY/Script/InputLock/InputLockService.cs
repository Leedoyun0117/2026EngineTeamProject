using System;

namespace LDY.Script
{
    public class InputLockService : IInputLock
    {
        private int _count; // 안 돌려준 토큰을 카운트를 할려고 만들어 둠

        public bool IsLocked => _count > 0; // IsLocked 라는 불타입을 만들어서 0 이상일 경우에 참으로 만들어 둠
        public event Action<bool> LockChanged; // LockChanged 이벤트를 만듬. 아마 잠기거나 열릴 때 이 이벤트를 호출하는 것 같음

        public IDisposable Acquire()
        {
            _count++; // 카운트를 추가함
            if (_count == 1) // 카운트가 1일때
                LockChanged?.Invoke(true); //LockedChanged에 true를 호출함

            return new Token(this); // 토큰 클래스에 반환함
        }

        private void Release()
        {
            _count--; // 카운트를 줄이고
            if (_count == 0) // 카운트가 0일 때
                LockChanged?.Invoke(false); // LockedChanged에 false를 호출함
        }

        private sealed class Token : IDisposable // IDisposable을 상속 받음
        {
            private InputLockService _owner; // InputLockService를 변수로 만들고

            public Token(InputLockService owner)
            {
                _owner = owner; // 생성자에 변수 설정 해주고
            }

            public void Dispose()
            {
                if (_owner == null) // 널값일 때 리턴 
                    return;

                InputLockService owner = _owner; //owner에 설정한 _owner 넣어주고
                _owner = null; // null로 만든 후
                owner.Release(); // Releasse를 실행함.
            }
        }
    }
}
