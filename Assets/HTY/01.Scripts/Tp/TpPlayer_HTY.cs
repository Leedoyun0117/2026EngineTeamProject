using UnityEngine;
using UnityEngine.EventSystems;

public class TpPlayer_HTY : MonoBehaviour,ISkill_HTY
{
    
    public GameObject _panel;

    public bool CanUse()
    {
        throw new System.NotImplementedException();
    }

    public void UseSkill()
    {
        _panel.SetActive(true);
    }
}
