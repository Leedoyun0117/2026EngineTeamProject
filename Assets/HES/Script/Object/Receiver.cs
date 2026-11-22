using System;
using System.Collections.Generic;
using JJB.Script;
using UnityEngine;

namespace HES.Script.Object {
    public class Receiver : MonoSingleton<Receiver> {
        [SerializeField] private List<GameObject> receivers;

        public bool response;

        private void Update() {
            foreach (var receiver in receivers) {
                if(receiver.CompareTag("Invisible"))
                    receiver.SetActive(!response);
                if(receiver.CompareTag("Visible"))
                    receiver.SetActive(response);
            }
        }
    }
}