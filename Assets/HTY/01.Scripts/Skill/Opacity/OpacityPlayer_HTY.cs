using UnityEngine;

public class OpacityPlayer_HTY : MonoBehaviour, ISkill_HTY
{
    public TargetSelector_HTY _targetSelector;
    private bool _isOpacity;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    public bool CanUse() => true;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _skillState = SkillState.Opacity;
        _skillSO = GameManager_HTY.Instance._skillList._skillDict[_skillState];
    }

    public void UseSkill()
    {
        if (CanUse())
        {
            Color _targetColor = _targetSelector._currentTarget.gameObject.GetComponent<SpriteRenderer>().color;
            _targetColor.a = _isOpacity ? 1 : 0.5f;
            _targetSelector._currentTarget.gameObject.GetComponent<SpriteRenderer>().color = _targetColor;
            _isOpacity = !_isOpacity;
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
