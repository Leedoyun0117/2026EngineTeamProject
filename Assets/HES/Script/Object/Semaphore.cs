using System;
using UnityEngine;

namespace HES.Script.Object {
    public class Semaphore : ButtonParent {
        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            Receiver.Instance.response = !Receiver.Instance.response;
        }
    }
}