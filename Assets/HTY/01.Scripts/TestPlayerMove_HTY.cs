using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerMove_HTY : MonoBehaviour
{
    private float _speed = 7f;
    private Rigidbody2D _rb;
    [field:SerializeField] public bool _isPlayObj {  get; private set; } = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_isPlayObj) _rb.linearVelocityX = _speed * GameManager_HTY.instance._inputValue._moveDir.x;
        else _rb.linearVelocityX = Vector2.zero.x;
    }

    public void SetPlay(bool isPlay)
    {
        _isPlayObj = isPlay;
    }
}
