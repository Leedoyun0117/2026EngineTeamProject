using System;
using DG.Tweening;
using UnityEngine;

namespace HES.Script.Object {
    public class Guillotine : MonoBehaviour {
        [SerializeField] private float time;
        [SerializeField] private AnimationCurve curve;
        
        private void Awake() {
            transform.DORotate(new Vector3(0, 0, -60), time).SetEase(curve).SetLoops(-1, LoopType.Yoyo);
        }
    }
}