using System.Collections;
using UnityEngine;

namespace HES.Script.Object {
    public class BreakableBlock : MonoBehaviour {
        [SerializeField] private float breakTime;

        private void OnCollisionEnter2D(Collision2D other) {
            StartCoroutine(Break());
        }

        private IEnumerator Break() {
            yield return new WaitForSeconds(breakTime);
            Destroy(gameObject);
        }
    }
}