using UnityEngine;

namespace LDY.Script
{
    // 컷씬 등 외부에서는 Lock.Acquire()로 잠그고 토큰을 Dispose해서 해제한다.
    public class PlayerInputLock : MonoBehaviour
    {
        private InputLockService _service;

        public IInputLock Lock => _service ??= new InputLockService();
    }
}
