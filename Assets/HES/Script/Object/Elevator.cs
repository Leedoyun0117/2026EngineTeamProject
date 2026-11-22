using System;
using DG.Tweening;
using UnityEngine;

namespace HES.Script.Object {
    public class Elevator : MonoBehaviour {
        [field:SerializeField] public float Duration { get; private set; }
        [SerializeField] private float moveY;
        [SerializeField] private bool flip;
        private bool _isWorked;
        
        public void MoveElevator() {
            if(!_isWorked) {
                transform.DOMoveY(flip ? transform.position.y +  moveY * -1 : transform.position.y +  moveY, Duration).SetEase(Ease.InOutSine);
                _isWorked = true;
            }
            else {
                transform.DOMoveY(flip ? transform.position.y +  moveY : transform.position.y +  moveY * -1, Duration).SetEase(Ease.InOutSine);
                _isWorked = false;
            }
        }
    }
}