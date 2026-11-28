using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class CursorLighting_HTY : MonoBehaviour, ISkill_HTY
{
    private Light2D _light;
    private bool _isLight;

    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }


    public void Init()
    {
        _light = transform.parent.gameObject.GetComponentInChildren<Light2D>();
        _skillState = SkillState.Luminosity;
        _skillSO = SkillManager_HTY.Instance._stateToSoDict[_skillState];
    }

    public bool CanUse() => true;

    public void UseSkill()
    {
        if (CanUse())
        {
            _light.enabled = !_isLight;
            _isLight = !_isLight;
        }
    }

    public void Enter()
    {
        _light.enabled = false;
        _isLight = false;
    }

    public void UpdateState()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            UseSkill();
        }
    }

    public void Exit()
    {
        _light.enabled = false;
        _isLight = false;
    }
}
