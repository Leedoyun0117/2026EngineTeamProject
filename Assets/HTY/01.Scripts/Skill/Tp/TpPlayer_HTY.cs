using System;
using UnityEngine;

public class TpPlayer_HTY : MonoBehaviour, ISkill_HTY {
    public GameObject _panel;

    public bool CanUse() {
        throw new NotImplementedException();
    }

    public void UseSkill() {
        _panel.SetActive(true);
    }
}