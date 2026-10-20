using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController_HTY : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private InputActionReference _moveAction;

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _moveRadius = 4f;

    public Vector2 CurrentPosition => transform.position;

    private void OnEnable()
    {
        _moveAction?.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.action.Disable();
    }

    private void Update()
    {
        MovePointer();
    }

    private void MovePointer()
    {
        if (_player == null || _moveAction == null)
            return;

        Vector2 input = _moveAction.action.ReadValue<Vector2>();
        Vector2 movement = input * _moveSpeed * Time.deltaTime;

        Vector2 nextPosition =(Vector2)transform.position + movement;
        Vector2 offset = nextPosition - (Vector2)_player.position;

        offset = Vector2.ClampMagnitude(offset, _moveRadius);

        transform.position = (Vector2)_player.position + offset;
    }

    public void ResetPointer()
    {
        if (_player == null)
            return;

        transform.position = _player.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (_player == null)
            return;

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            _player.position,
            _moveRadius
        );
    }
}