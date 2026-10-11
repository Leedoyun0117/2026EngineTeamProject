using UnityEngine;
using UnityEngine.EventSystems;

public class CopyPlayer_HTY : MonoBehaviour, ISkill_HTY
{
    public GameObject _copyObj;
    public bool _canCopy = false;
    


    private void Start()
    {
        gameObject.SetActive(false);
    }

    public bool CanUse() => _canCopy;

    public void UseSkill()
    {
        _canCopy = !_canCopy;
        if (_canCopy) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
}
