using UnityEngine;

public class OpacityPlayer_HTY : MonoBehaviour, ISkill_HTY {
    [SerializeField] private SpriteRenderer _spCompo;
    public bool _isOpacity;
    private readonly bool _canOpacity = true;

    public bool CanUse() {
        return _canOpacity;
    }

    public void UseSkill() {
        if (_isOpacity) {
            var alpha = _spCompo.color;
            alpha.a = 1;
            _spCompo.color = alpha;
        }
        else if (!_isOpacity) {
            var alpha = _spCompo.color;
            alpha.a = 0.5f;
            _spCompo.color = alpha;
        }

        _isOpacity = !_isOpacity;
    }
}