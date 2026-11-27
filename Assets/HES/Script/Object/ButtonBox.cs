using System.Collections.Generic;
using UnityEngine;

namespace HES.Script.Object {
    public class ButtonBox : MonoBehaviour {
        [SerializeField] private List<GameObject> receiver;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Box"))
                foreach(var item in receiver)
                    item.SetActive(false);
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Box"))
                foreach( var item in receiver)
                    item.SetActive(true);
        }
    }
}