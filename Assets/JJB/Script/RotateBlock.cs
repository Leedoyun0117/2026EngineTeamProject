using System;
using UnityEngine;

namespace JJB.Script
{
    public class RotateBlock : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed = 10f;

        private void Update()
        {
            transform.Rotate(0, 0, rotateSpeed);
        }
    }
}
