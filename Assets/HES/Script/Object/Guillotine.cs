using System;
using DG.Tweening;
using UnityEngine;

namespace HES.Script.Object {
    public class Guillotine : MonoBehaviour {
        private void Awake() {
            transform.DORotate(new Vector3(0, 0, -60), 2f).SetEase(Ease.InOutQuint).SetLoops(-1, LoopType.Yoyo);
        }
    }
}