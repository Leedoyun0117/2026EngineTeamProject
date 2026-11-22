using UnityEngine;
using UnityEngine.InputSystem;

public class PointerMove_HTY : MonoBehaviour
{
    [SerializeField] private float _speed = 7;
    private Vector2 _moveDir;
    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _moveDir * _speed;
    }

    public void OnMove(InputValue value)
    {
        _moveDir = value.Get<Vector2>();
    }
}
