using UnityEngine;

public class TestPlayerMove_HTY : MonoBehaviour {
    [field: SerializeField] public bool _isPlayObj { get; private set; } = true;
    private Rigidbody2D _rb;
    private readonly float _speed = 7f;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        if (_isPlayObj) _rb.linearVelocityX = _speed * GameManager_HTY.instance._inputValue._moveDir.x;
        else _rb.linearVelocityX = Vector2.zero.x;
    }

    public void SetPlay(bool isPlay) {
        _isPlayObj = isPlay;
    }
}