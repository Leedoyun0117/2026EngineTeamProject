using System;

namespace LDY.Script
{
    public interface IInputLock
    {
        bool IsLocked { get; }
        event Action<bool> LockChanged;

        // 반환된 토큰을 Dispose하면 해제. 활성 토큰이 하나라도 있으면 잠김.
        IDisposable Acquire();
    }
}
