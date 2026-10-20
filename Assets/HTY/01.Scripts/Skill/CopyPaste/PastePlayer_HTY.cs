using System.Net.NetworkInformation;
using UnityEngine;

public class PastePlayer_HTY : MonoBehaviour,ISkill_HTY
{
    private GameObject _playObj;
    public GameObject _otherObj;

    private void Start()
    {
        _playObj = GameManager_HTY.instance._player;
    }

    public bool CanUse() => _otherObj != null;

    public void UseSkill()
    {
        if (CanUse())
        {
            if (_playObj.TryGetComponent<TestPlayerMove_HTY>(out TestPlayerMove_HTY moveCompo))
            {
                moveCompo.SetPlay(false);
                if (_otherObj.TryGetComponent<TestPlayerMove_HTY>(out TestPlayerMove_HTY moveCompoOther))
                {
                    moveCompoOther.SetPlay(true);
                }
            }
            GameObject someObj = _playObj;
            _playObj = _otherObj;
            _otherObj = someObj;
        }
        else
        {
            //나중에 효과 넣을듯
        }
    }
}
