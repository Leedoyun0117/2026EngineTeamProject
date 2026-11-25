using System;
using LDY.StageSelect;

namespace LDY.Script {
    // 선택 화면이 막혀 있으면(대화상자·설정창·실행 중...) 메뉴 실행을 막는다. 타이틀의 MenuGate에 넣는다.
    public sealed class FileBoardBlocker : IMenuBlocker {
        private readonly IFileBoardHost _host;

        public FileBoardBlocker(IFileBoardHost host) {
            _host = host;
        }

        public bool Blocking => _host.Blocking;
    }

    // 화면이 막혀 있는 동안 플레이어 입력을 잠근다(기존 PlayerInputLock 토큰). 풀리면 토큰을 반납한다.
    // 잠금은 TitlePlayerControl을 거쳐 이동·점프를 멈추고, Alt 키와 상호작용 키도 같이 막힌다.
    public sealed class FileBoardInputLock : IDisposable {
        private readonly IFileBoardHost _host;
        private readonly IInputLock _lock;
        private IDisposable _token;

        public FileBoardInputLock(IFileBoardHost host, IInputLock inputLock) {
            _host = host;
            _lock = inputLock;
        }

        public void Dispose() {
            _token?.Dispose();
            _token = null;
        }

        public void Tick() {
            var blocking = _host.Blocking;
            if (blocking && _token == null)
                _token = _lock.Acquire();
            else if (!blocking && _token != null)
                Dispose();
        }
    }
}
