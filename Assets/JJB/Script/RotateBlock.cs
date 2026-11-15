using System;
using UnityEngine;

namespace JJB.Script
{
    public class RotateBlock : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed = 10f;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            transform.Rotate(0, 0, rotateSpeed);
        }
    }
}
