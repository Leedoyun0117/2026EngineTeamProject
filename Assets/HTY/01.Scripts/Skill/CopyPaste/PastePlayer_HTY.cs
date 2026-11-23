using UnityEngine;

public class PastePlayer_HTY : MonoBehaviour, ISkill_HTY
{
    private GameObject _playObj;
    public GameObject _otherObj;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    private void Start()
    {
        //_playObj = ;
        Init();
    }

    public void Init()
    {
        _skillState = SkillState.Paste;
        _skillSO = GetComponentInParent<SkillManager_HTY>()._skillSoList._skillDict[_skillState];
    }

    public bool CanUse() => _otherObj != null;

    public void UseSkill()
    {
        if (CanUse())
        {
            
        }
        else
        {
            //나중에 효과 넣을듯
        }
    }

    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
