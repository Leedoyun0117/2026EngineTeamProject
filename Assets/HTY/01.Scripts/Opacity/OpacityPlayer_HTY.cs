using UnityEngine;

public class OpacityPlayer_HTY : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spCompo;

    public void OnOpacity()
    {
        Color alpha = _spCompo.color;
        alpha.a = 0.5f;
        _spCompo.color = alpha;
    }
    public void OffOpacity()
    {
        Color alpha = _spCompo.color;
        alpha.a = 1;
        _spCompo.color = alpha;
    }

}
