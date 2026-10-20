using System.Net.NetworkInformation;
using UnityEngine;

public class OpacityPlayer_HTY : MonoBehaviour,ISkill_HTY
{
    [SerializeField] private SpriteRenderer _spCompo;
    private bool _canOpacity = true;
    public bool _isOpacity = false;

    public bool CanUse()
    {
        return _canOpacity;
    }

    public void UseSkill()
    {
        if (_isOpacity)
        {
            Color alpha = _spCompo.color;
            alpha.a = 1;
            _spCompo.color = alpha;
        }
        else if (!_isOpacity)
        {
            Color alpha = _spCompo.color;
            alpha.a = 0.5f;
            _spCompo.color = alpha;
        }
        _isOpacity = !_isOpacity;
    }
}
