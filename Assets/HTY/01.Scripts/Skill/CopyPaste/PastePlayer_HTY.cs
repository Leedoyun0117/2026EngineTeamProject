using UnityEngine;

public class PastePlayer_HTY : MonoBehaviour, ISkill_HTY {
    public GameObject _otherObj;
    private GameObject _playObj;

    private void Start() {
        _playObj = GameManager_HTY.instance._player;
    }

    public bool CanUse() {
        return _otherObj != null;
    }

    public void UseSkill() {
        if (CanUse()) {
            if (_playObj.TryGetComponent(out TestPlayerMove_HTY moveCompo)) {
                moveCompo.SetPlay(false);
                if (_otherObj.TryGetComponent(out TestPlayerMove_HTY moveCompoOther)) moveCompoOther.SetPlay(true);
            }

            var someObj = _playObj;
            _playObj = _otherObj;
            _otherObj = someObj;
        }
        //나중에 효과 넣을듯
    }
}