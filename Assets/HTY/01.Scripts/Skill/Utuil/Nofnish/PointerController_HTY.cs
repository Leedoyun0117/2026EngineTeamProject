using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController_HTY : MonoBehaviour {
    [Header("References")] [SerializeField]
    private Transform _player;

    [SerializeField] private InputActionReference _moveAction;

    [Header("Settings")] [SerializeField] private float _moveSpeed = 5f;

    [SerializeField] private float _moveRadius = 4f;

    public Vector2 CurrentPosition => transform.position;

    private void Update() {
        MovePointer();
    }

    private void OnEnable() {
        _moveAction?.action.Enable();
    }

    private void OnDisable() {
        _moveAction?.action.Disable();
    }

    private void OnDrawGizmosSelected() {
        if (_player == null)
            return;

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            _player.position,
            _moveRadius
        );
    }

    private void MovePointer() {
        if (_player == null || _moveAction == null)
            return;

        var input = _moveAction.action.ReadValue<Vector2>();
        var movement = input * _moveSpeed * Time.deltaTime;

        var nextPosition = (Vector2)transform.position + movement;
        var offset = nextPosition - (Vector2)_player.position;

        offset = Vector2.ClampMagnitude(offset, _moveRadius);

        transform.position = (Vector2)_player.position + offset;
    }

    public void ResetPointer() {
        if (_player == null)
            return;

        transform.position = _player.position;
    }
}