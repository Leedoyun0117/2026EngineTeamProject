using UnityEngine;
using UnityEngine.InputSystem;

public class CursorResize_HTY : MonoBehaviour, ISkill_HTY {
    [Header("Resize Settings")] [SerializeField]
    private float _reSizeSpeed = 1.5f;

    private bool _canResize;

    private CapsuleCollider2D _coll;
    private CapsuleDirection2D _originalDirection;
    private Vector3 _originalScale;
    private float _reSize;

    private void Awake() {
        _coll = GetComponent<CapsuleCollider2D>();

        _originalScale = transform.localScale;
        _originalDirection = _coll.direction;
    }

    private void Update() {
        if (!CanUse()) return;

        var direction = 0f;

        if (Keyboard.current.upArrowKey.isPressed)
            direction = 1;

        else if (Keyboard.current.downArrowKey.isPressed)
            direction = -1;

        if (direction != 0) TryResize(direction);
    }

    public bool CanUse() {
        return _canResize && _coll != null;
    }

    public void UseSkill() {
        if (_coll == null) return;

        _canResize = !_canResize;

        if (!_canResize)
            if (CanResize(_originalScale)) {
                transform.localScale = _originalScale;
                _coll.direction = _originalDirection;
                Physics2D.SyncTransforms();
            }
    }

    private void TryResize(float direction) {
        var nextResize = Mathf.Clamp(_reSize - direction * _reSizeSpeed * Time.deltaTime, -1f, 1f);

        float width;
        float height;

        if (nextResize >= 0f) {
            width = Mathf.Lerp(1f, 2.5f, nextResize);
            height = Mathf.Lerp(1f, 0.25f, nextResize);
        }
        else {
            var t = -nextResize;
            width = Mathf.Lerp(1f, 0.25f, t);
            height = Mathf.Lerp(1f, 2.5f, t);
        }

        var targetScale = new Vector3(width, height, 1f);

        if (!CanResize(targetScale))
            return;
        _reSize = nextResize;
        transform.localScale = targetScale;

        _coll.direction = width >= height ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;

        Physics2D.SyncTransforms();
    }

    private bool CanResize(Vector3 targetScale) {
        var previousScale = transform.localScale;
        var previousDirection = _coll.direction;
        Physics2D.SyncTransforms();

        var near = Physics2D.OverlapCapsuleAll(_coll.bounds.center, _coll.bounds.size, _coll.direction, 0f);
        transform.localScale = targetScale;

        _coll.direction = targetScale.x >= targetScale.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
        Physics2D.SyncTransforms();

        var hits = Physics2D.OverlapCapsuleAll(_coll.bounds.center, _coll.bounds.size, _coll.direction, 0f);

        var blocked = false;

        transform.localScale = previousScale;
        _coll.direction = previousDirection;

        Physics2D.SyncTransforms();

        return !blocked;
    }
}