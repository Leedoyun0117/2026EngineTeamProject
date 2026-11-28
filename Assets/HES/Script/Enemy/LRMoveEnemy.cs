using System;
using System.Collections;
using System.Transactions;
using DG.Tweening;
using UnityEngine;

namespace HES.Script.Enemy {
    public class LRMoveEnemy : MonoBehaviour {
        [SerializeField] private float speed;
        [SerializeField] private float stopTime;
        
        [Header("What is can't Move")]
        [SerializeField] private LayerMask whatIsCantMove;
        [SerializeField] private Vector2 cantMoveSize;
        [SerializeField] private Vector2 cantMoveDetectPos;
        [Header("What is Ground")]
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private Vector2 isGroundSize;
        [SerializeField] private Vector2 isGroundPos;
        
        private Rigidbody2D _rb;
        private bool _isFlip;
        private bool _wait;
        private WaitForSeconds _stop;
        
        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _rb.linearVelocityX = speed;
            _stop = new WaitForSeconds(stopTime);
        }

        private void FixedUpdate() {
            if(_wait) return;
            var cantMoveHit = Physics2D.OverlapBox(transform.position + new Vector3(_isFlip ? -cantMoveDetectPos.x : cantMoveDetectPos.x, cantMoveDetectPos.y), cantMoveSize, 0f, whatIsCantMove);
            var isGroundHit = Physics2D.OverlapBox(transform.position + new Vector3(_isFlip ? -isGroundPos.x : isGroundPos.x, isGroundPos.y), isGroundSize, 0f, whatIsGround);
            if(isGroundHit && !cantMoveHit) {
                _rb.linearVelocityX = _isFlip ? -speed : speed;
                return;
            }
            StartCoroutine(FlipCoroutine());
        }

        private IEnumerator FlipCoroutine() {
            _wait = true;
            _rb.linearVelocityX = 0;
            yield return _stop;
            _wait = false;
            _isFlip = !_isFlip;
            transform.rotation = Quaternion.Euler(0, _isFlip ? 180f : 0f, 0);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + new Vector3(_isFlip ? -cantMoveDetectPos.x : cantMoveDetectPos.x, cantMoveDetectPos.y), cantMoveSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + new Vector3(_isFlip ? -isGroundPos.x : isGroundPos.x, isGroundPos.y), isGroundSize);
        }
    }
}