using System;

namespace LDY.Script
{
    public class InputLockService : IInputLock
    {
        private int _count;

        public bool IsLocked => _count > 0;
        public event Action<bool> LockChanged;

        public IDisposable Acquire()
        {
            _count++;
            if (_count == 1)
                LockChanged?.Invoke(true);

            return new Token(this);
        }

        private void Release()
        {
            _count--;
            if (_count == 0)
                LockChanged?.Invoke(false);
        }

        private sealed class Token : IDisposable
        {
            private InputLockService _owner;

            public Token(InputLockService owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner == null)
                    return;

                InputLockService owner = _owner;
                _owner = null;
                owner.Release();
            }
        }
    }
}
