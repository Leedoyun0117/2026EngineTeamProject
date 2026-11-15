using JJB.Script.Player;
using JJB.Script.Player.Interface;
using UnityEngine;

namespace LDY.Script {
    [RequireComponent(typeof(RopeHangController))]
    public class RopeInteractable : JJBInteractable {
        private IRopeGrabber _grabber;

        private void Awake() {
            _grabber = GetComponent<IRopeGrabber>();
        }

        public override bool Interact(JJBPlayerInteract target) {
            return _grabber != null && _grabber.TryGrab();
        }
    }
}
