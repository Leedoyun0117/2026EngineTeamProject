using DG.Tweening;
using UnityEngine;

namespace HES.Script.Object {
    public class Blind : MonoBehaviour {
        [SerializeField] private Ease ease;
        [SerializeField] private float time;
        private readonly Color _blindColor = Color.black;
        private readonly Color _unblind = new(0, 0, 0, 0);

        private SpriteRenderer _renderer;

        private void Awake() {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            _renderer.DOColor(_unblind, time).SetEase(Ease.Linear);
        }

        private void OnTriggerExit2D(Collider2D other) {
            _renderer.DOColor(_blindColor, time).SetEase(Ease.Linear);
        }
    }
}