using System;
using UnityEngine;

namespace JJB.Script
{
    public class Gear : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed;

        private void Update()
        {
            transform.Rotate(0,0, rotateSpeed);
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            Debug.Log("Collision Enter: " + other.gameObject.name);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"충돌 대상: {other.name}");
            Debug.Log($"충돌 대상 Layer: {other.gameObject.layer}");

            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log("asdfasdf");
            }
        }
    }
}