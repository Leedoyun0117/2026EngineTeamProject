using JJB.Script.Player;
using JJB.Script.Player.Interface;
using UnityEngine;

namespace LDY.Script {
    // JJB 상호작용(F) 진입점. TryGrab 호출만 하는 얇은 어댑터라 줄 로직이 JJB 타입에 오염되지 않는다.
    [RequireComponent(typeof(RopeHangController))]
    public class RopeInteractable : JJBInteractable {
        private IRopeGrabber _grabber;

        private void Awake() {
            _grabber = GetComponent<IRopeGrabber>();
        }

        // 상호작용 성공 여부 = 실제로 잡았는지
        public override bool Interact(JJBPlayerInteract target) {
            return _grabber != null && _grabber.TryGrab();
        }
    }
}
