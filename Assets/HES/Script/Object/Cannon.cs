using System;
using System.Collections;
using UnityEngine;

namespace HES.Script.Object {
    public class Cannon : MonoBehaviour {
        [SerializeField] private float coolTime;
        [SerializeField] private GameObject bulletPrefab;
        
        private void Awake() {
            StartCoroutine(Shoot());
        }

        private IEnumerator Shoot() {
            var wait = new WaitForSeconds(coolTime);
            while (true) {
                Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                yield return wait;
            }
        }
    }
}