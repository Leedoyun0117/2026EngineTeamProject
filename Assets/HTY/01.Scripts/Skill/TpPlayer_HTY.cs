using UnityEngine;
using UnityEngine.InputSystem;

public class TpPlayer_HTY : MonoBehaviour, ISkill_HTY
{
    public SkillSo_HTY _skillSO { get; set; }
    public SkillState _skillState { get; set; }
    

    public bool CanUse() => true;

    public void Init()
    {
        _skillState = SkillState.Tp;
        _skillSO = SkillManager_HTY.Instance._skillSoList._skillDict[_skillState];
    }

    public void UseSkill()
    {
        Vector2 pointer = SkillManager_HTY.Instance.GetPointerPos();
        if (CanUse())
        {
            Collider2D[] coll = Physics2D.OverlapPointAll(pointer);
            foreach (Collider2D coll2d in coll)
            {
                if (coll2d.gameObject.TryGetComponent<TpObj_HTY>(out _))
                {
                    transform.parent.position = pointer;
                }
            }
        }
    }

    public void Enter()
    {
        SkillManager_HTY.Instance._utilityCompo.OnPointer();
    }

    public void Exit()
    {
        SkillManager_HTY.Instance._utilityCompo.OffPointer();
    }

    public void UpdateState()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            UseSkill();
        }
    }
}
