using UnityEngine;
using UnityEngine.InputSystem;

public class OpacitySkill_HTY : MonoBehaviour, ISkill_HTY
{
    private TargetSelector_HTY _targetSelector;
    private bool _isOpacity;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }

    public bool CanUse() => true;

    public void Init()
    {
        _targetSelector = GetComponentInParent<TargetSelector_HTY>();
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
        
    }

    public void UpdateState()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame)
            UseSkill();
        if (Keyboard.current.rKey.wasPressedThisFrame)
            _targetSelector.TargetDetectAndSelect();
    }

    public void Exit()
    {
    }
}
