using UnityEngine;

public class CopyPlayer_HTY : MonoBehaviour, ISkill_HTY
{
    public GameObject _copyObj;
    private bool _canCopy = true;

    private GameObject _currentCopyObj;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get;set; }

    public bool CanUse() => _canCopy;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _skillState = SkillState.Copy;
        _skillSO = GameManager_HTY.Instance._skillList._skillDict[_skillState];
    }

    public void UseSkill()
    {
        if (CanUse())
        {
            //_currentCopyObj = Instantiate(_copyObj, _pointerCompo._pointer.position, Quaternion.identity);
            _currentCopyObj.GetComponent<TestPlayerMove_HTY>().SetPlay(false);
            FindAnyObjectByType<PastePlayer_HTY>()._otherObj = _currentCopyObj;
            _canCopy = false;
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
